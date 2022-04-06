using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSizer : MonoBehaviour
{
    public MapLens map;

    public Vector2 smallSize;
    public Vector2 mediumSize;
    public Vector2 largeSize;

    public Vector3 smallPos;
    public Vector3 mediumPos;
    public Vector3 largePos;

    private float initialZoom;
    private Vector2 initialSize;

    private void Start()
    {
        map.OnReady += delegate
        {
            initialZoom = map.abstractMap.Zoom;
            initialSize = new Vector2(map.clipController.width, map.clipController.height);
        };
    }

    public void SetSmall()
    {
        map.PlayScaleByDimensionAnimate(smallSize.y, smallSize.x, delegate {
            map.PlayAnimationMovement(smallPos);
        });
    }

    public void SetMedium()
    {
        map.PlayScaleByDimensionAnimate(mediumSize.y, mediumSize.x, delegate {
            map.PlayAnimationMovement(mediumPos);
        });
    }

    public void SetLarge()
    {
        map.PlayScaleByDimensionAnimate(largeSize.y, largeSize.x, delegate {
            map.PlayAnimationMovement(largePos);
        });

    }
}
