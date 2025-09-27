using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oven : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private Bread breadPrefab;
    [SerializeField] private Transform trayTransform;   // 트레이 부모(빵이 잠시 놓이는 곳)
    [SerializeField] private float produceTime = 0.8f;  // 주기적으로 한 개씩 생성
    [SerializeField] private int trayMax = 12;     // 트레이 적재 상한
    [SerializeField] private Vector2 trayGrid = new Vector2(3, 4);
    [SerializeField] private Vector2 traySpacing = new Vector2(0.22f, 0.22f);

    private readonly Queue<Bread> tray = new();
    private Coroutine produceCoroutine;

    private void OnEnable() 
    { 
        produceCoroutine = StartCoroutine(ProduceRoutine()); 
    }
    private void OnDisable() 
    { 
        if (produceCoroutine != null) 
            StopCoroutine(produceCoroutine); 
    }

    private Vector3 TrayLocalPos(int i)
    {
        int cx = i % (int)trayGrid.x;
        int cy = i / (int)trayGrid.x;
        float offX = (cx - (trayGrid.x - 1) * 0.5f) * traySpacing.x;
        float offZ = (cy - (trayGrid.y - 1) * 0.5f) * traySpacing.y;
        return new Vector3(offX, 0, offZ);
    }

    private IEnumerator ProduceRoutine()
    {
        while (true)
        {
            if (tray.Count < trayMax)
            {
                var bread = Instantiate(breadPrefab, trayTransform);
                bread.transform.localPosition = TrayLocalPos(tray.Count);
                tray.Enqueue(bread);
            }
            yield return new WaitForSeconds(produceTime);
        }
    }

    public Bread TryPopOne()
    {
        if (tray.Count == 0) return null;
        return tray.Dequeue();
    }

    //플레이어 상호작용
    void OnTriggerEnter(Collider other)
    {
        var stack = other.GetComponentInChildren<BreadStack>();
        if (stack) stack.StartPullFrom(TryPopOne);
    }
    void OnTriggerExit(Collider other)
    {
        var stack = other.GetComponentInChildren<BreadStack>();
        if (stack) stack.StopPull(); // 범위 벗어나면 즉시 중단
    }
}
