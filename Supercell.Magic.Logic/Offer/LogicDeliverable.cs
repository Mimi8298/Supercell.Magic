namespace Supercell.Magic.Logic.Offer
{
    using System.Text;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;

    public class LogicDeliverable
    {
        public void Decode(ByteStream stream)
        {
            this.ReadFromJSON(LogicJSONParser.ParseObject(stream.ReadString(900000) ?? string.Empty));
        }

        public void Encode(ChecksumEncoder encoder)
        {
            LogicJSONObject jsonObject = new LogicJSONObject();
            StringBuilder stringBuilder = new StringBuilder();

            this.WriteToJSON(jsonObject);

            jsonObject.WriteToString(stringBuilder);
            encoder.WriteString(stringBuilder.ToString());
        }

        public virtual void Destruct()
        {
            // Destruct.
        }

        public virtual void WriteToJSON(LogicJSONObject jsonObject)
        {
            Debugger.DoAssert(this.GetDeliverableType() != 0, "Deliverable type not set!");
            jsonObject.Put("type", new LogicJSONString(this.GetDeliverableType().ToString()));
        }

        public virtual void ReadFromJSON(LogicJSONObject jsonObject)
        {
            // ReadFromJSON.
        }

        public virtual int GetDeliverableType()
        {
            return -1;
        }

        public virtual LogicDeliverableBundle Compensate(LogicLevel level)
        {
            return null;
        }

        public virtual bool Deliver(LogicLevel level)
        {
            return true;
        }

        public virtual bool CanBeDeliver(LogicLevel level)
        {
            return true;
        }
    }
}