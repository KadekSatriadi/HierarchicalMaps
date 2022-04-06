using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionPolicy : MonoBehaviour
{
    public enum Policy
    {
        Include, Exclude, NoPolicy
    }

    public Policy policy;
    public List<MapLens> mapLenses;

    public bool FollowPolicy(MapLens m)
    {
        if (m == null) return false;
        if (mapLenses.Count == 0) return true;

        switch (policy)
        {
            case Policy.Include:
                if (mapLenses.Contains(m))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case Policy.Exclude:
                if (mapLenses.Contains(m))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            default:
                return true;
        }
    }
}
