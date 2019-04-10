namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicNewShopItemsSeenCommand : LogicCommand
    {
        private LogicDataType m_newShopItemsType;
        private int m_newShopItemsIndex;
        private int m_newShopItemsCount;


        public LogicNewShopItemsSeenCommand()
        {
            // LogicNewShopItemsSeenCommand.
        }

        public LogicNewShopItemsSeenCommand(int index, int type, int count)
        {
            this.m_newShopItemsIndex = index;
            this.m_newShopItemsType = (LogicDataType) type;
            this.m_newShopItemsCount = count;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_newShopItemsIndex = stream.ReadInt();
            this.m_newShopItemsType = (LogicDataType) stream.ReadInt();
            this.m_newShopItemsCount = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_newShopItemsIndex);
            encoder.WriteInt((int) this.m_newShopItemsType);
            encoder.WriteInt(this.m_newShopItemsCount);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.NEW_SHOP_ITEMS_SEEN;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_newShopItemsType == LogicDataType.BUILDING || 
                this.m_newShopItemsType == LogicDataType.TRAP || 
                this.m_newShopItemsType == LogicDataType.DECO)
            {
                if (level.SetUnlockedShopItemCount((LogicGameObjectData) LogicDataTables.GetTable(this.m_newShopItemsType).GetItemAt(this.m_newShopItemsIndex),
                    this.m_newShopItemsIndex,
                    this.m_newShopItemsCount,
                    level.GetVillageType()))
                {
                    return 0;
                }

                return -2;
            }

            return -1;
        }
    }
}