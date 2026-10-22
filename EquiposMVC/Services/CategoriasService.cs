using System.Text;
using System.Text.Json;
using EquiposMVC.Models;

namespace EquiposMVC.Services;

public interface ICategoriasService
{
    Task<ApiResponse<List<CategoriaViewModel>>> GetAllAsync();
    Task<ApiResponse<CategoriaViewModel>> GetByIdAsync(int id);
    Task<ApiResponse<CategoriaViewModel>> CreateAsync(CategoriaViewModel model);
    Task<ApiResponse<CategoriaViewModel>> UpdateAsync(int id, CategoriaViewModel model);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public class CategoriasService : BaseApiService, ICategoriasService
{
    public CategoriasService(HttpClient httpClient, ILogger<CategoriasService> logger)
        : base(httpClient, logger)
    {
    }

    public async Task<ApiResponse<List<CategoriaViewModel>>> GetAllAsync()
    {
        var response = await HttpClient.GetAsync("api/categorias");
        return await HandleResponseAsync<List<CategoriaViewModel>>(response);
    }

    public async Task<ApiResponse<CategoriaViewModel>> GetByIdAsync(int id)
    {
        var response = await HttpClient.GetAsync($"api/categorias/{id}");
        return await HandleResponseAsync<CategoriaViewModel>(response);
    }

    public async Task<ApiResponse<CategoriaViewModel>> CreateAsync(CategoriaViewModel model)
    {
        var payload = new { Nombre = model.Nombre.Trim() };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await HttpClient.PostAsync("api/categorias", content);
        return await HandleResponseAsync<CategoriaViewModel>(response);
    }

    public async Task<ApiResponse<CategoriaViewModel>> UpdateAsync(int id, CategoriaViewModel model)
    {
        var payload = new { Id = id, Nombre = model.Nombre.Trim() };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await HttpClient.PutAsync($"api/categorias/{id}", content);
        return await HandleResponseAsync<CategoriaViewModel>(response);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var response = await HttpClient.DeleteAsync($"api/categorias/{id}");
        return await HandleResponseAsync<bool>(response);
    }
}
