namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public class LogicAllianceUnitReceivedCommand : LogicServerCommand
    {
        private LogicCombatItemData m_unitData;

        private int m_upgLevel;
        private string m_senderName;

        public void SetData(string senderName, LogicCombatItemData data, int upgLevel)
        {
            this.m_senderName = senderName;
            this.m_unitData = data;
            this.m_upgLevel = upgLevel;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_unitData = null;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_senderName = stream.ReadString(900000);
            this.m_unitData = (LogicCombatItemData) ByteStreamHelper.ReadDataReference(stream, stream.ReadInt() != 0 ? LogicDataType.SPELL : LogicDataType.CHARACTER);
            this.m_upgLevel = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteString(this.m_senderName);
            encoder.WriteInt(this.m_unitData.GetCombatItemType());
            ByteStreamHelper.WriteDataReference(encoder, this.m_unitData);
            encoder.WriteInt(this.m_upgLevel);

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                if (this.m_unitData != null)
                {
                    playerAvatar.AddAllianceUnit(this.m_unitData, this.m_upgLevel);
                    playerAvatar.GetChangeListener().AllianceUnitAdded(this.m_unitData, this.m_upgLevel);
                    level.GetGameListener().UnitReceivedFromAlliance(this.m_senderName, this.m_unitData, this.m_upgLevel);

                    if (level.GetState() == 1 || level.GetState() == 3)
                    {
                        level.GetComponentManagerAt(0).AddAvatarAllianceUnitsToCastle();
                    }

                    return 0;
                }
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.ALLIANCE_UNIT_RECEIVED;
        }
    }
}