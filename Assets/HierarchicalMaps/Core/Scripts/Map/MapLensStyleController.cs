using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapLensStyleController : MonoBehaviour
{

    [System.Serializable]
    public struct MapLensStyle
    {
         public ImagerySourceType type;
         public string url;
         public string label;
         public Sprite icon;
    };

    public MapLens map;
    public bool overrideMapSyleOnStart = false;
    public int defaultStyleIndex = 0;
    public List<MapLensStyle> styles = new List<MapLensStyle>();
    [Header("UI")]
    public Button buttonPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (styles.Count > 0)
        {
            switch (styles[0].type)
            {
                case ImagerySourceType.Custom:
                    map.styleURL = styles[0].url;
                    break;
                case ImagerySourceType.MapboxStreets:
                    SetStreets();
                    break;
                case ImagerySourceType.MapboxSatellite:
                    SetSatellite();
                    break;
            }

            foreach(MapLensStyle style in styles)
            {
                GameObject b = Instantiate(buttonPrefab.gameObject);
                b.transform.position = buttonPrefab.transform.position + buttonPrefab.transform.up * 0.1f * styles.IndexOf(style);
                b.transform.rotation = buttonPrefab.transform.rotation;
                b.transform.SetParent(buttonPrefab.transform.parent);
                b.transform.localScale = Vector3.one;
                b.GetComponentInChildren<Image>().sprite = style.icon;
                b.GetComponent<Button>().onClick.AddListener(delegate () {
                    SetStyle(styles.IndexOf(style));
                });
            }
            buttonPrefab.gameObject.SetActive(false);
        }

        if (overrideMapSyleOnStart)
        {
            map.OnReady += delegate
            {
                SetStyle(defaultStyleIndex);
            };
        }        
    }

    public void SetSatellite()
    {
        SetLayer(ImagerySourceType.MapboxSatellite);
    }

    public void SetStreets()
    {
        SetLayer(ImagerySourceType.MapboxStreets);
    }

    public void SetLayer(Mapbox.Unity.Map.ImagerySourceType type)
    {
        map.abstractMap.ImageLayer.SetLayerSource(type);
        map.abstractMap.UpdateMap();
    }

    public void SetStyle(int id)
    {
        switch (styles[id].type)
        {
            case ImagerySourceType.Custom:
                map.abstractMap.ImageLayer.SetLayerSource(ImagerySourceType.Custom);
                map.abstractMap.ImageLayer.SetLayerSource(styles[id].url);
                map.abstractMap.UpdateMap();
                break;
            case ImagerySourceType.MapboxStreets:
                SetStreets();
                break;
            case ImagerySourceType.MapboxSatellite:
                SetSatellite();
                break;
        }
        
    }
}
