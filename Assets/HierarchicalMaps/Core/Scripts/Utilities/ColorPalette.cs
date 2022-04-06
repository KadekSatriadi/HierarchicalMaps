using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorPalette 
{
   public static List<Color> GetColourPalette12()
    {
        Color color = Color.black;
        List<Color> colors = new List<Color>();

        ColorUtility.TryParseHtmlString("#b15928", out color);
        colors.Add(color);

        ColorUtility.TryParseHtmlString("#1f78b4", out color);
        colors.Add(color);

        ColorUtility.TryParseHtmlString("#b2df8a", out color);
        colors.Add(color);

        ColorUtility.TryParseHtmlString("#33a02c", out color);
        colors.Add(color);

        ColorUtility.TryParseHtmlString("#fb9a99", out color);
        colors.Add(color);

        ColorUtility.TryParseHtmlString("#e31a1c", out color);
        colors.Add(color);

        ColorUtility.TryParseHtmlString("#fdbf6f", out color);
        colors.Add(color);

        ColorUtility.TryParseHtmlString("#ff7f00", out color);
        colors.Add(color);

        ColorUtility.TryParseHtmlString("#cab2d6", out color);
        colors.Add(color);

        ColorUtility.TryParseHtmlString("#6a3d9a", out color);
        colors.Add(color);

        ColorUtility.TryParseHtmlString("#ffff99", out color);
        colors.Add(color);

        ColorUtility.TryParseHtmlString("#a6cee3", out color);
        colors.Add(color);

        return colors;
    }
}
