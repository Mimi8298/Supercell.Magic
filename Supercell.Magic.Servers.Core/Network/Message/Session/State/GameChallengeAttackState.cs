namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class GameChallengeAttackState : GameState
    {
        public LogicLong LiveReplayId { get; set; }
        public LogicLong StreamId { get; set; }
        public LogicLong AllianceId { get; set; }

        public int MapId { get; set; }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteLong(this.LiveReplayId);
            stream.WriteLong(this.StreamId);
            stream.WriteLong(this.AllianceId);
            stream.WriteVInt(this.MapId);
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.LiveReplayId = stream.ReadLong();
            this.StreamId = stream.ReadLong();
            this.AllianceId = stream.ReadLong();
            this.MapId = stream.ReadVInt();
        }

        public override GameStateType GetGameStateType()
        {
            return GameStateType.CHALLENGE_ATTACK;
        }

        public override SimulationServiceNodeType GetSimulationServiceNodeType()
        {
            return SimulationServiceNodeType.BATTLE;
        }
    }
}