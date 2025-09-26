using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FloatingJoystic : Bakery.Joystick
{
    [Header("Floating")]
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private bool hideOnRelease = true;
    // Start is called before the first frame update
    private void OnEnable()
    {
        if (hideOnRelease && backgroundRect)
            backgroundRect.gameObject.SetActive(false);
    }
    private float CanvasScale => mainCanvas ? mainCanvas.transform.localScale.x : 1f;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        var cam = (mainCanvas && mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
              ? null
              : eventData.pressEventCamera;
        MoveBackgroundToScreenPoint(eventData.position, cam);

        // 2) 배경 보이기
        if (hideOnRelease && backgroundRect)
            backgroundRect.gameObject.SetActive(true);

        base.OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (backgroundRect && hideOnRelease)
            backgroundRect.gameObject.SetActive(false);
    }
    private void MoveBackgroundToScreenPoint(Vector2 screenPos, Camera cam)
    {
        RectTransform parent = (RectTransform)rectTransform.parent;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            screenPos,
            cam,                  // Overlay면 null, Camera/World면 해당 카메라
            out local
        );
        rectTransform.anchoredPosition = local;
    }
}
