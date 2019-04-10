namespace Supercell.Magic.Servers.Core.Settings
{
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;

    public static class ResourceSettings
    {
        public static LogicArrayList<string> ContentUrlList { get; private set; }
        public static LogicArrayList<string> ChronosContentUrlList { get; private set; }
        public static LogicArrayList<string> AppStoreUrlList { get; private set; }

        public static string ResourceSHA { get; private set; }
        public static string GoogleServiceId { get; private set; }
        public static string FacebookAppId { get; private set; }

        public static void Init()
        {
            ResourceSettings.Load(ServerHttpClient.DownloadJSON("resource.json"));
        }

        public static string GetContentUrl()
        {
            if (ResourceSettings.ContentUrlList.Size() > 1)
            {
                return ResourceSettings.ContentUrlList[1];
            }

            return null;
        }

        public static string GetAppStoreUrl(bool androidVersion)
        {
            return ResourceSettings.AppStoreUrlList[androidVersion ? 1 : 0];
        }

        public static void Load(LogicJSONObject jsonObject)
        {
            ResourceSettings.ContentUrlList = ResourceSettings.LoadStringArray(jsonObject.GetJSONArray("contentUrls"));
            ResourceSettings.ChronosContentUrlList = ResourceSettings.LoadStringArray(jsonObject.GetJSONArray("chronosContentUrls"));
            ResourceSettings.AppStoreUrlList = ResourceSettings.LoadStringArray(jsonObject.GetJSONArray("appstoreUrls"));
            ResourceSettings.ResourceSHA = jsonObject.GetJSONString("resourceSHA").GetStringValue();
            ResourceSettings.GoogleServiceId = jsonObject.GetJSONString("googleServiceId").GetStringValue();
            ResourceSettings.FacebookAppId = jsonObject.GetJSONString("fbAppId").GetStringValue();
        }

        private static LogicArrayList<string> LoadStringArray(LogicJSONArray jsonArray)
        {
            LogicArrayList<string> arrayList = new LogicArrayList<string>();

            if (jsonArray != null)
            {
                for (int i = 0; i < jsonArray.Size(); i++)
                {
                    arrayList.Add(jsonArray.GetJSONString(i).GetStringValue());
                }
            }

            return arrayList;
        }


        public static LogicJSONObject Save()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("contentUrls", ResourceSettings.SaveStringArray(ResourceSettings.ContentUrlList));
            jsonObject.Put("chronosContentUrls", ResourceSettings.SaveStringArray(ResourceSettings.ChronosContentUrlList));
            jsonObject.Put("appstoreUrls", ResourceSettings.SaveStringArray(ResourceSettings.AppStoreUrlList));
            jsonObject.Put("resourceSHA", new LogicJSONString(ResourceSettings.ResourceSHA));
            jsonObject.Put("googleServiceId", new LogicJSONString(ResourceSettings.GoogleServiceId));
            jsonObject.Put("fbAppId", new LogicJSONString(ResourceSettings.FacebookAppId));

            return jsonObject;
        }

        private static LogicJSONArray SaveStringArray(LogicArrayList<string> arrayList)
        {
            LogicJSONArray jsonArray = new LogicJSONArray();

            for (int i = 0; i < arrayList.Size(); i++)
            {
                jsonArray.Add(new LogicJSONString(arrayList[i]));
            }

            return jsonArray;
        }
    }
}