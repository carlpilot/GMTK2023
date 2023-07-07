using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    CharacterController cc;

    Vector2 rotation = Vector2.zero;
	public float rotationSpeed = 3;

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

        cc.Move(moveVec.normalized * Time.deltaTime * 5f);

        rotation.y += Input.GetAxis("Mouse X");
		rotation.x += -Input.GetAxis("Mouse Y");
		transform.eulerAngles = (Vector2)rotation * rotationSpeed;
    }
}
