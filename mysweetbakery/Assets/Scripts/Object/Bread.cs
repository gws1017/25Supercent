using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bread : MonoBehaviour
{
    [Header("Curves")]
    [SerializeField] private AnimationCurve flyCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve heightCurve =
        new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.35f, 1), new Keyframe(1, 0));

    [Header("Height")]
    [SerializeField] private float minHeight = 0.15f;
    [SerializeField] private float maxHeight = 0.9f;

    private const float elapsed = 0.0001f; //0 ¹æÁö¿ë
    private Coroutine flyCorutine;

    public void FlyTo(Transform target, float duration, Action onArrive = null, bool scaleByCurve = true)
    {
        if (flyCorutine != null) StopCoroutine(flyCorutine);
        OffPhysics();
        flyCorutine = StartCoroutine(FlyRoutine(target, duration, onArrive, scaleByCurve));
    }

    private void OffPhysics()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb == null) return;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        var col = GetComponent<Collider>();
        if (col == null) return;
        col.enabled = false;
    }

    IEnumerator FlyRoutine(Transform target, float duration, Action onArrive, bool scaleByCurve)
    {
        if (!target) yield break;

        Vector3 start = transform.position;
        Vector3 end = target.position;

        float dist = Vector3.Distance(start, end);
        float height = Mathf.Max(minHeight, maxHeight * dist);

        float t = 0;
        float inv = 1f / Mathf.Max(elapsed, duration);

        while (t < 1f)
        {
            t += Time.deltaTime * inv;
            float u = Mathf.Clamp01(t);

            float p = flyCurve.Evaluate(u);
            float h = height * heightCurve.Evaluate(u);

            Vector3 pos = Vector3.Lerp(start, end, p) + Vector3.up * h;
            transform.position = pos;

            if (scaleByCurve)
            {
                float s = heightCurve.Evaluate(u);
                transform.localScale = Vector3.one * s;
            }
            yield return null;
        }

        onArrive?.Invoke();
    }
}
