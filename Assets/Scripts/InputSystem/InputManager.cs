using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class InputManager : MonoBehaviour {
    public static InputManager Instance;

    public enum ControlScheme {
        Keyboard,
        Gamepad,
    }

    public delegate void ControlSchemeChange(ControlScheme newControlScheme);
    public event ControlSchemeChange controlSchemeChange;

    private PlayerInput input;
    public GameplayActionMap Gameplay { get; private set; }
    public DialogueActionMap Dialogue { get; private set; }
    public MenuActionMap Menu { get; private set; }
    public ControlScheme CurrentControlScheme { get; private set; }

    private void OnEnable() {
        if (Instance) {
            Destroy(gameObject);
            return;
        } else {
            Instance = this;
        }

        input = GetComponent<PlayerInput>();
        Gameplay = new GameplayActionMap(input.actions.FindActionMap("Gameplay"));
        Menu = new MenuActionMap(input.actions.FindActionMap("Menu"));
        Dialogue = new DialogueActionMap(input.actions.FindActionMap("Dialogue"));
        Gameplay.SendInitialStatusEvent();
        Menu.SendInitialStatusEvent();
    }

    public void SwitchTo(InternalActionMap map) {
        if (Gameplay.enabled && map != Gameplay) {
            Gameplay.Disable();
        }
        if (Menu.enabled && map != Menu) {
            Menu.Disable();
        }

        map.Enable();
    }

    void OnControlsChanged(PlayerInput playerInput) {
        CurrentControlScheme = playerInput.currentControlScheme.Contains("Keyboard") ? ControlScheme.Keyboard : ControlScheme.Gamepad;
        controlSchemeChange?.Invoke(CurrentControlScheme);

        if (CurrentControlScheme == ControlScheme.Keyboard && !Cursor.visible) {
            Cursor.visible = true;
        } else if (CurrentControlScheme == ControlScheme.Gamepad && Cursor.visible) {
            Cursor.visible = false;
        }
    }

    public class InternalActionMap {
        public delegate void ActionMapStatusChanged(bool enabled);
        public event ActionMapStatusChanged statusChanged;

        protected InputActionMap inputActions;

        public bool enabled { get { return inputActions.enabled; } }

        public InternalActionMap(InputActionMap inputActions) {
            this.inputActions = inputActions;
        }

        internal void SendInitialStatusEvent() {
            statusChanged?.Invoke(enabled);
        }

        public void Enable() {
            inputActions.Enable();
            statusChanged?.Invoke(true);
        }

        public void Disable() {
            inputActions.Disable();
            statusChanged?.Invoke(false);
        }
    }

    public class GameplayActionMap : InternalActionMap {
        public GameplayActionMap(InputActionMap map) : base(map) { }

        public InputAction Pan { get { return inputActions.FindAction("Pan"); } }
        public InputAction Zoom { get { return inputActions.FindAction("Zoom"); } }
        public InputAction Scan { get { return inputActions.FindAction("Scan"); } }
        public InputAction Enhance { get { return inputActions.FindAction("Enhance"); } }
        public InputAction SwitchLeft { get { return inputActions.FindAction("SwitchLeft"); } }
        public InputAction SwitchRight { get { return inputActions.FindAction("SwitchRight"); } }
        public InputAction QuitGame { get { return inputActions.FindAction("QuitGame"); } }
    }

    public class MenuActionMap : InternalActionMap {
        public MenuActionMap(InputActionMap map) : base(map) { }

        public InputAction Select { get { return inputActions.FindAction("Select"); } }
        public InputAction OK { get { return inputActions.FindAction("OK"); } }
        public InputAction Back { get { return inputActions.FindAction("Back"); } }
        public InputAction QuitGame { get { return inputActions.FindAction("QuitGame"); } }
    }

    public class DialogueActionMap : InternalActionMap {
        public DialogueActionMap(InputActionMap map) : base(map) { }

        public InputAction Advance { get { return inputActions.FindAction("Advance"); } }
        public InputAction QuitGame { get { return inputActions.FindAction("QuitGame"); } }
    }
}