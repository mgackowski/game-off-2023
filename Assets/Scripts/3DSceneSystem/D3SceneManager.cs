using Cinemachine;
using Cinemachine.PostFX;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class D3SceneManager: MonoBehaviour {
    [Header("Camera Setup")]
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    [SerializeField] private CinemachineVirtualCamera d3Camera;
    [SerializeField] private CinemachineVirtualCamera pinboardCamera;
    [SerializeField] private CinemachineVirtualCamera scannerCamera; // to control post-processing during ending

    [SerializeField] private int basePriority;
    [SerializeField] private int activePriority;

    [Header("Zooming")]
    [SerializeField] private float zoomSpeedKeysModifier = 1f;
    [SerializeField] private float zoomSpeedScrollModifier = 1f;
    [SerializeField] private float zoomSpeedScrollDampOver = .2f;

    [Header("Dialogue")]
    [SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] private string nodeOnMaxZoomoutReached;

    [Header("Player")]
    [SerializeField] private Scanner scanner;
    [SerializeField] private CharacterManager characterManager;

    float zoomSpeed = 0f;
    bool lastZoomByScroll;
    float currentZoomSpeedDamp = 1;
    CinemachineTrackedDolly dolly;
    CinemachineVolumeSettings scannerVolume;
    bool zoomoutReachedOnce = false;

    private void Awake()
    {
        dolly = d3Camera.GetCinemachineComponent<CinemachineTrackedDolly>();
        scannerVolume = scannerCamera.GetComponent<CinemachineVolumeSettings>();
    }

    private void Update()
    {
        if (d3Camera.Priority == activePriority)
        {
            ApplyZoom();
        }
    }

    private void OnEnable() {
        //InputManager.Instance.Gameplay.QuitGame.performed += OnSwitch;
        InputManager.Instance.Gameplay.Zoom.performed += OnZoom;
    }

    private void OnDisable() {
        //InputManager.Instance.Gameplay.QuitGame.performed -= OnSwitch;
        InputManager.Instance.Gameplay.Zoom.performed += OnZoom;
    }

    void Reset()
    {
        if (dialogueSystem == null)
        {
            dialogueSystem = GameObject.FindGameObjectWithTag("DialogueSystem").GetComponent<DialogueSystem>();
        }
    }

    private void OnSwitch(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (mainCamera.Priority == activePriority) {
            SwitchTo3D();
        } else {
            SwitchToMain();
        }
    }

    public void OnZoom(InputAction.CallbackContext ctx)
    {
        var newSpeed = ctx.ReadValue<float>();
        if (ctx.control.path.Contains("scroll"))
        {
            if (newSpeed != 0)
            {
                zoomSpeed = newSpeed;
                lastZoomByScroll = true;
                currentZoomSpeedDamp = 0;
            }
        }
        else
        {
            lastZoomByScroll = false;
            zoomSpeed = newSpeed;
        }
    }

    public void SwitchToMain() {
        mainCamera.Priority = activePriority;
        d3Camera.Priority = basePriority;
        pinboardCamera.Priority = basePriority;
        characterManager.enabled = false;
        scanner.enabled = true;

        if (scannerVolume != null)
        {
            scannerVolume.enabled = true;
        }
    }

    public void SwitchTo3D() {
        mainCamera.Priority = basePriority;
        d3Camera.Priority = activePriority;
        pinboardCamera.Priority = basePriority;
        //characterManager.enabled = true;
        scanner.enabled = false;
    }

    [YarnCommand("switchToPinboard")]
    public void SwitchToPinboard()
    {
        mainCamera.Priority = basePriority;
        d3Camera.Priority = basePriority;
        pinboardCamera.Priority = activePriority;
        characterManager.enabled = false;
        scanner.enabled = true;

        if (scannerVolume!= null)
        {
            scannerVolume.enabled = false;
        }
    }

    void ApplyZoom()
    {
        if (zoomSpeed != 0f)
        {
            float modifier = lastZoomByScroll ? zoomSpeedScrollModifier : zoomSpeedKeysModifier;

            float zoomDelta = zoomSpeed * modifier * Time.deltaTime * -1f;
            float newPathPosition = dolly.m_PathPosition + zoomDelta;

            Debug.Log(newPathPosition);
            if (newPathPosition < 0f)
            {
                SwitchToMain(); // zoomed in on screen; go back to regular gameplay
            }
            if (!zoomoutReachedOnce && newPathPosition >= 1f)
            {
                zoomoutReachedOnce = true;
                dialogueSystem.RunDialogue(nodeOnMaxZoomoutReached);
            }

            dolly.m_PathPosition = Mathf.Clamp(newPathPosition, 0f, 1f);

            if (lastZoomByScroll)
            {
                currentZoomSpeedDamp += Time.deltaTime;
                if (currentZoomSpeedDamp > zoomSpeedScrollDampOver)
                {
                    currentZoomSpeedDamp = zoomSpeedScrollDampOver;
                }
                zoomSpeed = Mathf.Lerp(zoomSpeed, 0, currentZoomSpeedDamp / zoomSpeedScrollDampOver);
            }
        }
    }
}
