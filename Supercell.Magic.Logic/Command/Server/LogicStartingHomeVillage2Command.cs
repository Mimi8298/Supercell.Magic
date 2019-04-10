namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;

    public class LogicStartingHomeVillage2Command : LogicServerCommand
    {
        private byte[] m_compressedHomeJSON;

        public override void Destruct()
        {
            base.Destruct();
            this.m_compressedHomeJSON = null;
        }

        public override void Decode(ByteStream stream)
        {
            if (stream.ReadBoolean())
            {
                this.m_compressedHomeJSON = stream.ReadBytes(stream.ReadBytesLength(), 900000);
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            if (this.m_compressedHomeJSON != null)
            {
                encoder.WriteBoolean(true);
                encoder.WriteBytes(this.m_compressedHomeJSON, this.m_compressedHomeJSON.Length);
            }
            else
            {
                encoder.WriteBoolean(false);
            }

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                LogicGameObjectManager gameObjectManager = level.GetGameObjectManagerAt(1);

                if (gameObjectManager.GetTownHall() == null)
                {
                    LogicJSONObject jsonObject = level.GetGameListener().ParseCompressedHomeJSON(this.m_compressedHomeJSON, this.m_compressedHomeJSON.Length);

                    level.SetLoadingVillageType(1);

                    this.LoadGameObjectsJsonArray(level, jsonObject.GetJSONArray("buildings2"), 1);
                    this.LoadGameObjectsJsonArray(level, jsonObject.GetJSONArray("obstacles2"), 1);
                    this.LoadGameObjectsJsonArray(level, jsonObject.GetJSONArray("traps2"), 1);
                    this.LoadGameObjectsJsonArray(level, jsonObject.GetJSONArray("decos2"), 1);

                    level.SetLoadingVillageType(-1);

                    if (playerAvatar.GetResourceCount(LogicDataTables.GetGold2Data()) == 0)
                    {
                        playerAvatar.CommodityCountChangeHelper(0, LogicDataTables.GetGold2Data(), LogicDataTables.GetGlobals().GetStartingGold2());
                    }

                    if (playerAvatar.GetResourceCount(LogicDataTables.GetElixir2Data()) == 0)
                    {
                        playerAvatar.CommodityCountChangeHelper(0, LogicDataTables.GetElixir2Data(), LogicDataTables.GetGlobals().GetStartingElixir2());
                    }
                }

                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.STARTING_HOME_VILLAGE2;
        }

        public void SetData(byte[] compressedHomeJSON)
        {
            this.m_compressedHomeJSON = compressedHomeJSON;
        }

        public void LoadGameObjectsJsonArray(LogicLevel level, LogicJSONArray array, int villageType)
        {
            if (array != null)
            {
                for (int i = 0; i < array.Size(); i++)
                {
                    LogicJSONObject jsonObject = array.GetJSONObject(i);

                    if (jsonObject != null)
                    {
                        LogicGameObjectData data = (LogicGameObjectData) LogicDataTables.GetDataById(jsonObject.GetJSONNumber("data").GetIntValue());

                        if (data != null)
                        {
                            LogicGameObject gameObject = LogicGameObjectFactory.CreateGameObject(data, level, villageType);
                            gameObject.Load(jsonObject);
                            level.GetGameObjectManagerAt(1).AddGameObject(gameObject, -1);
                        }
                    }
                }
            }
        }
    }
}