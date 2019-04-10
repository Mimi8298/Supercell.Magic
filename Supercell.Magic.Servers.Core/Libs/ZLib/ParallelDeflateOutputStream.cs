//#define Trace

// ParallelDeflateOutputStream.cs
// ------------------------------------------------------------------
//
// A DeflateStream that does compression only, it uses a
// divide-and-conquer approach with multiple threads to exploit multiple
// CPUs for the DEFLATE computation.
//
// last saved: <2011-July-31 14:49:40>
//
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 by Dino Chiesa
// All rights reserved!
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------

namespace Supercell.Magic.Servers.Core.Libs.ZLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    internal class WorkItem
    {
        public byte[] buffer;
        public byte[] compressed;
        public int crc;
        public int index;
        public int ordinal;
        public int inputBytesAvailable;
        public int compressedBytesAvailable;
        public ZLibCodec compressor;

        public WorkItem(int size,
                        CompressionLevel compressLevel,
                        CompressionStrategy strategy,
                        int ix)
        {
            this.buffer = new byte[size];
            // alloc 5 bytes overhead for every block (margin of safety= 2)
            int n = size + (size / 32768 + 1) * 5 * 2;
            this.compressed = new byte[n];
            this.compressor = new ZLibCodec();
            this.compressor.InitializeDeflate(compressLevel, false);
            this.compressor.OutputBuffer = this.compressed;
            this.compressor.InputBuffer = this.buffer;
            this.index = ix;
        }
    }

    public class ParallelDeflateOutputStream : Stream
    {
        private static readonly int IO_BUFFER_SIZE_DEFAULT = 64 * 1024; // 128k
        private static readonly int BufferPairsPerCore = 4;

        private List<WorkItem> m_pool;
        private readonly bool m_leaveOpen;
        private bool emitting;
        private Stream m_outStream;
        private int m_maxBufferPairs;
        private int m_bufferSize = ParallelDeflateOutputStream.IO_BUFFER_SIZE_DEFAULT;

        private AutoResetEvent m_newlyCompressedBlob;

        //private ManualResetEvent            m_writingDone;
        //private ManualResetEvent            m_sessionReset;
        private readonly object m_outputLock = new object();
        private bool m_isClosed;
        private bool m_firstWriteDone;
        private int m_currentlyFilling;
        private int m_lastFilled;
        private int m_lastWritten;
        private int m_latestCompressed;
        private CRC32 m_runningCrc;
        private readonly object m_latestLock = new object();
        private Queue<int> m_toWrite;
        private Queue<int> m_toFill;
        private readonly CompressionLevel m_compressLevel;
        private volatile Exception m_pendingException;
        private bool m_handlingException;
        private readonly object m_eLock = new object(); // protects m_pendingException

        // This bitfield is used only when Trace is defined.
        //private TraceBits m_DesiredTrace = TraceBits.Write | TraceBits.WriteBegin |
        //TraceBits.WriteDone | TraceBits.Lifecycle | TraceBits.Fill | TraceBits.Flush |
        //TraceBits.Session;

        //private TraceBits m_DesiredTrace = TraceBits.WriteBegin | TraceBits.WriteDone | TraceBits.Synch | TraceBits.Lifecycle  | TraceBits.Session ;

        private readonly TraceBits m_DesiredTrace =
            TraceBits.Session |
            TraceBits.Compress |
            TraceBits.WriteTake |
            TraceBits.WriteEnter |
            TraceBits.EmitEnter |
            TraceBits.EmitDone |
            TraceBits.EmitLock |
            TraceBits.EmitSkip |
            TraceBits.EmitBegin;

        public ParallelDeflateOutputStream(Stream stream)
            : this(stream, CompressionLevel.Default, CompressionStrategy.Default, false)
        {
        }

        public ParallelDeflateOutputStream(Stream stream, CompressionLevel level)
            : this(stream, level, CompressionStrategy.Default, false)
        {
        }

        public ParallelDeflateOutputStream(Stream stream, bool leaveOpen)
            : this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen)
        {
        }

        public ParallelDeflateOutputStream(Stream stream, CompressionLevel level, bool leaveOpen)
            : this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen)
        {
        }

        public ParallelDeflateOutputStream(Stream stream,
                                           CompressionLevel level,
                                           CompressionStrategy strategy,
                                           bool leaveOpen)
        {
            this.TraceOutput(TraceBits.Lifecycle | TraceBits.Session, "-------------------------------------------------------");
            this.TraceOutput(TraceBits.Lifecycle | TraceBits.Session, "Create {0:X8}", this.GetHashCode());
            this.m_outStream = stream;
            this.m_compressLevel = level;
            this.Strategy = strategy;
            this.m_leaveOpen = leaveOpen;
            this.MaxBufferPairs = 16; // default
        }


        public CompressionStrategy Strategy { get; }

        public int MaxBufferPairs
        {
            get
            {
                return this.m_maxBufferPairs;
            }
            set
            {
                if (value < 4)
                {
                    throw new ArgumentException("MaxBufferPairs",
                                                "Value must be 4 or greater.");
                }

                this.m_maxBufferPairs = value;
            }
        }

        public int BufferSize
        {
            get
            {
                return this.m_bufferSize;
            }
            set
            {
                if (value < 1024)
                {
                    throw new ArgumentOutOfRangeException("BufferSize",
                                                          "BufferSize must be greater than 1024 bytes");
                }

                this.m_bufferSize = value;
            }
        }

        public int Crc32 { get; private set; }


        public long BytesProcessed { get; private set; }


        private void m_InitializePoolOfWorkItems()
        {
            this.m_toWrite = new Queue<int>();
            this.m_toFill = new Queue<int>();
            this.m_pool = new List<WorkItem>();
            int nTasks = ParallelDeflateOutputStream.BufferPairsPerCore * Environment.ProcessorCount;
            nTasks = Math.Min(nTasks, this.m_maxBufferPairs);
            for (int i = 0; i < nTasks; i++)
            {
                this.m_pool.Add(new WorkItem(this.m_bufferSize, this.m_compressLevel, this.Strategy, i));
                this.m_toFill.Enqueue(i);
            }

            this.m_newlyCompressedBlob = new AutoResetEvent(false);
            this.m_runningCrc = new CRC32();
            this.m_currentlyFilling = -1;
            this.m_lastFilled = -1;
            this.m_lastWritten = -1;
            this.m_latestCompressed = -1;
        }


        public override void Write(byte[] buffer, int offset, int count)
        {
            bool mustWait = false;

            // This method does this:
            //   0. handles any pending exceptions
            //   1. write any buffers that are ready to be written,
            //   2. fills a work buffer; when full, flip state to 'Filled',
            //   3. if more data to be written,  goto step 1

            if (this.m_isClosed)
            {
                throw new InvalidOperationException();
            }

            // dispense any exceptions that occurred on the BG threads
            if (this.m_pendingException != null)
            {
                this.m_handlingException = true;
                Exception pe = this.m_pendingException;
                this.m_pendingException = null;
                throw pe;
            }

            if (count == 0)
            {
                return;
            }

            if (!this.m_firstWriteDone)
            {
                // Want to do this on first Write, first session, and not in the
                // constructor.  We want to allow MaxBufferPairs to
                // change after construction, but before first Write.
                this.m_InitializePoolOfWorkItems();
                this.m_firstWriteDone = true;
            }


            do
            {
                // may need to make buffers available
                this.EmitPendingBuffers(false, mustWait);

                mustWait = false;
                // use current buffer, or get a new buffer to fill
                int ix = -1;
                if (this.m_currentlyFilling >= 0)
                {
                    ix = this.m_currentlyFilling;
                    this.TraceOutput(TraceBits.WriteTake,
                                     "Write    notake   wi({0}) lf({1})",
                                     ix, this.m_lastFilled);
                }
                else
                {
                    this.TraceOutput(TraceBits.WriteTake, "Write    take?");
                    if (this.m_toFill.Count == 0)
                    {
                        // no available buffers, so... need to emit
                        // compressed buffers.
                        mustWait = true;
                        continue;
                    }

                    ix = this.m_toFill.Dequeue();
                    this.TraceOutput(TraceBits.WriteTake,
                                     "Write    take     wi({0}) lf({1})",
                                     ix, this.m_lastFilled);
                    ++this.m_lastFilled; // TODO: consider rollover?
                }

                WorkItem workitem = this.m_pool[ix];

                int limit = workitem.buffer.Length - workitem.inputBytesAvailable > count
                    ? count
                    : workitem.buffer.Length - workitem.inputBytesAvailable;

                workitem.ordinal = this.m_lastFilled;

                this.TraceOutput(TraceBits.Write,
                                 "Write    lock     wi({0}) ord({1}) iba({2})",
                                 workitem.index,
                                 workitem.ordinal,
                                 workitem.inputBytesAvailable
                );

                // copy from the provided buffer to our workitem, starting at
                // the tail end of whatever data we might have in there currently.
                Buffer.BlockCopy(buffer,
                                 offset,
                                 workitem.buffer,
                                 workitem.inputBytesAvailable,
                                 limit);

                count -= limit;
                offset += limit;
                workitem.inputBytesAvailable += limit;
                if (workitem.inputBytesAvailable == workitem.buffer.Length)
                {
                    // No need for interlocked.increment: the Write()
                    // method is documented as not multi-thread safe, so
                    // we can assume Write() calls come in from only one
                    // thread.
                    this.TraceOutput(TraceBits.Write,
                                     "Write    QUWI     wi({0}) ord({1}) iba({2}) nf({3})",
                                     workitem.index,
                                     workitem.ordinal,
                                     workitem.inputBytesAvailable);

                    if (!ThreadPool.QueueUserWorkItem(this.m_DeflateOne, workitem))
                    {
                        throw new Exception("Cannot enqueue workitem");
                    }

                    this.m_currentlyFilling = -1; // will get a new buffer next time
                }
                else
                {
                    this.m_currentlyFilling = ix;
                }

                if (count > 0)
                {
                    this.TraceOutput(TraceBits.WriteEnter, "Write    more");
                }
            } while (count > 0); // until no more to write

            this.TraceOutput(TraceBits.WriteEnter, "Write    exit");
        }


        private void m_FlushFinish()
        {
            // After writing a series of compressed buffers, each one closed
            // with Flush.Sync, we now write the final one as Flush.Finish,
            // and then stop.
            byte[] buffer = new byte[128];
            ZLibCodec compressor = new ZLibCodec();
            int rc = compressor.InitializeDeflate(this.m_compressLevel, false);
            compressor.InputBuffer = null;
            compressor.NextIn = 0;
            compressor.AvailableBytesIn = 0;
            compressor.OutputBuffer = buffer;
            compressor.NextOut = 0;
            compressor.AvailableBytesOut = buffer.Length;
            rc = compressor.Deflate(FlushType.Finish);

            if (rc != ZLibConstants.Z_STREAM_END && rc != ZLibConstants.Z_OK)
            {
                throw new Exception("deflating: " + compressor.Message);
            }

            if (buffer.Length - compressor.AvailableBytesOut > 0)
            {
                this.TraceOutput(TraceBits.EmitBegin,
                                 "Emit     begin    flush bytes({0})",
                                 buffer.Length - compressor.AvailableBytesOut);

                this.m_outStream.Write(buffer, 0, buffer.Length - compressor.AvailableBytesOut);

                this.TraceOutput(TraceBits.EmitDone,
                                 "Emit     done     flush");
            }

            compressor.EndDeflate();

            this.Crc32 = this.m_runningCrc.Crc32Result;
        }


        private void m_Flush(bool lastInput)
        {
            if (this.m_isClosed)
            {
                throw new InvalidOperationException();
            }

            if (this.emitting)
            {
                return;
            }

            // compress any partial buffer
            if (this.m_currentlyFilling >= 0)
            {
                WorkItem workitem = this.m_pool[this.m_currentlyFilling];
                this.m_DeflateOne(workitem);
                this.m_currentlyFilling = -1; // get a new buffer next Write()
            }

            if (lastInput)
            {
                this.EmitPendingBuffers(true, false);
                this.m_FlushFinish();
            }
            else
            {
                this.EmitPendingBuffers(false, false);
            }
        }


        public override void Flush()
        {
            if (this.m_pendingException != null)
            {
                this.m_handlingException = true;
                Exception pe = this.m_pendingException;
                this.m_pendingException = null;
                throw pe;
            }

            if (this.m_handlingException)
            {
                return;
            }

            this.m_Flush(false);
        }


        public override void Close()
        {
            this.TraceOutput(TraceBits.Session, "Close {0:X8}", this.GetHashCode());

            if (this.m_pendingException != null)
            {
                this.m_handlingException = true;
                Exception pe = this.m_pendingException;
                this.m_pendingException = null;
                throw pe;
            }

            if (this.m_handlingException)
            {
                return;
            }

            if (this.m_isClosed)
            {
                return;
            }

            this.m_Flush(true);

            if (!this.m_leaveOpen)
            {
                this.m_outStream.Close();
            }

            this.m_isClosed = true;
        }


        // workitem 10030 - implement a new Dispose method

        public new void Dispose()
        {
            this.TraceOutput(TraceBits.Lifecycle, "Dispose  {0:X8}", this.GetHashCode());
            this.Close();
            this.m_pool = null;
            this.Dispose(true);
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }


        public void Reset(Stream stream)
        {
            this.TraceOutput(TraceBits.Session, "-------------------------------------------------------");
            this.TraceOutput(TraceBits.Session, "Reset {0:X8} firstDone({1})", this.GetHashCode(), this.m_firstWriteDone);

            if (!this.m_firstWriteDone)
            {
                return;
            }

            // reset all status
            this.m_toWrite.Clear();
            this.m_toFill.Clear();
            foreach (WorkItem workitem in this.m_pool)
            {
                this.m_toFill.Enqueue(workitem.index);
                workitem.ordinal = -1;
            }

            this.m_firstWriteDone = false;
            this.BytesProcessed = 0L;
            this.m_runningCrc = new CRC32();
            this.m_isClosed = false;
            this.m_currentlyFilling = -1;
            this.m_lastFilled = -1;
            this.m_lastWritten = -1;
            this.m_latestCompressed = -1;
            this.m_outStream = stream;
        }


        private void EmitPendingBuffers(bool doAll, bool mustWait)
        {
            // When combining parallel deflation with a ZipSegmentedStream, it's
            // possible for the ZSS to throw from within this method.  In that
            // case, Close/Dispose will be called on this stream, if this stream
            // is employed within a using or try/finally pair as required. But
            // this stream is unaware of the pending exception, so the Close()
            // method invokes this method AGAIN.  This can lead to a deadlock.
            // Therefore, failfast if re-entering.

            if (this.emitting)
            {
                return;
            }

            this.emitting = true;
            if (doAll || mustWait)
            {
                this.m_newlyCompressedBlob.WaitOne();
            }

            do
            {
                int firstSkip = -1;
                int millisecondsToWait = doAll ? 200 : (mustWait ? -1 : 0);
                int nextToWrite = -1;

                do
                {
                    if (Monitor.TryEnter(this.m_toWrite, millisecondsToWait))
                    {
                        nextToWrite = -1;
                        try
                        {
                            if (this.m_toWrite.Count > 0)
                            {
                                nextToWrite = this.m_toWrite.Dequeue();
                            }
                        }
                        finally
                        {
                            Monitor.Exit(this.m_toWrite);
                        }

                        if (nextToWrite >= 0)
                        {
                            WorkItem workitem = this.m_pool[nextToWrite];
                            if (workitem.ordinal != this.m_lastWritten + 1)
                            {
                                // out of order. requeue and try again.
                                this.TraceOutput(TraceBits.EmitSkip,
                                                 "Emit     skip     wi({0}) ord({1}) lw({2}) fs({3})",
                                                 workitem.index,
                                                 workitem.ordinal, this.m_lastWritten,
                                                 firstSkip);

                                lock (this.m_toWrite)
                                {
                                    this.m_toWrite.Enqueue(nextToWrite);
                                }

                                if (firstSkip == nextToWrite)
                                {
                                    // We went around the list once.
                                    // None of the items in the list is the one we want.
                                    // Now wait for a compressor to signal again.
                                    this.m_newlyCompressedBlob.WaitOne();
                                    firstSkip = -1;
                                }
                                else if (firstSkip == -1)
                                {
                                    firstSkip = nextToWrite;
                                }

                                continue;
                            }

                            firstSkip = -1;

                            this.TraceOutput(TraceBits.EmitBegin,
                                             "Emit     begin    wi({0}) ord({1})              cba({2})",
                                             workitem.index,
                                             workitem.ordinal,
                                             workitem.compressedBytesAvailable);

                            this.m_outStream.Write(workitem.compressed, 0, workitem.compressedBytesAvailable);
                            this.m_runningCrc.Combine(workitem.crc, workitem.inputBytesAvailable);
                            this.BytesProcessed += workitem.inputBytesAvailable;
                            workitem.inputBytesAvailable = 0;

                            this.TraceOutput(TraceBits.EmitDone,
                                             "Emit     done     wi({0}) ord({1})              cba({2}) mtw({3})",
                                             workitem.index,
                                             workitem.ordinal,
                                             workitem.compressedBytesAvailable,
                                             millisecondsToWait);

                            this.m_lastWritten = workitem.ordinal;
                            this.m_toFill.Enqueue(workitem.index);

                            // don't wait next time through
                            if (millisecondsToWait == -1)
                            {
                                millisecondsToWait = 0;
                            }
                        }
                    }
                    else
                    {
                        nextToWrite = -1;
                    }
                } while (nextToWrite >= 0);
            } while (doAll && this.m_lastWritten != this.m_latestCompressed);

            this.emitting = false;
        }


    #if OLD
        private void m_PerpetualWriterMethod(object state)
        {
            TraceOutput(TraceBits.WriterThread, "_PerpetualWriterMethod START");

            try
            {
                do
                {
                    // wait for the next session
                    TraceOutput(TraceBits.Synch | TraceBits.WriterThread, "Synch    m_sessionReset.WaitOne(begin) PWM");
                    m_sessionReset.WaitOne();
                    TraceOutput(TraceBits.Synch | TraceBits.WriterThread, "Synch    m_sessionReset.WaitOne(done)  PWM");

                    if (_isDisposed) break;

                    TraceOutput(TraceBits.Synch | TraceBits.WriterThread, "Synch    m_sessionReset.Reset()        PWM");
                    m_sessionReset.Reset();

                    // repeatedly write buffers as they become ready
                    WorkItem workitem = null;
                    Supercell.Magic.Servers.Core.Libs.ZLib.CRC32 c = new Supercell.Magic.Servers.Core.Libs.ZLib.CRC32();
                    do
                    {
                        workitem = m_pool[_nextToWrite % m_pc];
                        lock(workitem)
                        {
                            if (_noMoreInputForThisSegment)
                                TraceOutput(TraceBits.Write,
                                               "Write    drain    wi({0}) stat({1}) canuse({2})  cba({3})",
                                               workitem.index,
                                               workitem.status,
                                               (workitem.status == (int)WorkItem.Status.Compressed),
                                               workitem.compressedBytesAvailable);

                            do
                            {
                                if (workitem.status == (int)WorkItem.Status.Compressed)
                                {
                                    TraceOutput(TraceBits.WriteBegin,
                                                   "Write    begin    wi({0}) stat({1})              cba({2})",
                                                   workitem.index,
                                                   workitem.status,
                                                   workitem.compressedBytesAvailable);

                                    workitem.status = (int)WorkItem.Status.Writing;
                                    m_outStream.Write(workitem.compressed, 0, workitem.compressedBytesAvailable);
                                    c.Combine(workitem.crc, workitem.inputBytesAvailable);
                                    m_totalBytesProcessed += workitem.inputBytesAvailable;
                                    m_nextToWrite++;
                                    workitem.inputBytesAvailable = 0;
                                    workitem.status = (int)WorkItem.Status.Done;

                                    TraceOutput(TraceBits.WriteDone,
                                                   "Write    done     wi({0}) stat({1})              cba({2})",
                                                   workitem.index,
                                                   workitem.status,
                                                   workitem.compressedBytesAvailable);


                                    Monitor.Pulse(workitem);
                                    break;
                                }
                                else
                                {
                                    int wcycles = 0;
                                    // I've locked a workitem I cannot use.
                                    // Therefore, wake someone else up, and then release the lock.
                                    while (workitem.status != (int)WorkItem.Status.Compressed)
                                    {
                                        TraceOutput(TraceBits.WriteWait,
                                                       "Write    waiting  wi({0}) stat({1}) nw({2}) nf({3}) nomore({4})",
                                                       workitem.index,
                                                       workitem.status,
                                                       m_nextToWrite, m_nextToFill,
                                                       m_noMoreInputForThisSegment );

                                        if (_noMoreInputForThisSegment && m_nextToWrite == m_nextToFill)
                                            break;

                                        wcycles++;

                                        // wake up someone else
                                        Monitor.Pulse(workitem);
                                        // release and wait
                                        Monitor.Wait(workitem);

                                        if (workitem.status == (int)WorkItem.Status.Compressed)
                                            TraceOutput(TraceBits.WriteWait,
                                                           "Write    A-OK     wi({0}) stat({1}) iba({2}) cba({3}) cyc({4})",
                                                           workitem.index,
                                                           workitem.status,
                                                           workitem.inputBytesAvailable,
                                                           workitem.compressedBytesAvailable,
                                                           wcycles);
                                    }

                                    if (_noMoreInputForThisSegment && m_nextToWrite == m_nextToFill)
                                        break;

                                }
                            }
                            while (true);
                        }

                        if (_noMoreInputForThisSegment)
                            TraceOutput(TraceBits.Write,
                                           "Write    nomore  nw({0}) nf({1}) break({2})",
                                           m_nextToWrite, m_nextToFill, (_nextToWrite == m_nextToFill));

                        if (_noMoreInputForThisSegment && m_nextToWrite == m_nextToFill)
                            break;

                    } while (true);


                    // Finish:
                    // After writing a series of buffers, closing each one with
                    // Flush.Sync, we now write the final one as Flush.Finish, and
                    // then stop.
                    byte[] buffer = new byte[128];
                    ZLibCodec compressor = new ZLibCodec();
                    int rc = compressor.InitializeDeflate(_compressLevel, false);
                    compressor.InputBuffer = null;
                    compressor.NextIn = 0;
                    compressor.AvailableBytesIn = 0;
                    compressor.OutputBuffer = buffer;
                    compressor.NextOut = 0;
                    compressor.AvailableBytesOut = buffer.Length;
                    rc = compressor.Deflate(FlushType.Finish);

                    if (rc != ZLibConstants.Z_STREAM_END && rc != ZLibConstants.Z_OK)
                        throw new Exception("deflating: " + compressor.Message);

                    if (buffer.Length - compressor.AvailableBytesOut > 0)
                    {
                        TraceOutput(TraceBits.WriteBegin,
                                       "Write    begin    flush bytes({0})",
                                       buffer.Length - compressor.AvailableBytesOut);

                        m_outStream.Write(buffer, 0, buffer.Length - compressor.AvailableBytesOut);

                        TraceOutput(TraceBits.WriteBegin,
                                       "Write    done     flush");
                    }

                    compressor.EndDeflate();

                    m_Crc32 = c.Crc32Result;

                    // signal that writing is complete:
                    TraceOutput(TraceBits.Synch, "Synch    m_writingDone.Set()           PWM");
                    m_writingDone.Set();
                }
                while (true);
            }
            catch (System.Exception exc1)
            {
                lock(_eLock)
                {
                    // expose the exception to the main thread
                    if (_pendingException!=null)
                        m_pendingException = exc1;
                }
            }

            TraceOutput(TraceBits.WriterThread, "_PerpetualWriterMethod FINIS");
        }
                #endif


        private void m_DeflateOne(object wi)
        {
            // compress one buffer
            WorkItem workitem = (WorkItem) wi;
            try
            {
                int myItem = workitem.index;
                CRC32 crc = new CRC32();

                // calc CRC on the buffer
                crc.SlurpBlock(workitem.buffer, 0, workitem.inputBytesAvailable);

                // deflate it
                this.DeflateOneSegment(workitem);

                // update status
                workitem.crc = crc.Crc32Result;
                this.TraceOutput(TraceBits.Compress,
                                 "Compress          wi({0}) ord({1}) len({2})",
                                 workitem.index,
                                 workitem.ordinal,
                                 workitem.compressedBytesAvailable
                );

                lock (this.m_latestLock)
                {
                    if (workitem.ordinal > this.m_latestCompressed)
                    {
                        this.m_latestCompressed = workitem.ordinal;
                    }
                }

                lock (this.m_toWrite)
                {
                    this.m_toWrite.Enqueue(workitem.index);
                }

                this.m_newlyCompressedBlob.Set();
            }
            catch (Exception exc1)
            {
                lock (this.m_eLock)
                {
                    // expose the exception to the main thread
                    if (this.m_pendingException != null)
                    {
                        this.m_pendingException = exc1;
                    }
                }
            }
        }


        private bool DeflateOneSegment(WorkItem workitem)
        {
            ZLibCodec compressor = workitem.compressor;
            int rc = 0;
            compressor.ResetDeflate();
            compressor.NextIn = 0;

            compressor.AvailableBytesIn = workitem.inputBytesAvailable;

            // step 1: deflate the buffer
            compressor.NextOut = 0;
            compressor.AvailableBytesOut = workitem.compressed.Length;
            do
            {
                compressor.Deflate(FlushType.None);
            } while (compressor.AvailableBytesIn > 0 || compressor.AvailableBytesOut == 0);

            // step 2: flush (sync)
            rc = compressor.Deflate(FlushType.Sync);

            workitem.compressedBytesAvailable = (int) compressor.TotalBytesOut;
            return true;
        }


        [Conditional("Trace")]
        private void TraceOutput(TraceBits bits, string format, params object[] varParams)
        {
            if ((bits & this.m_DesiredTrace) != 0)
            {
                lock (this.m_outputLock)
                {
                    int tid = Thread.CurrentThread.GetHashCode();
                #if !SILVERLIGHT
                    Console.ForegroundColor = (ConsoleColor) (tid % 8 + 8);
                #endif
                    Console.Write("{0:000} PDOS ", tid);
                    Console.WriteLine(format, varParams);
                #if !SILVERLIGHT
                    Console.ResetColor();
                #endif
                }
            }
        }


        // used only when Trace is defined
        [Flags]
        private enum TraceBits : uint
        {
            None = 0,
            NotUsed1 = 1,
            EmitLock = 2,
            EmitEnter = 4, // enter m_EmitPending
            EmitBegin = 8, // begin to write out
            EmitDone = 16, // done writing out
            EmitSkip = 32, // writer skipping a workitem
            EmitAll = 58, // All Emit flags
            Flush = 64,
            Lifecycle = 128, // constructor/disposer
            Session = 256, // Close/Reset
            Synch = 512, // thread synchronization
            Instance = 1024, // instance settings
            Compress = 2048, // compress task
            Write = 4096, // filling buffers, when caller invokes Write()
            WriteEnter = 8192, // upon entry to Write()
            WriteTake = 16384, // on m_toFill.Take()
            All = 0xffffffff
        }


        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }


        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.m_outStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                return this.m_outStream.Position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
}