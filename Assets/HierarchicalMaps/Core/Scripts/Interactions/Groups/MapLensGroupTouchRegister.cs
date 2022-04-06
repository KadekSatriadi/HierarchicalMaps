using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapLensGroupTouchRegister : MonoBehaviour
{
    public MapLensGroup mapLensGroup;
    public AnimationCurve animationCurve;
    public TextMeshPro textHelp;

    protected Vector3 startScale;
    protected bool active = false;
    protected float animationTime = 0.25f;
    protected bool isVisible = true;

    private void Awake()
    {
        startScale = transform.localScale;
        Shrink();
        textHelp.text = "Grab to select";
    }

    public void SetText(string t)
    {
        textHelp.text = t;
    }

    public void Hide()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        isVisible = false;
    }

    public void Show()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }
        isVisible = true;
    }

    public bool IsVisible()
    {
        return isVisible;
    }

    public void Shrink()
    {
        StartCoroutine(ScaleTween(startScale, startScale * 0.8f));
    }

    public void HideDelay()
    {
        StartCoroutine(HideDelayStart());
    }

    IEnumerator HideDelayStart()
    {
        yield return new WaitForSeconds(3f);
        if(!active) MoveToHide();
    }

    public void MoveToHide()
    {
        textHelp.text = "";
        Vector3 pos = mapLensGroup.transform.position;
        StartCoroutine(MovementTween(transform.localPosition, mapLensGroup.transform.InverseTransformPoint(pos)));
    }

    public void Expand()
    {
        StartCoroutine(ScaleTween(startScale * 0.8f, startScale));
    }

    public void SetActive(bool a)
    {
        active = a;
    }


    /// <summary>
    /// Tween animation
    /// </summary>
    /// <param name="s">start position</param>
    /// <param name="e">end position</param>
    /// <param name="map">map gameobject</param>
    /// <returns></returns>
    private IEnumerator MovementTween(Vector3 s, Vector3 e)
    {
        float journey = 0f;
        while (journey <= animationTime)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationTime);
            float curvePercent = animationCurve.Evaluate(percent);
            Vector3 current = Vector3.LerpUnclamped(s, e, curvePercent);
            transform.localPosition = current;
            yield return null;
        }
    }

    private IEnumerator ScaleTween(Vector3 s, Vector3 e)
    {
        float journey = 0f;
        while (journey <= animationTime)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / animationTime);
            float curvePercent = animationCurve.Evaluate(percent);
            Vector3 current = Vector3.LerpUnclamped(s, e, curvePercent);
            transform.localScale = current;
            yield return null;
        }
    }


    public void ReturnToPositon()
    {
        textHelp.text = "Grab to select";
        Vector3 pos = mapLensGroup.transform.position + mapLensGroup.transform.up * (mapLensGroup.transform.lossyScale.y + 0.001f);
        StartCoroutine(MovementTween(transform.localPosition, mapLensGroup.transform.InverseTransformPoint(pos)));
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!active) return;

        MapLens map = other.transform.root.gameObject.GetComponentInChildren<MapLens>();
        MapLensGroup group = other.gameObject.GetComponent<MapLensGroup>();
        if (map != null || group != null)
        {
            mapLensGroup.Register(other.transform.root);
        }
    }

 
}
