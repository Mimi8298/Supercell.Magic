namespace Supercell.Magic.Servers.Core.Network.Request
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using Supercell.Magic.Servers.Core.Network.Message.Request;

    public static class ServerRequestManager
    {
        private static ConcurrentDictionary<long, ServerRequestArgs> m_requests;
        private static Thread m_thread;

        private static bool m_started;
        private static long m_counters;

        public static void Init()
        {
            ServerRequestManager.m_started = true;
            ServerRequestManager.m_requests = new ConcurrentDictionary<long, ServerRequestArgs>();
            ServerRequestManager.m_thread = new Thread(ServerRequestManager.Update);
            ServerRequestManager.m_thread.Start();
        }

        private static void Update()
        {
            while (ServerRequestManager.m_started)
            {
                DateTime timeUtc = DateTime.UtcNow;

                foreach (KeyValuePair<long, ServerRequestArgs> request in ServerRequestManager.m_requests)
                {
                    if (timeUtc > request.Value.ExpireTime && ServerRequestManager.m_requests.TryRemove(request.Key, out _))
                        request.Value.Abort();
                }

                Thread.Sleep(500);
            }
        }

        public static ServerRequestArgs Create(ServerRequestMessage message, ServerSocket socket, int timeout = 30)
        {
            ServerRequestArgs request = new ServerRequestArgs(timeout);
            long requestId = Interlocked.Increment(ref ServerRequestManager.m_counters);
            message.RequestId = requestId;

            if (!ServerRequestManager.m_requests.TryAdd(requestId, request))
                throw new Exception("Unable to add new message");
            ServerMessaging.Send(message, socket);

            return request;
        }

        public static void SendResponse(ServerResponseMessage response, ServerRequestMessage request)
        {
            response.RequestId = request.RequestId;
            ServerMessaging.Send(response, ServerManager.GetSocket(request.SenderType, request.SenderId));
        }

        internal static void ResponseReceived(ServerResponseMessage response)
        {
            if (ServerRequestManager.m_requests.TryRemove(response.RequestId, out ServerRequestArgs request))
            {
                request.SetResponseMessage(response);
            }
        }
    }
}