using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
    private TrackCheckpoints trackCheckpoints;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CarAgent>(out CarAgent carAgent))
        {
            Debug.Log("car through checkpoint");
            trackCheckpoints.CarThroughCheckpoint(this, carAgent);
        }
    }

    public void SetTrackCheckpoints(TrackCheckpoints trackCheckpoints)
    {
        this.trackCheckpoints = trackCheckpoints;
    }
}
