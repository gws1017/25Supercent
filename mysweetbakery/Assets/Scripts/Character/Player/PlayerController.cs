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
    private bool isStack = false;
    public float HorizonItalInput {  get => hAxis;  set => hAxis = value; }
    public float VerticalInput {  get => vAxis;  set => vAxis = value; }


    public void SetStackMode(bool value)
    {
        isStack = value;
        if(anim != null)
            anim.SetBool("isStack",isStack);
    }

    public void ClearCache()
    {
        hAxis = 0;
        vAxis = 0;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        //hAxis = Input.GetAxisRaw("Horizontal");
        //vAxis = Input.GetAxisRaw("Vertical");

        moveVec = new Vector3(hAxis,0, vAxis).normalized;
        rb.position += moveVec * speed * Time.deltaTime;

        anim.SetBool("isMove",moveVec != Vector3.zero);
        transform.LookAt(transform.position + moveVec);
    }
}
