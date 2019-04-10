namespace Supercell.Magic.Logic.Message.Avatar.Attack
{
    public class Village2AttackEntryFactory
    {
        public static Village2AttackEntry CreateAttackEntryByType(int type)
        {
            switch (type)
            {
                case Village2AttackEntry.ATTACK_ENTRY_TYPE_BASE:
                    return new Village2AttackEntry();
                case Village2AttackEntry.ATTACK_ENTRY_TYPE_BATTLE_PROGRESS:
                    return new Village2BattleProgressAttackEntry();
                default:
                    return null;
            }
        }
    }
}