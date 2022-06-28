using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public event System.Action OnReachedWinZone;

    public int moveSpeed = 8;
    public float smoothMoveTime = .1f;
    public float turnSpeed = 8;

    float angle;
    float smoothInputMagnitude;
    float smoothMoveVelocity;
    Vector3 velocity;
    bool disabled;

    Rigidbody myRigidbody;

    private void Start() {
        disabled = false;
        myRigidbody = GetComponent<Rigidbody>();
        Guard.OnPlayerSeen += Disable;
    }

    private void Update() {
        MoveOnInput();
    }

    private void MoveOnInput() {
        Vector3 inputDirection = Vector3.zero;
        if(!disabled) {
            inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        }
        float inputMagnitude = inputDirection.magnitude;
        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, inputMagnitude, ref smoothMoveVelocity, smoothMoveTime);

        // Compute angle rotation to face direction
        float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
        angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * turnSpeed * inputMagnitude);

        // Compute velocity to move towards direction
        velocity = transform.forward * moveSpeed * smoothInputMagnitude;
    }

    private void Disable() {
        disabled = true;
    }

    private void FixedUpdate() {
        myRigidbody.MoveRotation(Quaternion.Euler(Vector3.up * angle));
        myRigidbody.MovePosition(myRigidbody.position + velocity * Time.deltaTime);
    }

    private void OnDestroy() {
        Guard.OnPlayerSeen -= Disable;
    }

    private void OnTriggerEnter(Collider triggerCollider) {
        if(triggerCollider.tag == "WinZone") {
            Disable();
            if(OnReachedWinZone != null) {
                OnReachedWinZone();
            }
        }
    }
}
