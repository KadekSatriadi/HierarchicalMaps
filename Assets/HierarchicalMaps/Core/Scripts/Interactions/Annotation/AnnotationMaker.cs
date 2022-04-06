using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AnnotationMaker : InteractionModule
{
    public float distance = 0.1f;

    protected List<LineRenderer> lines = new List<LineRenderer>();
    protected LineRenderer line;
    protected LineRenderer activeLine;
    public void CreateNewLine(GameObject p)
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;

        GameObject l = new GameObject("line" + lines.Count);
        l.transform.SetParent(p.transform);

        l.AddComponent<LineRenderer>();
        l.GetComponent<LineRenderer>().material = line.material;
        l.GetComponent<LineRenderer>().widthMultiplier = line.widthMultiplier;
        l.GetComponent<LineRenderer>().loop = line.loop;
        l.GetComponent<LineRenderer>().useWorldSpace = line.useWorldSpace;
        l.GetComponent<LineRenderer>().alignment = line.alignment;

        

        activeLine = l.GetComponent<LineRenderer>();
        activeLine.positionCount = 0;
        lines.Add(activeLine);
    }

    public void Draw(Vector3 point)
    {
        if (!enable) return;

        //offset
        point -= new Vector3(0, 0, 0.005f);
        if(activeLine.positionCount <= 0)
        {
            activeLine.positionCount += 1;
            activeLine.SetPosition(0, point);
        }
        else
        {
            Vector3 lastPoint = activeLine.GetPosition(activeLine.positionCount - 1);
            if (Vector3.Distance(point, lastPoint) > distance)
            {
                activeLine.positionCount++;
                activeLine.SetPosition(activeLine.positionCount - 1, point);
            }
        }
    }


}
