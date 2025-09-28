using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowBasket : MonoBehaviour
{

    [Header("Layout")]
    [SerializeField] private Transform shelfRoot;
    [SerializeField] private int cols = 4;
    [SerializeField] private int rows = 3;
    [SerializeField] private Vector2 spacing = new Vector2(0.24f, 0.24f);

    [SerializeField] private int breadCnt = 0;

    private readonly List<Transform> slots = new List<Transform>();
    private int slotIndex; // 다음 채울 슬롯

    public Transform GetEmptyShowBasketSlot()
    {
        for (int i = 0; i < slots.Count; ++i)
        {
            var t = slots[i];
            if (t && t.childCount == 0)
                return t;
        }
        return null;

    }

    public Bread TakeBread()
    {
        // 뒤에서부터(최근 채운 슬롯부터) 탐색해서 child가 있으면 하나 꺼냄
        for (int i = slots.Count - 1; i >= 0; --i)
        {
            var slot = slots[i];
            if (slot && slot.childCount > 0)
            {
                var child = slot.GetChild(0);
                var bread = child.GetComponent<Bread>();
                if (bread)
                {
                    bread.transform.SetParent(null, true);
                    slotIndex = 0;                        
                    return bread;
                }
            }
        }
        return null;
    }

    public int GetAvailableBreadCount()
    {
        int cnt = 0;
        for (int i = 0; i < slots.Count; ++i)
            if (slots[i] && slots[i].childCount > 0) cnt++;
        return cnt;
    }

    private void Awake()
    {
        // 슬롯 초기화 (빵 배치위치)
        int total = cols * rows;
        for (int i = 0; i < total; ++i)
        {
            var t = new GameObject($"ShelfSlot_{i}").transform;
            t.SetParent(shelfRoot, false);

            int x = i % cols;
            int y = i / cols;
            float offX = (x - (cols - 1) * 0.5f) * spacing.x;
            float offZ = (y - (rows - 1) * 0.5f) * spacing.y;
            t.localPosition = new Vector3(offX, 0, offZ);
            t.localEulerAngles = new Vector3(0, 35, 0); 
            slots.Add(t);
        }
        slotIndex = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        var stack = other.GetComponentInChildren<BreadStack>();
        if (stack) stack.StartPushTo(GetEmptyShowBasketSlot);

    }
    void OnTriggerExit(Collider other)
    {
        var stack = other.GetComponentInChildren<BreadStack>();
        if (stack) stack.StopPush(); // 범위 벗어나면 즉시 중단
    }
}
