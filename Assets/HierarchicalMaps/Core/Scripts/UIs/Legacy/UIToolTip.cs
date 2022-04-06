using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class UIToolTip : MonoBehaviour
{
    protected Vector3 pointPosition;
    public Vector3 offset;
    public Text textArea;

    protected LineRenderer line;

    private void Start()
    {
        Initiate();
    }

    private void Update()
    {
        UpdatePosition();
    }

    protected void Initiate()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        Hide();
    }

    protected virtual void UpdatePosition()
    {
        if (line != null)
        {
            line.SetPositions(new Vector3[2]
            {
                pointPosition, pointPosition + offset
            });
        }
        transform.position = pointPosition + offset;
    }

    public virtual void SetPosition(Vector3 pos)
    {
        pointPosition = pos;
    }

    public void Hide()
    {
        foreach(Transform r in GetComponentsInChildren<Transform>(true))
        {
             r.gameObject.SetActive(false);
        }
    }

    public virtual void Show()
    {
        foreach (Transform r in GetComponentsInChildren<Transform>(true))
        {
             r.gameObject.SetActive(true);
        }
    }

    public void SetText(Dictionary<string, object> record)
    {
        string text = "";
        foreach (KeyValuePair<string, object> field in record)
        {
            text += field.Key + ": " + field.Value + " \n";
        }

        textArea.text = text;
    }

    public void SetText(string text)
    {
        textArea.text = text;
    }


}
