using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

public class ArticulationBodyNetMQ : MonoBehaviour
{
    [SerializeField]
    public ArticulationBody articulationBody;

    [SerializeField]
    public string publisherAddress = "tcp://*:5555";

    [SerializeField]
    public string subscriberAddress = "tcp://*:5556";

    private PublisherSocket publisherSocket;
    private SubscriberSocket subscriberSocket;
    private Thread publisherThread;
    private Thread subscriberThread;
    private volatile bool isRunning;
    private ConcurrentQueue<string> commandQueue;
    private ConcurrentQueue<string> dataQueue;

    void Reset()
    {
        // Check the current GameObject for an ArticulationBody component
        articulationBody = GetComponent<ArticulationBody>();
        if (articulationBody == null)
        {
            Debug.LogError("ArticulationBody component not assigned in the Inspector.", this);
        }
    }

    void Start()
    {
        // Initialize queues for thread-safe communication
        commandQueue = new ConcurrentQueue<string>();
        dataQueue = new ConcurrentQueue<string>();

        // Initialize publisher socket
        publisherSocket = new PublisherSocket();
        try
        {
            publisherSocket.Bind(publisherAddress);
            Debug.Log($"Publisher socket bound to {publisherAddress}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to bind publisher socket: {e.Message}");
            return;
        }

        // Initialize subscriber socket
        subscriberSocket = new SubscriberSocket();
        try
        {
            subscriberSocket.Bind(subscriberAddress);
            subscriberSocket.Subscribe(""); // Subscribe to all topics
            Debug.Log($"Subscriber socket bound to {subscriberAddress}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to bind subscriber socket: {e.Message}");
            publisherSocket?.Close();
            publisherSocket?.Dispose();
            return;
        }

        // Start NetMQ threads
        isRunning = true;
        publisherThread = new Thread(PublisherThreadLoop);
        publisherThread.IsBackground = true;
        publisherThread.Start();

        subscriberThread = new Thread(SubscriberThreadLoop);
        subscriberThread.IsBackground = true;
        subscriberThread.Start();
    }

    void PublisherThreadLoop()
    {
        while (isRunning)
        {
            try
            {
                if (dataQueue.TryDequeue(out string data))
                {
                    // Send data with topic (command name)
                    string[] parts = data.Split(new[] { ':' }, 2);
                    string topic = parts[0];
                    publisherSocket.SendMoreFrame(topic).SendFrame(data);
                }
                else
                {
                    // Avoid busy looping
                    Thread.Sleep(1);
                }
            }
            catch (Exception e)
            {
                if (isRunning)
                {
                    Debug.LogError($"Publisher thread error: {e.Message}");
                }
                break;
            }
        }
    }

    void SubscriberThreadLoop()
    {
        while (isRunning)
        {
            try
            {
                // Blocking receive
                string message = subscriberSocket.ReceiveFrameString();
                commandQueue.Enqueue(message);
            }
            catch (Exception e)
            {
                if (isRunning)
                {
                    Debug.LogError($"Subscriber thread error: {e.Message}");
                }
                break;
            }
        }
    }

    void Update()
    {
        // Process incoming commands
        while (commandQueue.TryDequeue(out string message))
        {
            string response = ProcessMessage(message);
            if (response != null)
            {
                dataQueue.Enqueue(response);
            }
        }
    }

    string ProcessMessage(string message)
    {
        try
        {
            string[] parts = message.Split(new[] { ':' }, 2);
            string command = parts[0];

            switch (command)
            {
                case "SET_JOINT_FORCES":
                    // Expected format: SET_JOINT_FORCES:force1,force2,...
                    string[] forceStrings = parts[1].Split(',');
                    List<float> currentForces = new List<float>();
                    articulationBody.GetJointForces(currentForces);
                    List<float> newForces = new List<float>(currentForces);

                    for (int i = 0; i < forceStrings.Length && i < newForces.Count; i++)
                    {
                        float additionalForce = float.Parse(forceStrings[i]);
                        newForces[i] += additionalForce;
                    }

                    articulationBody.SetJointForces(newForces);
                    return "SET_JOINT_FORCES:OK";

                case "GET_DOF_START_INDICES":
                    List<int> dofStartIndices = new List<int>();
                    articulationBody.GetDofStartIndices(dofStartIndices);
                    return $"GET_DOF_START_INDICES:{string.Join(",", dofStartIndices)}";

                case "GET_JOINT_POSITIONS":
                    List<float> positions = new List<float>();
                    articulationBody.GetJointPositions(positions);
                    return $"GET_JOINT_POSITIONS:{string.Join(",", positions)}";

                case "GET_JOINT_VELOCITIES":
                    List<float> velocities = new List<float>();
                    articulationBody.GetJointVelocities(velocities);
                    return $"GET_JOINT_VELOCITIES:{string.Join(",", velocities)}";

                case "GET_JOINT_FORCES":
                    List<float> forces = new List<float>();
                    articulationBody.GetJointForces(forces);
                    return $"GET_JOINT_FORCES:{string.Join(",", forces)}";

                case "GET_JOINT_EXTERNAL_FORCES":
                    List<float> externalForces = new List<float>();
                    int dofCount = articulationBody.GetJointExternalForces(externalForces, Time.fixedDeltaTime);
                    return $"GET_JOINT_EXTERNAL_FORCES:{dofCount},{string.Join(",", externalForces)}";

                default:
                    return $"ERROR:Unknown command:{command}";
            }
        }
        catch (Exception e)
        {
            return $"ERROR:{e.Message}";
        }
    }

    void OnDestroy()
    {
        // Stop NetMQ threads and clean up resources
        isRunning = false;

        if (publisherThread != null && publisherThread.IsAlive)
        {
            publisherThread.Join(100);
        }

        if (subscriberThread != null && subscriberThread.IsAlive)
        {
            subscriberThread.Join(100);
        }

        publisherSocket?.Close();
        publisherSocket?.Dispose();
        subscriberSocket?.Close();
        subscriberSocket?.Dispose();
        NetMQConfig.Cleanup(false);
    }
}