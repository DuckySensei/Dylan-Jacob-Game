using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    //get the spriteMovement script
    public GameObject player;
    public GameObject Items;
    public GameObject settingsMenu;
    public Canvas mainCanvas;
    private bool isPaused = false;
    private bool inBattle = false;

    private void start()
    {
        // Set the settings menu to inactive 
        settingsMenu.SetActive(false);
        // Set the time scale to 1 for the main canvas
        Time.timeScale = 1f;
        inBattle = false;
    }

    void Update()
    {
        //get the inbattle bool from player
        SpriteMovement playerScript = player.GetComponent<SpriteMovement>();

        if (!inBattle){
            // Check if the 'escape' key was pressed
            if (Input.GetKeyDown(KeyCode.Escape) && isPaused == false)
            {
                // Set the settings menu to active 
                settingsMenu.SetActive(true);
                // Unpause the main canvas
                mainCanvas.enabled = true;
                // Set the time scale to 0 for the main canvas
                Time.timeScale = 0f;
                isPaused = true;
            }

            //if the settings menu is open and 'escape' is pressed again then unpause the game and close the menu
            else if (Input.GetKeyDown(KeyCode.Escape) && isPaused == true)
            {
                // Set the settings menu to inactive 
                settingsMenu.SetActive(false);
                // Set the time scale to 1 for the main canvas
                Time.timeScale = 1f;
                isPaused = false;
            }
        }
    }

    public void settings()
    {
        Debug.Log("Settings was clicked");
    }

    public void turnOffOn(bool onOff)
    {
        inBattle = onOff;
    }

    public void exitGame()
    {
        // if (player != null)
        // {
        //     //get the player script
        //     SpriteMovement playerScript = player.GetComponent<SpriteMovement>();
        //     if (playerScript != null)
        //     {
        //         //save the game in sprite movement
        //         playerScript.saveData();
        //     }
        // }

        // if (Items != null)
        // {
        //     //get the menu controller
        //     MenuController menuScript = Items.GetComponent<MenuController>();
        //     if (menuScript != null)
        //     {
        //         //save the game in menu controller
        //         menuScript.SaveData();
        //     }
        // }

        //set the time back to 1f
        Time.timeScale = 1f;

        UnityEngine.SceneManagement.SceneManager.LoadScene("start_screen");
    }
}