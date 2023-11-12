using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D3SceneManager: MonoBehaviour {
    [Header("Camera Setup")]
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    [SerializeField] private CinemachineVirtualCamera d3Camera;

    [SerializeField] private int basePriority;
    [SerializeField] private int activePriority;

    [Header("Player")]
    [SerializeField] private Scanner scanner;
    [SerializeField] private CharacterManager characterManager;

    private void OnEnable() {
        InputManager.Instance.Gameplay.QuitGame.performed += OnSwitch;
    }

    private void OnDisable() {
        InputManager.Instance.Gameplay.QuitGame.performed -= OnSwitch;
    }

    private void OnSwitch(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        if (mainCamera.Priority == activePriority) {
            SwitchTo3D();
        } else {
            SwitchToMain();
        }
    }

    public void SwitchToMain() {
        mainCamera.Priority = activePriority;
        d3Camera.Priority = basePriority;
        characterManager.enabled = false;
        scanner.enabled = true;
    }

    public void SwitchTo3D() {
        mainCamera.Priority = basePriority;
        d3Camera.Priority = activePriority;
        characterManager.enabled = true;
        scanner.enabled = false;
    }
}
