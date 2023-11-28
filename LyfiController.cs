using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LyfiController : MonoBehaviour
{
    //waypoint controller
    public bool followingPoints;
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    //stats
    public float speed = 3f;

    void Start()
    {
        if (followingPoints)
        {
            moveTowardsWaypoint();
        }
    }

    void moveTowardsWaypoint()
    {
        if (currentWaypointIndex < waypoints.Length)
        {
            Vector3 targetPosition = waypoints[currentWaypointIndex].position;
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            if (transform.position == targetPosition)
            {
                currentWaypointIndex++;
            }
        }
        else
        {
            // Reset to the first waypoint to create a looping path
            currentWaypointIndex = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (followingPoints)
        {
            moveTowardsWaypoint();
        }
    }
}
