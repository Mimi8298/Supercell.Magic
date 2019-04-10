namespace Supercell.Magic.Logic.Avatar.Change
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;

    public class LogicAvatarChange
    {
        public virtual void Destruct()
        {
            // Destruct.
        }

        public virtual int GetAvatarChangeType()
        {
            Debugger.Error("LogicAvatarChange::getAvatarChangeType() must be overridden");
            return 0;
        }

        public virtual void Decode(ByteStream stream)
        {
            // Decode.
        }

        public virtual void Encode(ChecksumEncoder encoder)
        {
            // Encode.
        }

        public virtual void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            // ApplyAvatarChange.
        }
    }
}