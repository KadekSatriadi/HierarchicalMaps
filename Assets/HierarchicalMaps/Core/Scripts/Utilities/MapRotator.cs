using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRotator : MonoBehaviour
{
    public Vector3 targetRotationEuler;
    public Vector3 targetPosition;
    public bool keepPosition;

    public MapLens map;

    private Quaternion initialRotation;
    private Quaternion targetRotation;
    private Vector3 initialPosition;

    private Quaternion beforeRotation;
    private Quaternion afterRotation;

    private bool isRotated = false;
    // Start is called before the first frame update
    void Start()
    {
        initialRotation = map.transform.rotation;
        targetRotation = Quaternion.Euler(targetRotationEuler);
        initialPosition = map.transform.position;
    }

    public void Apply()
    {
        map.PlayAnimationRotation(initialRotation, targetRotation);
        map.PlayAnimationMovement(targetPosition, delegate {
            isRotated = true;
        });
    }

    public void ToggleLookUp()
    {
        if (isRotated)
        {
            map.PlayAnimationRotation(afterRotation, beforeRotation, delegate {
                isRotated = false;
            });
        }
        else
        {
            beforeRotation = map.transform.rotation;

            Vector3 relativePos = Camera.main.transform.position - map.transform.position;

            afterRotation = Quaternion.LookRotation(relativePos, Vector3.up);

            map.PlayAnimationRotation(beforeRotation, afterRotation, delegate {
                isRotated = false;
            });
        }
    }

    public void ToggleRotate()
    {
        if (isRotated)
        {
            if (keepPosition)
            {
                map.PlayAnimationRotation(targetRotation, initialRotation, delegate {
                    isRotated = false;
                });

            }
            else
            {
                map.PlayAnimationMovement(initialPosition, delegate {
                    isRotated = false;
                });
            }

        }
        else
        {
            if (keepPosition)
            {
                map.PlayAnimationRotation(initialRotation, targetRotation, delegate {
                    isRotated = true;
                });

            }
            else
            {
                map.PlayAnimationRotation(initialRotation, targetRotation);
                map.PlayAnimationMovement(targetPosition, delegate {
                    isRotated = true;
                });
            }
           
        }
    }
}
