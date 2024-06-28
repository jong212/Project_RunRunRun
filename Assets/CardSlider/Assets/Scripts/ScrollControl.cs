using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace tkitfacn.UI
{
    public class ScrollControl : MonoBehaviour
    {
        [SerializeField] CardSlider cardSlider = default;
        [SerializeField] float speed = 1;
        [SerializeField] bool clamp = false;
        float controlSpeed = 0;
        float value = 0;
        RectTransform rect;
        Vector2 initPos = Vector2.zero;

        private InputAction clickAction;
        private InputAction dragAction;

        private void Start()
        {
            rect = gameObject.GetComponent<RectTransform>();
            controlSpeed = (Screen.width / 1080f) * 0.01f;

            // Input System 설정
            var inputActions = new InputActionMap("UI");
            clickAction = inputActions.AddAction("Click", binding: "<Pointer>/press");
            dragAction = inputActions.AddAction("Drag", binding: "<Pointer>/delta");

            clickAction.performed += ctx => OnPointerDown();
            dragAction.performed += ctx => OnDrag(ctx);

            clickAction.Enable();
            dragAction.Enable();
        }

        private void OnDestroy()
        {
            clickAction.performed -= ctx => OnPointerDown();
            dragAction.performed -= ctx => OnDrag(ctx);
        }

        public void OnCardSliderDestroyed()
        {
            cardSlider = null;
        }

        private void OnPointerDown()
        {
            if (rect != null && InsideRect(rect))
            {
                initPos = Pointer.current.position.ReadValue();
                isControl = true;
            }
        }

        private void OnDrag(InputAction.CallbackContext context)
        {
            if (!isControl || rect == null || cardSlider == null) return;

            Vector2 currentPos = Pointer.current.position.ReadValue();
            var delta = (currentPos - initPos) * controlSpeed * speed;
            value -= delta.x;

            if (clamp)
            {
                value = Mathf.Clamp(value, -1, 1);
            }
            else
            {
                if (Mathf.Abs(value) > 2)
                {
                    value = Mathf.Clamp(value, -1, 1);
                    cardSlider.SetCardIndex((int)Mathf.Clamp(cardSlider.CardIndex + value, 0, cardSlider.CardLength), true);
                    value = 0;
                }
            }
            initPos = currentPos;
        }

        public bool isControl { get; set; }
        private void Update()
        {
            if (clickAction.WasReleasedThisFrame() && cardSlider != null)
            {
                if (clamp)
                    cardSlider.SetCardIndex((int)Mathf.Clamp(cardSlider.CardIndex + value, 0, cardSlider.CardLength), true);
                value = 0;
                isControl = false;
            }
        }

        [SerializeField] bool canvasWorldSpace = false;

        bool InsideRect(RectTransform content)
        {
            if (content == null) return false;

            if (canvasWorldSpace)
                return content.rect.Contains(content.InverseTransformPoint(Camera.main.ScreenToWorldPoint((Vector3)Pointer.current.position.ReadValue())));
            else
                return content.rect.Contains(content.InverseTransformPoint((Vector3)Pointer.current.position.ReadValue()));
        }
    }
}
