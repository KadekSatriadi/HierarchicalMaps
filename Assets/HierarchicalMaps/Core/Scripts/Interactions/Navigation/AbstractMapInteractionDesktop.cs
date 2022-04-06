using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractMapInteractionDesktop : AbstractMapInteraction
{
    Camera mainCamera;
    Vector3 previousPosition = Vector3.negativeInfinity;
    float zoomSpeedFactor = 0.01f;
    private void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    private new void Update()
    {
        if (!enable) return;

        if(mainCamera != null)
        {
            //mouse down
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit))
                {
                    AbstractMap abstractMap = hit.transform.root.GetComponentInChildren<AbstractMap>();
                    if (abstractMap)
                    {
                        map = abstractMap;
                    }
                    previousPosition = hit.point;

                }
            }

            //mouse hold
            if (Input.GetMouseButton(0))
            {
                Debug.Log("Hold mouse");
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit))
                {
                    AbstractMap abstractMap = hit.transform.root.GetComponentInChildren<AbstractMap>();
                    if (abstractMap)
                    {
                        Debug.Log("Panning");

                        map = abstractMap;
                        Vector3 direction = (previousPosition - hit.point);
                        Pan(direction);
                        previousPosition = hit.point;
                    }

                }
                
            }

            //mouse up
            if(Input.GetMouseButtonUp(0))
            {
                previousPosition = Vector3.negativeInfinity;
            }

            float mouseScroll = Input.mouseScrollDelta.y;
            if(mouseScroll * mouseScroll > 0)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit))
                {
                    AbstractMap abstractMap = hit.transform.root.GetComponentInChildren<AbstractMap>();
                    if (abstractMap)
                    {
                        map = abstractMap;
                    }

                    if(mouseScroll > 0)
                    {
                        Zoom(hit.point, zoomSpeed * zoomSpeedFactor);
                    }
                    if(mouseScroll < 0)
                    {
                        Zoom(hit.point, -zoomSpeed * zoomSpeedFactor);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No main camera!");
        }

        //keyboard
        if(map != null)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            if (x != 0 || y != 0)
            {
                Debug.Log(x + "," + y);
                Pan(new Vector3(x,y,0));
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Zoom(0.1f);
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                Zoom(-0.1f);
            }
        }
        else
        {
            Debug.LogWarning("No abstract map!");
        }

    }
}
