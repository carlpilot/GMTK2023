using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerMovement : MonoBehaviour {
    CharacterController cc;

    public Transform camera;

    float yaw = 0;
    float pitch = 0;

    public bool isDeer = false;

    Volume deerVol;
    Vignette exhaustionVingette;

    [Header("Camera")]
    public float rotationSpeed = 3;


    [Header("Speed")]
    public float sprintSpeed = 5f;
    public float walkSpeed = 5f;
    public float crouchSpeed = 2f;

    [Header("State")]
    public bool isCrouching = false;
    public bool isSprinting = false;
    public bool isTrapped = false;

    [Header("Noise")]
    public float noiseRange = 30f;
    public float crouchNoiseRange = 15f;
    public float sprintNoiseRange = 50f;

    [Header("Trapping")]
    public float chanceToFreeFromTrap = 0.05f;

    [Header("Sprinting")]
    public float exhaustionVingetteMinIntensity = 0f;
    public float exhaustionVingetteMaxIntensity = 1f;
    public float sprintTime = 5f;
    public float sprintCooldown = 5f;

    float sprintTimer = 0f;
    float sprintCooldownTimer = 0f;

    void Awake () {
        cc = GetComponent<CharacterController> ();
        if (isDeer){
            deerVol = GetComponentInChildren<Volume>();
            deerVol.profile.TryGet(out exhaustionVingette);
        }
        
    }

    void Start () {

    }

    void Update () {
        if (!isTrapped) {
            Vector3 moveVec = Vector3.zero;
            var moveSpeed = walkSpeed;
            if (Input.GetKey (KeyCode.W)) {
                moveVec += transform.forward;
            }
            if (Input.GetKey (KeyCode.S)) {
                moveVec -= transform.forward;
            }
            if (Input.GetKey (KeyCode.A)) {
                moveVec -= transform.right;
            }
            if (Input.GetKey (KeyCode.D)) {
                moveVec += transform.right;
            }
            if (Input.GetKey (KeyCode.LeftShift)) {
                isCrouching = true;
                isSprinting = false;
                moveSpeed = crouchSpeed;
            } else if (Input.GetKey (KeyCode.LeftControl)) {
                isCrouching = false;
                isSprinting = true;
                moveSpeed = sprintSpeed;
            } else {
                isCrouching = false;
                isSprinting = false;
                moveSpeed = walkSpeed;
            }

            if (exhaustionVingette) exhaustionVingette.intensity.value = Mathf.Lerp (exhaustionVingetteMinIntensity, exhaustionVingetteMaxIntensity, moveVec.magnitude);

            cc.Move (moveVec.normalized * Time.deltaTime * moveSpeed);

            if (moveVec.magnitude > 0) {
                Collider[] hitColliders = Physics.OverlapSphere (transform.position, (isCrouching ? crouchNoiseRange : noiseRange));
                foreach (Collider c in hitColliders) {
                    if (c.gameObject.tag == "Deer" && c.transform.parent != null && c.transform.parent.gameObject.GetComponent<DeerMovement>() != null) {
                        c.transform.parent.gameObject.GetComponent<DeerMovement>().flee(transform.position);
                    }
                }
            }
        } else {
            if (Input.GetKeyDown (KeyCode.Space) && Random.value < chanceToFreeFromTrap) isTrapped = false;
        }

        yaw += Input.GetAxis ("Mouse X") * rotationSpeed;
        pitch += -Input.GetAxis ("Mouse Y") * rotationSpeed;
        transform.eulerAngles = Vector3.up * yaw;
        camera.localEulerAngles = Vector3.right * pitch;

        if (!cc.isGrounded) {
            cc.Move (Vector3.down * Time.deltaTime * 9.81f);
        }
    }

    void OnEnable () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable () {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}