namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceChallengeSpectatorCountMessage : ServerAccountMessage
    {
        public LogicLong StreamId { get; set; }
        public int Count { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.StreamId);
            stream.WriteVInt(this.Count);
        }

        public override void Decode(ByteStream stream)
        {
            this.StreamId = stream.ReadLong();
            this.Count = stream.ReadVInt();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.ALLIANCE_CHALLENGE_SPECTATOR_COUNT;
        }
    }
}