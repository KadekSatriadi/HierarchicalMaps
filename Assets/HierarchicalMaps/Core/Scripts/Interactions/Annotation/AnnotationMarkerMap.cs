using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationMarkerMap : AnnotationMaker
{

    protected Transform parentObj;
    public GameObject lineMenuPrefab;
    public Color lineColor; 

    public void CreateNewLineOnMap(GameObject p, Vector3 hitPoint)
    {
        MapLens m = p.GetComponent<MapLens>();
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;


        GameObject l = new GameObject("line" + lines.Count);
        l.tag = "Annotation";
        l.transform.position = hitPoint;
        l.transform.SetParent(p.transform);
        l.AddComponent<LineMap>();
        LineRenderer lr =  l.AddComponent<LineRenderer>();
        lr.material = line.material;
        lr.widthMultiplier = line.widthMultiplier;
        lr.loop = line.loop;
        lr.useWorldSpace = line.useWorldSpace;
        lr.alignment = line.alignment;
        lr.material.SetFloat("_StencilRef", m.clipController.clipID);
        lr.material.renderQueue++;
        lr.material.color = lineColor;
        lr.SetColors(lineColor, lineColor);

        if (m) l.GetComponent<LineMap>().Register(lr, m, hitPoint);

        m.clipController.AddExtraClipMaterial(lr.material);
    
        activeLine = l.GetComponent<LineRenderer>();
        activeLine.positionCount = 0;
        lines.Add(activeLine);

        parentObj = l.transform;

        GameObject lm = Instantiate(lineMenuPrefab);
        lm.transform.position = l.transform.position;
        lm.transform.SetParent(l.transform);
        LineMapMenu lmn = lm.GetComponentInChildren<LineMapMenu>();
        if (lmn) lmn.line = l.transform;
    }

    Vector3 Offset(Vector3 i)
    {
        return  (i - parentObj.transform.forward * 0.005f);
    }

    public new void Draw(Vector3 point)
    {
        if (!enable) return;

        if (activeLine.positionCount <= 0)
        {
            activeLine.positionCount += 1;
            activeLine.SetPosition(0, Offset(parentObj.InverseTransformPoint(point)));
        }
        else
        {
            Vector3 lastPoint = activeLine.GetPosition(activeLine.positionCount - 1);
            if (Vector3.Distance(point, lastPoint) > distance)
            {
                activeLine.positionCount++;
                Vector3 offset = new Vector3(0, 0, -0.005f);
                //activeLine.SetPosition(activeLine.positionCount - 1, Offset(parentObj.InverseTransformPoint(point)));
                activeLine.SetPosition(activeLine.positionCount - 1, activeLine.transform.InverseTransformPoint(point) + offset);
            }
        }
    }
}
