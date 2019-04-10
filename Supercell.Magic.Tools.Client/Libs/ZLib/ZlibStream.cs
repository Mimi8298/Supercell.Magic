// ZLibStream.cs
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
// Time-stamp: <2011-July-31 14:53:33>
//
// ------------------------------------------------------------------
//
// This module defines the ZLibStream class, which is similar in idea to
// the System.IO.Compression.DeflateStream and
// System.IO.Compression.GZipStream classes in the .NET BCL.
//
// ------------------------------------------------------------------

namespace Supercell.Magic.Tools.Client.Libs.ZLib
{
    using System;
    using System.IO;

    public class ZLibStream : Stream
    {
        internal ZLibBaseStream m_baseStream;
        private bool m_disposed;

        public ZLibStream(Stream stream, CompressionMode mode)
            : this(stream, mode, CompressionLevel.Default, false)
        {
        }

        public ZLibStream(Stream stream, CompressionMode mode, CompressionLevel level)
            : this(stream, mode, level, false)
        {
        }

        public ZLibStream(Stream stream, CompressionMode mode, bool leaveOpen)
            : this(stream, mode, CompressionLevel.Default, leaveOpen)
        {
        }

        public ZLibStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
        {
            this.m_baseStream = new ZLibBaseStream(stream, mode, level, ZlibStreamFlavor.ZLIB, leaveOpen);
        }

    #region Zlib properties

        public virtual FlushType FlushMode
        {
            get
            {
                return this.m_baseStream.m_flushMode;
            }
            set
            {
                if (this.m_disposed)
                {
                    throw new ObjectDisposedException("ZLibStream");
                }

                this.m_baseStream.m_flushMode = value;
            }
        }

        public int BufferSize
        {
            get
            {
                return this.m_baseStream.m_bufferSize;
            }
            set
            {
                if (this.m_disposed)
                {
                    throw new ObjectDisposedException("ZLibStream");
                }

                if (this.m_baseStream.m_workingBuffer != null)
                {
                    throw new ZLibException("The working buffer is already set.");
                }

                if (value < ZLibConstants.WorkingBufferSizeMin)
                {
                    throw new ZLibException(string.Format("Don't be silly. {0} bytes?? Use a bigger buffer, at least {1}.", value, ZLibConstants.WorkingBufferSizeMin));
                }

                this.m_baseStream.m_bufferSize = value;
            }
        }

        public virtual long TotalIn
        {
            get
            {
                return this.m_baseStream.m_z.TotalBytesIn;
            }
        }

        public virtual long TotalOut
        {
            get
            {
                return this.m_baseStream.m_z.TotalBytesOut;
            }
        }

    #endregion

    #region System.IO.Stream methods

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!this.m_disposed)
                {
                    if (disposing && this.m_baseStream != null)
                    {
                        this.m_baseStream.Close();
                    }

                    this.m_disposed = true;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }


        public override bool CanRead
        {
            get
            {
                if (this.m_disposed)
                {
                    throw new ObjectDisposedException("ZLibStream");
                }

                return this.m_baseStream.m_stream.CanRead;
            }
        }

        public override bool CanSeek
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
                if (this.m_disposed)
                {
                    throw new ObjectDisposedException("ZLibStream");
                }

                return this.m_baseStream.m_stream.CanWrite;
            }
        }

        public override void Flush()
        {
            if (this.m_disposed)
            {
                throw new ObjectDisposedException("ZLibStream");
            }

            this.m_baseStream.Flush();
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
                if (this.m_baseStream.m_streamMode == ZLibBaseStream.StreamMode.Writer)
                {
                    return this.m_baseStream.m_z.TotalBytesOut;
                }

                if (this.m_baseStream.m_streamMode == ZLibBaseStream.StreamMode.Reader)
                {
                    return this.m_baseStream.m_z.TotalBytesIn;
                }

                return 0;
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.m_disposed)
            {
                throw new ObjectDisposedException("ZLibStream");
            }

            return this.m_baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.m_disposed)
            {
                throw new ObjectDisposedException("ZLibStream");
            }

            this.m_baseStream.Write(buffer, offset, count);
        }

    #endregion


        public static byte[] CompressString(string s)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Stream compressor =
                    new ZLibStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZLibBaseStream.CompressString(s, compressor);
                return ms.ToArray();
            }
        }


        public static byte[] CompressBuffer(byte[] b, CompressionLevel level)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Stream compressor =
                    new ZLibStream(ms, CompressionMode.Compress, level);

                ZLibBaseStream.CompressBuffer(b, compressor);
                return ms.ToArray();
            }
        }


        public static string UncompressString(byte[] compressed)
        {
            using (MemoryStream input = new MemoryStream(compressed))
            {
                Stream decompressor =
                    new ZLibStream(input, CompressionMode.Decompress);

                return ZLibBaseStream.UncompressString(compressed, decompressor);
            }
        }


        public static byte[] UncompressBuffer(byte[] compressed)
        {
            using (MemoryStream input = new MemoryStream(compressed))
            {
                Stream decompressor =
                    new ZLibStream(input, CompressionMode.Decompress);

                return ZLibBaseStream.UncompressBuffer(compressed, decompressor);
            }
        }
    }
}