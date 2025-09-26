using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int currentMoney = 0;

    public int CurrentMoney { get => currentMoney; set => currentMoney = value; }

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
}
