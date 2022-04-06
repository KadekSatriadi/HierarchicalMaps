using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuplicateMapLens : MonoBehaviour
{
    public MapLens map;
    private float offset = 0.15f;
    public void Duplicate()
    {
        MapLens parent = map.GetComponent<MapLens>().parent;

        GameObject newMap = Instantiate(map.gameObject) as GameObject;
        GameObject newVF = Instantiate(map.GetComponent<MapLens>().viewFinder.gameObject) as GameObject;

        MapLens m = newMap.GetComponent<MapLens>();
        ViewFinder vf = newVF.GetComponent<ViewFinder>();

        Vector3 position = map.transform.position + map.transform.right * (map.clipController.width + offset);
        m.Zoom(map.abstractMap.Zoom);
        m.PlayAnimationMovement(position);
        m.clipController.clipID = newMap.GetInstanceID();

        map.parent = parent;
        m.parent = parent;

        map.viewFinder.parent = parent;
        map.viewFinder.child = map;
        vf.parent = parent;
        vf.child = m;
        
    }
}
