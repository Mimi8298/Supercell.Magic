namespace Supercell.Magic.Logic.Helper
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Offer;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public static class LogicJSONHelper
    {
        public static bool GetBool(LogicJSONObject jsonObject, string key)
        {
            LogicJSONBoolean jsonBoolean = jsonObject.GetJSONBoolean(key);

            if (jsonBoolean != null)
            {
                return jsonBoolean.IsTrue();
            }

            return false;
        }

        public static LogicLong GetLogicLong(LogicJSONObject jsonObject, string key)
        {
            return new LogicLong(LogicJSONHelper.GetInt(jsonObject, key + "_hi"), LogicJSONHelper.GetInt(jsonObject, key + "_lo"));
        }

        public static void SetInt(LogicJSONObject jsonObject, string key, int value)
        {
            jsonObject.Put(key, new LogicJSONNumber(value));
        }

        public static void SetString(LogicJSONObject jsonObject, string key, string value)
        {
            jsonObject.Put(key, new LogicJSONString(value));
        }

        public static void SetBool(LogicJSONObject jsonObject, string key, bool value)
        {
            jsonObject.Put(key, new LogicJSONBoolean(value));
        }

        public static void SetLogicLong(LogicJSONObject jsonObject, string key, LogicLong value)
        {
            if (value != null)
            {
                LogicJSONHelper.SetInt(jsonObject, key + "_hi", value.GetHigherInt());
                LogicJSONHelper.SetInt(jsonObject, key + "_lo", value.GetLowerInt());
            }
        }

        public static int GetInt(LogicJSONObject jsonObject, string key)
        {
            return LogicJSONHelper.GetInt(jsonObject, key, -1, true);
        }

        public static int GetInt(LogicJSONObject jsonObject, string key, int defaultValue)
        {
            return LogicJSONHelper.GetInt(jsonObject, key, defaultValue, false);
        }

        public static int GetInt(LogicJSONObject jsonObject, string key, int defaultValue, bool throwIfNotExist)
        {
            if (jsonObject != null)
            {
                if (key.Length != 0)
                {
                    LogicJSONNumber number = jsonObject.GetJSONNumber(key);

                    if (number != null)
                    {
                        return number.GetIntValue();
                    }

                    if (!throwIfNotExist)
                    {
                        return defaultValue;
                    }

                    Debugger.Error(string.Format("Json number with key='{0}' not found!", key));
                }
            }

            return -1;
        }

        public static string GetString(LogicJSONObject jsonObject, string key)
        {
            return LogicJSONHelper.GetString(jsonObject, key, string.Empty, true);
        }

        public static string GetString(LogicJSONObject jsonObject, string key, string defaultValue, bool throwIfNotExist)
        {
            if (jsonObject != null)
            {
                if (key.Length != 0)
                {
                    LogicJSONString stringValue = jsonObject.GetJSONString(key);

                    if (stringValue != null)
                    {
                        return stringValue.GetStringValue();
                    }

                    if (!throwIfNotExist)
                    {
                        return defaultValue;
                    }

                    Debugger.Error(string.Format("Json string with key='{0}' not found!", key));
                }
            }

            return null;
        }

        public static LogicData GetLogicData(LogicJSONObject jsonObject, string key)
        {
            LogicData data = LogicDataTables.GetDataById(LogicStringUtil.ConvertToInt(LogicJSONHelper.GetString(jsonObject, key, string.Empty, true)));

            if (data == null)
            {
                Debugger.Error("Unable to load data. key:" + key);
            }

            return data;
        }

        public static LogicDeliverable GetLogicDeliverable(LogicJSONObject jsonObject)
        {
            LogicDeliverable deliverable = LogicDeliverableFactory.CreateByType(LogicStringUtil.ConvertToInt(LogicJSONHelper.GetString(jsonObject, "type")));
            deliverable.ReadFromJSON(jsonObject);
            return deliverable;
        }

        public static void SetLogicData(LogicJSONObject jsonObject, string key, LogicData value)
        {
            if (value != null)
            {
                jsonObject.Put(key, new LogicJSONString(value.GetGlobalID().ToString()));
            }
            else
            {
                Debugger.Error("Unable to set null data. key:" + key);
            }
        }
    }
}