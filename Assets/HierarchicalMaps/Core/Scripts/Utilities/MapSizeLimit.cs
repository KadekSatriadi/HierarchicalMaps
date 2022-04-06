using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSizeLimit : MonoBehaviour
{
    public MapLens map;
    public bool limitMaxSize = false;
    public bool limitMinSize = false;
    public bool enable = true;

    public Vector2 maxLimit;
    public Vector2 minLimit;

    private bool isAnimating = false;
    // Update is called once per frame
    void Update()
    {
        if (map.IsMapReady() && !map.IsAnimating() && enable && !isAnimating)
        {
            if (limitMaxSize && map.clipController.width > maxLimit.x || map.clipController.height > maxLimit.y)
            {
               float w = Mathf.Min(map.clipController.width, maxLimit.x);
               float h = Mathf.Min(map.clipController.height, maxLimit.y);
                isAnimating = true;
                map.PlayHeightWidthAnimate(h, w, delegate {

                    isAnimating = false;
                });

            }
            if (limitMinSize && map.clipController.width < minLimit.x || map.clipController.height < minLimit.y)
            {
                float w = Mathf.Max(map.clipController.width, minLimit.x);
                float h = Mathf.Max(map.clipController.height, minLimit.y);
                isAnimating = true;
                map.PlayHeightWidthAnimate(h, w, delegate {

                    isAnimating = false;
                });
            }
        }
    }
}
