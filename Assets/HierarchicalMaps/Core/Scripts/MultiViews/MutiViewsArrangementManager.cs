using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutiViewsArrangementManager : MonoBehaviour
{
    public static MutiViewsArrangementManager Instance = null;
    protected List<MapLens> maps = new List<MapLens>();
    protected Dictionary<Color, MapLens> colorMapLensDictionary = new Dictionary<Color, MapLens>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Add map to list
    /// </summary>
    /// <param name="map"></param>
    public virtual void Register(MapLens map)
    {
        if (!maps.Contains(map))
        {
            maps.Add(map);
            SetColor(map);
            UpdateAllMaps();
            map.OnRemove += delegate
            {
                Remove(map);
            };
        }
    }

    /// <summary>
    /// Remove map from list
    /// </summary>
    /// <param name="map"></param>
    public virtual void Remove(MapLens map)
    {
        maps.Remove(map);
    }

    /// <summary>
    /// Update all maps
    /// </summary>
    protected void UpdateAllMaps()
    {
        foreach (MapLens m in maps)
        {
            if (m != null) m.abstractMap.UpdateMap(m.abstractMap.Zoom);
        }
    }

    /// <summary>
    /// Swap position
    /// </summary>
    /// <param name="m1"></param>
    /// <param name="m2"></param>
    protected void Swap(MapLens m1, MapLens m2)
    {
        int id1 = maps.IndexOf(m1);
        int id2 = maps.IndexOf(m2);

        maps[id1] = m2;
        maps[id2] = m1;
    }

    /// <summary>
    /// Get current maximum level
    /// </summary>
    /// <returns></returns>
    protected int GetMaxLevel()
    {
        int l = 0;
        foreach (MapLens m in maps)
        {
            if (m.level > l) l = m.level;
        }

        return l;
    }

    /// <summary>
    /// Get root map
    /// </summary>
    /// <returns></returns>
    protected MapLens GetRootMap()
    {
        MapLens root = maps[0];
        foreach(MapLens m in maps)
        {
            if(m.level == 0)
            {
                root = m;
            }
        }

        return root;
    }

    /// <summary>
    /// Get list of map in a particular level
    /// </summary>
    /// <param name="lvl"></param>
    /// <returns></returns>
    protected List<MapLens> GetMapsInLevel(int lvl)
    {
        List<MapLens> l = new List<MapLens>();
        foreach (MapLens m in maps)
        {
            if (m.level == lvl) l.Add(m);
        }
        return l;
    }

    /// <summary>
    /// Get map in level but exclude a map
    /// </summary>
    /// <param name="lvl"></param>
    /// <param name="exclude"></param>
    /// <returns></returns>
    protected List<MapLens> GetMapsInLevel(int lvl, MapLens exclude)
    {
        List<MapLens> l = new List<MapLens>();
        foreach (MapLens m in maps)
        {
            if (m.level == lvl && exclude != m) l.Add(m);
        }
        return l;
    }

    /// <summary>
    /// Get all children (first generation under it, not entire nodes)
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    public List<MapLens> GetChildren(MapLens map)
    {
        List<MapLens> children = new List<MapLens>();
        foreach (MapLens m in maps)
        {
            if (m != null && m != map)
            {
                if (m.parent != null && m.parent == map)
                {
                    children.Add(m);
                }
            }
        }
        return children;
    }

    /// <summary>
    /// Get all viewfinder attached to map
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    public List<ViewFinder> GetViewFinders(MapLens map)
    {
        List<ViewFinder> v = new List<ViewFinder>();
        foreach(MapLens m in GetChildren(map))
        {
            v.Add(m.viewFinder);
        }
        return v;

    }

    /// <summary>
    /// Check if viewfinder c is inside viewfinder f
    /// </summary>
    /// <param name="f"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public static bool IsViewFinderEncloses(ViewFinder f, ViewFinder c)
    {
        if (Vector3.Distance(f.transform.position, c.transform.position) + c.radius * 0.5f <= f.radius * 0.5f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Get all maps inside viewfinder f
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    protected List<MapLens> GetAllMapInsiderViewfinder(ViewFinder f)
    {
        ViewFinder[] viewFinders = FindObjectsOfType<ViewFinder>();
        List<MapLens> list = new List<MapLens>();
        for(int i = 0; i < viewFinders.Length; i++)
        {
            ViewFinder v = viewFinders[i];
            if (v != null && !f.Equals(v) && v.parent == f.parent)
            {
                if (IsViewFinderEncloses(f, v))
                {
                    list.Add(v.child);
                }
            }
        }
        return list;
    }

    /// <summary>
    /// Move all children map viewfinders to new parent
    /// </summary>
    /// <param name="newParent"></param>
    public void UpScaleChildrenViewFinders(MapLens newParent)
    {
        List<MapLens> children = GetAllMapInsiderViewfinder(newParent.viewFinder);
        foreach (MapLens m in children)
        {
            if (m != newParent)
            {
                m.viewFinder.parent = newParent;
                m.parent = newParent;
            }
        }

        UpdateColorAccordingToNewChildren(newParent, children);
    }

    /// <summary>
    /// Update color of new parent according to new children inside its viewfinder
    /// </summary>
    /// <param name="newParent"></param>
    /// <param name="children"></param>
    public void UpdateColorAccordingToNewChildren(MapLens newParent, List<MapLens> children)
    {
        int smallestLevel = 100;
        int idx = 0;
        foreach (MapLens m in children)
        {
            if (m.level < smallestLevel)
            {
                smallestLevel = m.level;
                idx = children.IndexOf(m);
            }
        }

        //update color
        if (children.Count > 0)
        {
            float x = 0.15f;
            float b = x * (smallestLevel - newParent.level);
            Color parentColor = children[idx].viewFinder.color;
            Color c = new Color(parentColor.r + b, parentColor.g + b, parentColor.b + b, parentColor.a);
            newParent.SetColor(c);
        }
    }

    /// <summary>
    /// Check whether two viewfinders overlap
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public bool AreViewFinderEnclosed(ViewFinder v1, ViewFinder v2)
    {
        bool r = false;
        if (v1.shape == ClipShape.Circle && v2.shape == ClipShape.Circle)
        {
            if (v1.radius > v2.radius)
            {
                r = IsViewFinderEncloses(v1, v2);
            }
            else
            {
                r = IsViewFinderEncloses(v2, v1);
            }
        }
        if (v1.shape == ClipShape.Rectangle && v2.shape == ClipShape.Rectangle)
        {
            float area1 = v1.width * v1.height;
            float area2 = v2.width * v2.height;
            if (area1 > area2)
            {
                return IsPointOnRectangleViewFinder(v1, v2.GetRectangleCenter());
            }
            else
            {

            }
        }


        return r;
    }

    /// <summary>
    /// Is viewfinder outsie the map m
    /// </summary>
    /// <param name="v"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    public bool IsViewFinderOutsideMap(ViewFinder v, MapLens m)
    {
        if (v.parent != m)
        {
            return false;
        }
        else
        {
            bool res = true;
            //if(m.clipController.shape == ClipShape.Circle)
            //{
            //    float d = Vector3.Distance(v.transform.position, m.center.position);
            //    float dR = d - v.radius * 0.5f;
            //    if (m.clipController.radius * 0.5f + m.clipController.border < dR)
            //    {
            //        res = true;
            //    }
            //    else
            //    {
            //        res = false;
            //    }
            //}
            Vector3 position = v.GetRectangleCenter();
            Vector3 vectorToCenter = position - m.center.position;
            Vector3 widthProject = Vector3.Project(vectorToCenter, m.transform.right);
            if (Vector3.Dot(vectorToCenter, m.transform.right) < 0)
            {
                widthProject = Vector3.Project(vectorToCenter, -m.transform.right);
            }
            Debug.DrawRay(m.center.position, widthProject, Color.blue);
            Vector3 heightProject = Vector3.Project(vectorToCenter, m.transform.forward);
            if (Vector3.Dot(vectorToCenter, heightProject) < 0)
            {
                heightProject = Vector3.Project(vectorToCenter, -m.transform.forward);
            }
            Debug.DrawRay(m.center.position, heightProject, Color.blue);
            Debug.Log(widthProject.magnitude.ToString());
            Debug.Log(m.clipController.width.ToString());


            if (widthProject.magnitude > m.clipController.width * 0.5f || heightProject.magnitude > m.clipController.height * 0.5f)
            {
                res = true;
            }
            else
            {
                res = false;
            }
            return res;
        }
    }

    /// <summary>
    /// Is a point on viewfinder rectangle
    /// </summary>
    /// <param name="v"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsPointOnRectangleViewFinder(ViewFinder v, Vector3 position)
    {
        bool res = false;

        Vector3 vectorToCenter = position - v.GetRectangleCenter();
        Vector3 widthProject = Vector3.Project(vectorToCenter, v.transform.right);
        if (Vector3.Dot(vectorToCenter, v.transform.right) < 0)
        {
            widthProject = Vector3.Project(vectorToCenter, -v.transform.right);
        }
        Vector3 heightProject = Vector3.Project(vectorToCenter, v.transform.forward);
        if (Vector3.Dot(vectorToCenter, heightProject) < 0)
        {
            heightProject = Vector3.Project(vectorToCenter, -v.transform.forward);
        }
    

        if (widthProject.magnitude > v.width * 0.5f || heightProject.magnitude > v.height * 0.5f)
        {
            res = true;
        }
        else
        {
            res = false;
        }

        return res;
    }

    /// <summary>
    /// Focus all maps from m to root
    /// </summary>
    /// <param name="m"></param>
    public void FocusMapPath(MapLens m)
    {
        if (m.viewFinder != null)
        {
            m.viewFinder.ShowVisualLink();
            FocusMapPath(m.parent);
        }
    }

    /// <summary>
    /// Outfocus all maps from m to root
    /// </summary>
    /// <param name="m"></param>
    public void OutFocusMapPath(MapLens m)
    {
        if (m.viewFinder != null)
        {
            m.viewFinder.HideVisualLink();
            OutFocusMapPath(m.parent);
        }
    }

    /// <summary>
    /// Get available color from color map dictionary
    /// </summary>
    /// <returns></returns>
    protected Color GetAvailableColor(MapLens map)
    {
        Color c = new Color();
        //Update missing map
        foreach (var pair in colorMapLensDictionary)
        {
            if(pair.Value != null)
            {
                if (pair.Value == map)
                {
                    return pair.Key;
                }
            }         
        }

        //Get colour
        foreach (var pair in colorMapLensDictionary)
        {
            if (pair.Value == null)
            {
                c = pair.Key;
                colorMapLensDictionary[c] = map;
                break;
            }
        }

        return c;
    }

    /// <summary>
    /// Get color brightness according to map parent's hue
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    protected Color GetColorBrightness(MapLens map, bool isLevelUp)
    {
        float x = 0.05f;
        float b = x * map.level;
        Color parentColor = new Color();
        if (map.parent.viewFinder == null)
        {
            parentColor = map.GetColor();
        }
        else
        {
            parentColor = map.viewFinder.parent.GetColor();
        }

        if (isLevelUp)
        {
            return new Color(parentColor.r - b, parentColor.g - b, parentColor.b - b, parentColor.a);

        }
        else
        {
            return new Color(parentColor.r + b, parentColor.g + b, parentColor.b + b, parentColor.a);
        }

    }

    /// <summary>
    /// Set color of the map, first detail lens has different hue, the children have different brightness
    /// </summary>
    /// <param name="map"></param>
    public void SetColor(MapLens map)
    {
        if (map.parent == null) return;

        Color c = new Color();
        if (map.parent.level == 0)
        {
            c = GetAvailableColor(map);
        }
        else
        {
            c = GetColorBrightness(map, true);
        }

        map.SetColor(c);
    }

    /// <summary>
    /// Intiate path colors
    /// </summary>
    protected void InitColors()
    {
        //color pallete
        List<Color> lensColors = ColorPalette.GetColourPalette12();

        //copy color to stack
        foreach (Color c in lensColors)
        {
            colorMapLensDictionary.Add(c, null);
        }
    }

    /// <summary>
    /// Combining two maplenses
    /// </summary>
    /// <param name="m1"></param>
    /// <param name="m2"></param>
    public void CombineMapLenses(MapLens m1, MapLens m2)
    {
        //combine only if one is inside another
        if (AreViewFinderEnclosed(m1.viewFinder, m2.viewFinder))
        {
            //if different level
            if (m1.level != m2.level)
            {
                if (m1.viewFinder.radius < m2.viewFinder.radius)
                {
                    m1.viewFinder.parent = m2;
                    m1.parent = m2;
                }
                else
                {
                    m2.viewFinder.parent = m1;
                    m2.parent = m1;
                }
            }
            else //same level
            {
                if (m1.viewFinder.radius < m2.viewFinder.radius)
                {
                    m1.viewFinder.parent = m2;
                    m1.parent = m2;
                    m1.HideMapLens();
                }
                else
                {
                    m2.viewFinder.parent = m1;
                    m2.parent = m1;
                    m2.HideMapLens();
                }
            }
        }
    }
}
