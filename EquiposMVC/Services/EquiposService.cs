using System.Text;
using System.Text.Json;
using EquiposMVC.Models;

namespace EquiposMVC.Services;

public interface IEquiposService
{
    Task<ApiResponse<List<EquipoViewModel>>> GetAllAsync(string? texto = null, int? categoriaId = null, string? estado = null);
    Task<ApiResponse<EquipoViewModel>> GetByIdAsync(int id);
    Task<ApiResponse<EquipoViewModel>> CreateAsync(EquipoCreateViewModel model);
    Task<ApiResponse<EquipoViewModel>> UpdateAsync(int id, EquipoEditViewModel model);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public class EquiposService : BaseApiService, IEquiposService
{
    public EquiposService(HttpClient httpClient, ILogger<EquiposService> logger)
        : base(httpClient, logger)
    {
    }

    public async Task<ApiResponse<List<EquipoViewModel>>> GetAllAsync(string? texto = null, int? categoriaId = null, string? estado = null)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(texto))
        {
            queryParams.Add($"texto={Uri.EscapeDataString(texto.Trim())}");
        }
        if (categoriaId.HasValue && categoriaId.Value > 0)
        {
            queryParams.Add($"categoriaId={categoriaId.Value}");
        }
        if (!string.IsNullOrWhiteSpace(estado))
        {
            queryParams.Add($"estado={Uri.EscapeDataString(estado.Trim())}");
        }

        var url = "api/equipos" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : string.Empty);
        var response = await HttpClient.GetAsync(url);
        return await HandleResponseAsync<List<EquipoViewModel>>(response);
    }

    public async Task<ApiResponse<EquipoViewModel>> GetByIdAsync(int id)
    {
        var response = await HttpClient.GetAsync($"api/equipos/{id}");
        return await HandleResponseAsync<EquipoViewModel>(response);
    }

    public async Task<ApiResponse<EquipoViewModel>> CreateAsync(EquipoCreateViewModel model)
    {
        var payload = new
        {
            CodigoInventario = model.CodigoInventario.Trim(),
            Nombre = model.Nombre.Trim(),
            Descripcion = model.Descripcion,
            CategoriaId = model.CategoriaId,
            Estado = model.Estado
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await HttpClient.PostAsync("api/equipos", content);
        return await HandleResponseAsync<EquipoViewModel>(response);
    }

    public async Task<ApiResponse<EquipoViewModel>> UpdateAsync(int id, EquipoEditViewModel model)
    {
        var payload = new
        {
            Id = id,
            CodigoInventario = model.CodigoInventario.Trim(),
            Nombre = model.Nombre.Trim(),
            Descripcion = model.Descripcion,
            CategoriaId = model.CategoriaId,
            Estado = model.Estado,
            RowVersion = model.RowVersion // Concurrencia optimista
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await HttpClient.PutAsync($"api/equipos/{id}", content);
        return await HandleResponseAsync<EquipoViewModel>(response);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var response = await HttpClient.DeleteAsync($"api/equipos/{id}");
        return await HandleResponseAsync<bool>(response);
    }
}
