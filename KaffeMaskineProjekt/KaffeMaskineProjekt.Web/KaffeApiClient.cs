using System.Net.Http.Json;
using KaffeMaskineProjekt.DTO;
using KaffeMaskineProjekt.DomainModels;
using static KaffeMaskineProjekt.Web.Components.Pages.Orders;

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
    // Login
    #region Login
    public async Task<User?> LoginAsync(LoginModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/User/Login", model, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<User>(cancellationToken);
        }
        return null;
    }
    #endregion
    // Orders
    #region Orders
    public async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<Order>>("api/Orders/Index", cancellationToken)
               ?? new List<Order>();
    }

    public async Task<Order?> CreateOrderAsync(CreateOrderModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/Orders/Create", model, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Order>(cancellationToken);
    }

    public async Task<bool> DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/Orders/Delete/{orderId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
    public async Task<List<User>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<User>>("api/User/Index", cancellationToken)
               ?? new List<User>();
    }
    #endregion
}
