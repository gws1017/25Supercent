using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using static Customer;

public class BreadStack : MonoBehaviour
{
    [SerializeField] public Transform stackRootTransform;      // 팔/손 위치(부모)
    [SerializeField] public Transform bagTransform;      // 팔/손 위치(부모)
    [SerializeField] private TMP_Text maxText;

    [SerializeField] private float slotHeight = 0.12f;
    [SerializeField] private float flyDuration = 0.22f;
    [SerializeField] private float stagger = 0.04f;
    private int slotIndex = 0;

    private readonly List<Bread> BreadStacks = new List<Bread>();
    private readonly List<Transform> slots = new List<Transform>();
    private Coroutine pullCoroutine, pushCoroutine,pickCoroutine;
    private bool isPicking;
    public int Count => BreadStacks.Count;
    public float SlotHeight => slotHeight;
    public int SlotIndex {get =>  slotIndex; set =>  slotIndex = value;}
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
        if (pushCoroutine != null) StopCoroutine(pushCoroutine);
        pushCoroutine = StartCoroutine(PushRoutine(showBasketSlot));
    }
    public void StopPush() 
    {
        if (pushCoroutine != null)
        {
            StopCoroutine(pushCoroutine); 
            pushCoroutine = null;
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

    public void StartPick(Func<Bread> provider, Customer customer)
    {
        if (pickCoroutine != null) StopCoroutine(pickCoroutine);
        if (isPicking) return;
        isPicking = true;
        pickCoroutine = StartCoroutine(PickFromShelfRoutine(provider, customer));
    }
    public void StopPick()
    {
        if (pickCoroutine != null)
        {
            StopCoroutine(pickCoroutine);
            pickCoroutine = null;
            isPicking = false;
        }
    }

    IEnumerator PickFromShelfRoutine(Func<Bread> provider, Customer customer)
    {
        if (customer.RemainingToPick <= 0)
            goto Done;

        while (customer.RemainingToPick > 0)
        {
            var bread = provider?.Invoke();
            if (!bread)
            {
                // 더 이상 집을 빵이 없으면 대기 상태로 전환
                customer.ChangeState(Customer.CustomerState.WaitShelf);
                pickCoroutine = null;
                isPicking = false;
                yield break;
            }

            int index = slots.Count; 
            var slot = new GameObject($"CustSlot_{index}").transform;
            slot.SetParent(stackRootTransform, false);
            slot.localPosition = new Vector3(0, index * slotHeight, 0);
            slot.localRotation = Quaternion.identity;
            slots.Add(slot);

            bool arrived = false;
            bread.FlyTo(slot, 0.22f, onArrive: () =>
            {
                bread.transform.SetParent(slot, false);
                bread.transform.localPosition = Vector3.zero;
                bread.transform.localRotation = Quaternion.identity;
                bread.transform.localScale = Vector3.one * 0.5f;

                BreadStacks.Add(bread);
                customer.OnPickedOne();           
                customer.ShowOrderUI(customer.RemainingToPick);
                arrived = true;
            }, scaleByCurve: true);
            while (!arrived) yield return null;
            yield return new WaitForSeconds(0.04f);
        }
        Done:
        yield return null;
        pickCoroutine = null;
        isPicking = false;
        customer.ChangeState(Customer.CustomerState.ToQueue);
        customer.MoveToCashier();
    }


    public IEnumerator PackBreadRoutine(Transform target, float duration = 0.22f, float stagger = 0.04f)
    {
        while (BreadStacks.Count > 0)
        {
            int last = BreadStacks.Count - 1;
            var bread = BreadStacks[last];
            BreadStacks.RemoveAt(last);

            // 위에서부터 슬롯 제거
            int slotIdx = slots.Count - 1;
            if (slotIdx >= 0)
            {
                var slot = slots[slotIdx];
                slots.RemoveAt(slotIdx);
                if (slot) Destroy(slot.gameObject);
            }
            slotIndex = Mathf.Max(0, slotIndex - 1);

            // 손에서 분리 후 가방 입구로 흡입, 도착하면 빵 제거
            if (bread)
            {
                bread.transform.SetParent(null, true);
                bread.FlyTo(target, duration, onArrive: () =>
                {
                    Destroy(bread.gameObject);
                }, scaleByCurve: true);
            }

            yield return new WaitForSeconds(stagger);
        }
    }
}
