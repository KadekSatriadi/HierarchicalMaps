using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMap : MonoBehaviour
{
    public LineRenderer line;
    public MapLens map;

    protected Vector2d latLong;
    protected float initalZoom;
    public void Register(LineRenderer l, MapLens m, Vector3 center)
    {
        line = l;
        map = m;
        latLong = m.abstractMap.WorldToGeoPosition(center);
        initalZoom = m.abstractMap.Zoom;
    }

    private void Update()
    {
        if (line != null && map != null)
        {
            // transform.localRotation = map.transform.rotation;\
            float offset = 0.025f;
            transform.localRotation = Quaternion.Euler(90, 0, 0);
            transform.localPosition = map.transform.InverseTransformPoint(map.abstractMap.GeoToWorldPosition(latLong) + map.transform.up * offset);
            Vector3 scale = Vector3.one *  (float) MapFormula.ZoomToMeterInterpolation(initalZoom, map.abstractMap.Zoom, 1f);
            transform.localScale = new Vector3(scale.x, scale.y, 1);
        }
    }

}
