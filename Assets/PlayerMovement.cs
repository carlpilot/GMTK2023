using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    CharacterController cc;

    public Transform camera;

    float yaw = 0;
    float pitch = 0;
	public float rotationSpeed = 3;
    public float runSpeed = 5f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }
    
    void Start()
    {
        
    }
    
    void Update()
    {
        Vector3 moveVec = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            moveVec += transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveVec -= transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveVec -= transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveVec += transform.right;
        }

        cc.Move(moveVec.normalized * Time.deltaTime * runSpeed);

        yaw += Input.GetAxis("Mouse X")*rotationSpeed;
		pitch += -Input.GetAxis("Mouse Y")*rotationSpeed;
		transform.eulerAngles = Vector3.up * yaw;
        camera.localEulerAngles = Vector3.right * pitch;
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
