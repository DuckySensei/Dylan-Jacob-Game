using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CameraMovements : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    public void FollowOnOff(bool follow)
    {
        //set the script to active or inactive
        this.enabled = follow;
    }


    private void LateUpdate()
    {
            transform.position = target.position + offset;
    }
}