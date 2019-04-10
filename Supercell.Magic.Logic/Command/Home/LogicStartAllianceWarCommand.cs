namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicStartAllianceWarCommand : LogicCommand
    {
        private LogicArrayList<LogicLong> m_excludeMemberList;

        public LogicStartAllianceWarCommand()
        {
            this.m_excludeMemberList = new LogicArrayList<LogicLong>();
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            int count = stream.ReadInt();

            if (count > 0)
            {
                this.m_excludeMemberList = new LogicArrayList<LogicLong>();
                this.m_excludeMemberList.EnsureCapacity(count);

                if (count > 50)
                {
                    Debugger.Error(string.Format("Number of excluded players ({0}) is too high.", count));
                }

                for (int i = 0; i < count; i++)
                {
                    this.m_excludeMemberList.Add(stream.ReadLong());
                }
            }
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);

            if (this.m_excludeMemberList != null)
            {
                encoder.WriteInt(this.m_excludeMemberList.Size());

                for (int i = 0; i < this.m_excludeMemberList.Size(); i++)
                {
                    encoder.WriteLong(this.m_excludeMemberList[i]);
                }
            }
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.START_ALLIANCE_WAR;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_excludeMemberList = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_excludeMemberList == null || this.m_excludeMemberList.Size() <= LogicDataTables.GetGlobals().GetWarMaxExcludeMembers())
            {
                LogicAvatar homeOwnerAvatar = level.GetHomeOwnerAvatar();

                if (homeOwnerAvatar.IsInAlliance())
                {
                    if (homeOwnerAvatar.GetAllianceRole() == LogicAvatarAllianceRole.LEADER || homeOwnerAvatar.GetAllianceRole() == LogicAvatarAllianceRole.CO_LEADER)
                    {
                        homeOwnerAvatar.GetChangeListener().StartWar(this.m_excludeMemberList);
                        return 0;
                    }

                    return -3;
                }

                return -2;
            }

            return -1;
        }
    }
}