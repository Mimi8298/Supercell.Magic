namespace Supercell.Magic.Servers.Core.Database.Document
{
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class AccountDocument : CouchbaseDocument
    {
        private const string JSON_ATTRIBUTE_PASS_TOKEN = "passToken";
        private const string JSON_ATTRIBUTE_STATE = "state";
        private const string JSON_ATTRIBUTE_STATE_ARG = "stateArg";
        private const string JSON_ATTRIBUTE_RANK = "rankingType";
        private const string JSON_ATTRIBUTE_COUNTRY = "country";
        private const string JSON_ATTRIBUTE_FACEBOOK_ID = "facebookId";
        private const string JSON_ATTRIBUTE_GAMECENTER_ID = "gamecenterId";
        private const string JSON_ATTRIBUTE_CREATE_TIME = "createTime";
        private const string JSON_ATTRIBUTE_LAST_SESSION_TIME = "lastSessionTime";
        private const string JSON_ATTRIBUTE_SESSION_COUNT = "sessionCount";
        private const string JSON_ATTRIBUTE_PLAY_TIME_SECS = "playTimeSecs";

        public const string CHARS = "abcdefghijklmnopqrstuvwxyz0123456789";
        public const int PASS_TOKEN_LENGTH = 40;

        public string PassToken { get; private set; }
        public string Country { get; set; }
        public string FacebookId { get; set; }
        public string GamecenterId { get; set; }
        public int CreateTime { get; private set; }
        public int LastSessionTime { get; set; }
        public int SessionCount { get; set; }
        public int PlayTimeSeconds { get; set; }
        
        public AccountState State { get; set; }
        public AccountRankingType Rank { get; set; }

        public string StateArg { get; set; }

        public AccountDocument()
        {
        }

        public AccountDocument(LogicLong id) : base(id)
        {
        }

        public void Init()
        {
            char[] chars = new char[AccountDocument.PASS_TOKEN_LENGTH];

            for (int i = 0; i < AccountDocument.PASS_TOKEN_LENGTH; i++)
            {
                chars[i] = AccountDocument.CHARS[ServerCore.Random.Rand(AccountDocument.CHARS.Length)];
            }

            this.PassToken = new string(chars);
            this.State = AccountState.NORMAL;
            this.Rank = AccountRankingType.NORMAL;
            this.CreateTime = TimeUtil.GetTimestamp();
        }

        protected override void Save(LogicJSONObject jsonObject)
        {
            jsonObject.Put(AccountDocument.JSON_ATTRIBUTE_PASS_TOKEN, new LogicJSONString(this.PassToken));
            jsonObject.Put(AccountDocument.JSON_ATTRIBUTE_STATE, new LogicJSONNumber((int) this.State));

            if(this.State != AccountState.NORMAL && this.StateArg != null)
                jsonObject.Put(AccountDocument.JSON_ATTRIBUTE_STATE_ARG, new LogicJSONString(this.StateArg));

            jsonObject.Put(AccountDocument.JSON_ATTRIBUTE_RANK, new LogicJSONNumber((int) this.Rank));
            jsonObject.Put(AccountDocument.JSON_ATTRIBUTE_COUNTRY, new LogicJSONString(this.Country));

            if (this.FacebookId != null)
                jsonObject.Put(AccountDocument.JSON_ATTRIBUTE_FACEBOOK_ID, new LogicJSONString(this.FacebookId));
            if (this.GamecenterId != null)
                jsonObject.Put(AccountDocument.JSON_ATTRIBUTE_GAMECENTER_ID, new LogicJSONString(this.GamecenterId));

            jsonObject.Put(AccountDocument.JSON_ATTRIBUTE_CREATE_TIME, new LogicJSONNumber(this.CreateTime));
            jsonObject.Put(AccountDocument.JSON_ATTRIBUTE_LAST_SESSION_TIME, new LogicJSONNumber(this.LastSessionTime));
            jsonObject.Put(AccountDocument.JSON_ATTRIBUTE_SESSION_COUNT, new LogicJSONNumber(this.SessionCount));
            jsonObject.Put(AccountDocument.JSON_ATTRIBUTE_PLAY_TIME_SECS, new LogicJSONNumber(this.PlayTimeSeconds));
        }

        protected override void Load(LogicJSONObject jsonObject)
        {
            this.PassToken = jsonObject.GetJSONString(AccountDocument.JSON_ATTRIBUTE_PASS_TOKEN).GetStringValue();
            this.State = (AccountState) jsonObject.GetJSONNumber(AccountDocument.JSON_ATTRIBUTE_STATE).GetIntValue();

            if (this.State != AccountState.NORMAL)
                this.StateArg = jsonObject.GetJSONString(AccountDocument.JSON_ATTRIBUTE_STATE_ARG)?.GetStringValue();

            this.Rank = (AccountRankingType) jsonObject.GetJSONNumber(AccountDocument.JSON_ATTRIBUTE_RANK).GetIntValue();
            this.Country = jsonObject.GetJSONString(AccountDocument.JSON_ATTRIBUTE_COUNTRY).GetStringValue();
            this.FacebookId = jsonObject.GetJSONString(AccountDocument.JSON_ATTRIBUTE_FACEBOOK_ID)?.GetStringValue();
            this.GamecenterId = jsonObject.GetJSONString(AccountDocument.JSON_ATTRIBUTE_GAMECENTER_ID)?.GetStringValue();
            this.CreateTime = jsonObject.GetJSONNumber(AccountDocument.JSON_ATTRIBUTE_CREATE_TIME).GetIntValue();
            this.LastSessionTime = jsonObject.GetJSONNumber(AccountDocument.JSON_ATTRIBUTE_LAST_SESSION_TIME).GetIntValue();
            this.SessionCount = jsonObject.GetJSONNumber(AccountDocument.JSON_ATTRIBUTE_SESSION_COUNT).GetIntValue();
            this.PlayTimeSeconds = jsonObject.GetJSONNumber(AccountDocument.JSON_ATTRIBUTE_PLAY_TIME_SECS).GetIntValue();
        }

        protected override void Encode(ByteStream stream)
        {
            stream.WriteString(this.PassToken);
            stream.WriteVInt((int) this.State);
            stream.WriteVInt((int) this.Rank);
            stream.WriteString(this.Country);
            stream.WriteString(this.FacebookId);
            stream.WriteString(this.GamecenterId);
            stream.WriteVInt(this.CreateTime);
            stream.WriteVInt(this.LastSessionTime);
            stream.WriteVInt(this.SessionCount);
            stream.WriteVInt(this.PlayTimeSeconds);
        }

        protected override void Decode(ByteStream stream)
        {
            this.PassToken = stream.ReadString(900000);
            this.State = (AccountState) stream.ReadVInt();
            this.Rank = (AccountRankingType) stream.ReadVInt();
            this.Country = stream.ReadString(900000);
            this.FacebookId = stream.ReadString(900000);
            this.GamecenterId = stream.ReadString(900000);
            this.CreateTime = stream.ReadVInt();
            this.LastSessionTime = stream.ReadVInt();
            this.SessionCount = stream.ReadVInt();
            this.PlayTimeSeconds = stream.ReadVInt();
        }
    }

    public enum AccountState
    {
        NORMAL,
        LOCKED,
        BANNED
    }

    public enum AccountRankingType
    {
        NORMAL,
        PREMIUM,
        ADMIN
    }
}