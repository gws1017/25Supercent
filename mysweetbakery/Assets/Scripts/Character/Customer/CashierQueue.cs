using System.Collections.Generic;
using UnityEngine;

public class CashierQueue : MonoBehaviour
{
    [SerializeField] private Transform[] waitPoints;  // 계산대 앞에서부터 뒤로
    private readonly Queue<Customer> line = new Queue<Customer>();
    private readonly List<Customer> snapshot = new List<Customer>();

    public int Enqueue(Customer c)
    {
        line.Enqueue(c);
        RebuildSnapshot();
        return snapshot.IndexOf(c);
    }

    public void DequeueIfFront(Customer c)
    {
        if (line.Count == 0) return;
        if (line.Peek() != c) return;
        line.Dequeue();
        RebuildSnapshot();
    }

    public Vector3 GetPointPos(int index)
    {
        if (waitPoints == null || waitPoints.Length == 0) return transform.position;
        index = Mathf.Clamp(index, 0, waitPoints.Length - 1);
        return waitPoints[index].position;
    }

    public bool IsFront(Customer c) => line.Count > 0 && line.Peek() == c;

    private void RebuildSnapshot()
    {
        snapshot.Clear();
        foreach (var x in line) snapshot.Add(x);
    }
}