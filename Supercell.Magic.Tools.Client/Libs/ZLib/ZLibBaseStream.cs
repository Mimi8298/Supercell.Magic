// ZLibBaseStream.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa and Microsoft Corporation.
// All rights reserved.
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
//
// last saved (in emacs):
// Time-stamp: <2011-August-06 21:22:38>
//
// ------------------------------------------------------------------
//
// This module defines the ZLibBaseStream class, which is an intnernal
// base class for DeflateStream, ZLibStream and GZipStream.
//
// ------------------------------------------------------------------

namespace Supercell.Magic.Tools.Client.Libs.ZLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    internal enum ZlibStreamFlavor
    {
        ZLIB = 1950,
        DEFLATE = 1951,
        GZIP = 1952
    }

    internal class ZLibBaseStream : Stream
    {
        protected internal ZLibCodec m_z; // deferred init... new ZLibCodec();

        protected internal StreamMode m_streamMode = StreamMode.Undefined;
        protected internal FlushType m_flushMode;
        protected internal ZlibStreamFlavor m_flavor;
        protected internal CompressionMode m_compressionMode;
        protected internal CompressionLevel m_level;
        protected internal bool m_leaveOpen;
        protected internal byte[] m_workingBuffer;
        protected internal int m_bufferSize = ZLibConstants.WorkingBufferSizeDefault;
        protected internal byte[] m_buf1 = new byte[1];

        protected internal Stream m_stream;
        protected internal CompressionStrategy Strategy = CompressionStrategy.Default;

        // workitem 7159
        private readonly CRC32 crc;
        protected internal string m_GzipFileName;
        protected internal string m_GzipComment;
        protected internal DateTime m_GzipMtime;
        protected internal int m_gzipHeaderByteCount;

        internal int Crc32
        {
            get
            {
                if (this.crc == null)
                {
                    return 0;
                }

                return this.crc.Crc32Result;
            }
        }

        public ZLibBaseStream(Stream stream,
                              CompressionMode compressionMode,
                              CompressionLevel level,
                              ZlibStreamFlavor flavor,
                              bool leaveOpen)
        {
            this.m_flushMode = FlushType.None;
            //this.m_workingBuffer = new byte[WORKING_BUFFER_SIZE_DEFAULT];
            this.m_stream = stream;
            this.m_leaveOpen = leaveOpen;
            this.m_compressionMode = compressionMode;
            this.m_flavor = flavor;
            this.m_level = level;
            // workitem 7159
            if (flavor == ZlibStreamFlavor.GZIP)
            {
                this.crc = new CRC32();
            }
        }


        protected internal bool m_wantCompress
        {
            get
            {
                return this.m_compressionMode == CompressionMode.Compress;
            }
        }

        private ZLibCodec z
        {
            get
            {
                if (this.m_z == null)
                {
                    bool wantRfc1950Header = this.m_flavor == ZlibStreamFlavor.ZLIB;
                    this.m_z = new ZLibCodec();
                    if (this.m_compressionMode == CompressionMode.Decompress)
                    {
                        this.m_z.InitializeInflate(wantRfc1950Header);
                    }
                    else
                    {
                        this.m_z.Strategy = this.Strategy;
                        this.m_z.InitializeDeflate(this.m_level, wantRfc1950Header);
                    }
                }

                return this.m_z;
            }
        }


        private byte[] workingBuffer
        {
            get
            {
                if (this.m_workingBuffer == null)
                {
                    this.m_workingBuffer = new byte[this.m_bufferSize];
                }

                return this.m_workingBuffer;
            }
        }


        public override void Write(byte[] buffer, int offset, int count)
        {
            // workitem 7159
            // calculate the CRC on the unccompressed data  (before writing)
            if (this.crc != null)
            {
                this.crc.SlurpBlock(buffer, offset, count);
            }

            if (this.m_streamMode == StreamMode.Undefined)
            {
                this.m_streamMode = StreamMode.Writer;
            }
            else if (this.m_streamMode != StreamMode.Writer)
            {
                throw new ZLibException("Cannot Write after Reading.");
            }

            if (count == 0)
            {
                return;
            }

            // first reference of z property will initialize the private var m_z
            this.z.InputBuffer = buffer;
            this.m_z.NextIn = offset;
            this.m_z.AvailableBytesIn = count;
            bool done = false;
            do
            {
                this.m_z.OutputBuffer = this.workingBuffer;
                this.m_z.NextOut = 0;
                this.m_z.AvailableBytesOut = this.m_workingBuffer.Length;
                int rc = this.m_wantCompress
                    ? this.m_z.Deflate(this.m_flushMode)
                    : this.m_z.Inflate(this.m_flushMode);
                if (rc != ZLibConstants.Z_OK && rc != ZLibConstants.Z_STREAM_END)
                {
                    throw new ZLibException((this.m_wantCompress ? "de" : "in") + "flating: " + this.m_z.Message);
                }

                //if (_workingBuffer.Length - m_z.AvailableBytesOut > 0)
                this.m_stream.Write(this.m_workingBuffer, 0, this.m_workingBuffer.Length - this.m_z.AvailableBytesOut);

                done = this.m_z.AvailableBytesIn == 0 && this.m_z.AvailableBytesOut != 0;

                // If GZIP and de-compress, we're done when 8 bytes remain.
                if (this.m_flavor == ZlibStreamFlavor.GZIP && !this.m_wantCompress)
                {
                    done = this.m_z.AvailableBytesIn == 8 && this.m_z.AvailableBytesOut != 0;
                }
            } while (!done);
        }


        private void finish()
        {
            if (this.m_z == null)
            {
                return;
            }

            if (this.m_streamMode == StreamMode.Writer)
            {
                bool done = false;
                do
                {
                    this.m_z.OutputBuffer = this.workingBuffer;
                    this.m_z.NextOut = 0;
                    this.m_z.AvailableBytesOut = this.m_workingBuffer.Length;
                    int rc = this.m_wantCompress
                        ? this.m_z.Deflate(FlushType.Finish)
                        : this.m_z.Inflate(FlushType.Finish);

                    if (rc != ZLibConstants.Z_STREAM_END && rc != ZLibConstants.Z_OK)
                    {
                        string verb = (this.m_wantCompress ? "de" : "in") + "flating";
                        if (this.m_z.Message == null)
                        {
                            throw new ZLibException(string.Format("{0}: (rc = {1})", verb, rc));
                        }

                        throw new ZLibException(verb + ": " + this.m_z.Message);
                    }

                    if (this.m_workingBuffer.Length - this.m_z.AvailableBytesOut > 0)
                    {
                        this.m_stream.Write(this.m_workingBuffer, 0, this.m_workingBuffer.Length - this.m_z.AvailableBytesOut);
                    }

                    done = this.m_z.AvailableBytesIn == 0 && this.m_z.AvailableBytesOut != 0;
                    // If GZIP and de-compress, we're done when 8 bytes remain.
                    if (this.m_flavor == ZlibStreamFlavor.GZIP && !this.m_wantCompress)
                    {
                        done = this.m_z.AvailableBytesIn == 8 && this.m_z.AvailableBytesOut != 0;
                    }
                } while (!done);

                this.Flush();

                // workitem 7159
                if (this.m_flavor == ZlibStreamFlavor.GZIP)
                {
                    if (this.m_wantCompress)
                    {
                        // Emit the GZIP trailer: CRC32 and  size mod 2^32
                        int c1 = this.crc.Crc32Result;
                        this.m_stream.Write(BitConverter.GetBytes(c1), 0, 4);
                        int c2 = (int) (this.crc.TotalBytesRead & 0x00000000FFFFFFFF);
                        this.m_stream.Write(BitConverter.GetBytes(c2), 0, 4);
                    }
                    else
                    {
                        throw new ZLibException("Writing with decompression is not supported.");
                    }
                }
            }
            // workitem 7159
            else if (this.m_streamMode == StreamMode.Reader)
            {
                if (this.m_flavor == ZlibStreamFlavor.GZIP)
                {
                    if (!this.m_wantCompress)
                    {
                        // workitem 8501: handle edge case (decompress empty stream)
                        if (this.m_z.TotalBytesOut == 0L)
                        {
                            return;
                        }

                        // Read and potentially verify the GZIP trailer:
                        // CRC32 and size mod 2^32
                        byte[] trailer = new byte[8];

                        // workitems 8679 & 12554
                        if (this.m_z.AvailableBytesIn < 8)
                        {
                            // Make sure we have read to the end of the stream
                            Array.Copy(this.m_z.InputBuffer, this.m_z.NextIn, trailer, 0, this.m_z.AvailableBytesIn);
                            int bytesNeeded = 8 - this.m_z.AvailableBytesIn;
                            int bytesRead = this.m_stream.Read(trailer, this.m_z.AvailableBytesIn,
                                                              bytesNeeded);
                            if (bytesNeeded != bytesRead)
                            {
                                throw new ZLibException(string.Format("Missing or incomplete GZIP trailer. Expected 8 bytes, got {0}.", this.m_z.AvailableBytesIn + bytesRead));
                            }
                        }
                        else
                        {
                            Array.Copy(this.m_z.InputBuffer, this.m_z.NextIn, trailer, 0, trailer.Length);
                        }

                        int crc32_expected = BitConverter.ToInt32(trailer, 0);
                        int crc32_actual = this.crc.Crc32Result;
                        int isize_expected = BitConverter.ToInt32(trailer, 4);
                        int isize_actual = (int) (this.m_z.TotalBytesOut & 0x00000000FFFFFFFF);

                        if (crc32_actual != crc32_expected)
                        {
                            throw new ZLibException(string.Format("Bad CRC32 in GZIP trailer. (actual({0:X8})!=expected({1:X8}))", crc32_actual, crc32_expected));
                        }

                        if (isize_actual != isize_expected)
                        {
                            throw new ZLibException(string.Format("Bad size in GZIP trailer. (actual({0})!=expected({1}))", isize_actual, isize_expected));
                        }
                    }
                    else
                    {
                        throw new ZLibException("Reading with compression is not supported.");
                    }
                }
            }
        }


        private void end()
        {
            if (this.z == null)
            {
                return;
            }

            if (this.m_wantCompress)
            {
                this.m_z.EndDeflate();
            }
            else
            {
                this.m_z.EndInflate();
            }

            this.m_z = null;
        }


        public override void Close()
        {
            if (this.m_stream == null)
            {
                return;
            }

            try
            {
                this.finish();
            }
            finally
            {
                this.end();
                if (!this.m_leaveOpen)
                {
                    this.m_stream.Close();
                }

                this.m_stream = null;
            }
        }

        public override void Flush()
        {
            this.m_stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
            //_outStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.m_stream.SetLength(value);
        }


    #if NOT
        public int Read()
        {
            if (Read(_buf1, 0, 1) == 0)
                return 0;
            // calculate CRC after reading
            if (crc!=null)
                crc.SlurpBlock(_buf1,0,1);
            return (_buf1[0] & 0xFF);
        }
        #endif

        private bool nomoreinput;


        private string ReadZeroTerminatedString()
        {
            List<byte> list = new List<byte>();
            bool done = false;
            do
            {
                // workitem 7740
                int n = this.m_stream.Read(this.m_buf1, 0, 1);
                if (n != 1)
                {
                    throw new ZLibException("Unexpected EOF reading GZIP header.");
                }

                if (this.m_buf1[0] == 0)
                {
                    done = true;
                }
                else
                {
                    list.Add(this.m_buf1[0]);
                }
            } while (!done);

            byte[] a = list.ToArray();
            return GZipStream.iso8859dash1.GetString(a, 0, a.Length);
        }


        private int m_ReadAndValidateGzipHeader()
        {
            int totalBytesRead = 0;
            // read the header on the first read
            byte[] header = new byte[10];
            int n = this.m_stream.Read(header, 0, header.Length);

            // workitem 8501: handle edge case (decompress empty stream)
            if (n == 0)
            {
                return 0;
            }

            if (n != 10)
            {
                throw new ZLibException("Not a valid GZIP stream.");
            }

            if (header[0] != 0x1F || header[1] != 0x8B || header[2] != 8)
            {
                throw new ZLibException("Bad GZIP header.");
            }

            int timet = BitConverter.ToInt32(header, 4);
            this.m_GzipMtime = GZipStream.m_unixEpoch.AddSeconds(timet);
            totalBytesRead += n;
            if ((header[3] & 0x04) == 0x04)
            {
                // read and discard extra field
                n = this.m_stream.Read(header, 0, 2); // 2-byte length field
                totalBytesRead += n;

                short extraLength = (short) (header[0] + header[1] * 256);
                byte[] extra = new byte[extraLength];
                n = this.m_stream.Read(extra, 0, extra.Length);
                if (n != extraLength)
                {
                    throw new ZLibException("Unexpected end-of-file reading GZIP header.");
                }

                totalBytesRead += n;
            }

            if ((header[3] & 0x08) == 0x08)
            {
                this.m_GzipFileName = this.ReadZeroTerminatedString();
            }

            if ((header[3] & 0x10) == 0x010)
            {
                this.m_GzipComment = this.ReadZeroTerminatedString();
            }

            if ((header[3] & 0x02) == 0x02)
            {
                this.Read(this.m_buf1, 0, 1); // CRC16, ignore
            }

            return totalBytesRead;
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            // According to MS documentation, any implementation of the IO.Stream.Read function must:
            // (a) throw an exception if offset & count reference an invalid part of the buffer,
            //     or if count < 0, or if buffer is null
            // (b) return 0 only upon EOF, or if count = 0
            // (c) if not EOF, then return at least 1 byte, up to <count> bytes

            if (this.m_streamMode == StreamMode.Undefined)
            {
                if (!this.m_stream.CanRead)
                {
                    throw new ZLibException("The stream is not readable.");
                }

                // for the first read, set up some controls.
                this.m_streamMode = StreamMode.Reader;
                // (The first reference to m_z goes through the private accessor which
                // may initialize it.)
                this.z.AvailableBytesIn = 0;
                if (this.m_flavor == ZlibStreamFlavor.GZIP)
                {
                    this.m_gzipHeaderByteCount = this.m_ReadAndValidateGzipHeader();
                    // workitem 8501: handle edge case (decompress empty stream)
                    if (this.m_gzipHeaderByteCount == 0)
                    {
                        return 0;
                    }
                }
            }

            if (this.m_streamMode != StreamMode.Reader)
            {
                throw new ZLibException("Cannot Read after Writing.");
            }

            if (count == 0)
            {
                return 0;
            }

            if (this.nomoreinput && this.m_wantCompress)
            {
                return 0; // workitem 8557
            }

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (offset < buffer.GetLowerBound(0))
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (offset + count > buffer.GetLength(0))
            {
                throw new ArgumentOutOfRangeException("count");
            }

            int rc = 0;

            // set up the output of the deflate/inflate codec:
            this.m_z.OutputBuffer = buffer;
            this.m_z.NextOut = offset;
            this.m_z.AvailableBytesOut = count;

            // This is necessary in case m_workingBuffer has been resized. (new byte[])
            // (The first reference to m_workingBuffer goes through the private accessor which
            // may initialize it.)
            this.m_z.InputBuffer = this.workingBuffer;

            do
            {
                // need data in m_workingBuffer in order to deflate/inflate.  Here, we check if we have any.
                if (this.m_z.AvailableBytesIn == 0 && !this.nomoreinput)
                {
                    // No data available, so try to Read data from the captive stream.
                    this.m_z.NextIn = 0;
                    this.m_z.AvailableBytesIn = this.m_stream.Read(this.m_workingBuffer, 0, this.m_workingBuffer.Length);
                    if (this.m_z.AvailableBytesIn == 0)
                    {
                        this.nomoreinput = true;
                    }
                }

                // we have data in InputBuffer; now compress or decompress as appropriate
                rc = this.m_wantCompress
                    ? this.m_z.Deflate(this.m_flushMode)
                    : this.m_z.Inflate(this.m_flushMode);

                if (this.nomoreinput && rc == ZLibConstants.Z_BUF_ERROR)
                {
                    return 0;
                }

                if (rc != ZLibConstants.Z_OK && rc != ZLibConstants.Z_STREAM_END)
                {
                    throw new ZLibException(string.Format("{0}flating:  rc={1}  msg={2}", this.m_wantCompress ? "de" : "in", rc, this.m_z.Message));
                }

                if ((this.nomoreinput || rc == ZLibConstants.Z_STREAM_END) && this.m_z.AvailableBytesOut == count)
                {
                    break; // nothing more to read
                }
            }
            //while (_z.AvailableBytesOut == count && rc == ZLibConstants.Z_OK);
            while (this.m_z.AvailableBytesOut > 0 && !this.nomoreinput && rc == ZLibConstants.Z_OK);


            // workitem 8557
            // is there more room in output?
            if (this.m_z.AvailableBytesOut > 0)
            {
                if (rc == ZLibConstants.Z_OK && this.m_z.AvailableBytesIn == 0)
                {
                    // deferred
                }

                // are we completely done reading?
                if (this.nomoreinput)
                {
                    // and in compression?
                    if (this.m_wantCompress)
                    {
                        // no more input data available; therefore we flush to
                        // try to complete the read
                        rc = this.m_z.Deflate(FlushType.Finish);

                        if (rc != ZLibConstants.Z_OK && rc != ZLibConstants.Z_STREAM_END)
                        {
                            throw new ZLibException(string.Format("Deflating:  rc={0}  msg={1}", rc, this.m_z.Message));
                        }
                    }
                }
            }


            rc = count - this.m_z.AvailableBytesOut;

            // calculate CRC after reading
            if (this.crc != null)
            {
                this.crc.SlurpBlock(buffer, offset, rc);
            }

            return rc;
        }


        public override bool CanRead
        {
            get
            {
                return this.m_stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.m_stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.m_stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return this.m_stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        internal enum StreamMode
        {
            Writer,
            Reader,
            Undefined
        }


        public static void CompressString(string s, Stream compressor)
        {
            byte[] uncompressed = Encoding.UTF8.GetBytes(s);
            using (compressor)
            {
                compressor.Write(uncompressed, 0, uncompressed.Length);
            }
        }

        public static void CompressBuffer(byte[] b, Stream compressor)
        {
            // workitem 8460
            using (compressor)
            {
                compressor.Write(b, 0, b.Length);
            }
        }

        public static string UncompressString(byte[] compressed, Stream decompressor)
        {
            // workitem 8460
            byte[] working = new byte[1024];
            Encoding encoding = Encoding.UTF8;
            using (MemoryStream output = new MemoryStream())
            {
                using (decompressor)
                {
                    int n;
                    while ((n = decompressor.Read(working, 0, working.Length)) != 0)
                    {
                        output.Write(working, 0, n);
                    }
                }

                // reset to allow read from start
                output.Seek(0, SeekOrigin.Begin);
                StreamReader sr = new StreamReader(output, encoding);
                return sr.ReadToEnd();
            }
        }

        public static byte[] UncompressBuffer(byte[] compressed, Stream decompressor)
        {
            // workitem 8460
            byte[] working = new byte[1024];
            using (MemoryStream output = new MemoryStream())
            {
                using (decompressor)
                {
                    int n;
                    while ((n = decompressor.Read(working, 0, working.Length)) != 0)
                    {
                        output.Write(working, 0, n);
                    }
                }

                return output.ToArray();
            }
        }
    }
}