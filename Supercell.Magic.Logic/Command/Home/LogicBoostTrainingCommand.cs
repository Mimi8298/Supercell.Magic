namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Unit;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicBoostTrainingCommand : LogicCommand
    {
        private int m_productionType;

        public LogicBoostTrainingCommand()
        {
            // LogicBoostTrainingCommand.
        }

        public LogicBoostTrainingCommand(int productionType)
        {
            this.m_productionType = productionType;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_productionType = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_productionType);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.BOOST_TRAINING;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            if (LogicDataTables.GetGlobals().UseNewTraining())
            {
                if (level.GetVillageType() == 0)
                {
                    LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
                    LogicUnitProduction unitProduction = this.m_productionType == 1
                        ? level.GetGameObjectManagerAt(0).GetSpellProduction()
                        : level.GetGameObjectManagerAt(0).GetUnitProduction();

                    if (unitProduction.CanBeBoosted())
                    {
                        int cost = unitProduction.GetBoostCost();

                        if (playerAvatar.HasEnoughDiamonds(cost, true, level))
                        {
                            playerAvatar.UseDiamonds(cost);
                            playerAvatar.GetChangeListener().DiamondPurchaseMade(15, 0, 0, cost, level.GetVillageType());

                            unitProduction.Boost();

                            return 0;
                        }

                        return -2;
                    }

                    return -1;
                }

                return -32;
            }

            return -99;
        }
    }
}