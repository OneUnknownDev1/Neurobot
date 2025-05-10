using UnityEngine;

[CreateAssetMenu(fileName = "NewServoMotor", menuName = "Motor/ServoMotor")]
public class ServoMotor : ScriptableObject
{
    public float ratedTorque = 0.5f;  // Nm
    public float ratedSpeed = 60.0f;  // RPM
    public float voltageRating = 6.0f; // V
    public float inertia = 0.01f;     // kg·m²
    public float weight = 0.1f;       // kg
}