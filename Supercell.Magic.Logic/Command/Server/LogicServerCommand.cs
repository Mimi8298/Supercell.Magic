namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Titan.DataStream;

    public class LogicServerCommand : LogicCommand
    {
        private int m_id;

        public LogicServerCommand()
        {
            this.m_id = -1;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_id = -1;
        }

        public int GetId()
        {
            return this.m_id;
        }

        public void SetId(int id)
        {
            this.m_id = id;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_id = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_id);
            base.Encode(encoder);
        }

        public sealed override bool IsServerCommand()
        {
            return true;
        }
    }
}