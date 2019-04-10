namespace Supercell.Magic.Servers.Admin.Logic
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using Supercell.Magic.Servers.Admin.Attribute;
    using Supercell.Magic.Servers.Core;

    public static class AuthManager
    {
        private static ConcurrentDictionary<string, SessionEntry> m_sessions;
        private static Thread m_sessionUpdateThread;
        private static List<UserEntry> m_offlineUsers;

        public static void Init()
        {
            AuthManager.m_offlineUsers = new List<UserEntry>();
            AuthManager.m_offlineUsers.Add(new UserEntry("Mike", "n52uy3SjXCPqmMUG", UserRole.ADMIN));
            AuthManager.m_offlineUsers.Add(new UserEntry("Milo", "q8wNwtt3HfSqAvQu", UserRole.ADMIN));

            AuthManager.m_sessions = new ConcurrentDictionary<string, SessionEntry>();
            AuthManager.m_sessionUpdateThread = new Thread(AuthManager.Update);
            AuthManager.m_sessionUpdateThread.Start();
        }

        private static void Update()
        {
            while (true)
            {
                DateTime utc = DateTime.UtcNow;

                foreach (SessionEntry entry in AuthManager.m_sessions.Values)
                {
                    if (utc.Subtract(entry.UpdateTime).TotalMinutes >= 30d || utc.Subtract(entry.CreateTime).TotalDays >= 1d)
                        AuthManager.CloseSession(entry.Token);
                }

                Thread.Sleep(10000);
            }
        }

        public static bool OpenSession(string user, string password, out string token)
        {
            if (AuthManager.LoadUser(user, password, out UserEntry userEntry))
            {
                if (userEntry.CurrentSession != null)
                    AuthManager.CloseSession(userEntry.CurrentSession.Token);
                SessionEntry session = new SessionEntry(userEntry, token = AuthManager.GenerateToken());

                bool success = AuthManager.m_sessions.TryAdd(session.Token, session);
                if (success) userEntry.CurrentSession = session;
                Logging.Print(string.Format("AuthManager.openSession: user: {0} password: {1} token: {2}", user, password, session.Token));
                return success;
            }

            token = null;
            return false;
        }

        public static bool CloseSession(string token)
        {
            Logging.Print(string.Format("AuthManager.closeSession: token: {0}", token));
            return AuthManager.m_sessions.Remove(token, out _);
        }

        public static bool IsOpenSession(string token)
        {
            return AuthManager.TryGetSession(token, out _);
        }

        public static bool TryGetSession(string token, out SessionEntry entry)
        {
            bool success = AuthManager.m_sessions.TryGetValue(token, out entry);
            if (success) entry.UpdateTime = DateTime.UtcNow;
            return success;
        }
        
        private static bool LoadUser(string user, string password, out UserEntry entry)
        {
            for (int i = 0; i < AuthManager.m_offlineUsers.Count; i++)
            {
                entry = AuthManager.m_offlineUsers[i];

                if (entry.User == user && entry.Password == password)
                    return true;
            }

            entry = null;
            return false;
        }

        private static string GenerateToken()
        {
            return Guid.NewGuid().ToString("N");
        }
    }

    public class SessionEntry
    {
        public string Token { get; }
        public UserEntry User { get; }
        public DateTime CreateTime { get; }
        public DateTime UpdateTime { get; set; }

        public SessionEntry(UserEntry user, string token)
        {
            this.User = user;
            this.Token = token;
            this.CreateTime = DateTime.UtcNow;
            this.UpdateTime = this.CreateTime;
        }
    }

    public class UserEntry
    {
        public string User { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public SessionEntry CurrentSession { get; set; }

        public UserEntry(string user, string password, UserRole role)
        {
            this.User = user;
            this.Password = password;
            this.Role = role;
        }
    }

    public enum UserRole
    {
        NULL,
        DEFAULT,
        TESTER,
        MODERATOR,
        ADMIN
    }
}