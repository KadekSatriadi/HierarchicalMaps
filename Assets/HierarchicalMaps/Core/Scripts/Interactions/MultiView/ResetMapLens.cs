using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetMapLens : MonoBehaviour
{
    public MapLens map;

    private string latLong;
    private float zoom;

    private void Awake()
    {
        if (map.isInitialisedOnStart)
        {
            if (map.latLong.Length > 0)
            {
                latLong = map.latLong;
            }
            zoom = map.zoom;
        }
       
    }

    public void Reset()
    {
        map.abstractMap.Options.locationOptions.latitudeLongitude = latLong;
        map.abstractMap.Options.locationOptions.zoom = zoom;
        map.abstractMap.UpdateMap();
    }
}
