namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public class ClientUpdateLiveReplayMessage : ServerAccountMessage
    {
        public int SubTick { get; set; }
        public LogicArrayList<LogicCommand> Commands { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.SubTick);

            if (this.Commands != null)
            {
                stream.WriteVInt(this.Commands.Size());

                for (int i = 0; i < this.Commands.Size(); i++)
                {
                    LogicCommandManager.EncodeCommand(stream, this.Commands[i]);
                }
            }
            else
            {
                stream.WriteVInt(-1);
            }
        }

        public override void Decode(ByteStream stream)
        {
            this.SubTick = stream.ReadVInt();

            int count = stream.ReadVInt();

            if (count >= 0)
            {
                this.Commands = new LogicArrayList<LogicCommand>(count);

                for (int i = 0; i < count; i++)
                {
                    this.Commands.Add(LogicCommandManager.DecodeCommand(stream));
                }
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.CLIENT_UPDATE_LIVE_REPLAY;
        }
    }
}