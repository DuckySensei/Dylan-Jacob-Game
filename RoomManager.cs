using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //get a list of GameObjects with the tag "Room"
        GameObject[] rooms = GameObject.FindGameObjectsWithTag("Room");

        //loop through each room
        foreach (GameObject room in rooms)
        {
            Debug.Log(room.name);
            if (room.name == "Area" + PlayerPrefs.GetInt("RoomNumber").ToString())
            {
                //set the room to active
                room.SetActive(true);
            }
            else
            {
                //set the room to inactive
                room.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
