using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [System.Serializable]
    public struct InteractModule
    {
        public InteractionModule module;
        public bool enable;
    }

    public List<InteractModule> modules = new List<InteractModule>();


    private void Start()
    {
        foreach(InteractModule m in modules)
        {
            ApplyInteract(m);
        }        
    }

    public void ActivateOnly(int idx)
    {
        DeactivateAll();
        Activate(idx);
    }

    public void Activate(int idx)
    {
        modules[idx].module.Enable();
    }

    public void DeactivateAll()
    {
        for(int i = 0; i < modules.Count; i++)
        {
            modules[i].module.Disable();
        }
    }

    private void ApplyInteract(InteractModule m)
    {
        if (m.enable)
        {
            m.module.Enable();
        }
        else
        {
            m.module.Disable();
        }
    }
}
