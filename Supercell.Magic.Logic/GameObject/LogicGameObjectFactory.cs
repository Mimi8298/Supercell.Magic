namespace Supercell.Magic.Logic.GameObject
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;

    public static class LogicGameObjectFactory
    {
        public static LogicGameObject CreateGameObject(LogicGameObjectData data, LogicLevel level, int villageType)
        {
            LogicGameObject gameObject = null;

            switch (data.GetDataType())
            {
                case LogicDataType.BUILDING:
                    gameObject = new LogicBuilding(data, level, villageType);
                    break;
                case LogicDataType.CHARACTER:
                case LogicDataType.HERO:
                    gameObject = new LogicCharacter(data, level, villageType);
                    break;
                case LogicDataType.PROJECTILE:
                    gameObject = new LogicProjectile(data, level, villageType);
                    break;
                case LogicDataType.OBSTACLE:
                    gameObject = new LogicObstacle(data, level, villageType);
                    break;
                case LogicDataType.TRAP:
                    gameObject = new LogicTrap(data, level, villageType);
                    break;
                case LogicDataType.ALLIANCE_PORTAL:
                    gameObject = new LogicAlliancePortal(data, level, villageType);
                    break;
                case LogicDataType.DECO:
                    gameObject = new LogicDeco(data, level, villageType);
                    break;
                case LogicDataType.SPELL:
                    gameObject = new LogicSpell(data, level, villageType);
                    break;
                case LogicDataType.VILLAGE_OBJECT:
                    gameObject = new LogicVillageObject(data, level, villageType);
                    break;
                default:
                {
                    Debugger.Warning("Trying to create game object with data that does not inherit LogicGameObjectData. GlobalId=" + data.GetGlobalID());
                    break;
                }
            }

            return gameObject;
        }
    }
}