namespace Supercell.Magic.Logic.Message
{
    using Supercell.Magic.Logic.Message.Account;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Logic.Message.Alliance.Stream;
    using Supercell.Magic.Logic.Message.Alliance.War;
    using Supercell.Magic.Logic.Message.Avatar;
    using Supercell.Magic.Logic.Message.Avatar.Attack;
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Logic.Message.Battle;
    using Supercell.Magic.Logic.Message.Chat;
    using Supercell.Magic.Logic.Message.Facebook;
    using Supercell.Magic.Logic.Message.Friend;
    using Supercell.Magic.Logic.Message.Google;
    using Supercell.Magic.Logic.Message.Home;
    using Supercell.Magic.Logic.Message.League;
    using Supercell.Magic.Logic.Message.Scoring;
    using Supercell.Magic.Logic.Message.Security;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Message.Account;
    using Supercell.Magic.Titan.Message.Security;

    public class LogicMagicMessageFactory : LogicMessageFactory
    {
        public static readonly LogicMessageFactory Instance;

        static LogicMagicMessageFactory()
        {
            LogicMagicMessageFactory.Instance = new LogicMagicMessageFactory();
        }

        public override PiranhaMessage CreateMessageByType(int type)
        {
            PiranhaMessage message = null;

            if (type < 20000)
            {
                switch (type)
                {
                    case ClientCryptoErrorMessage.MESSAGE_TYPE:
                    {
                        message = new ClientCryptoErrorMessage();
                        break;
                    }

                    case ClientHelloMessage.MESSAGE_TYPE:
                    {
                        message = new ClientHelloMessage();
                        break;
                    }

                    case LoginMessage.MESSAGE_TYPE:
                    {
                        message = new LoginMessage();
                        break;
                    }

                    case KeepAliveMessage.MESSAGE_TYPE:
                    {
                        message = new KeepAliveMessage();
                        break;
                    }

                    case SetDeviceTokenMessage.MESSAGE_TYPE:
                    {
                        message = new SetDeviceTokenMessage();
                        break;
                    }

                    case ResetAccountMessage.MESSAGE_TYPE:
                    {
                        message = new ResetAccountMessage();
                        break;
                    }

                    case ReportUserMessage.MESSAGE_TYPE:
                    {
                        message = new ReportUserMessage();
                        break;
                    }

                    case AccountSwitchedMessage.MESSAGE_TYPE:
                    {
                        message = new AccountSwitchedMessage();
                        break;
                    }

                    case UnlockAccountMessage.MESSAGE_TYPE:
                    {
                        message = new UnlockAccountMessage();
                        break;
                    }

                    case AppleBillingRequestMessage.MESSAGE_TYPE:
                    {
                        message = new AppleBillingRequestMessage();
                        break;
                    }

                    case ChangeAvatarNameMessage.MESSAGE_TYPE:
                    {
                        message = new ChangeAvatarNameMessage();
                        break;
                    }

                    case AcceptFriendMessage.MESSAGE_TYPE:
                    {
                        message = new AcceptFriendMessage();
                        break;
                    }

                    case AddFriendMessage.MESSAGE_TYPE:
                    {
                        message = new AddFriendMessage();
                        break;
                    }

                    case AskForFriendListMessage.MESSAGE_TYPE:
                    {
                        message = new AskForFriendListMessage();
                        break;
                    }

                    case RemoveFriendMessage.MESSAGE_TYPE:
                    {
                        message = new RemoveFriendMessage();
                        break;
                    }

                    case StartFriendLiveSpectateMessage.MESSAGE_TYPE:
                    {
                        message = new StartFriendLiveSpectateMessage();
                        break;
                    }

                    case InboxOpenedMessage.MESSAGE_TYPE:
                    {
                        message = new InboxOpenedMessage();
                        break;
                    }

                    case GoHomeMessage.MESSAGE_TYPE:
                    {
                        message = new GoHomeMessage();
                        break;
                    }

                    case EndClientTurnMessage.MESSAGE_TYPE:
                    {
                        message = new EndClientTurnMessage();
                        break;
                    }

                    case CancelMatchmakingMessage.MESSAGE_TYPE:
                    {
                        message = new CancelMatchmakingMessage();
                        break;
                    }

                    case AttackHomeMessage.MESSAGE_TYPE:
                    {
                        message = new AttackHomeMessage();
                        break;
                    }

                    case StartFriendlyChallengeSpectateMessage.MESSAGE_TYPE:
                    {
                        message = new StartFriendlyChallengeSpectateMessage();
                        break;
                    }

                    case ScoutFriendlyBattleMessage.MESSAGE_TYPE:
                    {
                        message = new ScoutFriendlyBattleMessage();
                        break;
                    }

                    case VisitHomeMessage.MESSAGE_TYPE:
                    {
                        message = new VisitHomeMessage();
                        break;
                    }

                    case HomeBattleReplayMessage.MESSAGE_TYPE:
                    {
                        message = new HomeBattleReplayMessage();
                        break;
                    }

                    case AttackMatchedHomeMessage.MESSAGE_TYPE:
                    {
                        message = new AttackMatchedHomeMessage();
                        break;
                    }

                    case AcceptFriendlyBattleMessage.MESSAGE_TYPE:
                    {
                        message = new AcceptFriendlyBattleMessage();
                        break;
                    }

                    case CancelChallengeMessage.MESSAGE_TYPE:
                    {
                        message = new CancelChallengeMessage();
                        break;
                    }

                    case AttackNpcMessage.MESSAGE_TYPE:
                    {
                        message = new AttackNpcMessage();
                        break;
                    }

                    case DuelNpcMessage.MESSAGE_TYPE:
                    {
                        message = new DuelNpcMessage();
                        break;
                    }

                    case BindGoogleServiceAccountMessage.MESSAGE_TYPE:
                    {
                        message = new BindGoogleServiceAccountMessage();
                        break;
                    }

                    case BindFacebookAccountMessage.MESSAGE_TYPE:
                    {
                        message = new BindFacebookAccountMessage();
                        break;
                    }

                    case CreateAllianceMessage.MESSAGE_TYPE:
                    {
                        message = new CreateAllianceMessage();
                        break;
                    }

                    case AskForAllianceDataMessage.MESSAGE_TYPE:
                    {
                        message = new AskForAllianceDataMessage();
                        break;
                    }

                    case AskForJoinableAlliancesListMessage.MESSAGE_TYPE:
                    {
                        message = new AskForJoinableAlliancesListMessage();
                        break;
                    }

                    case JoinAllianceMessage.MESSAGE_TYPE:
                    {
                        message = new JoinAllianceMessage();
                        break;
                    }

                    case ChangeAllianceMemberRoleMessage.MESSAGE_TYPE:
                    {
                        message = new ChangeAllianceMemberRoleMessage();
                        break;
                    }

                    case LeaveAllianceMessage.MESSAGE_TYPE:
                    {
                        message = new LeaveAllianceMessage();
                        break;
                    }

                    case DonateAllianceUnitMessage.MESSAGE_TYPE:
                    {
                        message = new DonateAllianceUnitMessage();
                        break;
                    }

                    case ChatToAllianceStreamMessage.MESSAGE_TYPE:
                    {
                        message = new ChatToAllianceStreamMessage();
                        break;
                    }

                    case ChangeAllianceSettingsMessage.MESSAGE_TYPE:
                    {
                        message = new ChangeAllianceSettingsMessage();
                        break;
                    }

                    case RequestJoinAllianceMessage.MESSAGE_TYPE:
                    {
                        message = new RequestJoinAllianceMessage();
                        break;
                    }

                    case RespondToAllianceJoinRequestMessage.MESSAGE_TYPE:
                    {
                        message = new RespondToAllianceJoinRequestMessage();
                        break;
                    }

                    case SendAllianceInvitationMessage.MESSAGE_TYPE:
                    {
                        message = new SendAllianceInvitationMessage();
                        break;
                    }

                    case JoinAllianceUsingInvitationMessage.MESSAGE_TYPE:
                    {
                        message = new JoinAllianceUsingInvitationMessage();
                        break;
                    }

                    case SearchAlliancesMessage.MESSAGE_TYPE:
                    {
                        message = new SearchAlliancesMessage();
                        break;
                    }

                    case AskForAvatarProfileMessage.MESSAGE_TYPE:
                    {
                        message = new AskForAvatarProfileMessage();
                        break;
                    }

                    case AskForAllianceBookmarksFullDataMessage.MESSAGE_TYPE:
                    {
                        message = new AskForAllianceBookmarksFullDataMessage();
                        break;
                    }

                    case AddAllianceBookmarkMessage.MESSAGE_TYPE:
                    {
                        message = new AddAllianceBookmarkMessage();
                        break;
                    }

                    case RemoveAllianceBookmarkMessage.MESSAGE_TYPE:
                    {
                        message = new RemoveAllianceBookmarkMessage();
                        break;
                    }

                    case AskForAllianceRankingListMessage.MESSAGE_TYPE:
                    {
                        message = new AskForAllianceRankingListMessage();
                        break;
                    }

                    case AskForAvatarRankingListMessage.MESSAGE_TYPE:
                    {
                        message = new AskForAvatarRankingListMessage();
                        break;
                    }

                    case AskForAvatarLocalRankingListMessage.MESSAGE_TYPE:
                    {
                        message = new AskForAvatarLocalRankingListMessage();
                        break;
                    }

                    case AskForAvatarLastSeasonRankingListMessage.MESSAGE_TYPE:
                    {
                        message = new AskForAvatarLastSeasonRankingListMessage();
                        break;
                    }

                    case AskForAvatarDuelLastSeasonRankingListMessage.MESSAGE_TYPE:
                    {
                        message = new AskForAvatarDuelLastSeasonRankingListMessage();
                        break;
                    }

                    case RemoveAvatarStreamEntryMessage.MESSAGE_TYPE:
                    {
                        message = new RemoveAvatarStreamEntryMessage();
                        break;
                    }

                    case AskForLeagueMemberListMessage.MESSAGE_TYPE:
                    {
                        message = new AskForLeagueMemberListMessage();
                        break;
                    }

                    case BattleEndClientTurnMessage.MESSAGE_TYPE:
                    {
                        message = new BattleEndClientTurnMessage();
                        break;
                    }

                    case AvatarNameCheckRequestMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarNameCheckRequestMessage();
                        break;
                    }

                    case SendGlobalChatLineMessage.MESSAGE_TYPE:
                    {
                        message = new SendGlobalChatLineMessage();
                        break;
                    }

                    case Village2AttackStartSpectateMessage.MESSAGE_TYPE:
                    {
                        message = new Village2AttackStartSpectateMessage();
                        break;
                    }
                }
            }
            else
            {
                switch (type)
                {
                    case SetEncryptionMessage.MESSAGE_TYPE:
                    {
                        message = new ExtendedSetEncryptionMessage();
                        break;
                    }

                    case ServerHelloMessage.MESSAGE_TYPE:
                    {
                        message = new ServerHelloMessage();
                        break;
                    }

                    case LoginFailedMessage.MESSAGE_TYPE:
                    {
                        message = new LoginFailedMessage();
                        break;
                    }

                    case LoginOkMessage.MESSAGE_TYPE:
                    {
                        message = new LoginOkMessage();
                        break;
                    }

                    case FriendListMessage.MESSAGE_TYPE:
                    {
                        message = new FriendListMessage();
                        break;
                    }

                    case FriendListUpdateMessage.MESSAGE_TYPE:
                    {
                        message = new FriendListUpdateMessage();
                        break;
                    }

                    case KeepAliveServerMessage.MESSAGE_TYPE:
                    {
                        message = new KeepAliveServerMessage();
                        break;
                    }

                    case AddFriendErrorMessage.MESSAGE_TYPE:
                    {
                        message = new AddFriendErrorMessage();
                        break;
                    }

                    case ReportUserStatusMessage.MESSAGE_TYPE:
                    {
                        message = new ReportUserStatusMessage();
                        break;
                    }

                    case UnlockAccountOkMessage.MESSAGE_TYPE:
                    {
                        message = new UnlockAccountOkMessage();
                        break;
                    }

                    case UnlockAccountFailedMessage.MESSAGE_TYPE:
                    {
                        message = new UnlockAccountFailedMessage();
                        break;
                    }

                    case AppleBillingProcessedByServerMessage.MESSAGE_TYPE:
                    {
                        message = new AppleBillingProcessedByServerMessage();
                        break;
                    }

                    case ShutdownStartedMessage.MESSAGE_TYPE:
                    {
                        message = new ShutdownStartedMessage();
                        break;
                    }

                    case PersonalBreakStartedMessage.MESSAGE_TYPE:
                    {
                        message = new PersonalBreakStartedMessage();
                        break;
                    }

                    case FacebookAccountBoundMessage.MESSAGE_TYPE:
                    {
                        message = new FacebookAccountBoundMessage();
                        break;
                    }

                    case AvatarNameChangeFailedMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarNameChangeFailedMessage();
                        break;
                    }

                    case AvatarOnlineStatusUpdated.MESSAGE_TYPE:
                    {
                        message = new AvatarOnlineStatusUpdated();
                        break;
                    }

                    case AvatarOnlineStatusListMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarOnlineStatusListMessage();
                        break;
                    }

                    case AllianceOnlineStatusUpdatedMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceOnlineStatusUpdatedMessage();
                        break;
                    }

                    case GoogleServiceAccountBoundMessage.MESSAGE_TYPE:
                    {
                        message = new GoogleServiceAccountBoundMessage();
                        break;
                    }

                    case GoogleServiceAccountAlreadyBoundMessage.MESSAGE_TYPE:
                    {
                        message = new GoogleServiceAccountAlreadyBoundMessage();
                        break;
                    }

                    case AvatarNameCheckResponseMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarNameCheckResponseMessage();
                        break;
                    }

                    case AcceptFriendErrorMessage.MESSAGE_TYPE:
                    {
                        message = new AcceptFriendErrorMessage();
                        break;
                    }

                    case OwnHomeDataMessage.MESSAGE_TYPE:
                    {
                        message = new OwnHomeDataMessage();
                        break;
                    }

                    case AttackHomeFailedMessage.MESSAGE_TYPE:
                    {
                        message = new AttackHomeFailedMessage();
                        break;
                    }

                    case OutOfSyncMessage.MESSAGE_TYPE:
                    {
                        message = new OutOfSyncMessage();
                        break;
                    }

                    case EnemyHomeDataMessage.MESSAGE_TYPE:
                    {
                        message = new EnemyHomeDataMessage();
                        break;
                    }

                    case AvailableServerCommandMessage.MESSAGE_TYPE:
                    {
                        message = new AvailableServerCommandMessage();
                        break;
                    }

                    case WaitingToGoHomeMessage.MESSAGE_TYPE:
                    {
                        message = new WaitingToGoHomeMessage();
                        break;
                    }

                    case VisitedHomeDataMessage.MESSAGE_TYPE:
                    {
                        message = new VisitedHomeDataMessage();
                        break;
                    }

                    case HomeBattleReplayDataMessage.MESSAGE_TYPE:
                    {
                        message = new HomeBattleReplayDataMessage();
                        break;
                    }

                    case ServerErrorMessage.MESSAGE_TYPE:
                    {
                        message = new ServerErrorMessage();
                        break;
                    }

                    case HomeBattleReplayFailedMessage.MESSAGE_TYPE:
                    {
                        message = new HomeBattleReplayFailedMessage();
                        break;
                    }

                    case LiveReplayHeaderMessage.MESSAGE_TYPE:
                    {
                        message = new LiveReplayHeaderMessage();
                        break;
                    }

                    case LiveReplayDataMessage.MESSAGE_TYPE:
                    {
                        message = new LiveReplayDataMessage();
                        break;
                    }

                    case ChallengeFailedMessage.MESSAGE_TYPE:
                    {
                        message = new ChallengeFailedMessage();
                        break;
                    }

                    case VisitFailedMessage.MESSAGE_TYPE:
                    {
                        message = new VisitFailedMessage();
                        break;
                    }

                    case AttackSpectatorCountMessage.MESSAGE_TYPE:
                    {
                        message = new AttackSpectatorCountMessage();
                        break;
                    }

                    case LiveReplayEndMessage.MESSAGE_TYPE:
                    {
                        message = new LiveReplayEndMessage();
                        break;
                    }

                    case LiveReplayFailedMessage.MESSAGE_TYPE:
                    {
                        message = new LiveReplayFailedMessage();
                        break;
                    }

                    case NpcDataMessage.MESSAGE_TYPE:
                    {
                        message = new NpcDataMessage();
                        break;
                    }

                    case AllianceDataMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceDataMessage();
                        break;
                    }

                    case AllianceJoinFailedMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceJoinFailedMessage();
                        break;
                    }

                    case JoinableAllianceListMessage.MESSAGE_TYPE:
                    {
                        message = new JoinableAllianceListMessage();
                        break;
                    }

                    case AllianceListMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceListMessage();
                        break;
                    }

                    case AllianceStreamMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceStreamMessage();
                        break;
                    }

                    case AllianceStreamEntryMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceStreamEntryMessage();
                        break;
                    }

                    case AllianceStreamEntryRemovedMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceStreamEntryRemovedMessage();
                        break;
                    }

                    case AllianceJoinRequestOkMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceJoinRequestOkMessage();
                        break;
                    }

                    case AllianceJoinRequestFailedMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceJoinRequestFailedMessage();
                        break;
                    }

                    case AllianceInvitationSendFailedMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceInvitationSendFailedMessage();
                        break;
                    }

                    case AllianceInvitationSentOkMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceInvitationSentOkMessage();
                        break;
                    }

                    case AllianceFullEntryUpdateMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceFullEntryUpdateMessage();
                        break;
                    }

                    case AllianceWarSearchDataMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceWarSearchDataMessage();
                        break;
                    }

                    case AllianceWarDataMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceWarDataMessage();
                        break;
                    }

                    case AllianceCreateFailedMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceCreateFailedMessage();
                        break;
                    }

                    case AvatarProfileMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarProfileMessage();
                        break;
                    }

                    case AllianceWarFullEntryMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceWarFullEntryMessage();
                        break;
                    }

                    case AllianceWarDataFailedMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceWarDataFailedMessage();
                        break;
                    }

                    case AllianceWarHistoryMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceWarHistoryMessage();
                        break;
                    }

                    case AvatarProfileFailedMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarProfileFailedMessage();
                        break;
                    }

                    case BookmarksListMessage.MESSAGE_TYPE:
                    {
                        message = new BookmarksListMessage();
                        break;
                    }

                    case AllianceBookmarksFullDataMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceBookmarksFullDataMessage();
                        break;
                    }

                    case Village2AttackEntryListMessage.MESSAGE_TYPE:
                    {
                        message = new Village2AttackEntryListMessage();
                        break;
                    }

                    case Village2AttackEntryUpdateMessage.MESSAGE_TYPE:
                    {
                        message = new Village2AttackEntryUpdateMessage();
                        break;
                    }

                    case Village2AttackEntryAddedMessage.MESSAGE_TYPE:
                    {
                        message = new Village2AttackEntryAddedMessage();
                        break;
                    }

                    case Village2AttackEntryRemovedMessage.MESSAGE_TYPE:
                    {
                        message = new Village2AttackEntryRemovedMessage();
                        break;
                    }

                    case AllianceRankingListMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceRankingListMessage();
                        break;
                    }

                    case AllianceLocalRankingListMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceLocalRankingListMessage();
                        break;
                    }

                    case AvatarRankingListMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarRankingListMessage();
                        break;
                    }

                    case AvatarLocalRankingListMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarLocalRankingListMessage();
                        break;
                    }

                    case AvatarLastSeasonRankingListMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarLastSeasonRankingListMessage();
                        break;
                    }

                    case AvatarDuelLocalRankingListMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarDuelLocalRankingListMessage();
                        break;
                    }

                    case AvatarDuelLastSeasonRankingListMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarDuelLastSeasonRankingListMessage();
                        break;
                    }

                    case AvatarDuelRankingListMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarDuelRankingListMessage();
                        break;
                    }

                    case AvatarStreamMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarStreamMessage();
                        break;
                    }

                    case AvatarStreamEntryMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarStreamEntryMessage();
                        break;
                    }

                    case AvatarStreamEntryRemovedMessage.MESSAGE_TYPE:
                    {
                        message = new AvatarStreamEntryRemovedMessage();
                        break;
                    }

                    case LeagueMemberListMessage.MESSAGE_TYPE:
                    {
                        message = new LeagueMemberListMessage();
                        break;
                    }

                    case GlobalChatLineMessage.MESSAGE_TYPE:
                    {
                        message = new GlobalChatLineMessage();
                        break;
                    }

                    case AllianceWarEventMessage.MESSAGE_TYPE:
                    {
                        message = new AllianceWarEventMessage();
                        break;
                    }

                    case FriendlyScoutHomeDataMessage.MESSAGE_TYPE:
                    {
                        message = new FriendlyScoutHomeDataMessage();
                        break;
                    }

                    case Village2AttackAvatarDataMessage.MESSAGE_TYPE:
                    {
                        message = new Village2AttackAvatarDataMessage();
                        break;
                    }

                    case AttackEventMessage.MESSAGE_TYPE:
                    {
                        message = new AttackEventMessage();
                        break;
                    }

                    case TitanDisconnectedMessage.MESSAGE_TYPE:
                    {
                        message = new DisconnectedMessage();
                        break;
                    }

                    case CryptoErrorMessage.MESSAGE_TYPE:
                    {
                        message = new CryptoErrorMessage();
                        break;
                    }
                }
            }

            return message;
        }
    }
}