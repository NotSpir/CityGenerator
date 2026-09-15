using System.Text.Json;
using CityGeneration.Contracts.Rest.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CityGeneration.Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CityGenerationController : ControllerBase
{
    [HttpGet("generate-network")]
    public async Task<IActionResult> GenerateRoadNetwork(
        int seed = 0,
        int width = 512,
        int height = 512,
        int splitIterations = 0,
        float blockChance = 0.01f,
        float deviation = 0.03f)
    {
        var result = CityDataManager.CreateAndRetrieveCityData(
            seed, width, height, splitIterations, blockChance, deviation);
        return Ok(new { result });
    }
    
    [HttpPost("generate-buildings")]
    public async Task<IActionResult> GenerateBuildings(
        [FromBody] RoadPlacementData roadNetworkData)
    {
        var roadNetwork = CityDataManager.LoadRoadNetwork(roadNetworkData);
        var result = CityDataManager.GetBuildingsInNetwork(roadNetwork);
        return Ok(new { result });
    }
}