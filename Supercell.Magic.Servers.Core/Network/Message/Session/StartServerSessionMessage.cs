namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class StartServerSessionMessage : ServerSessionMessage
    {
        public LogicLong AccountId { get; set; }
        public string Country { get; set; }
        
        public LogicArrayList<int> ServerSocketTypeList { get; set; }
        public LogicArrayList<int> ServerSocketIdList { get; set; }
        public BindServerSocketRequestMessage BindRequestMessage { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.AccountId);
            stream.WriteString(this.Country);
            stream.WriteVInt(this.ServerSocketTypeList.Size());

            for (int i = 0; i < this.ServerSocketTypeList.Size(); i++)
            {
                stream.WriteVInt(this.ServerSocketTypeList[i]);
                stream.WriteVInt(this.ServerSocketIdList[i]);
            }

            if (this.BindRequestMessage != null)
            {
                stream.WriteBoolean(true);

                this.BindRequestMessage.EncodeHeader(stream);
                this.BindRequestMessage.Encode(stream);
            }
            else
            {
                stream.WriteBoolean(false);
            }
        }

        public override void Decode(ByteStream stream)
        {
            this.AccountId = stream.ReadLong();
            this.Country = stream.ReadString(900000);

            this.ServerSocketTypeList = new LogicArrayList<int>();
            this.ServerSocketIdList = new LogicArrayList<int>();

            for (int i = stream.ReadVInt(); i > 0; i--)
            {
                this.ServerSocketTypeList.Add(stream.ReadVInt());
                this.ServerSocketIdList.Add(stream.ReadVInt());
            }

            if (stream.ReadBoolean())
            {
                this.BindRequestMessage = new BindServerSocketRequestMessage();
                this.BindRequestMessage.DecodeHeader(stream);
                this.BindRequestMessage.Decode(stream);
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.START_SERVER_SESSION;
        }
    }
}