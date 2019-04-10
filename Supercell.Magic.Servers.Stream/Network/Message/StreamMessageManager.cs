namespace Supercell.Magic.Servers.Stream.Network.Message
{
    using System;
    using Supercell.Magic.Logic;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Command.Server;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Message;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Logic.Message.Alliance.Stream;
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Helper;
    using Supercell.Magic.Servers.Stream.Logic;
    using Supercell.Magic.Servers.Stream.Session;
    using Supercell.Magic.Servers.Stream.Util;

    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Core;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Message.Request.Stream;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Network.Message.Session.Change;
    using Supercell.Magic.Servers.Core.Network.Request;

    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class StreamMessageManager : ServerMessageManager
    {
        public override void OnReceiveAccountMessage(ServerAccountMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.LEAVE_ALLIANCE_MEMBER:
                    StreamMessageManager.OnLeaveAllianceMemberMessageReceived((LeaveAllianceMemberMessage) message);
                    break;
                case ServerMessageType.AVATAR_STREAM_SEEN:
                    StreamMessageManager.OnAvatarStreamSeenMessageReceived((AvatarStreamSeenMessage) message);
                    break;
                case ServerMessageType.REMOVE_AVATAR_STREAM:
                    StreamMessageManager.OnRemoveAvatarStreamMessageReceived((RemoveAvatarStreamMessage) message);
                    break;
                case ServerMessageType.ALLIANCE_AVATAR_CHANGES:
                    StreamMessageManager.OnAllianceUpdateAvatarDataMessageReceived((AllianceAvatarChangesMessage) message);
                    break;
                case ServerMessageType.ALLIANCE_CREATE_MAIL:
                    StreamMessageManager.OnAllianceCreateMailMessageReceived((AllianceCreateMailMessage) message);
                    break;
                case ServerMessageType.ALLIANCE_SHARE_REPLAY:
                    StreamMessageManager.OnAllianceShareReplayMessageReceivedReceived((AllianceShareReplayMessage) message);
                    break;
                case ServerMessageType.ALLIANCE_REQUEST_ALLIANCE_UNITS:
                    StreamMessageManager.OnAllianceRequestAllianceUnitsMessageReceived((AllianceRequestAllianceUnitsMessage) message);
                    break;
                case ServerMessageType.ALLIANCE_UNIT_DONATE_RESPONSE:
                    StreamMessageManager.OnAllianceUnitDonateResponseMessageReceived((AllianceUnitDonateResponseMessage) message);
                    break;
                case ServerMessageType.ALLIANCE_CHALLENGE_REQUEST:
                    StreamMessageManager.OnAllianceChallengeRequestMessageReceived((AllianceChallengeRequestMessage) message);
                    break;
                case ServerMessageType.ALLIANCE_CHALLENGE_REPORT:
                    StreamMessageManager.OnAllianceChallengeReportMessageReceived((AllianceChallengeReportMessage) message);
                    break;
                case ServerMessageType.ALLIANCE_CHALLENGE_LIVE_REPLAY_ID:
                    StreamMessageManager.OnAllianceChallengeLiveReplayIdMessageReceived((AllianceChallengeLiveReplayIdMessage) message);
                    break;
                case ServerMessageType.ALLIANCE_CHALLENGE_SPECTATOR_COUNT:
                    StreamMessageManager.OnAllianceChallengeSpectatorCountMessageReceived((AllianceChallengeSpectatorCountMessage) message);
                    break;
            }
        }

        private static void OnRemoveAvatarStreamMessageReceived(RemoveAvatarStreamMessage message)
        {
            StreamManager.RemoveAvatarStream(message.AccountId);
        }

        private static void OnLeaveAllianceMemberMessageReceived(LeaveAllianceMemberMessage message)
        {
            if (AllianceManager.TryGet(message.AccountId, out Alliance alliance) && alliance.Members.TryGetValue(message.MemberId, out AllianceMemberEntry memberEntry))
            {
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

                alliance.RemoveMember(message.MemberId);

                AllianceSession session = alliance.GetCurrentOnlineMemberSession(message.AccountId);

                if (session != null)
                {
                    AllianceSessionManager.Remove(session.Id);
                }

                AllianceEventStreamEntry allianceEventStreamEntry = new AllianceEventStreamEntry();
                AllianceStreamEntryUtil.SetSenderInfo(allianceEventStreamEntry, memberEntry);

                allianceEventStreamEntry.SetEventType(AllianceEventStreamEntryType.LEFT);
                allianceEventStreamEntry.SetEventAvatarId(memberEntry.GetAvatarId());
                allianceEventStreamEntry.SetEventAvatarName(memberEntry.GetName());

                StreamManager.Create(alliance.Id, allianceEventStreamEntry);

                alliance.AddStreamEntry(allianceEventStreamEntry);

                AllianceManager.Save(alliance);
            }

            ServerMessageManager.SendMessage(new AllianceLeavedMessage
            {
                AccountId = message.MemberId,
                AllianceId = message.AccountId
            }, 9);
        }

        private static void OnAllianceUpdateAvatarDataMessageReceived(AllianceAvatarChangesMessage message)
        {
            if (AllianceManager.TryGet(message.AccountId, out Alliance alliance) && alliance.Members.TryGetValue(message.MemberId, out AllianceMemberEntry memberEntry))
            {
                bool updateScoring = false;

                for (int i = 0; i < message.AvatarChanges.Size(); i++)
                {
                    AvatarChange avatarChange = message.AvatarChanges[i];
                    
                    avatarChange.ApplyAvatarChange(memberEntry);

                    if (avatarChange.GetAvatarChangeType() == AvatarChangeType.SCORE || avatarChange.GetAvatarChangeType() == AvatarChangeType.DUEL_SCORE)
                        updateScoring = true;
                }

                AllianceSession currentSession = alliance.GetCurrentOnlineMemberSession(message.MemberId);

                if (currentSession != null && currentSession.LogicClientAvatar != null)
                {
                    for (int i = 0; i < message.AvatarChanges.Size(); i++)
                    {
                        message.AvatarChanges[i].ApplyAvatarChange(currentSession.LogicClientAvatar);
                    }
                }

                if (updateScoring)
                    alliance.UpdateScoring();

                AllianceManager.Save(alliance);
            }
        }

        private static void OnAllianceCreateMailMessageReceived(AllianceCreateMailMessage message)
        {
            if (AllianceManager.TryGet(message.AccountId, out Alliance alliance) && alliance.Members.TryGetValue(message.MemberId, out AllianceMemberEntry senderMemberEntry))
            {
                AllianceMailAvatarStreamEntry allianceMailAvatarStreamEntry = new AllianceMailAvatarStreamEntry();

                allianceMailAvatarStreamEntry.SetSenderAvatarId(senderMemberEntry.GetAvatarId());
                allianceMailAvatarStreamEntry.SetSenderHomeId(senderMemberEntry.GetHomeId());
                allianceMailAvatarStreamEntry.SetSenderName(senderMemberEntry.GetName());
                allianceMailAvatarStreamEntry.SetSenderLevel(senderMemberEntry.GetExpLevel());
                allianceMailAvatarStreamEntry.SetSenderLeagueType(senderMemberEntry.GetLeagueType());

                allianceMailAvatarStreamEntry.SetMessage(message.Message);
                allianceMailAvatarStreamEntry.SetAllianceId(alliance.Id);
                allianceMailAvatarStreamEntry.SetAllianceName(alliance.Header.GetAllianceName());
                allianceMailAvatarStreamEntry.SetAllianceBadgeId(alliance.Header.GetAllianceBadgeId());
                
                foreach (AllianceMemberEntry memberEntry in alliance.Members.Values)
                {
                    ServerMessageManager.SendMessage(new CreateAvatarStreamMessage
                    {
                        AccountId = memberEntry.GetAvatarId(),
                        Entry = allianceMailAvatarStreamEntry
                    }, 9);
                }
            }
        }

        private static void OnAvatarStreamSeenMessageReceived(AvatarStreamSeenMessage message)
        {
            AvatarStreamEntry entry = StreamManager.GetAvatarStream(message.AccountId);

            if (entry != null && entry.IsNew())
            {
                entry.SetNew(false);
                StreamManager.Save(entry);
            }
        }

        private static void OnAllianceShareReplayMessageReceivedReceived(AllianceShareReplayMessage message)
        {
            if (AllianceManager.TryGet(message.AccountId, out Alliance alliance) && alliance.Members.TryGetValue(message.MemberId, out AllianceMemberEntry memberEntry))
            {
                ServerRequestManager.Create(new LoadAvatarStreamRequestMessage
                {
                    Id = message.ReplayId
                }, ServerManager.GetDocumentSocket(11, message.ReplayId)).OnComplete = args =>
                {
                    if (args.ErrorCode == ServerRequestError.Success && args.ResponseMessage.Success)
                    {
                        LoadAvatarStreamResponseMessage responseMessage = (LoadAvatarStreamResponseMessage) args.ResponseMessage;

                        if (responseMessage.Entry.GetAvatarStreamEntryType() == AvatarStreamEntryType.ATTACKER_BATTLE_REPORT ||
                            responseMessage.Entry.GetAvatarStreamEntryType() == AvatarStreamEntryType.DEFENDER_BATTLE_REPORT)
                        {
                            BattleReportStreamEntry battleReportStreamEntry = (BattleReportStreamEntry) responseMessage.Entry;
                            ReplayStreamEntry replayStreamEntry = new ReplayStreamEntry();
                            AllianceStreamEntryUtil.SetSenderInfo(replayStreamEntry, memberEntry);

                            replayStreamEntry.SetAttack(responseMessage.Entry.GetAvatarStreamEntryType() == AvatarStreamEntryType.ATTACKER_BATTLE_REPORT);
                            replayStreamEntry.SetBattleLogJSON(battleReportStreamEntry.GetBattleLogJSON());
                            replayStreamEntry.SetMajorVersion(battleReportStreamEntry.GetMajorVersion());
                            replayStreamEntry.SetBuildVersion(battleReportStreamEntry.GetBuildVersion());
                            replayStreamEntry.SetContentVersion(battleReportStreamEntry.GetContentVersion());
                            replayStreamEntry.SetReplayId(battleReportStreamEntry.GetReplayId());
                            replayStreamEntry.SetReplayShardId(battleReportStreamEntry.GetReplayShardId());
                            replayStreamEntry.SetOpponentName(battleReportStreamEntry.GetSenderName());

                            string replayMessage = message.Message;

                            if (replayMessage != null && replayMessage.Length > 128)
                                replayMessage = replayMessage.Substring(0, 128);

                            replayStreamEntry.SetMessage(replayMessage);

                            StreamManager.Create(alliance.Id, replayStreamEntry);

                            alliance.AddStreamEntry(replayStreamEntry);

                            AllianceManager.Save(alliance);
                        }
                    }
                };
            }
        }

        private static void OnAllianceRequestAllianceUnitsMessageReceived(AllianceRequestAllianceUnitsMessage message)
        {
            if (AllianceManager.TryGet(message.AccountId, out Alliance alliance) && alliance.Members.TryGetValue(message.MemberId, out AllianceMemberEntry memberEntry))
            {
                for (int i = 0; i < alliance.StreamEntryList.Size(); i++)
                {
                    StreamEntry streamEntry = StreamManager.GetAllianceStream(alliance.StreamEntryList[i]);

                    if (streamEntry != null && streamEntry.GetStreamEntryType() == StreamEntryType.DONATE && streamEntry.GetSenderAvatarId().Equals(memberEntry.GetAvatarId()))
                        alliance.RemoveStreamEntry(streamEntry.GetId());
                }

                DonateStreamEntry donateStreamEntry = new DonateStreamEntry();
                AllianceStreamEntryUtil.SetSenderInfo(donateStreamEntry, memberEntry);

                string donateMessage = message.Message;

                if (donateMessage.Length > 128)
                    donateMessage = donateMessage.Substring(0, 128);

                donateStreamEntry.SetMessage(donateMessage);
                donateStreamEntry.SetCasteLevel(message.CastleUpgradeLevel, message.CastleUsedCapacity, message.CastleSpellUsedCapacity, message.CastleTotalCapacity, message.CastleSpellTotalCapacity);

                StreamManager.Create(alliance.Id, donateStreamEntry);

                alliance.AddStreamEntry(donateStreamEntry);

                AllianceManager.Save(alliance);
            }
        }

        private static void OnAllianceUnitDonateResponseMessageReceived(AllianceUnitDonateResponseMessage message)
        {
            if (AllianceManager.TryGet(message.AccountId, out Alliance alliance))
            {
                StreamEntry streamEntry = StreamManager.GetAllianceStream(message.StreamId);

                if (streamEntry != null)
                {
                    DonateStreamEntry donateStreamEntry = (DonateStreamEntry) streamEntry;
                    
                    if (message.Success)
                    {
                        LogicAllianceLevelData allianceLevel = LogicDataTables.GetAllianceLevel(alliance.Header.GetAllianceLevel());
                        LogicAllianceUnitReceivedCommand logicAllianceUnitReceivedCommand = new LogicAllianceUnitReceivedCommand();
                        logicAllianceUnitReceivedCommand.SetData(message.MemberName, message.Data,
                                                                 LogicMath.Clamp(message.UpgradeLevel + allianceLevel.GetTroopDonationUpgrade(), 0, message.Data.GetUpgradeLevelCount() - 1));
                        ServerMessageManager.SendMessage(new GameAllowServerCommandMessage
                        {
                            AccountId = donateStreamEntry.GetSenderAvatarId(),
                            ServerCommand = logicAllianceUnitReceivedCommand
                        }, 9);
                    }
                    else
                    {
                        donateStreamEntry.RemoveDonation(message.MemberId, message.Data, message.UpgradeLevel);
                        alliance.UpdateStreamEntry(donateStreamEntry);
                    }
                    
                    donateStreamEntry.SetDonationPendingRequestCount(donateStreamEntry.GetDonationPendingRequestCount() - 1);

                    if (donateStreamEntry.IsCastleFull() && donateStreamEntry.GetDonationPendingRequestCount() <= 0)
                    {
                        alliance.RemoveStreamEntry(donateStreamEntry.GetId());
                        AllianceManager.Save(alliance);
                    }
                    else
                    {
                        StreamManager.Save(donateStreamEntry);
                    }
                }
            }
        }

        private static void OnAllianceChallengeRequestMessageReceived(AllianceChallengeRequestMessage message)
        {
            if (AllianceManager.TryGet(message.AccountId, out Alliance alliance) && alliance.Members.TryGetValue(message.MemberId, out AllianceMemberEntry memberEntry))
            {
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

                ChallengeStreamEntry challengeStreamEntry = new ChallengeStreamEntry();
                AllianceStreamEntryUtil.SetSenderInfo(challengeStreamEntry, memberEntry);

                string challengeMessage = message.Message;

                if (challengeMessage.Length > 128)
                    challengeMessage = challengeMessage.Substring(0, 128);

                challengeStreamEntry.SetMessage(challengeMessage);
                challengeStreamEntry.SetSnapshotHomeJSON(message.HomeJSON);
                challengeStreamEntry.SetWarLayout(message.WarLayout);

                StreamManager.Create(alliance.Id, challengeStreamEntry);

                alliance.AddStreamEntry(challengeStreamEntry);

                AllianceManager.Save(alliance);
            }
        }

        private static void OnAllianceChallengeReportMessageReceived(AllianceChallengeReportMessage message)
        {
            if (AllianceManager.TryGet(message.AccountId, out Alliance alliance))
            {
                ChallengeStreamEntry streamEntry = (ChallengeStreamEntry) StreamManager.GetAllianceStream(message.StreamId);

                if (streamEntry != null)
                {
                    ChallengeReplayStreamEntry challengeReplayStreamEntry = new ChallengeReplayStreamEntry();
                    
                    challengeReplayStreamEntry.SetSenderAvatarId(challengeReplayStreamEntry.GetSenderAvatarId());
                    challengeReplayStreamEntry.SetSenderHomeId(challengeReplayStreamEntry.GetSenderHomeId());
                    challengeReplayStreamEntry.SetSenderName(challengeReplayStreamEntry.GetSenderName());
                    challengeReplayStreamEntry.SetSenderLevel(challengeReplayStreamEntry.GetSenderLevel());
                    challengeReplayStreamEntry.SetSenderLeagueType(challengeReplayStreamEntry.GetSenderLeagueType());
                    challengeReplayStreamEntry.SetSenderRole(challengeReplayStreamEntry.GetSenderRole());

                    challengeReplayStreamEntry.SetBattleLogJSON(message.BattleLog);
                    challengeReplayStreamEntry.SetReplayMajorVersion(LogicVersion.MAJOR_VERSION);
                    challengeReplayStreamEntry.SetReplayBuildVersion(LogicVersion.BUILD_VERSION);
                    challengeReplayStreamEntry.SetReplayContentVersion(ResourceManager.GetContentVersion());
                    challengeReplayStreamEntry.SetReplayId(message.ReplayId);

                    StreamManager.Create(alliance.Id, challengeReplayStreamEntry);

                    alliance.RemoveStreamEntry(streamEntry.GetId());
                    alliance.AddStreamEntry(challengeReplayStreamEntry);
                }
                else
                {
                    Logging.Warning("StreamMessageManager.onAllianceChallengeReportMessageReceived: pStreamEntry has been deleted. Replay ignored.");
                }
            }
        }

        private static void OnAllianceChallengeLiveReplayIdMessageReceived(AllianceChallengeLiveReplayIdMessage message)
        {
            ChallengeStreamEntry streamEntry = (ChallengeStreamEntry)StreamManager.GetAllianceStream(message.AccountId);

            if (streamEntry != null)
            {
                streamEntry.SetLiveReplayId(message.LiveReplayId);
            }
        }

        private static void OnAllianceChallengeSpectatorCountMessageReceived(AllianceChallengeSpectatorCountMessage message)
        {
            if (AllianceManager.TryGet(message.AccountId, out Alliance alliance))
            {
                ChallengeStreamEntry streamEntry = (ChallengeStreamEntry) StreamManager.GetAllianceStream(message.StreamId);

                if (streamEntry != null && streamEntry.GetSpectatorCount() != message.Count)
                {
                    streamEntry.SetSpectatorCount(message.Count);
                    alliance.UpdateStreamEntry(streamEntry);
                }
            }
        }

        public override void OnReceiveRequestMessage(ServerRequestMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.CREATE_ALLIANCE_REQUEST:
                    StreamMessageManager.OnCreateAllianceRequestMessageReceived((CreateAllianceRequestMessage) message);
                    break;
                case ServerMessageType.ALLIANCE_JOIN_REQUEST:
                    StreamMessageManager.OnAllianceJoinRequestMessageReceived((AllianceJoinRequestMessage) message);
                    break;

                case ServerMessageType.CREATE_AVATAR_STREAM_REQUEST:
                    StreamMessageManager.OnCreateAvatarStreamRequestMessageReceived((CreateAvatarStreamRequestMessage) message);
                    break;
                case ServerMessageType.CREATE_REPLAY_STREAM_REQUEST:
                    StreamMessageManager.OnCreateReplayStreamRequestMessageReceived((CreateReplayStreamRequestMessage) message);
                    break;
                case ServerMessageType.LOAD_REPLAY_STREAM_REQUEST:
                    StreamMessageManager.OnLoadReplayStreamRequestMessageReceived((LoadReplayStreamRequestMessage) message);
                    break;
                case ServerMessageType.LOAD_AVATAR_STREAM_REQUEST:
                    StreamMessageManager.OnLoadAvatarStreamRequestMessageReceived((LoadAvatarStreamRequestMessage) message);
                    break;
                case ServerMessageType.LOAD_AVATAR_STREAM_OF_TYPE_REQUEST:
                    StreamMessageManager.OnLoadAvatarStreamOfTypeRequestMessageReceived((LoadAvatarStreamOfTypeRequestMessage) message);
                    break;
                case ServerMessageType.REQUEST_ALLIANCE_JOIN_REQUEST:
                    StreamMessageManager.OnRequestAllianceJoinRequestMessageReceived((RequestAllianceJoinRequestMessage) message);
                    break;
            }
        }

        private static void OnCreateAllianceRequestMessageReceived(CreateAllianceRequestMessage message)
        {
            CreateAllianceResponseMessage createAllianceResponseMessage = new CreateAllianceResponseMessage();

            string allianceName = message.AllianceName;

            if (allianceName == null || allianceName.Length < 2)
            {
                createAllianceResponseMessage.ErrorReason = CreateAllianceResponseMessage.Reason.NAME_TOO_SHORT;
                ServerRequestManager.SendResponse(createAllianceResponseMessage, message);
                return;
            }

            if (allianceName.Length > 15)
            {
                createAllianceResponseMessage.ErrorReason = CreateAllianceResponseMessage.Reason.NAME_TOO_LONG;
                ServerRequestManager.SendResponse(createAllianceResponseMessage, message);
                return;
            }
            
            Alliance alliance = AllianceManager.Create();

            alliance.Header.SetAllianceName(allianceName);
            alliance.SetAllianceSettings(message.AllianceDescription, message.AllianceType, message.AllianceBadgeId, message.RequiredScore, message.RequiredDuelScore, message.WarFrequency,
                                         message.OriginData, message.PublicWarLog, message.ArrangedWarEnabled);

            createAllianceResponseMessage.Success = true;
            createAllianceResponseMessage.AllianceId = alliance.Id;

            AllianceManager.Save(alliance);
            ServerRequestManager.SendResponse(createAllianceResponseMessage, message);
        }

        private static void OnAllianceJoinRequestMessageReceived(AllianceJoinRequestMessage message)
        {
            AllianceJoinResponseMessage joinAllianceResponseMessage = new AllianceJoinResponseMessage();

            if (!AllianceManager.TryGet(message.AllianceId, out Alliance alliance))
            {
                joinAllianceResponseMessage.ErrorReason = AllianceJoinResponseMessage.Reason.GENERIC;
                ServerRequestManager.SendResponse(joinAllianceResponseMessage, message);
                return;
            }

            if (!message.Created)
            {
                if (alliance.IsFull())
                {
                    joinAllianceResponseMessage.ErrorReason = AllianceJoinResponseMessage.Reason.FULL;
                    ServerRequestManager.SendResponse(joinAllianceResponseMessage, message);
                    return;
                }

                if (!message.Invited)
                {
                    if (alliance.Header.GetAllianceType() != AllianceType.OPEN || alliance.Header.GetNumberOfMembers() == 0)
                    {
                        joinAllianceResponseMessage.ErrorReason =
                            alliance.Header.GetAllianceType() == AllianceType.CLOSED ? AllianceJoinResponseMessage.Reason.CLOSED : AllianceJoinResponseMessage.Reason.GENERIC;
                        ServerRequestManager.SendResponse(joinAllianceResponseMessage, message);
                        return;
                    }

                    if (alliance.IsBanned(message.Avatar.GetId()))
                    {
                        joinAllianceResponseMessage.ErrorReason = AllianceJoinResponseMessage.Reason.BANNED;
                        ServerRequestManager.SendResponse(joinAllianceResponseMessage, message);
                        return;
                    }

                    if (alliance.Header.GetRequiredScore() > message.Avatar.GetScore() || alliance.Header.GetRequiredDuelScore() > message.Avatar.GetDuelScore())
                    {
                        joinAllianceResponseMessage.ErrorReason = AllianceJoinResponseMessage.Reason.SCORE;
                        ServerRequestManager.SendResponse(joinAllianceResponseMessage, message);
                        return;
                    }
                }
            }
            else
            {
                if (alliance.Header.GetNumberOfMembers() != 0)
                    throw new Exception("StreamMessageManager.joinAllianceRequestMessageReceived: A new alliance must be empty!");
            }

            AllianceMemberEntry memberEntry = AllianceMemberUtil.GetAllianceMemberEntryFromAvatar(message.Avatar);
            memberEntry.SetAllianceRole(message.Created ? LogicAvatarAllianceRole.LEADER : LogicAvatarAllianceRole.MEMBER);
            alliance.AddMember(memberEntry);

            if (!message.Created)
            {
                AllianceEventStreamEntry allianceEventStreamEntry = new AllianceEventStreamEntry();
                AllianceStreamEntryUtil.SetSenderInfo(allianceEventStreamEntry, memberEntry);

                allianceEventStreamEntry.SetEventType(AllianceEventStreamEntryType.JOINED);
                allianceEventStreamEntry.SetEventAvatarId(memberEntry.GetAvatarId());
                allianceEventStreamEntry.SetEventAvatarName(memberEntry.GetName());

                StreamManager.Create(alliance.Id, allianceEventStreamEntry);

                alliance.AddStreamEntry(allianceEventStreamEntry);
            }
            
            joinAllianceResponseMessage.Success = true;
            joinAllianceResponseMessage.AllianceId = alliance.Id;
            joinAllianceResponseMessage.AllianceName = alliance.Header.GetAllianceName();
            joinAllianceResponseMessage.AllianceBadgeId = alliance.Header.GetAllianceBadgeId();
            joinAllianceResponseMessage.AllianceLevel = alliance.Header.GetAllianceLevel();
            joinAllianceResponseMessage.Created = message.Created;

            AllianceManager.Save(alliance);
            ServerRequestManager.SendResponse(joinAllianceResponseMessage, message);
        }

        private static void OnCreateAvatarStreamRequestMessageReceived(CreateAvatarStreamRequestMessage message)
        {
            StreamManager.Create(message.OwnerId, message.Entry);
            ServerRequestManager.SendResponse(new CreateAvatarStreamResponseMessage
            {
                Success = true,
                Entry = message.Entry
            }, message);
        }

        private static void OnCreateReplayStreamRequestMessageReceived(CreateReplayStreamRequestMessage message)
        {
            ZLibHelper.CompressInZLibFormat(LogicStringUtil.GetBytes(message.JSON), out byte[] entry);
            StreamManager.Create(entry, out LogicLong id);
            ServerRequestManager.SendResponse(new CreateReplayStreamResponseMessage
            {
                Success = true,
                Id = id
            }, message);
        }

        private static void OnLoadReplayStreamRequestMessageReceived(LoadReplayStreamRequestMessage message)
        {
            Core.Database.Document.ReplayStreamEntry entry = StreamManager.GetReplayStream(message.Id);

            if (entry != null)
            {
                ServerRequestManager.SendResponse(new LoadReplayStreamResponseMessage
                {
                    StreamData = entry.GetStreamData(),
                    MajorVersion = entry.GetMajorVersion(),
                    BuildVersion = entry.GetBuildVersion(),
                    ContentVersion = entry.GetContentVersion(),
                    Success = true
                }, message);
            }
            else
            {
                ServerRequestManager.SendResponse(new LoadReplayStreamResponseMessage(), message);
            }
        }

        private static void OnLoadAvatarStreamRequestMessageReceived(LoadAvatarStreamRequestMessage message)
        {
            AvatarStreamEntry entry = StreamManager.GetAvatarStream(message.Id);

            if (entry != null)
            {
                ServerRequestManager.SendResponse(new LoadAvatarStreamResponseMessage
                {
                    Entry = entry,
                    Success = true
                }, message);
            }
            else
            {
                ServerRequestManager.SendResponse(new LoadAvatarStreamResponseMessage(), message);
            }
        }

        private static void OnLoadAvatarStreamOfTypeRequestMessageReceived(LoadAvatarStreamOfTypeRequestMessage message)
        {
            LoadAvatarStreamOfTypeResponseMessage loadAvatarStreamOfTypeResponseMessage = new LoadAvatarStreamOfTypeResponseMessage();
            LogicArrayList<AvatarStreamEntry> streamEntryList = new LogicArrayList<AvatarStreamEntry>(message.StreamIds.Size());

            for (int i = 0; i < message.StreamIds.Size(); i++)
            {
                AvatarStreamEntry entry = StreamManager.GetAvatarStream(message.StreamIds[i]);

                if (entry != null && entry.GetAvatarStreamEntryType() == message.Type)
                {
                    if (message.SenderAvatarId != null && !entry.GetSenderAvatarId().Equals(message.SenderAvatarId))
                        continue;

                    streamEntryList.Add(entry);
                }
            }

            loadAvatarStreamOfTypeResponseMessage.Success = true;
            loadAvatarStreamOfTypeResponseMessage.StreamList = streamEntryList;

            ServerRequestManager.SendResponse(loadAvatarStreamOfTypeResponseMessage, message);
        }

        private static void OnRequestAllianceJoinRequestMessageReceived(RequestAllianceJoinRequestMessage message)
        {
            if (AllianceManager.TryGet(message.AllianceId, out Alliance alliance))
            {
                LogicLong avatarId = message.Avatar.GetId();

                if (alliance.Members.ContainsKey(avatarId) || alliance.Header.GetAllianceType() == AllianceType.OPEN)
                {
                    ServerRequestManager.SendResponse(new RequestAllianceJoinResponseMessage
                    {
                        ErrorReason = RequestAllianceJoinResponseMessage.Reason.GENERIC
                    }, message);
                    return;
                }

                if (alliance.Header.GetAllianceType() == AllianceType.CLOSED)
                {
                    ServerRequestManager.SendResponse(new RequestAllianceJoinResponseMessage
                    {
                        ErrorReason = RequestAllianceJoinResponseMessage.Reason.CLOSED
                    }, message);
                    return;
                }

                if (alliance.IsBanned(avatarId))
                {
                    ServerRequestManager.SendResponse(new RequestAllianceJoinResponseMessage
                    {
                        ErrorReason = RequestAllianceJoinResponseMessage.Reason.BANNED
                    }, message);
                    return;
                }

                int pendingRequestCount = 0;

                for (int i = 0; i < alliance.StreamEntryList.Size(); i++)
                {
                    StreamEntry streamEntry = StreamManager.GetAllianceStream(alliance.StreamEntryList[i]);

                    if (streamEntry != null && streamEntry.GetStreamEntryType() == StreamEntryType.JOIN_REQUEST)
                    {
                        JoinRequestAllianceStreamEntry prevJoinRequestStreamEntry = (JoinRequestAllianceStreamEntry) streamEntry;

                        if (prevJoinRequestStreamEntry.GetState() == 1)
                        {
                            pendingRequestCount += 1;

                            if (prevJoinRequestStreamEntry.GetSenderAvatarId().Equals(avatarId))
                            {
                                ServerRequestManager.SendResponse(new RequestAllianceJoinResponseMessage
                                {
                                    ErrorReason = RequestAllianceJoinResponseMessage.Reason.ALREADY_SENT
                                }, message);
                                return;
                            }
                        }
                        else if (prevJoinRequestStreamEntry.GetState() == 0 && prevJoinRequestStreamEntry.GetAgeSeconds() < 3600)
                        {
                            ServerRequestManager.SendResponse(new RequestAllianceJoinResponseMessage
                            {
                                ErrorReason = RequestAllianceJoinResponseMessage.Reason.ALREADY_SENT
                            }, message);
                            return;
                        }
                    }
                }

                if (pendingRequestCount >= 10)
                {
                    ServerRequestManager.SendResponse(new RequestAllianceJoinResponseMessage
                    {
                        ErrorReason = RequestAllianceJoinResponseMessage.Reason.TOO_MANY_PENDING_REQUESTS
                    }, message);
                    return;
                }

                if (message.Avatar.GetScore() < alliance.Header.GetRequiredScore())
                {
                    ServerRequestManager.SendResponse(new RequestAllianceJoinResponseMessage
                    {
                        ErrorReason = RequestAllianceJoinResponseMessage.Reason.NO_SCORE
                    }, message);
                    return;
                }

                if (message.Avatar.GetDuelScore() < alliance.Header.GetRequiredDuelScore())
                {
                    ServerRequestManager.SendResponse(new RequestAllianceJoinResponseMessage
                    {
                        ErrorReason = RequestAllianceJoinResponseMessage.Reason.NO_DUEL_SCORE
                    }, message);
                    return;
                }

                JoinRequestAllianceStreamEntry joinRequestAllianceStreamEntry = new JoinRequestAllianceStreamEntry();
                AllianceStreamEntryUtil.SetSenderInfo(joinRequestAllianceStreamEntry, message.Avatar);

                string requestMessage = message.Message;

                if (requestMessage != null && requestMessage.Length >= 128)
                    requestMessage = requestMessage.Substring(0, 128);

                joinRequestAllianceStreamEntry.SetMessage(requestMessage);

                StreamManager.Create(alliance.Id, joinRequestAllianceStreamEntry);
                
                alliance.AddStreamEntry(joinRequestAllianceStreamEntry);

                AllianceManager.Save(alliance);
                ServerRequestManager.SendResponse(new RequestAllianceJoinResponseMessage
                {
                    Success = true
                }, message);
            }
            else
            {
                ServerRequestManager.SendResponse(new RequestAllianceJoinResponseMessage
                {
                    ErrorReason = RequestAllianceJoinResponseMessage.Reason.GENERIC
                }, message);
            }
        }

        public override void OnReceiveSessionMessage(ServerSessionMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.START_SERVER_SESSION:
                    AllianceSessionManager.OnStartServerSessionMessageReceived((StartServerSessionMessage) message);
                    break;
                case ServerMessageType.STOP_SERVER_SESSION:
                    AllianceSessionManager.OnStopServerSessionMessageReceived((StopServerSessionMessage) message);
                    break;
                case ServerMessageType.UPDATE_SOCKET_SERVER_SESSION:
                    StreamMessageManager.OnUpdateSocketServerSessionMessageReceived((UpdateSocketServerSessionMessage) message);
                    break;
                case ServerMessageType.FORWARD_LOGIC_MESSAGE:
                    StreamMessageManager.OnForwardLogicMessageReceived((ForwardLogicMessage) message);
                    break;
                case ServerMessageType.FORWARD_LOGIC_REQUEST_MESSAGE:
                    StreamMessageManager.OnForwardLogicMessageRequestMessageReceived((ForwardLogicRequestMessage)message);
                    break;

                case ServerMessageType.SEND_AVATAR_STREAMS_TO_CLIENT:
                    StreamMessageManager.OnSendAvatarStreamsToClientMessageReceived((SendAvatarStreamsToClientMessage) message);
                    break;
            }
        }

        private static void OnUpdateSocketServerSessionMessageReceived(UpdateSocketServerSessionMessage message)
        {
            if (AllianceSessionManager.TryGet(message.SessionId, out AllianceSession session))
            {
                session.UpdateSocketServerSessionMessageReceived(message);
            }
        }

        private static void OnForwardLogicMessageReceived(ForwardLogicMessage message)
        {
            if (AllianceSessionManager.TryGet(message.SessionId, out AllianceSession session))
            {
                PiranhaMessage logicMessage = LogicMagicMessageFactory.Instance.CreateMessageByType(message.MessageType);

                if (logicMessage == null)
                    throw new Exception("logicMessage should not be NULL!");

                logicMessage.GetByteStream().SetByteArray(message.MessageBytes, message.MessageLength);
                logicMessage.SetMessageVersion(message.MessageVersion);
                logicMessage.Decode();

                if (!logicMessage.IsServerToClientMessage())
                {
                    session.LogicMessageManager.ReceiveMessage(logicMessage);
                }
            }
        }

        private static void OnForwardLogicMessageRequestMessageReceived(ForwardLogicRequestMessage message)
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
                    case AskForAllianceDataMessage.MESSAGE_TYPE:
                        AskForAllianceDataMessage askForAllianceDataMessage = (AskForAllianceDataMessage)logicMessage;
                        LogicLong allianceId = askForAllianceDataMessage.RemoveAllianceId();

                        if (AllianceManager.TryGet(allianceId, out Alliance alliance))
                        {
                            ServerMessageManager.SendMessage(StreamMessageManager.CreateForwardLogicMessage(alliance.GetAllianceDataMessage(), message.SessionId),
                                                             ServerManager.GetProxySocket(message.SessionId));
                        }

                        break;
                }
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

        private static void OnSendAvatarStreamsToClientMessageReceived(SendAvatarStreamsToClientMessage message)
        {
            AvatarStreamMessage avatarStreamMessage = new AvatarStreamMessage();
            LogicArrayList<LogicLong> ids = message.StreamIds;
            LogicArrayList<AvatarStreamEntry> avatarStreamList = new LogicArrayList<AvatarStreamEntry>(ids.Size());

            for (int i = 0; i < ids.Size(); i++)
            {
                AvatarStreamEntry avatarStreamEntry = StreamManager.GetAvatarStream(ids[i]);

                if (avatarStreamEntry != null)
                    avatarStreamList.Add(avatarStreamEntry);
            }

            avatarStreamMessage.SetStreamEntries(avatarStreamList);
            avatarStreamMessage.Encode();

            ServerMessageManager.SendMessage(StreamMessageManager.CreateForwardLogicMessage(avatarStreamMessage, message.SessionId), ServerManager.GetProxySocket(message.SessionId));

            for (int i = 0; i < avatarStreamList.Size(); i++)
            {
                AvatarStreamEntry avatarStreamEntry = avatarStreamList[i];

                if (avatarStreamEntry.IsNew())
                {
                    avatarStreamEntry.SetNew(false);
                    StreamManager.Save(avatarStreamEntry);
                }
            }
        }
        
        public override void OnReceiveCoreMessage(ServerCoreMessage message)
        {
            switch (message.GetMessageType())
            {
                case ServerMessageType.SERVER_PERFORMANCE:
                    ServerMessageManager.SendMessage(new ServerPerformanceDataMessage
                    {
                        SessionCount = AllianceSessionManager.Count
                    }, message.Sender);
                    break;
            }
        }
    }
}