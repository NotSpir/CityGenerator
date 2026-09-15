using System.Diagnostics;
using CityGeneration.Core.RoadLayouts;
using CityGeneration.Core.RoadModels;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using CityGeneration.Core.BuildingModels;
using CityGeneration.Core.Data.DataModels;

RoadNetwork roadNetwork =  new RoadNetwork();
RectangleLayout rectangle = new RectangleLayout(0, 1536, 1024, 5, 0.002f, 0.2f);
rectangle.GenerateCity(ref roadNetwork);
var buildings = BuildingSetter.SetBuildingDataInNetwork(ref roadNetwork, 33f, 4);

string directory = AppContext.BaseDirectory;
DrawNetwork(roadNetwork, buildings, directory + "\\output.png");


Process.Start(new ProcessStartInfo
{
    FileName = directory + "\\output.png",
    UseShellExecute = true 
});
return;



static void DrawNetwork(RoadNetwork roadNetwork, List<BuildingData> buildings, string outputPath)
{
    int width = 2600;
    int height = 2000;
    float halfWidth = width / 2f;
    float halfHeight = height / 2f;
    using var bitmap = new Bitmap(width, height);
    using (Graphics g = Graphics.FromImage(bitmap))
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Color.White);

        foreach (var seg in roadNetwork.RoadSegments)
        {
            var startV = roadNetwork.GetVertex(seg.StartId);
            var endV = roadNetwork.GetVertex(seg.EndId);
            using var bluePen = new Pen(Color.Blue, 2);
            g.DrawLine(bluePen, startV.X + halfWidth, startV.Y + halfHeight, endV.X + halfWidth, endV.Y + halfHeight);
        }

        foreach (var building in buildings)
        {
            var a = building.Corners[0];
            var b = building.Corners[1];
            var c = building.Corners[2];
            var d = building.Corners[3];
            var w = MathF.Abs(c.X - a.X);
            var h = MathF.Abs(c.Y - a.Y);
            /*using var buildBrush = new SolidBrush(Color.Gray);
            g.FillRectangle(buildBrush, a.X+halfWidth,a.Y+halfHeight,w,h);*/
            
            using var buildingPen = new Pen(Color.Black, 4);
            g.DrawLine(buildingPen, a.X + halfWidth, a.Y + halfHeight, b.X + halfWidth, b.Y + halfHeight);
            g.DrawLine(buildingPen, b.X + halfWidth, b.Y + halfHeight, c.X + halfWidth, c.Y + halfHeight);
            g.DrawLine(buildingPen, c.X + halfWidth, c.Y + halfHeight, d.X + halfWidth, d.Y + halfHeight);
            g.DrawLine(buildingPen, d.X + halfWidth, d.Y + halfHeight, a.X + halfWidth, a.Y + halfHeight);
        }
    }

    // Save as PNG
    bitmap.Save(outputPath, ImageFormat.Png);
}
