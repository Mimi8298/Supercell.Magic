namespace Supercell.Magic.Servers.Chat.Logic
{
    using System.Collections.Generic;
    using Supercell.Magic.Titan.Util;

    public static class ChatInstanceManager
    {
        private static Dictionary<string, LogicArrayList<ChatInstance>> m_instances;

        public static void Init()
        {
            ChatInstanceManager.m_instances = new Dictionary<string, LogicArrayList<ChatInstance>>();
        }

        public static ChatInstance GetJoinableInstance(string country)
        {
            if (!ChatInstanceManager.m_instances.TryGetValue(country, out LogicArrayList<ChatInstance> instances))
                ChatInstanceManager.m_instances.Add(country, instances = new LogicArrayList<ChatInstance>(500));

            ChatInstance instance;

            for (int i = 0; i < instances.Size(); i++)
            {
                instance = instances[i];

                if (!instance.IsFull())
                    return instance;
            }

            instance = new ChatInstance();
            instances.Add(instance);

            return instance;
        }
    }
}