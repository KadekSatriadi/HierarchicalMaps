using Mapbox.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ViewFinder : MonoBehaviour
{
    public MapLens parent;
    public MapLens child;
    public Vector2d latLongCenter;
    public ClipShape shape;
    public float radius;
    public float width;
    public float height;
    public Color color;

    private int segments = 100;
    private VisualLinkController visualLinkController;
    private MeshLineStripRenderer line;
    private float previousChildWidth;
    private float previousChildHeight;
    private float previousChildZoom;
    private float previousParentZoom;
    private float radiusRatio;
    private float widthRatio;
    private float heightRatio;
    private float diagonal;
    private Vector3 previousPosition;
    private Vector3 w;
    private Vector3 h;
    private Vector3 d;
    private Vector3 s;
    private Vector3 offset = new Vector3(0, 0, -0.001f);
    private Vector2d parentCenter;
    private Vector2d prevChildCenter;
    private Vector2d prevParentCenter;
    private bool isRectangleDimensionAnimationPlaying = false;
    private bool isChildMapFollow = false;

    void Awake()
    {
        visualLinkController = GetComponentInChildren<VisualLinkController>();
        radius = 0f;
        line = gameObject.GetComponent<MeshLineStripRenderer>();
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.SetColor("_Color", Color.white);
        line.material = mat;
        color = Color.white;

        // HideVisualLink();
    }


    /// <summary>
    /// Add child map
    /// </summary>
    /// <param name="c"></param>
    public void AddChild(MapLens c)
    {
        child = c;
        previousChildHeight = c.clipController.height;
        previousChildWidth = c.clipController.width;
        heightRatio = height / previousChildHeight;
        widthRatio = width / previousChildWidth;
        previousChildZoom = c.abstractMap.Zoom;
    }

    /// <summary>
    /// Intiate parent, and center
    /// </summary>
    /// <param name="m"></param>
    /// <param name="latlong"></param>
    public void Initiate(MapLens m, Vector2d latlong)
    {
        parent = m;
        latLongCenter = latlong;
        prevChildCenter = latlong;
        previousParentZoom = m.abstractMap.Zoom;
        prevParentCenter = m.abstractMap.CenterLatitudeLongitude;
        UpdatePosition();
    }
    public void Initiate(MapLens m, Vector2d latlong, float h, float w)
    {
        parent = m;
        latLongCenter = latlong;
        prevChildCenter = latlong;
        previousParentZoom = m.abstractMap.Zoom;
        prevParentCenter = m.abstractMap.CenterLatitudeLongitude;
        height = h;
        width = w;
        UpdatePosition();
    }

    /// <summary>
    /// Set child map to follow the viewfinder when the parent map is panned
    /// </summary>
    public void ChildMapFollowMe()
    {
        isChildMapFollow = true;
    }

    /// <summary>
    /// Set child map to unfollow the viewfinder when the parent map is panned
    /// </summary>
    public void ChildMapUnfollowMe()
    {
        isChildMapFollow = false;
    }

    private void UpdateChildViewFinder()
    {
        if (child == null) return;
        if(child.viewFinder == null)
        {
            child.viewFinder = this;
        }
    }

    private void Update()
    {
        
        Draw();
        UpdateVisibility();
        UpdateChildViewFinder();

        if (shape == ClipShape.Rectangle)
        {
            //follow point
            if (parent != null)
            {
                if (child != null)
                {
                    Vector2d currentChildCenter = child.abstractMap.CenterLatitudeLongitude;
                    Vector2d currentParentCenter = parent.abstractMap.CenterLatitudeLongitude;
                    float currentWidth = child.clipController.width;
                    float currentHeight = child.clipController.height;
                    float currentChildZoom = child.abstractMap.Zoom;
                    float currentParentZoom = parent.abstractMap.Zoom;
                    //update radius when child is zoomed
                    if (previousChildZoom != currentChildZoom)
                    {
                        width = (float)MapFormula.ZoomToMeterInterpolation(currentChildZoom, currentParentZoom, currentWidth);
                        height = (float)MapFormula.ZoomToMeterInterpolation(currentChildZoom, currentParentZoom, currentHeight);
                        widthRatio = width / previousChildWidth;
                        heightRatio = height / previousChildHeight;
                        previousChildZoom = currentChildZoom;
                    }
                    ////update center when child is panned
                    if (prevChildCenter != null && !currentChildCenter.Equals(prevChildCenter))
                    {
                        Vector3 center = parent.abstractMap.GeoToWorldPosition(currentChildCenter);
                        latLongCenter = parent.abstractMap.WorldToGeoPosition(center);
                        prevChildCenter = currentChildCenter;
                        //Debug.Log("Children is panned");
                    }
                    ////update radius when child viewport radius is changed
                    if (previousChildWidth != currentWidth || previousChildHeight != currentHeight)
                    {
                        width = (float)MapFormula.ZoomToMeterInterpolation(currentChildZoom, currentParentZoom, currentWidth);
                        height = (float)MapFormula.ZoomToMeterInterpolation(currentChildZoom, currentParentZoom, currentHeight);
                        widthRatio = width / previousChildWidth;
                        heightRatio = height / previousChildHeight;
                        previousChildHeight = height;
                        previousChildWidth = width;
                    }
                    //update if parent is zoomed
                    if (previousParentZoom != currentParentZoom)
                    {
                        width = (float)MapFormula.ZoomToMeterInterpolation(currentChildZoom, currentParentZoom, currentWidth);
                        height = (float)MapFormula.ZoomToMeterInterpolation(currentChildZoom, currentParentZoom, currentHeight);
                        widthRatio = width / previousChildWidth;
                        heightRatio = height / previousChildHeight;
                        previousParentZoom = currentParentZoom;

                    }

                    //parent is panned
                    if (prevParentCenter != null && !prevParentCenter.Equals(currentParentCenter))
                    {
                        prevParentCenter = currentParentCenter;
                        if (isChildMapFollow) UpdateChildPosition();
                    }

                    //if child map size is smaller than viewfinder and and visible on parent, and is pinned,  rescale
                    if(IsOnMapView() && child.IsPinned())
                    {
                        child.PlayScaleByDimensionAnimate(height, width);
                    }

                    CheckEnclosedViewFinder();
                }

                UpdatePosition();
                UpdateOffScreenVisibility();

            }
            //if (child != null)
            //{
            //    visualLinkController.top = child.transform;
            //    visualLinkController.bottom = transform;
            //    visualLinkController.topRadius = child.clipController.radius;
            //    visualLinkController.bottomRadius = radius * 0.5f;
            //}
        }

        //set color 
        line.material.SetColor("_Color", color);
        if(visualLinkController) visualLinkController.color = color;
    }

    
    private void CheckEnclosedViewFinder()
    {
        List<ViewFinder> vs = parent.GetAttachedViewfinders();
        foreach(ViewFinder v in vs)
        {
            if (v == this) continue;
            if(v.height * v.width < height * width)
            {
                bool enclose = IsViewfinderAEnclosesB(this, v);
                if (enclose)
                {
                    v.parent = child;
                    v.child.parent = child;
                    v.child.level = v.parent.parent.level + 1;
                    MutiViewsArrangementManager.Instance.SetColor(v.child);
                    MapLens.SetColorOfChildren(v.child);
                }
               
            }
        }
    }

    

    /// <summary>
    /// If viewfinder a encloses viewfinder b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool IsViewfinderAEnclosesB(ViewFinder a, ViewFinder b)
    {
        Vector3 d = b.GetRectangleCenter() - a.GetRectangleCenter();
        d = new Vector3(Mathf.Abs(d.x), Mathf.Abs(d.y), Mathf.Abs(d.z));

        Vector3 hd = Vector3.Project(d, a.transform.right);
        hd = hd.normalized * (hd.magnitude + b.width * 0.5f);
        Vector3 vd = Vector3.Project(d, a.transform.forward);
        vd = vd.normalized * (vd.magnitude + b.height * 0.5f);

        Debug.DrawRay(a.GetRectangleCenter(), hd, Color.red);
        Debug.DrawRay(a.GetRectangleCenter(), vd, Color.blue);

        bool h = hd.magnitude < a.width * 0.5f;
        bool v = vd.magnitude < a.height * 0.5f;

       // Debug.Log(h && v);

        return (h && v);
    }

    /// <summary>
    /// Update position and rotation
    /// </summary>
    private void UpdatePosition()
    {
        offset = transform.up * 0.005f;
        Vector3 position = parent.abstractMap.GeoToWorldPosition(latLongCenter) + offset;
        transform.position = position;
        transform.rotation = parent.transform.rotation;
        Vector3 direction = position - previousPosition;
        previousPosition = position;  
    }

    private void UpdateOffScreenVisibility()
    {
        if (!IsOnMapView())
        {
            if(parent.parent != null)
            {
                parent = parent.parent;
                child.parent = parent;
                child.level = parent.level + 1;
                MutiViewsArrangementManager.Instance.SetColor(child);
            }
            else
            {
                line.SetActive(false);
            }
        }
        else
        {
            line.SetActive(true);
        }
    }


    /// <summary>
    /// Is Viewfinder visible on map
    /// </summary>
    /// <returns></returns>
    private bool IsOnMapView()
    {
        Vector3 d = GetRectangleCenter() - parent.transform.position;
        d = new Vector3(Mathf.Abs(d.x), Mathf.Abs(d.y), Mathf.Abs(d.z));

        Vector3 hd = Vector3.Project(d, parent.transform.right);
        hd = hd.normalized * (hd.magnitude + width * 0.5f);
        Vector3 vd = Vector3.Project(d, parent.transform.forward);
        vd = vd.normalized * (vd.magnitude + height * 0.5f);

        Debug.DrawRay(parent.transform.position, hd, Color.red);
        Debug.DrawRay(parent.transform.position, vd, Color.blue);

        bool h = hd.magnitude <= parent.clipController.width * 0.5f;
        bool v = vd.magnitude <= parent.clipController.height * 0.5f;

       // Debug.Log(h && v);

        return (h && v);
    }

    /// <summary>
    /// Update position of child maps when parent maps is panning
    /// </summary>
    private void UpdateChildPosition()
    {
        Vector3 position = parent.abstractMap.GeoToWorldPosition(latLongCenter) + offset;
        Vector3 direction = position - previousPosition;
        if (child != null && IsOnMapView()) child.transform.position = position;
    }

    /// <summary>
    /// Hide visual linking
    /// </summary>
    public void HideVisualLink()
    {
        //disable visual link line
        VisualLinkLine vline = GetComponentInChildren<VisualLinkLine>();
        if (vline != null)
        {
            vline.Hide();
        }
        if (visualLinkController) visualLinkController.gameObject.SetActive(false);
        
    }

    /// <summary>
    /// Change dimension in animated way
    /// </summary>
    /// <param name="newH"></param>
    /// <param name="newW"></param>
    public void PlayAnimateRectangleDimension(float newH, float newW)
    {
        if (!isRectangleDimensionAnimationPlaying)
        {
            isRectangleDimensionAnimationPlaying = true;
            StartCoroutine(RectangleDimensionTween(height, width, newH, newW));
        }
    }


    /// <summary>
    /// Changing dimension in animated way
    /// </summary>
    /// <param name="h"></param>
    /// <param name="w"></param>
    /// <param name="newH"></param>
    /// <param name="newW"></param>
    /// <returns></returns>
    IEnumerator RectangleDimensionTween(float h, float w, float newH, float newW)
    {
        float duration = 0.95f;
        float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);
            float curvePercent =  parent.curve.Evaluate(percent);
            float currentH = Mathf.LerpUnclamped(h, newH, curvePercent);
            float currentW = Mathf.LerpUnclamped(w, newW, curvePercent);
            height = currentH;
            width = currentW;
            yield return null;
        }

        isRectangleDimensionAnimationPlaying = false;
    }

    /// <summary>
    /// Show visual link
    /// </summary>
    public void ShowVisualLink()
    {
        if (visualLinkController) visualLinkController.gameObject.SetActive(true);
        VisualLinkLine vline = GetComponentInChildren<VisualLinkLine>();
        if (vline != null)
        {
            vline.Show();
        }
    }

    /// <summary>
    /// Set radius if it is circle
    /// </summary>
    /// <param name="r"></param>
    public void SetRadius(float r)
    {
        radius = r;
        //line.radius = r;
    }

     /// <summary>
     /// Set the size
     /// </summary>
     /// <param name="w"></param>
     /// <param name="h"></param>
    public void SetSize(float w, float h)
    {
        height = h;
        width = w;
    }

    private void SetRectangleSize()
    {
        width = w.magnitude;
        height = h.magnitude;
        diagonal = d.magnitude;
    }

    /// <summary>
    /// Get the topR, topR, botR, botL points of the rectangle
    /// </summary>
    /// <returns></returns>
    public Vector3[] GetRectanglePoints()
    {
        float halfH = 0.5f * height;
        float halfW = 0.5f * width;

        Vector3 tL = transform.position + (transform.forward * halfH) - (transform.right * halfW);
        Vector3 tR = transform.position + (transform.forward * halfH) + (transform.right * halfW);
        Vector3 bR = transform.position - (transform.forward * halfH) + (transform.right * halfW);
        Vector3 bL = transform.position - (transform.forward * halfH) - (transform.right * halfW);

        Vector3[] r = new Vector3[4];
        r[0] = tL;
        r[1] = tR;
        r[2] = bR;
        r[3] = bL;

        return r;
    }

    /// <summary>
    /// Get the topR, topR, botR, botL points of the rectangle
    /// </summary>
    /// <returns></returns>
    private Vector3[] GetRectangleLinePoints()
    {
        return line.points;
    }

    /// <summary>
    /// Hide viewfinder and visual link
    /// </summary>
    public void Hide()
    {
        HideVisualLink();
        line.SetActive(false);
    }


    /// <summary>
    /// Show the viewfinder and visual link
    /// </summary>
    public void Show()
    {
        ShowVisualLink();
        line.SetActive(true);
    }

    public void Show(bool visualLink)
    {
        if(visualLink) ShowVisualLink();
        line.SetActive(true);
    }


    /// <summary>
    /// The center of the object is not the center of the rectangle :( use this to get the center of the rectangle
    /// </summary>
    /// <returns></returns>
    public Vector3 GetRectangleCenter()
    {
        return transform.position; 
    }


    /// <summary>
    /// Draw circle or rectangle according to radius height and width
    /// </summary>
    public void Draw()
    {
        if(shape == ClipShape.Circle)
        {
            line.SetPointsCount(segments + 1);
            //line.radius = radius;
            //line.CreateCirclePoints();
           // line.CreateMesh();
        }
        if(shape == ClipShape.Rectangle)
        {
            line.SetPointsCount(5);
            float halfH = 0.5f * height;
            float halfW = 0.5f * width;

            Vector3 tL = transform.position + (transform.forward * halfH) - (transform.right * halfW);
            Vector3 tR = transform.position + (transform.forward * halfH) + (transform.right * halfW);
            Vector3 bR = transform.position - (transform.forward * halfH) + (transform.right * halfW);
            Vector3 bL = transform.position - (transform.forward * halfH) - (transform.right * halfW);

            line.SetPosition(0, tL);
            line.SetPosition(1, tR);
            line.SetPosition(2, bR);
            line.SetPosition(3, bL);
            line.SetPosition(4, tL);  
        }
    }

    protected void UpdateVisibility()
    {
        if(parent == null)
        {
            Hide();
            return;
        }

        if (height * width > parent.clipController.height * parent.clipController.width)
        {
            Hide();
        }
        else
        {
            Show(false);
        }
    }

}
