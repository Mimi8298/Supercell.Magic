// GZipStream.cs
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
// Time-stamp: <2011-July-11 21:42:34>
//
// ------------------------------------------------------------------
//
// This module defines the GZipStream class, which can be used as a replacement for
// the System.IO.Compression.GZipStream class in the .NET BCL.  NB: The design is not
// completely OO clean: there is some intelligence in the ZLibBaseStream that reads the
// GZip header.
//
// ------------------------------------------------------------------


namespace Supercell.Magic.Servers.Core.Libs.ZLib
{
    using System;
    using System.IO;
    using System.Text;

    public class GZipStream : Stream
    {
        // GZip format
        // source: http://tools.ietf.org/html/rfc1952
        //
        //  header id:           2 bytes    1F 8B
        //  compress method      1 byte     8= DEFLATE (none other supported)
        //  flag                 1 byte     bitfield (See below)
        //  mtime                4 bytes    time_t (seconds since jan 1, 1970 UTC of the file.
        //  xflg                 1 byte     2 = max compress used , 4 = max speed (can be ignored)
        //  OS                   1 byte     OS for originating archive. set to 0xFF in compression.
        //  extra field length   2 bytes    optional - only if FEXTRA is set.
        //  extra field          varies
        //  filename             varies     optional - if FNAME is set.  zero terminated. ISO-8859-1.
        //  file comment         varies     optional - if FCOMMENT is set. zero terminated. ISO-8859-1.
        //  crc16                1 byte     optional - present only if FHCRC bit is set
        //  compressed data      varies
        //  CRC32                4 bytes
        //  isize                4 bytes    data size modulo 2^32
        //
        //     FLG (FLaGs)
        //                bit 0   FTEXT - indicates file is ASCII text (can be safely ignored)
        //                bit 1   FHCRC - there is a CRC16 for the header immediately following the header
        //                bit 2   FEXTRA - extra fields are present
        //                bit 3   FNAME - the zero-terminated filename is present. encoding; ISO-8859-1.
        //                bit 4   FCOMMENT  - a zero-terminated file comment is present. encoding: ISO-8859-1
        //                bit 5   reserved
        //                bit 6   reserved
        //                bit 7   reserved
        //
        // On consumption:
        // Extra field is a bunch of nonsense and can be safely ignored.
        // Header CRC and OS, likewise.
        //
        // on generation:
        // all optional fields get 0, except for the OS, which gets 255.
        //


        public string Comment
        {
            get
            {
                return this.m_Comment;
            }
            set
            {
                if (this.m_disposed)
                {
                    throw new ObjectDisposedException("GZipStream");
                }

                this.m_Comment = value;
            }
        }

        public string FileName
        {
            get
            {
                return this.m_FileName;
            }
            set
            {
                if (this.m_disposed)
                {
                    throw new ObjectDisposedException("GZipStream");
                }

                this.m_FileName = value;
                if (this.m_FileName == null)
                {
                    return;
                }

                if (this.m_FileName.IndexOf("/") != -1)
                {
                    this.m_FileName = this.m_FileName.Replace("/", "\\");
                }

                if (this.m_FileName.EndsWith("\\"))
                {
                    throw new Exception("Illegal filename");
                }

                if (this.m_FileName.IndexOf("\\") != -1)
                {
                    // trim any leading path
                    this.m_FileName = Path.GetFileName(this.m_FileName);
                }
            }
        }

        public DateTime? LastModified;

        public int Crc32 { get; private set; }

        private int m_headerByteCount;
        internal ZLibBaseStream m_baseStream;
        private bool m_disposed;
        private bool m_firstReadDone;
        private string m_FileName;
        private string m_Comment;


        public GZipStream(Stream stream, CompressionMode mode)
            : this(stream, mode, CompressionLevel.Default, false)
        {
        }

        public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level)
            : this(stream, mode, level, false)
        {
        }

        public GZipStream(Stream stream, CompressionMode mode, bool leaveOpen)
            : this(stream, mode, CompressionLevel.Default, leaveOpen)
        {
        }

        public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
        {
            this.m_baseStream = new ZLibBaseStream(stream, mode, level, ZlibStreamFlavor.GZIP, leaveOpen);
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
                    throw new ObjectDisposedException("GZipStream");
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
                    throw new ObjectDisposedException("GZipStream");
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

    #region Stream methods

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!this.m_disposed)
                {
                    if (disposing && this.m_baseStream != null)
                    {
                        this.m_baseStream.Close();
                        this.Crc32 = this.m_baseStream.Crc32;
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
                    throw new ObjectDisposedException("GZipStream");
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
                    throw new ObjectDisposedException("GZipStream");
                }

                return this.m_baseStream.m_stream.CanWrite;
            }
        }

        public override void Flush()
        {
            if (this.m_disposed)
            {
                throw new ObjectDisposedException("GZipStream");
            }

            this.m_baseStream.Flush();
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                if (this.m_baseStream.m_streamMode == ZLibBaseStream.StreamMode.Writer)
                {
                    return this.m_baseStream.m_z.TotalBytesOut + this.m_headerByteCount;
                }

                if (this.m_baseStream.m_streamMode == ZLibBaseStream.StreamMode.Reader)
                {
                    return this.m_baseStream.m_z.TotalBytesIn + this.m_baseStream.m_gzipHeaderByteCount;
                }

                return 0;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.m_disposed)
            {
                throw new ObjectDisposedException("GZipStream");
            }

            int n = this.m_baseStream.Read(buffer, offset, count);

            // Console.WriteLine("GZipStream::Read(buffer, off({0}), c({1}) = {2}", offset, count, n);
            // Console.WriteLine( Util.FormatByteArray(buffer, offset, n) );

            if (!this.m_firstReadDone)
            {
                this.m_firstReadDone = true;
                this.FileName = this.m_baseStream.m_GzipFileName;
                this.Comment = this.m_baseStream.m_GzipComment;
            }

            return n;
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.m_disposed)
            {
                throw new ObjectDisposedException("GZipStream");
            }

            if (this.m_baseStream.m_streamMode == ZLibBaseStream.StreamMode.Undefined)
            {
                //Console.WriteLine("GZipStream: First write");
                if (this.m_baseStream.m_wantCompress)
                {
                    // first write in compression, therefore, emit the GZIP header
                    this.m_headerByteCount = this.EmitHeader();
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            this.m_baseStream.Write(buffer, offset, count);
        }

    #endregion


        internal static readonly DateTime m_unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static readonly Encoding iso8859dash1 = Encoding.GetEncoding("iso-8859-1");


        private int EmitHeader()
        {
            byte[] commentBytes = this.Comment == null ? null : GZipStream.iso8859dash1.GetBytes(this.Comment);
            byte[] filenameBytes = this.FileName == null ? null : GZipStream.iso8859dash1.GetBytes(this.FileName);

            int cbLength = this.Comment == null ? 0 : commentBytes.Length + 1;
            int fnLength = this.FileName == null ? 0 : filenameBytes.Length + 1;

            int bufferLength = 10 + cbLength + fnLength;
            byte[] header = new byte[bufferLength];
            int i = 0;
            // ID
            header[i++] = 0x1F;
            header[i++] = 0x8B;

            // compression method
            header[i++] = 8;
            byte flag = 0;
            if (this.Comment != null)
            {
                flag ^= 0x10;
            }

            if (this.FileName != null)
            {
                flag ^= 0x8;
            }

            // flag
            header[i++] = flag;

            // mtime
            if (!this.LastModified.HasValue)
            {
                this.LastModified = DateTime.Now;
            }

            TimeSpan delta = this.LastModified.Value - GZipStream.m_unixEpoch;
            int timet = (int) delta.TotalSeconds;
            Array.Copy(BitConverter.GetBytes(timet), 0, header, i, 4);
            i += 4;

            // xflg
            header[i++] = 0; // this field is totally useless
            // OS
            header[i++] = 0xFF; // 0xFF == unspecified

            // extra field length - only if FEXTRA is set, which it is not.
            //header[i++]= 0;
            //header[i++]= 0;

            // filename
            if (fnLength != 0)
            {
                Array.Copy(filenameBytes, 0, header, i, fnLength - 1);
                i += fnLength - 1;
                header[i++] = 0; // terminate
            }

            // comment
            if (cbLength != 0)
            {
                Array.Copy(commentBytes, 0, header, i, cbLength - 1);
                i += cbLength - 1;
                header[i++] = 0; // terminate
            }

            this.m_baseStream.m_stream.Write(header, 0, header.Length);

            return header.Length; // bytes written
        }


        public static byte[] CompressString(string s)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Stream compressor =
                    new GZipStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZLibBaseStream.CompressString(s, compressor);
                return ms.ToArray();
            }
        }


        public static byte[] CompressBuffer(byte[] b)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Stream compressor =
                    new GZipStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);

                ZLibBaseStream.CompressBuffer(b, compressor);
                return ms.ToArray();
            }
        }


        public static string UncompressString(byte[] compressed)
        {
            using (MemoryStream input = new MemoryStream(compressed))
            {
                Stream decompressor = new GZipStream(input, CompressionMode.Decompress);
                return ZLibBaseStream.UncompressString(compressed, decompressor);
            }
        }


        public static byte[] UncompressBuffer(byte[] compressed)
        {
            using (MemoryStream input = new MemoryStream(compressed))
            {
                Stream decompressor =
                    new GZipStream(input, CompressionMode.Decompress);

                return ZLibBaseStream.UncompressBuffer(compressed, decompressor);
            }
        }
    }
}