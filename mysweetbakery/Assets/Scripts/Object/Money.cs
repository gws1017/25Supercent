using System;
using System.Collections;
using UnityEngine;

public class Money : MonoBehaviour
{
    private MoneyDummy owner;

    [Header("Curves")]
    [SerializeField] private AnimationCurve flyCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("������ �̵� Ŀ��")]
    [SerializeField] private AnimationCurve heightCurve =
        new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.35f, 1), new Keyframe(1, 0));

    [Header("Height Value")]
    [SerializeField] float maxHeight = 0.35f;
    [SerializeField] float minHeight = 0.15f;

    private const float elapsed = 0.0001f; //0 ������

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
        //�̸� ������
        float inv = 1f / Mathf.Max(elapsed, duration);

        while (t < 1f)
        {
            t += Time.deltaTime * inv;
            float u = Mathf.Clamp01(t);

            float p = flyCurve.Evaluate(u);  // ������ ���� ��
            float h = height * heightCurve.Evaluate(u);   // ���� ���� ��

            Vector3 pos = Vector3.Lerp(start, end, p) + Vector3.up * h;
            transform.position = pos;
    

            //ũ�� ��ȭ�� ����ȿ�� ��, ������ � ��Ȱ����
            float s = heightCurve.Evaluate(u);
            transform.localScale = Vector3.one * s;

            yield return null;
        }

        //�÷��̾� �� ������Ű���Լ� ȣ��
        onArrive?.Invoke();
        Destroy(gameObject);
    }
}
