using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardObject : MonoBehaviour
{
    private Camera cam;
    private Canvas worldCanvas;
    private void Awake() 
    { 
        cam = Camera.main;
        worldCanvas = GetComponent<Canvas>();
        if (worldCanvas) worldCanvas.worldCamera = cam;
    }
    private void LateUpdate()
    {
        if (!cam) 
        { 
            cam = Camera.main; 
            if (!cam) return;
        }
        Vector3 dir = transform.position - cam.transform.position;
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}
