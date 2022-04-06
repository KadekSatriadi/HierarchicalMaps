using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayAtCorner : MonoBehaviour
{
    public MapLens map;
    [Range(0, 3)]
    public int cornerIndex = 3;
    public Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (map.IsMapReady())
        {
            transform.localPosition = transform.parent.InverseTransformPoint(map.clipController.GetRectanglePoints()[cornerIndex]) + offset;
        }
    }
}
