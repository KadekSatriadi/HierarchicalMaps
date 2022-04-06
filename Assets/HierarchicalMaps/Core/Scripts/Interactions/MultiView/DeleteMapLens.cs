using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteMapLens : MonoBehaviour
{
    public MapLens map;

    public void Delete()
    {
        Destroy(map.gameObject);
    }
}
