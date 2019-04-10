namespace Supercell.Magic.Servers.Core.Network.Message.Core
{
    using Supercell.Magic.Titan.DataStream;

    public class GameLogMessage : ServerCoreMessage
    {
        public int LogType { get; set; }
        public string Message { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.LogType);
            stream.WriteString(this.Message);
        }

        public override void Decode(ByteStream stream)
        {
            this.LogType = stream.ReadVInt();
            this.Message = stream.ReadString(900000);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_LOG;
        }
    }
}