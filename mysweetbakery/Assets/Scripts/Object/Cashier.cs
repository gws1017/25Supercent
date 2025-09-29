using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class Cashier : MonoBehaviour
{
    #region Singleton
    public static Cashier instance { get; private set; }

    private void Singleton()
    {
        instance = this;
    }
    #endregion
    [Header("Components")]
    [SerializeField] private CashierQueue takeoutQueue;     // 계산대 위 가방 스폰 위치
    [SerializeField] private CashierQueue dineInQueue;     // 계산대 위 가방 스폰 위치
    [SerializeField] private Transform bagSpawnPoint;     // 계산대 위 가방 스폰 위치
    [SerializeField] private PaperBag bagPrefab;
    [SerializeField] private Transform moneySpawnPoint;   // 돈 더미 스폰 위치
    [SerializeField] private int pricePerBread = 1;       // 1빵당 지폐 개수(= MoneyManager amount)

    [Header("FX (optional)")]
    [SerializeField] private GameObject happyEmojiPrefab; // 고객 머리 위에서 작아지며 사라지는 이모티콘
    [SerializeField] private float happyDuration = 0.6f;

    private bool isBusy;
    private bool isEnter;
    private Customer pending;
    public int PricePerBread => pricePerBread;
    public bool IsBusy => isBusy;
    public CashierQueue TakeOutQueue => takeoutQueue;
    public CashierQueue DineInQueue => dineInQueue;
    public void OnCustomerArrived(Customer c)
    {
        if (c == null) return;
        if (!c.IsQueueFront()) return;

        pending = c;                  // 대기열의 맨 앞 손님 등록
        TryServe();
    }

    private void TryServe()
    {
        if (isBusy) return;
        if (!isEnter) return;    
        
        var takeFront = takeoutQueue ? takeoutQueue.PeekFront() : null;
        if (takeFront != null && !isBusy)
        {
            pending = takeFront;
            StartCoroutine(ServeRoutine(pending));
            return;
        }

        var dineFront = dineInQueue ? dineInQueue.PeekFront() : null;
        if (dineFront != null && BuildManager.Instance.IsUnlocked(UnlockId.Table))
        {
            var seat = TableSeat.Instance.GetFreeSeat();
            if (seat != null && !isBusy)
            {
                pending = dineFront;
                StartCoroutine(ServeDineInRoutine(pending, seat));
                return;
            }
        }

    }

    private IEnumerator ServeRoutine(Customer c)
    {
        isBusy = true;

        // 1) 가방 소환 + appear
        var bag = Instantiate(bagPrefab, bagSpawnPoint.position, bagSpawnPoint.rotation);
        bag.Appear();
        yield return new WaitForSeconds(0.4f);

        // 2) 고객 손의 빵을 전부 가방 입구로 흡입(삭제)
        if (c != null && c.CarryStackExists())
        {
            yield return c.GetCarryStack().PackBreadRoutine(bag.Point, 0.22f, 0.04f);
        }

        // 3) 가방 닫기
        bag.CloseBag();
        yield return new WaitForSeconds(0.4f); // close 애니 길이만큼

        // 4) 가방을 고객 손으로 전달
        var hand = c.GetHand(); // carryStack.stackRootTransform 반환
        if (hand) yield return bag.FlyToHand(hand, 0.25f);

        // 5) 돈 더미 스폰 (이전 Money 시스템 그대로 사용)
        int bills = Mathf.Max(0, c.NeedBreadCount) * Mathf.Max(1, pricePerBread);
        MoneyManager.Instance.Create(moneySpawnPoint.position, bills);

        // 6) 고객 머리 UI 정리 & 해피 이모티콘
        c.HideHeadIcon();
        if (happyEmojiPrefab)
        {
            var fx = Instantiate(happyEmojiPrefab, c.transform);
            fx.transform.localPosition = Vector3.zero + Vector3.up * 2.5f;
            fx.transform.localRotation = Quaternion.identity;
            StartCoroutine(HappyShrinkAndKill(fx, happyDuration));
        }

        isBusy = false;

        if (takeoutQueue) takeoutQueue.DequeueIfFront(c);
        c.OnServedAndLeave();

        pending = null;
        TryServe();
    }

    private IEnumerator ServeDineInRoutine(Customer customer, TableSeat seat)
    {
        isBusy = true;


        if (dineInQueue) dineInQueue.DequeueIfFront(customer);

        if (seat && customer)
        {
            customer.HideHeadIcon();
            customer.GoTo(seat.SeatPoint.position);            
            yield return customer.WaitArrive();                
            yield return seat.SeatCustomer(customer);          
            customer.HideHeadIcon();
            if (happyEmojiPrefab)
            {
                var fx = Instantiate(happyEmojiPrefab, customer.transform);
                fx.transform.localPosition = Vector3.zero + Vector3.up * 2.5f;
                fx.transform.localRotation = Quaternion.identity;
                StartCoroutine(HappyShrinkAndKill(fx, happyDuration));
            }
        }

        pending = null;
        isBusy = false;
        TryServe(); // 다음 손님 연속 처리
    }

    private IEnumerator HappyShrinkAndKill(GameObject go, float dur)
    {
        if (!go) yield break;
        Vector3 s0 = go.transform.localScale;
        float t = 0f, inv = 1f / Mathf.Max(0.0001f, dur);
        while (t < 1f)
        {
            t += Time.deltaTime * inv;
            float u = Mathf.Clamp01(t);
            if (go) go.transform.localScale = Vector3.Lerp(s0, Vector3.zero, u);
            yield return null;
        }
        Destroy(go);
    }

    private void Awake()
    {
        Singleton();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerCharacter>() != null)
        {
            isEnter = true;
            TryServe();               // 조건 맞으면 이때 시작
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerCharacter>() != null)
            isEnter = false;
    }

}
