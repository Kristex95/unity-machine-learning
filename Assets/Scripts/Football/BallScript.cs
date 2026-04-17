using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour
{

    [SerializeField] private FootballAgent lastFootballAgentHit;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<FootballAgent>(out FootballAgent footballAgent))
        {
            lastFootballAgentHit = footballAgent;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (lastFootballAgentHit != null)
        {
            lastFootballAgentHit.OnGoal();
        }
    }
}
