namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public sealed class LogicShareReplayCommand : LogicCommand
    {
        private LogicLong m_battleEntryId;

        private bool m_duelReplay;
        private string m_message;

        public override void Decode(ByteStream stream)
        {
            this.m_battleEntryId = stream.ReadLong();
            this.m_duelReplay = stream.ReadBoolean();

            if (stream.ReadBoolean())
            {
                this.m_message = stream.ReadString(900000);
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteLong(this.m_battleEntryId);
            encoder.WriteBoolean(this.m_duelReplay);

            if (this.m_message != null)
            {
                encoder.WriteBoolean(true);
                encoder.WriteString(this.m_message);
            }
            else
            {
                encoder.WriteBoolean(false);
            }

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.SHARE_REPLAY;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_message = null;
            this.m_battleEntryId = null;
        }

        public override int Execute(LogicLevel level)
        {
            LogicBuilding allianceCastle = level.GetGameObjectManagerAt(0).GetAllianceCastle();

            if (allianceCastle != null)
            {
                LogicBunkerComponent bunkerComponent = allianceCastle.GetBunkerComponent();

                if (bunkerComponent != null)
                {
                    if (bunkerComponent.GetReplayShareCooldownTime() == 0)
                    {
                        bunkerComponent.StartReplayShareCooldownTime();

                        if (this.m_duelReplay)
                        {
                            level.GetGameListener().DuelReplayShared(this.m_battleEntryId);
                            level.GetHomeOwnerAvatar().GetChangeListener().ShareDuelReplay(this.m_battleEntryId, this.m_message);
                        }
                        else
                        {
                            level.GetHomeOwnerAvatar().GetChangeListener().ShareReplay(this.m_battleEntryId, this.m_message);
                        }

                        return 0;
                    }
                }
            }

            return -1;
        }
    }
}