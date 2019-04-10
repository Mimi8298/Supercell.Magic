namespace Supercell.Magic.Logic.Cooldown
{
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class LogicCooldown
    {
        private int m_targetGlobalId;
        private int m_cooldownTime;

        public LogicCooldown()
        {
            // LogicCooldown.
        }

        public LogicCooldown(int targetGlobalId, int cooldownSecs)
        {
            this.m_targetGlobalId = targetGlobalId;
            this.m_cooldownTime = LogicTime.GetCooldownSecondsInTicks(cooldownSecs);
        }

        public void Tick()
        {
            if (this.m_cooldownTime > 0)
            {
                --this.m_cooldownTime;
            }
        }

        public void FastForwardTime(int secs)
        {
            this.m_cooldownTime = LogicMath.Max(this.m_cooldownTime - LogicTime.GetCooldownSecondsInTicks(secs), 0);
        }

        public void Load(LogicJSONObject jsonObject)
        {
            LogicJSONNumber cooldownNumber = jsonObject.GetJSONNumber("cooldown");
            LogicJSONNumber targetNumber = jsonObject.GetJSONNumber("target");

            if (cooldownNumber == null)
            {
                Debugger.Error("LogicCooldown::load - Cooldown was not found!");
                return;
            }

            if (targetNumber == null)
            {
                Debugger.Error("LogicCooldown::load - Target was not found!");
                return;
            }

            this.m_cooldownTime = cooldownNumber.GetIntValue();
            this.m_targetGlobalId = targetNumber.GetIntValue();
        }

        public void Save(LogicJSONObject jsonObject)
        {
            jsonObject.Put("cooldown", new LogicJSONNumber(this.m_cooldownTime));
            jsonObject.Put("target", new LogicJSONNumber(this.m_targetGlobalId));
        }

        public int GetCooldownSeconds()
        {
            return LogicTime.GetCooldownTicksInSeconds(this.m_cooldownTime);
        }

        public int GetTargetGlobalId()
        {
            return this.m_targetGlobalId;
        }
    }
}