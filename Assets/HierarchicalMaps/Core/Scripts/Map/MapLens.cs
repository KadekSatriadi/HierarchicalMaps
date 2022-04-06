using Mapbox.Unity.Map;
using Mapbox.Unity.Map.TileProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mapbox.Utils;

public class MapLens : MonoBehaviour
{
    public enum Dimension
    {
        Map2D, Map3D
    }

    [Header("Basic settings")]
    public AbstractMap abstractMap;
    public bool isInitialisedOnStart;
    public ClipController clipController;
    public Transform center;

    [Header("Initial style")]
    public string latLong;
    public float zoom;
    public string styleURL;

    [Header("Elevation")]
    public Dimension dimension;
    [Range(0.1f, 10f)]
    public float exaggerationFactor = 1f;
    [Range(2, 255)]
    public int terrainResolution = 20;

    [Header("Multiview")]
    public MapLens parent;
    public int level;
    public ViewFinder viewFinder;

    [Header("Animation")]
    public AnimationCurve curve;

    public event ZoomEventHandler MapZoomed = delegate { };
    public delegate void ZoomEventHandler(Vector2d zoomCenter);

    #region EVENTS
    public System.Action OnFocus = delegate { };
    public System.Action OnOutFocus = delegate { };
    public System.Action OnRadiusGrow = delegate { };
    public System.Action OnReady = delegate { };
    public System.Action OnMoveAnimationEnd = delegate { };
    public System.Action OnRotationAnimationEnd = delegate { };
    public System.Action OnScaleAnimationEnd = delegate { };
    public System.Action OnHeightAnimationEnd = delegate { };
    public System.Action OnRadiusAnimationEnd = delegate { };
    public System.Action OnViewWidthAnimationEnd = delegate { };
    public System.Action OnViewHeighAnimationEnd = delegate { };
    public System.Action OnBeforeDelete = delegate { };
    public System.Action OnRemove = delegate { };
    public System.Action OnZoomed = delegate { };
    public System.Action OnPanned = delegate { };
    public System.Action OnZoomEnd = delegate { };
    public System.Action OnPanEnd = delegate { };
    public System.Action OnResize = delegate { };
    #endregion



    private float prevWidth;
    private float prevHeight;
    private float maxLensRadius = float.NegativeInfinity;
    private float previousRadius;
    private float initialBorderWidth;
    private float animationDuration = 0.5f;
    private float animationDurationForRotation = 0.5f;
    private float animationDurationForZoom = 0.35f;
    private float prevZoom;
    private string prevLatLong;

    private bool isMoveAnimationPlaying = false;
    private bool isScaleAnimationPlaying = false;
    private bool isRotationAnimationPlaying = false;
    private bool isHeightAnimationPlaying = false;
    private bool isRadiusAnimationPlaying = false;
    private bool isMapLensReady = false;
    private bool isViewWidthAnimationPlaying = false;
    private bool isViewHeightAnimationPlaying = false;
    private bool isFocusAnimating = false;
    private bool isFocused = false;
    private bool isHorizontal = false;
    private bool isPinned = false;
    private bool isZoomEndTriggered = false;
    private bool isPanEndTriggered = false;

    private Vector2 previousRectSize;

    #region MONOBEHAVIOURS
    private void Awake()
    {

        if(abstractMap == null) abstractMap = GetComponentInChildren<AbstractMap>();
        if(clipController == null) clipController = GetComponentInChildren<ClipController>();

        previousRectSize = new Vector2(0, 0);       

        this.OnRadiusGrow += UpdateMapExtent;

        if (isInitialisedOnStart)
        {
            Initiate();
            OnReady += delegate
            {
                abstractMap.SetZoom(zoom);
            };
        }

        Hide();
        clipController.OnClipReady += delegate
        {
            Show();
        };
    }

    private void Update()
    {
        if (!new Vector2(clipController.width, clipController.height).Equals(previousRectSize))
        {
            OnRadiusGrow.Invoke();
            previousRectSize = new Vector2(clipController.width, clipController.height);
        }
        
        if(!isMapLensReady && abstractMap.MapVisualizer != null)
        {
            isMapLensReady = true;
            OnReady.Invoke();
        }

        if (isMapLensReady)
        {
            if(prevZoom != abstractMap.Zoom)
            {
                OnZoomed.Invoke();
                prevZoom = abstractMap.Zoom;
                isZoomEndTriggered = false;
            }
            else if (!isZoomEndTriggered)
            {
                OnZoomEnd.Invoke();
                isZoomEndTriggered = true;
            }

            if(prevWidth != clipController.width)
            {
                OnResize.Invoke();
                prevWidth = clipController.width;
            }
            else if(prevHeight != clipController.height)
            {
                OnResize.Invoke();
                prevHeight = clipController.height;
            }


            if(prevLatLong != abstractMap.CenterLatitudeLongitude.ToString())
            {
                OnPanned.Invoke();
                prevLatLong = abstractMap.CenterLatitudeLongitude.ToString();
                isPanEndTriggered = false;
            }
            else if (!isPanEndTriggered)
            {
                OnPanEnd.Invoke();
                isPanEndTriggered = true;
            }
            //update level
            if (parent != null) level = parent.level + 1;
        }
    }

    public void OnDestroy()
    {
        Remove();
    }
    #endregion

    protected void ChildrenToParent()
    {
        foreach (MapLens m in GetChildren())
        {
            m.parent = parent;
            m.viewFinder.parent = parent;
            if (m.parent != null) m.level = m.parent.level + 1;
            MapLens.SetColorOfChildren(m);
            FindObjectOfType<MutiViewsArrangementManager>().SetColor(m);
        }

    }

    /// <summary>
    /// Map tiles intiation callback
    /// </summary>
    private void MapTilesFinishedLoading(ModuleState state)
    {
        if (state == ModuleState.Finished) UpdateClipControllerMaterials();
    }

    /// <summary>
    /// Panning
    /// </summary>
    /// <param name="wordDirection">Vector direction in world position</param>
    public void Pan(Vector3 wordDirection)
    {
        // float factor = panSpeed * (Conversions.GetTileScaleInDegrees((float) map.CenterLatitudeLongitude.x, map.AbsoluteZoom));
        //  var latitudeLongitude = new Vector2d(map.CenterLatitudeLongitude.x + y * factor * 2.0f, map.CenterLatitudeLongitude.y + x * factor * 4.0f);
        Vector3 centerWorld = abstractMap.GeoToWorldPosition(abstractMap.CenterLatitudeLongitude);
        Vector3 newCenter = centerWorld + wordDirection;
        var latitudeLongitude = abstractMap.WorldToGeoPosition(newCenter);

        abstractMap.UpdateMap(latitudeLongitude);
    }

    /*
     * <summary>
     * Zoom to point on Map instead of zooming to center
     * </summary>
     * */
    public void Zoom(Vector3 target, float value, float zoomSpeed)
    {
        Vector3 mapCenter = abstractMap.GeoToWorldPosition(abstractMap.CenterLatitudeLongitude);

        target -= mapCenter;
        var zoom = Mathf.Max(0.0f, Mathf.Min(abstractMap.Zoom + value * zoomSpeed, 21.0f));
        var change = zoom - abstractMap.Zoom;

        //0.7f is a constant
        var offsetX = target.x * change * 0.7f;
        var offsetY = target.z * change * 0.7f;

        Vector3 newCenterUnity = new Vector3(offsetX, 0, offsetY) + mapCenter;
        Vector2d newCenter = abstractMap.WorldToGeoPosition(newCenterUnity);

        abstractMap.UpdateMap(newCenter, zoom);
        MapZoomed(abstractMap.WorldToGeoPosition(newCenterUnity));
    }

    public void Zoom(Vector2d target, float value, float zoomSpeed)
    {
        Vector3 targetUnity = abstractMap.GeoToWorldPosition(target);
        Zoom(targetUnity, value, zoomSpeed);
    }

    public void Zoom(float value, float zoomSpeed)
    {
        var zoom = Mathf.Max(0.0f, Mathf.Min(abstractMap.Zoom + value * zoomSpeed, 21.0f));
        abstractMap.UpdateMap(abstractMap.CenterLatitudeLongitude, zoom);
        MapZoomed(abstractMap.CenterLatitudeLongitude);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsMapReady()
    {
        return isMapLensReady;
    }

    /// <summary>
    /// Is map currently zooming
    /// </summary>
    public bool IsZooming()
    {
        return !isZoomEndTriggered;
    }

    /// <summary>
    /// Check whether map is animating
    /// </summary>
    /// <returns></returns>
    public bool IsAnimating()
    {
        return isScaleAnimationPlaying || isMoveAnimationPlaying;
    }

    /// <summary>
    /// Zoom map
    /// </summary>
    /// <param name="z"></param>
    
    public void Zoom(float z)
    {
        abstractMap.UpdateMap(z);
    }

    public void ZoomIn()
    {
       if(abstractMap.AbsoluteZoom < 22)  PlayZoomAnimation(abstractMap.Zoom + 1f);
    }

    public void ZoomOut()
    {
        if (abstractMap.AbsoluteZoom > 0) PlayZoomAnimation(abstractMap.Zoom - 1f);
    }

    public void Zoom(float z, Action a)
    {
        abstractMap.OnUpdated += delegate
        {
            a.Invoke();
            abstractMap.OnUpdated -= a;
        };
        abstractMap.UpdateMap(z);
    }

    /// <summary>
    /// Update map extend
    /// </summary>
    public void UpdateMapExtent()
    {
        UpdateRectangleTiles();
        clipController.SetMaterials(GetMapMaterials());
    }

    public ViewFinder GetHitViewFinder(Vector3 destinationPosition, MapLens mapLens)
    {
        return null;
    }

    /// <summary>
    /// Hide map but not the viewfinder
    /// </summary>
    public void HideMapLens()
    {
        foreach(Renderer t in GetComponentsInChildren<Renderer>(true))
        {
            t.enabled = false;
        }
        foreach (Collider c in GetComponentsInChildren<Collider>(true))
        {
            c.enabled = false;
        }
        if (viewFinder) viewFinder.HideVisualLink();
    }

    /// <summary>
    /// Hide map including viewfinder
    /// </summary>
    public void Hide()
    {
        HideMapLens();        
        if(viewFinder) viewFinder.Hide();
    }

    /// <summary>
    /// Showmap
    /// </summary>
    public void Show()
    {
        foreach (Renderer t in GetComponentsInChildren<Renderer>(true))
        {
            t.enabled = true;
        }
        foreach (Collider c in GetComponentsInChildren<Collider>(true))
        {
            c.enabled = true;
        }
        if (viewFinder) viewFinder.Show();
    }

    /// <summary>
    /// Map initialisation
    /// </summary>
   public void Initiate()
    {
        abstractMap.Options.locationOptions.latitudeLongitude = latLong;
        abstractMap.Options.locationOptions.zoom = zoom;

        prevZoom = zoom;
        prevLatLong = abstractMap.CenterLatitudeLongitude.ToString();

        if (styleURL != null && styleURL.Trim().Length > 0)
        {
            abstractMap.ImageLayer.SetLayerSource(ImagerySourceType.Custom);
            abstractMap.ImageLayer.SetLayerSource(styleURL);
        }

        //if (clipController.shape == ClipShape.Circle)
        //{
        //    float r = clipController.radius;
        //    float unitySize = abstractMap.Options.scalingOptions.unityTileSize;
        //    float remain = (r >= unitySize) ? r - unitySize : 0;
        //    int tiles = (int)Mathf.Round(remain / unitySize) + 1;

        //    RangeTileProviderOptions extentOptions = (RangeTileProviderOptions)abstractMap.Options.extentOptions.GetTileProviderOptions();
        //    extentOptions.east = tiles;
        //    extentOptions.north = tiles;
        //    extentOptions.west = tiles;
        //    extentOptions.south = tiles;

        //    abstractMap.SetExtentOptions(extentOptions);

        //    abstractMap.Initialize(Mapbox.Unity.Utilities.Conversions.StringToLatLon(abstractMap.Options.locationOptions.latitudeLongitude), (int)abstractMap.Options.locationOptions.zoom);
        //    abstractMap.MapVisualizer.OnMapVisualizerStateChanged += MapTilesFinishedLoading;

        //    if (r < unitySize) clipController.SetMaterials(GetMapMaterials());

        //    //border
        //    initialBorderWidth = clipController.border;
        //}

        abstractMap.Initialize(Mapbox.Unity.Utilities.Conversions.StringToLatLon(abstractMap.Options.locationOptions.latitudeLongitude), (int)abstractMap.Options.locationOptions.zoom);
        UpdateRectangleTiles();

        if (abstractMap.MapVisualizer != null) abstractMap.MapVisualizer.OnMapVisualizerStateChanged += MapTilesFinishedLoading;

        //border
        initialBorderWidth = clipController.border;

        if (dimension == Dimension.Map3D)
        {
            Set3D();
        }
        else
        {
            Set2D();
        }

        prevHeight = clipController.height;
        prevWidth = clipController.width;
    }

   public  void UpdateRectangleTiles()
    {
        float unitySize = abstractMap.Options.scalingOptions.unityTileSize;

        int nW = (int)((clipController.width / unitySize) * 0.5f) + 1;
        int nH = (int)((clipController.height / unitySize) * 0.5f) + 1;

        RangeTileProviderOptions extentOptions = (RangeTileProviderOptions)abstractMap.Options.extentOptions.GetTileProviderOptions();
        extentOptions.east = nW;
        extentOptions.north = nH;
        extentOptions.west = nW;
        extentOptions.south = nH;

        abstractMap.SetExtentOptions(extentOptions);
    }

    /// <summary>
    /// Update materials controlled by clipcontroller
    /// </summary>
    public void UpdateClipControllerMaterials()
    {
        clipController.SetMaterials(GetMapMaterials());
    }

    /// <summary>
    /// Override lens max radius to improve performance
    /// </summary>
    /// <param name="r"></param>
    public void SetMaxLensRadius(float r)
    {
        maxLensRadius = r;
    }

    /// <summary>
    /// Update map radius or rectangle dimension based on given zoom level
    /// </summary>
    /// <param name="z2">New Zoom</param>
    public void ScaleByZoom(float z2)
    {
        //if(clipController.shape == ClipShape.Circle)
        //{
        //    float z1 = abstractMap.Zoom;
        //    float r1 = clipController.radius;
        //    float r2 = (float)MapFormula.ZoomToMeterInterpolation(z1, z2, r1);
        //    //override r2
        //    if (maxLensRadius != float.NegativeInfinity && r2 > maxLensRadius)
        //    {
        //        r2 = maxLensRadius;
        //    }
        //    clipController.radius = r2;
        //}

        float z1 = abstractMap.Zoom;
        float h1 = clipController.height;
        float w1 = clipController.width;
        float h2 = (float)MapFormula.ZoomToMeterInterpolation(z1, z2, h1);
        float w2 = (float)MapFormula.ZoomToMeterInterpolation(z1, z2, w1);
        clipController.height = h2;
        clipController.width = w2;
        abstractMap.UpdateMap(z2);
    }


    /// <summary>
    /// Get the Zoom level by new dimension if extent is preseved
    /// </summary>
    /// <param name="current">radius or height</param>
    /// <returns></returns>
    public float GetZoomByNewDimension(float current, float prev, ClipShape clipShape)
    {
        if(clipShape == ClipShape.Circle)
        {
            return (float)MapFormula.MeterToZoomInterpolation(current, prev, abstractMap.Zoom);
        }
        else
        {
            return (float)MapFormula.MeterToZoomInterpolation(current, prev, abstractMap.Zoom);
        }
    }

    /// <summary>
    /// Scale and zoom based on the given new over old dimension ratio 
    /// </summary>
    /// <param name="ratio">new dimension scale factor, 2 will double the size</param>
    public void ScaleByDimension(float newRadius, float newHeight, float newWidth)
    {
        float newZoom = abstractMap.Zoom;
        //if(clipController.shape == ClipShape.Circle)
        //{
        //    float r1 = clipController.radius;
        //    newZoom = (float)MapFormula.MeterToZoomInterpolation(newRadius, r1, abstractMap.Zoom);
        //    clipController.radius = newRadius;
        //}

        float h1 = clipController.height;
        newZoom = (float)MapFormula.MeterToZoomInterpolation(newHeight, h1, abstractMap.Zoom);
        clipController.height = newHeight;
        clipController.width = newWidth;

        abstractMap.UpdateMap(newZoom);
    }

    /// <summary>
    /// Get new height and new width by new zoom level
    /// </summary>
    /// <param name="z2"></param>
    /// <returns></returns>
    public Vector2 GetHeightWidthByZoom(float z2)
    {
        float z1 = abstractMap.Zoom;
        float h1 = clipController.height;
        float w1 = clipController.width;
        float h2 = (float)MapFormula.ZoomToMeterInterpolation(z1, z2, h1);
        float w2 = (float)MapFormula.ZoomToMeterInterpolation(z1, z2, w1);

        return new Vector2(h2, w2);
    }

    /// <summary>
    /// Set zoom based on the new r or h, to change zoom and dimension use ScaleByDimension
    /// </summary>
    /// <param name="newRadius"></param>
    /// <param name="newHeight"></param>
    public void ZoomByDimension(float newRadius, float newHeight)
    {
        float newZoom = abstractMap.Zoom;
        //if (clipController.shape == ClipShape.Circle)
        //{
        //    float r1 = clipController.radius;
        //    newZoom = (float)MapFormula.MeterToZoomInterpolation(newRadius, r1, abstractMap.Zoom);
        //}

        float h1 = clipController.height;
        newZoom = (float)MapFormula.MeterToZoomInterpolation(newHeight, h1, abstractMap.Zoom);
        abstractMap.UpdateMap(newZoom);
    }

    #region ANIMATIONS
    /// <summary>
    /// Remove map with animation
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="z"></param>
    public void RemoveWithAnimation(Vector3 destination, float r)
    {
        //if (clipController.shape == ClipShape.Circle)
        //{
        //    PlayRadiusAnimate(r);
        //    OnRadiusAnimationEnd += RemoveWaitAnimation;
        //    PlayAnimationMovement(destination, RemoveWaitAnimation);

        //}
        PlayHeightWidthAnimate(viewFinder.height, viewFinder.width);
        PlayAnimationMovement(viewFinder.GetRectangleCenter(), RemoveWaitAnimation);
    }
    /// <summary>
    /// Play the scale animation by new zoom level
    /// </summary>
    /// <param name="newZoom"></param>
    public void PlayScaleByZoomAnimate(float newZoom)
    {
        if (!isScaleAnimationPlaying)
        {
            isScaleAnimationPlaying = true;
            StartCoroutine(ScaleByZoomTween(abstractMap.Zoom, newZoom, OnScaleAnimationEnd));
        }
    }
    public void PlayScaleByZoomAnimate(float newZoom, Action a)
    {
        if (!isScaleAnimationPlaying)
        {
            isScaleAnimationPlaying = true;
            StartCoroutine(ScaleByZoomTween(abstractMap.Zoom, newZoom, a));
        }
    }

    /// <summary>
    /// Play the scale animation by new dimension (keep extent)
    /// </summary>
    /// <param name="ratio"></param>
    public void PlayScaleByDimensionAnimate(float newH, float newW)
    {
        if (!isScaleAnimationPlaying)
        {
            isScaleAnimationPlaying = true;
            StartCoroutine(ScaleByDimensionTween(newH, newW));
        }
    }
    public void PlayScaleByDimensionAnimate(float newH, float newW, Action t)
    {
        if (!isScaleAnimationPlaying)
        {
            isScaleAnimationPlaying = true;
            StartCoroutine(ScaleByDimensionTween(newH, newW, t));
        }
    }

    /// <summary>
    /// Aspect ratio should be constant!
    /// </summary>
    /// <param name="newR"></param>
    /// <param name="newH"></param>
    public void PlayZoomByDimension(float newH)
    {
        if (!isScaleAnimationPlaying)
        {
            isScaleAnimationPlaying = true;
            StartCoroutine(ZoomByDimensionTween(newH));
        }
    }
   

    /// <summary>
    /// Animate the view size
    /// </summary>
    /// <param name="h"></param>
    /// <param name="w"></param>
    public void PlayHeightWidthAnimate(float h, float w)
    {
        PlayHeightWidthAnimate(h, w, OnViewWidthAnimationEnd);
    }
    public void PlayHeightWidthAnimate(float h, float w, Action action)
    {
        if (!isViewWidthAnimationPlaying)
        {
            isViewWidthAnimationPlaying = true;
            StartCoroutine(ViewWidthTween(clipController.width, w, action));
        }
        if (!isViewHeightAnimationPlaying)
        {
            isViewHeightAnimationPlaying = true;
            StartCoroutine(ViewHeightTween(clipController.height, h));
        }
    }

    /// <summary>
    /// Animate rotation
    /// </summary>
    /// <param name="s"></param>
    /// <param name="e"></param>
    public void PlayAnimationRotation(Quaternion s, Quaternion e)
    {
        StartCoroutine(RotationTween(s, e));
    }
    public void PlayAnimationRotation(Quaternion s, Quaternion e, Action a)
    {
        StartCoroutine(RotationTween(s, e, a));
    }


    /// <summary>
    /// Animate movement of the map
    /// </summary>
    /// <param name="s">start position</param>
    /// <param name="e">end position</param>
    /// <param name="m">map gameobject</param>
    public void PlayAnimationMovement(Vector3 s, Vector3 e)
    {
        if (!isMoveAnimationPlaying)
        {
            isMoveAnimationPlaying = true;
            StartCoroutine(MovementTween(s, e, OnMoveAnimationEnd));
        }
    }
    public void PlayAnimationMovement(Vector3 e)
    {
        PlayAnimationMovement(transform.position, e);
    }
    public void PlayAnimationMovement(Vector3 e, Action action)
    {
        if (!isMoveAnimationPlaying)
        {
            isMoveAnimationPlaying = true;
            StartCoroutine(MovementTween(transform.position, e, action));
        }
    }
    public void StopMovementAnimation()
    {
        StopCoroutine("MovementTween");
        isMoveAnimationPlaying = false;
    }
    
    /// <summary>
    /// Change the zoom of the map in animated way huh
    /// </summary>
    /// <param name="newZoom"></param>
    public void PlayZoomAnimation(float newZoom)
    {
        if (!isScaleAnimationPlaying)
        {
            isScaleAnimationPlaying = true;
            StartCoroutine(ZoomTween(abstractMap.Zoom, newZoom, OnScaleAnimationEnd));
        }
    }
    public void PlayZoomAnimation(float newZoom, Action action)
    {
        if (!isScaleAnimationPlaying)
        {
            isScaleAnimationPlaying = true;
            StartCoroutine(ZoomTween(abstractMap.Zoom, newZoom, action));
        }
    }

    /// <summary>
    /// Play animation to change terrain exaggeration
    /// </summary>
    /// <param name="s"></param>
    /// <param name="t"></param>
    public void PlayHeightAnimate(float start, float end)
    {
        if (!isHeightAnimationPlaying)
        {
            isHeightAnimationPlaying = true;
            StartCoroutine(HeightTween(start, end));
        }
    }

    #endregion
   
    /// <summary>
    /// Disable or enable collider
    /// </summary>
    /// <param name="status"></param>
    public void SetColliderStatus(bool status)
    {
        abstractMap.Terrain.EnableCollider(status);
        abstractMap.UpdateMap();
    }

    /// <summary>
    /// Set map to 3D
    /// </summary>
    public void Set3D()
    {
        abstractMap.Terrain.ElevationType = ElevationLayerType.TerrainWithElevation;
        abstractMap.Terrain.SetExaggerationFactor(exaggerationFactor);
        abstractMap.Terrain.EnableCollider(true);
        abstractMap.TerrainLayer.LayerProperty.modificationOptions.sampleCount = terrainResolution;
        abstractMap.UpdateMap();
    }

    /// <summary>
    /// Is map 3D
    /// </summary>
    /// <returns></returns>
    public bool Is3DMap()
    {
        return (abstractMap.Terrain.ElevationType == ElevationLayerType.TerrainWithElevation);
    }

    /// <summary>
    /// Show buildings
    /// </summary>
    public void ShowBuildings()
    {
        abstractMap.VectorData.GetFeatureSubLayerAtIndex(0).SetActive(true);
        abstractMap.VectorData.DisableVectorFeatureProcessingWithCoroutines();      
        abstractMap.UpdateMap();
        UpdateClipControllerMaterials();

    }

    /// <summary>
    /// Hide buildings
    /// </summary>
    public void HideBuildings()
    {
        abstractMap.VectorData.GetFeatureSubLayerAtIndex(0).SetActive(false);

        abstractMap.OnUpdated += delegate
        {
            UpdateClipControllerMaterials();
        };
        abstractMap.UpdateMap();
    }

    /// <summary>
    /// Set map to flat
    /// </summary>
    public void Set2D()
    {
        abstractMap.Terrain.ElevationType = ElevationLayerType.FlatTerrain;
        abstractMap.Terrain.EnableCollider(false);
        abstractMap.UpdateMap();
    }

    /// <summary>
    /// Animate rotation of the map
    /// </summary>
    /// <param name="rotation"></param>
    public void Rotate()
    {
        //if (!isRotationAnimationPlaying)
        //{
        //    isRotationAnimationPlaying = true;
        //    StartCoroutine(RotationTween(s, e));
        //}
        Vector3 rot = transform.localRotation.eulerAngles;
        if(rot.x >= 0)
        {
            transform.rotation =  Quaternion.Euler(new Vector3(0, 0, 0));
        }else 
        {
            transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
        }
    }


    /// <summary>
    /// Set colour of children
    /// </summary>
    /// <param name="map"></param>
    public static void SetColorOfChildren(MapLens map)
    {
        List<MapLens> children = map.GetChildren();
        foreach (MapLens m in children)
        {
            FindObjectOfType<MutiViewsArrangementManager>().SetColor(m);
            SetColorOfChildren(m);
        }
    }

    public void Remove()
    {
        OnBeforeDelete.Invoke();
        if (viewFinder) Destroy(viewFinder.gameObject);
        ChildrenToParent();
        Destroy(gameObject);
    }

    /// <summary>
    /// Destroy map and viewfinder when both animations stop
    /// </summary>
    private void RemoveWaitAnimation()
    {
        if(!isMoveAnimationPlaying && !isScaleAnimationPlaying)
        {
            OnBeforeDelete.Invoke();
            Destroy(this.gameObject);
            Destroy(viewFinder.gameObject);
            Debug.Log("RemoveWaitAnimation");
        }
    }

    /// <summary>
    /// Set rectangle dimension
    /// </summary>
    public void SetRectangleDimension(float h, float w)
    {
        clipController.height = h;
        clipController.width = w;
    }
   
    /// <summary>
    /// Set color of the border and viewfinder, the shader var name is _BaseColor
    /// </summary>
    /// <param name="c"></param>
    public void SetColor(Color c)
    {
        clipController.SetColor(c);
        if(viewFinder != null) viewFinder.color = c;
    }

    /// <summary>
    /// Get color (the shader var is _BaseColor
    /// </summary>
    /// <returns></returns>
    public Color GetColor()
    {
        return clipController.baseMap.GetComponent<Renderer>().material.GetColor("_Color");
    }

    /// <summary>
    /// Get all tile materials
    /// </summary>
    /// <returns></returns>
    public List<Material> GetMapMaterials()
    {
        List<Material> mapMaterials = new List<Material>();

        //tiles
        MeshRenderer[] c = abstractMap.GetComponentsInChildren<MeshRenderer>(true);
        foreach(MeshRenderer r in c)
        {
            mapMaterials.AddRange(r.materials);
            foreach(MeshRenderer mr in r.GetComponentsInChildren<MeshRenderer>(true))
            {
                mapMaterials.AddRange(mr.materials);
            }
        }

        //buildings
         foreach(Transform t in abstractMap.transform.GetComponentsInChildren<Transform>(true))
        {
            if(t.name == "building")
            {
                mapMaterials.AddRange(t.GetComponent<MeshRenderer>().materials);
            }
        }

        return mapMaterials.ToList();
    }

    /// <summary>
    /// Return all direct children of this map
    /// </summary>
    /// <returns></returns>
    public List<MapLens> GetChildren()
    {
        List<MapLens> list = new List<MapLens>();
        foreach(MapLens m in FindObjectsOfType<MapLens>())
        {
            if (m.parent == this) list.Add(m);
        }

        return list;
    }

    /// <summary>
    /// Get all predecessors
    /// </summary>
    /// <returns></returns>
    public List<MapLens> GetPredecessesors()
    {
        List<MapLens> list = new List<MapLens>();
        if(GetChildren().Count > 0)
        {
            foreach(MapLens m in GetChildren())
            {
                list.AddRange(m.GetPredecessesors());
            }
        }

        return list;
    }

    /// <summary>
    /// Return attached viewfinders
    /// </summary>
    /// <returns></returns>
    public List<ViewFinder> GetAttachedViewfinders()
    {
        List<ViewFinder> list = new List<ViewFinder>();
        foreach(MapLens m in GetChildren())
        {
            list.Add(m.viewFinder);
        }
        return list;
    }

    public void Focus()
    {
        //if (!isFocusAnimating && !isFocused)
        //{
        //   isFocused = true;
        //   isFocusAnimating = true;
        //   StartCoroutine(FocusTween(initialBorderWidth, initialBorderWidth * 5f));
        //}
        clipController.border = initialBorderWidth * 1.5f;
        OnFocus.Invoke();
        isFocused = false; 
    }

    public void OutFocus()
    {
        clipController.border = initialBorderWidth;
        OnOutFocus.Invoke();
        isFocused = false;
    }

    public void SetAnimationDuration(float a)
    {
        animationDuration = a;
    }

    public void SetAnimationZoomDuration(float a)
    {
        animationDurationForZoom = a;
    }

    /// <summary>
    /// Set pin status, it does not pin the map, just set the status
    /// </summary>
    /// <param name="pin"></param>
    public void SetPin(bool pin)
    {
        isPinned = pin;
    }

    /// <summary>
    /// Get pinned status
    /// </summary>
    /// <returns></returns>
    public bool IsPinned()
    {
        return isPinned;
    }
    /// <summary>
    /// Look at
    /// </summary>
    /// <param name="position">target position</param>
    public void LookAt(Vector3 position)
    {
        transform.rotation = GetLookAtRotation(position, transform.position);
    }
    /// <summary>
    /// Look at with animation
    /// </summary>
    /// <param name="position">target position</param>
    public void LookAtAnimate(Vector3 position)
    {
        PlayAnimationRotation(transform.rotation, GetLookAtRotation(position, transform.position));
    }

    /// <summary>
    /// Look at with animation
    /// </summary>
    /// <param name="position">target position</param>
    /// <param name="a">post rotation action</param>
    public void LookAtAnimate(Vector3 position, Action a)
    {
        PlayAnimationRotation(transform.rotation, GetLookAtRotation(position, transform.position), a);
    }

    /// <summary>
    /// Get rotation 
    /// </summary>
    /// <param name="target">target position</param>
    /// <param name="position">map (?) position</param>
    /// <returns></returns>
    public Quaternion GetLookAtRotation(Vector3 target, Vector3 position)
    {
        Vector3 direction = target - position;


        Quaternion look = Quaternion.LookRotation(direction, Vector3.up);
        Quaternion rotAdjustment = Quaternion.AngleAxis(180f, Vector3.up);
        Quaternion rotAdjustment2 = Quaternion.AngleAxis(-90f, Vector3.right);
        return look * rotAdjustment * rotAdjustment2;
    }


    #region TWEENS
    /// <summary>
    /// Rotation tween
    /// </summary>
    /// <param name="s"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private IEnumerator RotationTween(Quaternion s, Quaternion e)
    {
        StartCoroutine(RotationTween(s, e, delegate { }));
        yield return null;
    }

    private IEnumerator RotationTween(Quaternion s, Quaternion e, Action a)
    {
        float journey = 0f;
        while (journey <= animationDurationForRotation)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationDurationForRotation);
            float curvePercent = curve.Evaluate(percent);
            Quaternion current = Quaternion.Lerp(s, e, curvePercent);
            transform.rotation = current;
            yield return null;
        }
        a.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="r1"></param>
    /// <param name="r2"></param>
    /// <returns></returns>
    //private IEnumerator RadiusTween(float r1, float r2)
    //{
    //    float journey = 0f;
    //    while (journey <= animationDuration)
    //    {
    //        journey = journey + Time.deltaTime;
    //        float percent = Mathf.Clamp01(journey / animationDuration);
    //        float curvePercent = curve.Evaluate(percent);
    //        float current = Mathf.LerpUnclamped(r1, r2, curvePercent);
    //        clipController.radius = current;
    //        yield return null;
    //    }

    //    isRadiusAnimationPlaying = false;
    //    OnRadiusAnimationEnd.Invoke();
    //}

    /// <summary>
    /// Animate the scaling by zoom level changes
    /// </summary>
    /// <param name="z1"></param>
    /// <param name="z2"></param>
    /// <returns></returns>
    private IEnumerator ScaleByZoomTween(float z1, float z2, Action a)
    {
        float journey = 0f;
        while (journey <= animationDurationForZoom)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationDurationForZoom);
            float curvePercent = curve.Evaluate(percent);
            float current = Mathf.LerpUnclamped(z1, z2, curvePercent);
            ScaleByZoom(current);
            yield return null;
        }

        isScaleAnimationPlaying = false;
        a.Invoke();
    }

    /// <summary>
    /// Play scale animation by given new ratio dimension
    /// </summary>
    /// <param name="ratio">scale of dimension change</param>
    /// <returns></returns>
    private IEnumerator ScaleByDimensionTween(float newHeight, float newWidth)
    {
        float h = clipController.height;
        float w = clipController.width;
        float journey = 0f;
        while (journey <= animationDurationForZoom)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationDurationForZoom);
            float curvePercent = curve.Evaluate(percent);
            float currentH = Mathf.LerpUnclamped(h, newHeight, curvePercent);
            float currentW = Mathf.LerpUnclamped(w, newWidth, curvePercent);
            ScaleByDimension(0, currentH, currentW);

            yield return null;
        }

        isScaleAnimationPlaying = false;
        OnScaleAnimationEnd.Invoke();
    }

    private IEnumerator ScaleByDimensionTween(float newHeight, float newWidth, Action t)
    {
        float h = clipController.height;
        float w = clipController.width;
        float journey = 0f;
        while (journey <= animationDurationForZoom)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationDurationForZoom);
            float curvePercent = curve.Evaluate(percent);
            float currentH = Mathf.LerpUnclamped(h, newHeight, curvePercent);
            float currentW = Mathf.LerpUnclamped(w, newWidth, curvePercent);
            ScaleByDimension(0, currentH, currentW);
            yield return null;
        }

        isScaleAnimationPlaying = false;
        t.Invoke();
    }

    private IEnumerator ZoomByDimensionTween(float newHeight)
    {
        float h = clipController.height;
        float w = clipController.width;
        float journey = 0f;
        while (journey <= animationDurationForZoom)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationDurationForZoom);
            float curvePercent = curve.Evaluate(percent);
            //if (clipController.shape == ClipShape.Circle)
            //{
            //    float currentR = Mathf.LerpUnclamped(r, newRadius, curvePercent);
            //    ZoomByDimension(currentR, 0);
            //}

            float currentH = Mathf.LerpUnclamped(h, newHeight, curvePercent);
            ZoomByDimension(0, currentH);
            yield return null;
        }

        isScaleAnimationPlaying = false;
        OnScaleAnimationEnd.Invoke();
    }

    /// <summary>
    /// Animate the scaling
    /// </summary>
    /// <param name="z1"></param>
    /// <param name="z2"></param>
    /// <returns></returns>
    private IEnumerator HeightTween(float start, float finish)
    {
        float journey = 0f;
        while (journey <= animationDuration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationDuration);
            float curvePercent = curve.Evaluate(percent);
            float current = Mathf.LerpUnclamped(start, finish, curvePercent);
            abstractMap.Terrain.SetExaggerationFactor(current);
            abstractMap.UpdateMap();
            yield return null;
        }

        isHeightAnimationPlaying = false;
        OnHeightAnimationEnd.Invoke();
    }

    private IEnumerator ZoomTween(float s, float e, Action action)
    {
        float journey = 0f;
        while (journey <= animationDurationForZoom)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationDurationForZoom);
            float curvePercent = curve.Evaluate(percent);
            float current = Mathf.LerpUnclamped(s, e, curvePercent);
            abstractMap.UpdateMap(current);
            yield return null;
        }

        isScaleAnimationPlaying = false;
        action.Invoke();
    }

    //Does not work
    private IEnumerator HeightTween(int s, int e, Action action)
    {
        float journey = 0f;
        while (journey <= animationDuration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationDuration);
            float curvePercent = curve.Evaluate(percent);
            float current = Mathf.LerpUnclamped(s, e, curvePercent);
            abstractMap.Terrain.SetExaggerationFactor(current);
            abstractMap.UpdateMap();
            yield return null;
        }

        isHeightAnimationPlaying = false;
        action.Invoke();

    }

    /// <summary>
    /// Tween animation
    /// </summary>
    /// <param name="s">start position</param>
    /// <param name="e">end position</param>
    /// <param name="map">map gameobject</param>
    /// <returns></returns>
    private IEnumerator MovementTween(Vector3 s, Vector3 e, Action a)
    {
        float journey = 0f;
        while (journey <= animationDuration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationDuration);
            float curvePercent = curve.Evaluate(percent);
            Vector3 current = Vector3.LerpUnclamped(s, e, curvePercent);
            transform.position = current;
            yield return null;
        }

        transform.position = e;
        isMoveAnimationPlaying = false;
        a.Invoke();
    }

    private IEnumerator FocusTween(float r1, float r2)
    {
        float journey = 0f;
        float d = animationDuration * 0.5f;
        while (journey <= d)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / d);
            float curvePercent = curve.Evaluate(percent);
            float current = Mathf.LerpUnclamped(r1, r2, curvePercent);
            clipController.border = current;
            yield return null;
        }
        isFocusAnimating = false;
    }

    private IEnumerator ViewWidthTween(float w1, float w2, Action action)
    {
        float journey = 0f;
        while (journey <= animationDuration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationDuration);
            float curvePercent = curve.Evaluate(percent);
            float currentW = Mathf.LerpUnclamped(w1, w2, curvePercent);
            clipController.width = currentW;
            yield return null;
        }

        clipController.width = w2;
        isViewWidthAnimationPlaying = false;
        action.Invoke();
    }

    private IEnumerator ViewHeightTween(float h1, float h2)
    {
        float journey = 0f;
        while (journey <= animationDuration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationDuration);
            float curvePercent = curve.Evaluate(percent);
            float currentH = Mathf.LerpUnclamped(h1, h2, curvePercent);
            clipController.height = currentH;
            yield return null;
        }

        clipController.height = h2;
        isViewHeightAnimationPlaying = false;
        OnViewHeighAnimationEnd.Invoke();
    }
    #endregion

}
