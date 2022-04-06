using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLensGroup : MonoBehaviour
{
    public AnimationCurve animationCurve;

    protected List<Transform> maplenses = new List<Transform>();
    protected List<LineRenderer> lines = new List<LineRenderer>();

    protected bool visibility = true;
    protected MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        foreach(Transform m in maplenses)
        {
            if(m != null)
            {
                lines[maplenses.IndexOf(m)].positionCount = 2;
                lines[maplenses.IndexOf(m)].SetPosition(0, transform.position);
                lines[maplenses.IndexOf(m)].SetPosition(1, m.transform.position);
            }
        }
    }

    public void Register(Transform map)
    {
        if (!maplenses.Contains(map))
        {
            maplenses.Add(map);
            map.transform.SetParent(transform);
            CenterGroupAnchor();
            CreateLine();
        }
    }
    
    private void CreateLine()
    {
        GameObject lineRenderer = new GameObject("Line");
        lineRenderer.AddComponent<LineRenderer>();
        LineRenderer line = lineRenderer.GetComponent<LineRenderer>();
        line.material = GetComponent<MeshRenderer>().material;
        line.widthMultiplier = 0.01f;
        lines.Add(line);
        lineRenderer.transform.SetParent(transform);
    }

    /// <summary>
    /// Tween animation
    /// </summary>
    /// <param name="s">start position</param>
    /// <param name="e">end position</param>
    /// <param name="map">map gameobject</param>
    /// <returns></returns>
    private IEnumerator MovementTween(Vector3 s, Vector3 e, Action a)
    {
        float journey = 0f;
        float animtime = 0.2f;
        while (journey <= animtime)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animtime);
            float curvePercent = animationCurve.Evaluate(percent);
            Vector3 current = Vector3.LerpUnclamped(s, e, curvePercent);
            transform.position = current;
            yield return null;
        }
        a.Invoke();
    }

    public void Register(List<Transform> lenses)
    {
       foreach(Transform m in lenses)
        {
            Register(m);
        }
    }

    public void Remove(Transform map)
    {
        lines.RemoveAt(maplenses.IndexOf(map));
        maplenses.Remove(map);
      
    }

    public Vector3 GetCenterPoint()
    {
        Vector3 sum = Vector3.zero;
        foreach(Transform m in maplenses)
        {
            sum += m.transform.position;
        }

        return sum / maplenses.Count;
    }

    public void Hide()
    {
        foreach(LineRenderer r in GetComponentsInChildren<LineRenderer>())
        {
            r.enabled = false;
        }
        meshRenderer.enabled = false;
        visibility = false;
    }

    public void Show()
    {
        foreach (LineRenderer r in GetComponentsInChildren<LineRenderer>())
        {
            r.enabled = true;
        }
        meshRenderer.enabled = true;
        visibility = true;
    }

    public bool IsVisible()
    {
        return visibility;
    }

    public void CenterGroupAnchor()
    {
        if (maplenses.Count <= 1) return;

        foreach (Transform m in maplenses)
        {
            m.transform.parent = null;
        }

        Vector3 pos = GetCenterPoint();
        StartCoroutine(MovementTween(transform.position, pos, delegate {
            foreach (Transform m in maplenses)
            {
                m.transform.SetParent(transform);
            }
        }));

    }

    public void HideAnchor()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    public void ShowAnchor()
    {
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }


}
