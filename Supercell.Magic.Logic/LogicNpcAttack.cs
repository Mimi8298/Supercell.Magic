namespace Supercell.Magic.Logic
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Command.Battle;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class LogicNpcAttack
    {
        private LogicLevel m_level;
        private LogicNpcAvatar m_npcAvatar;
        private LogicBuildingClassData m_buildingClass;

        private bool m_unitsDeployed;
        private bool m_unitsDeployStarted;

        private int m_placePositionX;
        private int m_placePositionY;
        private int m_nextUnit;

        public LogicNpcAttack(LogicLevel level)
        {
            this.m_placePositionX = -1;
            this.m_placePositionY = -1;
            this.m_level = level;
            this.m_npcAvatar = (LogicNpcAvatar) level.GetVisitorAvatar();
            this.m_buildingClass = LogicDataTables.GetBuildingClassByName("Defense", null);

            if (this.m_buildingClass == null)
            {
                Debugger.Error("LogicNpcAttack - Unable to find Defense building class");
            }
        }

        public void Destruct()
        {
            this.m_level = null;
            this.m_npcAvatar = null;
            this.m_buildingClass = null;
        }

        public bool PlaceOneUnit()
        {
            if (this.m_placePositionX == -1 && this.m_placePositionY == -1)
            {
                int startAreaY = this.m_level.GetPlayArea().GetStartY();
                int widthInTiles = this.m_level.GetWidthInTiles();

                int minDistance = -1;

                for (int i = 0; i < widthInTiles; i++)
                {
                    int centerY = (startAreaY - 1) / 2;

                    for (int j = 0; j < startAreaY - 1; j++, centerY--)
                    {
                        int distance = ((widthInTiles >> 1) - i) * ((widthInTiles >> 1) - i) + centerY * centerY;

                        if (minDistance == -1 || distance < minDistance)
                        {
                            LogicTile tile = this.m_level.GetTileMap().GetTile(i, j);

                            if (tile.GetPassableFlag() != 0)
                            {
                                this.m_placePositionX = i;
                                this.m_placePositionY = j;
                                minDistance = distance;
                            }
                        }
                    }
                }
            }

            if (this.m_placePositionX == -1 && this.m_placePositionY == -1)
            {
                Debugger.Error("LogicNpcAttack::placeOneUnit - No attack position found!");
            }
            else
            {
                LogicArrayList<LogicDataSlot> units = this.m_npcAvatar.GetUnits();

                for (int i = 0; i < units.Size(); i++)
                {
                    LogicDataSlot slot = units[i];

                    if (slot.GetCount() > 0)
                    {
                        LogicCharacter character = LogicPlaceAttackerCommand.PlaceAttacker(this.m_npcAvatar, (LogicCharacterData) slot.GetData(), this.m_level,
                                                                                           this.m_placePositionX << 9,
                                                                                           this.m_placePositionY << 9);

                        if (!this.m_unitsDeployStarted)
                        {
                            character.GetListener().MapUnlocked();
                        }

                        character.GetCombatComponent().SetPreferredTarget(this.m_buildingClass, 100, false);

                        this.m_unitsDeployStarted = true;

                        return true;
                    }
                }
            }

            return false;
        }

        public void Tick()
        {
            if (!this.m_unitsDeployed)
            {
                this.m_nextUnit -= 64;

                if (this.m_nextUnit <= 0)
                {
                    this.m_unitsDeployed = !this.PlaceOneUnit();
                    this.m_nextUnit = 200;
                }
            }
        }
    }
}