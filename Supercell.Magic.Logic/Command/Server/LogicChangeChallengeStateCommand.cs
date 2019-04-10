namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class LogicChangeChallengeStateCommand : LogicServerCommand
    {
        private LogicLong m_challengeId;
        private LogicLong m_argsId;

        private int m_challengeState;

        public override void Destruct()
        {
            base.Destruct();

            this.m_challengeId = null;
            this.m_argsId = null;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_challengeId = stream.ReadLong();
            this.m_challengeState = stream.ReadInt();

            if (stream.ReadBoolean())
            {
                this.m_argsId = stream.ReadLong();
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteLong(this.m_challengeId);
            encoder.WriteInt(this.m_challengeState);
            encoder.WriteBoolean(this.m_argsId != null);

            if (this.m_argsId != null)
            {
                encoder.WriteLong(this.m_argsId);
            }

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                playerAvatar.SetChallengeId(this.m_challengeId);
                playerAvatar.SetChallengeState(this.m_challengeState);

                level.GetGameListener().ChallengeStateChanged(this.m_challengeId, this.m_argsId, this.m_challengeState);

                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CHANGE_CHALLENGE_STATE;
        }
    }
}