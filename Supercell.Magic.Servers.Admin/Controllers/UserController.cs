namespace Supercell.Magic.Servers.Admin.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Command.Debug;
    using Supercell.Magic.Logic.Message.Home;
    using Supercell.Magic.Servers.Admin.Attribute;
    using Supercell.Magic.Servers.Admin.Helper;
    using Supercell.Magic.Servers.Admin.Logic;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Servers.Core.Network;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Session;
    using Supercell.Magic.Servers.Core.Util;
    using StackExchange.Redis;
    using ServerManager = Supercell.Magic.Servers.Core.Network.ServerManager;

    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : Controller
    {
        [HttpPost("search")]
        [AuthCheck(UserRole.DEFAULT)]
        public async Task<JObject> Search([FromBody] UserSearchRequest request)
        {
            JArray result = await UserManager.Search(request.Name, request.Level, request.Score, request.AllianceName);

            if (result == null)
                return this.BuildResponse(HttpStatusCode.InternalServerError);
            return this.BuildResponse(HttpStatusCode.OK).AddAttribute("result", result);
        }

        [HttpGet("{id}")]
        public async Task<JObject> Get(long id, [FromQuery(Name = "pass")] string passToken)
        {
            Task<AccountDocument> taskAccount = UserManager.GetAccount(id);
            Task<GameDocument> taskDocument = UserManager.GetAvatar(id);

            AccountDocument accountDocument =  await taskAccount;
            GameDocument gameDocument = await taskDocument;

            if (accountDocument == null || gameDocument == null)
                return this.BuildResponse(HttpStatusCode.InternalServerError);
            if (this.GetUserRole() <= UserRole.NULL && passToken != accountDocument.PassToken)
                return this.BuildResponse(HttpStatusCode.Forbidden);

            bool online = await ServerAdmin.SessionDatabase.Exists(id);

            JObject response = this.BuildResponse(HttpStatusCode.OK).AddAttribute("online", online);
            JObject accountObj = new JObject();

            if (this.GetUserRole() >= UserRole.MODERATOR)
            {
                accountObj.Add("id", (long) accountDocument.Id);
                accountObj.Add("passToken", accountDocument.PassToken);
            }

            accountObj.Add("status", accountDocument.State.ToString());

            if (accountDocument.StateArg != null)
            {
                switch (accountDocument.State)
                {
                    case AccountState.BANNED:
                        accountObj.Add("reason", accountDocument.StateArg);
                        break;
                    case AccountState.LOCKED:
                        accountObj.Add("unlockCode", accountDocument.StateArg);
                        break;
                }
            }

            accountObj.Add("rank", accountDocument.Rank.ToString());
            accountObj.Add("country", accountDocument.Country);
            accountObj.Add("createTime", TimeUtil.GetDateTimeFromTimestamp(accountDocument.CreateTime).ToString("F"));
            accountObj.Add("lastSessionTime", TimeUtil.GetDateTimeFromTimestamp(accountDocument.LastSessionTime).ToString("F"));
            accountObj.Add("playTimeSecs", UserController.GetSecondsToTime(accountDocument.PlayTimeSeconds));

            response.Add("account", accountObj);

            JObject avatarObj = new JObject();

            avatarObj.Add("name", gameDocument.LogicClientAvatar.GetName());
            avatarObj.Add("nameChanged", gameDocument.LogicClientAvatar.GetNameChangeState() >= 1);
            avatarObj.Add("diamonds", gameDocument.LogicClientAvatar.GetDiamonds());
            avatarObj.Add("lvl", gameDocument.LogicClientAvatar.GetExpLevel());

            response.Add("avatar", avatarObj);

            if (gameDocument.LogicClientAvatar.IsInAlliance())
            {
                JObject allianceObj = new JObject();

                allianceObj.Add("name", gameDocument.LogicClientAvatar.GetAllianceName());
                allianceObj.Add("badgeId", gameDocument.LogicClientAvatar.GetAllianceBadgeId());
                allianceObj.Add("lvl", gameDocument.LogicClientAvatar.GetAllianceLevel());

                switch (gameDocument.LogicClientAvatar.GetAllianceRole())
                {
                    case LogicAvatarAllianceRole.MEMBER:
                        allianceObj.Add("role", "Member");
                        break;
                    case LogicAvatarAllianceRole.ELDER:
                        allianceObj.Add("role", "Elder");
                        break;
                    case LogicAvatarAllianceRole.LEADER:
                        allianceObj.Add("role", "Leader");
                        break;
                    case LogicAvatarAllianceRole.CO_LEADER:
                        allianceObj.Add("role", "Co Leader");
                        break;
                }

                response.Add("alliance", allianceObj);
            }

            if (online)
            {
                JArray opCommandArray = new JArray();

                opCommandArray.Add((int) OpCommandType.RESET_ACCOUNT);
                opCommandArray.Add((int) OpCommandType.LOAD_PRESET_VILLAGE);
                opCommandArray.Add((int) OpCommandType.ATTACK_GENERATED_VILLAGE);
                opCommandArray.Add((int) OpCommandType.ATTACK_RANDOM_VILLAGE);
                opCommandArray.Add((int) OpCommandType.ATTACK_MY_VILLAGE);

                response.Add("op-cmd", opCommandArray);

                JArray debugCommandArray = new JArray();

                if (this.GetUserRole() >= UserRole.TESTER)
                {
                    debugCommandArray.Add((int)DebugCommandType.FAST_FORWARD_1_MIN);
                    debugCommandArray.Add((int) DebugCommandType.FAST_FORWARD_1_HOUR);
                    debugCommandArray.Add((int) DebugCommandType.FAST_FORWARD_24_HOUR);

                    debugCommandArray.Add((int)DebugCommandType.LOCK_CLAN_CASTLE);
                    debugCommandArray.Add((int)DebugCommandType.CAUSE_DAMAGE);
                    debugCommandArray.Add((int)DebugCommandType.TOGGLE_INVULNERABILITY);
                    debugCommandArray.Add((int)DebugCommandType.TRAVEL);
                    debugCommandArray.Add((int)DebugCommandType.TOGGLE_RED);

                    debugCommandArray.Add((int)DebugCommandType.ADD_UNIT);
                    debugCommandArray.Add((int) DebugCommandType.ADD_UNITS);
                    debugCommandArray.Add((int)DebugCommandType.ADD_PRESET_TROOPS);
                    debugCommandArray.Add((int)DebugCommandType.ADD_ALLIANCE_UNITS);
                    debugCommandArray.Add((int)DebugCommandType.DEPLOY_ALL_TROOPS);
                    debugCommandArray.Add((int)DebugCommandType.REMOVE_UNITS);
                    debugCommandArray.Add((int)DebugCommandType.INCREASE_HERO_LEVELS);
                    debugCommandArray.Add((int)DebugCommandType.RESET_HERO_LEVELS);
                    debugCommandArray.Add((int)DebugCommandType.SET_MAX_UNIT_SPELL_LEVELS);
                    debugCommandArray.Add((int)DebugCommandType.SET_MAX_HERO_LEVELS);

                    debugCommandArray.Add((int)DebugCommandType.COMPLETE_TUTORIAL);
                    debugCommandArray.Add((int)DebugCommandType.COMPLETE_HOME_TUTORIALS);
                    debugCommandArray.Add((int)DebugCommandType.COMPLETE_WAR_TUTORIAL);
                    debugCommandArray.Add((int)DebugCommandType.RESET_ALL_TUTORIALS);
                    debugCommandArray.Add((int)DebugCommandType.RESET_WAR_TUTORIAL);

                    debugCommandArray.Add((int)DebugCommandType.SHIELD_TO_HALF);
                    debugCommandArray.Add((int)DebugCommandType.INCREASE_TROPHIES);
                    debugCommandArray.Add((int)DebugCommandType.DECREASE_TROPHIES);
                    debugCommandArray.Add((int)DebugCommandType.ADD_100_TROPHIES);
                    debugCommandArray.Add((int)DebugCommandType.REMOVE_100_TROPHIES);
                    debugCommandArray.Add((int)DebugCommandType.ADD_1000_TROPHIES);
                    debugCommandArray.Add((int)DebugCommandType.REMOVE_1000_TROPHIES);
                    debugCommandArray.Add((int)DebugCommandType.SET_RANDOM_TROPHIES);
                    
                    debugCommandArray.Add((int)DebugCommandType.INCREASE_XP_LEVEL);
                    debugCommandArray.Add((int)DebugCommandType.RANDOM_RESOURCES_TROPHY_XP);

                    debugCommandArray.Add((int)DebugCommandType.ADD_1000_CLAN_XP);
                    debugCommandArray.Add((int)DebugCommandType.RANDOM_ALLIANCE_EXP_LEVEL);

                }

                debugCommandArray.Add((int)DebugCommandType.ADD_GEMS);
                debugCommandArray.Add((int)DebugCommandType.ADD_RESOURCES);
                debugCommandArray.Add((int)DebugCommandType.ADD_WAR_RESOURCES);
                debugCommandArray.Add((int)DebugCommandType.REMOVE_RESOURCES);
                debugCommandArray.Add((int)DebugCommandType.REMOVE_WAR_RESOURCES);
                debugCommandArray.Add((int)DebugCommandType.COLLECT_WAR_RESOURCES);
                debugCommandArray.Add((int)DebugCommandType.UNLOCK_MAP);
                debugCommandArray.Add((int)DebugCommandType.RESET_MAP_PROGRESS);
                debugCommandArray.Add((int)DebugCommandType.PAUSE_ALL_BOOSTS);
                debugCommandArray.Add((int)DebugCommandType.GIVE_REENGAGEMENT_LOOT_FOR_30_DAYS);


                debugCommandArray.Add((int)DebugCommandType.UPGRADE_ALL_BUILDINGS);
                debugCommandArray.Add((int)DebugCommandType.UPGRADE_TO_MAX_FOR_TH);
                debugCommandArray.Add((int)DebugCommandType.REMOVE_OBSTACLES);
                debugCommandArray.Add((int)DebugCommandType.DISARM_TRAPS);
                debugCommandArray.Add((int)DebugCommandType.REMOVE_ALL_AMMO);
                debugCommandArray.Add((int)DebugCommandType.RESET_ALL_LAYOUTS);
                debugCommandArray.Add((int)DebugCommandType.LOAD_LEVEL);

                response.Add("debug-cmd", debugCommandArray);
            }

            if (this.GetUserRole() >= UserRole.MODERATOR)
            {
                JArray administrationCommandArray = new JArray();

                switch (accountDocument.State)
                {
                    case AccountState.NORMAL:
                        administrationCommandArray.Add((int)AdminCommandType.BAN_ACCOUNT);
                        administrationCommandArray.Add((int)AdminCommandType.LOCK_ACCOUNT);
                        break;
                    case AccountState.LOCKED:
                        administrationCommandArray.Add((int)AdminCommandType.UNLOCK_ACCOUNT);
                        break;
                    case AccountState.BANNED:
                        administrationCommandArray.Add((int)AdminCommandType.UNBAN_ACCOUNT);
                        break;
                }

                administrationCommandArray.Add((int)AdminCommandType.CHANGE_RANK);

                if (online)
                    administrationCommandArray.Add((int) AdminCommandType.DISCONNECT);

                response.Add("administration-cmd", administrationCommandArray);
            }

            return response;
        }

        [HttpPost("commands/admin/{id}")]
        [AuthCheck(UserRole.MODERATOR)]
        public async Task<JObject> ExecuteAdministrationCommand(int id, [FromQuery(Name = "userId")] long accountId, [FromBody] AdministrationCommandRequest administrationCommand)
        {
            AccountDocument accountDocument = await UserManager.GetAccount(accountId);

            if (accountDocument == null)
                return this.BuildResponse(HttpStatusCode.InternalServerError);

            switch ((AdminCommandType) id)
            {
                case AdminCommandType.BAN_ACCOUNT:
                    if (accountDocument.State == AccountState.NORMAL)
                    {
                        accountDocument.State = AccountState.BANNED;
                        accountDocument.StateArg = administrationCommand.GetArg<string>("reason");

                        UserManager.SaveAccount(accountDocument);

                        this.Disconnect(accountId);
                        return this.BuildResponse(HttpStatusCode.OK);
                    }

                    break;
                case AdminCommandType.LOCK_ACCOUNT:
                    if (accountDocument.State == AccountState.NORMAL)
                    {
                        char[] chars = new char[12];
                        for (int i = 0; i < 12; i++) chars[i] = AccountDocument.CHARS[ServerCore.Random.Rand(AccountDocument.CHARS.Length)];
                        string code = new string(chars);

                        accountDocument.State = AccountState.LOCKED;
                        accountDocument.StateArg = code;

                        UserManager.SaveAccount(accountDocument);

                        this.Disconnect(accountId);
                        return this.BuildResponse(HttpStatusCode.OK).AddAttribute("code", code);
                    }

                    break;
                case AdminCommandType.UNLOCK_ACCOUNT:
                    if (accountDocument.State == AccountState.LOCKED)
                    {
                        accountDocument.State = AccountState.NORMAL;
                        accountDocument.StateArg = null;

                        UserManager.SaveAccount(accountDocument);

                        return this.BuildResponse(HttpStatusCode.OK);
                    }

                    break;
                case AdminCommandType.UNBAN_ACCOUNT:
                    if (accountDocument.State == AccountState.BANNED)
                    {
                        accountDocument.State = AccountState.NORMAL;
                        accountDocument.StateArg = null;

                        UserManager.SaveAccount(accountDocument);

                        return this.BuildResponse(HttpStatusCode.OK);
                    }

                    break;
                case AdminCommandType.CHANGE_RANK:
                    int rankType = (int) administrationCommand.GetArg<long>("rank");

                    if (rankType >= 0 && rankType <= 2 && (int) accountDocument.Rank != rankType)
                    {
                        accountDocument.Rank = (AccountRankingType) rankType;

                        UserManager.SaveAccount(accountDocument);

                        this.Disconnect(accountId);
                        return this.BuildResponse(HttpStatusCode.OK);
                    }

                    break;
                case AdminCommandType.DISCONNECT:
                    this.Disconnect(accountId);
                    return this.BuildResponse(HttpStatusCode.OK);
            }

            return this.BuildResponse(HttpStatusCode.InternalServerError);
        }

        private async void Disconnect(long id)
        {
            RedisValue currentSession = await ServerAdmin.SessionDatabase.Get(id);

            if (!currentSession.IsNull)
            {
                long sessionId = long.Parse(currentSession);

                ServerMessageManager.SendMessage(new StopSessionMessage
                {
                    SessionId = sessionId
                }, ServerManager.GetProxySocket(sessionId));
            }
        }

        [HttpPost("commands/debug/{id}")]
        public async Task<JObject> ExecuteDebugCommand(int id, [FromQuery(Name = "userId")] long accountId, [FromQuery(Name = "pass")] string passToken)
        {
            AccountDocument accountDocument = await UserManager.GetAccount(accountId);

            if (accountDocument == null)
                return this.BuildResponse(HttpStatusCode.InternalServerError);
            if (this.GetUserRole() <= UserRole.NULL && passToken != accountDocument.PassToken)
                return this.BuildResponse(HttpStatusCode.Forbidden);

            RedisValue currentSession = await ServerAdmin.SessionDatabase.Get(accountDocument.Id);

            if (currentSession.IsNull)
                return this.BuildResponse(HttpStatusCode.Forbidden).AddAttribute("reason", "No client connected.");

            LogicDebugCommand logicDebugCommand = null;

            switch ((DebugCommandType) id)
            {
                case DebugCommandType.FAST_FORWARD_1_HOUR:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.FAST_FORWARD_1_HOUR);
                    break;
                case DebugCommandType.FAST_FORWARD_24_HOUR:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.FAST_FORWARD_24_HOUR);
                    break;
                case DebugCommandType.ADD_UNITS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.ADD_UNITS);
                    break;
                case DebugCommandType.ADD_RESOURCES:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.ADD_RESOURCES);
                    break;
                case DebugCommandType.INCREASE_XP_LEVEL:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.INCREASE_XP_LEVEL);
                    break;
                case DebugCommandType.UPGRADE_ALL_BUILDINGS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.UPGRADE_ALL_BUILDINGS);
                    break;
                case DebugCommandType.COMPLETE_TUTORIAL:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.COMPLETE_TUTORIAL);
                    break;
                case DebugCommandType.UNLOCK_MAP:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.UNLOCK_MAP);
                    break;
                case DebugCommandType.SHIELD_TO_HALF:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.SHIELD_TO_HALF);
                    break;
                case DebugCommandType.FAST_FORWARD_1_MIN:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.FAST_FORWARD_1_MIN);
                    break;
                case DebugCommandType.INCREASE_TROPHIES:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.INCREASE_TROPHIES);
                    break;
                case DebugCommandType.DECREASE_TROPHIES:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.DECREASE_TROPHIES);
                    break;
                case DebugCommandType.ADD_ALLIANCE_UNITS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.ADD_ALLIANCE_UNITS);
                    break;
                case DebugCommandType.INCREASE_HERO_LEVELS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.INCREASE_HERO_LEVELS);
                    break;
                case DebugCommandType.REMOVE_RESOURCES:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.REMOVE_RESOURCES);
                    break;
                case DebugCommandType.RESET_MAP_PROGRESS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.RESET_MAP_PROGRESS);
                    break;
                case DebugCommandType.DEPLOY_ALL_TROOPS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.DEPLOY_ALL_TROOPS);
                    break;
                case DebugCommandType.ADD_100_TROPHIES:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.ADD_100_TROPHIES);
                    break;
                case DebugCommandType.REMOVE_100_TROPHIES:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.REMOVE_100_TROPHIES);
                    break;
                case DebugCommandType.UPGRADE_TO_MAX_FOR_TH:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.UPGRADE_TO_MAX_FOR_TH);
                    break;
                case DebugCommandType.REMOVE_UNITS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.REMOVE_UNITS);
                    break;
                case DebugCommandType.DISARM_TRAPS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.DISARM_TRAPS);
                    break;
                case DebugCommandType.REMOVE_OBSTACLES:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.REMOVE_OBSTACLES);
                    break;
                case DebugCommandType.RESET_HERO_LEVELS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.RESET_HERO_LEVELS);
                    break;
                case DebugCommandType.COLLECT_WAR_RESOURCES:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.COLLECT_WAR_RESOURCES);
                    break;
                case DebugCommandType.SET_RANDOM_TROPHIES:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.SET_RANDOM_TROPHIES);
                    break;
                case DebugCommandType.COMPLETE_WAR_TUTORIAL:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.COMPLETE_WAR_TUTORIAL);
                    break;
                case DebugCommandType.ADD_WAR_RESOURCES:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.ADD_WAR_RESOURCES);
                    break;
                case DebugCommandType.REMOVE_WAR_RESOURCES:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.REMOVE_WAR_RESOURCES);
                    break;
                case DebugCommandType.RESET_WAR_TUTORIAL:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.RESET_WAR_TUTORIAL);
                    break;
                case DebugCommandType.ADD_UNIT:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.ADD_UNIT);
                    break;
                case DebugCommandType.SET_MAX_UNIT_SPELL_LEVELS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.SET_MAX_UNIT_SPELL_LEVELS);
                    break;
                case DebugCommandType.REMOVE_ALL_AMMO:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.REMOVE_ALL_AMMO);
                    break;
                case DebugCommandType.RESET_ALL_LAYOUTS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.RESET_ALL_LAYOUTS);
                    break;
                case DebugCommandType.LOCK_CLAN_CASTLE:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.LOCK_CLAN_CASTLE);
                    break;
                case DebugCommandType.RANDOM_RESOURCES_TROPHY_XP:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.RANDOM_RESOURCES_TROPHY_XP);
                    break;
                case DebugCommandType.LOAD_LEVEL:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.LOAD_LEVEL);
                    logicDebugCommand.SetDebugString(UserManager.GetPresetLevel());
                    break;
                case DebugCommandType.UPGRADE_BUILDING:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.UPGRADE_BUILDING);
                    break;
                case DebugCommandType.UPGRADE_BUILDINGS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.UPGRADE_BUILDINGS);
                    break;
                case DebugCommandType.ADD_1000_CLAN_XP:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.ADD_1000_CLAN_XP);
                    break;
                case DebugCommandType.RESET_ALL_TUTORIALS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.RESET_ALL_TUTORIALS);
                    break;
                case DebugCommandType.ADD_1000_TROPHIES:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.ADD_1000_TROPHIES);
                    break;
                case DebugCommandType.REMOVE_1000_TROPHIES:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.REMOVE_1000_TROPHIES);
                    break;
                case DebugCommandType.CAUSE_DAMAGE:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.CAUSE_DAMAGE);
                    break;
                case DebugCommandType.SET_MAX_HERO_LEVELS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.SET_MAX_HERO_LEVELS);
                    break;
                case DebugCommandType.ADD_PRESET_TROOPS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.ADD_PRESET_TROOPS);
                    break;
                case DebugCommandType.TOGGLE_INVULNERABILITY:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.TOGGLE_INVULNERABILITY);
                    break;
                case DebugCommandType.ADD_GEMS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.ADD_GEMS);
                    break;
                case DebugCommandType.PAUSE_ALL_BOOSTS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.PAUSE_ALL_BOOSTS);
                    break;
                case DebugCommandType.TRAVEL:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.TRAVEL);
                    break;
                case DebugCommandType.TOGGLE_RED:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.TOGGLE_RED);
                    break;
                case DebugCommandType.COMPLETE_HOME_TUTORIALS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.COMPLETE_HOME_TUTORIALS);
                    break;
                case DebugCommandType.UNLOCK_SHIPYARD:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.UNLOCK_SHIPYARD);
                    break;
                case DebugCommandType.GIVE_REENGAGEMENT_LOOT_FOR_30_DAYS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.GIVE_REENGAGEMENT_LOOT_FOR_30_DAYS);
                    break;
                case DebugCommandType.ADD_FREE_UNITS:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.ADD_FREE_UNITS);
                    break;
                case DebugCommandType.RANDOM_ALLIANCE_EXP_LEVEL:
                    logicDebugCommand = new LogicDebugCommand(LogicDebugActionType.RANDOM_ALLIANCE_EXP_LEVEL);
                    break;
            }

            if (logicDebugCommand == null)
                return this.BuildResponse(HttpStatusCode.InternalServerError);

            long sessionId = long.Parse(currentSession);

            AvailableServerCommandMessage availableServerCommandMessage = new AvailableServerCommandMessage();

            availableServerCommandMessage.SetServerCommand(logicDebugCommand);
            availableServerCommandMessage.Encode();

            ServerMessageManager.SendMessage(new ForwardLogicMessage
            {
                MessageType = availableServerCommandMessage.GetMessageType(),
                MessageLength = availableServerCommandMessage.GetEncodingLength(),
                MessageVersion = (short)availableServerCommandMessage.GetMessageVersion(),
                MessageBytes = availableServerCommandMessage.GetByteStream().GetByteArray(),
                SessionId = sessionId
            }, ServerManager.GetProxySocket(sessionId));

            return this.BuildResponse(HttpStatusCode.OK);
        }

        private UserRole GetUserRole()
        {
            if (this.Request.Headers.TryGetValue("token", out StringValues token) && AuthManager.TryGetSession(token, out SessionEntry session))
                return session.User.Role;
            return UserRole.NULL;
        }

        private static string GetSecondsToTime(int secs)
        {
            if (secs >= 60)
            {
                if (secs >= 3600)
                {
                    if (secs >= 86400)
                    {
                        if (secs >= 31536000)
                            return string.Format("{0}y {1}d {2}h {3}mn {4}secs", secs / 31536000, secs % 31536000 / 86400, secs % 31536000 % 86400 / 3600, secs % 31536000 % 86400 % 3600 / 60,
                                                 secs % 31536000 % 86400 % 3600 % 60);
                        return string.Format("{0}d {1}h {2}mn {3}secs", secs / 86400, secs % 86400 / 3600, secs % 86400 % 3600 / 60, secs % 86400 % 3600 % 60);
                    }

                    return string.Format("{0}h {1}mn {2}secs", secs / 3600, secs % 3600 / 60, secs % 3600 % 60);
                }

                return string.Format("{0}mn {1}secs", secs / 60, secs % 60);
            }

            return string.Format("{0}secs", secs);
        }

        public class UserSearchRequest
        {
            public string Name { get; set; }
            public int Level { get; set; }
            public int Score { get; set; }
            public string AllianceName { get; set; }
        }

        public class AdministrationCommandRequest
        {
            public AdministrationCommandArgEntry[] Args { get; set; }

            public T GetArg<T>(string name)
            {
                if (this.Args != null)
                {
                    for (int i = 0; i < this.Args.Length; i++)
                    {
                        if (this.Args[i].Name == name)
                            return (T) this.Args[i].Value;
                    }
                }

                return default(T);
            }

            public class AdministrationCommandArgEntry
            {
                public string Name { get; set; }
                public object Value { get; set; }
            }
        }

        public enum OpCommandType
        {
            RESET_ACCOUNT,
            LOAD_PRESET_VILLAGE,
            ATTACK_GENERATED_VILLAGE,
            ATTACK_RANDOM_VILLAGE,
            ATTACK_MY_VILLAGE
        }

        public enum DebugCommandType
        {
            FAST_FORWARD_1_HOUR,
            FAST_FORWARD_24_HOUR,
            ADD_UNITS,
            ADD_RESOURCES,
            INCREASE_XP_LEVEL,
            UPGRADE_ALL_BUILDINGS,
            COMPLETE_TUTORIAL,
            UNLOCK_MAP,
            SHIELD_TO_HALF,
            FAST_FORWARD_1_MIN,
            INCREASE_TROPHIES,
            DECREASE_TROPHIES,
            ADD_ALLIANCE_UNITS,
            INCREASE_HERO_LEVELS,
            REMOVE_RESOURCES,
            RESET_MAP_PROGRESS,
            DEPLOY_ALL_TROOPS,
            ADD_100_TROPHIES,
            REMOVE_100_TROPHIES,
            UPGRADE_TO_MAX_FOR_TH,
            REMOVE_UNITS,
            DISARM_TRAPS,
            REMOVE_OBSTACLES,
            RESET_HERO_LEVELS,
            COLLECT_WAR_RESOURCES,
            SET_RANDOM_TROPHIES,
            COMPLETE_WAR_TUTORIAL,
            ADD_WAR_RESOURCES,
            REMOVE_WAR_RESOURCES,
            RESET_WAR_TUTORIAL,
            ADD_UNIT,
            SET_MAX_UNIT_SPELL_LEVELS,
            REMOVE_ALL_AMMO,
            RESET_ALL_LAYOUTS,
            LOCK_CLAN_CASTLE,
            RANDOM_RESOURCES_TROPHY_XP,
            LOAD_LEVEL,
            UPGRADE_BUILDING,
            UPGRADE_BUILDINGS,
            ADD_1000_CLAN_XP,
            RESET_ALL_TUTORIALS,
            ADD_1000_TROPHIES,
            REMOVE_1000_TROPHIES,
            CAUSE_DAMAGE,
            SET_MAX_HERO_LEVELS,
            ADD_PRESET_TROOPS,
            TOGGLE_INVULNERABILITY,
            ADD_GEMS,
            PAUSE_ALL_BOOSTS,
            TRAVEL,
            TOGGLE_RED,
            COMPLETE_HOME_TUTORIALS,
            UNLOCK_SHIPYARD,
            GIVE_REENGAGEMENT_LOOT_FOR_30_DAYS,
            ADD_FREE_UNITS,
            RANDOM_ALLIANCE_EXP_LEVEL
        }

        public enum AdminCommandType
        {
            BAN_ACCOUNT,
            LOCK_ACCOUNT,
            UNLOCK_ACCOUNT,
            UNBAN_ACCOUNT,
            CHANGE_RANK,
            DISCONNECT
        }
    }
}