using ErrorOr;

namespace h.Contracts;
public static partial class SharedErrors
{
    public static class GameInvitations
    {
        public static Error GameInvitationNotFound()
            => Error.Validation(nameof(GameInvitationNotFound), "Room not found");

        public static Error UserAlreadyInRoom()
            => Error.Validation(nameof(UserAlreadyInRoom), "User is already in the room");
    }
}
