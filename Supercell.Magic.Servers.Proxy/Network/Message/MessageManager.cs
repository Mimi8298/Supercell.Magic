namespace Supercell.Magic.Servers.Proxy.Network.Message
{
    using System;
    using System.Collections.Generic;
    using Couchbase;
    using Couchbase.IO;
    using Supercell.Magic.Logic;
    using Supercell.Magic.Logic.Message;
    using Supercell.Magic.Logic.Message.Account;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Logic.Message.Avatar;
    using Supercell.Magic.Logic.Message.Facebook;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Servers.Core.Settings;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Servers.Proxy.Session;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Message.Security;

    using StackExchange.Redis;

    public class MessageManager
    {
        private readonly ClientConnection m_connection;

        private DateTime m_keepAliveTime;
        private Dictionary<short, DateTime> m_lastRequestPiranhaMessage;

        public MessageManager(ClientConnection connection)
        {
            this.m_connection = connection;
            this.m_keepAliveTime = DateTime.UtcNow;
            this.m_lastRequestPiranhaMessage = new Dictionary<short, DateTime>();
        }

        public void ReceiveMessage(PiranhaMessage message)
        {
            int messageType = message.GetMessageType();
            int messageServiceNodeType = message.GetServiceNodeType();

            ClientConnectionState state = this.m_connection.State;

            if (state != ClientConnectionState.LOGGED)
            {
                if (state == ClientConnectionState.DISCONNECTED)
                    return;
                if (messageType != ClientHelloMessage.MESSAGE_TYPE && messageType != LoginMessage.MESSAGE_TYPE &&
                    messageType != KeepAliveMessage.MESSAGE_TYPE && messageType != UnlockAccountMessage.MESSAGE_TYPE)
                    return;
            }

            Logging.Print("MessageManager.receiveMessage: " + message.GetType().Name);

            if (messageServiceNodeType == 1)
            {
                switch (messageType)
                {
                    case ClientHelloMessage.MESSAGE_TYPE:
                        this.ClientHelloMessageReceived((ClientHelloMessage) message);
                        break;
                    case LoginMessage.MESSAGE_TYPE:
                        this.LoginMessageReceived((LoginMessage) message);
                        break;
                    case KeepAliveMessage.MESSAGE_TYPE:
                        this.KeepAliveMessageReceived((KeepAliveMessage) message);
                        break;

                    case UnlockAccountMessage.MESSAGE_TYPE:
                        this.UnlockAccountMessageReceived((UnlockAccountMessage) message);
                        break;

                    case BindFacebookAccountMessage.MESSAGE_TYPE:
                        this.OnBindFacebookAccountMessageReceived((BindFacebookAccountMessage) message);
                        break;
                }
            }
            else
            {
                if (state != ClientConnectionState.LOGGED)
                    return;

                ProxySession session = this.m_connection.Session;

                if (session == null)
                    return;

                if (this.IsRequestPiranhaMessage(message))
                {
                    switch (messageType)
                    {
                        case AskForAvatarProfileMessage.MESSAGE_TYPE:
                            this.SendForwardLogicMessageRequestMessage(message, ServerManager.GetDocumentSocket(9, ((AskForAvatarProfileMessage)message).RemoveAvatarId()));
                            break;
                        case AskForAllianceDataMessage.MESSAGE_TYPE:
                            this.SendForwardLogicMessageRequestMessage(message, ServerManager.GetDocumentSocket(11, ((AskForAllianceDataMessage)message).RemoveAllianceId()));
                            break;
                    }
                }
                else if (messageServiceNodeType == 28 || messageServiceNodeType == 29)
                {
                    this.SendForwardLogicMessageRequestMessage(message, ServerManager.GetNextSocket(messageServiceNodeType));
                }
                else
                {
                    session.SendPiranhaMessage(message, messageServiceNodeType);
                }
            }
        }

        private bool IsRequestPiranhaMessage(PiranhaMessage message)
        {
            int messageType = message.GetMessageType();

            return messageType == AskForAvatarProfileMessage.MESSAGE_TYPE ||
                   messageType == AskForAllianceDataMessage.MESSAGE_TYPE;
        }

        private void ClientHelloMessageReceived(ClientHelloMessage message)
        {
            if (this.m_connection.State == ClientConnectionState.DEFAULT &&
                this.CheckClientVersion(message.GetMajorVersion(), message.GetBuildVersion(), null, message.GetContentHash(), message.GetDeviceType() == 2) &&
                this.CheckServerCapabilities())
            {
                Logging.Warning("MessageManager.clientHelloMessageReceived: pepper encryption not supported");
            }
        }

        private async void LoginMessageReceived(LoginMessage message)
        {
            if (this.m_connection.State == ClientConnectionState.DEFAULT &&
                this.CheckClientVersion(message.GetClientMajorVersion(), message.GetClientBuildVersion(), message.GetAppVersion(), message.GetResourceSha(), message.IsAndroidClient()) &&
                this.CheckServerCapabilities())
            {
                this.m_connection.Messaging.SetScramblerSeed(message.GetScramblerSeed());
                this.m_connection.SetState(ClientConnectionState.LOGIN);

                AccountDocument accountDocument;

                if (message.GetAccountId().IsZero() && message.GetPassToken() == null)
                {
                    IOperationResult<ulong> incrementSeedResult = await ServerProxy.AccountDatabase.IncrementSeed();

                    if (!incrementSeedResult.Success)
                    {
                        LoginFailedMessage loginFailedMessage = new LoginFailedMessage();
                        loginFailedMessage.SetErrorCode(LoginFailedMessage.ErrorCode.SERVER_MAINTENANCE);
                        loginFailedMessage.SetReason("Internal server error");
                        this.SendMessage(loginFailedMessage);
                        return;
                    }

                    accountDocument = new AccountDocument((long) incrementSeedResult.Value);
                    accountDocument.Init();
                    accountDocument.Country = this.m_connection.Location;

                    IOperationResult<string> createAccountResult = await ServerProxy.AccountDatabase.Insert((long) incrementSeedResult.Value, CouchbaseDocument.Save(accountDocument));

                    if (!createAccountResult.Success)
                    {
                        LoginFailedMessage loginFailedMessage = new LoginFailedMessage();
                        loginFailedMessage.SetErrorCode(LoginFailedMessage.ErrorCode.SERVER_MAINTENANCE);
                        loginFailedMessage.SetReason("Internal server error");
                        this.SendMessage(loginFailedMessage);
                        return;
                    }
                }
                else
                {
                    IOperationResult<string> getResult = await ServerProxy.AccountDatabase.Get(message.GetAccountId());

                    if (!getResult.Success)
                    {
                        if (getResult.Status == ResponseStatus.KeyNotFound)
                        {
                            LoginFailedMessage loginFailedMessage = new LoginFailedMessage();
                            loginFailedMessage.SetErrorCode(LoginFailedMessage.ErrorCode.ACCOUNT_NOT_EXISTS);
                            this.SendMessage(loginFailedMessage);
                        }
                        else
                        {
                            LoginFailedMessage loginFailedMessage = new LoginFailedMessage();
                            loginFailedMessage.SetErrorCode(LoginFailedMessage.ErrorCode.SERVER_MAINTENANCE);
                            loginFailedMessage.SetReason("Internal server error");
                            this.SendMessage(loginFailedMessage);
                        }

                        return;
                    }

                    accountDocument = CouchbaseDocument.Load<AccountDocument>(getResult.Value);

                    if (accountDocument.PassToken != message.GetPassToken())
                    {
                        LoginFailedMessage loginFailedMessage = new LoginFailedMessage();
                        loginFailedMessage.SetErrorCode(LoginFailedMessage.ErrorCode.ACCOUNT_NOT_EXISTS);
                        this.SendMessage(loginFailedMessage);
                        return;
                    }
                }

                if (accountDocument.State != AccountState.NORMAL)
                {
                    switch (accountDocument.State)
                    {
                        case AccountState.BANNED:
                        {
                            LoginFailedMessage loginFailedMessage = new LoginFailedMessage();
                            loginFailedMessage.SetErrorCode(LoginFailedMessage.ErrorCode.BANNED);
                            loginFailedMessage.SetReason(accountDocument.StateArg);
                            this.SendMessage(loginFailedMessage);
                            return;
                        }

                        case AccountState.LOCKED:
                        {
                            LoginFailedMessage loginFailedMessage = new LoginFailedMessage();
                            loginFailedMessage.SetErrorCode(LoginFailedMessage.ErrorCode.ACCOUNT_LOCKED);
                            this.SendMessage(loginFailedMessage);
                            return;
                        }
                    }
                }

                ProxySession session = ProxySessionManager.Create(this.m_connection, accountDocument);
                RedisValue prevSession = await ServerProxy.SessionDatabase.GetSet(accountDocument.Id, session.Id.ToString());

                if (!prevSession.IsNull)
                {
                    long prevSessionId = long.Parse(prevSession);

                    ServerMessageManager.SendMessage(new StopSessionMessage
                    {
                        Reason = 1,
                        SessionId = prevSessionId
                    }, ServerManager.GetProxySocket(prevSessionId));
                }

                session.SetSocket(ServerCore.Socket); // Proxy

                LoginOkMessage loginOkMessage = new LoginOkMessage();

                loginOkMessage.SetAccountId(accountDocument.Id);
                loginOkMessage.SetHomeId(accountDocument.Id);
                loginOkMessage.SetPassToken(accountDocument.PassToken);
                loginOkMessage.SetFacebookId(accountDocument.FacebookId);
                loginOkMessage.SetGamecenterId(accountDocument.GamecenterId);

                loginOkMessage.SetServerMajorVersion(LogicVersion.MAJOR_VERSION);
                loginOkMessage.SetServerBuildVersion(LogicVersion.BUILD_VERSION);
                loginOkMessage.SetContentVersion(ResourceManager.GetContentVersion());
                loginOkMessage.SetServerEnvironment(EnvironmentSettings.Environment);
                loginOkMessage.SetSessionCount(accountDocument.SessionCount);
                loginOkMessage.SetPlayTimeSeconds(accountDocument.PlayTimeSeconds);
                loginOkMessage.SetAccountCreatedDate(accountDocument.CreateTime.ToString());
                loginOkMessage.SetStartupCooldownSeconds(ServerProxy.GetStartupCooldownSeconds());
                loginOkMessage.SetRegion(this.m_connection.Location);

                loginOkMessage.SetFacebookAppId(ResourceSettings.FacebookAppId);
                loginOkMessage.SetGoogleServiceId(ResourceSettings.GoogleServiceId);

                loginOkMessage.SetContentUrlList(ResourceSettings.ContentUrlList);
                loginOkMessage.SetChronosContentUrlList(ResourceSettings.ChronosContentUrlList);

                this.SendMessage(loginOkMessage);
                this.m_connection.SetSession(session);
                this.m_connection.SetState(ClientConnectionState.LOGGED);

                if (this.m_connection.State == ClientConnectionState.LOGGED)
                {
                    accountDocument.SessionCount += 1;
                    accountDocument.LastSessionTime = TimeUtil.GetTimestamp();

                    ServerRequestManager.Create(new BindServerSocketRequestMessage
                    {
                        ServerType = 9,
                        ServerId = ServerManager.GetDocumentSocket(9, accountDocument.Id).ServerId,
                        SessionId = session.Id
                    }, ServerCore.Socket, 5).OnComplete = this.OnGameServerBound;

                    await ServerProxy.AccountDatabase.Update(accountDocument.Id, CouchbaseDocument.Save(accountDocument));
                }
                else
                {
                    this.m_connection.DestructSession();
                }
            }
        }

        private void OnGameServerBound(ServerRequestArgs args)
        {
            if (!this.m_connection.Destructed)
            {
                if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
                {
                    if (this.m_connection.State == ClientConnectionState.LOGGED)
                        this.m_connection.Session.SetStarted();
                }
                else
                {
                    ClientConnectionManager.Disconnect(this.m_connection, 0);
                }
            }
        }

        private void KeepAliveMessageReceived(KeepAliveMessage message)
        {
            this.m_keepAliveTime = DateTime.UtcNow;
            this.SendMessage(new KeepAliveServerMessage());
        }

        private async void UnlockAccountMessageReceived(UnlockAccountMessage message)
        {
            if (this.m_connection.State == ClientConnectionState.LOGIN_FAILED)
            {
                if (ServerStatus.Status == ServerStatusType.SHUTDOWN_STARTED || ServerStatus.Status == ServerStatusType.MAINTENANCE)
                {
                    UnlockAccountFailedMessage unlockAccountFailedMessage = new UnlockAccountFailedMessage();
                    unlockAccountFailedMessage.SetErrorCode(UnlockAccountFailedMessage.ErrorCode.SERVER_MAINTENANCE);
                    this.SendMessage(unlockAccountFailedMessage);
                    return;
                }

                IOperationResult<string> getResult = await ServerProxy.AccountDatabase.Get(message.GetAccountId());

                if (!getResult.Success)
                {
                    UnlockAccountFailedMessage unlockAccountFailedMessage = new UnlockAccountFailedMessage();
                    unlockAccountFailedMessage.SetErrorCode(UnlockAccountFailedMessage.ErrorCode.UNLOCK_UNAVAILABLE);
                    this.SendMessage(unlockAccountFailedMessage);
                    return;
                }

                AccountDocument accountDocument = CouchbaseDocument.Load<AccountDocument>(getResult.Value);

                if (accountDocument.PassToken != message.GetPassToken() || accountDocument.State != AccountState.LOCKED)
                {
                    UnlockAccountFailedMessage unlockAccountFailedMessage = new UnlockAccountFailedMessage();
                    unlockAccountFailedMessage.SetErrorCode(UnlockAccountFailedMessage.ErrorCode.UNLOCK_UNAVAILABLE);
                    this.SendMessage(unlockAccountFailedMessage);
                    return;
                }

                if (accountDocument.StateArg != null && !accountDocument.StateArg.Equals(message.GetUnlockCode(), StringComparison.InvariantCultureIgnoreCase))
                {
                    UnlockAccountFailedMessage unlockAccountFailedMessage = new UnlockAccountFailedMessage();
                    unlockAccountFailedMessage.SetErrorCode(UnlockAccountFailedMessage.ErrorCode.UNLOCK_FAILED);
                    this.SendMessage(unlockAccountFailedMessage);
                    return;
                }

                accountDocument.State = AccountState.NORMAL;
                accountDocument.StateArg = null;

                IOperationResult<string> updateResult = await ServerProxy.AccountDatabase.Update(accountDocument.Id, CouchbaseDocument.Save(accountDocument));

                if (!updateResult.Success)
                {
                    UnlockAccountFailedMessage unlockAccountFailedMessage = new UnlockAccountFailedMessage();
                    unlockAccountFailedMessage.SetErrorCode(UnlockAccountFailedMessage.ErrorCode.UNLOCK_UNAVAILABLE);
                    this.SendMessage(unlockAccountFailedMessage);
                    return;
                }

                UnlockAccountOkMessage unlockAccountOkMessage = new UnlockAccountOkMessage();
                unlockAccountOkMessage.SetAccountId(message.GetAccountId());
                unlockAccountOkMessage.SetPassToken(message.GetPassToken());
                this.SendMessage(unlockAccountOkMessage);
            }
        }

        private void OnBindFacebookAccountMessageReceived(BindFacebookAccountMessage message)
        {
            // TODO: Implement this.
            this.SendMessage(new FacebookAccountBoundMessage());
        }

        private void SendForwardLogicMessageRequestMessage(PiranhaMessage piranhaMessage, ServerSocket socket)
        {
            if (socket != null)
            {
                if (this.IsRequestPiranhaMessage(piranhaMessage) && !this.CanSendRequest(piranhaMessage.GetMessageType()))
                    return;

                ServerMessageManager.SendMessage(new ForwardLogicRequestMessage
                {
                    SessionId = this.m_connection.Session.Id,
                    AccountId = this.m_connection.Session.AccountId,
                    MessageType = piranhaMessage.GetMessageType(),
                    MessageVersion = (short)piranhaMessage.GetMessageVersion(),
                    MessageLength = piranhaMessage.GetEncodingLength(),
                    MessageBytes = piranhaMessage.GetMessageBytes()
                }, socket);
            }
        }

        private bool CanSendRequest(short messageType)
        {
            DateTime utc = DateTime.UtcNow;

            if (this.m_lastRequestPiranhaMessage.TryGetValue(messageType, out DateTime previousTime))
            {
                if (utc.Subtract(previousTime).TotalMilliseconds < 500d)
                    return false;
                this.m_lastRequestPiranhaMessage[messageType] = utc;
            }
            else
            {
                this.m_lastRequestPiranhaMessage.Add(messageType, utc);
            }

            return true;
        }
        
        public void SendMessage(PiranhaMessage message)
        {
            ClientConnectionState state = this.m_connection.State;

            if (state != ClientConnectionState.LOGGED)
            {
                if (state == ClientConnectionState.DISCONNECTED)
                    return;
                int messageType = message.GetMessageType();

                if (messageType != LoginFailedMessage.MESSAGE_TYPE &&
                    messageType != LoginOkMessage.MESSAGE_TYPE &&
                    messageType != UnlockAccountFailedMessage.MESSAGE_TYPE &&
                    messageType != UnlockAccountOkMessage.MESSAGE_TYPE)
                    return;

                if (messageType == LoginFailedMessage.MESSAGE_TYPE)
                    this.m_connection.SetState(ClientConnectionState.LOGIN_FAILED);
            }

            this.m_connection.Messaging.Send(message);
        }

        public bool IsAlive()
        {
            return DateTime.UtcNow.Subtract(this.m_keepAliveTime).TotalSeconds < 30d;
        }

        private bool CheckClientVersion(int majorVersion, int buildVersion, string appVersion, string resourceSha, bool androidClient)
        {
            if (majorVersion != LogicVersion.MAJOR_VERSION || buildVersion != LogicVersion.BUILD_VERSION || (appVersion != null && !EnvironmentSettings.IsSupportedAppVersion(appVersion)))
            {
                LoginFailedMessage loginFailedMessage = new LoginFailedMessage();

                loginFailedMessage.SetErrorCode(LoginFailedMessage.ErrorCode.CLIENT_VERSION);
                loginFailedMessage.SetUpdateUrl(ResourceSettings.GetAppStoreUrl(androidClient));

                this.SendMessage(loginFailedMessage);
                return false;
            }

            if (resourceSha != ResourceManager.FINGERPRINT_SHA)
            {
                LoginFailedMessage loginFailedMessage = new LoginFailedMessage();

                loginFailedMessage.SetErrorCode(LoginFailedMessage.ErrorCode.DATA_VERSION);
                loginFailedMessage.SetContentUrl(ResourceSettings.GetContentUrl());
                loginFailedMessage.SetContentUrlList(ResourceSettings.ContentUrlList);
                loginFailedMessage.SetCompressedFingerprint(ResourceManager.COMPRESSED_FINGERPRINT_DATA);

                this.SendMessage(loginFailedMessage);
                return false;
            }

            return true;
        }

        private bool CheckServerCapabilities()
        {
            if ((ServerStatus.Status == ServerStatusType.SHUTDOWN_STARTED ||
                 ServerStatus.Status == ServerStatusType.MAINTENANCE) && !EnvironmentSettings.IsDeveloperIP(this.m_connection.ClientIP.ToString()))
            {
                LoginFailedMessage loginFailedMessage = new LoginFailedMessage();
                loginFailedMessage.SetErrorCode(LoginFailedMessage.ErrorCode.SERVER_MAINTENANCE);
                loginFailedMessage.SetEndMaintenanceTime(LogicMath.Max(ServerStatus.Time + ServerStatus.NextTime - TimeUtil.GetTimestamp(), 0));
                this.SendMessage(loginFailedMessage);
                return false;
            }

            if (ProxySessionManager.Count >= EnvironmentSettings.Settings.Proxy.SessionCapacity)
            {
                LoginFailedMessage loginFailedMessage = new LoginFailedMessage();
                loginFailedMessage.SetErrorCode((LoginFailedMessage.ErrorCode) 1000);
                loginFailedMessage.SetReason("The servers are not able to connect you at this time. Try again in a few minutes.");
                this.SendMessage(loginFailedMessage);
                return false;
            }

            return true;
        }
    }
}