using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BreadStack : MonoBehaviour
{
    [SerializeField] private Transform stackRootTransform;      // 팔/손 위치(부모)
    [SerializeField] private TMP_Text maxText;

    [SerializeField] private float slotHeight = 0.12f;
    [SerializeField] private float flyDuration = 0.22f;
    [SerializeField] private float stagger = 0.04f;
    private int slotIndex = 0;

    private readonly List<Bread> BreadStacks = new List<Bread>();
    private readonly List<Transform> slots = new List<Transform>();
    private Coroutine pullCoroutine, pushCooroutine;
    public int Count => BreadStacks.Count;
    public bool IsFull() 
    {
        var player = PlayerCharacter.instance;
        var inven = PlayerCharacter.instance?.PlayerInventory;
        bool ret = false;
        if (inven)
        {
            ret = inven.FullBread;
            ret |= (slots.Count >= inven.MaxBread);
        }

        return ret;
    }

    void ShowMax(bool value)
    {
        if (maxText) maxText.gameObject.SetActive(value);
    }

    public void StartPullFrom(Func<Bread> provider)
    {
        if (pullCoroutine != null) StopCoroutine(pullCoroutine);
        pullCoroutine = StartCoroutine(PullRoutine(provider));
    }
    public void StopPull() 
    { 
        if (pullCoroutine != null)
        {
            StopCoroutine(pullCoroutine); 
            pullCoroutine = null; 

        }
    }

    IEnumerator PullRoutine(Func<Bread> provider)
    {
        while (true)
        {
            if (IsFull())
            {
                ShowMax(true);
                yield return null;
                continue;
            }
            ShowMax(false);

            var player = PlayerCharacter.instance;
            if (player == null) yield break;
            player.SetStackMode(true);

            

            var bread = provider?.Invoke();     // 오븐이 즉시 한 개 내줌(없으면 null)
            if (!bread) { yield return null; continue; }

            int index = slotIndex++;
            Transform slot = new GameObject($"BreadSlot_{index}").transform;
            slot.SetParent(stackRootTransform, false);
            slot.localPosition = new Vector3(0, index * slotHeight, 0);
            slot.localRotation = Quaternion.identity;
            slots.Add(slot);

            bread.FlyTo(slot, flyDuration, () =>
            {
                var rb = bread.GetComponent<Rigidbody>();
                if (rb == null) return;
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.velocity = Vector3.zero; 
                rb.angularVelocity = Vector3.zero;

                var col = bread.GetComponent<Collider>();
                if(col == null) return;
                col.enabled = false;

                bread.transform.SetParent(slot, false);
                bread.transform.localPosition = Vector3.zero;
                bread.transform.localRotation = Quaternion.identity;
                bread.transform.localScale = Vector3.one*0.5f;
                BreadStacks.Add(bread);
                if (player.PlayerInventory)
                    player.PlayerInventory.AddBread(1);
            });

            yield return new WaitForSeconds(stagger);
        }
    }

    public void StartPushTo(Func<Transform> showBasketSlot)
    {
        if (pushCooroutine != null) StopCoroutine(pushCooroutine);
        pushCooroutine = StartCoroutine(PushRoutine(showBasketSlot));
    }
    public void StopPush() 
    {
        if (pushCooroutine != null)
        {
            StopCoroutine(pushCooroutine); 
            pushCooroutine = null;
        }
    }

    IEnumerator PushRoutine(Func<Transform> GetEmptyShowBasketSlot)
    {
        while (BreadStacks.Count > 0)
        {
            // 위에서부터 하나씩
            int last = BreadStacks.Count - 1;
            var bread = BreadStacks[last];


            int slotIdx = slotIndex - 1;
            slotIndex--;

            if (slotIdx < 0 || slotIdx >= slots.Count)
                yield break;

            var slot = slots[slotIdx]; //플레이어에 부착된 슬롯

            var shelfSlot = GetEmptyShowBasketSlot?.Invoke();
            // 진열대가 자리가 없으면 되돌려 놓기
            if (!shelfSlot)
                yield break;

            BreadStacks.RemoveAt(last);
            bread.transform.SetParent(null, true);

            var placeholder = new GameObject("Reserved").transform;
            placeholder.SetParent(shelfSlot, false);

            bread.FlyTo(shelfSlot, flyDuration, () =>
            {
                var player = PlayerCharacter.instance;
                if (player == null) return;
                if (player.PlayerInventory == null) return;

                player.PlayerInventory.RemoveBread(1);

                bread.transform.SetParent(shelfSlot, true);
                bread.transform.localPosition = Vector3.zero;
                bread.transform.localRotation = Quaternion.identity;
                bread.transform.localScale = Vector3.one * 0.8f;

                if (placeholder)
                {
                    Destroy(placeholder.gameObject);
                    placeholder = null;
                }

                slots.Remove(slot);
                if (slot) Destroy(slot.gameObject); //플레이어에게 부착된 빈 슬롯 삭제

                //마지막 빵이면 스택상태 해제
                if (BreadStacks.Count <= 0 && player)
                    player.SetStackMode(false);
                ShowMax(IsFull());
            });

            ShowMax(IsFull());

            yield return new WaitForSeconds(stagger);
        }
    }

}
