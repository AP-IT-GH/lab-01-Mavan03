using UnityEngine;

public class FollowWaypoint : MonoBehaviour
{
    public GameObject[] waypoints; 
    public float speed = 5.0f;     
    public float rotSpeed = 2.0f;  
    public float accuracy = 2.0f;  

    private int currentWaypoint = 0;

    void Update()
    {
        if (waypoints.Length == 0) return;

        Vector3 direction = waypoints[currentWaypoint].transform.position - transform.position;

        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotSpeed);
        }

        transform.Translate(0, 0, speed * Time.deltaTime);


        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].transform.position) < accuracy)
        {
            currentWaypoint++; 

            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = 0;
            }
        }
    }
}
