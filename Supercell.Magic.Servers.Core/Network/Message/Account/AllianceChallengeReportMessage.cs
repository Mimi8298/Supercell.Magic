namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceChallengeReportMessage : ServerAccountMessage
    {
        public LogicLong StreamId { get; set; }
        public LogicLong ReplayId { get; set; }
        public string BattleLog { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.StreamId);

            if (this.ReplayId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.ReplayId);
            }
            else
            {
                stream.WriteBoolean(false);
            }

            stream.WriteString(this.BattleLog);
        }

        public override void Decode(ByteStream stream)
        {
            this.StreamId = stream.ReadLong();

            if (stream.ReadBoolean())
            {
                this.ReplayId = stream.ReadLong();
            }

            this.BattleLog = stream.ReadString(900000);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.ALLIANCE_CHALLENGE_REPORT;
        }
    }
}