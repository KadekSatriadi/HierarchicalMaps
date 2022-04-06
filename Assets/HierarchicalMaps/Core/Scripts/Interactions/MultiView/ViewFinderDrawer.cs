using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshLineStripRenderer))]
public class ViewFinderDrawer : MonoBehaviour
{
    protected MeshLineStripRenderer line;
    protected float offset = 0.01f;
    protected Vector3 center;
    protected float width;
    protected float heigth;

    protected void Awake()
    {
        line = GetComponent<MeshLineStripRenderer>();
        line.material = new Material(Shader.Find("Standard"));
        line.material.color = Color.black;
        line.SetPointsCount(5);
        Hide();
    }

    public void StartDraw(Vector3 position, Quaternion rotation)
    {
        transform.rotation = rotation;

        position += transform.up * offset;
        Show();
        transform.position = position;
        line.SetPosition(0, position);
        line.SetPosition(1, position);
        line.SetPosition(2, position);
        line.SetPosition(3, position);
        line.SetPosition(4, position);
        width = 0;
        heigth = 0;
    }

    public void Draw(Vector3 position)
    {
        position += transform.up * offset;
        Vector3 dir = position - transform.position;
        Vector3 top = transform.position + (Vector3.Project(dir, transform.forward));
        Vector3 bottom = transform.position + (Vector3.Project(dir, transform.right));
        line.SetPosition(2, position);
        line.SetPosition(1, top);
        line.SetPosition(3, bottom);

        center = transform.position + dir * 0.5f;
        heigth = (transform.position - top).magnitude;
        width = (transform.position  - bottom).magnitude;
    }

    public float GetWidth()
    {
        return width;
    }

    public float GetHeight()
    {
        return heigth;
    }

    public Vector3 GetCenter()
    {
        return center;
    }

    public void Hide()
    {
        line.Hide();
    }

    public void Show()
    {
        line.Show();
    }
}
