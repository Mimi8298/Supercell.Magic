namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicSpeedUpTrainingVillage2Command : LogicCommand
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
            return LogicCommandType.SPEED_UP_TRAINING_VILLAGE2;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicArrayList<LogicComponent> components = level.GetComponentManager().GetComponents(LogicComponentType.VILLAGE2_UNIT);
            int remainingSecs = 0;

            for (int i = 0; i < components.Size(); i++)
            {
                remainingSecs += ((LogicVillage2UnitComponent) components[i]).GetRemainingSecs();
            }

            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
            int speedUpCost = LogicGamePlayUtil.GetSpeedUpCost(remainingSecs, 4, 1);

            if (!playerAvatar.HasEnoughDiamonds(speedUpCost, true, level))
            {
                return -1;
            }

            playerAvatar.UseDiamonds(speedUpCost);

            for (int i = 0; i < components.Size(); i++)
            {
                LogicVillage2UnitComponent component = (LogicVillage2UnitComponent) components[i];

                if (component.GetCurrentlyTrainedUnit() != null && component.GetRemainingSecs() > 0)
                {
                    component.ProductionCompleted();
                }
            }

            playerAvatar.GetChangeListener().DiamondPurchaseMade(16, 0, 0, speedUpCost, 1);

            return 0;
        }
    }
}