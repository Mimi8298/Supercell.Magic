namespace Supercell.Magic.Servers.Discord
{
    using global::Discord.WebSocket;

    internal class Program
    {
        private static DiscordSocketClient m_client;

        private static void Main(string[] args)
        {
            Program.m_client = new DiscordSocketClient();
        }
    }
}