namespace Supercell.Magic.Servers.Game.Logic.Live
{
    using Supercell.Magic.Servers.Core;
    using Supercell.Magic.Servers.Core.Cluster;
    using Supercell.Magic.Servers.Core.Network.Message;
    using Supercell.Magic.Servers.Core.Network.Message.Account;
    using Supercell.Magic.Servers.Core.Network.Message.Request;
    using Supercell.Magic.Servers.Core.Network.Request;

    public class LiveReplayCluster : ClusterInstance
    {
        public LiveReplayCluster(int id, int logicUpdateFrequency = -1) : base(id, logicUpdateFrequency)
        {
        }

        protected override void ReceiveMessage(ServerMessage message)
        {
            Logging.Print("LiveReplayCluster.receiveMessage :" + message.GetMessageType());

            switch (message.GetMessageType())
            {
                case ServerMessageType.INITIALIZE_LIVE_REPLAY:
                    LiveReplayCluster.OnInitializeLiveReplayMessageReceived((InitializeLiveReplayMessage) message);
                    break;
                case ServerMessageType.CLIENT_UPDATE_LIVE_REPLAY:
                    LiveReplayCluster.OnClientUpdateLiveReplayMessageReceived((ClientUpdateLiveReplayMessage) message);
                    break;
                case ServerMessageType.SERVER_UPDATE_LIVE_REPLAY:
                    LiveReplayCluster.OnServerUpdateLiveReplayMessageReceived((ServerUpdateLiveReplayMessage) message);
                    break;
                case ServerMessageType.END_LIVE_REPLAY:
                    LiveReplayCluster.OnEndLiveReplayMessageReceived((EndLiveReplayMessage) message);
                    break;

                case ServerMessageType.LIVE_REPLAY_ADD_SPECTATOR_REQUEST:
                    LiveReplayCluster.OnLiveReplayAddSpectatorRequestMessageReceived((LiveReplayAddSpectatorRequestMessage) message);
                    break;
                case ServerMessageType.LIVE_REPLAY_REMOVE_SPECTATOR:
                    LiveReplayCluster.OnLiveReplayRemoveSpectatorMessageReceived((LiveReplayRemoveSpectatorMessage) message);
                    break;
            }
        }

        private static void OnInitializeLiveReplayMessageReceived(InitializeLiveReplayMessage message)
        {
            if (LiveReplayManager.TryGet(message.AccountId, out LiveReplay liveReplay))
            {
                liveReplay.Init(message.StreamData);
            }
        }

        private static void OnClientUpdateLiveReplayMessageReceived(ClientUpdateLiveReplayMessage message)
        {
            if (LiveReplayManager.TryGet(message.AccountId, out LiveReplay liveReplay))
            {
                liveReplay.SetClientUpdate(message.SubTick, message.Commands);
            }
        }

        private static void OnServerUpdateLiveReplayMessageReceived(ServerUpdateLiveReplayMessage message)
        {
            if (LiveReplayManager.TryGet(message.AccountId, out LiveReplay liveReplay))
            {
                liveReplay.Update(message.Milliseconds);

                if (liveReplay.IsEnded())
                {
                    LiveReplayManager.Remove(liveReplay.GetId());
                }
            }
        }

        private static void OnEndLiveReplayMessageReceived(EndLiveReplayMessage message)
        {
            if (LiveReplayManager.TryGet(message.AccountId, out LiveReplay liveReplay))
            {
                liveReplay.SetEnd();
            }
        }

        private static void OnLiveReplayAddSpectatorRequestMessageReceived(LiveReplayAddSpectatorRequestMessage message)
        {
            if (LiveReplayManager.TryGet(message.LiveReplayId, out LiveReplay liveReplay) && !liveReplay.ContainsSession(message.SessionId, message.SlotId) && liveReplay.IsInit())
            {
                if (liveReplay.IsFull())
                {
                    ServerRequestManager.SendResponse(new LiveReplayAddSpectatorResponseMessage
                    {
                        ErrorCode = LiveReplayAddSpectatorResponseMessage.Reason.FULL
                    }, message);
                }
                else
                {
                    ServerRequestManager.SendResponse(new LiveReplayAddSpectatorResponseMessage
                    {
                        Success = true
                    }, message);

                    liveReplay.AddSpectator(message.SessionId, message.SlotId);
                }
            }
            else
            {
                ServerRequestManager.SendResponse(new LiveReplayAddSpectatorResponseMessage
                {
                    ErrorCode = LiveReplayAddSpectatorResponseMessage.Reason.NOT_EXISTS
                }, message);
            }
        }

        private static void OnLiveReplayRemoveSpectatorMessageReceived(LiveReplayRemoveSpectatorMessage message)
        {
            if (LiveReplayManager.TryGet(message.AccountId, out LiveReplay liveReplay))
            {
                liveReplay.RemoveSpectator(message.SessionId, message.SlotId);
            }
        }
    }
}