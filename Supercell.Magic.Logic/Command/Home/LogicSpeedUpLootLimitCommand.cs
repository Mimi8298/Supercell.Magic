namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Calendar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public sealed class LogicSpeedUpLootLimitCommand : LogicCommand
    {
        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.SPEED_UP_LOOT_LIMIT;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                int lootLimitCooldown = playerAvatar.GetVariableByName("LootLimitCooldown");

                if (lootLimitCooldown == 1)
                {
                    LogicConfiguration configuration = level.GetGameMode().GetConfiguration();

                    if (configuration != null)
                    {
                        LogicCalendar calendar = level.GetGameMode().GetCalendar();

                        if (calendar != null)
                        {
                            int remainingSecs = playerAvatar.GetRemainingLootLimitTime();
                            int totalSecs = LogicCalendar.GetDuelLootLimitCooldownInMinutes(calendar, configuration) * 60;
                            int maxDiamondsCostPercent = LogicCalendar.GetDuelBonusMaxDiamondCostPercent(calendar, configuration);

                            int speedUpCost = LogicMath.Max(
                                LogicGamePlayUtil.GetLeagueVillage2(playerAvatar.GetDuelScore()).GetMaxDiamondCost() * maxDiamondsCostPercent * remainingSecs / totalSecs / 100, 1);

                            if (playerAvatar.HasEnoughDiamonds(speedUpCost, true, level))
                            {
                                playerAvatar.UseDiamonds(speedUpCost);
                                playerAvatar.GetChangeListener().DiamondPurchaseMade(18, 0, remainingSecs, speedUpCost, level.GetVillageType());
                                playerAvatar.FastForwardLootLimit(remainingSecs);

                                return 0;
                            }

                            return -3;
                        }

                        return -5;
                    }

                    return -4;
                }

                return -3;
            }

            return -2;
        }
    }
}