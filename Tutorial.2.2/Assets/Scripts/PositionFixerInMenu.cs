using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

public class PositionFixerInMenu : MonoBehaviour
{
    void LateUpdate()
    {
        transform.position = new Vector3(0f, transform.position.y, 0f);
    }
}
