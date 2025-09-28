using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour
{
    public enum CustomerState { ToShelf, WaitShelf, Picking, ToQueue, InQueue, AtCounter, Leaving }
    [Serializable] public class Order { public int breadCount; public bool dineIn; }

    [Header("Components Read Only")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private BreadStack carryStack;
    [SerializeField] private CustomerHeadUI headUI;
    private Animator anim;

    [Header("Targets")]
    [SerializeField] private ShowBasket targetShelf;      
    [SerializeField] private Transform shelfStandPoint;  

    [Header("Order Rules")]
    [SerializeField] private bool dineInFeatureEnabled = false; // ��� ���� �÷���
    [SerializeField] private Vector2Int breadRange = new Vector2Int(1, 3);

    private Transform exitPoint;
    private CashierQueue cashierQueue;
    private Cashier cashier;
    private Order order;
    private bool requestServing = false;
    private CustomerState state;
    private int myQueueIndex = -1;
    public int RemainingToPick { get; private set; }
    public int NeedBreadCount => order.breadCount;
    public BreadStack GetCarryStack() => carryStack;
    public bool CarryStackExists() => carryStack != null;
    public Transform GetHand() => carryStack ? carryStack.bagTransform : transform;
    public void HideHeadIcon() => headUI?.Hide();
    public bool IsQueueFront() { return cashierQueue ? cashierQueue.IsFront(this) : false; }
    public void OnServedAndLeave()
    {
        ChangeState(CustomerState.Leaving);
        var dest = exitPoint ? exitPoint.position : transform.position + (-transform.forward) * 5f;
        MoveTo(dest);
    }
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

    public void Init(CashierQueue queue, ShowBasket shelf, Transform point)
    {
        cashierQueue = queue;
        targetShelf = shelf;
        exitPoint = point;
        cashierQueue.QueueChanged += OnQueueChanged;
    }

    private void OnDestroy()
    {
        if (cashierQueue) cashierQueue.QueueChanged -= OnQueueChanged;
    }
    private void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!carryStack) carryStack = GetComponent<BreadStack>();
        if (!headUI) headUI = GetComponentInChildren<CustomerHeadUI>();
        if (!targetShelf) targetShelf = FindObjectOfType<ShowBasket>();
        if (!anim) anim = GetComponent<Animator>();
        
    }

    private void Start()
    {
        if (!cashier) cashier = Cashier.instance;
        if (!cashierQueue) cashierQueue = cashier.CashQueue;

        if (cashierQueue)
        {
            cashierQueue.QueueChanged -= OnQueueChanged;
            cashierQueue.QueueChanged += OnQueueChanged;
        }
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

        if(agent.remainingDistance <= Mathf.Max(eps, agent.stoppingDistance + 1f))
        {
            anim.SetBool("isMove", false);
            return true;
        }
        return false;
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
    private void TryPromoteToCounter()
    {
        if (state == CustomerState.AtCounter || cashierQueue == null) return;
        if (!cashierQueue.IsFront(this)) return;
        if (!Arrived()) return;

        // ���⼭�� ĳ�� �ٻ� üũ: �ٻڸ� '�Ǿ� �����'���� ��ٸ���
        if (cashier != null && cashier.IsBusy) return;

        state = CustomerState.AtCounter;
        requestServing = false;
    }

    public void OnQueueChanged()
    {
        if (cashierQueue == null) return;

        int idx = cashierQueue.GetIndex(this);
        if (idx < 0) return; // �ٿ� ������ ����

        bool iAmFront = cashierQueue.IsFront(this);
        if (iAmFront && cashier != null && cashier.IsBusy)
            return;

        Vector3 target = cashierQueue.GetPointPos(idx);
        MoveTo(target);

        TryPromoteToCounter();
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

                case CustomerState.ToQueue://����� �̵�
                    if (Arrived())
                    {
                        anim.SetBool("isMove", false);
                        state = CustomerState.InQueue;
                    }
                    break;

                case CustomerState.InQueue:
                        state = CustomerState.AtCounter;
                    break;

                case CustomerState.AtCounter:

                    if (cashierQueue &&  cashierQueue.IsFront(this))
                    {
                        anim.SetBool("isMove", false);
                        if (!requestServing && cashier)
                        {
                            requestServing = true;
                            cashier.OnCustomerArrived(this);   // �� ���� ��û
                        }
                    }
                    break;
                case CustomerState.Leaving:
                    if (Arrived())
                    {
                        anim.SetBool("isMove", false);
                        Destroy(gameObject);
                    }
                    break;
            }
            yield return null;
        }
    }

    
}