namespace Supercell.Magic.Tools.Client.Network
{
    using System;
    using Supercell.Magic.Logic.Message.Account;
    using Supercell.Magic.Logic.Message.Battle;
    using Supercell.Magic.Logic.Message.Home;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;
    using Supercell.Magic.Tools.Client.Helper;

    public class MessageManager
    {
        private readonly ServerConnection m_serverConnection;
        private readonly Messaging m_messaging;

        private float m_nextKeepAlive;
        private float m_nextClientTurn;

        private DateTime m_sendKeepAliveTime;

        public MessageManager(ServerConnection serverConnection, Messaging messaging)
        {
            this.m_serverConnection = serverConnection;
            this.m_messaging = messaging;
        }

        public void ReceiveMessage(PiranhaMessage piranhaMessage)
        {
            switch (piranhaMessage.GetMessageType())
            {
                case LoginFailedMessage.MESSAGE_TYPE:
                    this.OnLoginFailedMessageReceived((LoginFailedMessage) piranhaMessage);
                    break;
                case LoginOkMessage.MESSAGE_TYPE:
                    this.OnLoginOkMessageReceived((LoginOkMessage) piranhaMessage);
                    break;
                case KeepAliveServerMessage.MESSAGE_TYPE:
                    this.OnKeepAliveServerMessageReceived((KeepAliveServerMessage) piranhaMessage);
                    break;
            }
        }

        private void OnLoginFailedMessageReceived(LoginFailedMessage message)
        {
            this.m_serverConnection.SetState(ServerConnectionState.LOGIN_FAILED);

            switch (message.GetErrorCode())
            {
                case LoginFailedMessage.ErrorCode.DATA_VERSION:
                    ZLibHelper.DecompressInMySQLFormat(message.GetCompressedFingerprint(), out byte[] output);
                    ResourceManager.DownloadDataUpdate(LogicStringUtil.CreateString(output, 0, output.Length), message.GetContentUrl());
                    break;
                default:
                    Debugger.Warning("MessageManager.onLoginFailedMessageReceived: error code: " + message.GetErrorCode());
                    break;
            }
        }

        private void OnLoginOkMessageReceived(LoginOkMessage message)
        {
            this.m_serverConnection.SetState(ServerConnectionState.LOGGED);
            this.m_serverConnection.SetAccountInfo(message.GetAccountId(), message.GetPassToken());

            Debugger.Print(string.Format("MessageManager.onLoginOkMessageReceived: client logged (account: {0} passtoken: {1} server version: {2}", message.GetAccountId(), message.GetPassToken(),
                                         message.GetServerMajorVersion() + "." + message.GetServerBuildVersion() + "." + message.GetContentVersion()));
        }

        private void OnKeepAliveServerMessageReceived(KeepAliveServerMessage message)
        {
            if (this.m_sendKeepAliveTime != DateTime.MinValue)
            {
                int ping = (int) DateTime.UtcNow.Subtract(this.m_sendKeepAliveTime).TotalMilliseconds;
                this.m_sendKeepAliveTime = DateTime.MinValue;

                Debugger.Print("MessageManager.onKeepAliveServerMessageReceived: ping: " + ping + "ms");
            }
        }

        public void SendMessage(PiranhaMessage message)
        {
            if (message.GetMessageType() == EndClientTurnMessage.MESSAGE_TYPE || message.GetMessageType() == BattleEndClientTurnMessage.MESSAGE_TYPE)
            {
                this.m_nextClientTurn = 5f;
            }
            else if (message.GetMessageType() == KeepAliveMessage.MESSAGE_TYPE)
            {
                this.m_nextKeepAlive = 0.1f;

                if (this.m_sendKeepAliveTime == DateTime.MinValue)
                    this.m_sendKeepAliveTime = DateTime.UtcNow;
            }

            this.m_messaging.Send(message);
        }

        public void Update(float time)
        {
            this.m_nextClientTurn -= time;
            this.m_nextKeepAlive -= time;

            if (this.m_nextKeepAlive <= 0f)
                this.SendMessage(new KeepAliveMessage());
        }
    }
}