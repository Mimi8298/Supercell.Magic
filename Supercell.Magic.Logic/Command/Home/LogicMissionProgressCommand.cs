namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Mission;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicMissionProgressCommand : LogicCommand
    {
        private LogicMissionData m_missionData;

        public LogicMissionProgressCommand()
        {
            // LogicMissionProgressCommand.
        }

        public LogicMissionProgressCommand(LogicMissionData missionData)
        {
            this.m_missionData = missionData;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_missionData = (LogicMissionData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.MISSION);
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            ByteStreamHelper.WriteDataReference(encoder, this.m_missionData);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.MISSION_PROGRESS;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_missionData = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_missionData != null)
            {
                LogicMission mission = level.GetMissionManager().GetMissionByData(this.m_missionData);

                if (mission != null)
                {
                    mission.StateChangeConfirmed();
                    return 0;
                }

                return -2;
            }

            return -1;
        }
    }
}