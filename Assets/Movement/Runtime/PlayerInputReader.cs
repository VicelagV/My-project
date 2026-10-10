using UnityEngine;
using UnityEngine.InputSystem;

namespace AllAboard.Movement
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private InputAction move;
        private InputAction look;
        private InputAction jump;
        private InputAction interact;
        private InputAction sprint;
        private InputAction drop;
        private InputAction crouch;
        private InputAction weld;
        private InputAction grab;

        public Vector2 Move => move.ReadValue<Vector2>();
        public Vector2 Look => look.ReadValue<Vector2>();
        public bool JumpPressed => jump.WasPressedThisFrame();
        public bool InteractPressed => interact.WasPressedThisFrame();
        public bool SprintHeld => sprint.IsPressed();
        public bool DropHeld => drop.IsPressed();
        public bool DropPressed => drop.WasPressedThisFrame();
        public bool DropReleased => drop.WasReleasedThisFrame();
        public bool CrouchHeld => crouch.IsPressed();
        public bool WeldPressed => weld.WasPressedThisFrame();
        public bool GrabPressed => grab.WasPressedThisFrame();
        public bool GrabHeld => grab.IsPressed();
        public string WeldKeyLabel => Keyboard.current != null ? Keyboard.current.yKey.displayName : "Z";
        private bool menuOpen;
        private int menuClosedFrame=-1;
        public bool MenuOpen=>menuOpen;
        public bool HasControl => !menuOpen && Time.frameCount>menuClosedFrame && !(GetComponent<AllAboard.Train.CombatHealth>()?.Down??false) && Application.isFocused && Cursor.lockState == CursorLockMode.Locked;
        public void SetMenuOpen(bool open){menuOpen=open;if(open)ReleaseCursor();else{menuClosedFrame=Time.frameCount;CaptureCursor();}}

        private void Awake()
        {
            move = new InputAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            look = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
            jump = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            interact = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
            sprint = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
            sprint.AddBinding("<Keyboard>/rightShift");
            drop = new InputAction("Drop / Throw", InputActionType.Button, "<Keyboard>/q");
            crouch = new InputAction("Crouch", InputActionType.Button, "<Keyboard>/c");
            // Input System names physical keys by US layout: Y is labelled Z on QWERTZ.
            weld = new InputAction("Weld", InputActionType.Button, "<Keyboard>/y");
            grab = new InputAction("Grab control", InputActionType.Button, "<Mouse>/leftButton");
        }

        private void OnEnable()
        {
            move.Enable();
            look.Enable();
            jump.Enable();
            interact.Enable();
            sprint.Enable();
            drop.Enable();
            crouch.Enable();
            weld.Enable();
            grab.Enable();
            CaptureCursor();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame&&Time.frameCount!=menuClosedFrame)
                ReleaseCursor();
            else if (!menuOpen && Application.isFocused && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
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
            interact?.Disable();
            sprint?.Disable();
            drop?.Disable();
            crouch?.Disable();
            weld?.Disable();
            grab?.Disable();
            ReleaseCursor();
        }

        private void OnDestroy()
        {
            move?.Dispose();
            look?.Dispose();
            jump?.Dispose();
            interact?.Dispose();
            sprint?.Dispose();
            drop?.Dispose();
            crouch?.Dispose();
            weld?.Dispose();
            grab?.Dispose();
        }
    }
}
