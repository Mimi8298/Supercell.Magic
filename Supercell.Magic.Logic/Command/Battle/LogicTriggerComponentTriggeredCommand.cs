namespace Supercell.Magic.Logic.Command.Battle
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.GameObject.Listener;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;

    public sealed class LogicTriggerComponentTriggeredCommand : LogicCommand
    {
        private int m_id;

        private LogicJSONObject m_json;
        private LogicGameObjectData m_data;

        public LogicTriggerComponentTriggeredCommand()
        {
            this.m_json = new LogicJSONObject();
        }

        public LogicTriggerComponentTriggeredCommand(LogicGameObject gameObject) : this()
        {
            this.m_id = gameObject.GetGlobalID();
            this.m_data = gameObject.GetData();

            gameObject.Save(this.m_json, 0);
        }

        public override void Decode(ByteStream stream)
        {
            this.m_id = stream.ReadVInt();
            this.m_data = (LogicGameObjectData) LogicDataTables.GetDataById(stream.ReadVInt());
            this.m_json = LogicJSONParser.ParseObject(stream.ReadString(900000) ?? string.Empty);

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteVInt(this.m_id);
            encoder.WriteVInt(this.m_data.GetGlobalID());
            encoder.WriteString(LogicJSONParser.CreateJSONString(this.m_json));

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.TRIGGER_COMPONENT_TRIGGERED;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_data = null;
            this.m_json = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (level != null)
            {
                LogicGameObject gameObject;

                if (level.GetState() == 5)
                {
                    gameObject = LogicGameObjectFactory.CreateGameObject(this.m_data, level, level.GetVillageType());
                    gameObject.Load(this.m_json);
                    level.GetGameObjectManager().AddGameObject(gameObject, -1);
                }
                else
                {
                    gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_id);
                }

                if (gameObject != null)
                {
                    if (gameObject.GetGameObjectType() == LogicGameObjectType.TRAP)
                    {
                        LogicTrap trap = (LogicTrap) gameObject;
                        LogicGameObjectManagerListener listener = level.GetGameObjectManager().GetListener();

                        listener.AddGameObject(gameObject);

                        gameObject.LoadingFinished();
                        gameObject.GetListener().RefreshState();

                        LogicTriggerComponent triggerComponent = trap.GetTriggerComponent();

                        if (triggerComponent != null)
                        {
                            triggerComponent.SetTriggered();
                        }
                    }

                    return 0;
                }

                Debugger.Warning("PGO == NULL in LogicTriggerComponentTriggeredCommand");
                return -2;
            }

            return -1;
        }

        public override void LoadFromJSON(LogicJSONObject jsonRoot)
        {
            LogicJSONObject baseObject = jsonRoot.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("Replay LogicTriggerComponentTriggeredCommand load failed! Base missing!");
            }

            base.LoadFromJSON(baseObject);

            this.m_id = jsonRoot.GetJSONNumber("id").GetIntValue();
            this.m_data = (LogicGameObjectData) LogicDataTables.GetDataById(jsonRoot.GetJSONNumber("dataid").GetIntValue());
            this.m_json = jsonRoot.GetJSONObject("objs");
        }

        public override LogicJSONObject GetJSONForReplay()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("base", base.GetJSONForReplay());
            jsonObject.Put("id", new LogicJSONNumber(this.m_id));
            jsonObject.Put("dataid", new LogicJSONNumber(this.m_data.GetGlobalID()));
            jsonObject.Put("objs", this.m_json);

            return jsonObject;
        }
    }
}