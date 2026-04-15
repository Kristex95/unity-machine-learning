using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class MoveToGoal : Agent
{
    [SerializeField] private bool training = true;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenederer;

    public override void OnEpisodeBegin()
    {
        if (!training) return;

        transform.localPosition = Vector3.zero;

        float angle = Random.Range(0f, Mathf.PI * 2);

        float minDistance = 1.3f;
        float maxDistance = 4f;
        float distance = Random.Range(minDistance, maxDistance);

        float spawnX = Mathf.Cos(angle) * distance;
        float spawnZ = Mathf.Sin(angle) * distance;

        targetTransform.localPosition = new Vector3(spawnX, 0.5f, spawnZ);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        float moveSpeed = 2f;
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;

        AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!training) return;

        if (other.TryGetComponent<Target>(out Target target))
        {
            SetReward(+1f);
            floorMeshRenederer.material = winMaterial;
            EndEpisode();
        }
        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-1f);
            floorMeshRenederer.material = loseMaterial;
            EndEpisode();
        }
    }
}
