namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Mode;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public sealed class LogicBuyShieldCommand : LogicCommand
    {
        private LogicShieldData m_shieldData;

        public LogicBuyShieldCommand()
        {
            // LogicBuyShieldCommand.
        }

        public LogicBuyShieldCommand(LogicShieldData data)
        {
            this.m_shieldData = data;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_shieldData = (LogicShieldData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.SHIELD);
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            ByteStreamHelper.WriteDataReference(encoder, this.m_shieldData);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.BUY_SHIELD;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_shieldData = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_shieldData != null)
            {
                int cooldownSecs = level.GetCooldownManager().GetCooldownSeconds(this.m_shieldData.GetGlobalID());

                if (cooldownSecs <= 0)
                {
                    LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                    if (this.m_shieldData.GetScoreLimit() > playerAvatar.GetScore() || this.m_shieldData.GetScoreLimit() <= 0)
                    {
                        if (playerAvatar.HasEnoughDiamonds(this.m_shieldData.GetDiamondsCost(), true, level))
                        {
                            LogicGameMode gameMode = level.GetGameMode();

                            playerAvatar.UseDiamonds(this.m_shieldData.GetDiamondsCost());
                            playerAvatar.GetChangeListener().DiamondPurchaseMade(6, this.m_shieldData.GetGlobalID(), this.m_shieldData.GetTimeHours() * 3600,
                                                                                 this.m_shieldData.GetDiamondsCost(), level.GetVillageType());

                            int shieldTime = gameMode.GetShieldRemainingSeconds() + this.m_shieldData.GetTimeHours() * 3600;
                            int guardTime = gameMode.GetGuardRemainingSeconds();
                            int personalBreak = 0;

                            if (this.m_shieldData.GetTimeHours() <= 0)
                            {
                                if (shieldTime > 0)
                                {
                                    return -2;
                                }

                                guardTime += this.m_shieldData.GetGuardTimeHours() * 3600;
                            }
                            else
                            {
                                LogicLeagueData leagueData = playerAvatar.GetLeagueTypeData();

                                if (playerAvatar.GetAttackShieldReduceCounter() != 0)
                                {
                                    playerAvatar.SetAttackShieldReduceCounter(0);
                                    playerAvatar.GetChangeListener().AttackShieldReduceCounterChanged(0);
                                }

                                if (playerAvatar.GetDefenseVillageGuardCounter() != 0)
                                {
                                    playerAvatar.SetDefenseVillageGuardCounter(0);
                                    playerAvatar.GetChangeListener().DefenseVillageGuardCounterChanged(0);
                                }

                                guardTime += leagueData.GetVillageGuardInMins() * 60;
                            }

                            if (shieldTime <= 0)
                            {
                                personalBreak = LogicMath.Min(LogicDataTables.GetGlobals().GetPersonalBreakLimitSeconds() + this.m_shieldData.GetGuardTimeHours() * 3600,
                                                              gameMode.GetPersonalBreakCooldownSeconds() + this.m_shieldData.GetGuardTimeHours() * 3600);
                            }
                            else
                            {
                                personalBreak = shieldTime + LogicDataTables.GetGlobals().GetPersonalBreakLimitSeconds();
                            }

                            gameMode.SetPersonalBreakCooldownSeconds(personalBreak);
                            gameMode.SetShieldRemainingSeconds(shieldTime);
                            gameMode.SetGuardRemainingSeconds(guardTime);

                            level.GetHome().GetChangeListener().ShieldActivated(shieldTime, guardTime);
                            level.GetCooldownManager().AddCooldown(this.m_shieldData.GetGlobalID(), this.m_shieldData.GetCooldownSecs());

                            return 0;
                        }
                    }
                }
            }

            return -1;
        }
    }
}