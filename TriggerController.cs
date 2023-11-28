using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerController : MonoBehaviour
{
    // Start is called before the first frame update
    public int triggerNumber;
    void Start()
    {
        Debug.Log(gameObject.name);
        string[] parts = gameObject.name.Split('r');
        triggerNumber = int.Parse(parts[parts.Length-1]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D     other)
    {
        if (other.CompareTag("Player"))
        {
            switch (triggerNumber)
            {
                case 1:
                    Debug.Log("Trigger 1");
                    break;
                default:
                    Debug.Log("not set up");
                    break;
            }
        }
    }
}
