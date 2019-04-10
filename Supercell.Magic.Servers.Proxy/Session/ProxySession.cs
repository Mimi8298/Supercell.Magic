namespace Supercell.Magic.Servers.Proxy.Session
{
    using System;
    using Couchbase;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Session;
    using Supercell.Magic.Servers.Core.Settings;
    using Supercell.Magic.Servers.Core.Util;

    using Supercell.Magic.Servers.Proxy.Network;
    using Supercell.Magic.Titan.Util;

    public class ProxySession : ServerSession
    {
        public ClientConnection ClientConnection { get; }
        
        private int m_chatUnbanTime;
        private bool m_started;

        private readonly DateTime m_startSessionTime;

        public ProxySession(long sessionId, ClientConnection clientConnection, AccountDocument account) : base(sessionId, account.Id, clientConnection.Location)
        {
            this.ClientConnection = clientConnection;
            this.m_chatUnbanTime = -1; // TODO: Implement this.
            this.m_startSessionTime = DateTime.UtcNow;
        }

        public override void Destruct()
        {
            for (int i = 0; i < EnvironmentSettings.SERVER_TYPE_COUNT; i++)
            {
                if (this.m_sockets[i] != null)
                    this.SendMessage(new StopServerSessionMessage(), i);
            }

            ServerProxy.SessionDatabase.DeleteIfEquals(this.AccountId, this.Id.ToString());
            
            this.UpdatePlayTimeSeconds();
            base.Destruct();
        }

        private async void UpdatePlayTimeSeconds()
        {
            IOperationResult<string> getResult = await ServerProxy.AccountDatabase.Get(this.AccountId);

            if (getResult.Success)
            {
                AccountDocument accountDocument = CouchbaseDocument.Load<AccountDocument>(getResult.Value);

                accountDocument.SessionCount += 1;
                accountDocument.PlayTimeSeconds += (int) DateTime.UtcNow.Subtract(this.m_startSessionTime).TotalSeconds;

                IOperationResult<string> updateResult = await ServerProxy.AccountDatabase.Update(this.AccountId, CouchbaseDocument.Save(accountDocument), getResult.Cas);

                if (!updateResult.Success)
                {
                    this.UpdatePlayTimeSeconds();
                }
            }
        }
        
        public void SetSocket(ServerSocket socket, BindServerSocketRequestMessage requestMessage = null)
        {
            if (this.m_destructed)
                return;

            int serverType = socket.ServerType;
            int serverId = socket.ServerId;
            
            if (this.m_sockets[serverType] != null)
                this.SendMessage(new StopServerSessionMessage(), serverType);

            this.m_sockets[serverType] = socket;

            if (serverType == 1)
                return;
            if (!this.m_started && socket.ServerType != 9)
                Logging.Warning("ProxySession.setSocket called but session did not start.");

            LogicArrayList<int> serverTypeList = new LogicArrayList<int>();
            LogicArrayList<int> serverIdList = new LogicArrayList<int>();

            for (int i = 0; i < EnvironmentSettings.SERVER_TYPE_COUNT; i++)
            {
                ServerSocket serverSocket = this.m_sockets[i];

                if (serverSocket != null)
                {
                    serverTypeList.Add(serverSocket.ServerType);
                    serverIdList.Add(serverSocket.ServerId);
                }
            }

            this.SendMessage(new StartServerSessionMessage
            {
                AccountId = this.AccountId,
                Country = this.Country,
                ServerSocketTypeList = serverTypeList,
                ServerSocketIdList = serverIdList,
                BindRequestMessage = requestMessage
            }, serverType);

            for (int i = 2; i < EnvironmentSettings.SERVER_TYPE_COUNT; i++)
            {
                if (i != serverType && this.m_sockets[serverType] != null)
                {
                    this.SendMessage(new UpdateSocketServerSessionMessage
                    {
                        ServerType = serverType,
                        ServerId = serverId
                    }, i);
                }
            }
        }

        public void UnbindServer(int serverType)
        {
            if (this.m_sockets[serverType] != null)
            {
                this.SendMessage(new StopServerSessionMessage(), serverType);
                this.m_sockets[serverType] = null;

                for (int i = 2; i < EnvironmentSettings.SERVER_TYPE_COUNT; i++)
                {
                    if (i != serverType && this.m_sockets[serverType] != null)
                    {
                        this.SendMessage(new UpdateSocketServerSessionMessage
                        {
                            ServerType = serverType,
                            ServerId = -1
                        }, i);
                    }
                }
            }
        }

        public ServerSocket GetServer(int type)
        {
            return this.m_sockets[type];
        }
        
        public void Update()
        {
            if (this.m_started)
            {
                if (this.m_sockets[6] == null)
                {
                    if (this.m_chatUnbanTime == -1 || TimeUtil.GetTimestamp() > this.m_chatUnbanTime)
                    {
                        ServerSocket chatSocket = ServerManager.GetNextSocket(6);

                        if (chatSocket != null)
                        {
                            this.SetSocket(chatSocket);
                        }
                    }
                }
            }
        }

        public void SetStarted()
        {
            this.m_started = true;
        }
    }
}