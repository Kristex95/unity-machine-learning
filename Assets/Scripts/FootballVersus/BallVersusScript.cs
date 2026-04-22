using UnityEngine;

public class BallVersusScript : MonoBehaviour
{
    [SerializeField] private MatchController envController;
    private FootballVersusAgent lastTouch;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<FootballVersusAgent>(out var agent))
        {
            lastTouch = agent;
            agent.AddReward(0.01f); // Small reward for touching the ball
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Gate>(out Gate gate))
        {
            // If the ball enters a gate, tell the controller
            // You can also check 'gate' properties to see which side it is
            if (lastTouch != null)
            {
                envController.ResolveGoal(gate);
            }
            else
            {
                // Ball went in without touch? Just reset.
                envController.ResetScene();
            }
        }
    }
}
