using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class FootballVersusAgent : Agent
{
    [SerializeField] private bool isTraining = true;

    [SerializeField] private RayPerceptionSensorComponent3D eyeSensorFroward;
    [SerializeField] private RayPerceptionSensorComponent3D eyeSensorBackward;
    [SerializeField] private Transform goalTransform; // Opponent's goal
    [SerializeField] private Transform ballTransform;
    [SerializeField] private Rigidbody ballRb;
    [SerializeField] private float moveForce = 50f;
    [SerializeField] private float turnSpeed = 200f;
    private Rigidbody rb;

    private Vector3 startingPosition;

    private void Awake()
    {
        startingPosition = this.transform.localPosition;
        rb = GetComponent<Rigidbody>();
    }

    // This is now called by the Environment Controller, not automatically
    public override void OnEpisodeBegin()
    {
        this.transform.localPosition = startingPosition + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 0));
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Local velocity (How fast am I moving relative to where I'm facing?)
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);

        // Angular velocity (Am I spinning fast?)
        sensor.AddObservation(rb.angularVelocity.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        // Create a direction vector based on the agent's OWN orientation
        Vector3 moveDirection = transform.forward * moveZ + transform.right * moveX;

        rb.AddForce(moveDirection * moveForce);

        // NEW: Rotation Action (Index 2)
        float rotateY = actions.ContinuousActions[2];
        // We apply torque on the Y axis to spin left/right
        rb.AddTorque(transform.up * rotateY * turnSpeed, ForceMode.Acceleration);

        var rayOutputForward = RayPerceptionSensor.Perceive(eyeSensorFroward.GetRayPerceptionInput());
        foreach (var ray in rayOutputForward.RayOutputs)
        {
            if (ray.HitTagIndex == 0) // Assuming "Ball" is the first tag in your list
            {
                AddReward(0.001f);
                break;
            }
        }

        var rayOutputBackward = RayPerceptionSensor.Perceive(eyeSensorBackward.GetRayPerceptionInput());
        foreach (var ray in rayOutputBackward.RayOutputs)
        {
            if (ray.HitTagIndex == 0) // Assuming "Ball" is the first tag in your list
            {
                AddReward(-0.001f);
                break;
            }
        }

        AddReward(-0.005f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        continuousActions[1] = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        // NEW: Manual rotation control (e.g., using Q and E keys)
        float rotation = 0f;
        if (Input.GetKey(KeyCode.E)) rotation = 1f;
        if (Input.GetKey(KeyCode.Q)) rotation = -1f;
        continuousActions[2] = rotation;
    }

    public void OnGoal()
    {
        if (!isTraining) return;
                
        AddReward(+1f);
        EndEpisode(); 
    }
}
