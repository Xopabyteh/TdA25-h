namespace h.IntegrationTests.Matchmaking;
internal sealed class MatchmakingTestOrder
{
    public const int Matchmaking_UsersJoinMatch_GetMatched_AndGetNotified = 10;
    public const int Matchmaking_UserDeclinesMatch_AndPlayersGetNotifiedAboutCancel = 20;
    public const int Matchmaking_UserDeclinesMath_AndOtherPlayerIsPlacedBackToQueue = 30;
    public const int Matchmaking_UserCannotJoinQueueTwice = 40;
    public const int Matchmaking_PlayersAccept_AndGetNotifiedAboutNewGameSession = 40;

    public const int Matchmaking_MatchingExpire_RemovesHangingMatchings_AndUsersGetNotified = 10;
    public const int Matchmaking_RemoveingHangingMatchings_PlacesAccepteesBackToQueue = 20;
}
