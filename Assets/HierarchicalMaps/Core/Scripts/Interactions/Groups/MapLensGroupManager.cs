using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLensGroupManager : MonoBehaviour
{
    public void ToggleVisibility()
    {
        MapLensGroup[] maplenses = FindObjectsOfType<MapLensGroup>();
        MapLensGroupTouchRegister[] maplensesT = FindObjectsOfType<MapLensGroupTouchRegister>();

        foreach(MapLensGroup m in maplenses)
        {
            if (m.IsVisible())
            {
                m.Hide();
            }
            else
            {
                m.Show();
            }
        }

        foreach(MapLensGroupTouchRegister m in maplensesT)
        {
            if (m.IsVisible())
            {
                m.Hide();
            }
            else
            {
                m.Show();
            }
        }
    }


}
