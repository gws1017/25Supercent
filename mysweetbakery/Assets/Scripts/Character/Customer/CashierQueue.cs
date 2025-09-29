using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CashierQueue : MonoBehaviour
{
    [SerializeField] Transform lineStart;      // �Ǿ� �ڸ�(�ʼ� �ϳ���)
    [SerializeField] Transform lineEnd;        // ���� ���� ����(����)
    [SerializeField] float spacing = 0.6f;     // ��� ����
    [SerializeField] int initialSlots = 6;     // ���� ���� ��
    [SerializeField] int expandChunk = 4;

    private readonly List<Transform> waitPoints = new List<Transform>();  // ���� �տ������� �ڷ�

    private readonly Queue<Customer> line = new Queue<Customer>();
    private readonly List<Customer> snapshot = new List<Customer>();

    public event System.Action QueueChanged;
    public int GetIndex(Customer c) => snapshot.IndexOf(c);
    public bool IsFront(Customer c) => line.Count > 0 && line.Peek() == c;

    public Customer PeekFront()
    {
        return line.Count > 0 ? line.Peek() : null;
    }

    public int Enqueue(Customer c)
    {
        line.Enqueue(c);
        RebuildSnapshot();
        EnsureCapacity(snapshot.Count); 
        return snapshot.Count -1;
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
        EnsureCapacity(index + 1);
        index = Mathf.Clamp(index, 0, waitPoints.Count - 1);
        return waitPoints[index].position;
    }

    private void RebuildSnapshot()
    {
        snapshot.Clear();
        foreach (var x in line) snapshot.Add(x);
        QueueChanged?.Invoke();
    }
    private Vector3 GetLineDirection()
    {
        Vector3 d;
        if (lineEnd && lineEnd != lineStart) d = (lineEnd.position - lineStart.position);
        else d = -lineStart.forward;
        d.y = 0;
        return d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.back;
    }

    private void EnsureCapacity(int needed)
    {
        if (needed <= waitPoints.Count) return;

        var dir = GetLineDirection();
        int target = Mathf.Max(needed, waitPoints.Count + expandChunk);

        for (int i = waitPoints.Count; i < target; ++i)
        {
            var go = new GameObject($"Q{i:00}");
            go.transform.SetParent(transform, worldPositionStays: false);

            Vector3 basePos = lineStart.position;
            Vector3 pos = basePos + dir * (spacing * i);
            pos.y = basePos.y; // ���� ����
            go.transform.position = pos;

            waitPoints.Add(go.transform);
        }
    }

    private void Awake()
    {
        if (!lineStart) lineStart = transform; // �ּ� ������
        EnsureCapacity(initialSlots);
    }
}