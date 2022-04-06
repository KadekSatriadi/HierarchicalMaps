using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITween : MonoBehaviour
{
    public enum UITweenType
    {
        Scale,
        Position
    }

    public Vector3 start;
    public Vector3 end;
    public float duration;
    public UITweenType tweenType;

    public GameObject gameObject;
    public AnimationCurve curve;

    private bool isPlaying = false;
    private void Start()
    {
        Play();
    }

    IEnumerator Tween(Vector3 s, Vector3 e)
    {
        float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);
            float curvePercent = curve.Evaluate(percent);
            Vector3 current = Vector3.LerpUnclamped(s, e, curvePercent);

            switch (tweenType)
            {
                case UITweenType.Position:
                    gameObject.transform.position = current;
                    break;
                case UITweenType.Scale:
                    gameObject.transform.localScale = current;
                    break;
            }
            yield return null;
        }

        isPlaying = false;

    }

    void ResetObject()
    {
        switch (tweenType)
        {
            case UITweenType.Position:
                gameObject.transform.position = start;
                break;
            case UITweenType.Scale:
                gameObject.transform.localScale = start;
                break;
        }
    }

    public void ReversePlay()
    {
        Play(end, start);
    }

    public void Play(Vector3 s, Vector3 e)
    {
        if (!isPlaying)
        {
            isPlaying = true;
            StartCoroutine(Tween(s, e));
        }
    }

    public void Play()
    {
        Play(start, end);
    }
}
