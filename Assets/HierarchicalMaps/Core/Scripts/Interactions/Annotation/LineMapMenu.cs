using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMapMenu : MonoBehaviour
{
    public Transform line;
    public void Delete()
    {
        Destroy(line.gameObject, 0f);
    }
}
