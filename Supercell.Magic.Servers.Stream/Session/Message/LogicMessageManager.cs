namespace Supercell.Magic.Servers.Stream.Session.Message
{
    using System;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Command.Server;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Logic.Message.Alliance.Stream;
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Logic.Message.Home;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Stream.Logic;
    using Supercell.Magic.Servers.Stream.Session;
    using Supercell.Magic.Servers.Stream.Util;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Request;
    using Supercell.Magic.Servers.Core.Util;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class LogicMessageManager
    {
        private readonly AllianceSession m_session;
        private DateTime m_previousAllianceChatMessageTime;

        public LogicMessageManager(AllianceSession session)
        {
            this.m_session = session;
        }

        public void ReceiveMessage(PiranhaMessage message)
        {
            switch (message.GetMessageType())
            {
                case LeaveAllianceMessage.MESSAGE_TYPE:
                    this.OnLeaveAllianceMessageReceived((LeaveAllianceMessage) message);
                    break;
                case ChatToAllianceStreamMessage.MESSAGE_TYPE:
                    this.OnChatToAllianceStreamMessageReceived((ChatToAllianceStreamMessage) message);
                    break;
                case ChangeAllianceSettingsMessage.MESSAGE_TYPE:
                    this.OnChangeAllianceSettingsMessageReceived((ChangeAllianceSettingsMessage) message);
                    break;
                case ChangeAllianceMemberRoleMessage.MESSAGE_TYPE:
                    this.OnChangeAllianceMemberRoleMessageReceived((ChangeAllianceMemberRoleMessage) message);
                    break;
                case DonateAllianceUnitMessage.MESSAGE_TYPE:
                    this.OnDonateAllianceUnitMessageReceived((DonateAllianceUnitMessage) message);
                    break;
                case CancelChallengeMessage.MESSAGE_TYPE:
                    this.OnCancelChallengeMessageReceived((CancelChallengeMessage) message);
                    break;
                case ScoutFriendlyBattleMessage.MESSAGE_TYPE:
                    this.OnScoutFriendlyBattleMessageReceived((ScoutFriendlyBattleMessage) message);
                    break;
                case AcceptFriendlyBattleMessage.MESSAGE_TYPE:
                    this.OnAcceptFriendlyBattleMessageReceived((AcceptFriendlyBattleMessage) message);
                    break;
                case StartFriendlyChallengeSpectateMessage.MESSAGE_TYPE:
                    this.OnStartFriendlyChallengeSpectateMessageReceived((StartFriendlyChallengeSpectateMessage) message);
                    break;
                case RespondToAllianceJoinRequestMessage.MESSAGE_TYPE:
                    this.OnRespondToAllianceJoinRequestMessageReceived((RespondToAllianceJoinRequestMessage) message);
                    break;
                case SendAllianceInvitationMessage.MESSAGE_TYPE:
                    this.OnSendAllianceInvitationMessageReceived((SendAllianceInvitationMessage) message);
                    break;
            }
        }

        private void OnLeaveAllianceMessageReceived(LeaveAllianceMessage message)
        {
            if (this.m_session.Alliance != null)
            {
                Alliance alliance = this.m_session.Alliance;
                AllianceMemberEntry memberEntry = alliance.Members[this.m_session.AccountId];

                if (memberEntry.GetAllianceRole() == LogicAvatarAllianceRole.LEADER)
                {
                    AllianceMemberEntry higherMemberEntry = null;
                    LogicAvatarAllianceRole higherMemberRole = 0;

                    foreach (AllianceMemberEntry member in alliance.Members.Values)
                    {
                        if (member != memberEntry && (higherMemberEntry == null || !member.HasLowerRoleThan(higherMemberRole)))
                        {
                            higherMemberEntry = member;
                            higherMemberRole = member.GetAllianceRole();
                        }
                    }

                    if (higherMemberEntry != null)
                        alliance.ChangeMemberRole(higherMemberEntry, LogicAvatarAllianceRole.LEADER, memberEntry.GetAvatarId(), memberEntry.GetName());
                }

                AllianceEventStreamEntry allianceEventStreamEntry = new AllianceEventStreamEntry();
                AllianceStreamEntryUtil.SetSenderInfo(allianceEventStreamEntry, memberEntry);

                allianceEventStreamEntry.SetEventType(AllianceEventStreamEntryType.LEFT);
                allianceEventStreamEntry.SetEventAvatarId(memberEntry.GetAvatarId());
                allianceEventStreamEntry.SetEventAvatarName(memberEntry.GetName());

                StreamManager.Create(alliance.Id, allianceEventStreamEntry);

                alliance.AddStreamEntry(allianceEventStreamEntry);
                alliance.RemoveMember(this.m_session.AccountId);

                ServerMessageManager.SendMessage(new AllianceLeavedMessage
                {
                    AccountId = this.m_session.AccountId,
                    AllianceId = alliance.Id
                }, 9);

                AllianceManager.Save(alliance);
            }
        }

        public void OnChatToAllianceStreamMessageReceived(ChatToAllianceStreamMessage message)
        {
            if (!this.CanSendGlobalChatMessage())
                return;

            if (this.m_session.Alliance != null)
            {
                string chatMessage = message.RemoveMessage();

                if (string.IsNullOrEmpty(chatMessage))
                    return;
                if (chatMessage.Length > 128)
                    chatMessage = chatMessage.Substring(0, 128);

                AllianceMemberEntry memberEntry = this.m_session.Alliance.Members[this.m_session.AccountId];
                ChatStreamEntry chatStreamEntry = new ChatStreamEntry();
                AllianceStreamEntryUtil.SetSenderInfo(chatStreamEntry, memberEntry);

                chatStreamEntry.SetMessage(WordCensorUtil.FilterMessage(chatMessage));

                StreamManager.Create(this.m_session.Alliance.Id, chatStreamEntry);
                this.m_session.Alliance.AddStreamEntry(chatStreamEntry);
                AllianceManager.Save(this.m_session.Alliance);
            }
        }

        private void OnChangeAllianceSettingsMessageReceived(ChangeAllianceSettingsMessage message)
        {
            if (this.m_session.Alliance != null)
            {
                this.m_session.Alliance.SetAllianceSettings(message.GetAllianceDescription(), (AllianceType) message.GetAllianceType(), message.GetAllianceBadgeId(), message.GetRequiredScore(),
                                                            message.GetRequiredDuelScore(), message.GetWarFrequency(), message.GetOriginData(), message.IsPublicWarLog(), message.IsAmicalWarEnabled());

                AllianceMemberEntry memberEntry = this.m_session.Alliance.Members[this.m_session.AccountId];
                AllianceEventStreamEntry allianceEventStreamEntry = new AllianceEventStreamEntry();
                AllianceStreamEntryUtil.SetSenderInfo(allianceEventStreamEntry, memberEntry);

                allianceEventStreamEntry.SetEventType(AllianceEventStreamEntryType.CHANGED_SETTINGS);
                allianceEventStreamEntry.SetEventAvatarId(memberEntry.GetAvatarId());
                allianceEventStreamEntry.SetEventAvatarName(memberEntry.GetName());

                StreamManager.Create(this.m_session.Alliance.Id, allianceEventStreamEntry);

                this.m_session.Alliance.AddStreamEntry(allianceEventStreamEntry);

                AllianceManager.Save(this.m_session.Alliance);
            }
        }

        private void OnChangeAllianceMemberRoleMessageReceived(ChangeAllianceMemberRoleMessage message)
        {
            if (this.m_session.Alliance != null)
            {
                LogicLong memberId = message.RemoveMemberId();

                if (!this.m_session.Alliance.Members.TryGetValue(memberId, out AllianceMemberEntry memberEntry))
                    return;

                AllianceMemberEntry eventMemberEntry = this.m_session.Alliance.Members[this.m_session.AccountId];

                if (message.GetMemberRole() == LogicAvatarAllianceRole.MEMBER ||
                    message.GetMemberRole() == LogicAvatarAllianceRole.LEADER ||
                    message.GetMemberRole() == LogicAvatarAllianceRole.ELDER ||
                    message.GetMemberRole() == LogicAvatarAllianceRole.CO_LEADER)
                {
                    if (memberEntry.HasLowerRoleThan(eventMemberEntry.GetAllianceRole()) &&
                        !eventMemberEntry.HasLowerRoleThan(message.GetMemberRole()))
                    {
                        if (message.GetMemberRole() == LogicAvatarAllianceRole.LEADER)
                            this.m_session.Alliance.ChangeMemberRole(eventMemberEntry, LogicAvatarAllianceRole.CO_LEADER, eventMemberEntry.GetAvatarId(), eventMemberEntry.GetName());
                        this.m_session.Alliance.ChangeMemberRole(memberEntry, message.GetMemberRole(), memberEntry.GetAvatarId(), memberEntry.GetName());

                        AllianceManager.Save(this.m_session.Alliance);
                    }
                }
            }
        }

        private void OnDonateAllianceUnitMessageReceived(DonateAllianceUnitMessage message)
        {
            if (this.m_session.Alliance != null)
            {
                Alliance alliance = this.m_session.Alliance;
                LogicCombatItemData unitData = message.GetAllianceUnitData();

                if (!unitData.IsDonationDisabled())
                {
                    if (message.UseQuickDonate())
                    {
                        if (!LogicDataTables.GetGlobals().EnableQuickDonate() || this.m_session.LogicClientAvatar.GetDiamonds() < unitData.GetDonateCost())
                            return;
                    }
                    else
                    {
                        if (this.m_session.LogicClientAvatar.GetUnitCount(unitData) <= 0)
                            return;
                    }

                    if (alliance.StreamEntryList.IndexOf(message.GetStreamId()) != -1)
                    {
                        StreamEntry streamEntry = StreamManager.GetAllianceStream(message.GetStreamId());

                        if (streamEntry.GetStreamEntryType() == StreamEntryType.DONATE)
                        {
                            DonateStreamEntry donateStreamEntry = (DonateStreamEntry) streamEntry;

                            if (donateStreamEntry.CanAddDonation(this.m_session.AccountId, message.GetAllianceUnitData(), alliance.Header.GetAllianceLevel()))
                            {
                                donateStreamEntry.AddDonation(this.m_session.AccountId, unitData, this.m_session.LogicClientAvatar.GetUnitUpgradeLevel(unitData));
                                donateStreamEntry.SetDonationPendingRequestCount(donateStreamEntry.GetDonationPendingRequestCount() + 1);

                                StreamManager.Save(donateStreamEntry);

                                alliance.UpdateStreamEntry(donateStreamEntry);

                                if (message.UseQuickDonate())
                                    this.m_session.LogicClientAvatar.CommodityCountChangeHelper(0, unitData, -1);

                                LogicDonateAllianceUnitCommand logicDonateAllianceUnitCommand = new LogicDonateAllianceUnitCommand();
                                logicDonateAllianceUnitCommand.SetData(unitData, streamEntry.GetId(), message.UseQuickDonate());
                                ServerMessageManager.SendMessage(new GameAllowServerCommandMessage
                                {
                                    AccountId = this.m_session.AccountId,
                                    ServerCommand = logicDonateAllianceUnitCommand
                                }, 9);
                            }
                        }
                    }
                }
            }
        }

        private void OnCancelChallengeMessageReceived(CancelChallengeMessage message)
        {
            if (this.m_session.Alliance != null)
            {
                Alliance alliance = this.m_session.Alliance;
                AllianceMemberEntry memberEntry = alliance.Members[this.m_session.AccountId];

                for (int i = 0; i < alliance.StreamEntryList.Size(); i++)
                {
                    StreamEntry streamEntry = StreamManager.GetAllianceStream(alliance.StreamEntryList[i]);

                    if (streamEntry != null && streamEntry.GetStreamEntryType() == StreamEntryType.CHALLENGE && streamEntry.GetSenderAvatarId().Equals(memberEntry.GetAvatarId()))
                    {
                        ChallengeStreamEntry prevChallengeStreamEntry = (ChallengeStreamEntry)streamEntry;

                        if (prevChallengeStreamEntry.IsStarted())
                            return;

                        alliance.RemoveStreamEntry(streamEntry.GetId());
                    }
                }
            }
        }

        private void OnScoutFriendlyBattleMessageReceived(ScoutFriendlyBattleMessage message)
        {
            if (this.m_session.Alliance != null)
            {
                Alliance alliance = this.m_session.Alliance;
                LogicLong streamId = message.GetStreamId();

                if (alliance.StreamEntryList.IndexOf(streamId) != -1)
                {
                    StreamEntry streamEntry = StreamManager.GetAllianceStream(message.GetStreamId());

                    if (streamEntry.GetStreamEntryType() == StreamEntryType.CHALLENGE)
                    {
                        ChallengeStreamEntry challengeStreamEntry = (ChallengeStreamEntry) streamEntry;

                        if (challengeStreamEntry.IsStarted())
                        {
                            ChallengeFailedMessage challengeFailedMessage = new ChallengeFailedMessage();
                            challengeFailedMessage.SetReason(ChallengeFailedMessage.Reason.ALREADY_CLOSED);
                            this.m_session.SendPiranhaMessage(challengeFailedMessage, 1);
                            return;
                        }

                        this.m_session.SendMessage(new GameFriendlyScoutMessage
                        {
                            AccountId = challengeStreamEntry.GetSenderAvatarId(),
                            HomeJSON = challengeStreamEntry.GetSnapshotHomeJSON(),
                            MapId = challengeStreamEntry.IsWarLayout() ? 1 : 0,
                            StreamId = challengeStreamEntry.GetId()
                        }, 9);
                        return;
                    }
                }
            }

            ChallengeFailedMessage challengeFailedMessageGeneric = new ChallengeFailedMessage();
            challengeFailedMessageGeneric.SetReason(ChallengeFailedMessage.Reason.GENERIC);
            this.m_session.SendPiranhaMessage(challengeFailedMessageGeneric, 1);
        }

        private void OnAcceptFriendlyBattleMessageReceived(AcceptFriendlyBattleMessage message)
        {
            if (this.m_session.Alliance != null)
            {
                Alliance alliance = this.m_session.Alliance;
                LogicLong streamId = message.GetStreamId();

                if (alliance.StreamEntryList.IndexOf(streamId) != -1)
                {
                    StreamEntry streamEntry = StreamManager.GetAllianceStream(message.GetStreamId());

                    if (streamEntry.GetStreamEntryType() == StreamEntryType.CHALLENGE)
                    {
                        ChallengeStreamEntry challengeStreamEntry = (ChallengeStreamEntry)streamEntry;

                        if (challengeStreamEntry.IsStarted())
                        {
                            ChallengeFailedMessage challengeFailedMessage = new ChallengeFailedMessage();
                            challengeFailedMessage.SetReason(ChallengeFailedMessage.Reason.ALREADY_CLOSED);
                            this.m_session.SendPiranhaMessage(challengeFailedMessage, 1);
                            return;
                        }
                        
                        this.m_session.SendMessage(new ChangeGameStateMessage
                        {
                            StateType = GameStateType.CHALLENGE_ATTACK,
                            ChallengeAllianceId = alliance.Id,
                            ChallengeHomeId = challengeStreamEntry.GetSenderAvatarId(),
                            ChallengeStreamId = challengeStreamEntry.GetId(),
                            ChallengeHomeJSON = challengeStreamEntry.GetSnapshotHomeJSON(),
                            ChallengeMapId = challengeStreamEntry.IsWarLayout() ? 1 : 0
                        }, 9);

                        challengeStreamEntry.SetStarted(true);
                        alliance.UpdateStreamEntry(challengeStreamEntry);

                        return;
                    }
                }
            }

            ChallengeFailedMessage challengeFailedMessageGeneric = new ChallengeFailedMessage();
            challengeFailedMessageGeneric.SetReason(ChallengeFailedMessage.Reason.GENERIC);
            this.m_session.SendPiranhaMessage(challengeFailedMessageGeneric, 1);
        }

        private void OnStartFriendlyChallengeSpectateMessageReceived(StartFriendlyChallengeSpectateMessage message)
        {
            if (this.m_session.Alliance != null)
            {
                Alliance alliance = this.m_session.Alliance;
                LogicLong streamId = message.GetStreamId();

                if (alliance.StreamEntryList.IndexOf(streamId) != -1)
                {
                    StreamEntry streamEntry = StreamManager.GetAllianceStream(message.GetStreamId());

                    if (streamEntry.GetStreamEntryType() == StreamEntryType.CHALLENGE)
                    {
                        ChallengeStreamEntry challengeStreamEntry = (ChallengeStreamEntry) streamEntry;

                        if (challengeStreamEntry.IsStarted() && challengeStreamEntry.GetLiveReplayId() != null)
                        {
                            this.m_session.SendMessage(new GameSpectateLiveReplayMessage
                            {
                                LiveReplayId = challengeStreamEntry.GetLiveReplayId(),
                                IsEnemy = false
                            }, 9);
                            return;
                        }
                    }
                }
            }

            LiveReplayFailedMessage liveReplayFailedMessage = new LiveReplayFailedMessage();
            liveReplayFailedMessage.SetReason(LiveReplayFailedMessage.Reason.GENERIC);
            this.m_session.SendPiranhaMessage(liveReplayFailedMessage, 1);
        }

        private void OnRespondToAllianceJoinRequestMessageReceived(RespondToAllianceJoinRequestMessage message)
        {
            if (this.m_session.Alliance != null)
            {
                Alliance alliance = this.m_session.Alliance;
                AllianceMemberEntry memberEntry = alliance.Members[this.m_session.AccountId];

                if (memberEntry.GetAllianceRole() == LogicAvatarAllianceRole.MEMBER)
                {
                    AllianceInvitationSendFailedMessage allianceInvitationSendFailedMessage = new AllianceInvitationSendFailedMessage();
                    allianceInvitationSendFailedMessage.SetReason(AllianceInvitationSendFailedMessage.Reason.NO_RIGHTS);
                    this.m_session.SendPiranhaMessage(allianceInvitationSendFailedMessage, 1);
                    return;
                }

                LogicLong streamId = message.GetStreamEntryId();

                if (alliance.StreamEntryList.IndexOf(streamId) != -1)
                {
                    StreamEntry streamEntry = StreamManager.GetAllianceStream(message.GetStreamEntryId());

                    if (streamEntry.GetStreamEntryType() == StreamEntryType.JOIN_REQUEST)
                    {
                        JoinRequestAllianceStreamEntry joinRequestAllianceStreamEntry = (JoinRequestAllianceStreamEntry) streamEntry;

                        if (joinRequestAllianceStreamEntry.GetState() == 1)
                        {
                            if (message.IsAccepted())
                            {
                                ServerRequestManager.Create(new GameJoinAllianceRequestMessage
                                {
                                    AccountId = joinRequestAllianceStreamEntry.GetSenderAvatarId(),
                                    AllianceId = alliance.Id,
                                    Invited = true
                                }, ServerManager.GetDocumentSocket(9, joinRequestAllianceStreamEntry.GetSenderAvatarId())).OnComplete = args =>
                                {
                                    if (this.m_session.IsDestructed())
                                        return;

                                    if (args.ErrorCode == ServerRequestError.Success)
                                    {
                                        if (args.ResponseMessage.Success)
                                        {
                                            joinRequestAllianceStreamEntry.SetState(2);
                                            joinRequestAllianceStreamEntry.SetResponderName(memberEntry.GetName());
                                            alliance.UpdateStreamEntry(joinRequestAllianceStreamEntry);

                                            StreamManager.Save(joinRequestAllianceStreamEntry);
                                        }
                                        else
                                        {
                                            GameJoinAllianceResponseMessage gameJoinAllianceResponseMessage = (GameJoinAllianceResponseMessage)args.ResponseMessage;
                                            AllianceInvitationSendFailedMessage allianceInvitationSendFailedMessage = new AllianceInvitationSendFailedMessage();

                                            switch (gameJoinAllianceResponseMessage.ErrorReason)
                                            {
                                                case GameJoinAllianceResponseMessage.Reason.NO_CASTLE:
                                                    allianceInvitationSendFailedMessage.SetReason(AllianceInvitationSendFailedMessage.Reason.NO_CASTLE);

                                                    if (joinRequestAllianceStreamEntry.GetState() == 1)
                                                    {
                                                        alliance.RemoveStreamEntry(joinRequestAllianceStreamEntry.GetId());
                                                        AllianceManager.Save(alliance);
                                                    }

                                                    break;
                                                case GameJoinAllianceResponseMessage.Reason.ALREADY_IN_ALLIANCE:
                                                    allianceInvitationSendFailedMessage.SetReason(AllianceInvitationSendFailedMessage.Reason.ALREADY_IN_ALLIANCE);

                                                    if (joinRequestAllianceStreamEntry.GetState() == 1)
                                                    {
                                                        alliance.RemoveStreamEntry(joinRequestAllianceStreamEntry.GetId());
                                                        AllianceManager.Save(alliance);
                                                    }

                                                    break;
                                                default:
                                                    allianceInvitationSendFailedMessage.SetReason(AllianceInvitationSendFailedMessage.Reason.GENERIC);
                                                    break;
                                            }

                                            this.m_session.SendPiranhaMessage(allianceInvitationSendFailedMessage, 1);
                                        }
                                    }
                                    else
                                    {
                                        AllianceInvitationSendFailedMessage allianceInvitationSendFailedMessage = new AllianceInvitationSendFailedMessage();
                                        allianceInvitationSendFailedMessage.SetReason(AllianceInvitationSendFailedMessage.Reason.GENERIC);
                                        this.m_session.SendPiranhaMessage(allianceInvitationSendFailedMessage, 1);
                                    }
                                };
                            }
                            else
                            {
                                joinRequestAllianceStreamEntry.SetState(0);
                                joinRequestAllianceStreamEntry.SetResponderName(memberEntry.GetName());
                                alliance.UpdateStreamEntry(joinRequestAllianceStreamEntry);

                                StreamManager.Save(joinRequestAllianceStreamEntry);

                                JoinAllianceResponseAvatarStreamEntry joinAllianceResponseAvatarStreamEntry = new JoinAllianceResponseAvatarStreamEntry();

                                joinAllianceResponseAvatarStreamEntry.SetSenderAvatarId(memberEntry.GetAvatarId());
                                joinAllianceResponseAvatarStreamEntry.SetSenderHomeId(memberEntry.GetHomeId());
                                joinAllianceResponseAvatarStreamEntry.SetSenderName(memberEntry.GetName());
                                joinAllianceResponseAvatarStreamEntry.SetSenderLevel(memberEntry.GetExpLevel());
                                joinAllianceResponseAvatarStreamEntry.SetSenderLeagueType(memberEntry.GetLeagueType());

                                joinAllianceResponseAvatarStreamEntry.SetAllianceId(alliance.Id);
                                joinAllianceResponseAvatarStreamEntry.SetAllianceName(alliance.Header.GetAllianceName());
                                joinAllianceResponseAvatarStreamEntry.SetAllianceBadgeId(alliance.Header.GetAllianceBadgeId());
                               
                                ServerMessageManager.SendMessage(new CreateAvatarStreamMessage
                                {
                                    AccountId = joinRequestAllianceStreamEntry.GetSenderAvatarId(),
                                    Entry = joinAllianceResponseAvatarStreamEntry
                                }, 9);
                            }
                        }
                    }
                }
            }
        }

        private void OnSendAllianceInvitationMessageReceived(SendAllianceInvitationMessage message)
        {
            if (this.m_session.Alliance != null)
            {
                Alliance alliance = this.m_session.Alliance;
                AllianceMemberEntry memberEntry = alliance.Members[this.m_session.AccountId];

                if (memberEntry.GetAllianceRole() == LogicAvatarAllianceRole.MEMBER)
                {
                    AllianceInvitationSendFailedMessage allianceInvitationSendFailedMessage = new AllianceInvitationSendFailedMessage();
                    allianceInvitationSendFailedMessage.SetReason(AllianceInvitationSendFailedMessage.Reason.NO_RIGHTS);
                    this.m_session.SendPiranhaMessage(allianceInvitationSendFailedMessage, 1);
                    return;
                }

                if (memberEntry.GetAllianceRole() == LogicAvatarAllianceRole.ELDER)
                {
                    AllianceInvitationSendFailedMessage allianceInvitationSendFailedMessage = new AllianceInvitationSendFailedMessage();
                    allianceInvitationSendFailedMessage.SetReason(AllianceInvitationSendFailedMessage.Reason.USER_BANNED);
                    this.m_session.SendPiranhaMessage(allianceInvitationSendFailedMessage, 1);
                    return;
                }

                AllianceInvitationAvatarStreamEntry allianceInvitationAvatarStreamEntry = new AllianceInvitationAvatarStreamEntry();

                allianceInvitationAvatarStreamEntry.SetSenderAvatarId(memberEntry.GetAvatarId());
                allianceInvitationAvatarStreamEntry.SetSenderHomeId(memberEntry.GetHomeId());
                allianceInvitationAvatarStreamEntry.SetSenderName(memberEntry.GetName());
                allianceInvitationAvatarStreamEntry.SetSenderLevel(memberEntry.GetExpLevel());
                allianceInvitationAvatarStreamEntry.SetSenderLeagueType(memberEntry.GetLeagueType());

                allianceInvitationAvatarStreamEntry.SetAllianceId(alliance.Id);
                allianceInvitationAvatarStreamEntry.SetAllianceName(alliance.Header.GetAllianceName());
                allianceInvitationAvatarStreamEntry.SetAllianceBadgeId(alliance.Header.GetAllianceBadgeId());
                allianceInvitationAvatarStreamEntry.SetAllianceLevel(alliance.Header.GetAllianceLevel());

                ServerRequestManager.Create(new GameCreateAllianceInvitationRequestMessage
                {
                    AccountId = message.GetAvatarId(),
                    Entry = allianceInvitationAvatarStreamEntry
                }, ServerManager.GetDocumentSocket(9, message.GetAvatarId())).OnComplete = args =>
                {
                    if (this.m_session.IsDestructed())
                        return;

                    if (args.ErrorCode == ServerRequestError.Success)
                    {
                        GameCreateAllianceInvitationResponseMessage responseMessage = (GameCreateAllianceInvitationResponseMessage) args.ResponseMessage;

                        if (responseMessage.Success)
                        {
                            this.m_session.SendPiranhaMessage(new AllianceInvitationSentOkMessage(), 1);
                        }
                        else
                        {
                            AllianceInvitationSendFailedMessage allianceInvitationSendFailedMessage = new AllianceInvitationSendFailedMessage();

                            switch (responseMessage.ErrorReason)
                            {
                                case GameCreateAllianceInvitationResponseMessage.Reason.GENERIC:
                                    allianceInvitationSendFailedMessage.SetReason(AllianceInvitationSendFailedMessage.Reason.GENERIC);
                                    break;
                                case GameCreateAllianceInvitationResponseMessage.Reason.NO_CASTLE:
                                    allianceInvitationSendFailedMessage.SetReason(AllianceInvitationSendFailedMessage.Reason.NO_CASTLE);
                                    break;
                                case GameCreateAllianceInvitationResponseMessage.Reason.ALREADY_IN_ALLIANCE:
                                    allianceInvitationSendFailedMessage.SetReason(AllianceInvitationSendFailedMessage.Reason.ALREADY_IN_ALLIANCE);
                                    break;
                                case GameCreateAllianceInvitationResponseMessage.Reason.ALREADY_HAS_AN_INVITE:
                                    allianceInvitationSendFailedMessage.SetReason(AllianceInvitationSendFailedMessage.Reason.ALREADY_HAS_AN_INVITE);
                                    break;
                                case GameCreateAllianceInvitationResponseMessage.Reason.HAS_TOO_MANY_INVITES:
                                    allianceInvitationSendFailedMessage.SetReason(AllianceInvitationSendFailedMessage.Reason.HAS_TOO_MANY_INVITES);
                                    break;
                            }

                            this.m_session.SendPiranhaMessage(allianceInvitationSendFailedMessage, 1);
                        }
                    }
                    else
                    {
                        AllianceInvitationSendFailedMessage allianceInvitationSendFailedMessage = new AllianceInvitationSendFailedMessage();
                        allianceInvitationSendFailedMessage.SetReason(AllianceInvitationSendFailedMessage.Reason.GENERIC);
                        this.m_session.SendPiranhaMessage(allianceInvitationSendFailedMessage, 1);
                    }
                };
            }
        }

        private bool CanSendGlobalChatMessage()
        {
            return DateTime.UtcNow.Subtract(this.m_previousAllianceChatMessageTime).TotalSeconds >= 1d;
        }
    }
}