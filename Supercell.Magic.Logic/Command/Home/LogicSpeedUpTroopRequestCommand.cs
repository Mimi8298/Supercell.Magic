namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicSpeedUpTroopRequestCommand : LogicCommand
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
            return LogicCommandType.SPEED_UP_TROOP_REQUEST;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            if (LogicDataTables.GetGlobals().UseTroopRequestSpeedUp())
            {
                LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
                LogicGameObjectManager gameObjectManager = level.GetGameObjectManagerAt(0);
                LogicBuilding allianceCastle = gameObjectManager.GetAllianceCastle();

                if (allianceCastle != null)
                {
                    LogicBunkerComponent bunkerComponent = allianceCastle.GetBunkerComponent();

                    if (bunkerComponent != null)
                    {
                        if (playerAvatar.GetAllianceCastleUsedCapacity() < playerAvatar.GetAllianceCastleTotalCapacity() ||
                            playerAvatar.GetAllianceCastleUsedSpellCapacity() < playerAvatar.GetAllianceCastleTotalSpellCapacity())
                        {
                            int speedUpCost = LogicGamePlayUtil.GetSpeedUpCost(bunkerComponent.GetRequestCooldownTime(), 3, 0);

                            if (playerAvatar.HasEnoughDiamonds(speedUpCost, true, level))
                            {
                                playerAvatar.UseDiamonds(speedUpCost);
                                playerAvatar.GetChangeListener().DiamondPurchaseMade(11, 0, 0, speedUpCost, level.GetVillageType());
                                bunkerComponent.StopRequestCooldownTime();

                                return 0;
                            }

                            return -6;
                        }

                        return -5;
                    }

                    return -4;
                }

                return -3;
            }

            return -1;
        }
    }
}