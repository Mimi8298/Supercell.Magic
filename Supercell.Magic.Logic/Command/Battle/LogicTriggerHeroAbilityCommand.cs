namespace Supercell.Magic.Logic.Command.Battle
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicTriggerHeroAbilityCommand : LogicCommand
    {
        private LogicHeroData m_data;

        public override void Decode(ByteStream stream)
        {
            this.m_data = (LogicHeroData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.HERO);
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            ByteStreamHelper.WriteDataReference(encoder, this.m_data);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.TRIGGER_HERO_ABILITY;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_data = null;
        }

        public override int Execute(LogicLevel level)
        {
            LogicArrayList<LogicGameObject> gameObjects = level.GetGameObjectManager().GetGameObjects(LogicGameObjectType.CHARACTER);

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicCharacter character = (LogicCharacter) gameObjects[i];

                if (character.GetHitpointComponent().GetTeam() == 0 && character.IsHero() && character.GetData() == this.m_data && character.GetHitpointComponent().GetHitpoints() > 0)
                {
                    if (this.m_data.HasAbility(character.GetUpgradeLevel()))
                    {
                        if (!this.m_data.HasOnceAbility() && character.GetAbilityCooldown() == 0 || this.m_data.HasOnceAbility() && !character.IsAbilityUsed())
                        {
                            character.StartAbility();
                        }
                    }
                }
            }

            return 0;
        }

        public override void LoadFromJSON(LogicJSONObject jsonRoot)
        {
            LogicJSONObject baseObject = jsonRoot.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("Replay LogicTriggerHeroAbility load failed! Base missing!");
            }

            base.LoadFromJSON(baseObject);

            LogicJSONNumber dataNumber = jsonRoot.GetJSONNumber("d");

            if (dataNumber != null)
            {
                this.m_data = (LogicHeroData) LogicDataTables.GetDataById(dataNumber.GetIntValue(), LogicDataType.HERO);
            }

            if (this.m_data == null)
            {
                Debugger.Error("Replay LogicTriggerHeroAbility load failed! Hero is NULL!");
            }
        }

        public override LogicJSONObject GetJSONForReplay()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("base", base.GetJSONForReplay());

            if (this.m_data != null)
            {
                jsonObject.Put("d", new LogicJSONNumber(this.m_data.GetGlobalID()));
            }

            return jsonObject;
        }
    }
}