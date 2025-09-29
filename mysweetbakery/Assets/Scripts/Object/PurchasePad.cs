using System.Collections;
using TMPro;
using UnityEngine;

public class PurchasePad : MonoBehaviour
{
    [Header("Price")]
    [SerializeField] private int price = 30;              // 총 비용
    [SerializeField] private float payInterval = 0.06f;   // 돈 1장 투입 간격(초)

    [Header("Visuals")]
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Transform moneySinkPoint;

    [Header("Result")]
    [SerializeField] private GameObject resultPrefab;     // 완성 후 배치될 프리팹
    [SerializeField] private GameObject replaceObject;     // 완성 후 배치될 프리팹
    [SerializeField] private Transform spawnParent;       // 배치 부모
    [SerializeField] private Vector3 spawnOffset;         // 필요 시 보정

    [Header("FX (optional)")]
    [SerializeField] private Money moneyVisualPrefab;     // 기존 Money 프리팹 재활용(없어도 동작)
    [SerializeField] private float moneyFlyTime = 0.22f;  // 돈 1장 비행 시간

    private int remaining;
    private bool playerInside;
    private Coroutine payRoutine;


    private void TryStartPay()
    {
        if (payRoutine == null && remaining > 0 && playerInside)
            payRoutine = StartCoroutine(PayRoutine());
    }

    private void StopPay()
    {
        if (payRoutine != null)
        {
            StopCoroutine(payRoutine);
            payRoutine = null;
        }
    }

    private IEnumerator PayRoutine()
    {
        var pc = PlayerCharacter.instance;
        var inven = pc ? pc.PlayerInventory : null;

        while (playerInside && remaining > 0)
        {
            if (inven == null) yield break;

            if (!inven.HasEnoughMoney(1))
            {
                // 돈이 없으면 대기
                yield return null;
                continue;
            }

            if (moneyVisualPrefab && moneySinkPoint)
            {
                var moneyStart = pc.moneyCollectPoint.position;
                var m = Instantiate(moneyVisualPrefab, moneyStart, Quaternion.identity);
                m.Fly(moneySinkPoint, moneyFlyTime, onArrive: null);
            }

            // 실제 결제 처리
            inven.TryConsumeMoney(1);
            remaining--;
            UpdateText();

            if (remaining <= 0)
            {
                BuildResult();
                StopPay();
                yield break;
            }

            yield return new WaitForSeconds(payInterval);
        }

        payRoutine = null;
    }

    private void BuildResult()
    {
        if (resultPrefab)
        {
            var parent = spawnParent ? spawnParent : transform.parent;
            var go = Instantiate(resultPrefab, transform.position + spawnOffset, transform.rotation, parent);
            go.SetActive(true);
        }
        //기존 벽은 파괴?
        Destroy(replaceObject);
        Destroy(gameObject);

    }

    private void UpdateText()
    {
        if (countText) countText.text = remaining.ToString();
    }

    private void Awake()
    {
        remaining = Mathf.Max(0, price);
        UpdateText();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerCharacter>() == null) return;
        playerInside = true;
        TryStartPay();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerCharacter>() == null) return;
        playerInside = false;
        StopPay();
    }
}
