namespace Supercell.Magic.Logic.Offer
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;

    public class LogicDeliverableSpecial : LogicDeliverable
    {
        private int m_id;

        public LogicDeliverableSpecial()
        {
            this.m_id = -1;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_id = -1;
        }

        public override void WriteToJSON(LogicJSONObject jsonObject)
        {
            base.WriteToJSON(jsonObject);
            jsonObject.Put("id", new LogicJSONNumber(this.m_id));
        }

        public override void ReadFromJSON(LogicJSONObject jsonObject)
        {
            base.ReadFromJSON(jsonObject);
            this.m_id = LogicJSONHelper.GetInt(jsonObject, "id");
        }

        public override int GetDeliverableType()
        {
            return 4;
        }

        public override bool Deliver(LogicLevel level)
        {
            LogicAvatar avatar = level.GetHomeOwnerAvatar();

            switch (this.m_id)
            {
                case 0:
                    avatar.SetRedPackageState(avatar.GetRedPackageState() | 0x13);
                    break;
                default:
                    Debugger.Error("Unknown special delivery id " + this.m_id);
                    break;
            }

            return true;
        }

        public override bool CanBeDeliver(LogicLevel level)
        {
            return true;
        }

        public override LogicDeliverableBundle Compensate(LogicLevel level)
        {
            return null;
        }

        public int GetId()
        {
            return this.m_id;
        }

        public void SetId(int value)
        {
            this.m_id = value;
        }
    }
}