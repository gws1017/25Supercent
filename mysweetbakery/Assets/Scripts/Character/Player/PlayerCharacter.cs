using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    #region SingleTon
    public static PlayerCharacter instance { get; private set; }

    private void Singleton()
    {
        instance = this;
    }
    #endregion

    [SerializeField] public Transform moneyCollectPoint;

    private PlayerController playerController;
    private PlayerInventory inventory;
    // Start is called before the first frame update

    public PlayerInventory PlayerInventory => inventory;

    public void SetStackMode(bool stackMode)
    {
         playerController.SetStackMode(stackMode);
    }

    private void Awake()
    {
        Singleton();
        playerController = GetComponent<PlayerController>();
        inventory = GetComponent<PlayerInventory>();
    }

    private void Start()
    {
    }

}
