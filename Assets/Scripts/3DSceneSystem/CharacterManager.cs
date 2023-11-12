using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour {
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float rotationSmoothTime;

    private Vector2 rawInput;
    private Vector3 moveInput;
    private float currentVelocity;

    private void OnEnable() {
        InputManager.Instance.Gameplay.Pan.performed += OnMove;
        moveInput = new Vector3(0, gravity, 0);
    }

    private void OnDisable() {
        InputManager.Instance.Gameplay.Pan.performed -= OnMove;
    }

    private void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        rawInput = obj.ReadValue<Vector2>();
        moveInput = new Vector3(rawInput.x * movementSpeed, gravity, rawInput.y * movementSpeed);
    }

    private void Update() {
        if (rawInput.sqrMagnitude != 0) {
            var targetAngle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg;
            var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
        characterController.Move(moveInput * Time.deltaTime);
    }
}
