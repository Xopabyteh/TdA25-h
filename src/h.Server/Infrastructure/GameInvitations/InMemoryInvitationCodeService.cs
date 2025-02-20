using ErrorOr;
using h.Contracts;
using h.Server.Infrastructure.MultiplayerGames;
using System.Collections.Concurrent;

namespace h.Server.Infrastructure.GameInvitations;

public class InMemoryInvitationCodeService
{
    // Todo: room expiration

    private readonly ConcurrentDictionary<int, MultiplayerInvitationRoom> PendingRooms
        = new(concurrencyLevel: -1, 10);
    public int CreateNewRoom(MultiplayerGameUserIdentity byPlayer)
    {
        int roomCode;
        do
        {
            // Random 6 digit number
            roomCode = Random.Shared.Next(100_000, 999_999);
        } while (!PendingRooms.TryAdd(roomCode, new MultiplayerInvitationRoom(ownerId: byPlayer, roomCode)));

        return roomCode;
    }

    /// <summary>
    /// Attempts to join a player into a room
    /// </summary>
    /// <returns><see langword="true"/> if the room is full and game can be started</returns>
    public ErrorOr<bool> JoinRoom(int roomCode, MultiplayerGameUserIdentity joiningPlayer)
    {
        if (!PendingRooms.TryGetValue(roomCode, out var room))
            return SharedErrors.GameInvitations.GameInvitationNotFound();

        if (room.Players.Contains(joiningPlayer))
            return SharedErrors.GameInvitations.UserAlreadyInRoom();

        room.Players.Add(joiningPlayer);

        return room.Players.Count == 2;
    }

    public MultiplayerInvitationRoom? GetRoom(int roomCode)
    {
        PendingRooms.TryGetValue(roomCode, out var room);
        return room;
    }

    public void RemoveRoom(int roomCode)
    {
        PendingRooms.TryRemove(roomCode, out _);
    }
}
