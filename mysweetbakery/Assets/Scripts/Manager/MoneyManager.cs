using System.Collections.Generic;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    #region Singleton
    public static MoneyManager Instance { get; private set; }
    private void Singleton()
    {
        Instance = this;
    }
    #endregion

    [Header("Prefabs")]
    [SerializeField] private Money moneyPrefab;
    [SerializeField] private MoneyDummy dummyPrefab;

    [Header("Find/Spawn")]
    [SerializeField] private float findRadius = 0.6f; //탐지 범위

    private readonly HashSet<MoneyDummy> dummies = new();


    void Awake() => Singleton();
    public void Create(Vector3 createPosition, int amount)
    {
        if (amount <= 0) return;

        //같은 지점의 기존 더미 탐색
        MoneyDummy dummy = FindExistingDummy(createPosition);
        if (dummy == null)
        {
            dummy = Instantiate(dummyPrefab, createPosition, Quaternion.identity);
            dummies.Add(dummy);
        }

        //쌓기
        for (int i = 0; i < amount; ++i)
        {
            var money = Instantiate(moneyPrefab, dummy.transform);
            dummy.Add(money);
        }
    }

    private MoneyDummy FindExistingDummy(Vector3 pos)
    {
        foreach (var dummy in dummies)
        {
            if (!dummy) continue;
            if (Vector3.SqrMagnitude(dummy.transform.position - pos) <= findRadius * findRadius)
                return dummy;
        }
        return null;
    }
}