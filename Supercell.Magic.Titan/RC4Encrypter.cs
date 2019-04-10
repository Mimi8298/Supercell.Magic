namespace Supercell.Magic.Titan
{
    public class RC4Encrypter : StreamEncrypter
    {
        private byte[] m_key;
        private byte m_x;
        private byte m_y;

        public RC4Encrypter(string baseKey, string nonce)
        {
            this.InitState(baseKey, nonce);
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_key = null;
            this.m_x = 0;
            this.m_y = 0;
        }

        public void InitState(string baseKey, string nonce)
        {
            string key = baseKey + nonce;

            this.m_key = new byte[256];
            this.m_x = 0;
            this.m_y = 0;

            for (int i = 0; i < 256; i++)
            {
                this.m_key[i] = (byte) i;
            }

            for (int i = 0, j = 0; i < 256; i++)
            {
                j = (byte) (j + this.m_key[i] + key[i % key.Length]);

                byte tmpSwap = this.m_key[i];

                this.m_key[i] = this.m_key[j];
                this.m_key[j] = tmpSwap;
            }

            for (int i = 0; i < key.Length; i++)
            {
                this.m_x += 1;
                this.m_y += this.m_key[this.m_x];

                byte tmpSwap = this.m_key[this.m_y];

                this.m_key[this.m_y] = this.m_key[this.m_x];
                this.m_key[this.m_x] = tmpSwap;
            }
        }

        public override int Decrypt(byte[] input, byte[] output, int length)
        {
            return this.Encrypt(input, output, length);
        }

        public override int Encrypt(byte[] input, byte[] output, int length)
        {
            for (int i = 0; i < length; i++)
            {
                this.m_x += 1;
                this.m_y += this.m_key[this.m_x];

                byte tmpSwap = this.m_key[this.m_y];

                this.m_key[this.m_y] = this.m_key[this.m_x];
                this.m_key[this.m_x] = tmpSwap;

                output[i] = (byte) (input[i] ^ this.m_key[(byte) (this.m_key[this.m_x] + this.m_key[this.m_y])]);
            }

            return 0;
        }
    }
}