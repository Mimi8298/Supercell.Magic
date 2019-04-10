namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.Debug;

    public static class GameStateFactory
    {
        public static GameState CreateByType(GameStateType type)
        {
            switch (type)
            {
                case GameStateType.HOME: return new GameHomeState();
                case GameStateType.NPC_ATTACK: return new GameNpcAttackState();
                case GameStateType.NPC_DUEL: return new GameNpcDuelState();
                case GameStateType.MATCHED_ATTACK: return new GameMatchedAttackState();
                case GameStateType.CHALLENGE_ATTACK: return new GameChallengeAttackState();
                case GameStateType.FAKE_ATTACK: return new GameFakeAttackState();
                case GameStateType.VISIT: return new GameVisitState();

                default:
                    Debugger.Error("GameStateFactory: unknown game state type: " + type);
                    return null;
            }
        }
    }
}