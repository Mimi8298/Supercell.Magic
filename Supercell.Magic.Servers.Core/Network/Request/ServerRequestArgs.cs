namespace Supercell.Magic.Servers.Core.Network.Request
{
    using System;
    using Supercell.Magic.Servers.Core.Network.Message.Request;

    public class ServerRequestArgs
    {
        public delegate void CompleteHandler(ServerRequestArgs args);

        public CompleteHandler OnComplete { get; set; } = args => { };
        public ServerResponseMessage ResponseMessage { get; private set; }

        public ServerRequestError ErrorCode { get; private set; }

        internal DateTime ExpireTime { get; set; }

        private bool m_completed;

        public ServerRequestArgs(int timeout)
        {
            this.ExpireTime = DateTime.UtcNow.AddSeconds(timeout);
        }

        internal void Abort()
        {
            if (!this.m_completed)
            {
                this.m_completed = true;
                this.ErrorCode = ServerRequestError.Aborted;
                this.OnComplete(this);
            }
        }

        internal void SetResponseMessage(ServerResponseMessage message)
        {
            if (!this.m_completed)
            {
                this.m_completed = true;
                this.ResponseMessage = message;
                this.ErrorCode = ServerRequestError.Success;
                this.OnComplete(this);
            }
        }
    }

    public enum ServerRequestError
    {
        Success,
        Aborted
    }
}