namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicProjectileData : LogicGameObjectData
    {
        private LogicSpellData m_hitSpellData;
        private LogicEffectData m_effectData;
        private LogicEffectData m_destroyedEffectData;
        private LogicEffectData m_bounceEffectData;
        private LogicParticleEmitterData m_particleEmiterData;

        private string m_swf;
        private string m_exportName;
        private string m_shadowSWF;
        private string m_shadowExportName;
        private string m_particleEmitter;

        private int m_startHeight;
        private int m_startOffset;

        private int m_speed;
        private int m_scale;
        private int m_slowdownDefensePercent;
        private int m_hitSpellLevel;
        private int m_ballisticHeight;
        private int m_trajectoryStyle;
        private int m_fixedTravelTime;
        private int m_damageDelay;
        private int m_targetPosRandomRadius;

        private bool m_randomHitPosition;
        private bool m_ballistic;
        private bool m_playOnce;
        private bool m_useRotate;
        private bool m_useTopLayer;
        private bool m_trackTarget;
        private bool m_useDirection;
        private bool m_scaleTimeline;
        private bool m_directionFrame;

        public LogicProjectileData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicProjectileData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_swf = this.GetValue("SWF", 0);
            this.m_exportName = this.GetValue("ExportName", 0);
            this.m_shadowSWF = this.GetValue("ShadowSWF", 0);
            this.m_shadowExportName = this.GetValue("ShadowExportName", 0);

            this.m_startHeight = this.GetIntegerValue("StartHeight", 0);
            this.m_startOffset = this.GetIntegerValue("StartOffset", 0);
            this.m_randomHitPosition = this.GetBooleanValue("RandomHitPosition", 0);

            string particleEmiter = this.GetValue("ParticleEmitter", 0);

            if (particleEmiter.Length > 0)
            {
                this.m_particleEmiterData = LogicDataTables.GetParticleEmitterByName(particleEmiter, this);
            }

            this.m_ballistic = this.GetBooleanValue("IsBallistic", 0);
            this.m_speed = (this.GetIntegerValue("Speed", 0) << 9) / 100;
            this.m_playOnce = this.GetBooleanValue("PlayOnce", 0);
            this.m_useRotate = this.GetBooleanValue("UseRotate", 0);
            this.m_useTopLayer = this.GetBooleanValue("UseTopLayer", 0);
            this.m_scale = this.GetIntegerValue("Scale", 0);

            if (this.m_scale == 0)
            {
                this.m_scale = 100;
            }


            this.m_slowdownDefensePercent = this.GetIntegerValue("SlowdownDefencePercent", 0);
            this.m_hitSpellData = LogicDataTables.GetSpellByName(this.GetValue("HitSpell", 0), this);
            this.m_hitSpellLevel = this.GetIntegerValue("HitSpellLevel", 0);
            this.m_trackTarget = this.GetBooleanValue("DontTrackTarget", 0) ^ true;
            this.m_ballisticHeight = this.GetIntegerValue("BallisticHeight", 0);
            this.m_trajectoryStyle = this.GetIntegerValue("TrajectoryStyle", 0);
            this.m_fixedTravelTime = this.GetIntegerValue("FixedTravelTime", 0);
            this.m_damageDelay = this.GetIntegerValue("DamageDelay", 0);
            this.m_useDirection = this.GetBooleanValue("UseDirections", 0);
            this.m_scaleTimeline = this.GetBooleanValue("ScaleTimeline", 0);
            this.m_targetPosRandomRadius = this.GetIntegerValue("TargetPosRandomRadius", 0);
            this.m_directionFrame = this.GetBooleanValue("DirectionFrame", 0);
            this.m_effectData = LogicDataTables.GetEffectByName(this.GetValue("Effect", 0), this);
            this.m_destroyedEffectData = LogicDataTables.GetEffectByName(this.GetValue("DestroyedEffect", 0), this);
            this.m_bounceEffectData = LogicDataTables.GetEffectByName(this.GetValue("BounceEffect", 0), this);
        }

        public LogicSpellData GetHitSpell()
        {
            return this.m_hitSpellData;
        }

        public LogicEffectData GetEffect()
        {
            return this.m_effectData;
        }

        public LogicEffectData GetDestroyedEffect()
        {
            return this.m_destroyedEffectData;
        }

        public LogicEffectData GetBounceEffect()
        {
            return this.m_bounceEffectData;
        }

        public LogicParticleEmitterData GetParticleEmiter()
        {
            return this.m_particleEmiterData;
        }

        public string GetSwf()
        {
            return this.m_swf;
        }

        public string GetExportName()
        {
            return this.m_exportName;
        }

        public string GetShadowSWF()
        {
            return this.m_shadowSWF;
        }

        public string GetShadowExportName()
        {
            return this.m_shadowExportName;
        }

        public string GetParticleEmitter()
        {
            return this.m_particleEmitter;
        }

        public int GetStartHeight()
        {
            return this.m_startHeight;
        }

        public int GetStartOffset()
        {
            return this.m_startOffset;
        }

        public int GetSpeed()
        {
            return this.m_speed;
        }

        public int GetScale()
        {
            return this.m_scale;
        }

        public int GetSlowdownDefensePercent()
        {
            return this.m_slowdownDefensePercent;
        }

        public int GetHitSpellLevel()
        {
            return this.m_hitSpellLevel;
        }

        public int GetBallisticHeight()
        {
            return this.m_ballisticHeight;
        }

        public int GetTrajectoryStyle()
        {
            return this.m_trajectoryStyle;
        }

        public int GetFixedTravelTime()
        {
            return this.m_fixedTravelTime;
        }

        public int GetDamageDelay()
        {
            return this.m_damageDelay;
        }

        public int GetTargetPosRandomRadius()
        {
            return this.m_targetPosRandomRadius;
        }

        public bool GetRandomHitPosition()
        {
            return this.m_randomHitPosition;
        }

        public bool IsBallistic()
        {
            return this.m_ballistic;
        }

        public bool GetPlayOnce()
        {
            return this.m_playOnce;
        }

        public bool GetUseRotate()
        {
            return this.m_useRotate;
        }

        public bool GetUseTopLayer()
        {
            return this.m_useTopLayer;
        }

        public bool GetTrackTarget()
        {
            return this.m_trackTarget;
        }

        public bool GetUseDirection()
        {
            return this.m_useDirection;
        }

        public bool GetScaleTimeline()
        {
            return this.m_scaleTimeline;
        }

        public bool GetDirectionFrame()
        {
            return this.m_directionFrame;
        }
    }
}