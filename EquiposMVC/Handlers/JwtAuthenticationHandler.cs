using System.Net.Http.Headers;

namespace EquiposMVC.Handlers;

public class JwtAuthenticationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtAuthenticationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        var token = session?.GetString("JwtToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            // Se envía en el request específico sin modificar DefaultRequestHeaders del cliente compartido
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
