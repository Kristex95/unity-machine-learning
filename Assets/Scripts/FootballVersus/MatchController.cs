using UnityEngine;

public class MatchController : MonoBehaviour
{
    [SerializeField] private FootballVersusAgent agentA;
    [SerializeField] private FootballVersusAgent agentB;
    [SerializeField] private Gate gateA;
    [SerializeField] private Gate gateB;

    [SerializeField] private int stepThreshold;
    [SerializeField] private Rigidbody ballRb;

    private Vector3 ballStartingPos = new Vector3(0, 0.375f, 0);

    public void ResolveGoal(Gate goalGate)
    {
        if (goalGate == gateA)
        {
            agentB?.AddReward(1f);
            agentA?.AddReward(-1f);
        } 
        else 
        {
            agentA?.AddReward(1f);
            agentB?.AddReward(-1f);
        }

        ResetScene();
    }

    private void FixedUpdate()
    {
        if (agentA.StepCount >= stepThreshold || agentB.StepCount >= stepThreshold)
        {
            ResetScene();
        }
    }

    public void ResetScene()
    {
        agentA?.EndEpisode();
        agentB?.EndEpisode();

        // Reset Ball
        ballRb.transform.localPosition = ballStartingPos;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
    }
}
