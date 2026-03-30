using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ObelixAgent : Agent
{
    [Header("Settings")]
    public float moveSpeed = 5.0f; // Increased speed for the larger field
    public float turnSpeed = 300f;

    [Header("Visual Feedback")]
    public GameObject menhirOnBack;

    [Header("Game Objects (Lists)")]
    public List<GameObject> menhirsInScene;
    public List<GameObject> destinationsInScene;

    [Header("Materials")]
    public Material emptyDestinationMat;
    public Material fullDestinationMat;

    private bool hasMenhir = false;
    private Rigidbody rb;
    private int deliveredCount = 0;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset Agent
        hasMenhir = false;
        menhirOnBack.SetActive(false);
        deliveredCount = 0;

        // Spawn Obelix randomly in the larger field (-18 to 18 for a 40x40 plane)
        this.transform.localPosition = new Vector3(Random.Range(-18f, 18f), 0.5f, Random.Range(-18f, 18f));
        this.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset Menhirs (Random positions every episode)
        foreach (GameObject menhir in menhirsInScene)
        {
            menhir.SetActive(true);
            menhir.transform.localPosition = new Vector3(Random.Range(-18f, 18f), 0.5f, Random.Range(-18f, 18f));

            Rigidbody mRb = menhir.GetComponent<Rigidbody>();
            if (mRb != null)
            {
                mRb.linearVelocity = Vector3.zero;
                mRb.angularVelocity = Vector3.zero;
            }
        }

        // Reset Destinations (FIXED POSITIONS)
        // We removed the position change here. Now they just stay where you placed them in the Unity Editor!
        foreach (GameObject dest in destinationsInScene)
        {
            dest.GetComponent<MeshRenderer>().material = emptyDestinationMat;
            dest.tag = "Destination";
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Is he carrying a menhir?
        sensor.AddObservation(hasMenhir ? 1.0f : 0.0f);

        // How many have been delivered?
        sensor.AddObservation((float)deliveredCount);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveAction = actions.DiscreteActions[0];
        int turnAction = actions.DiscreteActions[1];

        // Forward movement only (0 = idle, 1 = forward)
        float moveAmount = 0f;
        if (moveAction == 1) moveAmount = 1f;

        float turnAmount = 0f;
        if (turnAction == 1) turnAmount = 1f;
        if (turnAction == 2) turnAmount = -1f;

        transform.Translate(Vector3.forward * moveAmount * moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(Vector3.up * turnAmount * turnSpeed * Time.fixedDeltaTime);

        // Slightly lower speed penalty so he has more time to search
        AddReward(-0.0005f);

        // Penalty for falling off the edge
        if (this.transform.localPosition.y < 0)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        discreteActionsOut[1] = 0;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) discreteActionsOut[0] = 1;

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) discreteActionsOut[1] = 1;
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) discreteActionsOut[1] = 2;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Menhir"))
        {
            if (!hasMenhir)
            {
                hasMenhir = true;
                menhirOnBack.SetActive(true);
                AddReward(0.1f);
                collision.gameObject.SetActive(false);
            }
            else
            {
                AddReward(-0.05f); // Penalty for bumping into a menhir while carrying one
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Destination"))
        {
            if (hasMenhir)
            {
                // Delivered successfully
                hasMenhir = false;
                menhirOnBack.SetActive(false);

                other.GetComponent<MeshRenderer>().material = fullDestinationMat;
                other.tag = "FullDestination";

                deliveredCount++;

                if (deliveredCount >= destinationsInScene.Count)
                {
                    // All 10 delivered! Big reward.
                    AddReward(1.0f);
                    EndEpisode();
                }
                else
                {
                    // Delivered one, but not finished yet. Increased reward for motivation!
                    AddReward(0.8f);
                }
            }
            else
            {
                AddReward(-0.01f); // Penalty for trying to deliver without a menhir
            }
        }
        else if (other.CompareTag("FullDestination"))
        {
            AddReward(-0.01f); // Penalty for bumping into an already full destination
        }
    }
}