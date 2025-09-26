using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input Raw")]
    [SerializeField] private float hAxis;
    [SerializeField] private float vAxis;

    [Header("Player Property")]
    [SerializeField] private float speed;

    Vector3 moveVec;
    private Rigidbody rb;
    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");

        moveVec = new Vector3(hAxis,0, vAxis).normalized;
        rb.position += moveVec * speed * Time.deltaTime;

        anim.SetBool("isMove",moveVec != Vector3.zero);
        transform.LookAt(transform.position + moveVec);
    }
}
