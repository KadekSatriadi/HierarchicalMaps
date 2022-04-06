using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiViewsInteractionDesktop : MultiViewsInteraction
{
    Camera mainCamera;
    MapLens map;
    Vector3 lastHitPosition;
    Quaternion lastHitRotation;
    private void Start()
    {
        mainCamera = Camera.main;
        CreateViewFinderDrawer();
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("No main camera");
            return;
        }

        if(hitMap != null)
        {
            multiViewManager.SetParentMap(hitMap);
        }

        switch (interactionStatus)
        {
            case InteractionStatus.NULL:
                Idle();
                break;
            case InteractionStatus.POINTING:
                Pointing();
                break;
            case InteractionStatus.VIEWFINDER_SIZE:
                ViewFinderSize();
                break;
            case InteractionStatus.CREATE_VIEWFINDER:
                CreateViewFinder(lastHitPosition, lastHitRotation);
                viewFinderDrawer.StartDraw(lastHitPosition, lastHitRotation);
                break;
            case InteractionStatus.CREATE_MAPLENS:
                CreateMapLens();
                viewFinderDrawer.Hide();
                break;
            case InteractionStatus.VIEWPORT_POSITION:
                break;
            case InteractionStatus.VIEWPORT_ZOOM:
                break;
            case InteractionStatus.VIEWPORT_SIZE:
                break;
            case InteractionStatus.LIGHTSABER:
                break;
            case InteractionStatus.REMOVE_MAPLENS:
                break;
        }
    }

    void Idle()
    {
        //pointing
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            hitMap = hit.transform.GetComponentInChildren<MapLens>();
            if (hitMap == null)
            {
                interactionStatus = InteractionStatus.NULL;
            }
            else
            {
                interactionStatus = InteractionStatus.POINTING;
            }
        }
        else
        {
            interactionStatus = InteractionStatus.NULL;
        }
    }

   void ViewFinderSize()
    {
        //pointing
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            hitMap = hit.transform.GetComponentInChildren<MapLens>();
            lastHitPosition = hit.point;
            lastHitRotation = hit.transform.rotation;

            viewFinderDrawer.Show();

            AdjustViewFinderSize(lastHitPosition);
            //mouse down
            if (Input.GetMouseButtonUp(1))
            {
                if (interactionStatus == InteractionStatus.VIEWFINDER_SIZE)
                {
                    interactionStatus = InteractionStatus.CREATE_MAPLENS;
                }
            }
        }
          
    }
    void Pointing()
    {
        //pointing
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            hitMap = hit.transform.GetComponentInChildren<MapLens>();
            lastHitPosition = hit.point;
            lastHitRotation = hit.transform.rotation;

            //go outside the map
            if (hitMap == null) interactionStatus = InteractionStatus.NULL;
            
            //mouse down
            if (Input.GetMouseButtonDown(1))
            {
                if(interactionStatus == InteractionStatus.POINTING)
                {
                    interactionStatus = InteractionStatus.CREATE_VIEWFINDER;
                }
                else if(interactionStatus == InteractionStatus.VIEWFINDER_SIZE)
                {
                    interactionStatus = InteractionStatus.CREATE_MAPLENS;
                }
            }
        }
        else
        {
            interactionStatus = InteractionStatus.NULL;
        }
    }
}
