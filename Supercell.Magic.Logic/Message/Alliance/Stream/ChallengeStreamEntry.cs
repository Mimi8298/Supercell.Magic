namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using System;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class ChallengeStreamEntry : StreamEntry
    {
        private string m_message;
        private string m_battleLogJSON;

        private int m_spectatorCount;

        private bool m_started;
        private bool m_warLayout;

        private byte[] m_snapshotHomeJSON;

        private LogicLong m_liveReplayId;

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_message = stream.ReadString(900000);

            if (stream.ReadBoolean())
            {
                this.m_battleLogJSON = stream.ReadString(900000);
            }

            stream.ReadVInt();
            this.m_spectatorCount = stream.ReadVInt();
            this.m_started = stream.ReadBoolean();
            stream.ReadVInt();
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteString(this.m_message);

            if (this.m_battleLogJSON != null)
            {
                stream.WriteBoolean(true);
                stream.WriteString(this.m_message);
            }
            else
            {
                stream.WriteBoolean(false);
            }

            stream.WriteVInt(0);
            stream.WriteVInt(this.m_spectatorCount);
            stream.WriteBoolean(this.m_started);
            stream.WriteVInt(0);
        }

        public override StreamEntryType GetStreamEntryType()
        {
            return StreamEntryType.CHALLENGE;
        }

        public string GetMessage()
        {
            return this.m_message;
        }

        public void SetMessage(string value)
        {
            this.m_message = value;
        }

        public string GetBattleLogJSON()
        {
            return this.m_battleLogJSON;
        }

        public void SetBattleLogJSON(string value)
        {
            this.m_battleLogJSON = value;
        }


        public byte[] GetSnapshotHomeJSON()
        {
            return this.m_snapshotHomeJSON;
        }

        public int GetSnapshotHomeLengthJSON()
        {
            return this.m_snapshotHomeJSON.Length;
        }

        public void SetSnapshotHomeJSON(byte[] value)
        {
            this.m_snapshotHomeJSON = value;
        }

        public int GetSpectatorCount()
        {
            return this.m_spectatorCount;
        }

        public void SetSpectatorCount(int value)
        {
            this.m_spectatorCount = value;
        }

        public LogicLong GetLiveReplayId()
        {
            return this.m_liveReplayId;
        }

        public void SetLiveReplayId(LogicLong value)
        {
            this.m_liveReplayId = value;
        }

        public bool IsStarted()
        {
            return this.m_started;
        }

        public void SetStarted(bool started)
        {
            this.m_started = started;
        }

        public bool IsWarLayout()
        {
            return this.m_warLayout;
        }

        public void SetWarLayout(bool warLayout)
        {
            this.m_warLayout = warLayout;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("ReplayVersusBattleStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            this.m_message = jsonObject.GetJSONString("message").GetStringValue();
            this.m_snapshotHomeJSON = Convert.FromBase64String(jsonObject.GetJSONString("snapshot_home").GetStringValue());
            this.m_warLayout = jsonObject.GetJSONBoolean("war_layout").IsTrue();

            LogicJSONString battleLogString = jsonObject.GetJSONString("battleLog");

            if (battleLogString != null)
            {
                this.m_battleLogJSON = battleLogString.GetStringValue();
            }
        }

        public override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = new LogicJSONObject();

            base.Save(baseObject);

            jsonObject.Put("base", baseObject);
            jsonObject.Put("message", new LogicJSONString(this.m_message));
            jsonObject.Put("snapshot_home", new LogicJSONString(Convert.ToBase64String(this.m_snapshotHomeJSON)));
            jsonObject.Put("war_layout", new LogicJSONBoolean(this.m_warLayout));

            if (this.m_battleLogJSON != null)
            {
                jsonObject.Put("battleLog", new LogicJSONString(this.m_battleLogJSON));
            }
        }
    }
}