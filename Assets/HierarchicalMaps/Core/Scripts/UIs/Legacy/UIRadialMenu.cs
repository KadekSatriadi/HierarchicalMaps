using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRadialMenu : MonoBehaviour
{

    public System.Action OnMenuHover = delegate { };
    public System.Action OnMenuTriggered = delegate { };

    private Vector3 worldCurrentPosition;
    private Vector3 worldStartPosition;
    public float innerRadius;
    public float outterRadius;

    private bool isHover = false;
    private bool isTriggered = false;

    private MeshLineStripRenderer innerLine;
    private MeshLineStripRenderer outterLine;
    public void Awake()
    {
        GameObject inner = new GameObject("innerLine");
        inner.AddComponent<MeshLineStripRenderer>();
        inner.transform.SetParent(transform);
        innerLine = inner.GetComponent<MeshLineStripRenderer>();
        CreateCircle(innerLine, 100, innerRadius);

        GameObject outter = new GameObject("outterLine");
        outter.AddComponent<MeshLineStripRenderer>();
        outter.transform.SetParent(transform);
        outterLine = outter.GetComponent<MeshLineStripRenderer>();
        CreateCircle(outterLine, 100, outterRadius);

        Hide();
    }

    public void Show(Vector3 pos, Quaternion rotation)
    {
        worldStartPosition = pos;
        transform.position = pos;
        transform.rotation = rotation;
        innerLine.gameObject.SetActive(true);
        outterLine.gameObject.SetActive(true);
    }

    public void Hide()
    {
        innerLine.gameObject.SetActive(false);
        outterLine.gameObject.SetActive(false);
    }

    public void UpdateCursorPosition(Vector3 currentPos)
    {
        worldCurrentPosition = currentPos;
        float distance = Vector3.Distance(transform.InverseTransformPoint(worldCurrentPosition), transform.InverseTransformPoint(worldStartPosition));
        if((distance > innerRadius) && (distance <= outterRadius))
        {
            if (!isHover)
            {
                OnMenuHover.Invoke();
                isHover = true;
            }
            SetLineColor(innerLine, Color.green);
        }
        else if((distance <= innerRadius) && (distance <= outterRadius))
        {
            isHover = false;
            isTriggered = false;
            SetLineColor(innerLine, Color.white);
        }
        else if ((distance > outterRadius))
        {
            if (!isTriggered)
            {
                OnMenuTriggered.Invoke();
                isTriggered = true;
                Hide();
            }
        }
    }

    void SetLineColor(MeshLineStripRenderer line, Color c)
    {
        line.material.SetColor("_Color", c);
    }

    void CreateCircle(MeshLineStripRenderer line, int segments, float radius)
    {
        Material mat =  new Material(Shader.Find("Lightweight Render Pipeline/Unlit"));
        mat.SetColor("_BaseColor", Color.white);
        line.material = mat;
        line.SetPointsCount(segments + 1);
        //line.useWorldSpace = false;

        float x;
        float y = 0f;
        float z;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, new Vector2(x, z));

            angle += (360f / segments);
        }

    }
}
