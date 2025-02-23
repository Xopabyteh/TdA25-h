using ErrorOr;

namespace h.Contracts;
public static partial class SharedErrors
{
    public static class Matchmaking
    {
        //public static Error UserAlreadyInQueue()
        //    => Error.Conflict(nameof(UserAlreadyInQueue), "User is already in the queue");

        public class UserAlreadyInQueueException : Exception
        {
            public UserAlreadyInQueueException() : base("User is already in the queue") { }
        }

        public static Error MatchingNotFound()
            => Error.NotFound(nameof(MatchingNotFound), "Matching not found");

        public static Error MatchingAlreadyAccepted()
            => Error.Conflict(nameof(MatchingAlreadyAccepted), "Matching already accepted");

        public static Error UserNotPartOfMatching()
            => Error.NotFound(nameof(UserNotPartOfMatching), "User is not part of the matching");
    }
}
