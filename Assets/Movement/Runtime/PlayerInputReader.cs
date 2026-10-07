using UnityEngine;
using UnityEngine.InputSystem;

namespace AllAboard.Movement
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private InputAction move;
        private InputAction look;
        private InputAction jump;

        public Vector2 Move => move.ReadValue<Vector2>();
        public Vector2 Look => look.ReadValue<Vector2>();
        public bool JumpPressed => jump.WasPressedThisFrame();
        public bool HasControl => Application.isFocused && Cursor.lockState == CursorLockMode.Locked;

        private void Awake()
        {
            move = new InputAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            look = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
            jump = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        }

        private void OnEnable()
        {
            move.Enable();
            look.Enable();
            jump.Enable();
            CaptureCursor();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                ReleaseCursor();
            else if (Application.isFocused && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                CaptureCursor();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) ReleaseCursor();
        }

        private static void CaptureCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void ReleaseCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDisable()
        {
            move?.Disable();
            look?.Disable();
            jump?.Disable();
            ReleaseCursor();
        }

        private void OnDestroy()
        {
            move?.Dispose();
            look?.Dispose();
            jump?.Dispose();
        }
    }
}
