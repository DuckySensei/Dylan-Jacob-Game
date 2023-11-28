using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MainMenu : MonoBehaviour
{
    string[] sentences = new string[]
    {
        "Welcome to my game!",
        "My name is dAIrrel and I am your guide.",
        "As you probably know I am a test AI.",
        "My goal is to provide the BEST experience possible",
        "I will interfere with you very little.",
        "But I will do occassional check-ins to make sure you are having fun!",
        "First things first, what is your name?",
        "Good to know, what is you favorite type of game?",
        "Do you like your game being hard or easy?",
        "Would you please play the game for a long time.",
        "Okay, well thank you for all the information!",
        "Let me just read back some of the information you gave me.",
        "Your name is Booger",
        "Your favorite type of game is RPG",
        "You like your games to be hard",
        "You will play my game for a long time.",
        "Does this information look correct?",
        "That's GREAT! Well let me throw something together real quick",
        "...",
        "Okay, I think I have something for you",
        "Have fun!"
    };

    public TMPro.TextMeshProUGUI dialogueText;
    public TMPro.TMP_InputField inputField;
    public Image fadeImage;
    public GameObject AI;
    public GameObject Background;
    public ScreenFader screenFader;
    public GameObject startButton;
    public GameObject continueButton;
    public GameObject settingsButton;
    public GameObject exit;
    public bool no;
    // Start is called before the first frame update
    bool clicked = false;

    public GameObject[] menuItems;
    private int selectedItemIndex = 0;

    private void Start()
    {
        startButton.SetActive(true);
        continueButton.SetActive(true);
        settingsButton.SetActive(true);
        exit.SetActive(true);
        //set the fade image to clear
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.gameObject.SetActive(false);
        //the text box starting position is (where it is currently)
        inputField.gameObject.SetActive(false);
        //the ai background
        Background.SetActive(false);
        //the ai
        AI.SetActive(false);
    }

    private void Update()
    {
        if (File.Exists(Application.persistentDataPath + "/playerData.dat") && File.Exists(Application.persistentDataPath + "/itemMenu.dat"))
        {
            // File exists, do something here
            continueButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
            //set to default color
            continueButton.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1);
            no = false;
        }
        else
        {
            // File does not exist, handle accordingly
            continueButton.GetComponent<UnityEngine.UI.Image>().color = Color.gray;
            continueButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
            no = true;

        }

        // Move up in the menu when the player presses 'W'
        if (Input.GetKeyDown(KeyCode.W))
        {
            selectedItemIndex--;
            if (selectedItemIndex < 0)
            {
                selectedItemIndex = menuItems.Length - 1;
            }
            UpdateSelectedMenuItem();
        }

        // Move down in the menu when the player presses 'S'
        if (Input.GetKeyDown(KeyCode.S))
        {
            selectedItemIndex++;
            if (selectedItemIndex >= menuItems.Length)
            {
                selectedItemIndex = 0;
            }
            UpdateSelectedMenuItem();
        }

        // Select the menu item when the player presses 'enter'
        if (Input.GetKeyDown(KeyCode.Return) && clicked == false)
        {
            clicked = true;
            switch (selectedItemIndex)
            {
                case 0:
                    startNewGame();
                    break;
                case 1:
                    continueGame();
                    break;
                case 2:
                    settings();
                    break;
                case 3:
                    exitGame();
                    break;
            }
        }
    }

    private void UpdateSelectedMenuItem()
    {
        // Set the selected item to active and the others to inactive
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (i == selectedItemIndex)
            {
                //make the menuItem 10% bigger
                menuItems[i].transform.localScale = new Vector3(3.425f, 2.725f, 1.0f);
            }
            else
            {;
                //set it back to original size
                menuItems[i].transform.localScale = new Vector3(3.025f, 2.325f, 1.0f);
            }
        }
    }
    


    public void startNewGame()
    {
        fadeImage.gameObject.SetActive(true);
        PlayerPrefs.Save();
        //delete the save file
        File.Delete(Application.persistentDataPath + "/playerData.dat");
        File.Delete(Application.persistentDataPath + "/itemMenu.dat");
        //Send to starting cutsceen
        StartCoroutine(StartingCutSceen());
    }

    public void continueGame()
    {
        if (no == true){
            clicked = false;
            return;
        }
        //load the scene
        PlayerPrefs.SetInt("isNewGame", 0);
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Test_grassy");
    }

    public void settings()
    {
        Debug.Log("Settings was clicked");

        //youll NEED to change this!
        clicked = false;
    }

    public void exitGame()
    {
        Application.Quit();
    }

    private IEnumerator StartingCutSceen()
    {
        //fade to black
        screenFader.FadeToColor(Color.black, 3f);
        //wait for 1 second
        yield return new WaitForSeconds(5f);
        //Hide all menu buttons and text
        startButton.SetActive(false);
        continueButton.SetActive(false);
        settingsButton.SetActive(false);
        exit.SetActive(false);
        //find the object game name
        GameObject gameName = GameObject.Find("GameName");
        gameName.SetActive(false);
        //turn on the background 
        Background.SetActive(true);
        //turn on the AI
        AI.SetActive(true);
        //turn on the ai_text
        dialogueText.gameObject.SetActive(true);

        screenFader.FadeToColor(Color.clear, 3f);
        yield return new WaitForSeconds(3f);
        //deactivate the fade image
        fadeImage.gameObject.SetActive(false);

        int index = 0;
        //I want the text to be displayed one at a time, and to move through the text the player has to press 'enter'
        while (index < sentences.Length)
        {
            bool isTyping = true;
            dialogueText.text = "";
            string sentence = sentences[index];

            if (sentence == "First things first, what is your name?" || sentence == "Good to know, what is you favorite type of game?" || sentence == "Do you like your game being hard or easy?" || sentence == "Would you please play the game for a long time." || sentence == "Does this information look correct?")
            {
                dialogueText.rectTransform.localPosition = new Vector3(0, 363, 0);
            }
            else
            {
                dialogueText.rectTransform.localPosition = new Vector3(0, -363, 0);
            }

            for (int i = 0; i < sentence.Length; i++)
            {
                dialogueText.text += sentence[i];
                yield return new WaitForSeconds(0.05f);
            }

            if (sentence == "First things first, what is your name?" || sentence == "Good to know, what is you favorite type of game?" || sentence == "Do you like your game being hard or easy?" || sentence == "Would you please play the game for a long time." || sentence == "Does this information look correct?")
            {
                // Set the input field game object active
                inputField.gameObject.SetActive(true);
                // Set the focus to the input field
                inputField.Select();
                inputField.ActivateInputField();
                // Wait for the player to enter their name
                yield return new WaitUntil(() => inputField.text != "");
                // Get the player's name from the input field
            }

            if (sentence == "Have fun!")
            {
                //turn on screen fader
                fadeImage.gameObject.SetActive(true);
                //fade to white
                screenFader.FadeToColor(Color.white, 3f);
                yield return new WaitForSeconds(3f);
                //load the scene
                PlayerPrefs.SetInt("isNewGame", 1);
                UnityEngine.SceneManagement.SceneManager.LoadScene("Test_grassy");
            }

            isTyping = false;

            // Wait for the player to press 'enter'
            while (!Input.GetKeyDown(KeyCode.Return) && isTyping == false)
            {
                yield return null;
            }

            //clear the input 
            inputField.text = "";
            inputField.gameObject.SetActive(false);

            index++;
        }
    }
}
