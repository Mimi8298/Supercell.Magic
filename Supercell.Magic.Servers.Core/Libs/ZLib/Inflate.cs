// Inflate.cs
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
// Time-stamp: <2010-January-08 18:32:12>
//
// ------------------------------------------------------------------
//
// This module defines classes for decompression. This code is derived
// from the jzlib implementation of zlib, but significantly modified.
// The object model is not the same, and many of the behaviors are
// different.  Nonetheless, in keeping with the license for jzlib, I am
// reproducing the copyright to that code here.
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


namespace Supercell.Magic.Servers.Core.Libs.ZLib
{
    using System;

    internal sealed class InflateBlocks
    {
        private const int MANY = 1440;

        // Table for deflate from PKZIP's appnote.txt.
        internal static readonly int[] border = {16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15};

        private enum InflateBlockMode
        {
            TYPE = 0, // get type bits (3, including end bit)
            LENS = 1, // get lengths for stored
            STORED = 2, // processing stored block
            TABLE = 3, // get table lengths
            BTREE = 4, // get bit lengths tree for a dynamic block
            DTREE = 5, // get length, distance trees for a dynamic block
            CODES = 6, // processing fixed or dynamic block
            DRY = 7, // output remaining window bytes
            DONE = 8, // finished last block, done
            BAD = 9 // ot a data error--stuck here
        }

        private InflateBlockMode mode; // current inflate_block mode

        internal int left; // if STORED, bytes left to copy

        internal int table; // table lengths (14 bits)
        internal int index; // index into blens (or border)
        internal int[] blens; // bit lengths of codes
        internal int[] bb = new int[1]; // bit length tree depth
        internal int[] tb = new int[1]; // bit length decoding tree

        internal InflateCodes codes = new InflateCodes(); // if CODES, current state

        internal int last; // true if this block is the last block

        internal ZLibCodec m_codec; // pointer back to this zlib stream

        // mode independent information
        internal int bitk; // bits in bit buffer
        internal int bitb; // bit buffer
        internal int[] hufts; // single malloc for tree space
        internal byte[] window; // sliding window
        internal int end; // one byte after sliding window
        internal int readAt; // window read pointer
        internal int writeAt; // window write pointer
        internal object checkfn; // check function
        internal uint check; // check on output

        internal InfTree inftree = new InfTree();

        internal InflateBlocks(ZLibCodec codec, object checkfn, int w)
        {
            this.m_codec = codec;
            this.hufts = new int[InflateBlocks.MANY * 3];
            this.window = new byte[w];
            this.end = w;
            this.checkfn = checkfn;
            this.mode = InflateBlockMode.TYPE;
            this.Reset();
        }

        internal uint Reset()
        {
            uint oldCheck = this.check;
            this.mode = InflateBlockMode.TYPE;
            this.bitk = 0;
            this.bitb = 0;
            this.readAt = this.writeAt = 0;

            if (this.checkfn != null)
            {
                this.m_codec.m_Adler32 = this.check = Adler.Adler32(0, null, 0, 0);
            }

            return oldCheck;
        }


        internal int Process(int r)
        {
            int t; // temporary storage
            int b; // bit buffer
            int k; // bits in bit buffer
            int p; // input data pointer
            int n; // bytes available there
            int q; // output window write pointer
            int m; // bytes to end of window or read pointer

            // copy input/output information to locals (UPDATE macro restores)

            p = this.m_codec.NextIn;
            n = this.m_codec.AvailableBytesIn;
            b = this.bitb;
            k = this.bitk;

            q = this.writeAt;
            m = q < this.readAt ? this.readAt - q - 1 : this.end - q;


            // process input based on current state
            while (true)
            {
                switch (this.mode)
                {
                    case InflateBlockMode.TYPE:

                        while (k < 3)
                        {
                            if (n != 0)
                            {
                                r = ZLibConstants.Z_OK;
                            }
                            else
                            {
                                this.bitb = b;
                                this.bitk = k;
                                this.m_codec.AvailableBytesIn = n;
                                this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                                this.m_codec.NextIn = p;
                                this.writeAt = q;
                                return this.Flush(r);
                            }

                            n--;
                            b |= (this.m_codec.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        t = b & 7;
                        this.last = t & 1;

                        switch ((uint) t >> 1)
                        {
                            case 0: // stored
                                b >>= 3;
                                k -= 3;
                                t = k & 7; // go to byte boundary
                                b >>= t;
                                k -= t;
                                this.mode = InflateBlockMode.LENS; // get length of stored block
                                break;

                            case 1: // fixed
                                int[] bl = new int[1];
                                int[] bd = new int[1];
                                int[][] tl = new int[1][];
                                int[][] td = new int[1][];
                                InfTree.inflate_trees_fixed(bl, bd, tl, td, this.m_codec);
                                this.codes.Init(bl[0], bd[0], tl[0], 0, td[0], 0);
                                b >>= 3;
                                k -= 3;
                                this.mode = InflateBlockMode.CODES;
                                break;

                            case 2: // dynamic
                                b >>= 3;
                                k -= 3;
                                this.mode = InflateBlockMode.TABLE;
                                break;

                            case 3: // illegal
                                b >>= 3;
                                k -= 3;
                                this.mode = InflateBlockMode.BAD;
                                this.m_codec.Message = "invalid block type";
                                r = ZLibConstants.Z_DATA_ERROR;
                                this.bitb = b;
                                this.bitk = k;
                                this.m_codec.AvailableBytesIn = n;
                                this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                                this.m_codec.NextIn = p;
                                this.writeAt = q;
                                return this.Flush(r);
                        }

                        break;

                    case InflateBlockMode.LENS:

                        while (k < 32)
                        {
                            if (n != 0)
                            {
                                r = ZLibConstants.Z_OK;
                            }
                            else
                            {
                                this.bitb = b;
                                this.bitk = k;
                                this.m_codec.AvailableBytesIn = n;
                                this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                                this.m_codec.NextIn = p;
                                this.writeAt = q;
                                return this.Flush(r);
                            }

                            ;
                            n--;
                            b |= (this.m_codec.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        if (((~b >> 16) & 0xffff) != (b & 0xffff))
                        {
                            this.mode = InflateBlockMode.BAD;
                            this.m_codec.Message = "invalid stored block lengths";
                            r = ZLibConstants.Z_DATA_ERROR;

                            this.bitb = b;
                            this.bitk = k;
                            this.m_codec.AvailableBytesIn = n;
                            this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                            this.m_codec.NextIn = p;
                            this.writeAt = q;
                            return this.Flush(r);
                        }

                        this.left = b & 0xffff;
                        b = k = 0; // dump bits
                        this.mode = this.left != 0 ? InflateBlockMode.STORED : (this.last != 0 ? InflateBlockMode.DRY : InflateBlockMode.TYPE);
                        break;

                    case InflateBlockMode.STORED:
                        if (n == 0)
                        {
                            this.bitb = b;
                            this.bitk = k;
                            this.m_codec.AvailableBytesIn = n;
                            this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                            this.m_codec.NextIn = p;
                            this.writeAt = q;
                            return this.Flush(r);
                        }

                        if (m == 0)
                        {
                            if (q == this.end && this.readAt != 0)
                            {
                                q = 0;
                                m = q < this.readAt ? this.readAt - q - 1 : this.end - q;
                            }

                            if (m == 0)
                            {
                                this.writeAt = q;
                                r = this.Flush(r);
                                q = this.writeAt;
                                m = q < this.readAt ? this.readAt - q - 1 : this.end - q;
                                if (q == this.end && this.readAt != 0)
                                {
                                    q = 0;
                                    m = q < this.readAt ? this.readAt - q - 1 : this.end - q;
                                }

                                if (m == 0)
                                {
                                    this.bitb = b;
                                    this.bitk = k;
                                    this.m_codec.AvailableBytesIn = n;
                                    this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                                    this.m_codec.NextIn = p;
                                    this.writeAt = q;
                                    return this.Flush(r);
                                }
                            }
                        }

                        r = ZLibConstants.Z_OK;

                        t = this.left;
                        if (t > n)
                        {
                            t = n;
                        }

                        if (t > m)
                        {
                            t = m;
                        }

                        Array.Copy(this.m_codec.InputBuffer, p, this.window, q, t);
                        p += t;
                        n -= t;
                        q += t;
                        m -= t;
                        if ((this.left -= t) != 0)
                        {
                            break;
                        }

                        this.mode = this.last != 0 ? InflateBlockMode.DRY : InflateBlockMode.TYPE;
                        break;

                    case InflateBlockMode.TABLE:

                        while (k < 14)
                        {
                            if (n != 0)
                            {
                                r = ZLibConstants.Z_OK;
                            }
                            else
                            {
                                this.bitb = b;
                                this.bitk = k;
                                this.m_codec.AvailableBytesIn = n;
                                this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                                this.m_codec.NextIn = p;
                                this.writeAt = q;
                                return this.Flush(r);
                            }

                            n--;
                            b |= (this.m_codec.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        this.table = t = b & 0x3fff;
                        if ((t & 0x1f) > 29 || ((t >> 5) & 0x1f) > 29)
                        {
                            this.mode = InflateBlockMode.BAD;
                            this.m_codec.Message = "too many length or distance symbols";
                            r = ZLibConstants.Z_DATA_ERROR;

                            this.bitb = b;
                            this.bitk = k;
                            this.m_codec.AvailableBytesIn = n;
                            this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                            this.m_codec.NextIn = p;
                            this.writeAt = q;
                            return this.Flush(r);
                        }

                        t = 258 + (t & 0x1f) + ((t >> 5) & 0x1f);
                        if (this.blens == null || this.blens.Length < t)
                        {
                            this.blens = new int[t];
                        }
                        else
                        {
                            Array.Clear(this.blens, 0, t);
                            // for (int i = 0; i < t; i++)
                            // {
                            //     blens[i] = 0;
                            // }
                        }

                        b >>= 14;
                        k -= 14;


                        this.index = 0;
                        this.mode = InflateBlockMode.BTREE;
                        goto case InflateBlockMode.BTREE;

                    case InflateBlockMode.BTREE:
                        while (this.index < 4 + (this.table >> 10))
                        {
                            while (k < 3)
                            {
                                if (n != 0)
                                {
                                    r = ZLibConstants.Z_OK;
                                }
                                else
                                {
                                    this.bitb = b;
                                    this.bitk = k;
                                    this.m_codec.AvailableBytesIn = n;
                                    this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                                    this.m_codec.NextIn = p;
                                    this.writeAt = q;
                                    return this.Flush(r);
                                }

                                n--;
                                b |= (this.m_codec.InputBuffer[p++] & 0xff) << k;
                                k += 8;
                            }

                            this.blens[InflateBlocks.border[this.index++]] = b & 7;

                            b >>= 3;
                            k -= 3;
                        }

                        while (this.index < 19)
                        {
                            this.blens[InflateBlocks.border[this.index++]] = 0;
                        }

                        this.bb[0] = 7;
                        t = this.inftree.inflate_trees_bits(this.blens, this.bb, this.tb, this.hufts, this.m_codec);
                        if (t != ZLibConstants.Z_OK)
                        {
                            r = t;
                            if (r == ZLibConstants.Z_DATA_ERROR)
                            {
                                this.blens = null;
                                this.mode = InflateBlockMode.BAD;
                            }

                            this.bitb = b;
                            this.bitk = k;
                            this.m_codec.AvailableBytesIn = n;
                            this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                            this.m_codec.NextIn = p;
                            this.writeAt = q;
                            return this.Flush(r);
                        }

                        this.index = 0;
                        this.mode = InflateBlockMode.DTREE;
                        goto case InflateBlockMode.DTREE;

                    case InflateBlockMode.DTREE:
                        while (true)
                        {
                            t = this.table;
                            if (!(this.index < 258 + (t & 0x1f) + ((t >> 5) & 0x1f)))
                            {
                                break;
                            }

                            int i, j, c;

                            t = this.bb[0];

                            while (k < t)
                            {
                                if (n != 0)
                                {
                                    r = ZLibConstants.Z_OK;
                                }
                                else
                                {
                                    this.bitb = b;
                                    this.bitk = k;
                                    this.m_codec.AvailableBytesIn = n;
                                    this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                                    this.m_codec.NextIn = p;
                                    this.writeAt = q;
                                    return this.Flush(r);
                                }

                                n--;
                                b |= (this.m_codec.InputBuffer[p++] & 0xff) << k;
                                k += 8;
                            }

                            t = this.hufts[(this.tb[0] + (b & InternalInflateConstants.InflateMask[t])) * 3 + 1];
                            c = this.hufts[(this.tb[0] + (b & InternalInflateConstants.InflateMask[t])) * 3 + 2];

                            if (c < 16)
                            {
                                b >>= t;
                                k -= t;
                                this.blens[this.index++] = c;
                            }
                            else
                            {
                                // c == 16..18
                                i = c == 18 ? 7 : c - 14;
                                j = c == 18 ? 11 : 3;

                                while (k < t + i)
                                {
                                    if (n != 0)
                                    {
                                        r = ZLibConstants.Z_OK;
                                    }
                                    else
                                    {
                                        this.bitb = b;
                                        this.bitk = k;
                                        this.m_codec.AvailableBytesIn = n;
                                        this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                                        this.m_codec.NextIn = p;
                                        this.writeAt = q;
                                        return this.Flush(r);
                                    }

                                    n--;
                                    b |= (this.m_codec.InputBuffer[p++] & 0xff) << k;
                                    k += 8;
                                }

                                b >>= t;
                                k -= t;

                                j += b & InternalInflateConstants.InflateMask[i];

                                b >>= i;
                                k -= i;

                                i = this.index;
                                t = this.table;
                                if (i + j > 258 + (t & 0x1f) + ((t >> 5) & 0x1f) || c == 16 && i < 1)
                                {
                                    this.blens = null;
                                    this.mode = InflateBlockMode.BAD;
                                    this.m_codec.Message = "invalid bit length repeat";
                                    r = ZLibConstants.Z_DATA_ERROR;

                                    this.bitb = b;
                                    this.bitk = k;
                                    this.m_codec.AvailableBytesIn = n;
                                    this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                                    this.m_codec.NextIn = p;
                                    this.writeAt = q;
                                    return this.Flush(r);
                                }

                                c = c == 16 ? this.blens[i - 1] : 0;
                                do
                                {
                                    this.blens[i++] = c;
                                } while (--j != 0);

                                this.index = i;
                            }
                        }

                        this.tb[0] = -1;
                    {
                        int[] bl = {9}; // must be <= 9 for lookahead assumptions
                        int[] bd = {6}; // must be <= 9 for lookahead assumptions
                        int[] tl = new int[1];
                        int[] td = new int[1];

                        t = this.table;
                        t = this.inftree.inflate_trees_dynamic(257 + (t & 0x1f), 1 + ((t >> 5) & 0x1f), this.blens, bl, bd, tl, td, this.hufts, this.m_codec);

                        if (t != ZLibConstants.Z_OK)
                        {
                            if (t == ZLibConstants.Z_DATA_ERROR)
                            {
                                this.blens = null;
                                this.mode = InflateBlockMode.BAD;
                            }

                            r = t;

                            this.bitb = b;
                            this.bitk = k;
                            this.m_codec.AvailableBytesIn = n;
                            this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                            this.m_codec.NextIn = p;
                            this.writeAt = q;
                            return this.Flush(r);
                        }

                        this.codes.Init(bl[0], bd[0], this.hufts, tl[0], this.hufts, td[0]);
                    }
                        this.mode = InflateBlockMode.CODES;
                        goto case InflateBlockMode.CODES;

                    case InflateBlockMode.CODES:
                        this.bitb = b;
                        this.bitk = k;
                        this.m_codec.AvailableBytesIn = n;
                        this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                        this.m_codec.NextIn = p;
                        this.writeAt = q;

                        r = this.codes.Process(this, r);
                        if (r != ZLibConstants.Z_STREAM_END)
                        {
                            return this.Flush(r);
                        }

                        r = ZLibConstants.Z_OK;
                        p = this.m_codec.NextIn;
                        n = this.m_codec.AvailableBytesIn;
                        b = this.bitb;
                        k = this.bitk;
                        q = this.writeAt;
                        m = q < this.readAt ? this.readAt - q - 1 : this.end - q;

                        if (this.last == 0)
                        {
                            this.mode = InflateBlockMode.TYPE;
                            break;
                        }

                        this.mode = InflateBlockMode.DRY;
                        goto case InflateBlockMode.DRY;

                    case InflateBlockMode.DRY:
                        this.writeAt = q;
                        r = this.Flush(r);
                        q = this.writeAt;
                        m = q < this.readAt ? this.readAt - q - 1 : this.end - q;
                        if (this.readAt != this.writeAt)
                        {
                            this.bitb = b;
                            this.bitk = k;
                            this.m_codec.AvailableBytesIn = n;
                            this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                            this.m_codec.NextIn = p;
                            this.writeAt = q;
                            return this.Flush(r);
                        }

                        this.mode = InflateBlockMode.DONE;
                        goto case InflateBlockMode.DONE;

                    case InflateBlockMode.DONE:
                        r = ZLibConstants.Z_STREAM_END;
                        this.bitb = b;
                        this.bitk = k;
                        this.m_codec.AvailableBytesIn = n;
                        this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                        this.m_codec.NextIn = p;
                        this.writeAt = q;
                        return this.Flush(r);

                    case InflateBlockMode.BAD:
                        r = ZLibConstants.Z_DATA_ERROR;

                        this.bitb = b;
                        this.bitk = k;
                        this.m_codec.AvailableBytesIn = n;
                        this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                        this.m_codec.NextIn = p;
                        this.writeAt = q;
                        return this.Flush(r);


                    default:
                        r = ZLibConstants.Z_STREAM_ERROR;

                        this.bitb = b;
                        this.bitk = k;
                        this.m_codec.AvailableBytesIn = n;
                        this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
                        this.m_codec.NextIn = p;
                        this.writeAt = q;
                        return this.Flush(r);
                }
            }
        }


        internal void Free()
        {
            this.Reset();
            this.window = null;
            this.hufts = null;
        }

        internal void SetDictionary(byte[] d, int start, int n)
        {
            Array.Copy(d, start, this.window, 0, n);
            this.readAt = this.writeAt = n;
        }

        // Returns true if inflate is currently at the end of a block generated
        // by Z_SYNC_FLUSH or Z_FULL_FLUSH.
        internal int SyncPoint()
        {
            return this.mode == InflateBlockMode.LENS ? 1 : 0;
        }

        // copy as much as possible from the sliding window to the output area
        internal int Flush(int r)
        {
            int nBytes;

            for (int pass = 0; pass < 2; pass++)
            {
                if (pass == 0)
                {
                    // compute number of bytes to copy as far as end of window
                    nBytes = (this.readAt <= this.writeAt ? this.writeAt : this.end) - this.readAt;
                }
                else
                {
                    // compute bytes to copy
                    nBytes = this.writeAt - this.readAt;
                }

                // workitem 8870
                if (nBytes == 0)
                {
                    if (r == ZLibConstants.Z_BUF_ERROR)
                    {
                        r = ZLibConstants.Z_OK;
                    }

                    return r;
                }

                if (nBytes > this.m_codec.AvailableBytesOut)
                {
                    nBytes = this.m_codec.AvailableBytesOut;
                }

                if (nBytes != 0 && r == ZLibConstants.Z_BUF_ERROR)
                {
                    r = ZLibConstants.Z_OK;
                }

                // update counters
                this.m_codec.AvailableBytesOut -= nBytes;
                this.m_codec.TotalBytesOut += nBytes;

                // update check information
                if (this.checkfn != null)
                {
                    this.m_codec.m_Adler32 = this.check = Adler.Adler32(this.check, this.window, this.readAt, nBytes);
                }

                // copy as far as end of window
                Array.Copy(this.window, this.readAt, this.m_codec.OutputBuffer, this.m_codec.NextOut, nBytes);
                this.m_codec.NextOut += nBytes;
                this.readAt += nBytes;

                // see if more to copy at beginning of window
                if (this.readAt == this.end && pass == 0)
                {
                    // wrap pointers
                    this.readAt = 0;
                    if (this.writeAt == this.end)
                    {
                        this.writeAt = 0;
                    }
                }
                else
                {
                    pass++;
                }
            }

            // done
            return r;
        }
    }


    internal static class InternalInflateConstants
    {
        // And'ing with mask[n] masks the lower n bits
        internal static readonly int[] InflateMask =
        {
            0x00000000, 0x00000001, 0x00000003, 0x00000007,
            0x0000000f, 0x0000001f, 0x0000003f, 0x0000007f,
            0x000000ff, 0x000001ff, 0x000003ff, 0x000007ff,
            0x00000fff, 0x00001fff, 0x00003fff, 0x00007fff, 0x0000ffff
        };
    }


    internal sealed class InflateCodes
    {
        // waiting for "i:"=input,
        //             "o:"=output,
        //             "x:"=nothing
        private const int START = 0; // x: set up for LEN
        private const int LEN = 1; // i: get length/literal/eob next
        private const int LENEXT = 2; // i: getting length extra (have base)
        private const int DIST = 3; // i: get distance next
        private const int DISTEXT = 4; // i: getting distance extra
        private const int COPY = 5; // o: copying bytes in window, waiting for space
        private const int LIT = 6; // o: got literal, waiting for output space
        private const int WASH = 7; // o: got eob, possibly still output waiting
        private const int END = 8; // x: got eob and all data flushed
        private const int BADCODE = 9; // x: got error

        internal int mode; // current inflate_codes mode

        // mode dependent information
        internal int len;

        internal int[] tree; // pointer into tree
        internal int tree_index;
        internal int need; // bits needed

        internal int lit;

        // if EXT or COPY, where and how much
        internal int bitsToGet; // bits to get for extra
        internal int dist; // distance back to copy from

        internal byte lbits; // ltree bits decoded per branch
        internal byte dbits; // dtree bits decoder per branch
        internal int[] ltree; // literal/length/eob tree
        internal int ltree_index; // literal/length/eob tree
        internal int[] dtree; // distance tree
        internal int dtree_index; // distance tree

        internal void Init(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index)
        {
            this.mode = InflateCodes.START;
            this.lbits = (byte) bl;
            this.dbits = (byte) bd;
            this.ltree = tl;
            this.ltree_index = tl_index;
            this.dtree = td;
            this.dtree_index = td_index;
            this.tree = null;
        }

        internal int Process(InflateBlocks blocks, int r)
        {
            int j; // temporary storage
            int tindex; // temporary pointer
            int e; // extra bits or operation
            int b = 0; // bit buffer
            int k = 0; // bits in bit buffer
            int p = 0; // input data pointer
            int n; // bytes available there
            int q; // output window write pointer
            int m; // bytes to end of window or read pointer
            int f; // pointer to copy strings from

            ZLibCodec z = blocks.m_codec;

            // copy input/output information to locals (UPDATE macro restores)
            p = z.NextIn;
            n = z.AvailableBytesIn;
            b = blocks.bitb;
            k = blocks.bitk;
            q = blocks.writeAt;
            m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;

            // process input and output based on current state
            while (true)
            {
                switch (this.mode)
                {
                    // waiting for "i:"=input, "o:"=output, "x:"=nothing
                    case InflateCodes.START: // x: set up for LEN
                        if (m >= 258 && n >= 10)
                        {
                            blocks.bitb = b;
                            blocks.bitk = k;
                            z.AvailableBytesIn = n;
                            z.TotalBytesIn += p - z.NextIn;
                            z.NextIn = p;
                            blocks.writeAt = q;
                            r = this.InflateFast(this.lbits, this.dbits, this.ltree, this.ltree_index, this.dtree, this.dtree_index, blocks, z);

                            p = z.NextIn;
                            n = z.AvailableBytesIn;
                            b = blocks.bitb;
                            k = blocks.bitk;
                            q = blocks.writeAt;
                            m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;

                            if (r != ZLibConstants.Z_OK)
                            {
                                this.mode = r == ZLibConstants.Z_STREAM_END ? InflateCodes.WASH : InflateCodes.BADCODE;
                                break;
                            }
                        }

                        this.need = this.lbits;
                        this.tree = this.ltree;
                        this.tree_index = this.ltree_index;

                        this.mode = InflateCodes.LEN;
                        goto case InflateCodes.LEN;

                    case InflateCodes.LEN: // i: get length/literal/eob next
                        j = this.need;

                        while (k < j)
                        {
                            if (n != 0)
                            {
                                r = ZLibConstants.Z_OK;
                            }
                            else
                            {
                                blocks.bitb = b;
                                blocks.bitk = k;
                                z.AvailableBytesIn = n;
                                z.TotalBytesIn += p - z.NextIn;
                                z.NextIn = p;
                                blocks.writeAt = q;
                                return blocks.Flush(r);
                            }

                            n--;
                            b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        tindex = (this.tree_index + (b & InternalInflateConstants.InflateMask[j])) * 3;

                        b >>= this.tree[tindex + 1];
                        k -= this.tree[tindex + 1];

                        e = this.tree[tindex];

                        if (e == 0)
                        {
                            // literal
                            this.lit = this.tree[tindex + 2];
                            this.mode = InflateCodes.LIT;
                            break;
                        }

                        if ((e & 16) != 0)
                        {
                            // length
                            this.bitsToGet = e & 15;
                            this.len = this.tree[tindex + 2];
                            this.mode = InflateCodes.LENEXT;
                            break;
                        }

                        if ((e & 64) == 0)
                        {
                            // next table
                            this.need = e;
                            this.tree_index = tindex / 3 + this.tree[tindex + 2];
                            break;
                        }

                        if ((e & 32) != 0)
                        {
                            // end of block
                            this.mode = InflateCodes.WASH;
                            break;
                        }

                        this.mode = InflateCodes.BADCODE; // invalid code
                        z.Message = "invalid literal/length code";
                        r = ZLibConstants.Z_DATA_ERROR;

                        blocks.bitb = b;
                        blocks.bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        blocks.writeAt = q;
                        return blocks.Flush(r);


                    case InflateCodes.LENEXT: // i: getting length extra (have base)
                        j = this.bitsToGet;

                        while (k < j)
                        {
                            if (n != 0)
                            {
                                r = ZLibConstants.Z_OK;
                            }
                            else
                            {
                                blocks.bitb = b;
                                blocks.bitk = k;
                                z.AvailableBytesIn = n;
                                z.TotalBytesIn += p - z.NextIn;
                                z.NextIn = p;
                                blocks.writeAt = q;
                                return blocks.Flush(r);
                            }

                            n--;
                            b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        this.len += b & InternalInflateConstants.InflateMask[j];

                        b >>= j;
                        k -= j;

                        this.need = this.dbits;
                        this.tree = this.dtree;
                        this.tree_index = this.dtree_index;
                        this.mode = InflateCodes.DIST;
                        goto case InflateCodes.DIST;

                    case InflateCodes.DIST: // i: get distance next
                        j = this.need;

                        while (k < j)
                        {
                            if (n != 0)
                            {
                                r = ZLibConstants.Z_OK;
                            }
                            else
                            {
                                blocks.bitb = b;
                                blocks.bitk = k;
                                z.AvailableBytesIn = n;
                                z.TotalBytesIn += p - z.NextIn;
                                z.NextIn = p;
                                blocks.writeAt = q;
                                return blocks.Flush(r);
                            }

                            n--;
                            b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        tindex = (this.tree_index + (b & InternalInflateConstants.InflateMask[j])) * 3;

                        b >>= this.tree[tindex + 1];
                        k -= this.tree[tindex + 1];

                        e = this.tree[tindex];
                        if ((e & 0x10) != 0)
                        {
                            // distance
                            this.bitsToGet = e & 15;
                            this.dist = this.tree[tindex + 2];
                            this.mode = InflateCodes.DISTEXT;
                            break;
                        }

                        if ((e & 64) == 0)
                        {
                            // next table
                            this.need = e;
                            this.tree_index = tindex / 3 + this.tree[tindex + 2];
                            break;
                        }

                        this.mode = InflateCodes.BADCODE; // invalid code
                        z.Message = "invalid distance code";
                        r = ZLibConstants.Z_DATA_ERROR;

                        blocks.bitb = b;
                        blocks.bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        blocks.writeAt = q;
                        return blocks.Flush(r);


                    case InflateCodes.DISTEXT: // i: getting distance extra
                        j = this.bitsToGet;

                        while (k < j)
                        {
                            if (n != 0)
                            {
                                r = ZLibConstants.Z_OK;
                            }
                            else
                            {
                                blocks.bitb = b;
                                blocks.bitk = k;
                                z.AvailableBytesIn = n;
                                z.TotalBytesIn += p - z.NextIn;
                                z.NextIn = p;
                                blocks.writeAt = q;
                                return blocks.Flush(r);
                            }

                            n--;
                            b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        this.dist += b & InternalInflateConstants.InflateMask[j];

                        b >>= j;
                        k -= j;

                        this.mode = InflateCodes.COPY;
                        goto case InflateCodes.COPY;

                    case InflateCodes.COPY: // o: copying bytes in window, waiting for space
                        f = q - this.dist;
                        while (f < 0)
                        {
                            // modulo window size-"while" instead
                            f += blocks.end; // of "if" handles invalid distances
                        }

                        while (this.len != 0)
                        {
                            if (m == 0)
                            {
                                if (q == blocks.end && blocks.readAt != 0)
                                {
                                    q = 0;
                                    m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;
                                }

                                if (m == 0)
                                {
                                    blocks.writeAt = q;
                                    r = blocks.Flush(r);
                                    q = blocks.writeAt;
                                    m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;

                                    if (q == blocks.end && blocks.readAt != 0)
                                    {
                                        q = 0;
                                        m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;
                                    }

                                    if (m == 0)
                                    {
                                        blocks.bitb = b;
                                        blocks.bitk = k;
                                        z.AvailableBytesIn = n;
                                        z.TotalBytesIn += p - z.NextIn;
                                        z.NextIn = p;
                                        blocks.writeAt = q;
                                        return blocks.Flush(r);
                                    }
                                }
                            }

                            blocks.window[q++] = blocks.window[f++];
                            m--;

                            if (f == blocks.end)
                            {
                                f = 0;
                            }

                            this.len--;
                        }

                        this.mode = InflateCodes.START;
                        break;

                    case InflateCodes.LIT: // o: got literal, waiting for output space
                        if (m == 0)
                        {
                            if (q == blocks.end && blocks.readAt != 0)
                            {
                                q = 0;
                                m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;
                            }

                            if (m == 0)
                            {
                                blocks.writeAt = q;
                                r = blocks.Flush(r);
                                q = blocks.writeAt;
                                m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;

                                if (q == blocks.end && blocks.readAt != 0)
                                {
                                    q = 0;
                                    m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;
                                }

                                if (m == 0)
                                {
                                    blocks.bitb = b;
                                    blocks.bitk = k;
                                    z.AvailableBytesIn = n;
                                    z.TotalBytesIn += p - z.NextIn;
                                    z.NextIn = p;
                                    blocks.writeAt = q;
                                    return blocks.Flush(r);
                                }
                            }
                        }

                        r = ZLibConstants.Z_OK;

                        blocks.window[q++] = (byte) this.lit;
                        m--;

                        this.mode = InflateCodes.START;
                        break;

                    case InflateCodes.WASH: // o: got eob, possibly more output
                        if (k > 7)
                        {
                            // return unused byte, if any
                            k -= 8;
                            n++;
                            p--; // can always return one
                        }

                        blocks.writeAt = q;
                        r = blocks.Flush(r);
                        q = blocks.writeAt;
                        m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;

                        if (blocks.readAt != blocks.writeAt)
                        {
                            blocks.bitb = b;
                            blocks.bitk = k;
                            z.AvailableBytesIn = n;
                            z.TotalBytesIn += p - z.NextIn;
                            z.NextIn = p;
                            blocks.writeAt = q;
                            return blocks.Flush(r);
                        }

                        this.mode = InflateCodes.END;
                        goto case InflateCodes.END;

                    case InflateCodes.END:
                        r = ZLibConstants.Z_STREAM_END;
                        blocks.bitb = b;
                        blocks.bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        blocks.writeAt = q;
                        return blocks.Flush(r);

                    case InflateCodes.BADCODE: // x: got error

                        r = ZLibConstants.Z_DATA_ERROR;

                        blocks.bitb = b;
                        blocks.bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        blocks.writeAt = q;
                        return blocks.Flush(r);

                    default:
                        r = ZLibConstants.Z_STREAM_ERROR;

                        blocks.bitb = b;
                        blocks.bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        blocks.writeAt = q;
                        return blocks.Flush(r);
                }
            }
        }


        // Called with number of bytes left to write in window at least 258
        // (the maximum string length) and number of input bytes available
        // at least ten.  The ten bytes are six bytes for the longest length/
        // distance pair plus four bytes for overloading the bit buffer.

        internal int InflateFast(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, InflateBlocks s, ZLibCodec z)
        {
            int t; // temporary pointer
            int[] tp; // temporary pointer
            int tp_index; // temporary pointer
            int e; // extra bits or operation
            int b; // bit buffer
            int k; // bits in bit buffer
            int p; // input data pointer
            int n; // bytes available there
            int q; // output window write pointer
            int m; // bytes to end of window or read pointer
            int ml; // mask for literal/length tree
            int md; // mask for distance tree
            int c; // bytes to copy
            int d; // distance back to copy from
            int r; // copy source pointer

            int tp_index_t_3; // (tp_index+t)*3

            // load input, output, bit values
            p = z.NextIn;
            n = z.AvailableBytesIn;
            b = s.bitb;
            k = s.bitk;
            q = s.writeAt;
            m = q < s.readAt ? s.readAt - q - 1 : s.end - q;

            // initialize masks
            ml = InternalInflateConstants.InflateMask[bl];
            md = InternalInflateConstants.InflateMask[bd];

            // do until not enough input or output space for fast loop
            do
            {
                // assume called with m >= 258 && n >= 10
                // get literal/length code
                while (k < 20)
                {
                    // max bits for literal/length code
                    n--;
                    b |= (z.InputBuffer[p++] & 0xff) << k;
                    k += 8;
                }

                t = b & ml;
                tp = tl;
                tp_index = tl_index;
                tp_index_t_3 = (tp_index + t) * 3;
                if ((e = tp[tp_index_t_3]) == 0)
                {
                    b >>= tp[tp_index_t_3 + 1];
                    k -= tp[tp_index_t_3 + 1];

                    s.window[q++] = (byte) tp[tp_index_t_3 + 2];
                    m--;
                    continue;
                }

                do
                {
                    b >>= tp[tp_index_t_3 + 1];
                    k -= tp[tp_index_t_3 + 1];

                    if ((e & 16) != 0)
                    {
                        e &= 15;
                        c = tp[tp_index_t_3 + 2] + (b & InternalInflateConstants.InflateMask[e]);

                        b >>= e;
                        k -= e;

                        // decode distance base of block to copy
                        while (k < 15)
                        {
                            // max bits for distance code
                            n--;
                            b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        t = b & md;
                        tp = td;
                        tp_index = td_index;
                        tp_index_t_3 = (tp_index + t) * 3;
                        e = tp[tp_index_t_3];

                        do
                        {
                            b >>= tp[tp_index_t_3 + 1];
                            k -= tp[tp_index_t_3 + 1];

                            if ((e & 16) != 0)
                            {
                                // get extra bits to add to distance base
                                e &= 15;
                                while (k < e)
                                {
                                    // get extra bits (up to 13)
                                    n--;
                                    b |= (z.InputBuffer[p++] & 0xff) << k;
                                    k += 8;
                                }

                                d = tp[tp_index_t_3 + 2] + (b & InternalInflateConstants.InflateMask[e]);

                                b >>= e;
                                k -= e;

                                // do the copy
                                m -= c;
                                if (q >= d)
                                {
                                    // offset before dest
                                    //  just copy
                                    r = q - d;
                                    if (q - r > 0 && 2 > q - r)
                                    {
                                        s.window[q++] = s.window[r++]; // minimum count is three,
                                        s.window[q++] = s.window[r++]; // so unroll loop a little
                                        c -= 2;
                                    }
                                    else
                                    {
                                        Array.Copy(s.window, r, s.window, q, 2);
                                        q += 2;
                                        r += 2;
                                        c -= 2;
                                    }
                                }
                                else
                                {
                                    // else offset after destination
                                    r = q - d;
                                    do
                                    {
                                        r += s.end; // force pointer in window
                                    } while (r < 0); // covers invalid distances

                                    e = s.end - r;
                                    if (c > e)
                                    {
                                        // if source crosses,
                                        c -= e; // wrapped copy
                                        if (q - r > 0 && e > q - r)
                                        {
                                            do
                                            {
                                                s.window[q++] = s.window[r++];
                                            } while (--e != 0);
                                        }
                                        else
                                        {
                                            Array.Copy(s.window, r, s.window, q, e);
                                            q += e;
                                            r += e;
                                            e = 0;
                                        }

                                        r = 0; // copy rest from start of window
                                    }
                                }

                                // copy all or what's left
                                if (q - r > 0 && c > q - r)
                                {
                                    do
                                    {
                                        s.window[q++] = s.window[r++];
                                    } while (--c != 0);
                                }
                                else
                                {
                                    Array.Copy(s.window, r, s.window, q, c);
                                    q += c;
                                    r += c;
                                    c = 0;
                                }

                                break;
                            }

                            if ((e & 64) == 0)
                            {
                                t += tp[tp_index_t_3 + 2];
                                t += b & InternalInflateConstants.InflateMask[e];
                                tp_index_t_3 = (tp_index + t) * 3;
                                e = tp[tp_index_t_3];
                            }
                            else
                            {
                                z.Message = "invalid distance code";

                                c = z.AvailableBytesIn - n;
                                c = k >> 3 < c ? k >> 3 : c;
                                n += c;
                                p -= c;
                                k -= c << 3;

                                s.bitb = b;
                                s.bitk = k;
                                z.AvailableBytesIn = n;
                                z.TotalBytesIn += p - z.NextIn;
                                z.NextIn = p;
                                s.writeAt = q;

                                return ZLibConstants.Z_DATA_ERROR;
                            }
                        } while (true);

                        break;
                    }

                    if ((e & 64) == 0)
                    {
                        t += tp[tp_index_t_3 + 2];
                        t += b & InternalInflateConstants.InflateMask[e];
                        tp_index_t_3 = (tp_index + t) * 3;
                        if ((e = tp[tp_index_t_3]) == 0)
                        {
                            b >>= tp[tp_index_t_3 + 1];
                            k -= tp[tp_index_t_3 + 1];
                            s.window[q++] = (byte) tp[tp_index_t_3 + 2];
                            m--;
                            break;
                        }
                    }
                    else if ((e & 32) != 0)
                    {
                        c = z.AvailableBytesIn - n;
                        c = k >> 3 < c ? k >> 3 : c;
                        n += c;
                        p -= c;
                        k -= c << 3;

                        s.bitb = b;
                        s.bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        s.writeAt = q;

                        return ZLibConstants.Z_STREAM_END;
                    }
                    else
                    {
                        z.Message = "invalid literal/length code";

                        c = z.AvailableBytesIn - n;
                        c = k >> 3 < c ? k >> 3 : c;
                        n += c;
                        p -= c;
                        k -= c << 3;

                        s.bitb = b;
                        s.bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        s.writeAt = q;

                        return ZLibConstants.Z_DATA_ERROR;
                    }
                } while (true);
            } while (m >= 258 && n >= 10);

            // not enough input or output--restore pointers and return
            c = z.AvailableBytesIn - n;
            c = k >> 3 < c ? k >> 3 : c;
            n += c;
            p -= c;
            k -= c << 3;

            s.bitb = b;
            s.bitk = k;
            z.AvailableBytesIn = n;
            z.TotalBytesIn += p - z.NextIn;
            z.NextIn = p;
            s.writeAt = q;

            return ZLibConstants.Z_OK;
        }
    }


    internal sealed class InflateManager
    {
        // preset dictionary flag in zlib header
        private const int PRESET_DICT = 0x20;

        private const int Z_DEFLATED = 8;

        private enum InflateManagerMode
        {
            METHOD = 0, // waiting for method byte
            FLAG = 1, // waiting for flag byte
            DICT4 = 2, // four dictionary check bytes to go
            DICT3 = 3, // three dictionary check bytes to go
            DICT2 = 4, // two dictionary check bytes to go
            DICT1 = 5, // one dictionary check byte to go
            DICT0 = 6, // waiting for inflateSetDictionary
            BLOCKS = 7, // decompressing blocks
            CHECK4 = 8, // four check bytes to go
            CHECK3 = 9, // three check bytes to go
            CHECK2 = 10, // two check bytes to go
            CHECK1 = 11, // one check byte to go
            DONE = 12, // finished check, done
            BAD = 13 // got an error--stay here
        }

        private InflateManagerMode mode; // current inflate mode
        internal ZLibCodec m_codec; // pointer back to this zlib stream

        // mode dependent information
        internal int method; // if FLAGS, method byte

        // if CHECK, check values to compare
        internal uint computedCheck; // computed check value
        internal uint expectedCheck; // stream check value

        // if BAD, inflateSync's marker bytes count
        internal int marker;

        // mode independent information
        //internal int nowrap; // flag for no wrapper
        internal bool HandleRfc1950HeaderBytes { get; set; } = true;

        internal int wbits; // log2(window size)  (8..15, defaults to 15)

        internal InflateBlocks blocks; // current inflate_blocks state

        public InflateManager()
        {
        }

        public InflateManager(bool expectRfc1950HeaderBytes)
        {
            this.HandleRfc1950HeaderBytes = expectRfc1950HeaderBytes;
        }

        internal int Reset()
        {
            this.m_codec.TotalBytesIn = this.m_codec.TotalBytesOut = 0;
            this.m_codec.Message = null;
            this.mode = this.HandleRfc1950HeaderBytes ? InflateManagerMode.METHOD : InflateManagerMode.BLOCKS;
            this.blocks.Reset();
            return ZLibConstants.Z_OK;
        }

        internal int End()
        {
            if (this.blocks != null)
            {
                this.blocks.Free();
            }

            this.blocks = null;
            return ZLibConstants.Z_OK;
        }

        internal int Initialize(ZLibCodec codec, int w)
        {
            this.m_codec = codec;
            this.m_codec.Message = null;
            this.blocks = null;

            // handle undocumented nowrap option (no zlib header or check)
            //nowrap = 0;
            //if (w < 0)
            //{
            //    w = - w;
            //    nowrap = 1;
            //}

            // set window size
            if (w < 8 || w > 15)
            {
                this.End();
                throw new ZLibException("Bad window size.");

                //return ZLibConstants.Z_STREAM_ERROR;
            }

            this.wbits = w;

            this.blocks = new InflateBlocks(codec, this.HandleRfc1950HeaderBytes ? this : null,
                                            1 << w);

            // reset state
            this.Reset();
            return ZLibConstants.Z_OK;
        }


        internal int Inflate(FlushType flush)
        {
            int b;

            if (this.m_codec.InputBuffer == null)
            {
                throw new ZLibException("InputBuffer is null. ");
            }

//             int f = (flush == FlushType.Finish)
//                 ? ZLibConstants.Z_BUF_ERROR
//                 : ZLibConstants.Z_OK;

            // workitem 8870
            int f = ZLibConstants.Z_OK;
            int r = ZLibConstants.Z_BUF_ERROR;

            while (true)
            {
                switch (this.mode)
                {
                    case InflateManagerMode.METHOD:
                        if (this.m_codec.AvailableBytesIn == 0)
                        {
                            return r;
                        }

                        r = f;
                        this.m_codec.AvailableBytesIn--;
                        this.m_codec.TotalBytesIn++;
                        if (((this.method = this.m_codec.InputBuffer[this.m_codec.NextIn++]) & 0xf) != InflateManager.Z_DEFLATED)
                        {
                            this.mode = InflateManagerMode.BAD;
                            this.m_codec.Message = string.Format("unknown compression method (0x{0:X2})", this.method);
                            this.marker = 5; // can't try inflateSync
                            break;
                        }

                        if ((this.method >> 4) + 8 > this.wbits)
                        {
                            this.mode = InflateManagerMode.BAD;
                            this.m_codec.Message = string.Format("invalid window size ({0})", (this.method >> 4) + 8);
                            this.marker = 5; // can't try inflateSync
                            break;
                        }

                        this.mode = InflateManagerMode.FLAG;
                        break;


                    case InflateManagerMode.FLAG:
                        if (this.m_codec.AvailableBytesIn == 0)
                        {
                            return r;
                        }

                        r = f;
                        this.m_codec.AvailableBytesIn--;
                        this.m_codec.TotalBytesIn++;
                        b = this.m_codec.InputBuffer[this.m_codec.NextIn++] & 0xff;

                        if (((this.method << 8) + b) % 31 != 0)
                        {
                            this.mode = InflateManagerMode.BAD;
                            this.m_codec.Message = "incorrect header check";
                            this.marker = 5; // can't try inflateSync
                            break;
                        }

                        this.mode = (b & InflateManager.PRESET_DICT) == 0
                            ? InflateManagerMode.BLOCKS
                            : InflateManagerMode.DICT4;
                        break;

                    case InflateManagerMode.DICT4:
                        if (this.m_codec.AvailableBytesIn == 0)
                        {
                            return r;
                        }

                        r = f;
                        this.m_codec.AvailableBytesIn--;
                        this.m_codec.TotalBytesIn++;
                        this.expectedCheck = (uint) ((this.m_codec.InputBuffer[this.m_codec.NextIn++] << 24) & 0xff000000);
                        this.mode = InflateManagerMode.DICT3;
                        break;

                    case InflateManagerMode.DICT3:
                        if (this.m_codec.AvailableBytesIn == 0)
                        {
                            return r;
                        }

                        r = f;
                        this.m_codec.AvailableBytesIn--;
                        this.m_codec.TotalBytesIn++;
                        this.expectedCheck += (uint) ((this.m_codec.InputBuffer[this.m_codec.NextIn++] << 16) & 0x00ff0000);
                        this.mode = InflateManagerMode.DICT2;
                        break;

                    case InflateManagerMode.DICT2:

                        if (this.m_codec.AvailableBytesIn == 0)
                        {
                            return r;
                        }

                        r = f;
                        this.m_codec.AvailableBytesIn--;
                        this.m_codec.TotalBytesIn++;
                        this.expectedCheck += (uint) ((this.m_codec.InputBuffer[this.m_codec.NextIn++] << 8) & 0x0000ff00);
                        this.mode = InflateManagerMode.DICT1;
                        break;


                    case InflateManagerMode.DICT1:
                        if (this.m_codec.AvailableBytesIn == 0)
                        {
                            return r;
                        }

                        r = f;
                        this.m_codec.AvailableBytesIn--;
                        this.m_codec.TotalBytesIn++;
                        this.expectedCheck += (uint) (this.m_codec.InputBuffer[this.m_codec.NextIn++] & 0x000000ff);
                        this.m_codec.m_Adler32 = this.expectedCheck;
                        this.mode = InflateManagerMode.DICT0;
                        return ZLibConstants.Z_NEED_DICT;


                    case InflateManagerMode.DICT0:
                        this.mode = InflateManagerMode.BAD;
                        this.m_codec.Message = "need dictionary";
                        this.marker = 0; // can try inflateSync
                        return ZLibConstants.Z_STREAM_ERROR;


                    case InflateManagerMode.BLOCKS:
                        r = this.blocks.Process(r);
                        if (r == ZLibConstants.Z_DATA_ERROR)
                        {
                            this.mode = InflateManagerMode.BAD;
                            this.marker = 0; // can try inflateSync
                            break;
                        }

                        if (r == ZLibConstants.Z_OK)
                        {
                            r = f;
                        }

                        if (r != ZLibConstants.Z_STREAM_END)
                        {
                            return r;
                        }

                        r = f;
                        this.computedCheck = this.blocks.Reset();
                        if (!this.HandleRfc1950HeaderBytes)
                        {
                            this.mode = InflateManagerMode.DONE;
                            return ZLibConstants.Z_STREAM_END;
                        }

                        this.mode = InflateManagerMode.CHECK4;
                        break;

                    case InflateManagerMode.CHECK4:
                        if (this.m_codec.AvailableBytesIn == 0)
                        {
                            return r;
                        }

                        r = f;
                        this.m_codec.AvailableBytesIn--;
                        this.m_codec.TotalBytesIn++;
                        this.expectedCheck = (uint) ((this.m_codec.InputBuffer[this.m_codec.NextIn++] << 24) & 0xff000000);
                        this.mode = InflateManagerMode.CHECK3;
                        break;

                    case InflateManagerMode.CHECK3:
                        if (this.m_codec.AvailableBytesIn == 0)
                        {
                            return r;
                        }

                        r = f;
                        this.m_codec.AvailableBytesIn--;
                        this.m_codec.TotalBytesIn++;
                        this.expectedCheck += (uint) ((this.m_codec.InputBuffer[this.m_codec.NextIn++] << 16) & 0x00ff0000);
                        this.mode = InflateManagerMode.CHECK2;
                        break;

                    case InflateManagerMode.CHECK2:
                        if (this.m_codec.AvailableBytesIn == 0)
                        {
                            return r;
                        }

                        r = f;
                        this.m_codec.AvailableBytesIn--;
                        this.m_codec.TotalBytesIn++;
                        this.expectedCheck += (uint) ((this.m_codec.InputBuffer[this.m_codec.NextIn++] << 8) & 0x0000ff00);
                        this.mode = InflateManagerMode.CHECK1;
                        break;

                    case InflateManagerMode.CHECK1:
                        if (this.m_codec.AvailableBytesIn == 0)
                        {
                            return r;
                        }

                        r = f;
                        this.m_codec.AvailableBytesIn--;
                        this.m_codec.TotalBytesIn++;
                        this.expectedCheck += (uint) (this.m_codec.InputBuffer[this.m_codec.NextIn++] & 0x000000ff);
                        if (this.computedCheck != this.expectedCheck)
                        {
                            this.mode = InflateManagerMode.BAD;
                            this.m_codec.Message = "incorrect data check";
                            this.marker = 5; // can't try inflateSync
                            break;
                        }

                        this.mode = InflateManagerMode.DONE;
                        return ZLibConstants.Z_STREAM_END;

                    case InflateManagerMode.DONE:
                        return ZLibConstants.Z_STREAM_END;

                    case InflateManagerMode.BAD:
                        throw new ZLibException(string.Format("Bad state ({0})", this.m_codec.Message));

                    default:
                        throw new ZLibException("Stream error.");
                }
            }
        }


        internal int SetDictionary(byte[] dictionary)
        {
            int index = 0;
            int length = dictionary.Length;
            if (this.mode != InflateManagerMode.DICT0)
            {
                throw new ZLibException("Stream error.");
            }

            if (Adler.Adler32(1, dictionary, 0, dictionary.Length) != this.m_codec.m_Adler32)
            {
                return ZLibConstants.Z_DATA_ERROR;
            }

            this.m_codec.m_Adler32 = Adler.Adler32(0, null, 0, 0);

            if (length >= 1 << this.wbits)
            {
                length = (1 << this.wbits) - 1;
                index = dictionary.Length - length;
            }

            this.blocks.SetDictionary(dictionary, index, length);
            this.mode = InflateManagerMode.BLOCKS;
            return ZLibConstants.Z_OK;
        }


        private static readonly byte[] mark = {0, 0, 0xff, 0xff};

        internal int Sync()
        {
            int n; // number of bytes to look at
            int p; // pointer to bytes
            int m; // number of marker bytes found in a row
            long r, w; // temporaries to save total_in and total_out

            // set up
            if (this.mode != InflateManagerMode.BAD)
            {
                this.mode = InflateManagerMode.BAD;
                this.marker = 0;
            }

            if ((n = this.m_codec.AvailableBytesIn) == 0)
            {
                return ZLibConstants.Z_BUF_ERROR;
            }

            p = this.m_codec.NextIn;
            m = this.marker;

            // search
            while (n != 0 && m < 4)
            {
                if (this.m_codec.InputBuffer[p] == InflateManager.mark[m])
                {
                    m++;
                }
                else if (this.m_codec.InputBuffer[p] != 0)
                {
                    m = 0;
                }
                else
                {
                    m = 4 - m;
                }

                p++;
                n--;
            }

            // restore
            this.m_codec.TotalBytesIn += p - this.m_codec.NextIn;
            this.m_codec.NextIn = p;
            this.m_codec.AvailableBytesIn = n;
            this.marker = m;

            // return no joy or set up to restart on a new block
            if (m != 4)
            {
                return ZLibConstants.Z_DATA_ERROR;
            }

            r = this.m_codec.TotalBytesIn;
            w = this.m_codec.TotalBytesOut;
            this.Reset();
            this.m_codec.TotalBytesIn = r;
            this.m_codec.TotalBytesOut = w;
            this.mode = InflateManagerMode.BLOCKS;
            return ZLibConstants.Z_OK;
        }


        // Returns true if inflate is currently at the end of a block generated
        // by Z_SYNC_FLUSH or Z_FULL_FLUSH. This function is used by one PPP
        // implementation to provide an additional safety check. PPP uses Z_SYNC_FLUSH
        // but removes the length bytes of the resulting empty stored block. When
        // decompressing, PPP checks that at the end of input packet, inflate is
        // waiting for these length bytes.
        internal int SyncPoint(ZLibCodec z)
        {
            return this.blocks.SyncPoint();
        }
    }
}