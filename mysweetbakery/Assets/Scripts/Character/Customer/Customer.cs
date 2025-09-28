using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour
{
    public enum CustomerState { ToShelf, WaitShelf, Picking, ToQueue, InQueue, AtCounter }
    [Serializable] public class Order { public int breadCount; public bool dineIn; }

    [Header("Components Read Only")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private BreadStack carryStack;       // ���� �տ� �׾� ��� ���� ����
    [SerializeField] private CustomerHeadUI headUI;
    private Animator anim;
    //[SerializeField] private Transform headFollow;        // �Ӹ� ��
    
    [Header("Components Register")]
    [SerializeField] public CashierQueue cashierQueue;

    [Header("Targets")]
    [SerializeField] private ShowBasket targetShelf;      // ���� �ϳ��� FindObjectOfType�� �ڵ����� ����
    [SerializeField] private Transform shelfStandPoint;   // ������ �� �� ��ġ(���� �и� ������ ������ �� ������Ʈ)

    [Header("Order Rules")]
    [SerializeField] private bool dineInFeatureEnabled = false; // ��� ���� �÷���
    [SerializeField] private Vector2Int breadRange = new Vector2Int(1, 3);

    private Order order;
    private CustomerState state;
    private int myQueueIndex = -1;
    public int RemainingToPick { get; private set; }
    public int NeedBreadCount => order.breadCount;

    public void ShowOrderUI(int amount)
    {
        headUI.ShowOrder(amount);

    }

    public void ChangeState(CustomerState cs)
    {
        state = cs;
    }

    public void MoveToCashier()
    {
        // �����
        if (cashierQueue)
        {
            myQueueIndex = cashierQueue.Enqueue(this);
            Vector3 qpos = cashierQueue.GetPointPos(myQueueIndex);
            MoveTo(qpos);
            ChangeState(CustomerState.ToQueue);
        }
        else
        {
            // ť�� ������ �׳� ���� �� Ʈ����������
            ChangeState(CustomerState.ToQueue);
        }
    }

    public void OnPickedOne()  // �� �� ���������� �տ� ������ ���� ����
    {
        if (RemainingToPick > 0) RemainingToPick--;
    }

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!carryStack) carryStack = GetComponent<BreadStack>();
        if (!headUI) headUI = GetComponentInChildren<CustomerHeadUI>();
        if (!targetShelf) targetShelf = FindObjectOfType<ShowBasket>();
        if (!anim) anim = GetComponent<Animator>();
    }

    void Start()
    {
        InitOrder();

        // ������ ������ �̵�
        state = CustomerState.ToShelf;
        var dest = shelfStandPoint ? shelfStandPoint.position : (targetShelf ? targetShelf.transform.position : transform.position);
        MoveTo(dest);
        StartCoroutine(StateRoutine());
    }

    private void MoveTo(Vector3 pos)
    {
        if (!agent) return;
        anim.SetBool("isMove", true);
        agent.isStopped = false;
        agent.SetDestination(pos);
    }

    private bool Arrived(float eps = 0.15f)
    {
        if (!agent) return true;
        if (agent.pathPending) return false;
        return agent.remainingDistance <= Mathf.Max(eps, agent.stoppingDistance + 0.05f);
    }

    private void InitOrder()
    {
        int need = UnityEngine.Random.Range(breadRange.x, breadRange.y + 1);
        RemainingToPick = Mathf.Max(0, need);

        bool dineIn = false;
        if (dineInFeatureEnabled)
            dineIn = (UnityEngine.Random.value < 0.5f);
        order = new Order { breadCount = need, dineIn = dineIn };
    }

    private void SetStackMode(bool value)
    {
        if(anim) anim.SetBool("isStack", value);
    }

    private IEnumerator StateRoutine()
    {
        while (true)
        {
            switch (state)
            {
                case CustomerState.ToShelf:
                    if (Arrived())
                    {
                        anim.SetBool("isMove", false);
                        headUI.ShowOrder(order.breadCount);
                        state = CustomerState.WaitShelf;
                    }
                    break;

                case CustomerState.Picking:
                    break;
                case CustomerState.WaitShelf:
                    // �����뿡 ���� ����ϸ� ���� ����
                    if (targetShelf && targetShelf.GetAvailableBreadCount() >= order.breadCount)
                    {
                        SetStackMode(true);
                        ChangeState(CustomerState.Picking);
                        carryStack.StartPick(targetShelf.TakeBread,this);
                    }
                    break;

                case CustomerState.ToQueue:
                    if (Arrived())
                    {
                        anim.SetBool("isMove", false);
                        state = CustomerState.InQueue;
                    }
                    break;

                case CustomerState.InQueue:
                        state = CustomerState.AtCounter;
                        headUI.ShowPOS();
                    break;

                case CustomerState.AtCounter:
                    if(cashierQueue &&  cashierQueue.IsFront(this))
                    {
                        //SetStackMode(false);

                        // ���⼭ ���� ���� ���̸� ��(���� �Ϸ� �� Dequeue)
                        // cashierQueue.DequeueIfFront(this); Destroy(gameObject); ��

                    }
                    break;
            }
            yield return null;
        }
    }

    
}