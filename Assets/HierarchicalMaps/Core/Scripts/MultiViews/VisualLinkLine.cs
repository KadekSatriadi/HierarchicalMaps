using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualLinkLine : MonoBehaviour
{
    public ViewFinder viewFinder;
    public LineRenderer line1;
    public LineRenderer line2;
    public bool active = false;
    public Mode mode;

    public enum Mode
    {
        TwoD, ThreeD, OneD, CurvedOneD
    }

    private GameObject point1;
    private GameObject point2;

    private void Awake()
    {
        if (line1 != null) line1.enabled = false;
        if (line2 != null) line2.enabled = false;

        point1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);        
        point2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        point1.transform.localScale = Vector3.one * 0.005f;
        point2.transform.localScale = Vector3.one * 0.005f;
        point1.transform.SetParent(transform);
        point2.transform.SetParent(transform);


    }

    public void Hide()
    {
        if (line1 != null) line1.enabled = false;
        if (line2 != null) line2.enabled = false;
        point1.SetActive(false);
        point2.SetActive(false);
        active = false;
    }

    public void Show()
    {
        if (line1 != null) line1.enabled = true;
        if (line2 != null) line2.enabled = true;
        point1.SetActive(true);
        point2.SetActive(true);
        active = true;
    }


    private void Update()
    {

        if (line1 != null) line1.enabled = active;
        if (line2 != null) line2.enabled = active;


        if (active)
        {
            if (viewFinder != null)
            {
                if (viewFinder.parent != null && viewFinder.child != null)
                {
                    Vector3[] rectanglePoints = viewFinder.GetRectanglePoints();
                    Vector3[] extentPoints = viewFinder.child.clipController.GetRectanglePoints();

                    if(mode == Mode.TwoD)
                    {
                        line1.positionCount = 4;
                        line1.loop = true;

                        //Get two closest points

                        int i1 = 0;
                        int i2 = 0;

                        float d = 100000f;

                        for (int i = 0; i < rectanglePoints.Length; i++)
                        {
                            if (Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]) < d)
                            {
                                i1 = i;
                                d = Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]);
                            }
                        }

                        d = 100000f;

                        for (int i = 0; i < rectanglePoints.Length; i++)
                        {         
                            if (Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]) < d && i != i1)
                            {
                               i2 = i;
                                d = Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]);
                            }
                        }

                        line1.SetPosition(0, rectanglePoints[i1]);
                        line1.SetPosition(1, rectanglePoints[i2]);


                        d = 100000f;

                        for (int i = 0; i < extentPoints.Length; i++)
                        {
                            if(Vector3.Distance(viewFinder.transform.position, extentPoints[i]) < d)
                            {
                                i1 = i;
                                d = Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]);
                            }
                        }

                        d = 100000f;

                        for (int i = 0; i < extentPoints.Length; i++)
                        {
                            if (Vector3.Distance(viewFinder.transform.position, extentPoints[i]) < d)
                            {
                                if (i1 != i)
                                {
                                    i2 = i;
                                    d = Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]);
                                }
                            }
                        }

                        line1.SetPosition(2, extentPoints[i1]);
                        line1.SetPosition(3, extentPoints[i2]);
                        line1.material.SetColor("_Color", viewFinder.color);
                    }

                    if (mode == Mode.CurvedOneD)
                    {
                        line1.positionCount = 2;
                        line1.loop = false;

                        //Get two closest points

                        int ir1 = 0;
                        int ir2 = 0;
                        int ie1 = 0;
                        int ie2 = 0;

                        float d = 100000f;

                        for (int i = 0; i < rectanglePoints.Length; i++)
                        {
                            if (Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]) < d)
                            {
                                ir1 = i;
                                d = Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]);
                            }
                        }

                        d = 100000f;

                        for (int i = 0; i < rectanglePoints.Length; i++)
                        {
                            if (Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]) < d)
                            {
                                if (ir1 != i)
                                {
                                    ir2 = i;
                                    d = Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]);
                                }

                            }
                        }

                        d = 100000f;

                        for (int i = 0; i < extentPoints.Length; i++)
                        {
                            if (Vector3.Distance(viewFinder.transform.position, extentPoints[i]) < d)
                            {
                                ie1 = i;
                                d = Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]);
                            }
                        }

                        d = 100000f;

                        for (int i = 0; i < extentPoints.Length; i++)
                        {
                            if (Vector3.Distance(viewFinder.transform.position, extentPoints[i]) < d)
                            {
                                if (ie1 != i)
                                {
                                    ie2 = i;
                                    d = Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]);
                                }
                            }
                        }

                        Vector3 P2 = 0.5f * (extentPoints[ie1] + extentPoints[ie2]);
                        if (Vector3.Distance(P2, viewFinder.child.transform.position) < 0.05f) P2 = extentPoints[ie1];
                        Vector3 offset = (viewFinder.child.transform.position - P2).normalized * 0.5f * -viewFinder.child.clipController.border;

                        Vector3 P0 = 0.5f * (rectanglePoints[ir1] + rectanglePoints[ir2]);
                        P2 = P2 + offset;


                        Vector3 p1 = P0 - viewFinder.parent.transform.forward * Mathf.Min(Vector3.Distance(P0, P2), 0.25f);
                        Vector3 p3 = P2 - viewFinder.child.transform.forward * Mathf.Min(Vector3.Distance(P0, P2), 0.25f);
                        List<Vector3> points = MapFormula.GetQuadBezierPoints(P0, P2, Mathf.Min(Vector3.Distance(P2, P0), 0.15f), Vector3.down, 50);

                        //List<Vector3> points = MapFormula.GetCubicBezierPoints(P0, P2, p1, p3, 50);
                        line1.positionCount = points.Count;
                        line1.SetPositions(points.ToArray());
                        line1.material.color = viewFinder.color;

                        point1.transform.localPosition = transform.InverseTransformPoint(P0);
                        point2.transform.localPosition = transform.InverseTransformPoint(P2);
                        point1.GetComponent<MeshRenderer>().material.color = viewFinder.color;
                        point2.GetComponent<MeshRenderer>().material.color = viewFinder.color;

                    }

                    if (mode == Mode.OneD)
                    {
                        line1.positionCount = 2;
                        line1.loop = false;

                        //Get two closest points

                        int ir1 = 0;
                        int ir2 = 0;
                        int ie1 = 0;
                        int ie2 = 0;

                        float d = 100000f;

                        for (int i = 0; i < rectanglePoints.Length; i++)
                        {
                            if (Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]) < d)
                            {
                                ir1 = i;
                                d = Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]);
                            }
                        }

                        d = 100000f;

                        for (int i = 0; i < rectanglePoints.Length; i++)
                        {
                            if (Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]) < d)
                            {
                                if (ir1 != i)
                                {
                                    ir2 = i;
                                    d = Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]);
                                }
                               
                            }
                        }

                       


                        d = 100000f;

                        for (int i = 0; i < extentPoints.Length; i++)
                        {
                            if (Vector3.Distance(viewFinder.transform.position, extentPoints[i]) < d)
                            {
                                ie1 = i;
                                d = Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]);
                            }
                        }

                        d = 100000f;

                        for (int i = 0; i < extentPoints.Length; i++)
                        {
                            if (Vector3.Distance(viewFinder.transform.position, extentPoints[i]) < d)
                            {
                                if (ie1 != i)
                                {
                                    ie2 = i;
                                    d = Vector3.Distance(viewFinder.child.transform.position, rectanglePoints[i]);
                                }
                            }
                        }

                        line1.SetPosition(0, 0.5f * (rectanglePoints[ir1] + rectanglePoints[ir2]));
                        line1.SetPosition(1, 0.5f * (extentPoints[ie1] + extentPoints[ie2]));
                        line1.material.SetColor("_Color", viewFinder.color);
                    }

                    if (mode == Mode.ThreeD)
                    {
                        line1.positionCount = 4;
                        line1.loop = true;
                        line1.SetPosition(0, rectanglePoints[2]);
                        line1.SetPosition(1, rectanglePoints[3]);
                        line1.SetPosition(2, extentPoints[2]);
                        line1.SetPosition(3, extentPoints[3]);

                        line1.material.SetColor("_Color", viewFinder.color);

                        line2.positionCount = 4;
                        line2.loop = true;
                        line2.SetPosition(0, rectanglePoints[0]);
                        line2.SetPosition(1, rectanglePoints[1]);
                        line2.SetPosition(2, extentPoints[0]);
                        line2.SetPosition(3, extentPoints[1]);

                        line2.material.SetColor("_Color", viewFinder.color);
                    }


                   
                }
            }
        }
        
    }

}
