using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperBag : MonoBehaviour
{
    [SerializeField] private Transform point;  // 빵이 빨려 들어갈 입구 위치
     private Animator anim;

    public Transform Point => point;

    public void Appear() => anim?.SetTrigger("appear");
    public void CloseBag() => anim?.SetTrigger("close");

    public IEnumerator FlyToHand(Transform hand, float duration = 0.25f)
    {
        Vector3 s = transform.position;
        Quaternion r0 = transform.rotation;

        float t = 0f;
        float inv = 1f / Mathf.Max(0.0001f, duration);
        while (t < 1f)
        {
            t += Time.deltaTime * inv;
            float u = Mathf.Clamp01(t);

            transform.position = Vector3.Lerp(s, hand.position, u);
            transform.rotation = Quaternion.Slerp(r0, hand.rotation, u);
            yield return null;
        }

        transform.SetParent(hand, worldPositionStays: false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    private void Awake()
    {
        if (!anim) anim = GetComponent<Animator>();
    }
}
