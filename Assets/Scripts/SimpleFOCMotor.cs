using UnityEngine;

public class SimpleFOCMotor : MonoBehaviour
{
    public int motorId = 0;
    public ServoMotor motorSpecs;
    public float P = 0.05f, I = 0.02f, D = 0.0f;
    public float target = 0.0f; // Target velocity (rad/s)

    private Rigidbody rb;
    private float integral = 0.0f;
    private float prevError = 0.0f;
    private const float RPM_TO_RADS = 2 * Mathf.PI / 60;

    void Start()
    {
        if (motorSpecs == null)
        {
            Debug.LogError("No ServoMotor assigned!");
            return;
        }
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody on bone!");
            return;
        }
        rb.mass = motorSpecs.weight;
        rb.inertiaTensor = new Vector3(motorSpecs.inertia, motorSpecs.inertia, motorSpecs.inertia);
    }

    void FixedUpdate()
    {
        if (motorSpecs == null || rb == null) return;

        float dt = Time.fixedDeltaTime;
        float currentVelocity = rb.angularVelocity.z;
        float error = target - currentVelocity;

        integral += error * dt;
        float derivative = (error - prevError) / dt;
        float output = P * error + I * integral + D * derivative;

        float torque = output * motorSpecs.ratedTorque / motorSpecs.voltageRating;
        rb.AddTorque(torque * Vector3.forward, ForceMode.Force);

        float maxVel = motorSpecs.ratedSpeed * RPM_TO_RADS;
        rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxVel);

        prevError = error;
    }

    public void UpdateParameters(float p, float i, float d, float targetVel)
    {
        P = p; I = i; D = d; target = targetVel;
    }
}