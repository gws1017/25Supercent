using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bakery
{
    public class Joystick : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {

        [SerializeField] protected RectTransform Handler;
        protected RectTransform rectTransform;
        [SerializeField] protected Canvas mainCanvas;

        [SerializeField] private PlayerController pc;

        [SerializeField, Range(10, 150)] private float HandlerRange;

        [SerializeField] private bool invert = false;
        private bool IsDrag = false;
        protected Vector2 InputDirection;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            IsDrag = true;
            ControllJoysticHandler(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            ControllJoysticHandler(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            IsDrag = false;
            Handler.anchoredPosition = Vector2.zero;
            InputDirection = Vector2.zero;
            pc.ClearCache();
        }
        public void InputControlVector()
        {
            float inverMultiplier = 1;
            if (invert) inverMultiplier = -1;

            pc.HorizonItalInput = InputDirection.x * inverMultiplier;
            pc.VerticalInput = InputDirection.y * inverMultiplier;
        }

        private void ControllJoysticHandler(PointerEventData eventData)
        {
            if (Handler == null) return;
            var cam = (mainCanvas && mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
              ? null
              : eventData.pressEventCamera;

            Vector2 local; // 배경 rectTransform 기준 로컬 좌표
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, eventData.position, cam, out local);

            var v = local.magnitude <= HandlerRange ? local : local.normalized * HandlerRange;
            Handler.anchoredPosition = v;
            InputDirection = v / HandlerRange;
        }

        private void Update()
        {
            if (IsDrag && pc)
            {
                InputControlVector();
            }
        }
    }
}

