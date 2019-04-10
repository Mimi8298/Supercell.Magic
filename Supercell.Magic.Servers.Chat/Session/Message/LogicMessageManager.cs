namespace Supercell.Magic.Servers.Chat.Session.Message
{
    using System;

    using Supercell.Magic.Logic.Command.Debug;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Message.Chat;
    using Supercell.Magic.Logic.Message.Home;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Servers.Chat.Session;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class LogicMessageManager
    {
        private readonly ChatSession m_session;
        private DateTime m_previousGlobalChatMessageTime;

        public LogicMessageManager(ChatSession session)
        {
            this.m_session = session;
        }

        public void ReceiveMessage(PiranhaMessage message)
        {
            switch (message.GetMessageType())
            {
                case SendGlobalChatLineMessage.MESSAGE_TYPE:
                    this.OnSendGlobalChatLineMessageReceived((SendGlobalChatLineMessage) message);
                    break;
            }
        }

        private void OnSendGlobalChatLineMessageReceived(SendGlobalChatLineMessage message)
        {
            if (!this.CanSendGlobalChatMessage())
                return;

            string chatMessage = StringUtil.RemoveMultipleSpaces(message.RemoveMessage());

            if (chatMessage.Length > 0)
            {
                if (chatMessage.Length > 128)
                    chatMessage = chatMessage.Substring(0, 128);

                if (chatMessage.StartsWith("/op "))
                {
                    string[] args = chatMessage.Trim().Split(' ');

                    if (args.Length < 3)
                        return;

                    string commandType = args[1];
                    string commandName = args[2];

                    switch (commandType)
                    {
                        case "attack":
                        {
                            if (string.Equals(commandName, "me", StringComparison.InvariantCultureIgnoreCase))
                            {
                                this.m_session.SendMessage(new GameStartFakeAttackMessage
                                {
                                    AccountId = this.m_session.AccountId,
                                    ArgData = null
                                }, 9);
                            }
                            else if (commandName.StartsWith("#"))
                            {
                                LogicLong accountId = HashTagCodeGenerator.m_instance.ToId(commandName.ToUpperInvariant());

                                if (accountId != null)
                                {
                                    this.m_session.SendMessage(new GameStartFakeAttackMessage
                                    {
                                        AccountId = accountId,
                                        ArgData = null
                                    }, 9);
                                }
                            }
                            else if (string.Equals(commandName, "generate", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (args.Length >= 4)
                                {
                                    if (int.TryParse(args[3], out int id))
                                    {
                                        LogicGameObjectData gameObjectData = null;

                                        switch (id / 100)
                                        {
                                            case 0:
                                                LogicBuildingData buildingData =
                                                    (LogicBuildingData) LogicDataTables.GetDataById(GlobalID.CreateGlobalID((int) LogicDataType.BUILDING + 1, id), LogicDataType.BUILDING);

                                                if (buildingData.IsTownHall() || buildingData.IsTownHallVillage2())
                                                    return;
                                                if (!buildingData.IsEnabledInVillageType(0))
                                                    return;
                                                if (buildingData.IsLocked() && !buildingData.IsAllianceCastle())
                                                    return;

                                                gameObjectData = buildingData;
                                                break;
                                            case 1:
                                                gameObjectData = (LogicGameObjectData) LogicDataTables.GetDataById(GlobalID.CreateGlobalID((int) LogicDataType.TRAP + 1, id), LogicDataType.TRAP);
                                                break;
                                        }

                                        if (gameObjectData != null)
                                        {
                                            this.m_session.SendMessage(new GameStartFakeAttackMessage
                                            {
                                                AccountId = null,
                                                ArgData = gameObjectData
                                            }, 9);
                                        }
                                    }
                                }
                                else
                                {
                                    this.m_session.SendMessage(new GameStartFakeAttackMessage
                                    {
                                        AccountId = null,
                                        ArgData = null
                                    }, 9);
                                }
                            }

                            break;
                        }

                        case "village":
                            switch (commandName)
                            {
                                case "upgrade":
                                {
                                    AvailableServerCommandMessage availableServerCommandMessage = new AvailableServerCommandMessage();
                                    availableServerCommandMessage.SetServerCommand(new LogicDebugCommand(LogicDebugActionType.UPGRADE_ALL_BUILDINGS));
                                    this.m_session.SendPiranhaMessage(availableServerCommandMessage, 1);
                                    break;
                                }
                                case "obstacle":
                                {
                                    AvailableServerCommandMessage availableServerCommandMessage = new AvailableServerCommandMessage();
                                    availableServerCommandMessage.SetServerCommand(new LogicDebugCommand(LogicDebugActionType.REMOVE_OBSTACLES));
                                    this.m_session.SendPiranhaMessage(availableServerCommandMessage, 1);
                                    break;
                                }
                                case "preset":
                                    // TODO: Implement this.
                                    break;
                            }

                            break;
                        case "hero":
                            switch (commandName)
                            {
                                case "max":
                                {
                                    AvailableServerCommandMessage availableServerCommandMessage = new AvailableServerCommandMessage();
                                    availableServerCommandMessage.SetServerCommand(new LogicDebugCommand(LogicDebugActionType.SET_MAX_HERO_LEVELS));
                                    this.m_session.SendPiranhaMessage(availableServerCommandMessage, 1);
                                    return;
                                }
                                case "reset":
                                {
                                    AvailableServerCommandMessage availableServerCommandMessage = new AvailableServerCommandMessage();
                                    availableServerCommandMessage.SetServerCommand(new LogicDebugCommand(LogicDebugActionType.RESET_HERO_LEVELS));
                                    this.m_session.SendPiranhaMessage(availableServerCommandMessage, 1);
                                    return;
                                }
                            }

                            break;
                        case "unit":
                            switch (commandName)
                            {
                                case "max":
                                {
                                    AvailableServerCommandMessage availableServerCommandMessage = new AvailableServerCommandMessage();
                                    availableServerCommandMessage.SetServerCommand(new LogicDebugCommand(LogicDebugActionType.SET_MAX_UNIT_SPELL_LEVELS));
                                    this.m_session.SendPiranhaMessage(availableServerCommandMessage, 1);
                                    return;
                                }
                            }

                            break;
                    }
                }
                else
                {
                    ServerRequestManager.Create(new AvatarRequestMessage
                    {
                        AccountId = this.m_session.AccountId
                    }, ServerManager.GetDocumentSocket(9, this.m_session.AccountId)).OnComplete = args =>
                    {
                        if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
                        {
                            this.m_session.ChatInstance.PublishMessage(((AvatarResponseMessage)args.ResponseMessage).LogicClientAvatar, WordCensorUtil.FilterMessage(chatMessage));
                            this.m_previousGlobalChatMessageTime = DateTime.UtcNow;
                        }
                    };
                }
            }
        }
        
        private bool CanSendGlobalChatMessage()
        {
            return DateTime.UtcNow.Subtract(this.m_previousGlobalChatMessageTime).TotalSeconds >= 1d;
        }
    }
}