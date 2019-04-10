namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class GameStartFakeAttackMessage : ServerSessionMessage
    {
        public LogicLong AccountId { get; set; }
        public LogicData ArgData { get; set; }

        public override void Encode(ByteStream stream)
        {
            if (this.AccountId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.AccountId);
            }
            else
            {
                stream.WriteBoolean(false);
            }

            ByteStreamHelper.WriteDataReference(stream, this.ArgData);
        }

        public override void Decode(ByteStream stream)
        {
            if (stream.ReadBoolean())
            {
                this.AccountId = stream.ReadLong();
            }

            this.ArgData = ByteStreamHelper.ReadDataReference(stream);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_START_FAKE_ATTACK;
        }
    }
}