using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerHeadUI : MonoBehaviour
{
    [SerializeField] private Canvas worldCanvas; // World Space
    [SerializeField] private Image iconImage;    // 빵/포스기 아이콘
    [SerializeField] private TMP_Text countText; // "xN"
    [SerializeField] private Sprite breadIcon;
    [SerializeField] private Sprite posIcon;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        worldCanvas.worldCamera = cam;
        Show(false);
    }

    void LateUpdate()
    {
        if (!worldCanvas || !cam) return;
        worldCanvas.transform.rotation = Quaternion.LookRotation(worldCanvas.transform.position - cam.transform.position);
    }

    public void ShowOrder(int needCount)
    {
        if (!worldCanvas) return;
        if(needCount <= 0)
        {
            ShowPOS();
            return;
        }
        Show(true);
        iconImage.sprite = breadIcon;
        iconImage.enabled = true;
        countText.text = needCount.ToString();
        countText.enabled = true;
    }

    public void ShowPOS()
    {
        if (!worldCanvas) return;
        Show(true);
        iconImage.sprite = posIcon;
        iconImage.enabled = true;
        countText.enabled = false;
    }

    public void Hide() => Show(false);

    private void Show(bool v)
    {
        if (worldCanvas) worldCanvas.enabled = v;
    }
}