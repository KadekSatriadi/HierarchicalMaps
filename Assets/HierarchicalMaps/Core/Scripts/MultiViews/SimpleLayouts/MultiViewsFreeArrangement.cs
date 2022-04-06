using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiViewsFreeArrangement : MutiViewsArrangementManager
{
    public float offset = 0.25f;
    public float scale = 2f;
    public Transform parent;
    public MutiViewsArrangementManager layout;
    protected List<Color> lensColors = new List<Color>();

    public float limitMinH;
    public float limitMinW;
    public float limitMaxH;
    public float limitMaxW;

    private void Start()
    {
        //override color palette
        lensColors = ColorPalette.GetColourPalette12();

        //copy color to stack
        foreach (Color c in lensColors)
        {
            colorMapLensDictionary.Add(c, null);
        }
    }
    public override void Register(MapLens map)
    {
        map.transform.SetParent(parent);
        //Position the child map
        if (map.parent != null) //not parent map
        {
            map.transform.rotation = map.parent.transform.rotation;
            map.transform.position = map.viewFinder.transform.position;
            map.PlayAnimationMovement(map.transform.position + map.viewFinder.transform.up * offset, delegate {
                float ratio = map.clipController.width / map.clipController.height;

                float h = Mathf.Min(Mathf.Max(map.clipController.height * scale, limitMinH), limitMaxH);
                float w = h * ratio;
                w = Mathf.Min(Mathf.Max(w, limitMinW), limitMaxW);
                map.PlayScaleByDimensionAnimate(h, w, delegate {
 
                });
            });
            SetColor(map);
        }
        if(layout) layout.Register(map);
    }

    public override void Remove(MapLens map)
    {
        throw new System.NotImplementedException();
    }
}
