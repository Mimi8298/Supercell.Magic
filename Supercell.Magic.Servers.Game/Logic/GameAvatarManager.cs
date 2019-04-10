namespace Supercell.Magic.Servers.Game.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Couchbase;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Mode;
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Servers.Core.Network;

    using Supercell.Magic.Titan.Math;

    public static class GameAvatarManager
    {
        private static Dictionary<long, GameAvatar> m_documents;

        public static void Init()
        {
            GameAvatarManager.m_documents = new Dictionary<long, GameAvatar>();

            int[] higherIDs = ServerGame.GameDatabase.GetCounterHigherIDs();

            for (int i = 0; i < higherIDs.Length; i++)
            {
                int highId = i;

                Parallel.For(1, higherIDs[i] + 1, lowId =>
                {
                    LogicLong id = new LogicLong(highId, lowId);

                    if (ServerManager.GetDocumentSocket(ServerCore.Type, id) == ServerCore.Socket)
                    {
                        IOperationResult<string> document = ServerGame.GameDatabase.Get(id).Result;

                        if (document.Success)
                        {
                            lock (GameAvatarManager.m_documents)
                            {
                                GameAvatar gameDocument = CouchbaseDocument.Load<GameAvatar>(document.Value);

                                GameAvatarManager.m_documents.Add(id, gameDocument);
                                GameMatchmakingManager.Enqueue(gameDocument);
                            }
                        }
                    }
                });
            }

        }
        
        public static void ExecuteServerCommandsInOfflineMode(GameAvatar document)
        {
            if (document.ServerCommands.Size() != 0)
            {
                LogicGameMode logicGameMode = new LogicGameMode();
                LogicLevel logicLevel = logicGameMode.GetLevel();

                logicLevel.SetVisitorAvatar(document.LogicClientAvatar);

                try
                {
                    for (int i = 0; i < document.ServerCommands.Size(); i++)
                    {
                        int result = document.ServerCommands[i].Execute(logicLevel);

                        if (result == 0)
                        {
                            document.ServerCommands.Remove(i--);
                        }
                    }

                    GameAvatarManager.Save(document);
                }
                catch (Exception exception)
                {
                    Logging.Error("GameAvatarManager.executeServerCommandsInOfflineMode: server command execution in offline mode failed: " + exception);
                }
            }
        }

        public static bool TryGet(LogicLong id, out GameAvatar document)
        {
            if (ServerManager.GetDocumentSocket(ServerCore.Type, id) != ServerCore.Socket)
                throw new Exception("GameAvatarManager.get - invalid document id");
            return GameAvatarManager.m_documents.TryGetValue(id, out document);
        }

        public static GameAvatar Get(LogicLong id)
        {
            return GameAvatarManager.m_documents[id];
        }

        public static GameAvatar Create(LogicLong id)
        {
            if (ServerManager.GetDocumentSocket(ServerCore.Type, id) != ServerCore.Socket)
                throw new Exception("GameAvatarManager.create - invalid document id");

            GameAvatar document = new GameAvatar(id);
            document.LogicClientHome.GetCompressibleHomeJSON().Set(GameResourceManager.CompressedStartingHomeJSON);
            GameAvatarManager.m_documents.Add(id, document);

            return document;
        }

        public static async void Save(GameAvatar document)
        {
            await ServerGame.GameDatabase.InsertOrUpdate(document.Id, CouchbaseDocument.Save(document));
        }
    }
}