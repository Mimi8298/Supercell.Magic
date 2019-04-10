namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class LeaveAllianceMemberMessage : ServerAccountMessage
    {
        public LogicLong MemberId { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.MemberId);
        }

        public override void Decode(ByteStream stream)
        {
            this.MemberId = stream.ReadLong();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.LEAVE_ALLIANCE_MEMBER;
        }
    }
}