using System.Collections;
using TMPro;
using UnityEngine;

public class PurchasePad : MonoBehaviour
{
    [Header("Price")]
    [SerializeField] private int price = 30;              // �� ���
    [SerializeField] private float payInterval = 0.06f;   // �� 1�� ���� ����(��)

    [Header("Visuals")]
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Transform moneySinkPoint;

    [Header("Result")]
    [SerializeField] private GameObject resultPrefab;     // �ϼ� �� ��ġ�� ������
    [SerializeField] private GameObject replaceObject;     // �ϼ� �� ��ġ�� ������
    [SerializeField] private Transform spawnParent;       // ��ġ �θ�
    [SerializeField] private Vector3 spawnOffset;         // �ʿ� �� ����

    [Header("FX (optional)")]
    [SerializeField] private Money moneyVisualPrefab;     // ���� Money ������ ��Ȱ��(��� ����)
    [SerializeField] private float moneyFlyTime = 0.22f;  // �� 1�� ���� �ð�

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
                // ���� ������ ���
                yield return null;
                continue;
            }

            if (moneyVisualPrefab && moneySinkPoint)
            {
                var moneyStart = pc.moneyCollectPoint.position;
                var m = Instantiate(moneyVisualPrefab, moneyStart, Quaternion.identity);
                m.Fly(moneySinkPoint, moneyFlyTime, onArrive: null);
            }

            // ���� ���� ó��
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
        //���� ���� �ı�?
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
