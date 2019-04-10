// ZLibCodec.cs
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
// Time-stamp: <2009-November-03 15:40:51>
//
// ------------------------------------------------------------------
//
// This module defines a Codec for ZLIB compression and
// decompression. This code extends code that was based the jzlib
// implementation of zlib, but this code is completely novel.  The codec
// class is new, and encapsulates some behaviors that are new, and some
// that were present in other classes in the jzlib code base.  In
// keeping with the license for jzlib, the copyright to the jzlib code
// is included below.
//
// ------------------------------------------------------------------
// 
// Copyright (c) 2000,2001,2002,2003 ymnk, JCraft,Inc. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright 
// notice, this list of conditions and the following disclaimer in 
// the documentation and/or other materials provided with the distribution.
// 
// 3. The names of the authors may not be used to endorse or promote products
// derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
// INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// -----------------------------------------------------------------------
//
// This program is based on zlib-1.1.3; credit to authors
// Jean-loup Gailly(jloup@gzip.org) and Mark Adler(madler@alumni.caltech.edu)
// and contributors of zlib.
//
// -----------------------------------------------------------------------


using Interop = System.Runtime.InteropServices;

namespace Supercell.Magic.Servers.Core.Libs.ZLib
{
    using System;

    [Interop.GuidAttribute("ebc25cf6-9120-4283-b972-0e5520d0000D"), Interop.ComVisibleAttribute(true), Interop.ClassInterfaceAttribute(Interop.ClassInterfaceType.AutoDispatch)]
#if !NETCF
#endif
    public sealed class ZLibCodec
    {
        public byte[] InputBuffer;

        public int NextIn;

        public int AvailableBytesIn;

        public long TotalBytesIn;

        public byte[] OutputBuffer;

        public int NextOut;

        public int AvailableBytesOut;

        public long TotalBytesOut;

        public string Message;

        internal DeflateManager dstate;
        internal InflateManager istate;

        internal uint m_Adler32;

        public CompressionLevel CompressLevel = CompressionLevel.Default;

        public int WindowBits = ZLibConstants.WindowBitsDefault;

        public CompressionStrategy Strategy = CompressionStrategy.Default;


        public int Adler32
        {
            get
            {
                return (int) this.m_Adler32;
            }
        }


        public ZLibCodec()
        {
        }

        public ZLibCodec(CompressionMode mode)
        {
            if (mode == CompressionMode.Compress)
            {
                int rc = this.InitializeDeflate();
                if (rc != ZLibConstants.Z_OK)
                {
                    throw new ZLibException("Cannot initialize for deflate.");
                }
            }
            else if (mode == CompressionMode.Decompress)
            {
                int rc = this.InitializeInflate();
                if (rc != ZLibConstants.Z_OK)
                {
                    throw new ZLibException("Cannot initialize for inflate.");
                }
            }
            else
            {
                throw new ZLibException("Invalid ZlibStreamFlavor.");
            }
        }

        public int InitializeInflate()
        {
            return this.InitializeInflate(this.WindowBits);
        }

        public int InitializeInflate(bool expectRfc1950Header)
        {
            return this.InitializeInflate(this.WindowBits, expectRfc1950Header);
        }

        public int InitializeInflate(int windowBits)
        {
            this.WindowBits = windowBits;
            return this.InitializeInflate(windowBits, true);
        }

        public int InitializeInflate(int windowBits, bool expectRfc1950Header)
        {
            this.WindowBits = windowBits;
            if (this.dstate != null)
            {
                throw new ZLibException("You may not call InitializeInflate() after calling InitializeDeflate().");
            }

            this.istate = new InflateManager(expectRfc1950Header);
            return this.istate.Initialize(this, windowBits);
        }

        public int Inflate(FlushType flush)
        {
            if (this.istate == null)
            {
                throw new ZLibException("No Inflate State!");
            }

            return this.istate.Inflate(flush);
        }


        public int EndInflate()
        {
            if (this.istate == null)
            {
                throw new ZLibException("No Inflate State!");
            }

            int ret = this.istate.End();
            this.istate = null;
            return ret;
        }

        public int SyncInflate()
        {
            if (this.istate == null)
            {
                throw new ZLibException("No Inflate State!");
            }

            return this.istate.Sync();
        }

        public int InitializeDeflate()
        {
            return this.m_InternalInitializeDeflate(true);
        }

        public int InitializeDeflate(CompressionLevel level)
        {
            this.CompressLevel = level;
            return this.m_InternalInitializeDeflate(true);
        }


        public int InitializeDeflate(CompressionLevel level, bool wantRfc1950Header)
        {
            this.CompressLevel = level;
            return this.m_InternalInitializeDeflate(wantRfc1950Header);
        }


        public int InitializeDeflate(CompressionLevel level, int bits)
        {
            this.CompressLevel = level;
            this.WindowBits = bits;
            return this.m_InternalInitializeDeflate(true);
        }

        public int InitializeDeflate(CompressionLevel level, int bits, bool wantRfc1950Header)
        {
            this.CompressLevel = level;
            this.WindowBits = bits;
            return this.m_InternalInitializeDeflate(wantRfc1950Header);
        }

        private int m_InternalInitializeDeflate(bool wantRfc1950Header)
        {
            if (this.istate != null)
            {
                throw new ZLibException("You may not call InitializeDeflate() after calling InitializeInflate().");
            }

            this.dstate = new DeflateManager();
            this.dstate.WantRfc1950HeaderBytes = wantRfc1950Header;

            return this.dstate.Initialize(this, this.CompressLevel, this.WindowBits, this.Strategy);
        }

        public int Deflate(FlushType flush)
        {
            if (this.dstate == null)
            {
                throw new ZLibException("No Deflate State!");
            }

            return this.dstate.Deflate(flush);
        }

        public int EndDeflate()
        {
            if (this.dstate == null)
            {
                throw new ZLibException("No Deflate State!");
            }

            // TODO: dinoch Tue, 03 Nov 2009  15:39 (test this)
            //int ret = dstate.End();
            this.dstate = null;
            return ZLibConstants.Z_OK; //ret;
        }

        public void ResetDeflate()
        {
            if (this.dstate == null)
            {
                throw new ZLibException("No Deflate State!");
            }

            this.dstate.Reset();
        }


        public int SetDeflateParams(CompressionLevel level, CompressionStrategy strategy)
        {
            if (this.dstate == null)
            {
                throw new ZLibException("No Deflate State!");
            }

            return this.dstate.SetParams(level, strategy);
        }


        public int SetDictionary(byte[] dictionary)
        {
            if (this.istate != null)
            {
                return this.istate.SetDictionary(dictionary);
            }

            if (this.dstate != null)
            {
                return this.dstate.SetDictionary(dictionary);
            }

            throw new ZLibException("No Inflate or Deflate state!");
        }

        // Flush as much pending output as possible. All deflate() output goes
        // through this function so some applications may wish to modify it
        // to avoid allocating a large strm->next_out buffer and copying into it.
        // (See also read_buf()).
        internal void flush_pending()
        {
            int len = this.dstate.pendingCount;

            if (len > this.AvailableBytesOut)
            {
                len = this.AvailableBytesOut;
            }

            if (len == 0)
            {
                return;
            }

            if (this.dstate.pending.Length <= this.dstate.nextPending || this.OutputBuffer.Length <= this.NextOut || this.dstate.pending.Length < this.dstate.nextPending + len ||
                this.OutputBuffer.Length < this.NextOut + len)
            {
                throw new ZLibException(string.Format("Invalid State. (pending.Length={0}, pendingCount={1})", this.dstate.pending.Length, this.dstate.pendingCount));
            }

            Array.Copy(this.dstate.pending, this.dstate.nextPending, this.OutputBuffer, this.NextOut, len);

            this.NextOut += len;
            this.dstate.nextPending += len;
            this.TotalBytesOut += len;
            this.AvailableBytesOut -= len;
            this.dstate.pendingCount -= len;
            if (this.dstate.pendingCount == 0)
            {
                this.dstate.nextPending = 0;
            }
        }

        // Read a new buffer from the current input stream, update the adler32
        // and total number of bytes read.  All deflate() input goes through
        // this function so some applications may wish to modify it to avoid
        // allocating a large strm->next_in buffer and copying from it.
        // (See also flush_pending()).
        internal int read_buf(byte[] buf, int start, int size)
        {
            int len = this.AvailableBytesIn;

            if (len > size)
            {
                len = size;
            }

            if (len == 0)
            {
                return 0;
            }

            this.AvailableBytesIn -= len;

            if (this.dstate.WantRfc1950HeaderBytes)
            {
                this.m_Adler32 = Adler.Adler32(this.m_Adler32, this.InputBuffer, this.NextIn, len);
            }

            Array.Copy(this.InputBuffer, this.NextIn, buf, start, len);
            this.NextIn += len;
            this.TotalBytesIn += len;
            return len;
        }
    }
}