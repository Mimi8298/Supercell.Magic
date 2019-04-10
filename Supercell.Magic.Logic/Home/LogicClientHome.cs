namespace Supercell.Magic.Logic.Home
{
    using Supercell.Magic.Logic.Home.Change;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class LogicClientHome
    {
        private LogicLong m_homeId;
        private LogicHomeChangeListener m_listener;

        private int m_shieldDurationSeconds;
        private int m_guardDurationSeconds;
        private int m_personalBreakSeconds;

        private LogicCompressibleString m_compressibleHomeJson;
        private LogicCompressibleString m_compressibleGlobalJson;
        private LogicCompressibleString m_compressibleCalendarJson;

        public LogicClientHome()
        {
            this.m_compressibleHomeJson = new LogicCompressibleString();
            this.m_compressibleGlobalJson = new LogicCompressibleString();
            this.m_compressibleCalendarJson = new LogicCompressibleString();

            this.Init();
        }

        public void Destruct()
        {
            if (this.m_compressibleGlobalJson != null)
            {
                this.m_compressibleGlobalJson.Destruct();
                this.m_compressibleGlobalJson = null;
            }

            if (this.m_compressibleCalendarJson != null)
            {
                this.m_compressibleCalendarJson.Destruct();
                this.m_compressibleCalendarJson = null;
            }

            if (this.m_compressibleHomeJson != null)
            {
                this.m_compressibleHomeJson.Destruct();
                this.m_compressibleHomeJson = null;
            }

            if (this.m_listener != null)
            {
                this.m_listener.Destruct();
                this.m_listener = null;
            }

            this.m_homeId = null;
        }

        public void Init()
        {
            this.m_homeId = new LogicLong();
            this.m_listener = new LogicHomeChangeListener();
        }

        public virtual void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteLong(this.m_homeId);

            encoder.WriteInt(this.m_shieldDurationSeconds);
            encoder.WriteInt(this.m_guardDurationSeconds);
            encoder.WriteInt(this.m_personalBreakSeconds);

            this.m_compressibleHomeJson.Encode(encoder);
            this.m_compressibleCalendarJson.Encode(encoder);
            this.m_compressibleGlobalJson.Encode(encoder);
        }

        public void Decode(ByteStream stream)
        {
            this.m_homeId = stream.ReadLong();
            this.m_shieldDurationSeconds = stream.ReadInt();
            this.m_guardDurationSeconds = stream.ReadInt();
            this.m_personalBreakSeconds = stream.ReadInt();

            this.m_compressibleHomeJson.Decode(stream);
            this.m_compressibleCalendarJson.Decode(stream);
            this.m_compressibleGlobalJson.Decode(stream);
        }

        public LogicLong GetHomeId()
        {
            return this.m_homeId;
        }

        public void SetHomeId(LogicLong value)
        {
            this.m_homeId = value;
        }

        public int GetShieldDurationSeconds()
        {
            return this.m_shieldDurationSeconds;
        }

        public int GetGuardDurationSeconds()
        {
            return this.m_guardDurationSeconds;
        }

        public int GetPersonalBreakSeconds()
        {
            return this.m_personalBreakSeconds;
        }

        public LogicCompressibleString GetCompressibleCalendarJSON()
        {
            return this.m_compressibleCalendarJson;
        }

        public LogicCompressibleString GetCompressibleGlobalJSON()
        {
            return this.m_compressibleGlobalJson;
        }

        public LogicCompressibleString GetCompressibleHomeJSON()
        {
            return this.m_compressibleHomeJson;
        }

        public string GetHomeJSON()
        {
            return this.m_compressibleHomeJson.Get();
        }

        public void SetHomeJSON(string json)
        {
            this.m_compressibleHomeJson.Set(json);
        }

        public string GetCalendarJSON()
        {
            return this.m_compressibleCalendarJson.Get();
        }

        public void SetCalendarJSON(string json)
        {
            this.m_compressibleCalendarJson.Set(json);
        }

        public string GetGlobalJSON()
        {
            return this.m_compressibleGlobalJson.Get();
        }

        public void SetGlobalJSON(string json)
        {
            this.m_compressibleGlobalJson.Set(json);
        }

        public void SetShieldDurationSeconds(int secs)
        {
            this.m_shieldDurationSeconds = secs;
        }

        public void SetGuardDurationSeconds(int secs)
        {
            this.m_guardDurationSeconds = secs;
        }

        public void SetPersonalBreakSeconds(int secs)
        {
            this.m_personalBreakSeconds = secs;
        }

        public LogicHomeChangeListener GetChangeListener()
        {
            return this.m_listener;
        }

        public void SetChangeListener(LogicHomeChangeListener listener)
        {
            this.m_listener = listener;
        }

        public LogicJSONObject Save()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("homeJSON", this.m_compressibleHomeJson.Save());
            jsonObject.Put("shield_t", new LogicJSONNumber(this.m_shieldDurationSeconds));
            jsonObject.Put("guard_t", new LogicJSONNumber(this.m_guardDurationSeconds));
            jsonObject.Put("personal_break_t", new LogicJSONNumber(this.m_personalBreakSeconds));

            return jsonObject;
        }

        public void Load(LogicJSONObject jsonObject)
        {
            this.m_compressibleHomeJson.Load(jsonObject.GetJSONObject("homeJSON"));

            this.m_shieldDurationSeconds = jsonObject.GetJSONNumber("shield_t").GetIntValue();
            this.m_guardDurationSeconds = jsonObject.GetJSONNumber("guard_t").GetIntValue();
            this.m_personalBreakSeconds = jsonObject.GetJSONNumber("personal_break_t").GetIntValue();
        }
    }
}