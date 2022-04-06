using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShrinkMapLens : MonoBehaviour
{
    public MapLens map;
    public UnityEvent OnShrink;
    public UnityEvent OnUnshrink;

    private bool isShrink = false;
    private float w = 0;
    private float h = 0;
    private float threshold = 0.15f;
    public void ShrinkToggle()
    {
        if (isShrink)
        {
            Unshrink();
        }
        else
        {
            Shrink();           
        }
    }

    public void Shrink()
    {
        if (map.viewFinder == null) return;

        w = map.clipController.width;
        h = map.clipController.height;

        if(map.viewFinder.width > threshold && map.viewFinder.height > threshold)
        {
            map.PlayScaleByDimensionAnimate(map.viewFinder.height, map.viewFinder.width, delegate {
                isShrink = true;
            });
        }
        else
        {
            map.PlayScaleByDimensionAnimate(threshold, threshold, delegate {
                isShrink = true;
            });
        }

        OnShrink.Invoke();
    }

    public void Unshrink()
    {
        if (map.viewFinder == null) return;

        map.PlayScaleByDimensionAnimate(h, w, delegate {
            isShrink = false;
        });

        OnUnshrink.Invoke();
    }
}
