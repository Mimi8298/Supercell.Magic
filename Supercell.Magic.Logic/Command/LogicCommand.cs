namespace Supercell.Magic.Logic.Command
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;

    public class LogicCommand
    {
        private int m_executeSubTick;

        public LogicCommand()
        {
            this.m_executeSubTick = -1;
        }

        public virtual bool IsServerCommand()
        {
            return false;
        }

        public virtual LogicCommandType GetCommandType()
        {
            return 0;
        }

        public virtual void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_executeSubTick);
        }

        public virtual void Decode(ByteStream stream)
        {
            this.m_executeSubTick = stream.ReadInt();
        }

        public virtual int Execute(LogicLevel level)
        {
            return 0;
        }

        public virtual LogicJSONObject GetJSONForReplay()
        {
            LogicJSONObject jsonRoot = new LogicJSONObject();
            jsonRoot.Put("t", new LogicJSONNumber(this.m_executeSubTick));
            return jsonRoot;
        }

        public virtual void LoadFromJSON(LogicJSONObject jsonRoot)
        {
            LogicJSONNumber tickNumber = jsonRoot.GetJSONNumber("t");

            if (tickNumber != null)
            {
                this.m_executeSubTick = tickNumber.GetIntValue();
            }
            else
            {
                Debugger.Error("Replay - Load command from JSON failed! Execute sub tick was not found!");
            }
        }

        public virtual void Destruct()
        {
            this.m_executeSubTick = -1;
        }

        public void SetExecuteSubTick(int tick)
        {
            this.m_executeSubTick = tick;
        }

        public int GetExecuteSubTick()
        {
            return this.m_executeSubTick;
        }
    }

    public enum LogicCommandType
    {
        JOIN_ALLIANCE = 1,
        LEAVE_ALLIANCE = 2,
        CHANGE_AVATAR_NAME = 3,
        DONATE_ALLIANCE_UNIT = 4,
        ALLIANCE_UNIT_RECEIVED = 5,
        ALLIANCE_SETTINGS_CHANGED = 6,
        DIAMONDS_ADDED = 7,
        CHANGE_ALLIANCE_ROLE = 8,
        TREASURY_WAR_REWARD = 9,
        DONATE_WAR_UNIT = 10,
        CHANGE_LEAGUE = 11,
        SHUFFLE_LEAGUE = 12,
        UPDATE_WAR_PREFERENCE = 14,
        ALLIANCE_EXP_EARNED = 15,
        CHANGE_NAME_CHANGE_STATE = 16,
        LEGEND_SEASON_SCORE = 17,
        TRANSACTIONS_REVOKED = 18,
        CHANGE_CHALLENGE_STATE = 19,
        SAVE_USED_ARMY = 20,
        CONTINUE_OFFER = 22,
        UPDATE_OFFER_STATE = 23,
        DELIVERING_OFFER = 24,
        DUEL_RESULT = 25,
        DUEL_RESOURCE_REWARD = 26,
        STARTING_HOME_VILLAGE2 = 27,
        BUY_BUILDING = 500,
        MOVE_BUILDING = 501,
        UPGRADE_BUILDING = 502,
        SELL_BUILDING = 503,
        SPEED_UP_CONSTRUCTION = 504,
        CANCEL_CONSTRUCTION = 505,
        COLLECT_RESOURCES = 506,
        CLEAR_OBSTACLE = 507,
        TRAIN_UNIT = 508,
        CANCEL_UNIT_PRODUCTION = 509,
        BUY_TRAP = 510,
        REQUEST_ALLIANCE_UNITS = 511,
        BUY_DECO = 512,
        SPEED_UP_TRAINING = 513,
        SPEED_UP_CLEARING = 514,
        CANCEL_UPGRADE_UNIT = 515,
        UPGRADE_UNIT = 516,
        SPEED_UP_UPGRADE_UNIT = 517,
        BUY_RESOURCES = 518,
        MISSION_PROGRESS = 519,
        UNLOCK_BUILDING = 520,
        FREE_WORKER = 521,
        BUY_SHIELD = 522,
        CLAIM_ACHIEVEMENT_REWARD = 523,
        TOGGLE_ATTACK_MODE = 524,
        LOAD_TURRET = 525,
        BOOST_BUILDING = 526,
        UPGRADE_HERO = 527,
        SPEED_UP_HERO_UPGRADE = 528,
        TOGGLE_HERO_SLEEP = 529,
        SPEED_UP_HERO_HEALTH = 530,
        CANCEL_HERO_UPGRADE = 531,
        NEW_SHOP_ITEMS_SEEN = 532,
        MOVE_MULTIPLE_BUILDING = 533,
        BREAK_SHIELD = 534,
        SEND_ALLIANCE_MAIL = 537,
        LEAGUE_NOTIFICATION_SEEN = 538,
        NEWS_SEEN = 539,
        TROOP_REQUEST_MESSAGE = 540,
        SPEED_UP_TROOP_REQUEST = 541,
        SHARE_REPLAY = 542,
        ELDER_KICK = 543,
        EDIT_MODE_SHOWN = 544,
        REPAIR_TRAPS = 545,
        MOVE_BUILDING_EDIT_MODE = 546,
        SAVE_BASE_LAYOUT = 548,
        UPGRADE_MULTIPLE_BUILDINGS = 549,
        REMOVE_UNITS = 550,
        RESUME_BOOST_TRAINING = 551,
        SET_LAYOUT_STATE = 552,
        SET_LAST_ALLIANCE_LEVEL = 553,
        ROTATE_BUILDING = 554,
        MOVE_ALL_BUILDINGS_EDIT_MODE = 556,
        SAVE_UNIT_PRESET = 558,
        LOAD_UNIT_PRESET = 559,
        START_ALLIANCE_WAR = 560,
        CANCEL_ALLIANCE_WAR = 561,
        TRANSFER_WAR_RESOURCES = 563,
        WAR_TROOP_REQUEST_MESSAGE = 564,
        HELP_OPENED = 566,
        CHANGE_LAYOUT = 567,
        COPY_LAYOUT = 568,
        SET_PERSISTENT_BOOL = 569,
        CHANGE_WAR_PREFERENCE = 570,
        CHANGE_ALLIANCE_CHAT_FILTER = 571,
        CHANGE_HERO_MODE = 572,
        POPUP_SEEN = 573,
        FRIENDLY_BATTLE_REQUEST = 574,
        DRAG_UNIT_PRODUCTION = 576,
        SWAP_BUILDING = 577,
        FRIEND_LIST_OPENED = 579,
        SEND_ARRANGED_WAR_REQUEST = 581,
        BOOST_TRAINING = 584,
        LOCK_UNIT_PRODUCTION = 585,
        CHANGE_ARMY_NAME = 586,
        PLACE_UNPLACED_OBJECT = 589,
        BUY_WALL_BLOCK = 590,
        SET_CURRENT_VILLAGE = 591,
        TRAIN_UNIT_VILLAGE2 = 592,
        SPEED_UP_TRAINING_VILLAGE2 = 593,
        SPEED_UP_BOOST_COOLDOWN = 595,
        CANCEL_UNIT_PRODUCTION_VILLAGE_2 = 596,
        EVENT_SEEN = 597,
        MOVE_MULTIPLE_BUILDINGS_EDIT_MODE = 598,
        SWAP_BUILDING_EDIT_MODE = 599,
        GEAR_UP_BUILDING = 600,
        MATCHMAKE_VILLAGE2 = 601,
        SPEED_UP_LOOT_LIMIT = 602,
        ACCOUNT_BOUND = 603,
        SEEN_BUILDER_MENU = 604,
        CHALLENGE_FRIEND_CANCEL = 605,
        PLACE_ATTACKER = 700,
        PLACE_ALLIANCE_PORTAL = 701,
        END_ATTACK_PREPARATION = 702,
        END_COMBAT = 703,
        CAST_SPELL = 704,
        PLACE_HERO = 705,
        TRIGGER_HERO_ABILITY = 706,
        TRIGGER_COMPONENT_TRIGGERED = 709,
        TRIGGER_TESLA = 710,
        CHANGE_UNIT_VILLAGE_2 = 711,
        MATCHMAKING = 800,
        DEBUG = 1000
    }
}