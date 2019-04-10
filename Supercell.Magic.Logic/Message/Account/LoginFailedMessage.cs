namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class LoginFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20103;

        private LogicArrayList<string> m_contentUrlList;

        private bool m_bannedShowHelpshiftContact;

        private ErrorCode m_errorCode;
        private int m_endMaintenanceTime;

        private string m_resourceFingerprintContent;
        private string m_redirectDomain;
        private string m_reason;
        private string m_updateUrl;
        private string m_contentUrl;

        private byte[] m_compressedFingerprintData;

        public LoginFailedMessage() : this(9)
        {
            // LoginFailedMessage.
        }

        public LoginFailedMessage(short messageVersion) : base(messageVersion)
        {
            this.m_compressedFingerprintData = new byte[0];
        }

        public override void Decode()
        {
            base.Decode();

            this.m_errorCode = (ErrorCode) this.m_stream.ReadInt();
            this.m_resourceFingerprintContent = this.m_stream.ReadString(900000);
            this.m_redirectDomain = this.m_stream.ReadString(900000);
            this.m_contentUrl = this.m_stream.ReadString(900000);

            if (this.m_version >= 1)
            {
                this.m_updateUrl = this.m_stream.ReadString(900000);

                if (this.m_version >= 2)
                {
                    this.m_reason = this.m_stream.ReadString(900000);

                    if (this.m_version >= 3)
                    {
                        this.m_endMaintenanceTime = this.m_stream.ReadInt();

                        if (this.m_version >= 4)
                        {
                            this.m_bannedShowHelpshiftContact = this.m_stream.ReadBoolean();

                            if (this.m_version >= 5)
                            {
                                this.m_compressedFingerprintData = this.m_stream.ReadBytes(this.m_stream.ReadBytesLength(), 900000);

                                int contentUrlListSize = this.m_stream.ReadInt();

                                if (contentUrlListSize != -1)
                                {
                                    this.m_contentUrlList = new LogicArrayList<string>(contentUrlListSize);

                                    for (int i = 0; i < contentUrlListSize; i++)
                                    {
                                        this.m_contentUrlList.Add(this.m_stream.ReadString(900000));
                                    }
                                }

                                if (this.m_version >= 6)
                                {
                                    this.m_stream.ReadInt();

                                    if (this.m_version >= 7)
                                    {
                                        this.m_stream.ReadInt();

                                        if (this.m_version >= 8)
                                        {
                                            this.m_stream.ReadString(900000);

                                            if (this.m_version >= 9)
                                            {
                                                this.m_stream.ReadInt();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt((int) this.m_errorCode);
            this.m_stream.WriteString(this.m_resourceFingerprintContent);
            this.m_stream.WriteString(this.m_redirectDomain);
            this.m_stream.WriteString(this.m_contentUrl);
            this.m_stream.WriteString(this.m_updateUrl);
            this.m_stream.WriteString(this.m_reason);
            this.m_stream.WriteInt(this.m_endMaintenanceTime);
            this.m_stream.WriteBoolean(this.m_bannedShowHelpshiftContact);
            this.m_stream.WriteBytes(this.m_compressedFingerprintData, this.m_compressedFingerprintData.Length);

            if (this.m_contentUrlList != null)
            {
                this.m_stream.WriteInt(this.m_contentUrlList.Size());

                for (int i = 0; i < this.m_contentUrlList.Size(); i++)
                {
                    this.m_stream.WriteString(this.m_contentUrlList[i]);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }

            this.m_stream.WriteInt(0);
            this.m_stream.WriteInt(0);
            this.m_stream.WriteString(null);
            this.m_stream.WriteInt(-1);
        }

        public override short GetMessageType()
        {
            return LoginFailedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_resourceFingerprintContent = null;
            this.m_redirectDomain = null;
            this.m_reason = null;
            this.m_updateUrl = null;
            this.m_contentUrl = null;
            this.m_compressedFingerprintData = null;
            this.m_contentUrlList = null;
        }

        public LogicArrayList<string> GetContentUrlList()
        {
            return this.m_contentUrlList;
        }

        public void SetContentUrlList(LogicArrayList<string> value)
        {
            this.m_contentUrlList = value;
        }

        public bool IsBannedShowHelpshiftContact()
        {
            return this.m_bannedShowHelpshiftContact;
        }

        public void SetBannedShowHelpshiftContact(bool value)
        {
            this.m_bannedShowHelpshiftContact = value;
        }

        public ErrorCode GetErrorCode()
        {
            return this.m_errorCode;
        }

        public void SetErrorCode(ErrorCode value)
        {
            this.m_errorCode = value;
        }

        public int GetEndMaintenanceTime()
        {
            return this.m_endMaintenanceTime;
        }

        public void SetEndMaintenanceTime(int value)
        {
            this.m_endMaintenanceTime = value;
        }

        public string GetResourceFingerprintContent()
        {
            return this.m_resourceFingerprintContent;
        }

        public void SetResourceFingerprintContent(string value)
        {
            this.m_resourceFingerprintContent = value;
        }

        public string GetRedirectDomain()
        {
            return this.m_redirectDomain;
        }

        public void SetRedirectDomain(string value)
        {
            this.m_redirectDomain = value;
        }

        public string GetReason()
        {
            return this.m_reason;
        }

        public void SetReason(string value)
        {
            this.m_reason = value;
        }

        public string GetUpdateUrl()
        {
            return this.m_updateUrl;
        }

        public void SetUpdateUrl(string value)
        {
            this.m_updateUrl = value;
        }

        public string GetContentUrl()
        {
            return this.m_contentUrl;
        }

        public void SetContentUrl(string value)
        {
            this.m_contentUrl = value;
        }

        public byte[] GetCompressedFingerprint()
        {
            return this.m_compressedFingerprintData;
        }

        public void SetCompressedFingerprint(byte[] value)
        {
            this.m_compressedFingerprintData = value;
        }

        public enum ErrorCode
        {
            ACCOUNT_NOT_EXISTS = 1,
            DATA_VERSION = 7,
            CLIENT_VERSION = 8,
            REDIRECTION = 9,
            SERVER_MAINTENANCE = 10,
            BANNED = 11,
            PERSONAL_BREAK = 12,
            ACCOUNT_LOCKED = 13,
            WRONG_STORE = 15,
            VERSION_NOT_UP_TO_DATE_STORE_NOT_READY = 16,
            CHINESE_APP_STORE_CONFLICT_MESSAGE = 18,
            PERSONAL_BREAK_EXTENDED = 19,
            PERSONAL_BREAK_EXTENDED_FINAL = 20,
            PERSONAL_BREAK_FINAL = 21
        }
    }
}