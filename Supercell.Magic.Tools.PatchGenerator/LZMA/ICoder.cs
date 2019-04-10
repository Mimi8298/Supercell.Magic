// ICoder.h

namespace SevenZip
{
    using System;
    using System.IO;

    internal class DataErrorException : ApplicationException
    {
        public DataErrorException() : base("Data Error")
        {
        }
    }

    internal class InvalidParamException : ApplicationException
    {
        public InvalidParamException() : base("Invalid Parameter")
        {
        }
    }

    public interface ICodeProgress
    {
        void SetProgress(long inSize, long outSize);
    }

    public interface ICoder
    {
        void Code(Stream inStream, Stream outStream,
                  long inSize, long outSize, ICodeProgress progress);
    }

    /*
    public interface ICoder2
    {
         void Code(ISequentialInStream []inStreams,
                const UInt64 []inSizes, 
                ISequentialOutStream []outStreams, 
                UInt64 []outSizes,
                ICodeProgress progress);
    };
  */

    public enum CoderPropID
    {
        DefaultProp = 0,

        DictionarySize,

        UsedMemorySize,

        Order,

        BlockSize,

        PosStateBits,

        LitContextBits,

        LitPosBits,

        NumFastBytes,

        MatchFinder,

        MatchFinderCycles,

        NumPasses,

        Algorithm,

        NumThreads,

        EndMarker
    }


    public interface ISetCoderProperties
    {
        void SetCoderProperties(CoderPropID[] propIDs, object[] properties);
    }

    public interface IWriteCoderProperties
    {
        void WriteCoderProperties(Stream outStream);
    }

    public interface ISetDecoderProperties
    {
        void SetDecoderProperties(byte[] properties);
    }
}