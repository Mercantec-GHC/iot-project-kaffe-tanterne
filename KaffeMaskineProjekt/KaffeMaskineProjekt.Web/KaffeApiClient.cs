using System.Net.Http.Json;
using KaffeMaskineProjekt.DTO;

namespace KaffeMaskineProjekt.Web;

/// <summary>
/// Client for calling coffee machine API endpoints for ingredients and recipes.
/// </summary>
public class KaffeApiClient(HttpClient httpClient)
{
    // Ingredients
    #region Ingredients
    /// <summary>
    /// Retrieves all ingredients from the API.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>List of <see cref="IngredientDTO"/>.</returns>
    public async Task<List<IngredientDTO>> GetIngredientsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<IngredientDTO>>("api/Ingredients/Index", cancellationToken) 
               ?? new List<IngredientDTO>();
    }

    /// <summary>
    /// Retrieves a specific ingredient by its ID.
    /// </summary>
    /// <param name="id">Identifier of the ingredient.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="IngredientDTO"/> or null if not found.</returns>
    public async Task<IngredientDTO?> GetIngredientAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IngredientDTO>($"api/Ingredients/Details/{id}", cancellationToken);
    }

    /// <summary>
    /// Creates a new ingredient in the API.
    /// </summary>
    /// <param name="model">Data for the new ingredient.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Created <see cref="IngredientDTO"/> or null on failure.</returns>
    public async Task<IngredientDTO?> CreateIngredientAsync(CreateIngredientModel model, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Creating ingredient: {model.Name}");
        var response = await httpClient.PostAsJsonAsync("api/Ingredients/Create", model, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IngredientDTO>(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates an existing ingredient.
    /// </summary>
    /// <param name="id">Identifier of the ingredient to update.</param>
    /// <param name="model">Updated ingredient data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if update succeeded, otherwise false.</returns>
    public async Task<bool> UpdateIngredientAsync(int id, EditIngredientModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/Ingredients/Edit/{id}", model, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Deletes an ingredient by its ID.
    /// </summary>
    /// <param name="id">Identifier of the ingredient to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if deletion succeeded, otherwise false.</returns>
    public async Task<bool> DeleteIngredientAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/Ingredients/Delete/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
    #endregion

    // Recipes
    #region Recipes
    /// <summary>
    /// Retrieves all recipes from the API.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>List of <see cref="RecipeDTO"/>.</returns>
    public async Task<List<RecipeDTO>> GetRecipesAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<RecipeDTO>>("api/Recipe/Index", cancellationToken)
               ?? new List<RecipeDTO>();
    }

    /// <summary>
    /// Retrieves a specific recipe by its ID.
    /// </summary>
    /// <param name="id">Identifier of the recipe.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="RecipeDTO"/> or null if not found.</returns>
    public async Task<RecipeDTO?> GetRecipeAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<RecipeDTO>($"api/Recipe/Details/{id}", cancellationToken);
    }

    /// <summary>
    /// Creates a new recipe in the API.
    /// </summary>
    /// <param name="model">Data for the new recipe.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Created <see cref="RecipeDTO"/> or null on failure.</returns>
    public async Task<RecipeDTO?> CreateRecipeAsync(CreateRecipeModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/Recipe/Create", model, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RecipeDTO>(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates an existing recipe.
    /// </summary>
    /// <param name="id">Identifier of the recipe to update.</param>
    /// <param name="model">Updated recipe data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if update succeeded, otherwise false.</returns>
    public async Task<bool> UpdateRecipeAsync(int id, EditRecipeModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/Recipe/Edit/{id}", model, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Deletes a recipe by its ID.
    /// </summary>
    /// <param name="id">Identifier of the recipe to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if deletion succeeded, otherwise false.</returns>
    public async Task<bool> DeleteRecipeAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/Recipe/Delete/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
    #endregion

    // Measurements
    #region Measurements
    public async Task<List<MeasurementsDTO>> GetMeasurementsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<MeasurementsDTO>>("api/Measurements/Index", cancellationToken)
            ?? new List<MeasurementsDTO>();
    }

    public async Task<MeasurementsDTO> GetMeasurementAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<MeasurementsDTO>($"api/Measurements/Details/{id}", cancellationToken);
    }

    public async Task<MeasurementsDTO> CreateMeasurementsAsync(CreateMeasurementsModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/Measurements/Create", model, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<MeasurementsDTO>(cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateMeasurementsAsync(int id, EditMeasurementsModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/Measurements/Edit/{id}", model, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteMeasurementsAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/Measurements/Delete/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
    #endregion

    // Statistics
    #region Statistics
    public async Task<List<StatisticsDTO>> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<StatisticsDTO>>("api/Statistics/Index", cancellationToken)
            ?? new List<StatisticsDTO>();
    }

    public async Task<StatisticsDTO> GetStatisticAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<StatisticsDTO>($"api/Statistics/Details/{id}", cancellationToken);
    }

    public async Task<StatisticsDTO> CreateStatisticsAsync(CreateStatisticsModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/Statistics/Create", model, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<StatisticsDTO>(cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateStatisticsAsync(int id, EditStatisticsModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/Statistics/Edit/{id}", model, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteStatisticsAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/Statistics/Delete/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
    #endregion
}
