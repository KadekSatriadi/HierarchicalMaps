using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiViewsPolyZoomArrangement : MutiViewsArrangementManager
{
    public float height = 1;
    public float width = 3;
    public Transform center;
    public bool solveLocalBranchOverlaps = true;

    private List<Color> lensColors = new List<Color>();
    private Vector3 currentHCenter;
    private Vector3 currentVCenter;
    private int hSegments = 0;
    private int vSegments = 0;
    private float margin = 0.01f;
    private float overviewZoom = 0;
    private int lastMaxLevel = 0;
    private Dictionary<MapLens, Vector3> mapPositionLayout = new Dictionary<MapLens, Vector3>();
    private Dictionary<MapLens, float> mapZoomLayout = new Dictionary<MapLens, float>();
    private Dictionary<MapLens, Vector2> mapDimensionLayout = new Dictionary<MapLens, Vector2>();
    private MapLens newMap;

    private void Awake()
    {
        currentHCenter = center.position;
        currentVCenter = center.position;
        lastMaxLevel = 0;
        //override color palette
        lensColors = ColorPalette.GetColourPalette12();

        //copy color to stack
        foreach (Color c in lensColors)
        {
            colorMapLensDictionary.Add(c, null);
        }

        foreach(MapLens m in GameObject.FindObjectsOfType<MapLens>())
        {
            Register(m);
        }
    }

    /// <summary>
    /// Register map to the layout
    /// </summary>
    /// <param name="map"></param>
    public override void Register(MapLens map)
    {

        if (map.level == 0)
        {
            overviewZoom = map.abstractMap.Zoom;
        }
        else
        {
            map.transform.position = map.viewFinder.GetRectangleCenter() + map.viewFinder.transform.up * 0.015f;
        }
        newMap = map;
        maps.Add(map);
        SetColor(map);
        UpdateLayout();
    }

 
    /// <summary>
    /// Rearrange maps on the same level as the result of map sliding left or right
    /// </summary>
    /// <param name="currentMap"></param>
    public void RearrangeLevel(MapLens currentMap)
    {
        if (currentMap == null) return;
        if (!mapPositionLayout.ContainsKey(currentMap)) return;

        List<MapLens> mapList = GetMapsInLevel(currentMap.level);
        //if 1 map, bring back position
        if (mapList.Count == 1)
        {
            currentMap.transform.position = mapPositionLayout[currentMap];
            return;
        }

      
        //Get closest map on the same level
        MapLens shiftMap = null;
        foreach(MapLens m in GetMapsInLevel(currentMap.level, currentMap))
        {
            if (m == null) continue;

            if(shiftMap == null)
            {
                shiftMap = m;
            }
            else
            {
               if(Vector3.Distance(m.transform.position, currentMap.transform.position) < Vector3.Distance(shiftMap.transform.position, currentMap.transform.position))
                {
                    shiftMap = m;
                }
            }
           
        }

        //Conflict with delete
        if (shiftMap == null) return;
        if (!mapPositionLayout.ContainsKey(currentMap)) return;
        
        //if Distance is less than half widht, ignore
        float distance = Vector3.Distance(currentMap.transform.position, shiftMap.transform.position);
        if (distance > currentMap.clipController.width / 2)
        {
            currentMap.transform.position = mapPositionLayout[currentMap];
            return;
        }

        //if 2 maps, swap position
        if (mapList.Count == 2)
        {
            Vector3 newShiftMapPosition = mapPositionLayout[currentMap];
            Vector3 newCurrentMapPosition = mapPositionLayout[shiftMap];
            shiftMap.PlayAnimationMovement(newShiftMapPosition);
            currentMap.PlayAnimationMovement(newCurrentMapPosition);
            //Update dictionary
            mapPositionLayout[shiftMap] = newShiftMapPosition;
            mapPositionLayout[currentMap] = newCurrentMapPosition;
            //Update maps list
            MapLens tmp = currentMap;
            maps[maps.IndexOf(currentMap)] = shiftMap;
            maps[maps.IndexOf(shiftMap)] = currentMap;
            return;
        }

        //Define shift direction, if index current map less higher than shift map, then shift left
        bool isShiftRight = true;
        if (mapList.IndexOf(currentMap) < mapList.IndexOf(shiftMap)) isShiftRight = false;

        //Shift position
        bool isShifting = true;
        int i = mapList.IndexOf(currentMap);
        //Need these dics and list to update current maps and dics
        Dictionary<MapLens, Vector3> newMapPosition = new Dictionary<MapLens, Vector3>();
        Dictionary<MapLens, int> newMapIndex = new Dictionary<MapLens, int>();
        List<MapLens> newMaplist = new List<MapLens>();
        while (isShifting)
        {
            MapLens m = mapList[i];
            Vector3 newPos = m.transform.position;
            int newIdx = i;
            if (m == currentMap)
            {
                newPos = mapPositionLayout[shiftMap];
                newIdx = maps.IndexOf(shiftMap);
            }
            else
            {
                if (isShiftRight)
                {
                    newPos = mapPositionLayout[mapList[i + 1]];
                    newIdx = maps.IndexOf(mapList[i + 1]);
                }
                else
                {
                    newPos = mapPositionLayout[mapList[i - 1]];
                    newIdx = maps.IndexOf(mapList[i - 1]);
                }
            }

            m.PlayAnimationMovement(newPos);

            newMapPosition.Add(m, newPos);
            newMapIndex.Add(m, newIdx);
            newMaplist.Add(m);

            if (i == mapList.IndexOf(shiftMap)) isShifting = false;
            i = (isShiftRight)? i - 1 : i + 1;
        }

        //Update dictionary and list
        foreach(MapLens m in newMaplist)
        {
            mapPositionLayout[m] = newMapPosition[m];
            int idx = newMapIndex[m];
            maps[idx] = m;
        }
    }


    /// <summary>
    /// Update layout out the maps
    /// </summary>
    private void UpdateLayout()
    {
        //Reset dictionary
        mapPositionLayout = new Dictionary<MapLens, Vector3>();
        //Update vertical segment
        vSegments = GetMaxLevel() + 1;  
        
        //UPDATE POSITIONS AND ZOOM LEVELS
        //Go through all levels
        for (int lvl = 0; lvl <= GetMaxLevel(); lvl++)
        {
            //Get new heigh, height is constant accross all levels
            float newHeight = height / vSegments;
            //Since height is constant, the width should be adjusted to maintain the wall aspect ratio
            float newWidth = ((newHeight / height) * width);
            //Get horizontal segment for each of the level (i.e. the number of maps in that level)
            int hseg = GetMapsInLevel(lvl).Count;
            //If there is only one map, put it in the center by changin the number of line segment to 2
            hseg = (hseg == 1)? 2 : hseg;
            //counter for odd number counting, 1, 3, 5, 7
            int c = 1;
            //count
            int count = GetMapsInLevel(lvl).Count;
            //Update each map
            foreach (MapLens m in GetMapsInLevel(lvl))
            {

                #region DealingWithDimension
                //if the total width is more than wall width, clip it
                if ((newWidth - margin) * count > width)
                {
                    newWidth = width / hseg;
                }
                //margined dimension
                float newHeightMargined = newHeight - margin;
                float newWidthMargined = newWidth - margin;

                //First, adjust the viewport dimension to obey the aspect ratio or clipped
                if(m.viewFinder != null)
                {
                    float h = m.viewFinder.height;
                    float w = m.viewFinder.width;
                    float aspectRation = newWidthMargined / newHeightMargined;
                    //if w > h, adjust h
                    if (w > h)
                    {
                        h = w / aspectRation;
                    }
                    else
                    {
                        w = h * aspectRation;
                    }
                    //Update viewfinder and map dimension
                    m.viewFinder.PlayAnimateRectangleDimension(h, w);
                    //m.SetRectangleDimension(h, w);
                }
               
               
                //Update to dictionary
                Vector2 newDimension = new Vector2(newHeightMargined, newWidthMargined);
                if (mapDimensionLayout.ContainsKey(m))
                {
                    mapDimensionLayout[m] = newDimension;
                }
                else
                {
                    mapDimensionLayout.Add(m, newDimension);
                }
                #endregion

                #region DealingWithPosition
                //Get the top center position of the wall
                Vector3 topPoint = center.position + center.up * height / 2;
                //First, determine vertical position on the wall
                Vector3 newPosition = topPoint;
                if (vSegments > 1)
                {
                    newPosition -= center.up * ((lvl * newHeight)  + (newHeight * 0.5f));
                }
                //First map keep on the center
                else
                {
                    newPosition = center.position;
                }
                //Then, horizontal position, first get the right most point on the wall then adjust previous vertical position if map is more than 1
                Vector3 leftPoint = newPosition - center.right * count * 0.5f * newWidth;
                if(count > 1)
                {
                    float half = newWidth / 2;
                    newPosition = leftPoint + center.right * c * half; 
                }
                //Lastly, update the map position on dictionary
                if (mapPositionLayout.ContainsKey(m))
                {
                    mapPositionLayout[m] = newPosition;
                }
                else
                {
                    mapPositionLayout.Add(m, newPosition);

                }
                #endregion

                #region DealingWithZoomLevel
                //The initial zoom level of the map is the same as its parent
                float newZoom = m.abstractMap.Zoom;

                ////If newly added map is the same, adjust it's zoom level based on the viewport size
                //if (m.level > 0 && newMap == m)
                //{

                //    //m.PlayScaleByDimensionAnimate(0, newHeightMargined, newWidthMargined, delegate {
                //    //    m.PlayWidthHeightAnimate(newHeightMargined, newWidthMargined);
                //    //});

                //    //Get new zoom
                //    newZoom = m.GetZoomByNewDimension(newHeightMargined, ClipShape.Rectangle);
                //    //Update dictionary


                //    newMap = null;
                //}
                //else if (newMap != m && lastMaxLevel != GetMaxLevel())
                //{
                //    newZoom = m.GetZoomByNewDimension(newHeightMargined, ClipShape.Rectangle);
                //    //m.PlayScaleByDimensionAnimate(0, newHeightMargined, newWidthMargined, delegate {
                //    //    m.PlayWidthHeightAnimate(newHeightMargined, newWidthMargined);
                //    //});
                //}
                //else
                //{
                //    m.PlayWidthHeightAnimate(newHeightMargined, newWidthMargined);
                //}

                //Update new zoom
                if (newMap == m && m.level > 0)
                {
                    newZoom = m.GetZoomByNewDimension(newHeightMargined, m.clipController.height, ClipShape.Rectangle);
                    newMap = null;
                }
                else if (newMap != m && lastMaxLevel != GetMaxLevel())
                {
                    newZoom = m.GetZoomByNewDimension(newHeightMargined, m.clipController.height, ClipShape.Rectangle);
                }

                //Update dictionary
                if (mapZoomLayout.ContainsKey(m))
                {
                    mapZoomLayout[m] = newZoom;
                }
                else
                {
                    mapZoomLayout.Add(m, newZoom);
                }
                #endregion

                c += 2;              
            }
        }

        #region CheckAndSolveLocalBranchOverlap
        if (solveLocalBranchOverlaps)
        {
            DetechAndSolveLocalBranchOverlap();
        }
        #endregion

        //UPDATE MAP POSITION DELAYED 0.5 seconds
        for (int lvl = 0; lvl <= GetMaxLevel(); lvl++)
        {
            UpdateMaps(lvl);
        }
        

        lastMaxLevel = GetMaxLevel();

    }

    /// <summary>
    /// Update map position, dimension, and scale with 0.5f delayed
    /// </summary>
    public void UpdateMaps(int lvl)
    {
        foreach (MapLens m in GetMapsInLevel(lvl))
        {
            //First move forward a little
            m.transform.position += m.transform.up * 0.05f;
            // m.PlayZoomAnimation(mapZoomLayout[m]);
            Debug.Log("Zoom " + mapZoomLayout[m]);
            if (!float.IsInfinity(mapZoomLayout[m])) m.Zoom(mapZoomLayout[m]);
            m.PlayAnimationMovement(mapPositionLayout[m]);
            m.PlayHeightWidthAnimate(mapDimensionLayout[m].x, mapDimensionLayout[m].y);
        }
    }

    IEnumerator MoveDelay(MapLens m, Vector3 z)
    {
        yield return new WaitForSeconds(1f);
        m.PlayAnimationMovement(z);
    }

    /// <summary>
    /// Solve Local Branch Crossing if there is any
    /// </summary>
    public void DetechAndSolveLocalBranchOverlap()
    {
        for (int lvl = 0; lvl <= GetMaxLevel(); lvl++)
        {
            foreach (MapLens m in GetMapsInLevel(lvl))
            {
                RemoveLocalCrossing(m);
            }
        }
    }


    private void SwapMapListPosition(MapLens m1, MapLens m2)
    {
        List<MapLens> l = new List<MapLens>();
        foreach(MapLens m in maps)
        {
            if(m == m1)
            {
                l.Add(m2);
            }else if (m == m2)
            {
                l.Add(m1);
            }
            else
            {
                l.Add(m);
            }
        }

        maps = l;
    }

    private void PostMoveActions(MapLens m)
    {
        if(m != null && m.viewFinder != null)
        {
            m.viewFinder.HideVisualLink();
        }       
    }

    protected int GetMaxNumberOfLeaf()
    {
        int n = 0;
        for(int i = 0; i <= GetMaxLevel(); i++)
        {
            n = n < GetMapsInLevel(i).Count? GetMapsInLevel(i).Count: n;
        }

        return n;
    }

    /// <summary>
    /// Check and Remove crossings
    /// </summary>
    public void RemoveLocalCrossing(MapLens m)
    {
        List<MapLens> children = GetChildren(m);
        List<ViewFinder> viewfinders = GetViewFinders(m);

        if (children == null || children.Count <= 1) return;
        if (viewfinders == null || viewfinders.Count <= 1) return;

        //Order view finder from left to right
        List<ViewFinder> viewFinderOrdered = OrderViewFinders(viewfinders);

        //Loop through all viewfinder
        foreach(ViewFinder v in viewFinderOrdered)
        {

            int idx = viewFinderOrdered.IndexOf(v);

            //Order map
            List<MapLens> mapOrder = OrderMaps(GetChildren(m));

            //Get map connected to viewfinder
            MapLens childMap = v.child;
            
            //Get map in position
            MapLens mapOnIdx = mapOrder[idx];

            //if viewfinder index and map index is not the same, there is crossing
            if(childMap != mapOnIdx)
            {
                Debug.Log("Crossing detected");
                Debug.Log("Current map " + mapOnIdx.name);
                Debug.Log("Correct map " + childMap);
                //swap map position with the map on index start at mapOrder
                SwapPosition(childMap, mapOnIdx);
            }
        }
    }

    private void SwapPosition(MapLens m1, MapLens m2)
    {
        Vector3 m1Pos = mapPositionLayout[m1];
        Vector3 m2Pos = mapPositionLayout[m2];
        mapPositionLayout[m1] = m2Pos;
        mapPositionLayout[m2] = m1Pos;
        SwapMapListPosition(m1, m2);
    }

    private List<MapLens> OrderMaps(List<MapLens> children)
    {
        List<Vector3> positions = new List<Vector3>();
        //get position from dictionary
        foreach (MapLens m in children)
        {
            if(m!= null)   positions.Add(mapPositionLayout[m]);
        }

        positions = ShortVector3sFromLToR(positions, GetRootMap().transform);

        //Reverse position to maps
        List<MapLens> ms = new List<MapLens>();
        foreach(Vector3 pos in positions)
        {
            foreach (MapLens m in children)
            {
                if(mapPositionLayout[m] == pos)
                {
                    ms.Add(m);
                    continue;
                }
            }
        }
       
        return ms;
    }

    private List<ViewFinder> OrderViewFinders(List<ViewFinder> viewfinders)
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (ViewFinder v in viewfinders)
        {
            positions.Add(v.transform.position);
        }

        positions = ShortVector3sFromLToR(positions, viewfinders[0].parent.transform);

        //Reverse position to maps
        List<ViewFinder> ms = new List<ViewFinder>();
        foreach (Vector3 pos in positions)
        {
            foreach (ViewFinder v in viewfinders)
            {
                if (v.transform.position == pos)
                {
                    ms.Add(v);
                    continue;
                }
            }
        }

        return ms;
    }

    private List<Vector3> ShortVector3sFromLToR(List<Vector3> points, Transform parent)
    {
        List<Vector3> orderedVector = new List<Vector3>();

        //Very far left point
        Vector3 leftPoint = parent.position  - parent.right * 100;
        Vector3 rightPoint = parent.position + parent.right * 100;

        //Long vector
        Vector3 leftToRightVector = rightPoint - leftPoint;

        Dictionary<float,  Vector3> vectorDistance = new Dictionary<float, Vector3>();

        List<float> distanceList = new List<float>();

        //Calculate distance
        foreach(Vector3 t in points)
        {
            Vector3 leftToT = t - leftPoint;
            Vector3 projectVector = Vector3.Project(leftToT, leftToRightVector);
            if (vectorDistance.ContainsKey(projectVector.magnitude))
            {
                projectVector *= projectVector.magnitude + 0.0001f;
            }
            vectorDistance.Add(projectVector.magnitude, t);
            distanceList.Add(projectVector.magnitude);
        }

        //Short!
        distanceList = InsertionShort(distanceList);
        foreach(float d in distanceList)
        {
            orderedVector.Add(vectorDistance[d]);
        }

        return orderedVector;
    }

    /// <summary>
    /// https://www.tutorialspoint.com/insertion-sort-in-chash
    /// </summary>
    private List<float> InsertionShort(List<float> arr)
    {
        float val;
        int n = arr.Count;
        int i, j, flag;
        for (i = 1; i < n; i++)
        {
            val = arr[i];
            flag = 0;
            for (j = i - 1; j >= 0 && flag != 1;)
            {
                if (val < arr[j])
                {
                    arr[j + 1] = arr[j];
                    j--;
                    arr[j + 1] = val;
                }
                else flag = 1;
            }
        }
        return arr;
    }

   

    public override void Remove(MapLens map)
    {
        int level = map.level;
        List<MapLens> children = GetChildren(map);
        List<MapLens> list = new List<MapLens>();
        MapLens parent = map.parent;
       
        //If there is one level without maps, upgrade the next following level
        //if (GetMapsInLevel(level).Count == 1 && GetMapsInLevel(level + 1).Count > 0)
        //{
        //    for (int i = level + 1; i <= GetMaxLevel(); i++)
        //    {
        //        foreach (MapLens m in GetMapsInLevel(i))
        //        {
        //            m.level--;
        //            m.parent = parent;
        //            SetColor(m);
        //        }
        //    }
        //}


        ////If removed map has children, upgrade its children to remove level crossing
        //if (children != null && children.Count > 0)
        //{
        //    foreach (MapLens m in children)
        //    {
        //        if (m != null)
        //        {
        //            m.level--;
        //            m.parent = parent;
        //            SetColor(m);
        //        }
        //    }
        //}

        //update map list
        foreach (MapLens m in maps)
        {
            if (m != map)
            {
                list.Add(m);
            }
        }
        mapPositionLayout.Remove(map);
        maps = list;

        UpdateLayout();
    }


}
