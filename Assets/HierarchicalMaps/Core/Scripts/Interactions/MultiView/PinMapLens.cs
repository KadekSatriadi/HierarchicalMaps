using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PinMapLens : MonoBehaviour
{
    public MapLens map;
    public ShrinkMapLens shrink;
    public UnityEvent OnPinned;
    public UnityEvent OnUnpinned;

    private bool isPinned = false;
    private Vector3 pos = Vector3.zero;
    private Quaternion rot;

    public void PinToggle()
    {
        if (isPinned)
        {
            map.PlayAnimationMovement(pos, delegate
            {
                map.transform.SetParent(null);
                map.viewFinder.ChildMapUnfollowMe();
                map.SetPin(false);
                isPinned = false;
            });
            if (shrink)
            {
                shrink.Unshrink();
            }
            
            OnUnpinned.Invoke();
        }
        else
        {
            pos = map.transform.position;
            rot = map.transform.rotation;

            Vector3 position = map.viewFinder.GetRectangleCenter();
            Quaternion rotation = map.viewFinder.transform.rotation;

            map.transform.rotation = rotation;
            map.PlayAnimationMovement(position, delegate
            {
                map.transform.SetParent(map.parent.transform);
                map.viewFinder.ChildMapFollowMe();
                map.SetPin(true);
                isPinned = true;
            });
            if (shrink)
            {
                shrink.Shrink();
            }
            OnPinned.Invoke();
        }
    }
}
