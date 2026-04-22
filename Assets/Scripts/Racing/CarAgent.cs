using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{
    [Header("Settings")]
    public float motorForce = 1500f;
    public float breakForce = 3000f;
    public float maxSteerAngle = 35f;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    [Header("Wheel Transforms")]
    public Transform frontLeftT;
    public Transform frontRightT;
    public Transform rearLeftT;
    public Transform rearRightT;

    [Header("Race")]
    public int checkpointIndex;
    public TrackCheckpoints trackCheckpoints;

    [Header("Others")]
    private Rigidbody rb;

    private float horizontalInput;
    private float verticalInput;
    private bool isBreaking;
    private Vector3 startingPosition;

    private void Awake()
    {
        this.rb = GetComponent<Rigidbody>();
        startingPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        // Reset velocity and position (Update this to your spawn point)
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        this.transform.position = startingPosition;
        this.transform.rotation = Quaternion.identity;
        checkpointIndex = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Local velocity (3 observations: x, y, z)
        // Helps the car know if it's sliding or moving backward
        sensor.AddObservation(transform.InverseTransformDirection(rb.velocity));

        // 2. Magnitude of velocity (1 observation)
        // Simple scalar for "how fast am I going"
        sensor.AddObservation(rb.velocity.magnitude);

        // Note: The Ray Perception Sensor 3D handles its own observations 
        // automatically. You don't need to add it here!

        Vector3 nextCheckpointPos = trackCheckpoints.GetNextCheckpoint(checkpointIndex).transform.position;
        Vector3 diff = nextCheckpointPos - transform.position;

        Vector3 next2CheckpointPos = trackCheckpoints.GetNextCheckpoint(checkpointIndex+1).transform.position;
        Vector3 diff2 = nextCheckpointPos - transform.position;

        // Tell the car exactly where the target is in its local space
        sensor.AddObservation(transform.InverseTransformDirection(diff.normalized));
        sensor.AddObservation(transform.InverseTransformDirection(diff2.normalized));

        float directionDot = Vector3.Dot(transform.forward, trackCheckpoints.GetNextCheckpoint(checkpointIndex).gameObject.transform.forward);
        sensor.AddObservation(directionDot);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        horizontalInput = actions.ContinuousActions[0];
        verticalInput = actions.ContinuousActions[1];

        isBreaking = (verticalInput < 0 && transform.InverseTransformDirection(rb.velocity).z > 0.5f);

        HandleMotor();
        HandleSteering();
        UpdateWheels();

        // REWARD: Directional Velocity
        // This rewards moving forward along the car's forward axis.
        float forwardSpeed = transform.InverseTransformDirection(rb.velocity).z;

        // Give a reward for moving forward, but also a tiny penalty for existing 
        // (encourages finishing faster)
        AddReward(forwardSpeed * 0.001f);
        AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }


    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        // 4WD Setup: Apply torque to all wheels
        frontLeft.motorTorque = verticalInput * motorForce;
        frontRight.motorTorque = verticalInput * motorForce;

        float currentbreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking(currentbreakForce);
    }

    private void ApplyBreaking(float force)
    {
        frontLeft.brakeTorque = force;
        frontRight.brakeTorque = force;
        rearLeft.brakeTorque = force;
        rearRight.brakeTorque = force;
    }

    private void HandleSteering()
    {
        float steerAngle = horizontalInput * maxSteerAngle;
        frontLeft.steerAngle = steerAngle;
        frontRight.steerAngle = steerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeft, frontLeftT);
        UpdateSingleWheel(frontRight, frontRightT);
        UpdateSingleWheel(rearLeft, rearLeftT);
        UpdateSingleWheel(rearRight, rearRightT);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.5f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f);
        }
    }

    public void OnRightCheckpoint()
    {
        AddReward(1.0f);
    }

    public void OnWrongCheckpoint()
    {
        SetReward(-1.0f);
        EndEpisode();
    }
}
