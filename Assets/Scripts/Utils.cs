using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static Color GetColorFromRGB(float r, float g, float b)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
    }

    public static int GetLayerMaskFromLayerIndex(int index)
    {
        return 1 << index;
    }
    
    public static List<Color> boulderColors = new List<Color>
    {
        Color.black,
        Color.white,
        GetColorFromRGB(0, 78, 0), // dark green
        GetColorFromRGB(255, 51, 154), // pink
        GetColorFromRGB(129, 24, 20), // dark red
        GetColorFromRGB(255, 102, 0), // orange
    };

    public static bool PositionsOverlap(Vector2Int p1, Vector2Int p2, float vicinity)
    {
        return p1.x - vicinity <= p2.x && p2.x <= p1.x + vicinity &&
               p1.y - vicinity <= p2.y && p2.y <= p1.y + vicinity;
    }

    public static Vector2Int Vec3ToVec2Int(Vector3 v)
    {
        Vector3Int v_int = Vector3Int.FloorToInt(v);
        return new Vector2Int(v_int.x, v_int.y);
    }

    public static Vector3 GetRandomPositionInZone(Transform zone, float width_dampner, float height_dampner)
    {
        Vector3 zone_scale = zone.localScale;
        Vector3 zone_position = zone.localPosition;
        
        return new Vector3(
            Random.Range(-zone_scale.x / 2 + width_dampner + zone_position.x,
                zone_scale.x / 2 - width_dampner + zone_position.x),
            Random.Range(-zone_scale.y / 4 + height_dampner + zone_position.y,
                zone_scale.y / 4 - height_dampner + zone_position.y),
            0.25f
        );
    }
}
