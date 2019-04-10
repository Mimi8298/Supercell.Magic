namespace Supercell.Magic.Logic.Calendar
{
    using System;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;

    public class LogicCalendarFunction
    {
        private LogicCalendarEventFunctionData m_functionData;
        private LogicCalendarEvent m_calendarEvent;
        private LogicCalendarErrorHandler m_errorHandler;
        private LogicArrayList<string> m_parameters;

        private int m_idx;

        public LogicCalendarFunction(LogicCalendarEvent calendarEvent, int idx, LogicJSONObject jsonObject, LogicCalendarErrorHandler errorHandler)
        {
            this.m_parameters = new LogicArrayList<string>();

            this.m_calendarEvent = calendarEvent;
            this.m_errorHandler = errorHandler;

            this.m_idx = idx;

            this.Load(jsonObject);
        }

        public void Destruct()
        {
            this.m_functionData = null;
            this.m_calendarEvent = null;
            this.m_errorHandler = null;
            this.m_parameters = null;
        }

        public void Load(LogicJSONObject jsonObject)
        {
            Debugger.DoAssert(this.m_errorHandler != null, "LogicCalendarErrorHandler must not be NULL!");

            if (jsonObject == null)
            {
                this.m_errorHandler.ErrorFunction(this.m_calendarEvent, this, "Event function malformed.");
                return;
            }

            string name = LogicJSONHelper.GetString(jsonObject, "name");
            this.m_functionData = LogicDataTables.GetCalendarEventFunctionByName(name, null);

            if (this.m_functionData == null)
            {
                this.m_errorHandler.ErrorFunction(this.m_calendarEvent, this, string.Format("event function '{0}' not found.", name));
                return;
            }

            LogicJSONArray parameterArray = jsonObject.GetJSONArray("parameters");

            if (parameterArray != null)
            {
                for (int i = 0; i < parameterArray.Size(); i++)
                {
                    this.m_parameters.Add(parameterArray.GetJSONString(i).GetStringValue());
                }
            }

            this.LoadingFinished();
        }

        public void LoadingFinished()
        {
            if (this.m_functionData.IsDeprecated())
            {
                this.m_errorHandler.ErrorFunction(this.m_calendarEvent, this, "Function is deprecated.");
            }

            if (this.m_parameters.Size() != this.m_functionData.GetParameterCount())
            {
                this.m_errorHandler.ErrorFunction(this.m_calendarEvent, this,
                                                  string.Format("Invalid number of parameters defined. Expected {0} got {1}.", this.m_functionData.GetParameterCount(),
                                                                this.m_parameters.Size()));
            }

            for (int i = 0; i < this.m_parameters.Size(); i++)
            {
                int type = this.m_functionData.GetParameterType(i);

                switch (type)
                {
                    case LogicCalendarEventFunctionData.PARAMETER_TYPE_BOOLEAN:
                        this.GetBoolParameter(i);
                        break;
                    case LogicCalendarEventFunctionData.PARAMETER_TYPE_INT:
                        this.GetIntParameter(i);
                        break;
                    case LogicCalendarEventFunctionData.PARAMETER_TYPE_STRING:
                        this.GetStringParameter(i);
                        break;
                    case LogicCalendarEventFunctionData.PARAMETER_TYPE_TROOP:
                        this.GetDataParameter(i, LogicDataType.CHARACTER);
                        break;
                    case LogicCalendarEventFunctionData.PARAMETER_TYPE_SPELL:
                        this.GetDataParameter(i, LogicDataType.SPELL);
                        break;
                    case LogicCalendarEventFunctionData.PARAMETER_TYPE_BUILDING:
                        this.GetDataParameter(i, LogicDataType.BUILDING);
                        break;
                    case LogicCalendarEventFunctionData.PARAMETER_TYPE_TRAP:
                        this.GetDataParameter(i, LogicDataType.TRAP);
                        break;
                    case LogicCalendarEventFunctionData.PARAMETER_TYPE_BUNDLE:
                        this.GetDataParameter(i, LogicDataType.GEM_BUNDLE);
                        break;
                    case LogicCalendarEventFunctionData.PARAMETER_TYPE_BILLING_PACKAGE:
                        this.GetDataParameter(i, LogicDataType.BILLING_PACKAGE);
                        break;
                    case LogicCalendarEventFunctionData.PARAMETER_TYPE_ANIMATION:
                        // TODO: Implement this (client)
                        break;
                    case LogicCalendarEventFunctionData.PARAMETER_TYPE_HERO:
                        this.GetDataParameter(i, LogicDataType.HERO);
                        break;
                    default:
                        this.m_errorHandler.ErrorFunction(this.m_calendarEvent, this, this.m_functionData.GetParameterName(i),
                                                          string.Format("Unhandled parameter type {0}!", type));
                        break;
                }
            }
        }

        public LogicJSONObject Save()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();
            LogicJSONArray parameterArray = new LogicJSONArray(this.m_parameters.Size());

            jsonObject.Put("name", new LogicJSONString(this.m_functionData.GetName()));

            for (int i = 0; i < this.m_parameters.Size(); i++)
            {
                parameterArray.Add(new LogicJSONString(this.m_parameters[i]));
            }

            jsonObject.Put("parameters", parameterArray);

            return jsonObject;
        }

        public void ApplyToEvent(LogicCalendarEvent calendarEvent)
        {
            switch (this.m_functionData.GetFunctionType())
            {
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_TROOP_TRAINING_BOOST:
                    calendarEvent.SetNewTrainingBoostBarracksCost(this.GetIntParameter(0));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_SPELL_BREWING_BOOST:
                    calendarEvent.SetNewTrainingBoostSpellCost(this.GetIntParameter(0));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_BUILDING_BOOST:
                    calendarEvent.AddBuildingBoost(this.GetDataParameter(0, LogicDataType.BUILDING), this.GetIntParameter(1));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_TROOP_DISCOUNT:
                    calendarEvent.AddTroopDiscount(this.GetDataParameter(0, LogicDataType.CHARACTER), this.GetIntParameter(1));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_SPELL_DISCOUNT:
                    calendarEvent.AddTroopDiscount(this.GetDataParameter(0, LogicDataType.SPELL), this.GetIntParameter(1));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_CLAN_XP_MULTIPLIER:
                    calendarEvent.SetAllianceXpMultiplier(this.GetIntParameter(0));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_OFFER_BUNDLE:
                    calendarEvent.AddEnabledData(this.GetDataParameter(0, LogicDataType.GEM_BUNDLE));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_STAR_BONUS_MULTIPLIER:
                    calendarEvent.SetStarBonusMultiplier(this.GetIntParameter(0));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_ENABLE_TROOP:
                    calendarEvent.AddEnabledData(this.GetDataParameter(0, LogicDataType.CHARACTER));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_ENABLE_SPELL:
                    calendarEvent.AddEnabledData(this.GetDataParameter(0, LogicDataType.SPELL));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_ENABLE_TRAP:
                    calendarEvent.AddEnabledData(this.GetDataParameter(0, LogicDataType.TRAP));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_USE_TROOP:
                    calendarEvent.AddUseTroop((LogicCombatItemData) this.GetDataParameter(0, LogicDataType.CHARACTER), this.GetIntParameter(1), this.GetIntParameter(2),
                                              this.GetIntParameter(3), this.GetIntParameter(4));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_TARGETING_TOWN_HALL_LEVEL:
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_TARGETING_PURCHASED_DIAMONDS:
                    Debugger.Warning("You should no longer target thru event functions.");
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_ENABLE_BILLING_PACKAGE:
                    calendarEvent.AddEnabledData(this.GetDataParameter(0, LogicDataType.BILLING_PACKAGE));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_CHANGE_WORKER_LOOK:
                    // TODO: Implement this (client).
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_GIVE_FREE_TROOPS:
                    calendarEvent.AddFreeTroop((LogicCombatItemData) this.GetDataParameter(0, LogicDataType.CHARACTER), this.GetIntParameter(1));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_GIVE_FREE_SPELLS:
                    calendarEvent.AddFreeTroop((LogicCombatItemData) this.GetDataParameter(0, LogicDataType.SPELL), this.GetIntParameter(1));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_GIVE_FREE_HERO_HEALTH:
                    if (this.GetDataParameter(0, LogicDataType.HERO) != null)
                    {
                        calendarEvent.AddFreeTroop((LogicCombatItemData) this.GetDataParameter(0, LogicDataType.HERO), 1);
                    }
                    else
                    {
                        LogicDataTable table = LogicDataTables.GetTable(LogicDataType.HERO);

                        for (int i = 0; i < table.GetItemCount(); i++)
                        {
                            calendarEvent.AddFreeTroop((LogicCombatItemData) table.GetItemAt(i), 1);
                        }
                    }

                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_BUILDING_SIGN:
                    // TODO: Implement this (client).
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_BUILDING_DESTROYED_SPAWN_UNIT:
                    calendarEvent.AddBuildingDestroyedSpawnUnit((LogicBuildingData) this.GetDataParameter(0, LogicDataType.BUILDING),
                                                                (LogicCharacterData) this.GetDataParameter(1, LogicDataType.CHARACTER),
                                                                this.GetIntParameter(2));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_USE_SPELL:
                    calendarEvent.AddUseTroop((LogicCombatItemData) this.GetDataParameter(0, LogicDataType.SPELL), this.GetIntParameter(1), this.GetIntParameter(2),
                                              this.GetIntParameter(3), this.GetIntParameter(4));
                    break;
                case LogicCalendarEventFunctionData.FUNCTION_TYPE_CLAN_WAR_LOOT_MULTIPLIER:
                    calendarEvent.SetAllianceWarWinLootMultiplier(this.GetIntParameter(0));
                    calendarEvent.SetAllianceWarDrawLootMultiplier(this.GetIntParameter(1));
                    calendarEvent.SetAllianceWarLooseLootMultiplier(this.GetIntParameter(2));
                    break;
                default:
                    Debugger.Error(string.Format("Unknown function type: {0}.", this.m_functionData.GetFunctionType()));
                    break;
            }
        }

        public LogicData GetDataParameter(int idx, LogicDataType tableIdx)
        {
            if (this.IsValidParameter(idx))
            {
                int value = LogicStringUtil.ConvertToInt(this.m_parameters[idx]);

                if (value != 0)
                {
                    LogicData data = LogicDataTables.GetDataById(value, tableIdx);

                    if (data != null)
                    {
                        return data;
                    }

                    this.m_errorHandler.WarningFunction(this.m_calendarEvent, this, this.m_functionData.GetParameterName(idx),
                                                        string.Format("Unable to find data by id {0} from tableId {1}.", value, tableIdx));
                }
                else
                {
                    this.m_errorHandler.WarningFunction(this.m_calendarEvent, this, this.m_functionData.GetParameterName(idx),
                                                        string.Format("Expected globalId got {0}.", value));
                }
            }

            return null;
        }

        public int GetIntParameter(int idx)
        {
            if (this.IsValidParameter(idx))
            {
                int value = LogicStringUtil.ConvertToInt(this.m_parameters[idx]);
                int minValue = this.m_functionData.GetMinValue(idx);
                int maxValue = this.m_functionData.GetMaxValue(idx);

                if (value < minValue || value > maxValue)
                {
                    this.m_errorHandler.WarningFunction(this.m_calendarEvent, this, this.m_functionData.GetParameterName(idx),
                                                        string.Format("Value {0} is not between {1} and {2}.", value, minValue, maxValue));
                    return minValue;
                }

                return value;
            }

            return 0;
        }

        public bool GetBoolParameter(int index)
        {
            if (this.IsValidParameter(index))
            {
                string value = this.m_parameters[index];

                if (!value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.m_errorHandler.ErrorFunction(this.m_calendarEvent, this, value, string.Format("Invalid boolean value {0}.", value));
                    }

                    return false;
                }

                return true;
            }

            return false;
        }

        public string GetStringParameter(int index)
        {
            if (this.IsValidParameter(index))
            {
                return this.m_parameters[index];
            }

            return string.Empty;
        }

        public bool IsValidParameter(int idx)
        {
            if (this.m_parameters.Size() <= idx)
            {
                this.m_errorHandler.ErrorFunction(this.m_calendarEvent, this, idx.ToString(), "Parameter has not been defined.");
            }
            else
            {
                if (idx >= 0)
                {
                    return true;
                }

                this.m_errorHandler.ErrorFunction(this.m_calendarEvent, this, "Got negative parameter index");
            }

            return false;
        }

        public string GetName()
        {
            return this.m_functionData.GetName();
        }

        public LogicArrayList<string> GetParameters()
        {
            return this.m_parameters;
        }
    }
}