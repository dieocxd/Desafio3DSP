using System.Text;
using System.Text.Json;
using EquiposMVC.Models;

namespace EquiposMVC.Services;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginViewModel model);
    Task<ApiResponse<UsuarioViewModel>> RegisterAsync(RegisterViewModel model);
    Task<ApiResponse<List<UsuarioViewModel>>> GetUsuariosAsync();
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class AuthService : BaseApiService, IAuthService
{
    public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
        : base(httpClient, logger)
    {
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginViewModel model)
    {
        var payload = new
        {
            Username = model.Username.Trim(),
            Password = model.Password
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await HttpClient.PostAsync("api/auth/login", content);
        return await HandleResponseAsync<LoginResponse>(response);
    }

    public async Task<ApiResponse<UsuarioViewModel>> RegisterAsync(RegisterViewModel model)
    {
        var payload = new
        {
            Username = model.Username.Trim(),
            Password = model.Password,
            Email = model.Email.Trim(),
            Rol = model.Rol
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await HttpClient.PostAsync("api/auth/register", content);
        return await HandleResponseAsync<UsuarioViewModel>(response);
    }

    public async Task<ApiResponse<List<UsuarioViewModel>>> GetUsuariosAsync()
    {
        var response = await HttpClient.GetAsync("api/auth/usuarios");
        return await HandleResponseAsync<List<UsuarioViewModel>>(response);
    }
}
