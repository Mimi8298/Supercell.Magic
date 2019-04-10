namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Titan.DataStream;

    public class GameFakeAttackState : GameState
    {
        public LogicClientAvatar HomeOwnerAvatar { get; set; }
        public int MaintenanceTime { get; set; }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);
            this.HomeOwnerAvatar.Encode(stream);
            stream.WriteVInt(this.MaintenanceTime);
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);
            this.HomeOwnerAvatar = new LogicClientAvatar();
            this.HomeOwnerAvatar.Decode(stream);
            this.MaintenanceTime = stream.ReadVInt();
        }

        public override GameStateType GetGameStateType()
        {
            return GameStateType.FAKE_ATTACK;
        }

        public override SimulationServiceNodeType GetSimulationServiceNodeType()
        {
            return SimulationServiceNodeType.BATTLE;
        }
    }
}