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
        // 이미 채워진 슬롯은 childCount>0
        while (slotIndex < slots.Count && slots[slotIndex].childCount > 0)
            slotIndex++;

        if (slotIndex >= slots.Count) return null;
        breadCnt = slotIndex;

        var slot = slots[slotIndex];
        return slot;
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
