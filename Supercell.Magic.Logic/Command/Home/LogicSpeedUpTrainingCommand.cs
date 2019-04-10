namespace Supercell.Magic.Logic.Command.Home
{
    using System;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Unit;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicSpeedUpTrainingCommand : LogicCommand
    {
        private int m_gameObjectId;
        private bool m_spellProduction;

        public LogicSpeedUpTrainingCommand()
        {
            // LogicSpeedUpTrainingCommand.
        }

        public LogicSpeedUpTrainingCommand(int gameObjectId)
        {
            this.m_gameObjectId = gameObjectId;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            this.m_spellProduction = stream.ReadBoolean();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            encoder.WriteBoolean(this.m_spellProduction);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.SPEED_UP_TRAINING;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            if (level.GetVillageType() == 0)
            {
                if (!LogicDataTables.GetGlobals().UseNewTraining())
                {
                    throw new NotImplementedException(); // TODO: Implement this.
                }

                return this.SpeedUpNewTrainingUnit(level);
            }

            return -32;
        }

        public int SpeedUpNewTrainingUnit(LogicLevel level)
        {
            if (LogicDataTables.GetGlobals().UseNewTraining())
            {
                LogicUnitProduction unitProduction = this.m_spellProduction ? level.GetGameObjectManager().GetSpellProduction() : level.GetGameObjectManager().GetUnitProduction();
                LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                int remainingSecs = unitProduction.GetTotalRemainingSeconds();
                int speedUpCost = LogicGamePlayUtil.GetSpeedUpCost(remainingSecs, this.m_spellProduction ? 1 : 4, level.GetVillageType());

                if (!level.GetMissionManager().IsTutorialFinished())
                {
                    if (speedUpCost > 0 && LogicDataTables.GetGlobals().GetTutorialTrainingSpeedUpCost() >= 0)
                    {
                        speedUpCost = LogicDataTables.GetGlobals().GetTutorialTrainingSpeedUpCost();
                    }
                }

                if (playerAvatar.HasEnoughDiamonds(speedUpCost, true, level))
                {
                    playerAvatar.UseDiamonds(speedUpCost);
                    unitProduction.SpeedUp();
                    playerAvatar.GetChangeListener().DiamondPurchaseMade(unitProduction.GetUnitProductionType() == LogicDataType.CHARACTER ? 2 : 7, 0, 0, speedUpCost, level.GetVillageType());

                    return 0;
                }

                return -1;
            }

            return -99;
        }
    }
}