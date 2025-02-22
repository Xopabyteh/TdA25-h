using Microsoft.AspNetCore.Components;

namespace h.Client.Pages.Login;

public partial class RegisterIndex
{
    private class RequestModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordMatch { get; set; }
    }
}