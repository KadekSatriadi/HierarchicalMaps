using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UISliderLabel : MonoBehaviour
{
    public Text text;
    public Slider slider;

    public void UpdateText()
    {
        text.text = slider.value.ToString();
    }

    public void Show()
    {
        text.enabled = true;
    }

    public void Hide()
    {
        text.enabled = false;
    }


}
