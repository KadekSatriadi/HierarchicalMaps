using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiViewsDepthLayersArrangement : MutiViewsArrangementManager
{
    public enum DetailViewsRotation
    {
        Forward, LookAtUser
    }
    public enum DetailViewsPosition
    {
        Forward, FOV
    }
    public enum Hierarchy
    {
        Hierarchy, NonHierarchy
    }

    public enum DepthLayer
    {
        Single, Dual, Multi
    }
    [Header("Design Factors")]
    public DetailViewsRotation detailViewsRotation;
    public DetailViewsPosition detailViewsPosition;
    public Hierarchy hierarchy;
    public DepthLayer depthLayer;

    [Header("Arrangement Position")]
    public float comfortViewDistance;

    [Range(0,1)]
    public float locality;
    [Range(0, 1)]
    public float comparability;
    [Range(0, 2)]
    public float comparabilityOffset;

    public Transform centerOfEyes;


    [Tooltip("Max lens radius")]
    public float maxLensRadius = 1f;

    private List<Color> lensColors = new List<Color>();
    private int lastMapsCount = 0;

    private void Start()
    {

        //color pallete
        lensColors = ColorPalette.GetColourPalette12();

        //copy color to stack
        foreach(Color c in lensColors)
        {
            colorMapLensDictionary.Add(c, null);
        }
    }

    /// <summary>
    /// Register the map to the layout
    /// </summary>
    /// <param name="map"></param>
    public override void Register(MapLens map)
    {

        //Position the child map
        if (map.parent != null) //not parent map
        {
            //Override map lens max radius
            map.SetMaxLensRadius(maxLensRadius);

            //Place on layer
            PlaceMap(map);

            //Adjust scale, of course
            ScaleMapPolyZoom(map);

                       //Set Colour
            SetColor(map);
        }

        maps.Add(map);
        lastMapsCount++;
    }

    public override void Remove(MapLens map)
    {
        maps.Remove(map);
        if(depthLayer == DepthLayer.Multi)
        {
            ArrangeLayerPosition();
        }
    }

    /// <summary>
    /// Unfold maps layer
    /// </summary>
    public void UnfoldLayer()
    {
        if (depthLayer != DepthLayer.Multi) return;
        int level = GetMaxLevel() + 1;
        List<MapLens> ms = new List<MapLens>();

        foreach(MapLens map in foldedLens)
        {
            if(map != null && map.level == level)
            {
               if(map.viewFinder != null)  map.viewFinder.ShowVisualLink();
                maps.Add(map);
                //remove callback
                if (mapDelegates.ContainsKey(map) && mapDelegates[map] != null)
                {
                    Action del = mapDelegates[map];
                    map.OnMoveAnimationEnd -= del;
                }
                map.PlayAnimationMovement(map.transform.position, foldedPosition[foldedLens.IndexOf(map)]);
                //map.Rotate(map.transform.rotation.eulerAngles, foldedRotation[foldedLens.IndexOf(map)]);
                map.transform.localEulerAngles = foldedRotation[foldedLens.IndexOf(map)];
                ms.Add(map);
            }
        }
        int i = 0;
        foreach(MapLens m in ms)
        {
            foldedPosition.RemoveAt(foldedLens.IndexOf(m));
            foldedRotation.RemoveAt(foldedLens.IndexOf(m));
            foldedLens.Remove(m);
            i++;
        }

        ArrangeLayerPosition();
    }

    /// <summary>
    /// Fold front layer: move it to head position
    /// </summary>
    /// 
    private float foldRotation = 0;
    private List<MapLens> foldedLens = new List<MapLens>();
    private List<Vector3> foldedPosition = new List<Vector3>();
    private List<Vector3> foldedRotation = new List<Vector3>();
    private Dictionary<MapLens, Action> mapDelegates = new Dictionary<MapLens, Action>();

    public void FoldFrontLayer()
    {
        if (maps.Count == 1 && foldedLens.Count == 0) return;
        if (depthLayer != DepthLayer.Multi) return;

        int max = GetMaxLevel();
        float stackDepth = 0.5f;
        List<MapLens> collideMaps = GetMapsInLevel(max);
        if (max == 0) return;
        foreach (MapLens map in collideMaps)
        {
            if (map == null) continue;

            Vector3 pointFront = GetProjectedPointOnOverview(centerOfEyes.transform.position) + GetVectorToUserAdjusted() * 1.15f;
            Vector3 pointA = pointFront + (GetOverviewMap().transform.right * 100);
            Vector3 pointB = pointFront - (GetOverviewMap().transform.right * 100);

            Vector3 position = map.transform.position;

            Vector3 projectedPointOnTheLine = pointA + Vector3.Dot(position - pointA, pointB - pointA) / Vector3.Dot(pointB - pointA, pointB - pointA) * (pointB - pointA);
            projectedPointOnTheLine -= GetOverviewMap().transform.forward * (stackDepth  + (0.05f * map.level));

            foldedPosition.Add(map.transform.position);
            foldedRotation.Add(map.transform.localEulerAngles);
            if (map.viewFinder != null) map.viewFinder.HideVisualLink();
            map.PlayAnimationMovement(map.transform.position, projectedPointOnTheLine);
            //map.Rotate(map.transform.rotation.eulerAngles, new Vector3(foldRotation, 0, 0));
            map.Rotate();
            //Commented because too slow
            //add callback
            //if(!mapDelegates.ContainsKey(map))
            //{
            //    Action del = delegate
            //    {
            //        map.PlayHeightAnimate(80, 100, 5);
            //    };
            //    mapDelegates.Add(map, del);
            //    map.OnMoveAnimationEnd += del;
            //}
            //else
            //{
            //    Action del = mapDelegates[map];
            //    map.OnMoveAnimationEnd += del;
            //}
            maps.Remove(map);

            foldedLens.Add(map);
        }
        ArrangeLayerPosition();
    }

    /// <summary>
    /// Check if the map is in level 1
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    private bool IsFirstLevelDetail(MapLens map)
    {
        return (map.level == 1) ? true : false;
    }

    /// <summary>
    /// Check if map is connected to overview map
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    private bool IsMapConnectedToOverview(MapLens map)
    {
        return (map.parent.level == 0) ? true : false;
    }

    /// <summary>
    /// Get all successors of the map
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    public List<MapLens> GetAllSuccessors(MapLens map)
    {
        List<MapLens> children = new List<MapLens>();
        foreach(MapLens m in maps)
        {
            if(m != null && m != map)
            {
                if(m.parent != null && m.parent == map)
                {
                    children.Add(m);
                    children.AddRange(GetAllSuccessors(m));
                }
            }
        }
        return children;
    }

    /// <summary>
    /// Level up map to higher layer
    /// </summary>
    /// <param name="map"></param>
    public void LevelUp(MapLens map)
    {
        if(map.level > 0)
        {

          //  bool isNewMaxLevel = (map.level + 1 > GetMaxLevel())? true : false;
            map.level++;

            // Vector3 newPosition = map.transform.position + map.transform.up * comfortViewDistance;
            switch (depthLayer)
            {
                case DepthLayer.Multi:
                    Vector3 newPosition = GetMultiDepthMapPosition(map);
                    map.SetColor(GetColorBrightness(map, true));
                    map.PlayAnimationMovement(map.transform.position, newPosition);
                    ScaleMapPolyZoom(map);
                    ArrangeLayerPosition();
                    break;
                case DepthLayer.Single:
                    ScaleMapPolyZoom(map);
                    break;
            }
           
            
            
        }
    }

    /// <summary>
    /// Level down
    /// </summary>
    /// <param name="map"></param>
    public void LevelDown(MapLens map)
    {
        if (map.level > 1)
        {

            map.level--;
           // bool isNewMaxLevel = (map.level > GetMaxLevel()) ? true : false;

            switch (depthLayer)
            {
                case DepthLayer.Multi:
                    Vector3 newPosition = GetMultiDepthMapPosition(map);
                    map.SetColor(GetColorBrightness(map, false));
                    map.PlayAnimationMovement(map.transform.position, newPosition);
                    ScaleMapPolyZoom(map);
                    ArrangeLayerPosition();
                    break;
                case DepthLayer.Single:
                    ScaleMapPolyZoom(map);
                    break;
            }
            //Vector3 newPosition = map.transform.position - map.transform.up * comfortViewDistance;
            //map.SetColor(GetColorBrightness(map, false));
            //map.PlayAnimationMovement(map.transform.position, newPosition);
            //ScaleMap(map);
        }
        else if(map.level == 1)
        {
            FindObjectOfType<MultiViewsManager>().Remove(map);
        }
    }

    /// <summary>
    /// Scale map according to PolyZoom approach
    /// </summary>
    /// <param name="map"></param>
    private void ScaleMapPolyZoom(MapLens map)
    {
        AdjustScalePolyZoom(map);

        foreach (MapLens m in maps)
        {
           if(m.level != 0) AdjustScalePolyZoom(m);
        }        
    }
    private void AdjustScalePolyZoom(MapLens map)
    {
        float overviewHeight = GetOverviewMap().clipController.height;
        float hSegment = overviewHeight / (GetMaxLevel() + 2);
        float height = hSegment;
        float width = height * map.viewFinder.width / map.viewFinder.height;
        map.PlayScaleByDimensionAnimate(height, width);
    }

    /// <summary>
    /// Place map according to layout specification
    /// </summary>
    /// <param name="map"></param>
    private void PlaceMap(MapLens map)
    {
        Vector3 position = LocalityToPosition(map);

        //Depth
        Vector3 vectorToEyes = GetVectorToUserAdjusted();
        float vectorToEyeDistance = vectorToEyes.magnitude;

        ///Assuming the direction is forward
        switch (depthLayer)
        {
            case DepthLayer.Single:
                position += GetOverviewMap().transform.up * 0.015f;
                break;
            case DepthLayer.Dual:
                position += GetOverviewMap().transform.up * 0.015f;
                if (map.level == 1)
                {
                    position += GetOverviewMap().transform.up * vectorToEyeDistance;
                }
                else
                {
                    position = GetProjectedPointOnOverview(map.transform.position) + GetOverviewMap().transform.up * vectorToEyeDistance * 1.015f;
                }
                break;
            case DepthLayer.Multi:
                //first max level
                if (map.level > GetMaxLevel() && map.level > 1)
                {
                    CompressDepthLayers();
                }
                else
                {
                    position += GetOverviewMap().transform.up * GetDistanceEachLevel(map.level);
                    ArrangeLayerPosition();
                }

                break;
        }

        map.transform.rotation = map.parent.transform.rotation;
        map.PlayAnimationMovement(map.viewFinder.transform.position, position);
    }

    private void ArrangeLayerPosition()
    {
        for (int i = 1; i <= GetMaxLevel(); i++)
        {
            foreach (MapLens map in GetMapsInLevel(i))
            {
                Vector3 position = GetMultiDepthMapPosition(map);
                map.PlayAnimationMovement(map.transform.position, position);
            }
        }
    }

    private Vector3 GetMultiDepthMapPosition(MapLens map)
    {
        return GetProjectedPointOnOverview(map.transform.position) + (GetVectorToPointFromOverview(map.transform.position).normalized * map.level * GetDistanceEachLevel(map.level));
    }

    private void CompressDepthLayers()
    {
        for(int i = 1; i <= GetMaxLevel(); i++)
        {
            AdjustCompressedPosition(i);
        }
    }

   
    /// <summary>
    /// Get distance based on Multilayer policy
    /// </summary>
    /// <param name="l"></param>
    /// <returns></returns>
    public float GetDistanceByLevelBeforeAddMap(int l)
    {
        int max = GetMaxLevel() + 1;
        return l * (GetVectorToUserAdjusted().magnitude / max);
    }

    ///// <summary>
    ///// Use this for Multilayer compression because GetMaxLevel has not been updated at that point
    ///// </summary>
    ///// <param name="l"></param>
    ///// <returns></returns>
    //public float GetDistanceByLevelForCompression(int l)
    //{
        
    //    return l * GetDistanceEachLevel(l);
    //}


    public float GetDistanceEachLevel(int l)
    {
        int max = GetMaxLevel();
        return (max > 0)? GetVectorToUserAdjusted().magnitude / max : GetVectorToUserAdjusted().magnitude;
    }


    /// <summary>
    /// Adjust position
    /// </summary>
    /// <param name="level"></param>
    public void AdjustCompressedPosition(int level)
    {      
        foreach(MapLens m in GetMapsInLevel(level))
        {
            m.PlayAnimationMovement(m.transform.position, GetPositionBasedOnCompressionDepth(m));
        }
    }



    private Vector3 GetPositionBasedOnCompressionDepth(MapLens map)
    {
        Vector3 position = GetProjectedPointOnOverview(map.transform.position) + (GetVectorToPointFromOverview(map.transform.position).normalized * GetDistanceByLevelBeforeAddMap(map.level));
        return position;
    }

   

    private void Update()
    {
        //Debug
        if(GetOverviewMap() != null)
        {
            Vector3 ToEyesVector = GetVectorToUserAdjusted();
            Debug.DrawRay(GetOverviewMap().transform.position, ToEyesVector, Color.blue);
        }


        //Rotation
        if(detailViewsRotation == DetailViewsRotation.LookAtUser)
        {
            foreach(MapLens m in maps)
            {
                if (m.level != 0)
                {
                    Vector3 mapEyeDirection = centerOfEyes.position - m.transform.position;
                    Vector3 project = Vector3.ProjectOnPlane(mapEyeDirection, -m.transform.forward).normalized;
                    float angle = Vector3.SignedAngle(m.transform.up, project, m.transform.forward);
                    Vector3 rotation = m.transform.rotation.eulerAngles;
                    float dot = Vector3.Dot(m.transform.up, project);
                    if(dot < 0.98f)
                    {
                      if(angle > 0)
                            m.transform.localEulerAngles += new Vector3(0, 0, 2f);
                      else
                            m.transform.localEulerAngles -= new Vector3(0, 0, 2f);
                    }
                    Debug.DrawRay(m.transform.position, project, Color.black);
                }
            }
        }
    }

    private int GetMaxLevel(List<MapLens> list)
    {
        int max = 0;
        foreach(MapLens m in list)
        {
            max = (m.level > max) ? m.level : max;
        }
        return max;
    }

    /// <summary>
    /// Get orthogonal vector from overview map to point
    /// </summary>
    /// <returns></returns>
    private Vector3 GetVectorToPointFromOverview(Vector3 point)
    {
        Vector3 projectedPoint = GetProjectedPointOnOverview(point);
        Vector3 projectedPointToHead = point - projectedPoint;
        return projectedPointToHead;
    }

    /// <summary>
    /// Return projected vector from center of overview to projected point on overview
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private Vector3 GetProjectedDirectionOnOverview(Vector3 point)
    {
        Vector3 overviewToHeadDirection = point - GetOverviewMap().center.position;
        return Vector3.ProjectOnPlane(overviewToHeadDirection, GetOverviewMap().transform.up);
    }

    /// <summary>
    /// Get the projected point on overview
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private Vector3 GetProjectedPointOnOverview(Vector3 point)
    {
        return GetOverviewMap().transform.position + GetProjectedDirectionOnOverview(point);
    }

    /// <summary>
    /// Get orthogonal vector from overview map to user eyes adjusted with comfort view distance
    /// </summary>
    /// <returns></returns>
    private Vector3 GetVectorToUserAdjusted()
    {
        Vector3 v = GetVectorToPointFromOverview(centerOfEyes.transform.position);
        float mag = v.magnitude - comfortViewDistance;
        v = v.normalized * mag;

        return v;
    }

    /// <summary>
    /// Move all maps forward
    /// </summary>
    /// <param name="f"></param>
    public void MoveForward(float f)
    {
        foreach(MapLens map in maps)
        {
            if (map != null)
            {
                map.transform.position += map.transform.up * f;
            }
        }
    }

    /// <summary>
    /// Move all maps backward
    /// </summary>
    /// <param name="f"></param>
    public void MoveBackward(float f)
    {
        foreach (MapLens map in maps)
        {
            if (map != null)
            {
                map.transform.position -= map.transform.up * f;
            }
        }
    }

    /// <summary>
    /// Get available color from color map dictionary
    /// </summary>
    /// <returns></returns>
    private Color GetAvailableColor(MapLens map)
    {
        Color c = new Color();
        foreach(var pair in colorMapLensDictionary)
        {
            if(pair.Value == null)
            {
                c = pair.Key;
                colorMapLensDictionary[c] = map;
                break;
            }
        }

        return c;
    }

    /// <summary>
    /// Set color of the map, first detail lens has different hue, the children have different brightness
    /// </summary>
    /// <param name="map"></param>
    private void SetColor(MapLens map)
    {
        Color c = new Color();
        if (IsMapConnectedToOverview(map))
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
    /// Get color brightness according to map parent's hue
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    private Color GetColorBrightness(MapLens map, bool isLevelUp)
    {
        float x = 0.05f;
        float b = x * map.level;
        Color parentColor = new Color();
        if(map.parent.viewFinder == null)
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
    /// Get overview map
    /// </summary>
    /// <returns></returns>
    private MapLens GetOverviewMap()
    {
        foreach(MapLens m in maps)
        {
            if (m.parent == null) return m;
        }

        return null;
    }
    

    /// <summary>
    /// Get position of based on the comparability value
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    protected Vector3 LocalityToPosition(MapLens map)
    {
        Vector3 finalPosition = map.transform.position;

        if(map.viewFinder != null)
        {
            Vector3 closetEdgePoint = map.parent.clipController.GetClosestEdgePoint(map.viewFinder.transform.position);
            Vector3 direction = closetEdgePoint - map.viewFinder.transform.position;

            finalPosition = map.viewFinder.transform.position + (direction * locality * map.clipController.height  * 0.5f);
        }

        return finalPosition;
    }
  
    private List<MapLens> GetMapsInLevel(int lvl, List<MapLens> list)
    {
        List<MapLens> l = new List<MapLens>();
        foreach(MapLens m in list)
        {
            if (m.level == lvl) l.Add(m);
        }
        return l;
    }
    /// <summary>
    /// |_____|______|
    /// L0    L1     L2
    /// Mapping between overview zoom and distance of detail view
    /// </summary>
    /// <param name="zoom"></param>
    /// <param name="distanceFromLayerZero"></param>
    /// <returns></returns>
    public float ZoomDistanceGain(float distanceFromLayerZero)
    {
        float overviewZoom = GetOverviewMap().abstractMap.Zoom;
        //float finalZoom = overviewZoom + (distanceFromLayerZero * depthPerLayerScale);
        float finalZoom = overviewZoom + distanceFromLayerZero; // (distanceFromLayerZero * depthPerLayerScale + distanceFromLayerZero * distanceFromLayerZero);


        Debug.Log(" map zoom = " + overviewZoom);
        Debug.Log("Overview map zoom = " + overviewZoom);
        Debug.Log("Final zoom  = " + finalZoom);
        return (finalZoom > 19) ? 19 : finalZoom;
    }

   


    /// <summary>
    /// Clear all single parents of map
    /// </summary>
    /// <param name="map"></param>
    public void ClearZoomTrail(MapLens map)
    {
        bool isStop = false;
        MapLens currentParent = map.parent;

        //trace back
        List<MapLens> deleteList = new List<MapLens>();
        while (currentParent != null && currentParent.parent != null && GetChildren(currentParent).Count <= 1)
        {
            if(currentParent.parent != null)
            {
                deleteList.Add(currentParent);
                currentParent = currentParent.parent;
            }
        }

        map.viewFinder.parent = currentParent;

        foreach (MapLens l in deleteList)
        {
            FindObjectOfType<MultiViewsManager>().Remove(l);
        }


        //while (!isStop)
        //{
        //    not single parent
        //    if (GetAllChildren(currentParent).Count > 1)
        //    {
        //        isStop = true;
        //    }
        //    else
        //    {
        //        MapLens tmpParent = currentParent.parent;
        //        if (tmpParent != null)
        //        {
        //            FindObjectOfType<MultiViewsManager>().RemoveViewPort(currentParent);
        //            currentParent = tmpParent;
        //        }
        //        else
        //        {
        //            isStop = true;
        //        }
        //    }
        //}
        //map.viewFinder.parent = currentParent;
    }
}
