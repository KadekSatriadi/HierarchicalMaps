using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIZoomCanvas : MonoBehaviour
{
    public MapLens map;
    // Update is called once per frame
    private Vector3 initScale;

    private void Start()
    {
        foreach(Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        initScale = transform.localScale;
        map.OnReady += delegate
        {
            UpdateScale();
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = true;
            }
        };
    }

    private void UpdateScale()
    {
        if (map.abstractMap.Terrain.ElevationType == Mapbox.Unity.Map.ElevationLayerType.FlatTerrain)
        {
            transform.position = map.clipController.GetRectanglePoints()[3] + map.transform.up * 0.002f;
        }
        if (map.abstractMap.Terrain.ElevationType == Mapbox.Unity.Map.ElevationLayerType.TerrainWithElevation)
        {
            transform.position = map.clipController.GetRectanglePoints()[3] + map.transform.up * 0.002f + map.transform.right * 0.065f;
        }
        transform.localScale = initScale * (map.clipController.height / 1f);
    }

    void Update()
    {
        if (map.IsMapReady())
        {
            UpdateScale(); 
        }
    }
}
