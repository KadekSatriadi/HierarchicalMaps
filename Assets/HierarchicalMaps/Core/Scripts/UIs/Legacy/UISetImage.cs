using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISetImage : MonoBehaviour
{
    public Image imageContainer;
   
    public void SetImage(Sprite img)
    {
        imageContainer.sprite = img;
    }
}
