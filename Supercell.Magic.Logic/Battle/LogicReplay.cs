namespace Supercell.Magic.Logic.Battle
{
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Json;

    public class LogicReplay
    {
        private readonly LogicLevel m_level;
        private LogicJSONObject m_replayObject;
        private LogicJSONNumber m_endTickNumber;
        private LogicJSONNumber m_preparationSkipNumber;

        public LogicReplay(LogicLevel level)
        {
            this.m_level = level;
            this.m_replayObject = new LogicJSONObject();
            this.StartRecord();
        }

        public void Destruct()
        {
            this.m_replayObject = null;
            this.m_endTickNumber = null;
            this.m_preparationSkipNumber = null;
        }

        public void StartRecord()
        {
            this.m_replayObject = new LogicJSONObject();
            this.m_endTickNumber = new LogicJSONNumber();

            LogicJSONObject levelObject = new LogicJSONObject();
            LogicJSONObject visitorObject = new LogicJSONObject();
            LogicJSONObject homeOwnerAvatarObject = new LogicJSONObject();

            this.m_level.SaveToJSON(levelObject);
            this.m_level.GetVisitorAvatar().SaveToReplay(visitorObject);
            this.m_level.GetHomeOwnerAvatar().SaveToReplay(homeOwnerAvatarObject);

            this.m_replayObject.Put("level", levelObject);
            this.m_replayObject.Put("attacker", visitorObject);
            this.m_replayObject.Put("defender", homeOwnerAvatarObject);
            this.m_replayObject.Put("end_tick", this.m_endTickNumber);
            this.m_replayObject.Put("timestamp", new LogicJSONNumber(this.m_level.GetGameMode().GetStartTime()));
            this.m_replayObject.Put("cmd", new LogicJSONArray());
            this.m_replayObject.Put("calendar", this.m_level.GetCalendar().Save());

            if (this.m_level.GetGameMode().GetConfiguration().GetJson() != null)
            {
                this.m_replayObject.Put("globals", this.m_level.GetGameMode().GetConfiguration().GetJson());
            }
        }

        public void SubTick()
        {
            this.m_endTickNumber.SetIntValue(this.m_level.GetLogicTime().GetTick() + 1);
        }

        public void RecordCommand(LogicCommand command)
        {
            LogicJSONArray commandArray = this.m_replayObject.GetJSONArray("cmd");
            LogicJSONObject commandObject = new LogicJSONObject();
            LogicCommandManager.SaveCommandToJSON(commandObject, command);

            commandArray.Add(commandObject);
        }

        public void RecordPreparationSkipTime(int secs)
        {
            if (secs > 0)
            {
                if (this.m_preparationSkipNumber == null)
                {
                    this.m_preparationSkipNumber = new LogicJSONNumber();
                    this.m_replayObject.Put("prep_skip", this.m_preparationSkipNumber);
                }

                this.m_preparationSkipNumber.SetIntValue(secs);
            }
        }

        public LogicJSONObject GetJson()
        {
            return this.m_replayObject;
        }
    }
}