using UnityEngine;

public class WheelBehaviour : MonoBehaviour
{
    public WheelCollider wheelCol; // wheel colider object
                                   // Use this for initialization
    void Start() { }
    // Update is called once per frame
    void Update()
    {
        // Get the wheel position and rotation from the wheelcolider
        Quaternion quat;
        Vector3 position;
        wheelCol.GetWorldPose(out position, out quat);
        transform.position = position;
        transform.rotation = quat;
    }
}
