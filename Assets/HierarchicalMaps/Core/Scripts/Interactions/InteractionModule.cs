using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionModule : MonoBehaviour
{
    public bool enable;
    public void Enable()
    {
        enable = true;
    }
    public void Disable()
    {
        enable = false;
    }
}
