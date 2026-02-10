using System.Collections.Generic;
using UnityEngine;


public class TankAi : MonoBehaviour
{
    public WPManager wpManager; 
    public float speed = 5.0f;
    public float rotSpeed = 2.0f;

    int currentIdx = 0;
    bool isMoving = false;
    GameObject startNode;

    void Start()
    {
        if (wpManager.waypoints.Length > 0)
        {
            startNode = wpManager.waypoints[0];
            transform.position = startNode.transform.position;
        }
    }

    public void GoToPalm(int targetIndex)
    {

        if (targetIndex >= wpManager.waypoints.Length || targetIndex < 0) return;

        GameObject endNode = wpManager.waypoints[targetIndex];

        
        if (startNode == null) startNode = wpManager.waypoints[0];

        
        bool pathFound = wpManager.graph.AStar(startNode, endNode);

        if (pathFound)
        {
            Debug.Log("Pad gevonden naar Palm " + targetIndex);
            currentIdx = 0;
            isMoving = true;
        }
    }

    void Update()
    {
        if (!isMoving) return;

        List<Node> path = wpManager.graph.pathList;

        if (currentIdx >= path.Count)
        {
            isMoving = false;
            if (path.Count > 0)
                startNode = path[path.Count - 1].getID();
            return;
        }

        Vector3 destination = path[currentIdx].getID().transform.position;
        Vector3 direction = destination - transform.position;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotSpeed);
        }

        transform.Translate(0, 0, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, destination) < 1.0f)
        {
            currentIdx++;
        }
    }
}