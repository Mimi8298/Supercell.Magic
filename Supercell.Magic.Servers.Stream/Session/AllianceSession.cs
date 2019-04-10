namespace Supercell.Magic.Servers.Stream.Session
{
    using System;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Servers.Stream.Logic;
    using Supercell.Magic.Servers.Stream.Session.Message;
    using Supercell.Magic.Servers.Stream.Util;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Servers.Core.Session;

    public class AllianceSession : ServerSession
    {
        public LogicMessageManager LogicMessageManager { get; }
        public Alliance Alliance { get; private set; }
        public LogicClientAvatar LogicClientAvatar { get; private set; }

        public AllianceSession(StartServerSessionMessage message) : base(message)
        {
            this.LogicMessageManager = new LogicMessageManager(this);

            ServerRequestManager.Create(new AvatarRequestMessage
            {
                AccountId = this.AccountId
            }, ServerManager.GetDocumentSocket(9, this.AccountId)).OnComplete = this.OnAvatarReceived;
        }

        private void OnAvatarReceived(ServerRequestArgs args)
        {
            if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
            {
                this.LogicClientAvatar = ((AvatarResponseMessage) args.ResponseMessage).LogicClientAvatar;

                if (AllianceManager.TryGet(this.LogicClientAvatar.GetAllianceId(), out Alliance avatarAlliance) && avatarAlliance.Members.ContainsKey(this.AccountId))
                {
                    this.Alliance = avatarAlliance;
                    this.Alliance.AddOnlineMember(this.AccountId, this);

                    this.SendPiranhaMessage(this.Alliance.GetAllianceFulEntryUpdateMessage(), 1);
                    this.SendPiranhaMessage(this.Alliance.GetAllianceStreamMessage(), 1);

                    AllianceMemberUtil.SetLogicClientAvatarToAllianceMemberEntry(this.LogicClientAvatar, this.Alliance.Members[this.AccountId], this.Alliance);
                    AllianceManager.Save(this.Alliance);
                }
                else
                {
                    this.SendMessage(new StopServerSessionMessage(), 1);

                    ServerMessageManager.SendMessage(new AllianceLeavedMessage
                    {
                        AccountId = this.AccountId,
                        AllianceId = this.LogicClientAvatar.GetAllianceId()
                    }, 9);
                    AllianceSessionManager.Remove(this.Id);
                }
            }
            else
            {
                this.SendMessage(new StopServerSessionMessage(), 1);
                AllianceSessionManager.Remove(this.Id);
            }
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.Alliance != null)
                this.Alliance.RemoveOnlineMember(this.LogicClientAvatar.GetId(), this);
        }
    }
}