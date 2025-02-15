using ErrorOr;

namespace h.Contracts;
public static partial class SharedErrors
{
    public static class User
    {
        public static Error UserNotFound()
            => Error.NotFound(nameof(UserNotFound), "User not found");

        public class UserNotFoundExceptin : Exception
        {
            public UserNotFoundExceptin() : base("User not found") { }
        }

        public static Error UsernameAlreadyTaken()
            => Error.Conflict(nameof(UsernameAlreadyTaken), "Username already taken");

        public static Error EmailAlreadyTaken()
            => Error.Conflict(nameof(EmailAlreadyTaken), "Email already taken");
    }
}
