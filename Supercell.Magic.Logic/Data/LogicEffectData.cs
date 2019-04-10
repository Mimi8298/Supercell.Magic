namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicEffectData : LogicData
    {
        public LogicEffectData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicEffectData.
        }

        public string SWF { get; protected set; }
        public string ExportName { get; protected set; }
        protected string[] ParticleEmitter { get; set; }
        public int EmitterDelayMs { get; protected set; }
        public int CameraShake { get; protected set; }
        public int CameraShakeTimeMS { get; protected set; }
        public bool CameraShakeInReplay { get; protected set; }
        protected bool[] AttachToParent { get; set; }
        protected bool[] DetachAfterStart { get; set; }
        protected bool[] DestroyWhenParentDies { get; set; }
        public bool Looping { get; protected set; }
        protected string[] IsoLayer { get; set; }
        public bool Targeted { get; protected set; }
        public int MaxCount { get; protected set; }
        protected string[] Sound { get; set; }
        protected int[] Volume { get; set; }
        protected int[] MinPitch { get; set; }
        protected int[] MaxPitch { get; set; }
        public string LowEndSound { get; protected set; }
        public int LowEndVolume { get; protected set; }
        public int LowEndMinPitch { get; protected set; }
        public int LowEndMaxPitch { get; protected set; }
        public bool Beam { get; protected set; }

        public override void CreateReferences()
        {
            base.CreateReferences();
        }

        public string GetParticleEmitter(int index)
        {
            return this.ParticleEmitter[index];
        }

        public bool GetAttachToParent(int index)
        {
            return this.AttachToParent[index];
        }

        public bool GetDetachAfterStart(int index)
        {
            return this.DetachAfterStart[index];
        }

        public bool GetDestroyWhenParentDies(int index)
        {
            return this.DestroyWhenParentDies[index];
        }

        public string GetIsoLayer(int index)
        {
            return this.IsoLayer[index];
        }

        public string GetSound(int index)
        {
            return this.Sound[index];
        }

        public int GetVolume(int index)
        {
            return this.Volume[index];
        }

        public int GetMinPitch(int index)
        {
            return this.MinPitch[index];
        }

        public int GetMaxPitch(int index)
        {
            return this.MaxPitch[index];
        }
    }
}