using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oven : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private Bread breadPrefab;
    [SerializeField] private Transform trayTransform;   // Ʈ���� �θ�(���� ��� ���̴� ��)
    [SerializeField] private float produceTime = 0.8f;  // �ֱ������� �� ���� ����
    [SerializeField] private int trayMax = 12;     // Ʈ���� ���� ����
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

    //�÷��̾� ��ȣ�ۿ�
    void OnTriggerEnter(Collider other)
    {
        var stack = other.GetComponentInChildren<BreadStack>();
        if (stack) stack.StartPullFrom(TryPopOne);
    }
    void OnTriggerExit(Collider other)
    {
        var stack = other.GetComponentInChildren<BreadStack>();
        if (stack) stack.StopPull(); // ���� ����� ��� �ߴ�
    }
}
