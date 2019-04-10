namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceChallengeLiveReplayIdMessage : ServerAccountMessage
    {
        public LogicLong LiveReplayId { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.LiveReplayId);
        }

        public override void Decode(ByteStream stream)
        {
            this.LiveReplayId = stream.ReadLong();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.ALLIANCE_CHALLENGE_LIVE_REPLAY_ID;
        }
    }
}