using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyDummy : MonoBehaviour
{
    [Header("Layout (3x3 per layer)")]
    [SerializeField] private float slotSpacingX = 0.22f;
    [SerializeField] private float slotSpacingZ = 0.22f;
    [SerializeField] private float layerHeight = 0.12f;

    [Header("Collect")]
    [SerializeField] private float flyTime = 0.25f;    // 한 장 당 흡수 시간
    [SerializeField] private float flyStagger = 0.03f; // 연출 지연

    private readonly List<Money> moneyDummy = new();
    private bool isCollecting;

    private Vector3 LocalPosForIndex(int index)
    {
        int layer = index / 9;
        int inL = index % 9;
        int cx = inL % 3;
        int cz = inL / 3;

        float offX = (cx - 1) * slotSpacingX;
        float offZ = (cz - 1) * slotSpacingZ;
        float y = layer * layerHeight;

        return new Vector3(offX, y, offZ);
    }

    public void Add(Money money)
    {
        int index = moneyDummy.Count;
        moneyDummy.Add(money);
        money.transform.localPosition = LocalPosForIndex(index);
        money.SetOwner(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollecting) return;

        var pc = other.GetComponentInParent<PlayerCharacter>();
        if (pc != null)
            StartCoroutine(CollectAllTo(pc.moneyCollectPoint));
    }

    private IEnumerator CollectAllTo(Transform target)
    {
        isCollecting = true;

        // 뒤에서부터(위층부터) 빨아들임
        for (int i = moneyDummy.Count - 1; i >= 0; --i)
        {
            var money = moneyDummy[i];
            if (money != null) money.Fly(target, flyTime, () =>
            {
                PlayerCharacter.instance.PlayerInventory.AddMoney(1);
            });

            yield return new WaitForSeconds(flyStagger);
        }

        // 모두 흡수될 때까지 잠깐 대기
        yield return new WaitForSeconds(flyTime + 0.05f);

        // 정리
        moneyDummy.Clear();
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    //배치 확인용
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < 18; ++i)
            Gizmos.DrawWireCube(transform.position + LocalPosForIndex(i), new Vector3(0.16f, 0.02f, 0.16f));
    }
#endif
}