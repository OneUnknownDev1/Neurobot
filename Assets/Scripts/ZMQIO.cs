using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;

public class ZMQIO : MonoBehaviour
{
    private SubscriberSocket socket;
    private Dictionary<int, SimpleFOCMotor> motors = new Dictionary<int, SimpleFOCMotor>();

    void Start()
    {
        foreach (var motor in FindObjectsOfType<SimpleFOCMotor>())
            motors[motor.motorId] = motor;

        AsyncIO.ForceDotNet.Force();
        socket = new SubscriberSocket();
        socket.Connect("tcp://localhost:5555");
        socket.Subscribe("");
    }

    void Update()
    {
        while (socket.TryReceiveFrameString(out string message))
        {
            try
            {
                var data = JsonUtility.FromJson<MotorData>(message);
                if (motors.TryGetValue(data.motor_id, out var motor))
                {
                    motor.UpdateParameters(data.P, data.I, data.D, data.target);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse ZMQ message: {e.Message}");
            }
        }
    }

    void OnDestroy()
    {
        socket?.Dispose();
        NetMQConfig.Cleanup();
    }

    [Serializable]
    private struct MotorData
    {
        public int motor_id;
        public float P, I, D, target;
    }
}