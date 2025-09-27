using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenBelt : MonoBehaviour
{
    [SerializeField] private float speed = 1.2f;
    [SerializeField] private bool preserveYVelocity = true;          // 낙하 속도 보존

    private Collider col;

    void Reset()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.attachedRigidbody) return;
        if(other.gameObject.GetComponent<Bread>() == null) return;

        var rb = other.attachedRigidbody;

        Vector3 dir = transform.forward;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        //수평 속도만
        Vector3 v = rb.velocity;
        Vector3 horiz = dir * speed;
        rb.velocity = preserveYVelocity ? new Vector3(horiz.x, v.y, horiz.z) : horiz;
    }

    void OnTriggerExit(Collider other)
    {
        var rb = other.attachedRigidbody;
        if (!rb) return;
        Vector3 v = rb.velocity;
        rb.velocity = new Vector3(0f, v.y, 0f);
    }
}
