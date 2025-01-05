using GLOKON.Baiters.GodotInterop.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GLOKON.Baiters.GodotInterop
{
    public static partial class TscnReader
    {
        public static TextScene ReadTextScene(string file, string[] pointTypes)
        {
            IDictionary<string, Vector3[]> pointLocations = new Dictionary<string, Vector3[]>();

            string fileContents = File.ReadAllText(file);
            var fileLines = fileContents.Split('\n');

            foreach (var pointType in pointTypes)
            {
                IList<Vector3> points = [];

                for (int i = 0; i < fileLines.Length; i++)
                {
                    Match isPointGroup = PointGroup().Match(fileLines[i]);
                    if (isPointGroup.Success && isPointGroup.Groups[1].Value == $"\"{pointType}\"")
                    {
                        string transformPattern = @"Transform\(.*?,\s*(-?\d+\.?\d*),\s*(-?\d+\.?\d*),\s*(-?\d+\.?\d*)\s*\)";
                        Match match = Regex.Match(fileLines[i + 1], transformPattern);

                        float x = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                        float y = float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                        float z = float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

                        points.Add(new(x, y, z));
                    }
                }

                pointLocations[pointType] = points.ToArray();
            }

            return new TextScene
            {
                SceneLocations = pointLocations,
            };
        }

        [GeneratedRegex(@"groups=\[([^\]]*)\]")]
        private static partial Regex PointGroup();
    }
}
