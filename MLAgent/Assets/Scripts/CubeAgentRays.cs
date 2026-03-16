using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CubeAgentRays : Agent
{
    public Transform Target;
    public float speedMultiplier = 0.5f;
    public float rotationMultiplier = 5f;

    public override void OnEpisodeBegin()
    {
        // reset de positie
        if (this.transform.localPosition.y < 0)
        {
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
            this.transform.localRotation = Quaternion.identity;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // target naar een nieuwe locatie
        Target.localPosition = new Vector3(UnityEngine.Random.value * 8 - 4, 0.5f, UnityEngine.Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent positie
        sensor.AddObservation(this.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Acties, size = 2   
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actionBuffers.ContinuousActions[0];

        transform.Translate(controlSignal * speedMultiplier);
        transform.Rotate(0.0f, rotationMultiplier * actionBuffers.ContinuousActions[1], 0.0f);

        // Beloningen 
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Target bereikt
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // Van het platform gevallen?
        if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }
}