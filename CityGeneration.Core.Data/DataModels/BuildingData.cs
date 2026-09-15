namespace CityGeneration.Core.Data.DataModels;

public class BuildingData
{
    public BuildingData(Vector2[] corners)
    {
        Corners = corners;
    }

    public Vector2[] Corners { get; set; }
}