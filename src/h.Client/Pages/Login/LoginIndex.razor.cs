using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using h.Contracts.Users;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Microsoft.AspNetCore.Components.Authorization;

namespace h.Client.Pages.Login;

public partial class LoginIndex
{
    private string nickname = "";
    private string password = "";
    private LoginUserRequest request;
    
    private string errorMessage = "";

    private bool showInvalidPopup = false;
    
    [Inject] protected HttpClient _client { get; set; }
    [Inject] protected NavigationManager _navigation { get; set; } = null!;

    private async void HandleLogin()
    {
        request = new LoginUserRequest(nickname, password);
        var response = await _client.PostAsJsonAsync("api/v1/users/login", request);

        
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine(response + ": " + nickname + " " + password);
            _navigation.NavigateTo("/");
        }
        else
        {
            showInvalidPopup = true;
        }
    }

    private void HandleClosePopup()
    {
        showInvalidPopup = false;
    }
}