using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegisterOnReady : MonoBehaviour
{
    public MapLens map;
    public MutiViewsArrangementManager layout;

    private void Start()
    {
        map.OnReady += delegate
        {
            layout.Register(map);
        };
    }
}
