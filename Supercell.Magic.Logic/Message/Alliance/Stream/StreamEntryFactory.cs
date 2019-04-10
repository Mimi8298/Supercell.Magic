namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    public class StreamEntryFactory
    {
        public static StreamEntry CreateStreamEntryByType(StreamEntryType type)
        {
            switch (type)
            {
                case StreamEntryType.DONATE: return new DonateStreamEntry();
                case StreamEntryType.CHAT: return new ChatStreamEntry();
                case StreamEntryType.JOIN_REQUEST: return new JoinRequestAllianceStreamEntry();
                case StreamEntryType.ALLIANCE_EVENT: return new AllianceEventStreamEntry();
                case StreamEntryType.REPLAY: return new ReplayStreamEntry();
                case StreamEntryType.CHALLENGE_REPLAY: return new ChallengeReplayStreamEntry();
                case StreamEntryType.CHALLENGE: return new ChallengeStreamEntry();
                case StreamEntryType.ALLIANCE_GIFT: return new AllianceGiftStreamEntry();
                case StreamEntryType.VERSUS_BATTLE_REQUEST: return new VersusChallengeStreamEntry();
                case StreamEntryType.VERSUS_BATTLE_REPLAY: return new VersusBattleReplayStreamEntry();
                case StreamEntryType.DUEL_REPLAY: return new DuelReplayStreamEntry();
                default: return null;
            }
        }
    }
}