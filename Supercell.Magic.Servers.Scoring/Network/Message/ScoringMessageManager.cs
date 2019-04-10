namespace Supercell.Magic.Servers.Scoring.Network.Message
{
    using System;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Logic.Message.Avatar;
    using Supercell.Magic.Logic.Message.Scoring;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Servers.Scoring.Logic;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class ScoringMessageManager : ServerMessageManager
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
                    ScoringMessageManager.OnForwardLogicRequestMessageReceived((ForwardLogicRequestMessage) message);
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
                    case AskForAllianceRankingListMessage.MESSAGE_TYPE:
                        ScoringMessageManager.OnAskForAllianceRankingListMessageReceived((AskForAllianceRankingListMessage) logicMessage, message);
                        break;
                    case AskForAvatarRankingListMessage.MESSAGE_TYPE:
                        ScoringMessageManager.OnAskForAvatarRankingListMessageReceived((AskForAvatarRankingListMessage) logicMessage, message);
                        break;
                    case AskForAvatarLocalRankingListMessage.MESSAGE_TYPE:
                        ScoringMessageManager.OnAskForLocalAvatarRankingListMessageReceived((AskForAvatarLocalRankingListMessage) logicMessage, message);
                        break;
                    case AskForAvatarLastSeasonRankingListMessage.MESSAGE_TYPE:
                        ScoringMessageManager.OnAskForAvatarLastSeasonRankingListMessageReceived((AskForAvatarLastSeasonRankingListMessage) logicMessage, message);
                        break;
                    case AskForAvatarDuelLastSeasonRankingListMessage.MESSAGE_TYPE:
                        ScoringMessageManager.OnAskForAvatarDuelLastSeasonRankingListMessageReceived((AskForAvatarDuelLastSeasonRankingListMessage) logicMessage, message);
                        break;
                }
            }
        }

        private static void OnAskForAllianceRankingListMessageReceived(AskForAllianceRankingListMessage message, ServerSessionMessage requestMessage)
        {
            if (message.GetLocalRanking())
            {
                AllianceLocalRankingListMessage allianceLocalRankingListMessage = new AllianceLocalRankingListMessage();

                allianceLocalRankingListMessage.SetAllianceRankingList(new LogicArrayList<AllianceRankingEntry>(0));
                allianceLocalRankingListMessage.SetVillageType(message.GetVillageType());

                ServerMessageManager.SendMessage(ScoringMessageManager.CreateForwardLogicMessage(allianceLocalRankingListMessage, requestMessage.SessionId),
                                                 ServerManager.GetProxySocket(requestMessage.SessionId));
            }
            else
            {
                AllianceRankingListMessage allianceRankingListMessage = new AllianceRankingListMessage();

                allianceRankingListMessage.SetAllianceRankingList(ScoringManager.GetAllianceRankingList(message.GetVillageType(), message.RemoveAllianceId()));
                allianceRankingListMessage.SetDiamondPrizes(ScoringManager.GetDiamondPrizes());
                allianceRankingListMessage.SetNextEndTimeSeconds(ScoringManager.GetNextEndTimeSeconds());
                allianceRankingListMessage.SetVillageType(message.GetVillageType());

                ServerMessageManager.SendMessage(ScoringMessageManager.CreateForwardLogicMessage(allianceRankingListMessage, requestMessage.SessionId),
                                                 ServerManager.GetProxySocket(requestMessage.SessionId));
            }
        }

        private static void OnAskForAvatarRankingListMessageReceived(AskForAvatarRankingListMessage message, ServerSessionMessage requestMessage)
        {
            if (message.GetVillageType() == 1)
            {
                AvatarDuelRankingListMessage avatarDuelRankingListMessage = new AvatarDuelRankingListMessage();
                LogicLong avatarId = message.RemoveAvatarId();

                avatarDuelRankingListMessage.SetAvatarRankingList(ScoringManager.GetAvatarDuelRankingList(avatarId));
                avatarDuelRankingListMessage.SetNextEndTimeSeconds(ScoringManager.GetNextEndTimeSeconds());
                avatarDuelRankingListMessage.SetSeasonMonth(ScoringManager.GetSeasonMonth());
                avatarDuelRankingListMessage.SetSeasonYear(ScoringManager.GetSeasonYear());
                avatarDuelRankingListMessage.SetLastSeasonAvatarRankingList(ScoringManager.GetLastSeasonAvatarDuelRankingList(avatarId));
                avatarDuelRankingListMessage.SetLastSeasonMonth(ScoringManager.GetLastSeasonMonth());
                avatarDuelRankingListMessage.SetLastSeasonYear(ScoringManager.GetLastSeasonYear());

                ServerMessageManager.SendMessage(ScoringMessageManager.CreateForwardLogicMessage(avatarDuelRankingListMessage, requestMessage.SessionId),
                                                 ServerManager.GetProxySocket(requestMessage.SessionId));
            }
            else
            {
                AvatarRankingListMessage avatarRankingListMessage = new AvatarRankingListMessage();
                LogicLong avatarId = message.RemoveAvatarId();

                avatarRankingListMessage.SetAvatarRankingList(ScoringManager.GetAvatarRankingList(avatarId));
                avatarRankingListMessage.SetNextEndTimeSeconds(ScoringManager.GetNextEndTimeSeconds());
                avatarRankingListMessage.SetSeasonMonth(ScoringManager.GetSeasonMonth());
                avatarRankingListMessage.SetSeasonYear(ScoringManager.GetSeasonYear());
                avatarRankingListMessage.SetLastSeasonAvatarRankingList(ScoringManager.GetLastSeasonAvatarRankingList(avatarId));
                avatarRankingListMessage.SetLastSeasonMonth(ScoringManager.GetLastSeasonMonth());
                avatarRankingListMessage.SetLastSeasonYear(ScoringManager.GetLastSeasonYear());

                ServerMessageManager.SendMessage(ScoringMessageManager.CreateForwardLogicMessage(avatarRankingListMessage, requestMessage.SessionId),
                                                 ServerManager.GetProxySocket(requestMessage.SessionId));
            }
        }

        private static void OnAskForLocalAvatarRankingListMessageReceived(AskForAvatarLocalRankingListMessage message, ServerSessionMessage requestMessage)
        {
            AvatarLocalRankingListMessage avatarLocalRankingListMessage = new AvatarLocalRankingListMessage();
            avatarLocalRankingListMessage.SetAvatarRankingList(new LogicArrayList<AvatarRankingEntry>(0));
            ServerMessageManager.SendMessage(ScoringMessageManager.CreateForwardLogicMessage(avatarLocalRankingListMessage, requestMessage.SessionId),
                                             ServerManager.GetProxySocket(requestMessage.SessionId));
        }

        private static void OnAskForAvatarLastSeasonRankingListMessageReceived(AskForAvatarLastSeasonRankingListMessage message, ServerSessionMessage requestMessage)
        {
            AvatarLastSeasonRankingListMessage avatarLastSeasonRankingListMessage = new AvatarLastSeasonRankingListMessage();
            avatarLastSeasonRankingListMessage.SetAvatarRankingList(ScoringManager.GetLastSeasonAvatarRankingList(message.RemoveAvatarId()));
            avatarLastSeasonRankingListMessage.SetSeasonYear(ScoringManager.GetLastSeasonYear());
            avatarLastSeasonRankingListMessage.SetSeasonMonth(ScoringManager.GetLastSeasonMonth());
            ServerMessageManager.SendMessage(ScoringMessageManager.CreateForwardLogicMessage(avatarLastSeasonRankingListMessage, requestMessage.SessionId),
                                             ServerManager.GetProxySocket(requestMessage.SessionId));
        }

        private static void OnAskForAvatarDuelLastSeasonRankingListMessageReceived(AskForAvatarDuelLastSeasonRankingListMessage message, ServerSessionMessage requestMessage)
        {
            AvatarDuelLastSeasonRankingListMessage avatarDuelLastSeasonRankingListMessage = new AvatarDuelLastSeasonRankingListMessage();
            avatarDuelLastSeasonRankingListMessage.SetAvatarRankingList(ScoringManager.GetLastSeasonAvatarDuelRankingList(message.RemoveAvatarId()));
            avatarDuelLastSeasonRankingListMessage.SetSeasonYear(ScoringManager.GetLastSeasonYear());
            avatarDuelLastSeasonRankingListMessage.SetSeasonMonth(ScoringManager.GetLastSeasonMonth());
            ServerMessageManager.SendMessage(ScoringMessageManager.CreateForwardLogicMessage(avatarDuelLastSeasonRankingListMessage, requestMessage.SessionId),
                                             ServerManager.GetProxySocket(requestMessage.SessionId));
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

        public override void OnReceiveCoreMessage(ServerCoreMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.SERVER_PERFORMANCE:
                    ServerMessageManager.SendMessage(new ServerPerformanceDataMessage(), message.Sender);
                    break;
                case ServerMessageType.SCORING_SYNC:
                    ScoringManager.OnScoringSyncMessageReceived((ScoringSyncMessage) message);
                    break;
            }
        }
    }
}