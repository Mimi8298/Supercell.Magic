// Deflate.cs
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
// Time-stamp: <2011-August-03 19:52:15>
//
// ------------------------------------------------------------------
//
// This module defines logic for handling the Deflate or compression.
//
// This code is based on multiple sources:
// - the original zlib v1.2.3 source, which is Copyright (C) 1995-2005 Jean-loup Gailly.
// - the original jzlib, which is Copyright (c) 2000-2003 ymnk, JCraft,Inc.
//
// However, this code is significantly different from both.
// The object model is not the same, and many of the behaviors are different.
//
// In keeping with the license for these other works, the copyrights for
// jzlib and zlib are here.
//
// -----------------------------------------------------------------------
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

    internal enum BlockState
    {
        NeedMore = 0, // block not completed, need more input or more output
        BlockDone, // block flush performed
        FinishStarted, // finish started, need only more output at next deflate
        FinishDone // finish done, accept no more input or output
    }

    internal enum DeflateFlavor
    {
        Store,
        Fast,
        Slow
    }

    internal sealed class DeflateManager
    {
        private static readonly int MEM_LEVEL_MAX = 9;
        private static readonly int MEM_LEVEL_DEFAULT = 8;

        internal delegate BlockState CompressFunc(FlushType flush);

        internal class Config
        {
            // Use a faster search when the previous match is longer than this
            internal int GoodLength; // reduce lazy search above this match length

            // Attempt to find a better match only when the current match is
            // strictly smaller than this value. This mechanism is used only for
            // compression levels >= 4.  For levels 1,2,3: MaxLazy is actually
            // MaxInsertLength. (See DeflateFast)

            internal int MaxLazy; // do not perform lazy search above this match length

            internal int NiceLength; // quit search above this match length

            // To speed up deflation, hash chains are never searched beyond this
            // length.  A higher limit improves compression ratio but degrades the speed.

            internal int MaxChainLength;

            internal DeflateFlavor Flavor;

            private Config(int goodLength, int maxLazy, int niceLength, int maxChainLength, DeflateFlavor flavor)
            {
                this.GoodLength = goodLength;
                this.MaxLazy = maxLazy;
                this.NiceLength = niceLength;
                this.MaxChainLength = maxChainLength;
                this.Flavor = flavor;
            }

            public static Config Lookup(CompressionLevel level)
            {
                return Config.Table[(int) level];
            }


            static Config()
            {
                Config.Table = new[]
                {
                    new Config(0, 0, 0, 0, DeflateFlavor.Store),
                    new Config(4, 4, 8, 4, DeflateFlavor.Fast),
                    new Config(4, 5, 16, 8, DeflateFlavor.Fast),
                    new Config(4, 6, 32, 32, DeflateFlavor.Fast),

                    new Config(4, 4, 16, 16, DeflateFlavor.Slow),
                    new Config(8, 16, 32, 32, DeflateFlavor.Slow),
                    new Config(8, 16, 128, 128, DeflateFlavor.Slow),
                    new Config(8, 32, 128, 256, DeflateFlavor.Slow),
                    new Config(32, 128, 258, 1024, DeflateFlavor.Slow),
                    new Config(32, 258, 258, 4096, DeflateFlavor.Slow)
                };
            }

            private static readonly Config[] Table;
        }


        private CompressFunc DeflateFunction;

        private static readonly string[] m_ErrorMessage =
        {
            "need dictionary",
            "stream end",
            "",
            "file error",
            "stream error",
            "data error",
            "insufficient memory",
            "buffer error",
            "incompatible version",
            ""
        };

        // preset dictionary flag in zlib header
        private static readonly int PRESET_DICT = 0x20;

        private static readonly int INIT_STATE = 42;
        private static readonly int BUSY_STATE = 113;
        private static readonly int FINISH_STATE = 666;

        // The deflate compression method
        private static readonly int Z_DEFLATED = 8;

        private static readonly int STORED_BLOCK = 0;
        private static readonly int STATIC_TREES = 1;
        private static readonly int DYN_TREES = 2;

        // The three kinds of block type
        private static readonly int Z_BINARY = 0;
        private static readonly int Z_ASCII = 1;
        private static readonly int Z_UNKNOWN = 2;

        private static readonly int Buf_size = 8 * 2;

        private static readonly int MIN_MATCH = 3;
        private static readonly int MAX_MATCH = 258;

        private static readonly int MIN_LOOKAHEAD = DeflateManager.MAX_MATCH + DeflateManager.MIN_MATCH + 1;

        private static readonly int HEAP_SIZE = 2 * InternalConstants.L_CODES + 1;

        private static readonly int END_BLOCK = 256;

        internal ZLibCodec m_codec; // the zlib encoder/decoder
        internal int status; // as the name implies
        internal byte[] pending; // output still pending - waiting to be compressed
        internal int nextPending; // index of next pending byte to output to the stream
        internal int pendingCount; // number of bytes in the pending buffer

        internal sbyte data_type; // UNKNOWN, BINARY or ASCII
        internal int last_flush; // value of flush param for previous deflate call

        internal int w_size; // LZ77 window size (32K by default)
        internal int w_bits; // log2(w_size)  (8..16)
        internal int w_mask; // w_size - 1

        //internal byte[] dictionary;
        internal byte[] window;

        // Sliding window. Input bytes are read into the second half of the window,
        // and move to the first half later to keep a dictionary of at least wSize
        // bytes. With this organization, matches are limited to a distance of
        // wSize-MAX_MATCH bytes, but this ensures that IO is always
        // performed with a length multiple of the block size.
        //
        // To do: use the user input buffer as sliding window.

        internal int window_size;
        // Actual size of window: 2*wSize, except when the user input buffer
        // is directly used as sliding window.

        internal short[] prev;
        // Link to older string with same hash index. To limit the size of this
        // array to 64K, this link is maintained only for the last 32K strings.
        // An index in this array is thus a window index modulo 32K.

        internal short[] head; // Heads of the hash chains or NIL.

        internal int ins_h; // hash index of string to be inserted
        internal int hash_size; // number of elements in hash table
        internal int hash_bits; // log2(hash_size)
        internal int hash_mask; // hash_size-1

        // Number of bits by which ins_h must be shifted at each input
        // step. It must be such that after MIN_MATCH steps, the oldest
        // byte no longer takes part in the hash key, that is:
        // hash_shift * MIN_MATCH >= hash_bits
        internal int hash_shift;

        // Window position at the beginning of the current output block. Gets
        // negative when the window is moved backwards.

        internal int block_start;

        private Config config;
        internal int match_length; // length of best match
        internal int prev_match; // previous match
        internal int match_available; // set if previous match exists
        internal int strstart; // start of string to insert into.....????
        internal int match_start; // start of matching string
        internal int lookahead; // number of valid bytes ahead in window

        // Length of the best match at previous step. Matches not greater than this
        // are discarded. This is used in the lazy match evaluation.
        internal int prev_length;

        // Insert new strings in the hash table only if the match length is not
        // greater than this length. This saves time but degrades compression.
        // max_insert_length is used only for compression levels <= 3.

        internal CompressionLevel compressionLevel; // compression level (1..9)
        internal CompressionStrategy compressionStrategy; // favor or force Huffman coding


        internal short[] dyn_ltree; // literal and length tree
        internal short[] dyn_dtree; // distance tree
        internal short[] bl_tree; // Huffman tree for bit lengths

        internal Tree treeLiterals = new Tree(); // desc for literal tree
        internal Tree treeDistances = new Tree(); // desc for distance tree
        internal Tree treeBitLengths = new Tree(); // desc for bit length tree

        // number of codes at each bit length for an optimal tree
        internal short[] bl_count = new short[InternalConstants.MAX_BITS + 1];

        // heap used to build the Huffman trees
        internal int[] heap = new int[2 * InternalConstants.L_CODES + 1];

        internal int heap_len; // number of elements in the heap
        internal int heap_max; // element of largest frequency

        // The sons of heap[n] are heap[2*n] and heap[2*n+1]. heap[0] is not used.
        // The same heap array is used to build all trees.

        // Depth of each subtree used as tie breaker for trees of equal frequency
        internal sbyte[] depth = new sbyte[2 * InternalConstants.L_CODES + 1];

        internal int m_lengthOffset; // index for literals or lengths


        // Size of match buffer for literals/lengths.  There are 4 reasons for
        // limiting lit_bufsize to 64K:
        //   - frequencies can be kept in 16 bit counters
        //   - if compression is not successful for the first block, all input
        //     data is still in the window so we can still emit a stored block even
        //     when input comes from standard input.  (This can also be done for
        //     all blocks if lit_bufsize is not greater than 32K.)
        //   - if compression is not successful for a file smaller than 64K, we can
        //     even emit a stored file instead of a stored block (saving 5 bytes).
        //     This is applicable only for zip (not gzip or zlib).
        //   - creating new Huffman trees less frequently may not provide fast
        //     adaptation to changes in the input data statistics. (Take for
        //     example a binary file with poorly compressible code followed by
        //     a highly compressible string table.) Smaller buffer sizes give
        //     fast adaptation but have of course the overhead of transmitting
        //     trees more frequently.

        internal int lit_bufsize;

        internal int last_lit; // running index in l_buf

        // Buffer for distances. To simplify the code, d_buf and l_buf have
        // the same number of elements. To use different lengths, an extra flag
        // array would be necessary.

        internal int m_distanceOffset; // index into pending; points to distance data??

        internal int opt_len; // bit length of current block with optimal trees
        internal int static_len; // bit length of current block with static trees
        internal int matches; // number of string matches in current block
        internal int last_eob_len; // bit length of EOB code for last block

        // Output buffer. bits are inserted starting at the bottom (least
        // significant bits).
        internal short bi_buf;

        // Number of valid bits in bi_buf.  All bits above the last valid bit
        // are always zero.
        internal int bi_valid;


        internal DeflateManager()
        {
            this.dyn_ltree = new short[DeflateManager.HEAP_SIZE * 2];
            this.dyn_dtree = new short[(2 * InternalConstants.D_CODES + 1) * 2]; // distance tree
            this.bl_tree = new short[(2 * InternalConstants.BL_CODES + 1) * 2]; // Huffman tree for bit lengths
        }


        // lm_init
        private void m_InitializeLazyMatch()
        {
            this.window_size = 2 * this.w_size;

            // clear the hash - workitem 9063
            Array.Clear(this.head, 0, this.hash_size);
            //for (int i = 0; i < hash_size; i++) head[i] = 0;

            this.config = Config.Lookup(this.compressionLevel);
            this.SetDeflater();

            this.strstart = 0;
            this.block_start = 0;
            this.lookahead = 0;
            this.match_length = this.prev_length = DeflateManager.MIN_MATCH - 1;
            this.match_available = 0;
            this.ins_h = 0;
        }

        // Initialize the tree data structures for a new zlib stream.
        private void m_InitializeTreeData()
        {
            this.treeLiterals.dyn_tree = this.dyn_ltree;
            this.treeLiterals.staticTree = StaticTree.Literals;

            this.treeDistances.dyn_tree = this.dyn_dtree;
            this.treeDistances.staticTree = StaticTree.Distances;

            this.treeBitLengths.dyn_tree = this.bl_tree;
            this.treeBitLengths.staticTree = StaticTree.BitLengths;

            this.bi_buf = 0;
            this.bi_valid = 0;
            this.last_eob_len = 8; // enough lookahead for inflate

            // Initialize the first block of the first file:
            this.m_InitializeBlocks();
        }

        internal void m_InitializeBlocks()
        {
            // Initialize the trees.
            for (int i = 0; i < InternalConstants.L_CODES; i++)
            {
                this.dyn_ltree[i * 2] = 0;
            }

            for (int i = 0; i < InternalConstants.D_CODES; i++)
            {
                this.dyn_dtree[i * 2] = 0;
            }

            for (int i = 0; i < InternalConstants.BL_CODES; i++)
            {
                this.bl_tree[i * 2] = 0;
            }

            this.dyn_ltree[DeflateManager.END_BLOCK * 2] = 1;
            this.opt_len = this.static_len = 0;
            this.last_lit = this.matches = 0;
        }

        // Restore the heap property by moving down the tree starting at node k,
        // exchanging a node with the smallest of its two sons if necessary, stopping
        // when the heap property is re-established (each father smaller than its
        // two sons).
        internal void pqdownheap(short[] tree, int k)
        {
            int v = this.heap[k];
            int j = k << 1; // left son of k
            while (j <= this.heap_len)
            {
                // Set j to the smallest of the two sons:
                if (j < this.heap_len && DeflateManager.m_IsSmaller(tree, this.heap[j + 1], this.heap[j], this.depth))
                {
                    j++;
                }

                // Exit if v is smaller than both sons
                if (DeflateManager.m_IsSmaller(tree, v, this.heap[j], this.depth))
                {
                    break;
                }

                // Exchange v with the smallest son
                this.heap[k] = this.heap[j];
                k = j;
                // And continue down the tree, setting j to the left son of k
                j <<= 1;
            }

            this.heap[k] = v;
        }

        internal static bool m_IsSmaller(short[] tree, int n, int m, sbyte[] depth)
        {
            short tn2 = tree[n * 2];
            short tm2 = tree[m * 2];
            return tn2 < tm2 || tn2 == tm2 && depth[n] <= depth[m];
        }


        // Scan a literal or distance tree to determine the frequencies of the codes
        // in the bit length tree.
        internal void scan_tree(short[] tree, int max_code)
        {
            int n; // iterates over all tree elements
            int prevlen = -1; // last emitted length
            int curlen; // length of current code
            int nextlen = tree[0 * 2 + 1]; // length of next code
            int count = 0; // repeat count of the current code
            int max_count = 7; // max repeat count
            int min_count = 4; // min repeat count

            if (nextlen == 0)
            {
                max_count = 138;
                min_count = 3;
            }

            tree[(max_code + 1) * 2 + 1] = 0x7fff; // guard //??

            for (n = 0; n <= max_code; n++)
            {
                curlen = nextlen;
                nextlen = tree[(n + 1) * 2 + 1];
                if (++count < max_count && curlen == nextlen)
                {
                    continue;
                }

                if (count < min_count)
                {
                    this.bl_tree[curlen * 2] = (short) (this.bl_tree[curlen * 2] + count);
                }
                else if (curlen != 0)
                {
                    if (curlen != prevlen)
                    {
                        this.bl_tree[curlen * 2]++;
                    }

                    this.bl_tree[InternalConstants.REP_3_6 * 2]++;
                }
                else if (count <= 10)
                {
                    this.bl_tree[InternalConstants.REPZ_3_10 * 2]++;
                }
                else
                {
                    this.bl_tree[InternalConstants.REPZ_11_138 * 2]++;
                }

                count = 0;
                prevlen = curlen;
                if (nextlen == 0)
                {
                    max_count = 138;
                    min_count = 3;
                }
                else if (curlen == nextlen)
                {
                    max_count = 6;
                    min_count = 3;
                }
                else
                {
                    max_count = 7;
                    min_count = 4;
                }
            }
        }

        // Construct the Huffman tree for the bit lengths and return the index in
        // bl_order of the last bit length code to send.
        internal int build_bl_tree()
        {
            int max_blindex; // index of last bit length code of non zero freq

            // Determine the bit length frequencies for literal and distance trees
            this.scan_tree(this.dyn_ltree, this.treeLiterals.max_code);
            this.scan_tree(this.dyn_dtree, this.treeDistances.max_code);

            // Build the bit length tree:
            this.treeBitLengths.build_tree(this);
            // opt_len now includes the length of the tree representations, except
            // the lengths of the bit lengths codes and the 5+5+4 bits for the counts.

            // Determine the number of bit length codes to send. The pkzip format
            // requires that at least 4 bit length codes be sent. (appnote.txt says
            // 3 but the actual value used is 4.)
            for (max_blindex = InternalConstants.BL_CODES - 1; max_blindex >= 3; max_blindex--)
            {
                if (this.bl_tree[Tree.bl_order[max_blindex] * 2 + 1] != 0)
                {
                    break;
                }
            }

            // Update opt_len to include the bit length tree and counts
            this.opt_len += 3 * (max_blindex + 1) + 5 + 5 + 4;

            return max_blindex;
        }


        // Send the header for a block using dynamic Huffman trees: the counts, the
        // lengths of the bit length codes, the literal tree and the distance tree.
        // IN assertion: lcodes >= 257, dcodes >= 1, blcodes >= 4.
        internal void send_all_trees(int lcodes, int dcodes, int blcodes)
        {
            int rank; // index in bl_order

            this.send_bits(lcodes - 257, 5); // not +255 as stated in appnote.txt
            this.send_bits(dcodes - 1, 5);
            this.send_bits(blcodes - 4, 4); // not -3 as stated in appnote.txt
            for (rank = 0; rank < blcodes; rank++)
            {
                this.send_bits(this.bl_tree[Tree.bl_order[rank] * 2 + 1], 3);
            }

            this.send_tree(this.dyn_ltree, lcodes - 1); // literal tree
            this.send_tree(this.dyn_dtree, dcodes - 1); // distance tree
        }

        // Send a literal or distance tree in compressed form, using the codes in
        // bl_tree.
        internal void send_tree(short[] tree, int max_code)
        {
            int n; // iterates over all tree elements
            int prevlen = -1; // last emitted length
            int curlen; // length of current code
            int nextlen = tree[0 * 2 + 1]; // length of next code
            int count = 0; // repeat count of the current code
            int max_count = 7; // max repeat count
            int min_count = 4; // min repeat count

            if (nextlen == 0)
            {
                max_count = 138;
                min_count = 3;
            }

            for (n = 0; n <= max_code; n++)
            {
                curlen = nextlen;
                nextlen = tree[(n + 1) * 2 + 1];
                if (++count < max_count && curlen == nextlen)
                {
                    continue;
                }

                if (count < min_count)
                {
                    do
                    {
                        this.send_code(curlen, this.bl_tree);
                    } while (--count != 0);
                }
                else if (curlen != 0)
                {
                    if (curlen != prevlen)
                    {
                        this.send_code(curlen, this.bl_tree);
                        count--;
                    }

                    this.send_code(InternalConstants.REP_3_6, this.bl_tree);
                    this.send_bits(count - 3, 2);
                }
                else if (count <= 10)
                {
                    this.send_code(InternalConstants.REPZ_3_10, this.bl_tree);
                    this.send_bits(count - 3, 3);
                }
                else
                {
                    this.send_code(InternalConstants.REPZ_11_138, this.bl_tree);
                    this.send_bits(count - 11, 7);
                }

                count = 0;
                prevlen = curlen;
                if (nextlen == 0)
                {
                    max_count = 138;
                    min_count = 3;
                }
                else if (curlen == nextlen)
                {
                    max_count = 6;
                    min_count = 3;
                }
                else
                {
                    max_count = 7;
                    min_count = 4;
                }
            }
        }

        // Output a block of bytes on the stream.
        // IN assertion: there is enough room in pending_buf.
        private void put_bytes(byte[] p, int start, int len)
        {
            Array.Copy(p, start, this.pending, this.pendingCount, len);
            this.pendingCount += len;
        }

    #if NOTNEEDED
        private void put_byte(byte c)
        {
            pending[pendingCount++] = c;
        }
        internal void put_short(int b)
        {
            unchecked
            {
                pending[pendingCount++] = (byte)b;
                pending[pendingCount++] = (byte)(b >> 8);
            }
        }
        internal void putShortMSB(int b)
        {
            unchecked
            {
                pending[pendingCount++] = (byte)(b >> 8);
                pending[pendingCount++] = (byte)b;
            }
        }
                #endif

        internal void send_code(int c, short[] tree)
        {
            int c2 = c * 2;
            this.send_bits(tree[c2] & 0xffff, tree[c2 + 1] & 0xffff);
        }

        internal void send_bits(int value, int length)
        {
            int len = length;
            unchecked
            {
                if (this.bi_valid > DeflateManager.Buf_size - len)
                {
                    //int val = value;
                    //      bi_buf |= (val << bi_valid);

                    this.bi_buf |= (short) ((value << this.bi_valid) & 0xffff);
                    //put_short(bi_buf);
                    this.pending[this.pendingCount++] = (byte) this.bi_buf;
                    this.pending[this.pendingCount++] = (byte) (this.bi_buf >> 8);


                    this.bi_buf = (short) ((uint) value >> (DeflateManager.Buf_size - this.bi_valid));
                    this.bi_valid += len - DeflateManager.Buf_size;
                }
                else
                {
                    //      bi_buf |= (value) << bi_valid;
                    this.bi_buf |= (short) ((value << this.bi_valid) & 0xffff);
                    this.bi_valid += len;
                }
            }
        }

        // Send one empty static block to give enough lookahead for inflate.
        // This takes 10 bits, of which 7 may remain in the bit buffer.
        // The current inflate code requires 9 bits of lookahead. If the
        // last two codes for the previous block (real code plus EOB) were coded
        // on 5 bits or less, inflate may have only 5+3 bits of lookahead to decode
        // the last real code. In this case we send two empty static blocks instead
        // of one. (There are no problems if the previous block is stored or fixed.)
        // To simplify the code, we assume the worst case of last real code encoded
        // on one bit only.
        internal void m_tr_align()
        {
            this.send_bits(DeflateManager.STATIC_TREES << 1, 3);
            this.send_code(DeflateManager.END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);

            this.bi_flush();

            // Of the 10 bits for the empty block, we have already sent
            // (10 - bi_valid) bits. The lookahead for the last real code (before
            // the EOB of the previous block) was thus at least one plus the length
            // of the EOB plus what we have just sent of the empty static block.
            if (1 + this.last_eob_len + 10 - this.bi_valid < 9)
            {
                this.send_bits(DeflateManager.STATIC_TREES << 1, 3);
                this.send_code(DeflateManager.END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);
                this.bi_flush();
            }

            this.last_eob_len = 7;
        }


        // Save the match info and tally the frequency counts. Return true if
        // the current block must be flushed.
        internal bool m_tr_tally(int dist, int lc)
        {
            this.pending[this.m_distanceOffset + this.last_lit * 2] = unchecked((byte) ((uint) dist >> 8));
            this.pending[this.m_distanceOffset + this.last_lit * 2 + 1] = unchecked((byte) dist);
            this.pending[this.m_lengthOffset + this.last_lit] = unchecked((byte) lc);
            this.last_lit++;

            if (dist == 0)
            {
                // lc is the unmatched char
                this.dyn_ltree[lc * 2]++;
            }
            else
            {
                this.matches++;
                // Here, lc is the match length - MIN_MATCH
                dist--; // dist = match distance - 1
                this.dyn_ltree[(Tree.LengthCode[lc] + InternalConstants.LITERALS + 1) * 2]++;
                this.dyn_dtree[Tree.DistanceCode(dist) * 2]++;
            }

            if ((this.last_lit & 0x1fff) == 0 && (int) this.compressionLevel > 2)
            {
                // Compute an upper bound for the compressed length
                int out_length = this.last_lit << 3;
                int in_length = this.strstart - this.block_start;
                int dcode;
                for (dcode = 0; dcode < InternalConstants.D_CODES; dcode++)
                {
                    out_length = (int) (out_length + this.dyn_dtree[dcode * 2] * (5L + Tree.ExtraDistanceBits[dcode]));
                }

                out_length >>= 3;
                if (this.matches < this.last_lit / 2 && out_length < in_length / 2)
                {
                    return true;
                }
            }

            return this.last_lit == this.lit_bufsize - 1 || this.last_lit == this.lit_bufsize;
            // dinoch - wraparound?
            // We avoid equality with lit_bufsize because of wraparound at 64K
            // on 16 bit machines and because stored blocks are restricted to
            // 64K-1 bytes.
        }


        // Send the block data compressed using the given Huffman trees
        internal void send_compressed_block(short[] ltree, short[] dtree)
        {
            int distance; // distance of matched string
            int lc; // match length or unmatched char (if dist == 0)
            int lx = 0; // running index in l_buf
            int code; // the code to send
            int extra; // number of extra bits to send

            if (this.last_lit != 0)
            {
                do
                {
                    int ix = this.m_distanceOffset + lx * 2;
                    distance = ((this.pending[ix] << 8) & 0xff00) |
                               (this.pending[ix + 1] & 0xff);
                    lc = this.pending[this.m_lengthOffset + lx] & 0xff;
                    lx++;

                    if (distance == 0)
                    {
                        this.send_code(lc, ltree); // send a literal byte
                    }
                    else
                    {
                        // literal or match pair
                        // Here, lc is the match length - MIN_MATCH
                        code = Tree.LengthCode[lc];

                        // send the length code
                        this.send_code(code + InternalConstants.LITERALS + 1, ltree);
                        extra = Tree.ExtraLengthBits[code];
                        if (extra != 0)
                        {
                            // send the extra length bits
                            lc -= Tree.LengthBase[code];
                            this.send_bits(lc, extra);
                        }

                        distance--; // dist is now the match distance - 1
                        code = Tree.DistanceCode(distance);

                        // send the distance code
                        this.send_code(code, dtree);

                        extra = Tree.ExtraDistanceBits[code];
                        if (extra != 0)
                        {
                            // send the extra distance bits
                            distance -= Tree.DistanceBase[code];
                            this.send_bits(distance, extra);
                        }
                    }

                    // Check that the overlay between pending and d_buf+l_buf is ok:
                } while (lx < this.last_lit);
            }

            this.send_code(DeflateManager.END_BLOCK, ltree);
            this.last_eob_len = ltree[DeflateManager.END_BLOCK * 2 + 1];
        }


        // Set the data type to ASCII or BINARY, using a crude approximation:
        // binary if more than 20% of the bytes are <= 6 or >= 128, ascii otherwise.
        // IN assertion: the fields freq of dyn_ltree are set and the total of all
        // frequencies does not exceed 64K (to fit in an int on 16 bit machines).
        internal void set_data_type()
        {
            int n = 0;
            int ascii_freq = 0;
            int bin_freq = 0;
            while (n < 7)
            {
                bin_freq += this.dyn_ltree[n * 2];
                n++;
            }

            while (n < 128)
            {
                ascii_freq += this.dyn_ltree[n * 2];
                n++;
            }

            while (n < InternalConstants.LITERALS)
            {
                bin_freq += this.dyn_ltree[n * 2];
                n++;
            }

            this.data_type = (sbyte) (bin_freq > ascii_freq >> 2 ? DeflateManager.Z_BINARY : DeflateManager.Z_ASCII);
        }


        // Flush the bit buffer, keeping at most 7 bits in it.
        internal void bi_flush()
        {
            if (this.bi_valid == 16)
            {
                this.pending[this.pendingCount++] = (byte) this.bi_buf;
                this.pending[this.pendingCount++] = (byte) (this.bi_buf >> 8);
                this.bi_buf = 0;
                this.bi_valid = 0;
            }
            else if (this.bi_valid >= 8)
            {
                //put_byte((byte)bi_buf);
                this.pending[this.pendingCount++] = (byte) this.bi_buf;
                this.bi_buf >>= 8;
                this.bi_valid -= 8;
            }
        }

        // Flush the bit buffer and align the output on a byte boundary
        internal void bi_windup()
        {
            if (this.bi_valid > 8)
            {
                this.pending[this.pendingCount++] = (byte) this.bi_buf;
                this.pending[this.pendingCount++] = (byte) (this.bi_buf >> 8);
            }
            else if (this.bi_valid > 0)
            {
                //put_byte((byte)bi_buf);
                this.pending[this.pendingCount++] = (byte) this.bi_buf;
            }

            this.bi_buf = 0;
            this.bi_valid = 0;
        }

        // Copy a stored block, storing first the length and its
        // one's complement if requested.
        internal void copy_block(int buf, int len, bool header)
        {
            this.bi_windup(); // align on byte boundary
            this.last_eob_len = 8; // enough lookahead for inflate

            if (header)
            {
                unchecked
                {
                    //put_short((short)len);
                    this.pending[this.pendingCount++] = (byte) len;
                    this.pending[this.pendingCount++] = (byte) (len >> 8);
                    //put_short((short)~len);
                    this.pending[this.pendingCount++] = (byte) ~len;
                    this.pending[this.pendingCount++] = (byte) (~len >> 8);
                }
            }

            this.put_bytes(this.window, buf, len);
        }

        internal void flush_block_only(bool eof)
        {
            this.m_tr_flush_block(this.block_start >= 0 ? this.block_start : -1, this.strstart - this.block_start, eof);
            this.block_start = this.strstart;
            this.m_codec.flush_pending();
        }

        // Copy without compression as much as possible from the input stream, return
        // the current block state.
        // This function does not insert new strings in the dictionary since
        // uncompressible data is probably not useful. This function is used
        // only for the level=0 compression option.
        // NOTE: this function should be optimized to avoid extra copying from
        // window to pending_buf.
        internal BlockState DeflateNone(FlushType flush)
        {
            // Stored blocks are limited to 0xffff bytes, pending is limited
            // to pending_buf_size, and each stored block has a 5 byte header:

            int max_block_size = 0xffff;
            int max_start;

            if (max_block_size > this.pending.Length - 5)
            {
                max_block_size = this.pending.Length - 5;
            }

            // Copy as much as possible from input to output:
            while (true)
            {
                // Fill the window as much as possible:
                if (this.lookahead <= 1)
                {
                    this.m_fillWindow();
                    if (this.lookahead == 0 && flush == FlushType.None)
                    {
                        return BlockState.NeedMore;
                    }

                    if (this.lookahead == 0)
                    {
                        break; // flush the current block
                    }
                }

                this.strstart += this.lookahead;
                this.lookahead = 0;

                // Emit a stored block if pending will be full:
                max_start = this.block_start + max_block_size;
                if (this.strstart == 0 || this.strstart >= max_start)
                {
                    // strstart == 0 is possible when wraparound on 16-bit machine
                    this.lookahead = this.strstart - max_start;
                    this.strstart = max_start;

                    this.flush_block_only(false);
                    if (this.m_codec.AvailableBytesOut == 0)
                    {
                        return BlockState.NeedMore;
                    }
                }

                // Flush if we may have to slide, otherwise block_start may become
                // negative and the data will be gone:
                if (this.strstart - this.block_start >= this.w_size - DeflateManager.MIN_LOOKAHEAD)
                {
                    this.flush_block_only(false);
                    if (this.m_codec.AvailableBytesOut == 0)
                    {
                        return BlockState.NeedMore;
                    }
                }
            }

            this.flush_block_only(flush == FlushType.Finish);
            if (this.m_codec.AvailableBytesOut == 0)
            {
                return flush == FlushType.Finish ? BlockState.FinishStarted : BlockState.NeedMore;
            }

            return flush == FlushType.Finish ? BlockState.FinishDone : BlockState.BlockDone;
        }


        // Send a stored block
        internal void m_tr_stored_block(int buf, int stored_len, bool eof)
        {
            this.send_bits((DeflateManager.STORED_BLOCK << 1) + (eof ? 1 : 0), 3); // send block type
            this.copy_block(buf, stored_len, true); // with header
        }

        // Determine the best encoding for the current block: dynamic trees, static
        // trees or store, and output the encoded block to the zip file.
        internal void m_tr_flush_block(int buf, int stored_len, bool eof)
        {
            int opt_lenb, static_lenb; // opt_len and static_len in bytes
            int max_blindex = 0; // index of last bit length code of non zero freq

            // Build the Huffman trees unless a stored block is forced
            if (this.compressionLevel > 0)
            {
                // Check if the file is ascii or binary
                if (this.data_type == DeflateManager.Z_UNKNOWN)
                {
                    this.set_data_type();
                }

                // Construct the literal and distance trees
                this.treeLiterals.build_tree(this);

                this.treeDistances.build_tree(this);

                // At this point, opt_len and static_len are the total bit lengths of
                // the compressed block data, excluding the tree representations.

                // Build the bit length tree for the above two trees, and get the index
                // in bl_order of the last bit length code to send.
                max_blindex = this.build_bl_tree();

                // Determine the best encoding. Compute first the block length in bytes
                opt_lenb = (this.opt_len + 3 + 7) >> 3;
                static_lenb = (this.static_len + 3 + 7) >> 3;

                if (static_lenb <= opt_lenb)
                {
                    opt_lenb = static_lenb;
                }
            }
            else
            {
                opt_lenb = static_lenb = stored_len + 5; // force a stored block
            }

            if (stored_len + 4 <= opt_lenb && buf != -1)
            {
                // 4: two words for the lengths
                // The test buf != NULL is only necessary if LIT_BUFSIZE > WSIZE.
                // Otherwise we can't have processed more than WSIZE input bytes since
                // the last block flush, because compression would have been
                // successful. If LIT_BUFSIZE <= WSIZE, it is never too late to
                // transform a block into a stored block.
                this.m_tr_stored_block(buf, stored_len, eof);
            }
            else if (static_lenb == opt_lenb)
            {
                this.send_bits((DeflateManager.STATIC_TREES << 1) + (eof ? 1 : 0), 3);
                this.send_compressed_block(StaticTree.lengthAndLiteralsTreeCodes, StaticTree.distTreeCodes);
            }
            else
            {
                this.send_bits((DeflateManager.DYN_TREES << 1) + (eof ? 1 : 0), 3);
                this.send_all_trees(this.treeLiterals.max_code + 1, this.treeDistances.max_code + 1, max_blindex + 1);
                this.send_compressed_block(this.dyn_ltree, this.dyn_dtree);
            }

            // The above check is made mod 2^32, for files larger than 512 MB
            // and uLong implemented on 32 bits.

            this.m_InitializeBlocks();

            if (eof)
            {
                this.bi_windup();
            }
        }

        // Fill the window when the lookahead becomes insufficient.
        // Updates strstart and lookahead.
        //
        // IN assertion: lookahead < MIN_LOOKAHEAD
        // OUT assertions: strstart <= window_size-MIN_LOOKAHEAD
        //    At least one byte has been read, or avail_in == 0; reads are
        //    performed for at least two bytes (required for the zip translate_eol
        //    option -- not supported here).
        private void m_fillWindow()
        {
            int n, m;
            int p;
            int more; // Amount of free space at the end of the window.

            do
            {
                more = this.window_size - this.lookahead - this.strstart;

                // Deal with !@#$% 64K limit:
                if (more == 0 && this.strstart == 0 && this.lookahead == 0)
                {
                    more = this.w_size;
                }
                else if (more == -1)
                {
                    // Very unlikely, but possible on 16 bit machine if strstart == 0
                    // and lookahead == 1 (input done one byte at time)
                    more--;

                    // If the window is almost full and there is insufficient lookahead,
                    // move the upper half to the lower one to make room in the upper half.
                }
                else if (this.strstart >= this.w_size + this.w_size - DeflateManager.MIN_LOOKAHEAD)
                {
                    Array.Copy(this.window, this.w_size, this.window, 0, this.w_size);
                    this.match_start -= this.w_size;
                    this.strstart -= this.w_size; // we now have strstart >= MAX_DIST
                    this.block_start -= this.w_size;

                    // Slide the hash table (could be avoided with 32 bit values
                    // at the expense of memory usage). We slide even when level == 0
                    // to keep the hash table consistent if we switch back to level > 0
                    // later. (Using level 0 permanently is not an optimal usage of
                    // zlib, so we don't care about this pathological case.)

                    n = this.hash_size;
                    p = n;
                    do
                    {
                        m = this.head[--p] & 0xffff;
                        this.head[p] = (short) (m >= this.w_size ? m - this.w_size : 0);
                    } while (--n != 0);

                    n = this.w_size;
                    p = n;
                    do
                    {
                        m = this.prev[--p] & 0xffff;
                        this.prev[p] = (short) (m >= this.w_size ? m - this.w_size : 0);
                        // If n is not on any hash chain, prev[n] is garbage but
                        // its value will never be used.
                    } while (--n != 0);

                    more += this.w_size;
                }

                if (this.m_codec.AvailableBytesIn == 0)
                {
                    return;
                }

                // If there was no sliding:
                //    strstart <= WSIZE+MAX_DIST-1 && lookahead <= MIN_LOOKAHEAD - 1 &&
                //    more == window_size - lookahead - strstart
                // => more >= window_size - (MIN_LOOKAHEAD-1 + WSIZE + MAX_DIST-1)
                // => more >= window_size - 2*WSIZE + 2
                // In the BIG_MEM or MMAP case (not yet supported),
                //   window_size == input_size + MIN_LOOKAHEAD  &&
                //   strstart + s->lookahead <= input_size => more >= MIN_LOOKAHEAD.
                // Otherwise, window_size == 2*WSIZE so more >= 2.
                // If there was sliding, more >= WSIZE. So in all cases, more >= 2.

                n = this.m_codec.read_buf(this.window, this.strstart + this.lookahead, more);
                this.lookahead += n;

                // Initialize the hash value now that we have some input:
                if (this.lookahead >= DeflateManager.MIN_MATCH)
                {
                    this.ins_h = this.window[this.strstart] & 0xff;
                    this.ins_h = ((this.ins_h << this.hash_shift) ^ (this.window[this.strstart + 1] & 0xff)) & this.hash_mask;
                }

                // If the whole input has less than MIN_MATCH bytes, ins_h is garbage,
                // but this is not important since only literal bytes will be emitted.
            } while (this.lookahead < DeflateManager.MIN_LOOKAHEAD && this.m_codec.AvailableBytesIn != 0);
        }

        // Compress as much as possible from the input stream, return the current
        // block state.
        // This function does not perform lazy evaluation of matches and inserts
        // new strings in the dictionary only for unmatched strings or for short
        // matches. It is used only for the fast compression options.
        internal BlockState DeflateFast(FlushType flush)
        {
            //    short hash_head = 0; // head of the hash chain
            int hash_head = 0; // head of the hash chain
            bool bflush; // set if current block must be flushed

            while (true)
            {
                // Make sure that we always have enough lookahead, except
                // at the end of the input file. We need MAX_MATCH bytes
                // for the next match, plus MIN_MATCH bytes to insert the
                // string following the next match.
                if (this.lookahead < DeflateManager.MIN_LOOKAHEAD)
                {
                    this.m_fillWindow();
                    if (this.lookahead < DeflateManager.MIN_LOOKAHEAD && flush == FlushType.None)
                    {
                        return BlockState.NeedMore;
                    }

                    if (this.lookahead == 0)
                    {
                        break; // flush the current block
                    }
                }

                // Insert the string window[strstart .. strstart+2] in the
                // dictionary, and set hash_head to the head of the hash chain:
                if (this.lookahead >= DeflateManager.MIN_MATCH)
                {
                    this.ins_h = ((this.ins_h << this.hash_shift) ^ (this.window[this.strstart + (DeflateManager.MIN_MATCH - 1)] & 0xff)) & this.hash_mask;

                    //  prev[strstart&w_mask]=hash_head=head[ins_h];
                    hash_head = this.head[this.ins_h] & 0xffff;
                    this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
                    this.head[this.ins_h] = unchecked((short) this.strstart);
                }

                // Find the longest match, discarding those <= prev_length.
                // At this point we have always match_length < MIN_MATCH

                if (hash_head != 0L && ((this.strstart - hash_head) & 0xffff) <= this.w_size - DeflateManager.MIN_LOOKAHEAD)
                {
                    // To simplify the code, we prevent matches with the string
                    // of window index 0 (in particular we have to avoid a match
                    // of the string with itself at the start of the input file).
                    if (this.compressionStrategy != CompressionStrategy.HuffmanOnly)
                    {
                        this.match_length = this.longest_match(hash_head);
                    }

                    // longest_match() sets match_start
                }

                if (this.match_length >= DeflateManager.MIN_MATCH)
                {
                    //        check_match(strstart, match_start, match_length);

                    bflush = this.m_tr_tally(this.strstart - this.match_start, this.match_length - DeflateManager.MIN_MATCH);

                    this.lookahead -= this.match_length;

                    // Insert new strings in the hash table only if the match length
                    // is not too large. This saves time but degrades compression.
                    if (this.match_length <= this.config.MaxLazy && this.lookahead >= DeflateManager.MIN_MATCH)
                    {
                        this.match_length--; // string at strstart already in hash table
                        do
                        {
                            this.strstart++;

                            this.ins_h = ((this.ins_h << this.hash_shift) ^ (this.window[this.strstart + (DeflateManager.MIN_MATCH - 1)] & 0xff)) & this.hash_mask;
                            //      prev[strstart&w_mask]=hash_head=head[ins_h];
                            hash_head = this.head[this.ins_h] & 0xffff;
                            this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
                            this.head[this.ins_h] = unchecked((short) this.strstart);

                            // strstart never exceeds WSIZE-MAX_MATCH, so there are
                            // always MIN_MATCH bytes ahead.
                        } while (--this.match_length != 0);

                        this.strstart++;
                    }
                    else
                    {
                        this.strstart += this.match_length;
                        this.match_length = 0;
                        this.ins_h = this.window[this.strstart] & 0xff;

                        this.ins_h = ((this.ins_h << this.hash_shift) ^ (this.window[this.strstart + 1] & 0xff)) & this.hash_mask;
                        // If lookahead < MIN_MATCH, ins_h is garbage, but it does not
                        // matter since it will be recomputed at next deflate call.
                    }
                }
                else
                {
                    // No match, output a literal byte

                    bflush = this.m_tr_tally(0, this.window[this.strstart] & 0xff);
                    this.lookahead--;
                    this.strstart++;
                }

                if (bflush)
                {
                    this.flush_block_only(false);
                    if (this.m_codec.AvailableBytesOut == 0)
                    {
                        return BlockState.NeedMore;
                    }
                }
            }

            this.flush_block_only(flush == FlushType.Finish);
            if (this.m_codec.AvailableBytesOut == 0)
            {
                if (flush == FlushType.Finish)
                {
                    return BlockState.FinishStarted;
                }

                return BlockState.NeedMore;
            }

            return flush == FlushType.Finish ? BlockState.FinishDone : BlockState.BlockDone;
        }

        // Same as above, but achieves better compression. We use a lazy
        // evaluation for matches: a match is finally adopted only if there is
        // no better match at the next window position.
        internal BlockState DeflateSlow(FlushType flush)
        {
            //    short hash_head = 0;    // head of hash chain
            int hash_head = 0; // head of hash chain
            bool bflush; // set if current block must be flushed

            // Process the input block.
            while (true)
            {
                // Make sure that we always have enough lookahead, except
                // at the end of the input file. We need MAX_MATCH bytes
                // for the next match, plus MIN_MATCH bytes to insert the
                // string following the next match.

                if (this.lookahead < DeflateManager.MIN_LOOKAHEAD)
                {
                    this.m_fillWindow();
                    if (this.lookahead < DeflateManager.MIN_LOOKAHEAD && flush == FlushType.None)
                    {
                        return BlockState.NeedMore;
                    }

                    if (this.lookahead == 0)
                    {
                        break; // flush the current block
                    }
                }

                // Insert the string window[strstart .. strstart+2] in the
                // dictionary, and set hash_head to the head of the hash chain:

                if (this.lookahead >= DeflateManager.MIN_MATCH)
                {
                    this.ins_h = ((this.ins_h << this.hash_shift) ^ (this.window[this.strstart + (DeflateManager.MIN_MATCH - 1)] & 0xff)) & this.hash_mask;
                    //  prev[strstart&w_mask]=hash_head=head[ins_h];
                    hash_head = this.head[this.ins_h] & 0xffff;
                    this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
                    this.head[this.ins_h] = unchecked((short) this.strstart);
                }

                // Find the longest match, discarding those <= prev_length.
                this.prev_length = this.match_length;
                this.prev_match = this.match_start;
                this.match_length = DeflateManager.MIN_MATCH - 1;

                if (hash_head != 0 && this.prev_length < this.config.MaxLazy &&
                    ((this.strstart - hash_head) & 0xffff) <= this.w_size - DeflateManager.MIN_LOOKAHEAD)
                {
                    // To simplify the code, we prevent matches with the string
                    // of window index 0 (in particular we have to avoid a match
                    // of the string with itself at the start of the input file).

                    if (this.compressionStrategy != CompressionStrategy.HuffmanOnly)
                    {
                        this.match_length = this.longest_match(hash_head);
                    }
                    // longest_match() sets match_start

                    if (this.match_length <= 5 && (this.compressionStrategy == CompressionStrategy.Filtered ||
                                                   this.match_length == DeflateManager.MIN_MATCH && this.strstart - this.match_start > 4096))
                    {
                        // If prev_match is also MIN_MATCH, match_start is garbage
                        // but we will ignore the current match anyway.
                        this.match_length = DeflateManager.MIN_MATCH - 1;
                    }
                }

                // If there was a match at the previous step and the current
                // match is not better, output the previous match:
                if (this.prev_length >= DeflateManager.MIN_MATCH && this.match_length <= this.prev_length)
                {
                    int max_insert = this.strstart + this.lookahead - DeflateManager.MIN_MATCH;
                    // Do not insert strings in hash table beyond this.

                    //          check_match(strstart-1, prev_match, prev_length);

                    bflush = this.m_tr_tally(this.strstart - 1 - this.prev_match, this.prev_length - DeflateManager.MIN_MATCH);

                    // Insert in hash table all strings up to the end of the match.
                    // strstart-1 and strstart are already inserted. If there is not
                    // enough lookahead, the last two strings are not inserted in
                    // the hash table.
                    this.lookahead -= this.prev_length - 1;
                    this.prev_length -= 2;
                    do
                    {
                        if (++this.strstart <= max_insert)
                        {
                            this.ins_h = ((this.ins_h << this.hash_shift) ^ (this.window[this.strstart + (DeflateManager.MIN_MATCH - 1)] & 0xff)) & this.hash_mask;
                            //prev[strstart&w_mask]=hash_head=head[ins_h];
                            hash_head = this.head[this.ins_h] & 0xffff;
                            this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
                            this.head[this.ins_h] = unchecked((short) this.strstart);
                        }
                    } while (--this.prev_length != 0);

                    this.match_available = 0;
                    this.match_length = DeflateManager.MIN_MATCH - 1;
                    this.strstart++;

                    if (bflush)
                    {
                        this.flush_block_only(false);
                        if (this.m_codec.AvailableBytesOut == 0)
                        {
                            return BlockState.NeedMore;
                        }
                    }
                }
                else if (this.match_available != 0)
                {
                    // If there was no match at the previous position, output a
                    // single literal. If there was a match but the current match
                    // is longer, truncate the previous match to a single literal.

                    bflush = this.m_tr_tally(0, this.window[this.strstart - 1] & 0xff);

                    if (bflush)
                    {
                        this.flush_block_only(false);
                    }

                    this.strstart++;
                    this.lookahead--;
                    if (this.m_codec.AvailableBytesOut == 0)
                    {
                        return BlockState.NeedMore;
                    }
                }
                else
                {
                    // There is no previous match to compare with, wait for
                    // the next step to decide.

                    this.match_available = 1;
                    this.strstart++;
                    this.lookahead--;
                }
            }

            if (this.match_available != 0)
            {
                bflush = this.m_tr_tally(0, this.window[this.strstart - 1] & 0xff);
                this.match_available = 0;
            }

            this.flush_block_only(flush == FlushType.Finish);

            if (this.m_codec.AvailableBytesOut == 0)
            {
                if (flush == FlushType.Finish)
                {
                    return BlockState.FinishStarted;
                }

                return BlockState.NeedMore;
            }

            return flush == FlushType.Finish ? BlockState.FinishDone : BlockState.BlockDone;
        }


        internal int longest_match(int cur_match)
        {
            int chain_length = this.config.MaxChainLength; // max hash chain length
            int scan = this.strstart; // current string
            int match; // matched string
            int len; // length of current match
            int best_len = this.prev_length; // best match length so far
            int limit = this.strstart > this.w_size - DeflateManager.MIN_LOOKAHEAD ? this.strstart - (this.w_size - DeflateManager.MIN_LOOKAHEAD) : 0;

            int niceLength = this.config.NiceLength;

            // Stop when cur_match becomes <= limit. To simplify the code,
            // we prevent matches with the string of window index 0.

            int wmask = this.w_mask;

            int strend = this.strstart + DeflateManager.MAX_MATCH;
            byte scan_end1 = this.window[scan + best_len - 1];
            byte scan_end = this.window[scan + best_len];

            // The code is optimized for HASH_BITS >= 8 and MAX_MATCH-2 multiple of 16.
            // It is easy to get rid of this optimization if necessary.

            // Do not waste too much time if we already have a good match:
            if (this.prev_length >= this.config.GoodLength)
            {
                chain_length >>= 2;
            }

            // Do not look for matches beyond the end of the input. This is necessary
            // to make deflate deterministic.
            if (niceLength > this.lookahead)
            {
                niceLength = this.lookahead;
            }

            do
            {
                match = cur_match;

                // Skip to next match if the match length cannot increase
                // or if the match length is less than 2:
                if (this.window[match + best_len] != scan_end || this.window[match + best_len - 1] != scan_end1 || this.window[match] != this.window[scan] ||
                    this.window[++match] != this.window[scan + 1])
                {
                    continue;
                }

                // The check at best_len-1 can be removed because it will be made
                // again later. (This heuristic is not always a win.)
                // It is not necessary to compare scan[2] and match[2] since they
                // are always equal when the other bytes match, given that
                // the hash keys are equal and that HASH_BITS >= 8.
                scan += 2;
                match++;

                // We check for insufficient lookahead only every 8th comparison;
                // the 256th check will be made at strstart+258.
                do
                {
                } while (this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] &&
                         this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] &&
                         this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && scan < strend);

                len = DeflateManager.MAX_MATCH - (strend - scan);
                scan = strend - DeflateManager.MAX_MATCH;

                if (len > best_len)
                {
                    this.match_start = cur_match;
                    best_len = len;
                    if (len >= niceLength)
                    {
                        break;
                    }

                    scan_end1 = this.window[scan + best_len - 1];
                    scan_end = this.window[scan + best_len];
                }
            } while ((cur_match = this.prev[cur_match & wmask] & 0xffff) > limit && --chain_length != 0);

            if (best_len <= this.lookahead)
            {
                return best_len;
            }

            return this.lookahead;
        }


        private bool Rfc1950BytesEmitted;
        internal bool WantRfc1950HeaderBytes { get; set; } = true;


        internal int Initialize(ZLibCodec codec, CompressionLevel level)
        {
            return this.Initialize(codec, level, ZLibConstants.WindowBitsMax);
        }

        internal int Initialize(ZLibCodec codec, CompressionLevel level, int bits)
        {
            return this.Initialize(codec, level, bits, DeflateManager.MEM_LEVEL_DEFAULT, CompressionStrategy.Default);
        }

        internal int Initialize(ZLibCodec codec, CompressionLevel level, int bits, CompressionStrategy compressionStrategy)
        {
            return this.Initialize(codec, level, bits, DeflateManager.MEM_LEVEL_DEFAULT, compressionStrategy);
        }

        internal int Initialize(ZLibCodec codec, CompressionLevel level, int windowBits, int memLevel, CompressionStrategy strategy)
        {
            this.m_codec = codec;
            this.m_codec.Message = null;

            // validation
            if (windowBits < 9 || windowBits > 15)
            {
                throw new ZLibException("windowBits must be in the range 9..15.");
            }

            if (memLevel < 1 || memLevel > DeflateManager.MEM_LEVEL_MAX)
            {
                throw new ZLibException(string.Format("memLevel must be in the range 1.. {0}", DeflateManager.MEM_LEVEL_MAX));
            }

            this.m_codec.dstate = this;

            this.w_bits = windowBits;
            this.w_size = 1 << this.w_bits;
            this.w_mask = this.w_size - 1;

            this.hash_bits = memLevel + 7;
            this.hash_size = 1 << this.hash_bits;
            this.hash_mask = this.hash_size - 1;
            this.hash_shift = (this.hash_bits + DeflateManager.MIN_MATCH - 1) / DeflateManager.MIN_MATCH;

            this.window = new byte[this.w_size * 2];
            this.prev = new short[this.w_size];
            this.head = new short[this.hash_size];

            // for memLevel==8, this will be 16384, 16k
            this.lit_bufsize = 1 << (memLevel + 6);

            // Use a single array as the buffer for data pending compression,
            // the output distance codes, and the output length codes (aka tree).
            // orig comment: This works just fine since the average
            // output size for (length,distance) codes is <= 24 bits.
            this.pending = new byte[this.lit_bufsize * 4];
            this.m_distanceOffset = this.lit_bufsize;
            this.m_lengthOffset = (1 + 2) * this.lit_bufsize;

            // So, for memLevel 8, the length of the pending buffer is 65536. 64k.
            // The first 16k are pending bytes.
            // The middle slice, of 32k, is used for distance codes.
            // The final 16k are length codes.

            this.compressionLevel = level;
            this.compressionStrategy = strategy;

            this.Reset();
            return ZLibConstants.Z_OK;
        }


        internal void Reset()
        {
            this.m_codec.TotalBytesIn = this.m_codec.TotalBytesOut = 0;
            this.m_codec.Message = null;
            //strm.data_type = Z_UNKNOWN;

            this.pendingCount = 0;
            this.nextPending = 0;

            this.Rfc1950BytesEmitted = false;

            this.status = this.WantRfc1950HeaderBytes ? DeflateManager.INIT_STATE : DeflateManager.BUSY_STATE;
            this.m_codec.m_Adler32 = Adler.Adler32(0, null, 0, 0);

            this.last_flush = (int) FlushType.None;

            this.m_InitializeTreeData();
            this.m_InitializeLazyMatch();
        }


        internal int End()
        {
            if (this.status != DeflateManager.INIT_STATE && this.status != DeflateManager.BUSY_STATE && this.status != DeflateManager.FINISH_STATE)
            {
                return ZLibConstants.Z_STREAM_ERROR;
            }

            // Deallocate in reverse order of allocations:
            this.pending = null;
            this.head = null;
            this.prev = null;
            this.window = null;
            // free
            // dstate=null;
            return this.status == DeflateManager.BUSY_STATE ? ZLibConstants.Z_DATA_ERROR : ZLibConstants.Z_OK;
        }


        private void SetDeflater()
        {
            switch (this.config.Flavor)
            {
                case DeflateFlavor.Store:
                    this.DeflateFunction = this.DeflateNone;
                    break;
                case DeflateFlavor.Fast:
                    this.DeflateFunction = this.DeflateFast;
                    break;
                case DeflateFlavor.Slow:
                    this.DeflateFunction = this.DeflateSlow;
                    break;
            }
        }


        internal int SetParams(CompressionLevel level, CompressionStrategy strategy)
        {
            int result = ZLibConstants.Z_OK;

            if (this.compressionLevel != level)
            {
                Config newConfig = Config.Lookup(level);

                // change in the deflate flavor (Fast vs slow vs none)?
                if (newConfig.Flavor != this.config.Flavor && this.m_codec.TotalBytesIn != 0)
                {
                    // Flush the last buffer:
                    result = this.m_codec.Deflate(FlushType.Partial);
                }

                this.compressionLevel = level;
                this.config = newConfig;
                this.SetDeflater();
            }

            // no need to flush with change in strategy?  Really?
            this.compressionStrategy = strategy;

            return result;
        }


        internal int SetDictionary(byte[] dictionary)
        {
            int length = dictionary.Length;
            int index = 0;

            if (dictionary == null || this.status != DeflateManager.INIT_STATE)
            {
                throw new ZLibException("Stream error.");
            }

            this.m_codec.m_Adler32 = Adler.Adler32(this.m_codec.m_Adler32, dictionary, 0, dictionary.Length);

            if (length < DeflateManager.MIN_MATCH)
            {
                return ZLibConstants.Z_OK;
            }

            if (length > this.w_size - DeflateManager.MIN_LOOKAHEAD)
            {
                length = this.w_size - DeflateManager.MIN_LOOKAHEAD;
                index = dictionary.Length - length; // use the tail of the dictionary
            }

            Array.Copy(dictionary, index, this.window, 0, length);
            this.strstart = length;
            this.block_start = length;

            // Insert all strings in the hash table (except for the last two bytes).
            // s->lookahead stays null, so s->ins_h will be recomputed at the next
            // call of fill_window.

            this.ins_h = this.window[0] & 0xff;
            this.ins_h = ((this.ins_h << this.hash_shift) ^ (this.window[1] & 0xff)) & this.hash_mask;

            for (int n = 0; n <= length - DeflateManager.MIN_MATCH; n++)
            {
                this.ins_h = ((this.ins_h << this.hash_shift) ^ (this.window[n + (DeflateManager.MIN_MATCH - 1)] & 0xff)) & this.hash_mask;
                this.prev[n & this.w_mask] = this.head[this.ins_h];
                this.head[this.ins_h] = (short) n;
            }

            return ZLibConstants.Z_OK;
        }


        internal int Deflate(FlushType flush)
        {
            int old_flush;

            if (this.m_codec.OutputBuffer == null ||
                this.m_codec.InputBuffer == null && this.m_codec.AvailableBytesIn != 0 ||
                this.status == DeflateManager.FINISH_STATE && flush != FlushType.Finish)
            {
                this.m_codec.Message = DeflateManager.m_ErrorMessage[ZLibConstants.Z_NEED_DICT - ZLibConstants.Z_STREAM_ERROR];
                throw new ZLibException(string.Format("Something is fishy. [{0}]", this.m_codec.Message));
            }

            if (this.m_codec.AvailableBytesOut == 0)
            {
                this.m_codec.Message = DeflateManager.m_ErrorMessage[ZLibConstants.Z_NEED_DICT - ZLibConstants.Z_BUF_ERROR];
                throw new ZLibException("OutputBuffer is full (AvailableBytesOut == 0)");
            }

            old_flush = this.last_flush;
            this.last_flush = (int) flush;

            // Write the zlib (rfc1950) header bytes
            if (this.status == DeflateManager.INIT_STATE)
            {
                int header = (DeflateManager.Z_DEFLATED + ((this.w_bits - 8) << 4)) << 8;
                int level_flags = (((int) this.compressionLevel - 1) & 0xff) >> 1;

                if (level_flags > 3)
                {
                    level_flags = 3;
                }

                header |= level_flags << 6;
                if (this.strstart != 0)
                {
                    header |= DeflateManager.PRESET_DICT;
                }

                header += 31 - header % 31;

                this.status = DeflateManager.BUSY_STATE;
                //putShortMSB(header);
                unchecked
                {
                    this.pending[this.pendingCount++] = (byte) (header >> 8);
                    this.pending[this.pendingCount++] = (byte) header;
                }

                // Save the adler32 of the preset dictionary:
                if (this.strstart != 0)
                {
                    this.pending[this.pendingCount++] = (byte) ((this.m_codec.m_Adler32 & 0xFF000000) >> 24);
                    this.pending[this.pendingCount++] = (byte) ((this.m_codec.m_Adler32 & 0x00FF0000) >> 16);
                    this.pending[this.pendingCount++] = (byte) ((this.m_codec.m_Adler32 & 0x0000FF00) >> 8);
                    this.pending[this.pendingCount++] = (byte) (this.m_codec.m_Adler32 & 0x000000FF);
                }

                this.m_codec.m_Adler32 = Adler.Adler32(0, null, 0, 0);
            }

            // Flush as much pending output as possible
            if (this.pendingCount != 0)
            {
                this.m_codec.flush_pending();
                if (this.m_codec.AvailableBytesOut == 0)
                {
                    //System.out.println("  avail_out==0");
                    // Since avail_out is 0, deflate will be called again with
                    // more output space, but possibly with both pending and
                    // avail_in equal to zero. There won't be anything to do,
                    // but this is not an error situation so make sure we
                    // return OK instead of BUF_ERROR at next call of deflate:
                    this.last_flush = -1;
                    return ZLibConstants.Z_OK;
                }

                // Make sure there is something to do and avoid duplicate consecutive
                // flushes. For repeated and useless calls with Z_FINISH, we keep
                // returning Z_STREAM_END instead of Z_BUFF_ERROR.
            }
            else if (this.m_codec.AvailableBytesIn == 0 &&
                     (int) flush <= old_flush &&
                     flush != FlushType.Finish)
            {
                // workitem 8557
                //
                // Not sure why this needs to be an error.  pendingCount == 0, which
                // means there's nothing to deflate.  And the caller has not asked
                // for a FlushType.Finish, but...  that seems very non-fatal.  We
                // can just say "OK" and do nothing.

                // m_codec.Message = z_errmsg[ZLibConstants.Z_NEED_DICT - (ZLibConstants.Z_BUF_ERROR)];
                // throw new ZLibException("AvailableBytesIn == 0 && flush<=old_flush && flush != FlushType.Finish");

                return ZLibConstants.Z_OK;
            }

            // User must not provide more input after the first FINISH:
            if (this.status == DeflateManager.FINISH_STATE && this.m_codec.AvailableBytesIn != 0)
            {
                this.m_codec.Message = DeflateManager.m_ErrorMessage[ZLibConstants.Z_NEED_DICT - ZLibConstants.Z_BUF_ERROR];
                throw new ZLibException("status == FINISH_STATE && m_codec.AvailableBytesIn != 0");
            }

            // Start a new block or continue the current one.
            if (this.m_codec.AvailableBytesIn != 0 || this.lookahead != 0 || flush != FlushType.None && this.status != DeflateManager.FINISH_STATE)
            {
                BlockState bstate = this.DeflateFunction(flush);

                if (bstate == BlockState.FinishStarted || bstate == BlockState.FinishDone)
                {
                    this.status = DeflateManager.FINISH_STATE;
                }

                if (bstate == BlockState.NeedMore || bstate == BlockState.FinishStarted)
                {
                    if (this.m_codec.AvailableBytesOut == 0)
                    {
                        this.last_flush = -1; // avoid BUF_ERROR next call, see above
                    }

                    return ZLibConstants.Z_OK;
                    // If flush != Z_NO_FLUSH && avail_out == 0, the next call
                    // of deflate should use the same flush parameter to make sure
                    // that the flush is complete. So we don't have to output an
                    // empty block here, this will be done at next call. This also
                    // ensures that for a very small output buffer, we emit at most
                    // one empty block.
                }

                if (bstate == BlockState.BlockDone)
                {
                    if (flush == FlushType.Partial)
                    {
                        this.m_tr_align();
                    }
                    else
                    {
                        // FlushType.Full or FlushType.Sync
                        this.m_tr_stored_block(0, 0, false);
                        // For a full flush, this empty block will be recognized
                        // as a special marker by inflate_sync().
                        if (flush == FlushType.Full)
                        {
                            // clear hash (forget the history)
                            for (int i = 0; i < this.hash_size; i++)
                            {
                                this.head[i] = 0;
                            }
                        }
                    }

                    this.m_codec.flush_pending();
                    if (this.m_codec.AvailableBytesOut == 0)
                    {
                        this.last_flush = -1; // avoid BUF_ERROR at next call, see above
                        return ZLibConstants.Z_OK;
                    }
                }
            }

            if (flush != FlushType.Finish)
            {
                return ZLibConstants.Z_OK;
            }

            if (!this.WantRfc1950HeaderBytes || this.Rfc1950BytesEmitted)
            {
                return ZLibConstants.Z_STREAM_END;
            }

            // Write the zlib trailer (adler32)
            this.pending[this.pendingCount++] = (byte) ((this.m_codec.m_Adler32 & 0xFF000000) >> 24);
            this.pending[this.pendingCount++] = (byte) ((this.m_codec.m_Adler32 & 0x00FF0000) >> 16);
            this.pending[this.pendingCount++] = (byte) ((this.m_codec.m_Adler32 & 0x0000FF00) >> 8);
            this.pending[this.pendingCount++] = (byte) (this.m_codec.m_Adler32 & 0x000000FF);
            //putShortMSB((int)(SharedUtils.URShift(_codec.m_Adler32, 16)));
            //putShortMSB((int)(_codec.m_Adler32 & 0xffff));

            this.m_codec.flush_pending();

            // If avail_out is zero, the application will call deflate again
            // to flush the rest.

            this.Rfc1950BytesEmitted = true; // write the trailer only once!

            return this.pendingCount != 0 ? ZLibConstants.Z_OK : ZLibConstants.Z_STREAM_END;
        }
    }
}