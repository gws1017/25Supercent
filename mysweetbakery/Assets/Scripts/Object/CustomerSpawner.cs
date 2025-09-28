using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private CashierQueue cashierQueue;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float interval = 3f;
    [SerializeField] private int maxAlive = 6;

    private ShowBasket shelf;
    private readonly List<Customer> alive = new();

    private void Awake()
    {
        if (!shelf) shelf = FindObjectOfType<ShowBasket>();
    }
    void OnEnable() { StartCoroutine(SpawnLoop()); }
    void OnDisable() { StopAllCoroutines(); }

    private IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(interval);
        while (true)
        {
            alive.RemoveAll(x => x == null);
            if (alive.Count < maxAlive)
            {
                var customer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
                customer.Init(cashierQueue, shelf, spawnPoint);
                alive.Add(customer);
            }
            yield return wait;
        }
    }
}