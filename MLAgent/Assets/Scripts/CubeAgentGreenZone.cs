using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CubeAgentRaysGreenZone : Agent
{
    public Transform Target;
    public Transform GreenZone;

    public float speedMultiplier = 0.5f;
    public float rotationMultiplier = 5f;

    // controle variable voor een gepakt blokje
    private bool hasCollectedBlock = false;

    public override void OnEpisodeBegin()
    {
        // Reset voor als die gevallen is en voor begin van test
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

        // Reset status
        hasCollectedBlock = false;
        Target.gameObject.SetActive(true);

        // verplaats Target naar een locatie
        Target.localPosition = new Vector3(UnityEngine.Random.value * 8 - 4, 0.5f, UnityEngine.Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Positie van de agent
        sensor.AddObservation(this.transform.localPosition);

        // Vertel het brein in welke fase we zitten (1 waarde)
        sensor.AddObservation(hasCollectedBlock);

        // Vertel waar de doelen zijn (3 + 3 = 6 waardes)
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(GreenZone.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actionBuffers.ContinuousActions[0];

        transform.Translate(controlSignal * speedMultiplier);
        transform.Rotate(0.0f, rotationMultiplier * actionBuffers.ContinuousActions[1], 0.0f);

        AddReward(-0.001f);

        if (this.transform.localPosition.y < 0)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Looptdoor de trigger van tqrget
        if (other.transform == Target && !hasCollectedBlock)
        {
            hasCollectedBlock = true;
            Target.gameObject.SetActive(false);
            AddReward(0.5f);
        }

        // Loopt door de trigger van groene zone en heeft blokje
        if (other.transform == GreenZone && hasCollectedBlock)
        {
            SetReward(1.0f);
            EndEpisode();
        }
    }
}