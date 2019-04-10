namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class GameMatchedAttackState : GameState
    {
        public LogicLong LiveReplayId { get; set; }
        public LogicClientAvatar HomeOwnerAvatar { get; set; }
        public int MaintenanceTime { get; set; }
        public bool GameDefenderLocked { get; set; }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);
            this.HomeOwnerAvatar.Encode(stream);

            stream.WriteVInt(this.MaintenanceTime);
            stream.WriteBoolean(this.GameDefenderLocked);
            stream.WriteLong(this.LiveReplayId);
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);
            this.HomeOwnerAvatar = new LogicClientAvatar();
            this.HomeOwnerAvatar.Decode(stream);
            this.MaintenanceTime = stream.ReadVInt();
            this.GameDefenderLocked = stream.ReadBoolean();
            this.LiveReplayId = stream.ReadLong();
        }

        public override GameStateType GetGameStateType()
        {
            return GameStateType.MATCHED_ATTACK;
        }
    }
}