namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceChallengeRequestMessage : ServerAccountMessage
    {
        public LogicLong MemberId { get; set; }
        public string Message { get; set; }
        public byte[] HomeJSON { get; set; }
        public bool WarLayout { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.MemberId);
            stream.WriteString(this.Message);
            stream.WriteBytes(this.HomeJSON, this.HomeJSON.Length);
            stream.WriteBoolean(this.WarLayout);
        }

        public override void Decode(ByteStream stream)
        {
            this.MemberId = stream.ReadLong();
            this.Message = stream.ReadString(900000);
            this.HomeJSON = stream.ReadBytes(stream.ReadBytesLength(), 900000);
            this.WarLayout = stream.ReadBoolean();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.ALLIANCE_CHALLENGE_REQUEST;
        }
    }
}