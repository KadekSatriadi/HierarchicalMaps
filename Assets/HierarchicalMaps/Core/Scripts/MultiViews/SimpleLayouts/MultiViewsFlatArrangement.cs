using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiViewsFlatArrangement : MutiViewsArrangementManager
{
    public float viewfinderOffset = 0;

    public override void Remove(MapLens map)
    {
        return;
    }
    public override void Register(MapLens map)
    {
        //Position the child map
        if (map.parent != null) //not parent map
        {
            //Vector3 directionFromParent = (map.viewFinder.transform.position - map.parent.transform.position).normalized;
            //float distanceFromParent = map.clipController.radius + map.parent.clipController.radius + viewfinderOffset;
            //directionFromParent *= distanceFromParent;
            //Vector3 closestPoint = directionFromParent + map.parent.transform.position;
            //map.transform.position = closestPoint;
            map.transform.position = map.viewFinder.transform.position + map.viewFinder.transform.up * 0.02f;
            map.transform.rotation = map.parent.transform.rotation;
            map.clipController.width = map.viewFinder.width * 2f;
            map.clipController.height = map.viewFinder.height * 2f;

        }
       


        //Put on sphere
        /*
        float azimuth = Mathf.Deg2Rad * azimuthalIncrement * map.level;
        float polarRad = Mathf.Deg2Rad * polar;

        //convert to Cartesian
        float z = radius * Mathf.Sin(polarRad) * Mathf.Cos(azimuth);
        float x = radius * Mathf.Sin(polarRad) * Mathf.Sin(azimuth);
        float y = radius * Mathf.Cos(polarRad);*/


        //map.transform.position = new Vector3(x, y, z);
       // map.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
        //get the closest point
        //Find the unit vector from center to point, multiply it by radius, that is the point you are looking for.

    }



    private void Update()
    {
        
    }
}
