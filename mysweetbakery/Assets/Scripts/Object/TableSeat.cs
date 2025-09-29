using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableSeat : MonoBehaviour
{
    #region Singleton
    public static TableSeat Instance { get; private set; }
    private void Singleton()
    {
        Instance = this;
    }
    #endregion

    [Header("Refs")]
    [SerializeField] private Transform seatPoint;
    [SerializeField] private Transform tableTop;
    [SerializeField] private Transform moneyPoint;
    [SerializeField] private GameObject chair;
    [SerializeField] private GameObject trashPrefab;

    [Header("Layout")]
    [SerializeField] private int cols = 3;
    [SerializeField] private int rows = 2;
    [SerializeField] private Vector2 spacing = new Vector2(0.22f, 0.22f);

    [Header("Timing")]
    [SerializeField] private float placeDuration = 0.18f;
    [SerializeField] private float placeStagger = 0.04f;
    [SerializeField] private Vector2 eatTimeRange = new Vector2(1.8f, 3.5f);

    private readonly List<Transform> slots = new();
    private bool isOccupied;
    private bool isDirty;
    private Quaternion chairInitRot;
    private GameObject trashObj;

    public bool IsFree => !isOccupied && !isDirty;
    public Transform SeatPoint => seatPoint;

    public TableSeat GetFreeSeat()
    {
        if (Instance && IsFree) return Instance;
        return null;
    }

    private void Awake()
    {
        Singleton();
        if (chair) chairInitRot = chair.transform.localRotation;

        // 테이블 슬롯 생성
        int total = cols * rows;
        for (int i = 0; i < total; ++i)
        {
            var t = new GameObject($"TableSlot_{i}").transform;
            t.SetParent(tableTop, false);
            int x = i % cols;
            int y = i / cols;
            float offX = (x - (cols - 1) * 0.5f) * spacing.x;
            float offZ = (y - (rows - 1) * 0.5f) * spacing.y;
            t.localPosition = new Vector3(offX, 0, offZ);
            t.localRotation = Quaternion.identity;
            slots.Add(t);
        }
    }

    public IEnumerator SeatCustomer(Customer c)
    {
        isOccupied = true;

        var stack = c.GetCarryStack();
        if (stack)
            yield return stack.PlaceAllTo(slots, placeDuration, placeStagger);

        c.PlaySit(true);

        float eat = Random.Range(eatTimeRange.x, eatTimeRange.y);
        yield return new WaitForSeconds(eat);
        c.PlaySit(false);

        int bills = Mathf.Max(0, Cashier.instance.PricePerBread);
        MoneyManager.Instance.Create(moneyPoint ? moneyPoint.position : transform.position, bills);

        RemoveBreadsOnTable();

        if (chair) chair.transform.localRotation = chairInitRot * Quaternion.Euler(0, 18f, 0);
        if (trashPrefab)
        {
            trashObj = Instantiate(trashPrefab, tableTop);
            trashObj.transform.localPosition = Vector3.zero;
        }
        isDirty = true;

        isOccupied = false;
        c.OnServedAndLeave();
    }

    public void CleanUp()
    {
        if (!isDirty) return;
        if (chair) chair.transform.localRotation = chairInitRot;
        if (trashObj) Destroy(trashObj);
        isDirty = false;
    }

    private void RemoveBreadsOnTable()
    {
        var breads = tableTop.GetComponentsInChildren<Bread>(true);
        for (int i = 0; i < breads.Length; ++i)
            if (breads[i]) Destroy(breads[i].gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isDirty) return;
        if (other.GetComponent<PlayerCharacter>() != null)
            CleanUp();
    }
}
