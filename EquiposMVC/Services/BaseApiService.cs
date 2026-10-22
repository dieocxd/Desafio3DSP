using System.Net;
using System.Text.Json;
using EquiposMVC.Models;

namespace EquiposMVC.Services;

public abstract class BaseApiService
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;

    protected BaseApiService(HttpClient httpClient, ILogger logger)
    {
        HttpClient = httpClient;
        Logger = logger;
    }

    protected async Task<ApiResponse<T>> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new ApiResponse<T>
                {
                    IsSuccess = true,
                    StatusCode = statusCode,
                    Data = default
                };
            }

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<T>(content, options);
                return new ApiResponse<T>
                {
                    IsSuccess = true,
                    StatusCode = statusCode,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al deserializar respuesta exitosa.");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = statusCode,
                    ErrorMessage = "Error al procesar los datos de la respuesta."
                };
            }
        }

        // Manejo específico de errores HTTP según la rúbrica
        string mensajeError;
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("mensaje", out var mensajeProp))
            {
                mensajeError = mensajeProp.GetString() ?? "Ocurrió un conflicto al procesar la solicitud.";
            }
            else if (doc.RootElement.TryGetProperty("title", out var titleProp))
            {
                mensajeError = titleProp.GetString() ?? "Error en los datos de la solicitud.";
            }
            else
            {
                mensajeError = content;
            }
        }
        catch
        {
            mensajeError = statusCode switch
            {
                401 => "Su sesión ha expirado o no está autenticado.",
                403 => "Acceso denegado. No posee los permisos requeridos.",
                409 => "Conflicto al guardar los datos.",
                >= 500 => "Ha ocurrido un error interno en el servidor. Por favor intente más tarde.",
                _ => $"Error en la operación (Código HTTP {statusCode})."
            };
        }

        if (statusCode >= 500)
        {
            mensajeError = "Ha ocurrido un error interno en el servidor. Por favor intente más tarde.";
        }

        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = mensajeError
        };
    }
}
