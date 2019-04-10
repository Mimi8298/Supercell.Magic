namespace Supercell.Magic.Logic.Command
{
    using Supercell.Magic.Logic.Command.Battle;
    using Supercell.Magic.Logic.Command.Home;
    using Supercell.Magic.Logic.Command.Listener;
    using Supercell.Magic.Logic.Command.Server;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;
    using Supercell.Magic.Logic.Command.Debug;

    public class LogicCommandManager
    {
        private LogicLevel m_level;
        private LogicCommandManagerListener m_listener;
        private LogicArrayList<LogicCommand> m_commandList;

        public LogicCommandManager(LogicLevel level)
        {
            this.m_level = level;
            this.m_commandList = new LogicArrayList<LogicCommand>();
        }

        public void Destruct()
        {
            if (this.m_commandList != null)
            {
                for (int i = this.m_commandList.Size() - 1; i >= 0; i--)
                {
                    this.m_commandList[i].Destruct();
                    this.m_commandList.Remove(i);
                }

                this.m_commandList = null;
            }

            this.m_listener = null;
            this.m_level = null;
        }

        public void SetListener(LogicCommandManagerListener listener)
        {
            this.m_listener = listener;
        }

        public void AddCommand(LogicCommand command)
        {
            if (command != null)
            {
                if (this.m_level.GetState() == 4)
                {
                    command.Destruct();
                    command = null;
                }
                else
                {
                    this.m_commandList.Add(command);
                }
            }
        }

        public void Decode(ByteStream stream)
        {
            for (int i = this.m_commandList.Size() - 1; i >= 0; i--)
            {
                this.m_commandList[i].Destruct();
                this.m_commandList.Remove(i);
            }

            stream.EnableCheckSum(false);

            for (int i = 0, commandCount = stream.ReadInt(); i < commandCount; i++)
            {
                this.m_commandList.Add(LogicCommandManager.DecodeCommand(stream));
            }

            stream.EnableCheckSum(true);
        }

        public void Encode(ChecksumEncoder encoder)
        {
            encoder.EnableCheckSum(false);
            encoder.WriteInt(this.m_commandList.Size());

            for (int i = 0; i < this.m_commandList.Size(); i++)
            {
                LogicCommand command = this.m_commandList[i];

                encoder.WriteInt((int) command.GetCommandType());
                command.Encode(encoder);
            }

            encoder.EnableCheckSum(true);
        }

        public bool IsCommandAllowedInCurrentState(LogicCommand command)
        {
            int commandType = (int) command.GetCommandType();
            int state = this.m_level.GetState();

            if (state == 4)
            {
                Debugger.Warning("Execute command failed! Commands are not allowed in visit state.");
                return false;
            }

            if (commandType < 1000)
            {
                if (commandType < 800)
                {
                    if (commandType >= 700)
                    {
                        if (state != 2 && state != 5)
                        {
                            Debugger.Error("Execute command failed! Command is only allowed in attack state.");
                            return false;
                        }
                    }
                    else if (commandType >= 500)
                    {
                        if (state != 1)
                        {
                            Debugger.Error("Execute command failed! Command is only allowed in home state.");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void SubTick()
        {
            int subTick = this.m_level.GetLogicTime().GetTick();

            for (int i = 0; i < this.m_commandList.Size(); i++)
            {
                LogicCommand command = this.m_commandList[i];

                if (command.GetExecuteSubTick() < subTick)
                {
                    Debugger.Error(string.Format("Execute command failed! Command should have been executed already. (type={0} server_tick={1} command_tick={2})",
                                                 command.GetCommandType(),
                                                 subTick,
                                                 command.GetExecuteSubTick()));
                }

                if (command.GetExecuteSubTick() == subTick)
                {
                    if (this.IsCommandAllowedInCurrentState(command))
                    {
                        if (command.Execute(this.m_level) == 0)
                        {
                            this.m_listener?.CommandExecuted(command);
                            this.m_level.GetGameMode().GetReplay()?.RecordCommand(command);
                        }

                        this.m_commandList.Remove(i--);
                    }
                    else
                    {
                        Debugger.Error(string.Format("Execute command failed! Command not allowed in current state. (type={0} current_state={1}",
                                                     command.GetCommandType(),
                                                     this.m_level.GetState()));
                    }
                }
            }
        }

        public static LogicCommand CreateCommand(LogicCommandType type)
        {
            LogicCommand command = null;

            if ((int) type < 500)
            {
                switch (type)
                {
                    case LogicCommandType.JOIN_ALLIANCE:
                        command = new LogicJoinAllianceCommand();
                        break;
                    case LogicCommandType.LEAVE_ALLIANCE:
                        command = new LogicLeaveAllianceCommand();
                        break;
                    case LogicCommandType.CHANGE_AVATAR_NAME:
                        command = new LogicChangeAvatarNameCommand();
                        break;
                    case LogicCommandType.DONATE_ALLIANCE_UNIT:
                        command = new LogicDonateAllianceUnitCommand();
                        break;
                    case LogicCommandType.ALLIANCE_UNIT_RECEIVED:
                        command = new LogicAllianceUnitReceivedCommand();
                        break;
                    case LogicCommandType.ALLIANCE_SETTINGS_CHANGED:
                        command = new LogicAllianceSettingsChangedCommand();
                        break;
                    case LogicCommandType.DIAMONDS_ADDED:
                        command = new LogicDiamondsAddedCommand();
                        break;
                    case LogicCommandType.CHANGE_ALLIANCE_ROLE:
                        command = new LogicChangeAllianceRoleCommand();
                        break;
                    case LogicCommandType.TREASURY_WAR_REWARD:
                        command = new LogicTreasuryWarRewardCommand();
                        break;
                    case LogicCommandType.DONATE_WAR_UNIT:
                        command = new LogicDonateWarUnitCommand();
                        break;
                    case LogicCommandType.CHANGE_LEAGUE:
                        command = new LogicChangeLeagueCommand();
                        break;
                    case LogicCommandType.SHUFFLE_LEAGUE:
                        command = new LogicShuffleLeagueCommand();
                        break;
                    case LogicCommandType.UPDATE_WAR_PREFERENCE:
                        command = new LogicUpdateWarPreferenceCommand();
                        break;
                    case LogicCommandType.ALLIANCE_EXP_EARNED:
                        command = new LogicAllianceExpEarnedCommand();
                        break;
                    case LogicCommandType.CHANGE_NAME_CHANGE_STATE:
                        command = new LogicChangeNameChangeStateCommand();
                        break;
                    case LogicCommandType.LEGEND_SEASON_SCORE:
                        command = new LogicLegendSeasonScoreCommand();
                        break;
                    case LogicCommandType.TRANSACTIONS_REVOKED:
                        command = new LogicTransactionsRevokedCommand();
                        break;
                    case LogicCommandType.CHANGE_CHALLENGE_STATE:
                        command = new LogicChangeChallengeStateCommand();
                        break;
                    case LogicCommandType.SAVE_USED_ARMY:
                        command = new LogicSaveUsedArmyCommand();
                        break;
                    case LogicCommandType.CONTINUE_OFFER:
                        command = new LogicContinueOfferCommand();
                        break;
                    case LogicCommandType.UPDATE_OFFER_STATE:
                        command = new LogicUpdateOfferStateCommand();
                        break;
                    case LogicCommandType.DELIVERING_OFFER:
                        command = new LogicDeliveringOfferCommand();
                        break;
                    case LogicCommandType.DUEL_RESULT:
                        command = new LogicDuelResultCommand();
                        break;
                    case LogicCommandType.DUEL_RESOURCE_REWARD:
                        command = new LogicDuelResourceRewardCommand();
                        break;
                    case LogicCommandType.STARTING_HOME_VILLAGE2:
                        command = new LogicStartingHomeVillage2Command();
                        break;

                    default:
                    {
                        Debugger.Warning("LogicCommandManager::createCommand() - Unknown command type: " + type);
                        break;
                    }
                }
            }
            else
            {
                switch (type)
                {
                    case LogicCommandType.BUY_BUILDING:
                        command = new LogicBuyBuildingCommand();
                        break;

                    case LogicCommandType.MOVE_BUILDING:
                        command = new LogicMoveBuildingCommand();
                        break;

                    case LogicCommandType.UPGRADE_BUILDING:
                        command = new LogicUpgradeBuildingCommand();
                        break;

                    case LogicCommandType.SELL_BUILDING:
                        command = new LogicSellBuildingCommand();
                        break;
                    case LogicCommandType.SPEED_UP_CONSTRUCTION:
                        command = new LogicSpeedUpConstructionCommand();
                        break;

                    case LogicCommandType.CANCEL_CONSTRUCTION:
                        command = new LogicCancelConstructionCommand();
                        break;

                    case LogicCommandType.COLLECT_RESOURCES:
                        command = new LogicCollectResourcesCommand();
                        break;

                    case LogicCommandType.CLEAR_OBSTACLE:
                        command = new LogicClearObstacleCommand();
                        break;

                    case LogicCommandType.TRAIN_UNIT:
                        command = new LogicTrainUnitCommand();
                        break;

                    case LogicCommandType.CANCEL_UNIT_PRODUCTION:
                        command = new LogicCancelUnitProductionCommand();
                        break;

                    case LogicCommandType.BUY_TRAP:
                        command = new LogicBuyTrapCommand();
                        break;

                    case LogicCommandType.REQUEST_ALLIANCE_UNITS:
                        command = new LogicRequestAllianceUnitsCommand();
                        break;

                    case LogicCommandType.BUY_DECO:
                        command = new LogicBuyDecoCommand();
                        break;

                    case LogicCommandType.SPEED_UP_TRAINING:
                        command = new LogicSpeedUpTrainingCommand();
                        break;

                    case LogicCommandType.SPEED_UP_CLEARING:
                        command = new LogicSpeedUpClearingCommand();
                        break;

                    case LogicCommandType.CANCEL_UPGRADE_UNIT:
                        command = new LogicCancelUpgradeUnitCommand();
                        break;

                    case LogicCommandType.UPGRADE_UNIT:
                        command = new LogicUpgradeUnitCommand();
                        break;

                    case LogicCommandType.SPEED_UP_UPGRADE_UNIT:
                        command = new LogicSpeedUpUpgradeUnitCommand();
                        break;

                    case LogicCommandType.BUY_RESOURCES:
                        command = new LogicBuyResourcesCommand();
                        break;

                    case LogicCommandType.MISSION_PROGRESS:
                        command = new LogicMissionProgressCommand();
                        break;

                    case LogicCommandType.UNLOCK_BUILDING:
                        command = new LogicUnlockBuildingCommand();
                        break;

                    case LogicCommandType.FREE_WORKER:
                        command = new LogicFreeWorkerCommand();
                        break;

                    case LogicCommandType.BUY_SHIELD:
                        command = new LogicBuyShieldCommand();
                        break;

                    case LogicCommandType.CLAIM_ACHIEVEMENT_REWARD:
                        command = new LogicClaimAchievementRewardCommand();
                        break;

                    case LogicCommandType.TOGGLE_ATTACK_MODE:
                        command = new LogicToggleAttackModeCommand();
                        break;

                    case LogicCommandType.LOAD_TURRET:
                        command = new LogicLoadTurretCommand();
                        break;

                    case LogicCommandType.BOOST_BUILDING:
                        command = new LogicBoostBuildingCommand();
                        break;

                    case LogicCommandType.UPGRADE_HERO:
                        command = new LogicUpgradeHeroCommand();
                        break;

                    case LogicCommandType.SPEED_UP_HERO_UPGRADE:
                        command = new LogicSpeedUpHeroUpgradeCommand();
                        break;

                    case LogicCommandType.TOGGLE_HERO_SLEEP:
                        command = new LogicToggleHeroSleepCommand();
                        break;

                    case LogicCommandType.SPEED_UP_HERO_HEALTH:
                        command = new LogicSpeedUpHeroHealthCommand();
                        break;

                    case LogicCommandType.CANCEL_HERO_UPGRADE:
                        command = new LogicCancelHeroUpgradeCommand();
                        break;

                    case LogicCommandType.NEW_SHOP_ITEMS_SEEN:
                        command = new LogicNewShopItemsSeenCommand();
                        break;

                    case LogicCommandType.MOVE_MULTIPLE_BUILDING:
                        command = new LogicMoveMultipleBuildingsCommand();
                        break;

                    case LogicCommandType.BREAK_SHIELD:
                        command = new LogicBreakShieldCommand();
                        break;

                    case LogicCommandType.SEND_ALLIANCE_MAIL:
                        command = new LogicSendAllianceMailCommand();
                        break;

                    case LogicCommandType.LEAGUE_NOTIFICATION_SEEN:
                        command = new LogicLeagueNotificationSeenCommand();
                        break;

                    case LogicCommandType.NEWS_SEEN:
                        command = new LogicNewsSeenCommand();
                        break;

                    case LogicCommandType.TROOP_REQUEST_MESSAGE:
                        command = new LogicTroopRequestMessageCommand();
                        break;

                    case LogicCommandType.SPEED_UP_TROOP_REQUEST:
                        command = new LogicSpeedUpTroopRequestCommand();
                        break;

                    case LogicCommandType.SHARE_REPLAY:
                        command = new LogicShareReplayCommand();
                        break;

                    case LogicCommandType.ELDER_KICK:
                        command = new LogicElderKickCommand();
                        break;

                    case LogicCommandType.EDIT_MODE_SHOWN:
                        command = new LogicEditModeShownCommand();
                        break;

                    case LogicCommandType.REPAIR_TRAPS:
                        command = new LogicRepairTrapsCommand();
                        break;

                    case LogicCommandType.MOVE_BUILDING_EDIT_MODE:
                        command = new LogicMoveBuildingEditModeCommand();
                        break;

                    case LogicCommandType.SAVE_BASE_LAYOUT:
                        command = new LogicSaveBaseLayoutCommand();
                        break;

                    case LogicCommandType.UPGRADE_MULTIPLE_BUILDINGS:
                        command = new LogicUpgradeMultipleBuildingsCommand();
                        break;

                    case LogicCommandType.REMOVE_UNITS:
                        command = new LogicRemoveUnitsCommand();
                        break;

                    case LogicCommandType.RESUME_BOOST_TRAINING:
                        command = new LogicResumeBoostTrainingCommand();
                        break;

                    case LogicCommandType.SET_LAYOUT_STATE:
                        command = new LogicSetLayoutStateCommand();
                        break;

                    case LogicCommandType.SET_LAST_ALLIANCE_LEVEL:
                        command = new LogicSetLastAllianceLevelCommand();
                        break;

                    case LogicCommandType.ROTATE_BUILDING:
                        command = new LogicRotateBuildingCommand();
                        break;

                    case LogicCommandType.MOVE_ALL_BUILDINGS_EDIT_MODE:
                        command = new LogicMoveAllBuildingsEditModeCommand();
                        break;

                    case LogicCommandType.SAVE_UNIT_PRESET:
                        command = new LogicSaveUnitPresetCommand();
                        break;

                    case LogicCommandType.LOAD_UNIT_PRESET:
                        command = new LogicLoadUnitPresetCommand();
                        break;

                    case LogicCommandType.START_ALLIANCE_WAR:
                        command = new LogicStartAllianceWarCommand();
                        break;
                    case LogicCommandType.CANCEL_ALLIANCE_WAR:
                        command = new LogicCancelAllianceWarCommand();
                        break;

                    case LogicCommandType.TRANSFER_WAR_RESOURCES:
                        command = new LogicTransferWarResourcesCommand();
                        break;

                    case LogicCommandType.WAR_TROOP_REQUEST_MESSAGE:
                        command = new LogicWarTroopRequestMessageCommand();
                        break;

                    case LogicCommandType.HELP_OPENED:
                        command = new LogicHelpOpenedCommand();
                        break;

                    case LogicCommandType.CHANGE_LAYOUT:
                        command = new LogicSwitchLayoutCommand();
                        break;

                    case LogicCommandType.COPY_LAYOUT:
                        command = new LogicCopyLayoutCommand();
                        break;

                    case LogicCommandType.SET_PERSISTENT_BOOL:
                        command = new LogicSetPersistentBoolCommand();
                        break;

                    case LogicCommandType.CHANGE_WAR_PREFERENCE:
                        command = new LogicChangeWarPreferenceCommand();
                        break;

                    case LogicCommandType.CHANGE_ALLIANCE_CHAT_FILTER:
                        command = new LogicChangeAllianceChatFilterCommand();
                        break;

                    case LogicCommandType.CHANGE_HERO_MODE:
                        command = new LogicChangeHeroModeCommand();
                        break;

                    case LogicCommandType.POPUP_SEEN:
                        command = new LogicPopupSeenCommand();
                        break;
                    case LogicCommandType.FRIENDLY_BATTLE_REQUEST:
                        command = new LogicFriendlyBattleRequestCommand();
                        break;

                    case LogicCommandType.DRAG_UNIT_PRODUCTION:
                        command = new LogicDragUnitProductionCommand();
                        break;

                    case LogicCommandType.SWAP_BUILDING:
                        command = new LogicSwapBuildingCommand();
                        break;

                    case LogicCommandType.FRIEND_LIST_OPENED:
                        command = new LogicFriendListOpenedCommand();
                        break;

                    case LogicCommandType.SEND_ARRANGED_WAR_REQUEST:
                        command = new LogicSendArrangedWarRequestCommand();
                        break;

                    case LogicCommandType.BOOST_TRAINING:
                        command = new LogicBoostTrainingCommand();
                        break;

                    case LogicCommandType.LOCK_UNIT_PRODUCTION:
                        command = new LogicLockUnitProductionCommand();
                        break;

                    case LogicCommandType.CHANGE_ARMY_NAME:
                        command = new LogicChangeArmyNameCommand();
                        break;

                    case LogicCommandType.PLACE_UNPLACED_OBJECT:
                        command = new LogicPlaceUnplacedObjectCommand();
                        break;

                    case LogicCommandType.BUY_WALL_BLOCK:
                        command = new LogicBuyWallBlockCommand();
                        break;

                    case LogicCommandType.SET_CURRENT_VILLAGE:
                        command = new LogicSetCurrentVillageCommand();
                        break;

                    case LogicCommandType.TRAIN_UNIT_VILLAGE2:
                        command = new LogicTrainUnitVillage2Command();
                        break;

                    case LogicCommandType.SPEED_UP_TRAINING_VILLAGE2:
                        command = new LogicSpeedUpTrainingVillage2Command();
                        break;

                    case LogicCommandType.SPEED_UP_BOOST_COOLDOWN:
                        command = new LogicSpeedUpBoostCooldownCommand();
                        break;

                    case LogicCommandType.CANCEL_UNIT_PRODUCTION_VILLAGE_2:
                        command = new LogicCancelUnitProductionVillage2Command();
                        break;

                    case LogicCommandType.EVENT_SEEN:
                        command = new LogicEventSeenCommand();
                        break;

                    case LogicCommandType.MOVE_MULTIPLE_BUILDINGS_EDIT_MODE:
                        command = new LogicMoveMultipleBuildingsEditModeCommand();
                        break;

                    case LogicCommandType.SWAP_BUILDING_EDIT_MODE:
                        command = new LogicSwapBuildingEditModeCommand();
                        break;

                    case LogicCommandType.GEAR_UP_BUILDING:
                        command = new LogicGearUpBuildingCommand();
                        break;

                    case LogicCommandType.MATCHMAKE_VILLAGE2:
                        command = new LogicMatchmakeVillage2Command();
                        break;

                    case LogicCommandType.SPEED_UP_LOOT_LIMIT:
                        command = new LogicSpeedUpLootLimitCommand();
                        break;

                    case LogicCommandType.ACCOUNT_BOUND:
                        command = new LogicAccountBoundCommand();
                        break;

                    case LogicCommandType.SEEN_BUILDER_MENU:
                        command = new LogicSeenBuilderMenuCommand();
                        break;

                    case LogicCommandType.CHALLENGE_FRIEND_CANCEL:
                        command = new LogicChallengeFriendCancelCommand();
                        break;

                    case LogicCommandType.PLACE_ATTACKER:
                        command = new LogicPlaceAttackerCommand();
                        break;

                    case LogicCommandType.PLACE_ALLIANCE_PORTAL:
                        command = new LogicPlaceAlliancePortalCommand();
                        break;

                    case LogicCommandType.END_ATTACK_PREPARATION:
                        command = new LogicEndAttackPreparationCommand();
                        break;

                    case LogicCommandType.END_COMBAT:
                        command = new LogicEndCombatCommand();
                        break;

                    case LogicCommandType.CAST_SPELL:
                        command = new LogicCastSpellCommand();
                        break;

                    case LogicCommandType.PLACE_HERO:
                        command = new LogicPlaceHeroCommand();
                        break;

                    case LogicCommandType.TRIGGER_HERO_ABILITY:
                        command = new LogicTriggerHeroAbilityCommand();
                        break;

                    case LogicCommandType.TRIGGER_COMPONENT_TRIGGERED:
                        command = new LogicTriggerComponentTriggeredCommand();
                        break;

                    case LogicCommandType.TRIGGER_TESLA:
                        command = new LogicTriggerTeslaCommand();
                        break;

                    case LogicCommandType.CHANGE_UNIT_VILLAGE_2:
                        command = new LogicChangeUnitVillage2Command();
                        break;

                    case LogicCommandType.MATCHMAKING:
                        command = new LogicMatchmakingCommand();
                        break;

                    case LogicCommandType.DEBUG:
                        command = new LogicDebugCommand();
                        break;

                    default:
                        Debugger.Warning("LogicCommandManager::createCommand() - Unknown command type: " + type);
                        break;
                }
            }

            return command;
        }

        public static LogicCommand DecodeCommand(ByteStream stream)
        {
            LogicCommand command = LogicCommandManager.CreateCommand((LogicCommandType) stream.ReadInt());

            if (command == null)
            {
                Debugger.Warning("LogicCommandManager::decodeCommand() - command is null");
            }
            else
            {
                command.Decode(stream);
            }

            return command;
        }

        public static void EncodeCommand(ChecksumEncoder encoder, LogicCommand command)
        {
            encoder.WriteInt((int) command.GetCommandType());
            command.Encode(encoder);
        }

        public static LogicCommand LoadCommandFromJSON(LogicJSONObject jsonObject)
        {
            LogicJSONNumber jsonNumber = jsonObject.GetJSONNumber("ct");

            if (jsonNumber == null)
            {
                Debugger.Error("loadCommandFromJSON - Unknown command type");
            }
            else
            {
                LogicCommand command = LogicCommandManager.CreateCommand((LogicCommandType) jsonNumber.GetIntValue());

                if (command != null)
                {
                    command.LoadFromJSON(jsonObject.GetJSONObject("c"));
                }

                return command;
            }

            return null;
        }

        public static void SaveCommandToJSON(LogicJSONObject jsonObject, LogicCommand command)
        {
            jsonObject.Put("ct", new LogicJSONNumber((int) command.GetCommandType()));
            jsonObject.Put("c", command.GetJSONForReplay());
        }
    }
}