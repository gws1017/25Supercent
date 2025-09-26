using System;
using System.Collections;
using UnityEngine;

public class Money : MonoBehaviour
{
    private MoneyDummy owner;

    [Header("Curves")]
    [SerializeField] private AnimationCurve flyCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("포물선 이동 커브")]
    [SerializeField] private AnimationCurve heightCurve =
        new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.35f, 1), new Keyframe(1, 0));

    [Header("Height Value")]
    [SerializeField] float maxHeight = 0.35f;
    [SerializeField] float minHeight = 0.15f;

    private const float elapsed = 0.0001f; //0 방지용

    public void SetOwner(MoneyDummy dummy) => owner = dummy;

    public void Fly(Transform target, float duration, Action onArrive)
    {
        StartCoroutine(FlyRoutine(target, duration, onArrive));
    }

    private IEnumerator FlyRoutine(Transform target, float duration, Action onArrive)
    {
        Vector3 start = transform.position;
        Vector3 end = target.position;

        float dist = Vector3.Distance(start, end);
        float height = Mathf.Max(minHeight, maxHeight * dist);

        float t = 0;
        //미리 나눗셈
        float inv = 1f / Mathf.Max(elapsed, duration);

        while (t < 1f)
        {
            t += Time.deltaTime * inv;
            float u = Mathf.Clamp01(t);

            float p = flyCurve.Evaluate(u);  // 앞으로 가는 양
            float h = height * heightCurve.Evaluate(u);   // 위로 점프 양

            Vector3 pos = Vector3.Lerp(start, end, p) + Vector3.up * h;
            transform.position = pos;
    

            //크기 변화로 연출효과 냄, 포물선 곡선 재활용함
            float s = heightCurve.Evaluate(u);
            transform.localScale = Vector3.one * s;

            yield return null;
        }

        //플레이어 돈 증가시키는함수 호출
        onArrive?.Invoke();
        Destroy(gameObject);
    }
}
