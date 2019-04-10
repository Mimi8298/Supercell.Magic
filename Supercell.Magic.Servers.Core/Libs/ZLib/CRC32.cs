// CRC32.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2011 Dino Chiesa.
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
// Last Saved: <2011-August-02 18:25:54>
//
// ------------------------------------------------------------------
//
// This module defines the CRC32 class, which can do the CRC32 algorithm, using
// arbitrary starting polynomials, and bit reversal. The bit reversal is what
// distinguishes this CRC-32 used in BZip2 from the CRC-32 that is used in PKZIP
// files, or GZIP files. This class does both.
//
// ------------------------------------------------------------------


using Interop = System.Runtime.InteropServices;

namespace Supercell.Magic.Servers.Core.Libs.ZLib
{
    using System;
    using System.IO;

    [Interop.GuidAttribute("ebc25cf6-9120-4283-b972-0e5520d0000C"), Interop.ComVisibleAttribute(true), Interop.ClassInterfaceAttribute(Interop.ClassInterfaceType.AutoDispatch)]
#if !NETCF
#endif
    public class CRC32
    {
        public long TotalBytesRead { get; private set; }

        public int Crc32Result
        {
            get
            {
                return unchecked((int) ~this.m_register);
            }
        }

        public int GetCrc32(Stream input)
        {
            return this.GetCrc32AndCopy(input, null);
        }

        public int GetCrc32AndCopy(Stream input, Stream output)
        {
            if (input == null)
            {
                throw new Exception("The input stream must not be null.");
            }

            unchecked
            {
                byte[] buffer = new byte[CRC32.BUFFER_SIZE];
                int readSize = CRC32.BUFFER_SIZE;

                this.TotalBytesRead = 0;
                int count = input.Read(buffer, 0, readSize);
                if (output != null)
                {
                    output.Write(buffer, 0, count);
                }

                this.TotalBytesRead += count;
                while (count > 0)
                {
                    this.SlurpBlock(buffer, 0, count);
                    count = input.Read(buffer, 0, readSize);
                    if (output != null)
                    {
                        output.Write(buffer, 0, count);
                    }

                    this.TotalBytesRead += count;
                }

                return (int) ~this.m_register;
            }
        }


        public int ComputeCrc32(int W, byte B)
        {
            return this.m_InternalComputeCrc32((uint) W, B);
        }

        internal int m_InternalComputeCrc32(uint W, byte B)
        {
            return (int) (this.crc32Table[(W ^ B) & 0xFF] ^ (W >> 8));
        }


        public void SlurpBlock(byte[] block, int offset, int count)
        {
            if (block == null)
            {
                throw new Exception("The data buffer must not be null.");
            }

            // bzip algorithm
            for (int i = 0; i < count; i++)
            {
                int x = offset + i;
                byte b = block[x];
                if (this.reverseBits)
                {
                    uint temp = (this.m_register >> 24) ^ b;
                    this.m_register = (this.m_register << 8) ^ this.crc32Table[temp];
                }
                else
                {
                    uint temp = (this.m_register & 0x000000FF) ^ b;
                    this.m_register = (this.m_register >> 8) ^ this.crc32Table[temp];
                }
            }

            this.TotalBytesRead += count;
        }


        public void UpdateCRC(byte b)
        {
            if (this.reverseBits)
            {
                uint temp = (this.m_register >> 24) ^ b;
                this.m_register = (this.m_register << 8) ^ this.crc32Table[temp];
            }
            else
            {
                uint temp = (this.m_register & 0x000000FF) ^ b;
                this.m_register = (this.m_register >> 8) ^ this.crc32Table[temp];
            }
        }

        public void UpdateCRC(byte b, int n)
        {
            while (n-- > 0)
            {
                if (this.reverseBits)
                {
                    uint temp = (this.m_register >> 24) ^ b;
                    this.m_register = (this.m_register << 8) ^ this.crc32Table[temp >= 0
                                                                                 ? temp
                                                                                 : temp + 256];
                }
                else
                {
                    uint temp = (this.m_register & 0x000000FF) ^ b;
                    this.m_register = (this.m_register >> 8) ^ this.crc32Table[temp >= 0
                                                                                 ? temp
                                                                                 : temp + 256];
                }
            }
        }


        private static uint ReverseBits(uint data)
        {
            unchecked
            {
                uint ret = data;
                ret = ((ret & 0x55555555) << 1) | ((ret >> 1) & 0x55555555);
                ret = ((ret & 0x33333333) << 2) | ((ret >> 2) & 0x33333333);
                ret = ((ret & 0x0F0F0F0F) << 4) | ((ret >> 4) & 0x0F0F0F0F);
                ret = (ret << 24) | ((ret & 0xFF00) << 8) | ((ret >> 8) & 0xFF00) | (ret >> 24);
                return ret;
            }
        }

        private static byte ReverseBits(byte data)
        {
            unchecked
            {
                uint u = (uint) data * 0x00020202;
                uint m = 0x01044010;
                uint s = u & m;
                uint t = (u << 2) & (m << 1);
                return (byte) ((0x01001001 * (s + t)) >> 24);
            }
        }


        private void GenerateLookupTable()
        {
            this.crc32Table = new uint[256];
            unchecked
            {
                uint dwCrc;
                byte i = 0;
                do
                {
                    dwCrc = i;
                    for (byte j = 8; j > 0; j--)
                    {
                        if ((dwCrc & 1) == 1)
                        {
                            dwCrc = (dwCrc >> 1) ^ this.dwPolynomial;
                        }
                        else
                        {
                            dwCrc >>= 1;
                        }
                    }

                    if (this.reverseBits)
                    {
                        this.crc32Table[CRC32.ReverseBits(i)] = CRC32.ReverseBits(dwCrc);
                    }
                    else
                    {
                        this.crc32Table[i] = dwCrc;
                    }

                    i++;
                } while (i != 0);
            }

        #if VERBOSE
            Console.WriteLine();
            Console.WriteLine("private static readonly UInt32[] crc32Table = {");
            for (int i = 0; i < crc32Table.Length; i += 4)
            {
                Console.Write("   ");
                for (int j = 0; j < 4; j++)
                {
                    Console.Write(" 0x{0:X8}U,", crc32Table[i+j]);
                }
                Console.WriteLine();
            }
            Console.WriteLine("};");
            Console.WriteLine();
            #endif
        }


        private uint gf2_matrix_times(uint[] matrix, uint vec)
        {
            uint sum = 0;
            int i = 0;
            while (vec != 0)
            {
                if ((vec & 0x01) == 0x01)
                {
                    sum ^= matrix[i];
                }

                vec >>= 1;
                i++;
            }

            return sum;
        }

        private void gf2_matrix_square(uint[] square, uint[] mat)
        {
            for (int i = 0; i < 32; i++)
            {
                square[i] = this.gf2_matrix_times(mat, mat[i]);
            }
        }


        public void Combine(int crc, int length)
        {
            uint[] even = new uint[32]; // even-power-of-two zeros operator
            uint[] odd = new uint[32]; // odd-power-of-two zeros operator

            if (length == 0)
            {
                return;
            }

            uint crc1 = ~this.m_register;
            uint crc2 = (uint) crc;

            // put operator for one zero bit in odd
            odd[0] = this.dwPolynomial; // the CRC-32 polynomial
            uint row = 1;
            for (int i = 1; i < 32; i++)
            {
                odd[i] = row;
                row <<= 1;
            }

            // put operator for two zero bits in even
            this.gf2_matrix_square(even, odd);

            // put operator for four zero bits in odd
            this.gf2_matrix_square(odd, even);

            uint len2 = (uint) length;

            // apply len2 zeros to crc1 (first square will put the operator for one
            // zero byte, eight zero bits, in even)
            do
            {
                // apply zeros operator for this bit of len2
                this.gf2_matrix_square(even, odd);

                if ((len2 & 1) == 1)
                {
                    crc1 = this.gf2_matrix_times(even, crc1);
                }

                len2 >>= 1;

                if (len2 == 0)
                {
                    break;
                }

                // another iteration of the loop with odd and even swapped
                this.gf2_matrix_square(odd, even);
                if ((len2 & 1) == 1)
                {
                    crc1 = this.gf2_matrix_times(odd, crc1);
                }

                len2 >>= 1;
            } while (len2 != 0);

            crc1 ^= crc2;

            this.m_register = ~crc1;

            //return (int) crc1;
        }


        public CRC32() : this(false)
        {
        }

        public CRC32(bool reverseBits) :
            this(unchecked((int) 0xEDB88320), reverseBits)
        {
        }


        public CRC32(int polynomial, bool reverseBits)
        {
            this.reverseBits = reverseBits;
            this.dwPolynomial = (uint) polynomial;
            this.GenerateLookupTable();
        }

        public void Reset()
        {
            this.m_register = 0xFFFFFFFFU;
        }

        // private member vars
        private readonly uint dwPolynomial;
        private readonly bool reverseBits;
        private uint[] crc32Table;
        private const int BUFFER_SIZE = 8192;
        private uint m_register = 0xFFFFFFFFU;
    }


    public class CrcCalculatorStream : Stream, IDisposable
    {
        private static readonly long UnsetLengthLimit = -99;

        internal Stream m_innerStream;
        private readonly CRC32 m_Crc32;
        private readonly long m_lengthLimit = -99;

        public CrcCalculatorStream(Stream stream)
            : this(true, CrcCalculatorStream.UnsetLengthLimit, stream, null)
        {
        }

        public CrcCalculatorStream(Stream stream, bool leaveOpen)
            : this(leaveOpen, CrcCalculatorStream.UnsetLengthLimit, stream, null)
        {
        }

        public CrcCalculatorStream(Stream stream, long length)
            : this(true, length, stream, null)
        {
            if (length < 0)
            {
                throw new ArgumentException("length");
            }
        }

        public CrcCalculatorStream(Stream stream, long length, bool leaveOpen)
            : this(leaveOpen, length, stream, null)
        {
            if (length < 0)
            {
                throw new ArgumentException("length");
            }
        }

        public CrcCalculatorStream(Stream stream, long length, bool leaveOpen,
                                   CRC32 crc32)
            : this(leaveOpen, length, stream, crc32)
        {
            if (length < 0)
            {
                throw new ArgumentException("length");
            }
        }


        // This ctor is private - no validation is done here.  This is to allow the use
        // of a (specific) negative value for the m_lengthLimit, to indicate that there
        // is no length set.  So we validate the length limit in those ctors that use an
        // explicit param, otherwise we don't validate, because it could be our special
        // value.
        private CrcCalculatorStream
            (bool leaveOpen, long length, Stream stream, CRC32 crc32)
        {
            this.m_innerStream = stream;
            this.m_Crc32 = crc32 ?? new CRC32();
            this.m_lengthLimit = length;
            this.LeaveOpen = leaveOpen;
        }


        public long TotalBytesSlurped
        {
            get
            {
                return this.m_Crc32.TotalBytesRead;
            }
        }

        public int Crc
        {
            get
            {
                return this.m_Crc32.Crc32Result;
            }
        }

        public bool LeaveOpen { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesToRead = count;

            // Need to limit the # of bytes returned, if the stream is intended to have
            // a definite length.  This is especially useful when returning a stream for
            // the uncompressed data directly to the application.  The app won't
            // necessarily read only the UncompressedSize number of bytes.  For example
            // wrapping the stream returned from OpenReader() into a StreadReader() and
            // calling ReadToEnd() on it, We can "over-read" the zip data and get a
            // corrupt string.  The length limits that, prevents that problem.

            if (this.m_lengthLimit != CrcCalculatorStream.UnsetLengthLimit)
            {
                if (this.m_Crc32.TotalBytesRead >= this.m_lengthLimit)
                {
                    return 0; // EOF
                }

                long bytesRemaining = this.m_lengthLimit - this.m_Crc32.TotalBytesRead;
                if (bytesRemaining < count)
                {
                    bytesToRead = (int) bytesRemaining;
                }
            }

            int n = this.m_innerStream.Read(buffer, offset, bytesToRead);
            if (n > 0)
            {
                this.m_Crc32.SlurpBlock(buffer, offset, n);
            }

            return n;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count > 0)
            {
                this.m_Crc32.SlurpBlock(buffer, offset, count);
            }

            this.m_innerStream.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get
            {
                return this.m_innerStream.CanRead;
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
                return this.m_innerStream.CanWrite;
            }
        }

        public override void Flush()
        {
            this.m_innerStream.Flush();
        }

        public override long Length
        {
            get
            {
                if (this.m_lengthLimit == CrcCalculatorStream.UnsetLengthLimit)
                {
                    return this.m_innerStream.Length;
                }

                return this.m_lengthLimit;
            }
        }

        public override long Position
        {
            get
            {
                return this.m_Crc32.TotalBytesRead;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }


        void IDisposable.Dispose()
        {
            this.Close();
        }

        public override void Close()
        {
            base.Close();
            if (!this.LeaveOpen)
            {
                this.m_innerStream.Close();
            }
        }
    }
}