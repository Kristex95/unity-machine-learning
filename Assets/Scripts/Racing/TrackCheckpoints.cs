using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour
{
    private List<CheckpointSingle> checkpointSingleList;

    private void Awake()
    {
        Transform checkpointsTransform = transform.Find("Checkpoints");

        checkpointSingleList = new List<CheckpointSingle>();

        foreach (Transform checkpointSingleTransform in checkpointsTransform)
        {
            CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();

            checkpointSingle.SetTrackCheckpoints(this);

            checkpointSingleList.Add(checkpointSingle);

            Debug.Log(checkpointSingle.name);
        }

    }

    public void CarThroughCheckpoint(CheckpointSingle checkpointSingle, CarAgent carAgent)
    {
        Debug.Log(checkpointSingle.name);
        Debug.Log(checkpointSingleList.IndexOf(checkpointSingle));
        Debug.Log(carAgent.checkpointIndex);
        if (checkpointSingleList.IndexOf(checkpointSingle) == carAgent.checkpointIndex)
        {
            carAgent.checkpointIndex = (carAgent.checkpointIndex + 1) % checkpointSingleList.Count;
            carAgent.OnRightCheckpoint();
        }
        else
        {
            carAgent.OnWrongCheckpoint();
        }
    }

    public CheckpointSingle GetNextCheckpoint(int index)
    {
        return checkpointSingleList[(index + 1) % checkpointSingleList.Count];
    }
}
