namespace Supercell.Magic.Servers.Search.Network.Message
{
    using System;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Logic.Message.Avatar;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Servers.Search.Logic;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class SearchMessageManager : ServerMessageManager
    {
        public override void OnReceiveAccountMessage(ServerAccountMessage message)
        {
            throw new NotSupportedException();
        }

        public override void OnReceiveRequestMessage(ServerRequestMessage message)
        {
            throw new NotSupportedException();
        }
        
        public override void OnReceiveSessionMessage(ServerSessionMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.FORWARD_LOGIC_REQUEST_MESSAGE:
                    SearchMessageManager.OnForwardLogicRequestMessageReceived((ForwardLogicRequestMessage) message);
                    break;
                case ServerMessageType.SEND_ALLIANCE_BOOKMARKS_FULL_DATA_TO_CLIENT:
                    SearchMessageManager.OnSendAllianceBookmarksFullDataToClientMessageReceived((SendAllianceBookmarksFullDataToClientMessage) message);
                    break;
            }
        }

        private static void OnForwardLogicRequestMessageReceived(ForwardLogicRequestMessage message)
        {
            PiranhaMessage logicMessage = LogicMagicMessageFactory.Instance.CreateMessageByType(message.MessageType);

            if (logicMessage == null)
                throw new Exception("logicMessage should not be NULL!");

            logicMessage.GetByteStream().SetByteArray(message.MessageBytes, message.MessageLength);
            logicMessage.SetMessageVersion(message.MessageVersion);
            logicMessage.Decode();

            if (!logicMessage.IsServerToClientMessage())
            {
                switch (logicMessage.GetMessageType())
                {
                    case AskForJoinableAlliancesListMessage.MESSAGE_TYPE:
                        ServerRequestManager.Create(new AvatarRequestMessage
                        {
                            AccountId = message.AccountId
                        }, ServerManager.GetDocumentSocket(9, message.AccountId), 5).OnComplete = args =>
                        {
                            SearchMessageManager.OnAvatarResponseMessageReceivedForJoinableAlliancesList(message, args);
                        };

                        break;

                    case SearchAlliancesMessage.MESSAGE_TYPE:
                        SearchMessageManager.OnSearchAlliancesMessageReceived((SearchAlliancesMessage) logicMessage, message);
                        break;
                }
            }
        }

        private static async void OnAvatarResponseMessageReceivedForJoinableAlliancesList(ForwardLogicRequestMessage requestMessage, ServerRequestArgs args)
        {
            int score = 0;
            int duelScore = 0;

            if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
            {
                AvatarResponseMessage responseMessage = (AvatarResponseMessage)args.ResponseMessage;

                score = responseMessage.LogicClientAvatar.GetScore();
                duelScore = responseMessage.LogicClientAvatar.GetDuelScore();
            }

            JoinableAllianceListMessage joinableAllianceListMessage = new JoinableAllianceListMessage();
            joinableAllianceListMessage.SetAlliances(await SearchManager.GetJoinableAlliancesList(score, duelScore));
            ServerMessageManager.SendMessage(SearchMessageManager.CreateForwardLogicMessage(joinableAllianceListMessage, requestMessage.SessionId),
                                             ServerManager.GetProxySocket(requestMessage.SessionId));
        }

        private static async void OnSearchAlliancesMessageReceived(SearchAlliancesMessage message, ForwardLogicRequestMessage requestMessage)
        {
            if (message.IsJoinableOnly())
            {
                ServerRequestManager.Create(new AvatarRequestMessage
                {
                    AccountId = requestMessage.AccountId
                }, ServerManager.GetDocumentSocket(9, requestMessage.AccountId), 5).OnComplete = async args =>
                {
                    LogicClientAvatar playerAvatar = null;

                    if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
                        playerAvatar = ((AvatarResponseMessage) args.ResponseMessage).LogicClientAvatar;

                    AllianceListMessage allianceListMessage = new AllianceListMessage();

                    allianceListMessage.SetAlliances(await SearchManager.SearchAlliances(message, playerAvatar));
                    allianceListMessage.SetBookmarkList(new LogicArrayList<LogicLong>());
                    allianceListMessage.SetSearchString(message.GetSearchString());

                    ServerMessageManager.SendMessage(SearchMessageManager.CreateForwardLogicMessage(allianceListMessage, requestMessage.SessionId),
                                                     ServerManager.GetProxySocket(requestMessage.SessionId));
                };
            }
            else
            {
                AllianceListMessage allianceListMessage = new AllianceListMessage();

                allianceListMessage.SetAlliances(await SearchManager.SearchAlliances(message, null));
                allianceListMessage.SetBookmarkList(new LogicArrayList<LogicLong>());
                allianceListMessage.SetSearchString(message.GetSearchString());

                ServerMessageManager.SendMessage(SearchMessageManager.CreateForwardLogicMessage(allianceListMessage, requestMessage.SessionId),
                                                 ServerManager.GetProxySocket(requestMessage.SessionId));
            }
        }

        private static ForwardLogicMessage CreateForwardLogicMessage(PiranhaMessage piranhaMessage, long sessionId)
        {
            if (piranhaMessage.GetEncodingLength() == 0)
                piranhaMessage.Encode();

            return new ForwardLogicMessage
            {
                SessionId = sessionId,
                MessageType = piranhaMessage.GetMessageType(),
                MessageVersion = (short)piranhaMessage.GetMessageVersion(),
                MessageLength = piranhaMessage.GetEncodingLength(),
                MessageBytes = piranhaMessage.GetMessageBytes()
            };
        }

        private static async void OnSendAllianceBookmarksFullDataToClientMessageReceived(SendAllianceBookmarksFullDataToClientMessage message)
        {
            AllianceBookmarksFullDataMessage allianceBookmarksFullDataMessage = new AllianceBookmarksFullDataMessage();
            allianceBookmarksFullDataMessage.SetAlliances(await SearchManager.GetAllianceHeaderList(message.AllianceIds));
            ServerMessageManager.SendMessage(SearchMessageManager.CreateForwardLogicMessage(allianceBookmarksFullDataMessage, message.SessionId), ServerManager.GetProxySocket(message.SessionId));
        }

        public override void OnReceiveCoreMessage(ServerCoreMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.SERVER_PERFORMANCE:
                    ServerMessageManager.SendMessage(new ServerPerformanceDataMessage(), message.Sender);
                    break;
            }
        }
    }
}