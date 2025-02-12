﻿using ErrorOr;

namespace h.Contracts;
public static partial class SharedErrors
{
    public static class User
    {
        public static Error UserNotFound()
            => Error.NotFound(nameof(UserNotFound), "User not found");
    }
}
