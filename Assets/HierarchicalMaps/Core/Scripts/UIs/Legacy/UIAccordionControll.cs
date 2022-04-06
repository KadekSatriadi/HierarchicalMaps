using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAccordionControll : MonoBehaviour
{
    public Vector2 range;
    public Vector2 domain;
    public GameObject handle;

    private float currentValue;


    private void Start()
    {

    }

    public void SetValue(float c)
    {
        currentValue = c;
    }

    public void Slide(float v)
    {
        currentValue = GetDomainValue(currentValue + v);
    }

    float GetDomainValue(float rangeValue)
    {
        return domain.x + (((rangeValue - range.x)/(range.y - range.x)) * (domain.y - domain.x));
    }
}
