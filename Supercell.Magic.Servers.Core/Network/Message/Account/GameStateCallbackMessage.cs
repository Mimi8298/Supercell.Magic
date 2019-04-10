namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Command.Server;

    using Supercell.Magic.Servers.Core.Network.Message.Session.Change;

    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public class GameStateCallbackMessage : ServerAccountMessage
    {
        public LogicClientAvatar LogicClientAvatar { get; set; }
        public LogicArrayList<AvatarChange> AvatarChanges { get; set; }
        public LogicArrayList<LogicServerCommand> ExecutedServerCommands { get; set; }

        public int SaveTime { get; set; }
        public int RemainingShieldTime { get; set; }
        public int RemainingGuardTime { get; set; }
        public int NextPersonalBreakTime { get; set; }
        public byte[] HomeJSON { get; set; }
        
        public long SessionId { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLongLong(this.SessionId);
            stream.WriteVInt(this.AvatarChanges.Size());

            for (int i = 0; i < this.AvatarChanges.Size(); i++)
            {
                AvatarChangeFactory.Encode(stream, this.AvatarChanges[i]);
            }

            this.LogicClientAvatar.Encode(stream);
            
            if (this.HomeJSON != null)
            {
                stream.WriteBoolean(true);
                stream.WriteVInt(this.ExecutedServerCommands.Size());

                for (int i = 0; i < this.ExecutedServerCommands.Size(); i++)
                {
                    LogicCommandManager.EncodeCommand(stream, this.ExecutedServerCommands[i]);
                }

                stream.WriteVInt(this.SaveTime);
                stream.WriteVInt(this.RemainingShieldTime);
                stream.WriteVInt(this.RemainingGuardTime);
                stream.WriteVInt(this.NextPersonalBreakTime);
                stream.WriteBytes(this.HomeJSON, this.HomeJSON.Length);
            }
            else
            {
                stream.WriteBoolean(false);
            }
        }

        public override void Decode(ByteStream stream)
        {
            this.SessionId = stream.ReadLongLong();
            this.AvatarChanges = new LogicArrayList<AvatarChange>();

            for (int i = stream.ReadVInt(); i > 0; i--)
            {
                this.AvatarChanges.Add(AvatarChangeFactory.Decode(stream));
            }

            this.LogicClientAvatar = new LogicClientAvatar();
            this.LogicClientAvatar.Decode(stream);

            if (stream.ReadBoolean())
            {
                this.ExecutedServerCommands = new LogicArrayList<LogicServerCommand>();

                for (int i = stream.ReadVInt(); i > 0; i--)
                {
                    this.ExecutedServerCommands.Add((LogicServerCommand)LogicCommandManager.DecodeCommand(stream));
                }

                this.SaveTime = stream.ReadVInt();
                this.RemainingShieldTime = stream.ReadVInt();
                this.RemainingGuardTime = stream.ReadVInt();
                this.NextPersonalBreakTime = stream.ReadVInt();
                this.HomeJSON = stream.ReadBytes(stream.ReadBytesLength(), 900000);
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_STATE_CALLBACK;
        }
    }
}