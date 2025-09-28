using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private CashierQueue cashierQueue;
    [SerializeField] private ShowBasket shelf;
    [SerializeField] private float interval = 3f;
    [SerializeField] private int maxAlive = 6;

    private readonly List<Customer> alive = new();

    void OnEnable() { StartCoroutine(SpawnLoop()); }
    void OnDisable() { StopAllCoroutines(); }

    IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(interval);
        while (true)
        {
            alive.RemoveAll(x => x == null);
            if (alive.Count < maxAlive)
            {
                var c = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
                // 간단한 의존성 주입
                if (!c.TryGetComponent(out Customer cust)) cust = c.GetComponent<Customer>();
                if (cust)
                {
                    if (cashierQueue) cust.GetType().GetField("cashierQueue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(cust, cashierQueue);
                    if (shelf) cust.GetType().GetField("targetShelf", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(cust, shelf);
                }
                alive.Add(cust);
            }
            yield return wait;
        }
    }
}