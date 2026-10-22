using System.Text;
using System.Text.Json;
using EquiposMVC.Models;

namespace EquiposMVC.Services;

public interface IPrestamosService
{
    Task<ApiResponse<List<PrestamoViewModel>>> GetAllAsync(bool soloVencidos = false);
    Task<ApiResponse<PrestamoViewModel>> GetByIdAsync(int id);
    Task<ApiResponse<PrestamoViewModel>> CreateAsync(PrestamoCreateViewModel model);
    Task<ApiResponse<PrestamoViewModel>> RegistrarDevolucionAsync(int id, string? observaciones);
}

public class PrestamosService : BaseApiService, IPrestamosService
{
    public PrestamosService(HttpClient httpClient, ILogger<PrestamosService> logger)
        : base(httpClient, logger)
    {
    }

    public async Task<ApiResponse<List<PrestamoViewModel>>> GetAllAsync(bool soloVencidos = false)
    {
        var url = $"api/prestamos{(soloVencidos ? "?soloVencidos=true" : string.Empty)}";
        var response = await HttpClient.GetAsync(url);
        return await HandleResponseAsync<List<PrestamoViewModel>>(response);
    }

    public async Task<ApiResponse<PrestamoViewModel>> GetByIdAsync(int id)
    {
        var response = await HttpClient.GetAsync($"api/prestamos/{id}");
        return await HandleResponseAsync<PrestamoViewModel>(response);
    }

    public async Task<ApiResponse<PrestamoViewModel>> CreateAsync(PrestamoCreateViewModel model)
    {
        var payload = new
        {
            EquipoId = model.EquipoId,
            UsuarioId = model.UsuarioId,
            FechaVencimiento = model.FechaVencimiento,
            Observaciones = model.Observaciones
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await HttpClient.PostAsync("api/prestamos", content);
        return await HandleResponseAsync<PrestamoViewModel>(response);
    }

    public async Task<ApiResponse<PrestamoViewModel>> RegistrarDevolucionAsync(int id, string? observaciones)
    {
        var payload = new { Observaciones = observaciones };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await HttpClient.PutAsync($"api/prestamos/{id}/devolucion", content);
        return await HandleResponseAsync<PrestamoViewModel>(response);
    }
}
