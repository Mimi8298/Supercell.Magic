namespace Supercell.Magic.Logic.Cooldown
{
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;

    public class LogicCooldownManager
    {
        private readonly LogicArrayList<LogicCooldown> m_cooldowns;

        public LogicCooldownManager()
        {
            this.m_cooldowns = new LogicArrayList<LogicCooldown>();
        }

        public void Destruct()
        {
            this.DeleteCooldowns();
        }

        public void DeleteCooldowns()
        {
            this.m_cooldowns.Destruct();
        }

        public void Tick()
        {
            for (int i = 0; i < this.m_cooldowns.Size(); i++)
            {
                this.m_cooldowns[i].Tick();

                if (this.m_cooldowns[i].GetCooldownSeconds() <= 0)
                {
                    this.m_cooldowns.Remove(i);
                }
            }
        }

        public void FastForwardTime(int secs)
        {
            for (int i = 0; i < this.m_cooldowns.Size(); i++)
            {
                this.m_cooldowns[i].FastForwardTime(secs);
            }
        }

        public void Load(LogicJSONObject jsonObject)
        {
            LogicJSONArray cooldownArray = jsonObject.GetJSONArray("cooldowns");

            if (cooldownArray != null)
            {
                int size = cooldownArray.Size();

                for (int i = 0; i < size; i++)
                {
                    LogicCooldown cooldown = new LogicCooldown();
                    cooldown.Load(cooldownArray.GetJSONObject(i));
                    this.m_cooldowns.Add(cooldown);
                }
            }
        }

        public void Save(LogicJSONObject jsonObject)
        {
            LogicJSONArray cooldownArray = new LogicJSONArray();

            for (int i = 0; i < this.m_cooldowns.Size(); i++)
            {
                LogicJSONObject cooldownObject = new LogicJSONObject();
                this.m_cooldowns[i].Save(cooldownObject);
                cooldownArray.Add(cooldownObject);
            }

            jsonObject.Put("cooldowns", cooldownArray);
        }

        public void AddCooldown(int targetGlobalId, int cooldownSecs)
        {
            this.m_cooldowns.Add(new LogicCooldown(targetGlobalId, cooldownSecs));
        }

        public int GetCooldownSeconds(int targetGlobalId)
        {
            for (int i = 0; i < this.m_cooldowns.Size(); i++)
            {
                if (this.m_cooldowns[i].GetTargetGlobalId() == targetGlobalId)
                {
                    return this.m_cooldowns[i].GetCooldownSeconds();
                }
            }

            return 0;
        }
    }
}