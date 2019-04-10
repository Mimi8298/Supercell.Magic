namespace Supercell.Magic.Servers.Game.Logic
{
    using Supercell.Magic.Logic.Command;
    using Supercell.Magic.Logic.Command.Server;
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Request.Stream;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Servers.Core.Util;

    using Supercell.Magic.Servers.Game.Session;
    using Supercell.Magic.Titan.Math;

    public class GameAvatar : GameDocument
    {
        public GameSession CurrentSession { get; set; }
        public GameHomeLock CurrentHomeLock { get; set; }

        public bool PendingAllianceJoinResponse { get; set; }

        public GameAvatar() : base() { }
        public GameAvatar(LogicLong id) : base(id) { }

        public bool HasServerCommandOfType(LogicCommandType type)
        {
            for (int i = 0; i < this.ServerCommands.Size(); i++)
            {
                if (this.ServerCommands[i].GetCommandType() == type)
                    return true;
            }

            return false;
        }

        public void AddRecentlyMatchedEnemy(LogicLong id)
        {
            if (this.RecentlyMatchedEnemies.Size() > 50)
                this.RecentlyMatchedEnemies.Remove(0);
            this.RecentlyMatchedEnemies.Add(new RecentlyEnemy(id, TimeUtil.GetTimestamp()));
        }

        public bool HasRecentlyMatchedWithEnemy(LogicLong id)
        {
            int timestamp = TimeUtil.GetTimestamp();

            for (int i = 0; i < this.RecentlyMatchedEnemies.Size(); i++)
            {
                RecentlyEnemy enemy = this.RecentlyMatchedEnemies[i];

                if (LogicLong.Equals(enemy.AvatarId, id) && timestamp - enemy.Timestamp <= 60 * 15)
                    return true;
            }

            return false;
        }

        public void AddServerCommand(LogicServerCommand serverCommand)
        {
            int id = -1;

            for (int i = 0; i < this.ServerCommands.Size(); i++)
            {
                if (this.ServerCommands[i].GetId() > id)
                {
                    id = this.ServerCommands[i].GetId();
                }
            }

            serverCommand.SetId(id + 1);
            this.ServerCommands.Add(serverCommand);

            if (this.CurrentSession != null && this.CurrentSession.GameState != null)
            {
                this.CurrentSession.SendMessage(new HomeServerCommandAllowedMessage
                {
                    ServerCommand = serverCommand
                }, 10);
            }
            else
            {
                GameAvatarManager.ExecuteServerCommandsInOfflineMode(this);
                GameAvatarManager.Save(this);
            }
        }

        public void AddAllianceBookmark(LogicLong allianceId)
        {
            int index = this.AllianceBookmarksList.IndexOf(allianceId);

            if (index == -1)
            {
                this.AllianceBookmarksList.Add(allianceId);
            }
        }

        public void RemoveAllianceBookmark(LogicLong allianceId)
        {
            int index = this.AllianceBookmarksList.IndexOf(allianceId);

            if (index != -1)
            {
                this.AllianceBookmarksList.Remove(index);
            }
        }

        private void AddAvatarStreamEntry(AvatarStreamEntry entry)
        {
            if (this.AvatarStreamList.Size() > 50)
                this.RemoveAvatarStreamEntry(this.AvatarStreamList[0]);
            this.AvatarStreamList.Add(entry.GetId());

            if (this.CurrentSession != null)
            {
                AvatarStreamEntryMessage avatarStreamEntryMessage = new AvatarStreamEntryMessage();
                avatarStreamEntryMessage.SetAvatarStreamEntry(entry);
                this.CurrentSession.SendPiranhaMessage(avatarStreamEntryMessage, 1);

                if (entry.IsNew())
                {
                    ServerMessageManager.SendMessage(new AvatarStreamSeenMessage
                    {
                        AccountId = entry.GetId()
                    }, 11);
                }
            }
            else
            {
                GameAvatar.Save(this);
            }
        }

        public void RemoveAvatarStreamEntry(LogicLong streamId)
        {
            int index = this.AvatarStreamList.IndexOf(streamId);

            if (index != -1)
            {
                this.AvatarStreamList.Remove(index);

                ServerMessageManager.SendMessage(new RemoveAvatarStreamMessage
                {
                    AccountId = streamId
                }, ServerManager.GetDocumentSocket(11, this.Id));

                if (this.CurrentSession != null)
                {
                    AvatarStreamEntryRemovedMessage avatarStreamEntryRemovedMessage = new AvatarStreamEntryRemovedMessage();
                    avatarStreamEntryRemovedMessage.SetStreamEntryId(streamId);
                    this.CurrentSession.SendPiranhaMessage(avatarStreamEntryRemovedMessage, 1);
                }
                else
                {
                    GameAvatar.Save(this);
                }
            }
        }

        public void OnAvatarStreamCreated(ServerRequestArgs args)
        {
            if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
                this.AddAvatarStreamEntry(((CreateAvatarStreamResponseMessage) args.ResponseMessage).Entry);
            else
                Logging.Warning("GameAvatar.onAvatarStreamCreated: The stream server " + args.ResponseMessage.Sender + " could not create a stream.");
        }
    }

    public class GameHomeLock
    {
        public LogicLong AttackerId { get; set; }
    }
}