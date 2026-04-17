using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BallBalanceAgent : Agent
{
    [SerializeField] private bool isTraining;
    [SerializeField] private Transform ballTransform;
    [SerializeField] private Rigidbody ballRigidbody;
    
    private float ballPadding = 0.25f;

    public override void OnEpisodeBegin()
    {
        // 1. Reset Platform first
        transform.rotation = Quaternion.identity;

        // 2. Reset Ball Physics
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;

        // 3. Place Ball randomly on top
        float xRange = (transform.localScale.x / 2) - ballPadding;
        float zRange = (transform.localScale.z / 2) - ballPadding;

        ballTransform.localPosition = new Vector3(
            Random.Range(-xRange, xRange),
            1.5f, // A bit of height so it drops onto the plate
            Random.Range(-zRange, zRange)
        );
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.rotation);
        sensor.AddObservation(ballTransform.localPosition - transform.localPosition);
        sensor.AddObservation(ballRigidbody.velocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {

        float rotationX = actions.ContinuousActions[0];
        float rotationZ = actions.ContinuousActions[1];

        float rotationSpeed = 50f;

        transform.Rotate(
            rotationX * rotationSpeed * Time.deltaTime,
            0,
            -rotationZ * rotationSpeed * Time.deltaTime
        );

        AddReward(0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Vertical");
        continuousActions[1] = Input.GetAxisRaw("Horizontal");
    }

    private void FixedUpdate()
    {
        if (!isTraining) return;

        if (ballTransform.localPosition.y < transform.localPosition.y)
        {
            SetReward(-1f);
            EndEpisode();
        }
    }
}
