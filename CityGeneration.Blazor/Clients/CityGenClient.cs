using System.Globalization;
using CityGeneration.Blazor.Models;
using CityGeneration.Contracts.Rest.Entities;
using CityGeneration.Core.Data.DataModels;
using Microsoft.AspNetCore.Http.Json;

namespace CityGeneration.Blazor.Clients;

public class CityGenClient(HttpClient client)
{
    private readonly HttpClient _client = client;
    public async Task<RoadPlacementData?> GenerateCityAsync(int seed = 0,
        int width = 512,
        int height = 512,
        int splitIterations = 0,
        float blockChance = 0.01f,
        float deviation = 0.03f)
    {
    var response = await _client.GetAsync(
        $"api/CityGeneration/generate-network?seed={seed}&width={width}&height={height}&splitIterations={splitIterations}&blockChance={blockChance
        .ToString(CultureInfo.InvariantCulture)}&deviation={deviation.ToString(CultureInfo.InvariantCulture)}");
    response.EnsureSuccessStatusCode();
    ApiResponseWrapper<RoadPlacementData>? wrapper = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<RoadPlacementData>>();
    return wrapper?.Result;
    }

    public async Task<List<BuildingData>> GenerateBuildingsAsync(RoadPlacementData data)
    {
        var response = await _client.PostAsJsonAsync(
            "api/CityGeneration/generate-buildings",
            data);
        response.EnsureSuccessStatusCode();
        ApiResponseWrapper<List<BuildingData>>? wrapper = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<BuildingData>>>();
        return wrapper?.Result;
    }
    
    public async Task<List<int>> GetPathAsync(RoadPlacementData data, int startId, int endId)
    {
        var response = await _client.PostAsJsonAsync(
            $"api/CityGeneration/get-road-path?svid={startId}&evid={endId}",
            data);
        response.EnsureSuccessStatusCode();
        ApiResponseWrapper<List<int>>? wrapper = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<int>>>();
        return wrapper?.Result;
    }
}