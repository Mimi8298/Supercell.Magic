namespace Supercell.Magic.Servers.Game.Logic.Live
{
    using System;
    using System.Collections.Generic;
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Message.Home;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Session;

    using Supercell.Magic.Servers.Game.Session;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class LiveReplay
    {
        public const int SLOT_CAPACITY = 100;

        private bool m_initialized;
        private bool m_ended;
        private bool m_end;

        private readonly LogicLong m_id;
        private readonly LogicLong m_allianceId;
        private readonly LogicLong m_allianceStreamId;
        private readonly GameSession m_attackerSession;
        private readonly Dictionary<long, LiveReplaySpectatorEntry>[] m_spectatorList;
        private readonly LogicArrayList<LogicCommand> m_commands;

        private byte[] m_streamData;
        private int m_clientSubTick;
        private int m_serverSubTick;

        public LiveReplay(LogicLong id, LogicLong allianceId, LogicLong allianceStreamId, GameSession attackerSession)
        {
            this.m_id = id;
            this.m_allianceId = allianceId;
            this.m_allianceStreamId = allianceStreamId;
            this.m_attackerSession = attackerSession;
            this.m_spectatorList = new Dictionary<long, LiveReplaySpectatorEntry>[2];
            this.m_spectatorList[0] = new Dictionary<long, LiveReplaySpectatorEntry>();
            this.m_spectatorList[1] = new Dictionary<long, LiveReplaySpectatorEntry>();
            this.m_commands = new LogicArrayList<LogicCommand>();
        }

        public LogicLong GetId()
        {
            return this.m_id;
        }

        public bool IsInit()
        {
            return this.m_initialized;
        }

        public bool IsEnded()
        {
            return this.m_ended;
        }

        public void Init(byte[] streamData)
        {
            if(this.m_initialized)
                throw new Exception("LiveReplay.init: live already initialized!");

            this.m_streamData = streamData;
            this.m_initialized = true;
        }

        public void SetClientUpdate(int clientSubTick, LogicArrayList<LogicCommand> commands)
        {
            this.m_clientSubTick = clientSubTick;

            if (commands != null)
            {
                this.m_commands.AddAll(commands);
            }
        }

        public void SetEnd()
        {
            this.m_end = true;
        }

        public void Update(int ms)
        {
            if (this.m_initialized && !this.m_ended)
            {
                int totalSubTick = LogicTime.GetMSInTicks(ms);
                
                if (this.m_clientSubTick < this.m_serverSubTick + totalSubTick)
                {
                    if (!this.m_end)
                        return;

                    totalSubTick = this.m_clientSubTick - this.m_serverSubTick;
                    this.m_ended = true;
                }
                
                for (int i = 0; i < 2; i++)
                {
                    Dictionary<long, LiveReplaySpectatorEntry> spectators = this.m_spectatorList[i];

                    if (spectators.Count >= 1)
                    {
                        LiveReplayDataMessage liveReplayDataMessage = new LiveReplayDataMessage();

                        liveReplayDataMessage.SetServerSubTick(this.m_serverSubTick + totalSubTick);
                        liveReplayDataMessage.SetCommands(this.GetCommands(this.m_serverSubTick, this.m_serverSubTick + totalSubTick));

                        if (i == 0)
                        {
                            liveReplayDataMessage.SetViewerCount(this.m_spectatorList[0].Count);
                            liveReplayDataMessage.SetEnemyViewerCount(this.m_spectatorList[1].Count);
                        }
                        else
                        {
                            liveReplayDataMessage.SetViewerCount(this.m_spectatorList[1].Count);
                            liveReplayDataMessage.SetEnemyViewerCount(this.m_spectatorList[0].Count);
                        }

                        foreach (LiveReplaySpectatorEntry entry in spectators.Values)
                        {
                            entry.SendPiranhaMessageToProxy(liveReplayDataMessage);
                        }

                        if (this.m_ended)
                        {
                            foreach (LiveReplaySpectatorEntry entry in spectators.Values)
                            {
                                entry.SendPiranhaMessageToProxy(new LiveReplayEndMessage());
                            }
                        }
                    }
                }

                this.m_serverSubTick += totalSubTick;
            }
        }

        public bool IsFull()
        {
            return this.m_spectatorList[0].Count +
                   this.m_spectatorList[1].Count >= LiveReplay.SLOT_CAPACITY;
        }

        public bool ContainsSession(long sessionId, int slot)
        {
            return this.m_spectatorList[slot].ContainsKey(sessionId);
        }
        
        public void AddSpectator(long sessionId, int slot)
        {
            LiveReplaySpectatorEntry liveReplaySpectatorEntry = new LiveReplaySpectatorEntry(sessionId);
            LiveReplayHeaderMessage liveReplayHeaderMessage = new LiveReplayHeaderMessage();

            int serverSubTick = this.m_serverSubTick;

            liveReplayHeaderMessage.SetCompressedStreamHeaderJson(this.m_streamData);
            liveReplayHeaderMessage.SetCommands(this.GetCommands(0, serverSubTick));
            liveReplayHeaderMessage.SetServerSubTick(serverSubTick);
            liveReplaySpectatorEntry.SendPiranhaMessageToProxy(liveReplayHeaderMessage);

            this.m_spectatorList[slot].Add(sessionId, liveReplaySpectatorEntry);
            this.SendAttackSpectatorCountMessage();

            if (this.m_allianceId != null)
                this.SendSpectatorCountToStreamService();
        }

        public void RemoveSpectator(long sessionId, int slot)
        {
            if (this.m_spectatorList[slot].Remove(sessionId))
            {
                this.SendAttackSpectatorCountMessage();

                if (this.m_allianceId != null)
                    this.SendSpectatorCountToStreamService();
            }
        }

        private void SendAttackSpectatorCountMessage()
        {
            if (!this.m_attackerSession.IsDestructed())
            {
                AttackSpectatorCountMessage attackSpectatorCountMessage = new AttackSpectatorCountMessage();

                attackSpectatorCountMessage.SetViewerCount(this.m_spectatorList[0].Count);
                attackSpectatorCountMessage.SetEnemyViewerCount(this.m_spectatorList[1].Count);

                this.m_attackerSession.SendPiranhaMessage(attackSpectatorCountMessage, 1);
            }
        }

        private void SendSpectatorCountToStreamService()
        {
            ServerMessageManager.SendMessage(new AllianceChallengeSpectatorCountMessage
            {
                AccountId = this.m_allianceId,
                StreamId = this.m_allianceStreamId,
                Count = this.m_spectatorList[0].Count +
                        this.m_spectatorList[1].Count
            }, 11);
        }

        private LogicArrayList<LogicCommand> GetCommands(int minSubTick, int maxSubTick)
        {
            LogicArrayList<LogicCommand> commands = new LogicArrayList<LogicCommand>();

            for (int i = 0; i < this.m_commands.Size(); i++)
            {
                LogicCommand command = this.m_commands[i];

                if(command.GetExecuteSubTick() >= minSubTick && command.GetExecuteSubTick() < maxSubTick)
                    commands.Add(command);
            }

            return commands;
        }
    }

    public class LiveReplaySpectatorEntry
    {
        public long SessionId { get; }

        public LiveReplaySpectatorEntry(long sessionId)
        {
            this.SessionId = sessionId;
        }

        public void SendPiranhaMessageToProxy(PiranhaMessage piranhaMessage)
        {
            if (piranhaMessage.GetEncodingLength() == 0)
                piranhaMessage.Encode();

            ServerMessageManager.SendMessage(new ForwardLogicMessage
            {
                SessionId = this.SessionId,
                MessageType = piranhaMessage.GetMessageType(),
                MessageVersion = (short) piranhaMessage.GetMessageVersion(),
                MessageLength = piranhaMessage.GetEncodingLength(),
                MessageBytes = piranhaMessage.GetMessageBytes()
            }, ServerManager.GetProxySocket(this.SessionId));
        }
    }
}