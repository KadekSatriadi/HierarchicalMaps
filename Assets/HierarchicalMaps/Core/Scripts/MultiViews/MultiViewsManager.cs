using Mapbox.Unity.Map;
using Mapbox.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiViewsManager : MonoBehaviour
{
    public MutiViewsArrangementManager mutiViewsArrangementManager;
    public GameObject mapPrefab;
    public GameObject viewFinderPrefab;

    #region EVENTS
    public event NewMapLensCreatedEventHandler NewMapLensCreated;
    public event NewMapLensRegisteredEventHandler NewMapLensRegistered;

    public delegate void NewMapLensCreatedEventHandler(MapLens map);
    public delegate void NewMapLensRegisteredEventHandler(MapLens map);

    #endregion

    private int nSeeds = 10;
    private GameObject mapSeed;
    private MapLens parentMap;
    private ViewFinder lastViewFinder;
    void Start()
    {
        //Counter of maps
        PlayerPrefs.SetInt("NMaps", 10);
        CreateMapSeed();
    }
    /// <summary>
    /// Set parent map
    /// </summary>
    /// <param name="p"></param>
    public void SetParentMap(MapLens p)
    {
        parentMap = p;
    }

    public void InstantiateChildMap(Vector3 position, Quaternion rotation, Vector3 center, ViewFinder v)
    {
        
        Vector2d centerMap = Vector2d.zero;
        if (v.shape == ClipShape.Circle)
        {
            centerMap = parentMap.abstractMap.WorldToGeoPosition(center);
        }
        if (v.shape == ClipShape.Rectangle)
        {
            centerMap = parentMap.abstractMap.WorldToGeoPosition(v.GetRectangleCenter());
        }
        CreateMapLens(parentMap.transform.position, parentMap.transform.rotation, centerMap.x + "," + centerMap.y, v.parent.abstractMap.Zoom, v.radius, v.height, v.width);
        lastViewFinder = v;
    }


    /// <summary>
    /// Remove viewport, allocate new parent to all children
    /// </summary>
    /// <param name="map"></param>
    public void Remove(MapLens map)
    {
        if (map.parent == null) return;

        ViewFinder viewFinder = map.viewFinder;

        mutiViewsArrangementManager.Remove(map);
        map.Remove();


    }

    /// <summary>
    /// Remove viewfinder
    /// </summary>
    public void RemoveViewFinder(ViewFinder v)
    {
        //if (v != null && v.child != null) viewFinders.Remove(v);
        Destroy(v.gameObject);
    }

    public ViewFinder CreateViewfinder(MapLens map, Vector3 position, Quaternion rotation)
    {
        GameObject viewFinderGameobject = Instantiate(viewFinderPrefab) as GameObject;

        ViewFinder v = viewFinderGameobject.GetComponent<ViewFinder>();
        v.Initiate(map, map.abstractMap.WorldToGeoPosition(position));
        lastViewFinder = v;

        return v;
        //viewFinders.Add(v);
    }
    public ViewFinder CreateViewfinder(MapLens map, Vector3 position, Quaternion rotation, float h, float w)
    {
        GameObject viewFinderGameobject = Instantiate(viewFinderPrefab) as GameObject;

        ViewFinder v = viewFinderGameobject.GetComponent<ViewFinder>();
        v.Initiate(map, map.abstractMap.WorldToGeoPosition(position), h, w);
        lastViewFinder = v;

        return v;
        //viewFinders.Add(v);
    }


    /// <summary>
    /// Update map radius based on increase of zoom level
    /// </summary>
    /// <param name="map">Map</param>
    /// <param name="increased">Zoom level increase</param>
    public void AdjustViewZoom(MapLens map, float increased)
    {
        AbstractMap abstractMap = map.abstractMap;
        float z1 = abstractMap.Zoom;
        float z2 = z1 + increased;

        //adjust radius from new zom
        map.ScaleByZoom(z2);
        //add to viewfinder
        //viewFinders[viewFinders.Count - 1].AddChild(map);
    }

    //public bool IsMapExist(MapLens g)
    //{
    //    return mapLenses.Contains(g);
    //}

    /// <summary>
    /// Initiate Mapbox abstract map
    /// </summary>
    /// <param name="map"></param>
    /// <param name="center"></param>
    /// <param name="zoom"></param>
    /// <param name="radius"></param>
    void InitiateMap(MapLens map, string center, float zoom, float radius, string style)
    {
        map.abstractMap.ImageLayer.SetLayerSource(ImagerySourceType.Custom);
        map.abstractMap.ImageLayer.SetLayerSource(style);
        map.abstractMap.Options.locationOptions.latitudeLongitude = center;
        map.abstractMap.Options.locationOptions.zoom = zoom;
        map.Initiate();
    }

    void InitiateMap(MapLens map, string center, float zoom, float height, float width, string style)
    {
        map.abstractMap.ImageLayer.SetLayerSource(ImagerySourceType.Custom);
        map.abstractMap.ImageLayer.SetLayerSource(style);
        map.abstractMap.Options.locationOptions.latitudeLongitude = center;
        map.abstractMap.Options.locationOptions.zoom = zoom;
        map.latLong = center;
        map.zoom = zoom;
        map.styleURL = style;
        map.clipController.width = width;
        map.clipController.height = height;
        map.Initiate();
    }

    //void InitiateUIClipController(MapLens map)
    //{
    //    UISphereClipController ui = map.GetComponentInChildren<UISphereClipController>();
    //    if (ui)
    //    {
    //        ui.ResetPosition();
    //        ui.SetActive(true);
    //    }
    //}

    /// <summary>
    /// Map seed is an innactive map that will be 
    /// </summary>
    void CreateMapSeed()
    {
        mapSeed = Instantiate(mapPrefab);
        mapSeed.name = "MapSeed";
        mapSeed.SetActive(false);
    }

    /// <summary>
    /// Create copy of Seed map
    /// </summary>
    /// <returns></returns>
    GameObject GetSeedCopy()
    {
        if (mapSeed == null) CreateMapSeed();
        mapSeed.SetActive(true);
        return mapSeed;
    }

    /// <summary>
    /// Activating map seed and creating new seed
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="center"></param>
    /// <param name="zoom"></param>
    /// <param name="radius"></param>
    public void CreateMapLens(Vector3 position, Quaternion rotation, string center, float zoom, float radius, float height, float width)
    {
        //prepare gameobject
        GameObject mapLensGameobject;
        mapLensGameobject = GetSeedCopy();

        mapLensGameobject.transform.position = position;
        mapLensGameobject.transform.rotation = rotation;
        mapLensGameobject.name = "Map_" + FindObjectsOfType<MapLens>().Length + 1;

        //Update map
        MapLens map = mapLensGameobject.GetComponentInChildren<MapLens>();

        //Set clip ID,  update local data
        int currentMapN = PlayerPrefs.GetInt("NMaps") + 1;
        map.clipController.clipID = currentMapN;
        PlayerPrefs.SetInt("NMaps", currentMapN);

        Debug.Log(zoom);

        //Map hierarchy level
        if (parentMap == null) //first map
        {
            map.level = 0;
        }
        else
        {
            map.level = parentMap.level + 1;
            map.parent = parentMap;
            map.viewFinder = lastViewFinder;
            map.viewFinder.child = map;
        }

        InitiateMap(map, center, zoom, height, width, parentMap.styleURL);

        map.OnReady += delegate
        {
            

            //created event
            if (NewMapLensCreated != null) NewMapLensCreated(map);

            //arrange        
            mutiViewsArrangementManager.Register(map);

            //check enclosed viewfinders
            mutiViewsArrangementManager.UpScaleChildrenViewFinders(map);

            //registered event
            if (NewMapLensRegistered != null) NewMapLensRegistered(map);

            //Create seed
            CreateMapSeed();
        };

    }
}
