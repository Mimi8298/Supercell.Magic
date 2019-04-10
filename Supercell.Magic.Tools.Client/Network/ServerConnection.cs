namespace Supercell.Magic.Tools.Client.Network
{
    using System.IO;
    using System.Reflection;
    using Supercell.Magic.Logic;
    using Supercell.Magic.Logic.Message.Account;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class ServerConnection
    {
        private Messaging m_messaging;
        private MessageManager m_messageManager;
        private AccountInfo m_accountInfo;

        private ServerConnectionState m_state;

        public ServerConnection(LogicLong accountId = null)
        {
            this.LoadAccount(accountId);
            this.Connect();
        }

        private void LoadAccount(LogicLong id)
        {
            if (id != null)
            {
                string path = string.Format("accounts/{0}-{1}", id.GetHigherInt(), id.GetLowerInt());

                if (File.Exists(path))
                {
                    LogicJSONObject jsonObject = LogicJSONParser.ParseObject(File.ReadAllText(path));

                    this.m_accountInfo = new AccountInfo
                    {
                        AccountId = new LogicLong(jsonObject.GetJSONNumber("id_high").GetIntValue(), jsonObject.GetJSONNumber("id_low").GetIntValue()),
                        PassToken = jsonObject.GetJSONString("passToken").GetStringValue()
                    };
                }
                else
                {
                    Debugger.Warning("ServerConnection.loadAccount: account doesn't exists!");
                }
            }
            else
            {
                this.m_accountInfo = new AccountInfo();
            }
        }

        public void Connect(string host = "127.0.0.1", int port = 9339)
        {
            if (this.m_messaging != null)
                this.m_messaging.Destruct();
            this.m_messaging = new Messaging();
            this.m_messaging.Connect(host, port);
            this.m_messageManager = new MessageManager(this, this.m_messaging);
            this.m_state = ServerConnectionState.CONNECT;
        }

        public void Update(float time)
        {
            switch (this.m_state)
            {
                case ServerConnectionState.CONNECT:
                    if (this.m_messaging.IsConnected())
                    {
                        this.m_state = ServerConnectionState.LOGIN;

                        LoginMessage loginMessage = new LoginMessage();

                        loginMessage.SetAccountId(this.m_accountInfo.AccountId);
                        loginMessage.SetPassToken(this.m_accountInfo.PassToken);
                        loginMessage.SetClientMajorVersion(LogicVersion.MAJOR_VERSION);
                        loginMessage.SetClientBuildVersion(LogicVersion.BUILD_VERSION);
                        loginMessage.SetResourceSha(ResourceManager.FINGERPRINT_SHA);
                        loginMessage.SetDevice(Assembly.GetExecutingAssembly().FullName);
                        loginMessage.SetScramblerSeed(this.m_messaging.GetScramblerSeed());

                        this.m_messaging.Send(loginMessage);
                    }
                    break;
                case ServerConnectionState.LOGIN:
                case ServerConnectionState.LOGGED:
                    while (this.m_messaging.TryDequeueReceiveMessage(out PiranhaMessage piranhaMessage))
                        this.m_messageManager.ReceiveMessage(piranhaMessage);
                    this.m_messageManager.Update(time);
                    this.m_messaging.SendQueue();

                    break;
            }
        }

        public void SetAccountInfo(LogicLong accountId, string passToken)
        {
            this.m_accountInfo.AccountId = accountId;
            this.m_accountInfo.PassToken = passToken;

            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("id_high", new LogicJSONNumber(accountId.GetHigherInt()));
            jsonObject.Put("id_low", new LogicJSONNumber(accountId.GetLowerInt()));
            jsonObject.Put("passToken", new LogicJSONString(passToken));

            Directory.CreateDirectory("accounts");
            File.WriteAllText(string.Format("accounts/{0}-{1}", accountId.GetHigherInt(), accountId.GetLowerInt()), LogicJSONParser.CreateJSONString(jsonObject));
        }

        public void SetState(ServerConnectionState state)
        {
            this.m_state = state;
        }
    }

    public class AccountInfo
    {
        public LogicLong AccountId { get; set; }
        public string PassToken { get; set; }

        public AccountInfo()
        {
            this.AccountId = new LogicLong();
        }
    }

    public enum ServerConnectionState
    {
        NULL,
        CONNECT,
        LOGIN,
        LOGIN_FAILED,
        LOGGED,
    }
}
