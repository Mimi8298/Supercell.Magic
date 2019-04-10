namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public class LogicAllianceExpEarnedCommand : LogicServerCommand
    {
        private int m_allianceExpLevel;
        private bool m_callListener;

        public LogicAllianceExpEarnedCommand()
        {
            // LogicAllianceExpEarnedCommand.
        }

        public LogicAllianceExpEarnedCommand(int expLevel)
        {
            this.m_allianceExpLevel = expLevel;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void Decode(ByteStream stream)
        {
            stream.ReadInt();
            stream.ReadInt();

            this.m_allianceExpLevel = stream.ReadInt();
            this.m_callListener = stream.ReadBoolean();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(0);
            encoder.WriteInt(0);

            encoder.WriteInt(this.m_allianceExpLevel);
            encoder.WriteBoolean(this.m_callListener);

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null && playerAvatar.IsInAlliance())
            {
                playerAvatar.SetAllianceLevel(this.m_allianceExpLevel);

                if (this.m_callListener)
                {
                    playerAvatar.GetChangeListener().AllianceLevelChanged(this.m_allianceExpLevel);
                }

                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.ALLIANCE_EXP_EARNED;
        }
    }
}