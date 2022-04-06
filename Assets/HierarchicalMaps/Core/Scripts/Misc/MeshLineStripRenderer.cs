using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class MeshLineStripRenderer : MonoBehaviour
{
    public Material material;
    public Vector3[] points;

    protected MeshRenderer render;
    protected MeshFilter filter;
    
    private void Awake()
    {
        render = GetComponent<MeshRenderer>();
        filter = GetComponent<MeshFilter>(); 
    }

    public void SetActive(bool active)
    {
        render.enabled = active;
    }

    protected void DrawMesh()
    {
        int[] indices = new int[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            indices[i] = i;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = points;
        mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
        mesh.RecalculateBounds();

        filter.mesh = mesh;
        render.material = material;
    }

    protected void Update()
    {
        if(points.Length > 1)
        {
            DrawMesh();
        }
    }

    public void SetPosition(int idx, Vector3 pos)
    {
        points[idx] = transform.InverseTransformPoint(pos);
    }

    /// <summary>
    /// Set count. Warning! Reset points
    /// </summary>
    /// <param name="count"></param>
    public void SetPointsCount(int count)
    {
        points = new Vector3[count];
    }

    public void Hide()
    {
       if(render) render.enabled = false;
    }

    public void Show()
    {
        if(render) render.enabled = true;
    }
}
