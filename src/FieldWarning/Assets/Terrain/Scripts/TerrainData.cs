using UnityEngine;

public class TerrainData
{
    public static Vector3 Size;
    static float[,,] alphaMap;
    static int width, height;

    public static void initialize(float[,,] alphaMap)
    {
        /*TerrainData.alphaMap = alphaMap;
        width = alphaMap.GetLength(0);
        height = alphaMap.GetLength(1);
        var data = GameObject.Find("Terrain").GetComponent<Terrain>().terrainData;
        
        size = data.size;*/
    }

    public static bool visionScore(Transform t1, Transform t2, float detLimit)
    {
        var d0 = VisibilityManager.MIN_VIEW_DISTANCE;
        float resolution = 1f;
        var p1 = t1.position;
        var p2 = t2.position;
        var dp = p2 - p1;

        var length = dp.magnitude;
        var dx = resolution * dp.x / length;
        var dy = resolution * dp.z / length;

        float clearScore = 1;
        float forestScore = 1 / 2f;
        float x = p1.x + d0 * dx;
        float y = p1.z + d0 * dy;
        float score = d0 * clearScore;
        //var offset = terrain.terrainData.size;
        for (float d = d0; d < length; d += resolution) {
            x += dx;
            y += dy;
            //var pos = t2.transform.position;
            //var x = (int)(fractalSize * Mathf.RoundToInt(pos.z + offset.x / 2) / offset.x) - 1;//possibly incorrect
            //var y = (int)(fractalSize * Mathf.RoundToInt(pos.x + offset.z / 2) / offset.z) - 1;
            //if (i == 0) Debug.Log(string.Format("[{0}, {1}]", x, y));
            var forestRate = getTerrainRate(x, y, TerrainType.Forest);
            score += (forestRate * forestScore * d + (1 - forestRate) * clearScore) * resolution;

            if (score >= detLimit)
                return false;
        }
        return true;
    }

    static float getTerrainRate(float x, float y, int type)
    {
        int i = Mathf.RoundToInt((y / Size.z + .5f) * width);
        int j = Mathf.RoundToInt((x / Size.x + .5f) * height);
        return alphaMap[i, j, type];
        /*var dx = 1.0f / (size.z - 1);
        var dy = 1.0f / (size.x - 1);
        var normX = (y + size.z / 2) * dx;
        var normY = (x + size.x / 2) * dy;

        var X = normX * (fractalSize - 2);
        var Y = normY * (fractalSize - 2);
        int xMin = (int)X;
        int yMin = (int)Y;
        float xt = X - xMin;
        float yt = Y - yMin;
        float frac = map[xMin, yMin] +
            (map[xMin + 1, yMin] - map[xMin, yMin]) * xt +
            (map[xMin, yMin + 1] - map[xMin, yMin]) * yt +
            (map[xMin + 1, yMin + 1] + map[xMin, yMin] - (map[xMin + 1, yMin] + map[xMin, yMin + 1])) * xt * yt;
        return frac;*/
    }

    static float forestMap(Vector3 position)
    {
        return 0;
    }

    public static float populationScore(Vector3 position, Vector3 offset)
    {
        float score = 0;
        var p = position;
        var i = 0;

        while (inBounds(p)) {

            score += getTerrainRate(position.x, position.z, TerrainType.Forest);
            p += offset;
            if (i > 1000) {
                Debug.LogError(p);
                break;
            }
            i++;
        }
        //Debug.Log(score);
        return score;
    }

    static bool inBounds(Vector3 position)
    {
        return position.z / Size.z < .5f && position.z / Size.z > -.5f && position.x / Size.x < .5f && position.x / Size.x > -.5f;
    }
}

class TerrainType
{
    public const int Forest = 0;
    public const int Plain = 1;
    public const int Field1 = 2;
    public const int Field2 = 3;
    public const int Hedge = 4;
    public const int Town = 5;
}
