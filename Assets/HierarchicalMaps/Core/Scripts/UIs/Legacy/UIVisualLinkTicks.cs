using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIVisualLinkTicks : MonoBehaviour
{
    public VisualLinkController visualLinkController;
    public int numberOfTicks = 0;

    protected float margin;
    protected List<Transform> ticksTransforms;
    protected bool isActive = false;
    private void Awake()
    {
        ticksTransforms = new List<Transform>();
        for(int i = 0; i < numberOfTicks; i++)
        {
            GameObject n = new GameObject("Tick" + i);
            n.AddComponent<MeshLineStripRenderer>();
            MeshLineStripRenderer line = n.GetComponent<MeshLineStripRenderer>();
            line.SetPointsCount(51);
            Material m = new Material(Shader.Find("Lightweight Render Pipeline/Unlit"));
            line.material = m;
            n.transform.SetParent(visualLinkController.transform);
            ticksTransforms.Add(n.transform);
        }
    }

    public void Show()
    {
        foreach (Transform t in ticksTransforms)
        {
            t.GetComponent<MeshLineStripRenderer>().SetActive(true);
        }
        isActive = true;
    }

    public void Hide()
    {
        foreach (Transform t in ticksTransforms)
        {
            t.GetComponent<MeshLineStripRenderer>().SetActive(false);
        }
        isActive = false;
    }

    private void Update()
    {
        if (!isActive) return;
        if (visualLinkController == null) return;
        if (visualLinkController.top == null || visualLinkController.bottom == null) return;

        float distance = Vector3.Distance(visualLinkController.top.position, visualLinkController.bottom.position);
        float margin = distance / (numberOfTicks + 1);
        int i = 1;
        foreach (Transform t in ticksTransforms)
        {
            float diff = visualLinkController.topRadius  - visualLinkController.bottomRadius ;
            float radius =  visualLinkController.bottomRadius + (diff * (i * margin/numberOfTicks));
            t.position = visualLinkController.bottom.position + (visualLinkController.top.position - visualLinkController.bottom.position).normalized  * margin * i;

            MeshLineStripRenderer line = t.GetComponent<MeshLineStripRenderer>();
            line.SetPointsCount(51);
            line.material.SetColor("_BaseColor", visualLinkController.color);
            i++;
        }
    }


  

}
