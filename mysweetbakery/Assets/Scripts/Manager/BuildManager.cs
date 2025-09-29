using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    #region Singleton
    public static BuildManager Instance { get; private set; }
    private void Singleton()
    {
        Instance = this;
    }
    #endregion

    private readonly Dictionary<UnlockId, bool> unlocked = new Dictionary<UnlockId, bool>();

    public event Action<UnlockId> OnUnlocked;

    // �ܺ� ����
    public bool IsUnlocked(UnlockId id) => unlocked.TryGetValue(id, out var v) && v;

    // ���� ó��(�ߺ� ����)
    public void Unlock(UnlockId id)
    {
        if (IsUnlocked(id)) return;
        unlocked[id] = true;
        OnUnlocked?.Invoke(id);
    }

    private void Awake()
    {
        Singleton();
    }
}
