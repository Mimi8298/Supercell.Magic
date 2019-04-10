namespace Supercell.Magic.Logic.League.Entry
{
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;

    public class LogicLegendSeasonEntry
    {
        private int m_bestSeasonState;
        private int m_bestSeasonMonth;
        private int m_bestSeasonYear;
        private int m_bestSeasonRank;
        private int m_bestSeasonScore;

        private int m_lastSeasonState;
        private int m_lastSeasonMonth;
        private int m_lastSeasonYear;
        private int m_lastSeasonRank;
        private int m_lastSeasonScore;

        public void Destruct()
        {
            // Destruct.
        }

        public void Decode(ByteStream stream)
        {
            this.m_bestSeasonState = stream.ReadInt();
            this.m_bestSeasonYear = stream.ReadInt();
            this.m_bestSeasonMonth = stream.ReadInt();
            this.m_bestSeasonRank = stream.ReadInt();
            this.m_bestSeasonScore = stream.ReadInt();
            this.m_lastSeasonState = stream.ReadInt();
            this.m_lastSeasonYear = stream.ReadInt();
            this.m_lastSeasonMonth = stream.ReadInt();
            this.m_lastSeasonRank = stream.ReadInt();
            this.m_lastSeasonScore = stream.ReadInt();
        }

        public void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_bestSeasonState);
            encoder.WriteInt(this.m_bestSeasonYear);
            encoder.WriteInt(this.m_bestSeasonMonth);
            encoder.WriteInt(this.m_bestSeasonRank);
            encoder.WriteInt(this.m_bestSeasonScore);
            encoder.WriteInt(this.m_lastSeasonState);
            encoder.WriteInt(this.m_lastSeasonYear);
            encoder.WriteInt(this.m_lastSeasonMonth);
            encoder.WriteInt(this.m_lastSeasonRank);
            encoder.WriteInt(this.m_lastSeasonScore);
        }

        public int GetLastSeasonState()
        {
            return this.m_lastSeasonState;
        }

        public void SetLastSeasonState(int value)
        {
            this.m_lastSeasonState = value;
        }

        public int GetLastSeasonYear()
        {
            return this.m_lastSeasonYear;
        }

        public int GetLastSeasonMonth()
        {
            return this.m_lastSeasonMonth;
        }

        public void SetLastSeasonDate(int year, int month)
        {
            this.m_lastSeasonYear = year;
            this.m_lastSeasonMonth = month;
        }

        public int GetLastSeasonScore()
        {
            return this.m_lastSeasonScore;
        }

        public void SetLastSeasonScore(int score)
        {
            this.m_lastSeasonScore = score;
        }

        public int GetLastSeasonRank()
        {
            return this.m_lastSeasonRank;
        }

        public void SetLastSeasonRank(int score)
        {
            this.m_lastSeasonRank = score;
        }

        public int GetBestSeasonState()
        {
            return this.m_bestSeasonState;
        }

        public void SetBestSeasonState(int value)
        {
            this.m_bestSeasonState = value;
        }

        public int GetBestSeasonYear()
        {
            return this.m_bestSeasonYear;
        }

        public int GetBestSeasonMonth()
        {
            return this.m_bestSeasonMonth;
        }

        public void SetBestSeasonDate(int year, int month)
        {
            this.m_bestSeasonYear = year;
            this.m_bestSeasonMonth = month;
        }

        public int GetBestSeasonScore()
        {
            return this.m_bestSeasonScore;
        }

        public void SetBestSeasonScore(int score)
        {
            this.m_bestSeasonScore = score;
        }

        public int GetBestSeasonRank()
        {
            return this.m_bestSeasonRank;
        }

        public void SetBestSeasonRank(int score)
        {
            this.m_bestSeasonRank = score;
        }

        public void ReadFromJSON(LogicJSONObject jsonObject)
        {
            this.m_bestSeasonState = LogicJSONHelper.GetInt(jsonObject, "best_season_state");
            this.m_bestSeasonYear = LogicJSONHelper.GetInt(jsonObject, "best_season_year");
            this.m_bestSeasonMonth = LogicJSONHelper.GetInt(jsonObject, "best_season_month");
            this.m_bestSeasonRank = LogicJSONHelper.GetInt(jsonObject, "best_season_rank");
            this.m_bestSeasonScore = LogicJSONHelper.GetInt(jsonObject, "best_season_score");
            this.m_lastSeasonState = LogicJSONHelper.GetInt(jsonObject, "last_season_state");
            this.m_lastSeasonYear = LogicJSONHelper.GetInt(jsonObject, "last_season_year");
            this.m_lastSeasonMonth = LogicJSONHelper.GetInt(jsonObject, "last_season_month");
            this.m_lastSeasonRank = LogicJSONHelper.GetInt(jsonObject, "last_season_rank");
            this.m_lastSeasonScore = LogicJSONHelper.GetInt(jsonObject, "last_season_score");
        }

        public void WriteToJSON(LogicJSONObject jsonObject)
        {
            jsonObject.Put("best_season_state", new LogicJSONNumber(this.m_bestSeasonState));
            jsonObject.Put("best_season_year", new LogicJSONNumber(this.m_bestSeasonYear));
            jsonObject.Put("best_season_month", new LogicJSONNumber(this.m_bestSeasonMonth));
            jsonObject.Put("best_season_rank", new LogicJSONNumber(this.m_bestSeasonRank));
            jsonObject.Put("best_season_score", new LogicJSONNumber(this.m_bestSeasonScore));
            jsonObject.Put("last_season_state", new LogicJSONNumber(this.m_lastSeasonState));
            jsonObject.Put("last_season_year", new LogicJSONNumber(this.m_lastSeasonYear));
            jsonObject.Put("last_season_month", new LogicJSONNumber(this.m_lastSeasonMonth));
            jsonObject.Put("last_season_rank", new LogicJSONNumber(this.m_lastSeasonRank));
            jsonObject.Put("last_season_score", new LogicJSONNumber(this.m_lastSeasonScore));
        }
    }
}