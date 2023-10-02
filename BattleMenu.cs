using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenu : MonoBehaviour
{   
    public RectTransform menuTransform;
    public float slideSpeed = 100f;
    public GameObject menu;
    public GameObject menuButton;

    private void Start()
    {
        // Hide the menu when the game starts
        menu.SetActive(false);
        //set position of menu pos x = 0, pos y = -400, pos z = 0
        menuTransform.localPosition = new Vector3(0, -875, 0);
    }

    public void ShowMenu()
    {
        Debug.Log("ShowMenu() called");
        // Show the menu when called
        menu.SetActive(true);
        menuButton.SetActive(true);

        //slide the menu up to pos
        StartCoroutine(SlideMenuUp());
    }

    IEnumerator SlideMenuUp(){
        //get the current position of the menu
        Vector3 startPos = menuTransform.localPosition;
        Vector3 endPos = new Vector3(0, 0, 0);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.5f; // Move over 0.5 seconds
            menuTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }

    public void FleeButtonClicked()
    {
        Debug.Log("Menu button clicked");
        
        //pick a random number between 1 and 100
        int randomNum = Random.Range(1, 101);
        
    }
}