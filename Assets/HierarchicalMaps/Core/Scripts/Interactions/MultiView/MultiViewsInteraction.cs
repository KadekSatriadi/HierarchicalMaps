using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MultiViewsInteraction : InteractionModule
{
    public enum InteractionStatus
    {
        NULL,
        POINTING,
        CREATE_VIEWFINDER,
        VIEWFINDER_SIZE,
        CREATE_MAPLENS,
        VIEWPORT_POSITION,
        VIEWPORT_ZOOM,
        VIEWPORT_SIZE,
        REMOVE_MAPLENS,
        LIGHTSABER,
        SELECT_VIEWPORT,
        BOOKMARK_VIEWPORT
    }
    public MultiViewsManager multiViewManager;
    public UnityEvent OnViewCreated;
    public UnityEvent OnViewRemoved;

    public InteractionStatus interactionStatus = InteractionStatus.NULL;
    protected ViewFinder currentViewFinder;
    protected Quaternion cursorRotation;
    protected MapLens hitMap;
    protected ViewFinderDrawer viewFinderDrawer;


    /// <summary>
    /// Create viewfinder drawer
    /// </summary>
    protected void CreateViewFinderDrawer()
    {
        GameObject vd = new GameObject("ViewFinderDrawer");
        vd.AddComponent<ViewFinderDrawer>();
        viewFinderDrawer = vd.GetComponent<ViewFinderDrawer>();
    }


    /// <summary>
    /// Instantiate ViewFinder
    /// </summary>
    /// <param name="position">center position and map center</param>
    /// <param name="rotation">rotation</param>
    public void CreateViewFinder(Vector3 position, Quaternion rotation)
    {
        if (hitMap != null)
        {
            currentViewFinder = multiViewManager.CreateViewfinder(hitMap, position, rotation);
            multiViewManager.SetParentMap(hitMap);
            interactionStatus = InteractionStatus.VIEWFINDER_SIZE;
        }

    }

    public void CreateViewFinder(Vector3 position, Quaternion rotation, float h, float w)
    {
        if (hitMap != null)
        {
            currentViewFinder =  multiViewManager.CreateViewfinder(hitMap, position, rotation, h, w);
            multiViewManager.SetParentMap(hitMap);
            interactionStatus = InteractionStatus.VIEWFINDER_SIZE;
        }

    }

    /// <summary>
    /// Instantiate map after viewfinder adjustment is finised
    /// </summary>
    public void CreateMapLens()
    {
        //Instantiate map
        currentViewFinder.SetSize(viewFinderDrawer.GetWidth(), viewFinderDrawer.GetHeight());
        currentViewFinder.transform.position = viewFinderDrawer.GetCenter();

        Vector3 position = currentViewFinder.transform.position;
        Quaternion rotation = currentViewFinder.transform.rotation;
        Debug.Log("Current vFinder h = " + currentViewFinder.height);
        Debug.Log("Current vFinder w = " + currentViewFinder.width);

        if(currentViewFinder.shape == ClipShape.Circle)
        {
            multiViewManager.InstantiateChildMap(position, rotation, position, currentViewFinder);
        }
        if (currentViewFinder.shape == ClipShape.Rectangle)
        {
            multiViewManager.InstantiateChildMap(position, rotation, currentViewFinder.GetRectangleCenter(), currentViewFinder);
        }

        interactionStatus = InteractionStatus.NULL;
        //Event
        OnViewCreated.Invoke();
    }


    /// <summary>
    /// Remove viewport
    /// </summary>
    public void RemoveMapLens()
    {
        if (hitMap != null)
        {
            multiViewManager.Remove(hitMap);
            //Event
            OnViewRemoved.Invoke();
            interactionStatus = InteractionStatus.NULL;
        }
    }

    public void AdjustViewFinderSize(Vector3 currentHitPosition)
    {
        Debug.Log("Drawing viewfinder ...");
        viewFinderDrawer.Draw(currentHitPosition);
    }
}
