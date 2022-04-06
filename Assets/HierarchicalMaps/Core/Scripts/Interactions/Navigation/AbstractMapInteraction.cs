using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractMapInteraction: InteractionModule
{
    public AbstractMap map;
    [Range(0, 200)]
    public float zoomSpeed = 1.5f;

    /// <summary>
    /// Panning
    /// </summary>
    /// <param name="wordDirection">Vector direction in world position</param>
    public void Pan(Vector3 wordDirection)
    {
         // float factor = panSpeed * (Conversions.GetTileScaleInDegrees((float) map.CenterLatitudeLongitude.x, map.AbsoluteZoom));
        //  var latitudeLongitude = new Vector2d(map.CenterLatitudeLongitude.x + y * factor * 2.0f, map.CenterLatitudeLongitude.y + x * factor * 4.0f);
        Vector3 centerWorld = map.GeoToWorldPosition(map.CenterLatitudeLongitude);
        Vector3 newCenter = centerWorld + wordDirection;
        var latitudeLongitude = map.WorldToGeoPosition(newCenter);

        map.UpdateMap(latitudeLongitude);
    }

    /*
     * <summary>
     * Zoom to point on Map instead of zooming to center
     * </summary>
     * */
    public  void Zoom(Vector3 target, float value)
    {
        Vector3 mapCenter = map.GeoToWorldPosition(map.CenterLatitudeLongitude);

        target -= mapCenter;
        var zoom = Mathf.Max(0.0f, Mathf.Min(map.Zoom + value * zoomSpeed, 21.0f));
        var change = zoom - map.Zoom;

        //0.7f is a constant
        var offsetX = target.x * change * 0.7f;
        var offsetY = target.z * change * 0.7f;

        Vector3 newCenterUnity = new Vector3(offsetX, 0, offsetY) + mapCenter;
        Vector2d newCenter = map.WorldToGeoPosition(newCenterUnity);

        map.UpdateMap(newCenter, zoom);
    }

    public void Zoom(Vector2d target, float value)
    {
        Vector3 targetUnity = map.GeoToWorldPosition(target);
        Zoom(targetUnity, value);
    }

    public  void Zoom(float value)
    {
        var zoom = Mathf.Max(0.0f, Mathf.Min(map.Zoom + value * zoomSpeed, 21.0f));
        map.UpdateMap(map.CenterLatitudeLongitude, zoom);
    }

}
