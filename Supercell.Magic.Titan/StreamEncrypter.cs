namespace Supercell.Magic.Titan
{
    public class StreamEncrypter
    {
        public virtual int Decrypt(byte[] input, byte[] output, int length)
        {
            return 0;
        }

        public virtual int Encrypt(byte[] input, byte[] output, int length)
        {
            return 0;
        }

        public virtual int GetOverheadEncryption()
        {
            return 0;
        }

        public virtual void Destruct()
        {
            // Destruct.
        }
    }
}