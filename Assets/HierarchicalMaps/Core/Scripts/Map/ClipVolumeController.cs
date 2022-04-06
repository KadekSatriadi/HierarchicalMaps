using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipVolumeController : MonoBehaviour
{
    public MapLens map;

    // Start is called before the first frame update
    void Start()
    {
        map.clipController.OnUpdate += delegate {
            UpdateDimension();
        };

        map.OnPanned += delegate
        {
            UpdateDimension();
        };

        map.OnZoomed += delegate
        {
            UpdateDimension();
        };
    }

    private void UpdateDimension()
    {
        //height and width
        float width = map.clipController.width;
        float height = map.clipController.height;
        float ratio = 1f/ transform.localScale.x;

        Material m = GetComponent<Renderer>().material;
        GetComponent<Renderer>().material.SetFloat("_Width",  ratio * width * 0.5f);
        GetComponent<Renderer>().material.SetFloat("_Height", ratio * height * 0.5f);

        Vector3[] p = map.clipController.GetRectanglePoints();
        m.SetVector("_P1", p[0]);
        m.SetVector("_P2", p[1]);
        m.SetVector("_P4", p[2]);
        m.SetVector("_P5", p[3]);
        m.SetFloat("_Width", width);
        m.SetFloat("_Length", height);

        //pivots
        Vector3 direction = transform.position - map.transform.position;
        direction = transform.InverseTransformDirection(direction);
        GetComponent<Renderer>().material.SetFloat("_OffsetW", ratio * direction.x);
        GetComponent<Renderer>().material.SetFloat("_OffsetH", ratio * direction.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
