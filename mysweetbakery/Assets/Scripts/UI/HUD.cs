using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    #region Singleton
    public static HUD instance { get; private set; }

    private void Singleton()
    {
        instance = this;
    }
    #endregion

    [SerializeField] TMP_Text moneyUIText;

    // Update is called once per frame
    private void Awake()
    {
        Singleton();
    }

    public void SetMoney(int amount)
    {
        moneyUIText.SetText("{0}", amount);
    }
}
