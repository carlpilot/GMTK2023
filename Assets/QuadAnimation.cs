using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadAnimation : MonoBehaviour
{
    public Transform frontLeft;
    public Transform frontRight;
    public Transform backLeft;
    public Transform backRight;

    public Transform frontLeftShoulder;
    public Transform frontRightShoulder;
    public Transform backLeftShoulder;
    public Transform backRightShoulder;

    public LayerMask walkable;

    Vector3 lastBodyPos;
    Vector3 bodyVel;

    public float DEBUGWalkSpeed = 2f;

    float t;

    public float bodyVelDampingAlpha = 0.1f;

    public float stepHeight = 0.1f;

    public float horizontalMultiplier = 0.5f;

    public float stepTime = 1f;

    void Awake()
    {
        lastBodyPos = transform.position;
    }
    
    void Start()
    {
        
    }
    
    void Update()
    {
        Vector3 bodyPosDelta = transform.position - lastBodyPos;
        lastBodyPos = transform.position;
        bodyVel = bodyVelDampingAlpha*(bodyPosDelta / Time.deltaTime) + (1-bodyVelDampingAlpha)*bodyVel;

        if (false && bodyVel.magnitude <= 0.1){
            frontRight.position = floorPosBelow(frontRightShoulder.position);
            frontLeft.position = floorPosBelow(frontLeftShoulder.position);
            backRight.position = floorPosBelow(backRightShoulder.position);
            backLeft.position = floorPosBelow(backLeftShoulder.position);
        } else{
            // Walking
            var yA = Mathf.Clamp(Mathf.Sin(t*Mathf.PI*2), 0, 1);
            var yB = Mathf.Clamp(Mathf.Sin((t+0.25f)*Mathf.PI*2), 0, 1);
            var yC = Mathf.Clamp(Mathf.Sin((t+0.5f)*Mathf.PI*2), 0, 1);
            var yD = Mathf.Clamp(Mathf.Sin((t+0.75f)*Mathf.PI*2), 0, 1);

            var xA = Mathf.Sin(t*Mathf.PI*2);
            var xB = Mathf.Sin((t+0.25f)*Mathf.PI*2);
            var xC = Mathf.Sin((t+0.5f)*Mathf.PI*2);
            var xD = Mathf.Sin((t+0.75f)*Mathf.PI*2);

            var vx = bodyVel.x*horizontalMultiplier*stepTime;
            var vz = bodyVel.z*horizontalMultiplier*stepTime;

            frontRight.position = floorPosBelow(frontRightShoulder.position) + new Vector3(xA*vx, yA*stepHeight, xA*vz);
            frontLeft.position = floorPosBelow(frontLeftShoulder.position) + new Vector3(xC*vx, yC*stepHeight, xC*vz);
            backRight.position = floorPosBelow(backRightShoulder.position) + new Vector3(xD*vx, yD*stepHeight, xD*vz);
            backLeft.position = floorPosBelow(backLeftShoulder.position) + new Vector3(xB*vx, yB*stepHeight, xB*vz);
        }

        t += Time.deltaTime / stepTime;
        t = Mathf.Repeat(t, 1);
    }

    Vector3 floorPosBelow(Vector3 shoulderPos){
        RaycastHit hit;
        if (Physics.Raycast(shoulderPos, Vector3.down, out hit, 10, walkable))
        {
            return hit.point;
        }
        return shoulderPos-Vector3.up*10;
    }
}
