namespace Supercell.Magic.Servers.Chat.Logic
{
    using System;
    using System.Collections.Generic;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Chat;
    using Supercell.Magic.Servers.Chat.Session;

    public class ChatInstance
    {
        private const int CAPACITY = 100;
        private readonly Dictionary<long, ChatSession> m_sessions;

        public ChatInstance()
        {
            this.m_sessions = new Dictionary<long, ChatSession>(ChatInstance.CAPACITY);
        }

        public void Add(ChatSession session)
        {
            if (this.IsFull())
                throw new Exception("ChatInstance.add: instance is full");
            this.m_sessions.Add(session.Id, session);
        }

        public void Remove(ChatSession session)
        {
            if (!this.m_sessions.Remove(session.Id))
                throw new Exception("ChatInstance.remove: session is not in instance");
        }

        public bool IsFull()
        {
            return this.m_sessions.Count >= ChatInstance.CAPACITY;
        }

        public void PublishMessage(LogicClientAvatar logicClientAvatar, string message)
        {
            GlobalChatLineMessage globalChatLineMessage = new GlobalChatLineMessage();

            globalChatLineMessage.SetMessage(message);
            globalChatLineMessage.SetAvatarName(logicClientAvatar.GetName());
            globalChatLineMessage.SetAvatarExpLevel(logicClientAvatar.GetExpLevel());
            globalChatLineMessage.SetAvatarLeagueType(logicClientAvatar.GetLeagueType());
            globalChatLineMessage.SetAvatarId(logicClientAvatar.GetId());
            globalChatLineMessage.SetHomeId(logicClientAvatar.GetCurrentHomeId());

            if (logicClientAvatar.IsInAlliance())
            {
                globalChatLineMessage.SetAllianceId(logicClientAvatar.GetAllianceId());
                globalChatLineMessage.SetAllianceName(logicClientAvatar.GetAllianceName());
                globalChatLineMessage.SetAllianceBadgeId(logicClientAvatar.GetAllianceBadgeId());
            }

            foreach (ChatSession session in this.m_sessions.Values)
            {
                session.SendPiranhaMessage(globalChatLineMessage, 1);
            }
        }
    }
}