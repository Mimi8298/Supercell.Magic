namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Servers.Core.Network.Message.Session.Change;

    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class AllianceAvatarChangesMessage : ServerAccountMessage
    {
        public LogicLong MemberId { get; set; }
        public LogicArrayList<AvatarChange> AvatarChanges { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.MemberId);
            stream.WriteVInt(this.AvatarChanges.Size());

            for (int i = 0; i < this.AvatarChanges.Size(); i++)
                AvatarChangeFactory.Encode(stream, this.AvatarChanges[i]);
        }

        public override void Decode(ByteStream stream)
        {
            this.MemberId = stream.ReadLong();
            this.AvatarChanges = new LogicArrayList<AvatarChange>();

            for (int i = stream.ReadVInt(); i > 0; i--)
                this.AvatarChanges.Add(AvatarChangeFactory.Decode(stream));
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.ALLIANCE_AVATAR_CHANGES;
        }
    }
}