namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Titan.DataStream;

    public class GameNpcDuelState : GameState
    {
        public LogicNpcAvatar NpcAvatar { get; set; }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);
            this.NpcAvatar.Encode(stream);
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);
            this.NpcAvatar = new LogicNpcAvatar();
            this.NpcAvatar.Decode(stream);
        }

        public override GameStateType GetGameStateType()
        {
            return GameStateType.NPC_DUEL;
        }
    }
}