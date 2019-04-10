namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Titan.DataStream;

    public class GameVisitState : GameState
    {
        public LogicClientAvatar HomeOwnerAvatar { get; set; }
        public int VisitType { get; set; }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);
            this.HomeOwnerAvatar.Encode(stream);

            stream.WriteVInt(this.VisitType);
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);
            this.HomeOwnerAvatar = new LogicClientAvatar();
            this.HomeOwnerAvatar.Decode(stream);

            this.VisitType = stream.ReadVInt();
        }

        public override GameStateType GetGameStateType()
        {
            return GameStateType.VISIT;
        }
    }
}