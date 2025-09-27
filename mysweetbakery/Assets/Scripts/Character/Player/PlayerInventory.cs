using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int currentMoney = 0;
    [SerializeField] private int currentBread = 0;
    [SerializeField] private int maxBread = 8;

    public int CurrentMoney { get => currentMoney; set => currentMoney = value; }
    public int CurrentBread { get => currentBread; set => currentBread = value; }
    public int MaxBread { get => maxBread; }

    public bool EmptyBread => currentBread == 0;
    public bool FullBread => currentBread >= maxBread;

    private void Start()
    {
        HUD.instance.SetMoney(currentMoney);
    }
    public bool TryConsumeMoney(int amount)
    {
        if (currentMoney < amount) return false;
        currentMoney -= amount;

        if (HUD.instance != null)
            HUD.instance.SetMoney(currentMoney);
        return true;
    }
    public void AddMoney(int amount)
    {
        currentMoney += Mathf.Max(0, amount);
        if (HUD.instance != null)
            HUD.instance.SetMoney(currentMoney);
    }

    public void AddBread(int amount)
    {
        currentBread += Mathf.Max(0, amount);
    }

    public bool RemoveBread(int amount)
    {
        if (currentBread < amount) return false;
        currentBread -= amount;
        return true;
    }
}
