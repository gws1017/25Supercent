using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class Cashier : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform bagSpawnPoint;     // 계산대 위 가방 스폰 위치
    [SerializeField] private PaperBag bagPrefab;
    [SerializeField] private Transform moneySpawnPoint;   // 돈 더미 스폰 위치
    [SerializeField] private int pricePerBread = 1;       // 1빵당 지폐 개수(= MoneyManager amount)

    [Header("FX (optional)")]
    [SerializeField] private GameObject happyEmojiPrefab; // 고객 머리 위에서 작아지며 사라지는 이모티콘
    [SerializeField] private float happyDuration = 0.6f;

    private bool busy;
    private bool isEnter;
    private Customer pending;
    public void OnCustomerArrived(Customer c)
    {
        if (c == null) return;
        if (!c.IsQueueFront()) return;

        pending = c;                  // 대기열의 맨 앞 손님 등록
        TryServe();
    }

    private void TryServe()
    {
        if (busy) return;
        if (!isEnter) return;      
        if (pending == null) return;
        if (!pending.IsQueueFront()) return;

        StartCoroutine(ServeRoutine(pending));
    }

    private IEnumerator ServeRoutine(Customer c)
    {
        busy = true;

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

        // 7) 큐에서 제거 + 손님 퇴장 명령
        if (c.cashierQueue) c.cashierQueue.DequeueIfFront(c);
        c.OnServedAndLeave();

        pending = null;
        busy = false;

        // 플레이어가 계속 서 있고 다음 손님이 있다면 연속 처리
        TryServe();
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
            go.transform.localScale = Vector3.Lerp(s0, Vector3.zero, u);
            yield return null;
        }
        Destroy(go);
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
