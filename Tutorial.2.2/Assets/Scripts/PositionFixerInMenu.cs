using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

public class PositionFixerInMenu : MonoBehaviour
{
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    public Transform wheelFLTransform;
    public Transform wheelFRTransform;
    public Transform wheelRLTransform;
    public Transform wheelRRTransform;

    void LateUpdate()
    {
        transform.position = new Vector3(0f, transform.position.y, 0f);
        wheelFLTransform.rotation = new Quaternion(transform.rotation.x, 0f,0f, 0f);
        wheelFRTransform.rotation = new Quaternion(transform.rotation.x, 0f, 0f, 0f);
        wheelRLTransform.rotation = new Quaternion(transform.rotation.x, 0f, 0f, 0f);
        wheelRRTransform.rotation = new Quaternion(transform.rotation.x, 0f, 0f, 0f);
    }
}
