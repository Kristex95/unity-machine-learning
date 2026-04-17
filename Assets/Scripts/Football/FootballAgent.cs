using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class FootballAgent : Agent
{
    [SerializeField] private bool isTraining = true;

    [SerializeField] private Transform goalTransform;
    [SerializeField] private Transform ballTransform;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Rigidbody ballRb;
    [SerializeField] [Range(1f, 500f)] private float moveForce = 10f;

    private Vector3 startingPosition;

    private void Awake()
    {
        startingPosition = this.transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        if (!isTraining) return;

        // Reset Agent
        this.transform.localPosition = startingPosition 
            + new Vector3(
                Random.Range(-1f, 0f),
                0,
                Random.Range(-2.5f, 2.5f)
            ); 
        rb.velocity = Vector3.zero;

        // Reset Ball with slight randomness
        ballTransform.localPosition = new Vector3(Random.Range(-1f, 1f), 0.375f, Random.Range(-2.5f, 2.5f));
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition); //3
        sensor.AddObservation(goalTransform.localPosition);  //3
        sensor.AddObservation(ballTransform.localPosition);  //3
        sensor.AddObservation(rb.velocity.x); //1
        sensor.AddObservation(rb.velocity.z); //1
        sensor.AddObservation(ballRb.velocity.x); //1
        sensor.AddObservation(ballRb.velocity.z); //1
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[1];
        float moveZ = actions.ContinuousActions[0];

        rb.AddForce(new Vector3(moveX, 0, -moveZ) * moveForce);

        // Optional: Existential penalty to make it move faster
        AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    public void OnGoal()
    {
        if (!isTraining) return;
                
        AddReward(+1f);
        EndEpisode(); 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<BallScript>(out BallScript ballScript))
        {
            AddReward(0.01f);
        }
    }
}
