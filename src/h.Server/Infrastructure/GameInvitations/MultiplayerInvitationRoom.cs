using h.Server.Infrastructure.MultiplayerGames;

namespace h.Server.Infrastructure.GameInvitations;

public class MultiplayerInvitationRoom
{
    public MultiplayerGameUserIdentity OwnerId { get; }
    public List<MultiplayerGameUserIdentity> Players { get; set; } = new(2);
    public int RoomCode { get; private set; }
    public MultiplayerInvitationRoom(MultiplayerGameUserIdentity ownerId, int roomCode)
    {
        OwnerId = ownerId;
        Players.Add(ownerId);
        RoomCode = roomCode;
    }
}