namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Titan.DataStream;

    public abstract class GameState
    {
        public LogicClientAvatar PlayerAvatar { get; set; }
        public LogicClientHome Home { get; set; }

        public int SaveTime { get; set; } = -1;

        public virtual void Encode(ByteStream stream)
        {
            this.PlayerAvatar.Encode(stream);
            this.Home.Encode(stream);

            stream.WriteVInt(this.SaveTime);
        }

        public virtual void Decode(ByteStream stream)
        {
            this.PlayerAvatar = new LogicClientAvatar();
            this.PlayerAvatar.Decode(stream);
            this.Home = new LogicClientHome();
            this.Home.Decode(stream);
            this.SaveTime = stream.ReadVInt();
        }

        public virtual SimulationServiceNodeType GetSimulationServiceNodeType()
        {
            return SimulationServiceNodeType.HOME;
        }

        public abstract GameStateType GetGameStateType();
    }

    public enum GameStateType
    {
        HOME,
        NPC_ATTACK,
        NPC_DUEL,
        MATCHED_ATTACK,
        CHALLENGE_ATTACK,
        FAKE_ATTACK,
        VISIT
    }

    public enum SimulationServiceNodeType
    {
        HOME = 10,
        BATTLE = 27
    }
}