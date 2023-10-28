using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.EventSystems;
using System.Linq;

public class MenuController : MonoBehaviour
{
    // Serialized fields
    public RectTransform menuTransform;
    public GameObject ButtonHolder;
    public GameObject menu;
    public GameObject EndBattleScreen;
    public GameObject player;
    public GameObject Continue;
    public GameObject categoryMenu;
    public GameObject itemsMenu;
    public Button itemPrefab;
    public Button categoryPrefab;
    public Transform categoryContent;
    public Transform content;
    public int selectedButtonIndex = 0;
    private int previouslySelectedButtonIndex = 0;
    private int selectedCategoryButtonIndex = 0;
    private int selectedItemsButtonIndex = 0;
    private Vector3 originalScale;
    private bool isBattleOver;
    private bool playerturn;

//The buttons you can press

private List<GameObject> Catbuttons = new List<GameObject>();
private List<GameObject> buttons = new List<GameObject>();
private List<GameObject> itemButtons = new List<GameObject>();

// Lists for categorized items
private List<Attacks> statusMoves = new List<Attacks>();
private List<Attacks> damagingMoves = new List<Attacks>();
private List<Attacks> collectedMoves = new List<Attacks>();
private List<Item> collectedItems = new List<Item>();


    [System.Serializable]
    private class ItemData
    {
        public List<Attacks> collectedMoves;
        public List<Item> collectedItems;
    }

    private void Start()
    {
        if(PlayerPrefs.GetInt("firstTime") == 1)
        {
            //clear collected moves and items
            collectedMoves.Clear();
            collectedItems.Clear();
            SaveData();
        }
        else
        {
            LoadData();
        }

        // //create 3 sets of items to go into the players items
        // Item potion = new Item();  
        // potion.name = "Evil Potion";
        // potion.itemType = "Damaging";
        // potion.damageAmount = 20;
        
        // Item superPotion = new Item();
        // superPotion.name = "Super Potion";
        // superPotion.itemType = "Status";
        // superPotion.damageAmount = 50;

        // Item hyperPotion = new Item();
        // hyperPotion.name = "Hyper Potion";
        // hyperPotion.itemType = "Status";
        // hyperPotion.damageAmount = 200;
        // //add the items to the list of items
        // collectedItems.Add(potion);
        // collectedItems.Add(superPotion);
        // collectedItems.Add(hyperPotion);

        //put the 3 buttons, attack, items and flee that exist already as Buttons into the list of buttons
        buttons.Add(GameObject.Find("Battle_button"));
        buttons.Add(GameObject.Find("Items_button"));
        buttons.Add(GameObject.Find("Flee_Button"));

        // Hide the menu when the game starts
        menu.SetActive(false);
        ButtonHolder.SetActive(false);
        Continue.SetActive(false);
        EndBattleScreen.SetActive(false);
        //set position of menu pos x = 0, pos y = -400, pos z = 0
        menuTransform.localPosition = new Vector3(0, -875, 0);
    }

    public void SaveData()
    {
        ItemData data = new ItemData();
        data.collectedMoves = collectedMoves;
        data.collectedItems = collectedItems;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/itemsMenu.dat");
        bf.Serialize(file, data);
        file.Close();
    }

    private void LoadData()
    {
        if (File.Exists(Application.persistentDataPath + "/itemsMenu.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/itemsMenu.dat", FileMode.Open);
            ItemData data = (ItemData)bf.Deserialize(file); // Change SaveItemData to ItemData
            // Update your properties with the deserialized data
            collectedMoves = data.collectedMoves;
            collectedItems = data.collectedItems;
            file.Close();
        }
    }

    public void pickUpItem(Item item)
    {
        //add the item to the list of items
        collectedItems.Add(item);
        //save the data
        SaveData();
    }

    public void ShowMenu()
    {
        // Show the menu when called
        menu.SetActive(true);
        ButtonHolder.SetActive(true);
        //set category menu to false
        categoryMenu.SetActive(false);
        //set items menu to false
        itemsMenu.SetActive(false);

        //hide the continue button
        Continue.SetActive(false);

        //hide the end battle screen
        EndBattleScreen.SetActive(false);

        // Set the first button as selected
        selectedButtonIndex = 0;
        buttons[selectedButtonIndex].GetComponent<Button>().Select();
        originalScale = buttons[0].transform.localScale;

        buttons[0].transform.localScale *= 1.1f;
        isBattleOver = false;
        playerturn = true;

        //slide the menu up to pos
        SlideMenuUp();
    }

    IEnumerator SlideMenu(Vector3 endPos)
    {
        //get the current position of the menu
        Vector3 startPos = menuTransform.localPosition;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.5f; // Move over 0.5 seconds
            menuTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }

    // Slide the menu up
    public void SlideMenuUp()
    {
        Vector3 endPos = new Vector3(0, 0, 0);
        StartCoroutine(SlideMenu(endPos));
    }

    // Slide the menu down
    public void SlideMenuDown()
    {
        itemsMenu.SetActive(false);
        categoryMenu.SetActive(false);
        Catbuttons.Clear();
        Vector3 endPos = new Vector3(0, -875, 0); // adjust the y value to the desired end position
        StartCoroutine(SlideMenu(endPos));
    }

    //reset the menu to its original position
    public void ResetMenu()
    {
        menuTransform.localPosition = new Vector3(0, -875, 0);
    }

    private void Update()
    {
        //make sure button has no duplicate items
        if (isBattleOver || !playerturn){
            return;
        }

        // Move up (if applicable)
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (categoryMenu.activeSelf == false)
            {
                previouslySelectedButtonIndex = selectedButtonIndex;
                selectedButtonIndex--;
                if (selectedButtonIndex < 0)
                {
                    selectedButtonIndex = buttons.Count - 1;
                }
                buttons[previouslySelectedButtonIndex].transform.localScale = originalScale; // Reset previous button size
                buttons[selectedButtonIndex].GetComponent<Button>().Select();
                buttons[selectedButtonIndex].transform.localScale *= 1.1f; // Increase size by 10%
            }
            else if (categoryMenu.activeSelf == true && itemsMenu.activeSelf == false)
            {
                previouslySelectedButtonIndex = selectedCategoryButtonIndex;
                selectedCategoryButtonIndex--;
                if (selectedCategoryButtonIndex < 0)
                {
                    selectedCategoryButtonIndex = Catbuttons.Count - 1;
                }
                Catbuttons[previouslySelectedButtonIndex].transform.localScale = originalScale;
                Catbuttons[selectedCategoryButtonIndex].GetComponent<Button>().Select();
                Catbuttons[selectedCategoryButtonIndex].transform.localScale *= 1.1f; // Increase size by 10%
            }
            else
            {   
                //print the itembutton count 

                previouslySelectedButtonIndex = selectedItemsButtonIndex;
                selectedItemsButtonIndex--;
                if (selectedItemsButtonIndex < 0)
                {
                    selectedItemsButtonIndex = itemButtons.Count - 1;
                }
                itemButtons[previouslySelectedButtonIndex].transform.localScale = originalScale; // Reset previous button size
                itemButtons[selectedItemsButtonIndex].GetComponent<Button>().Select();
                itemButtons[selectedItemsButtonIndex].transform.localScale *= 1.1f; // Increase size by 10%
            }
        }
        // Move down (if applicable)
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (categoryMenu.activeSelf == false)
            {
                previouslySelectedButtonIndex = selectedButtonIndex;
                selectedButtonIndex++;
                if (selectedButtonIndex >= buttons.Count)
                {
                    selectedButtonIndex = 0;
                }
                buttons[previouslySelectedButtonIndex].transform.localScale = originalScale; // Reset previous button size
                buttons[selectedButtonIndex].GetComponent<Button>().Select();
                buttons[selectedButtonIndex].transform.localScale *= 1.1f; // Increase size by 10%
            }
            else if (categoryMenu.activeSelf == true && itemsMenu.activeSelf == false)
            {
                previouslySelectedButtonIndex = selectedCategoryButtonIndex;
                selectedCategoryButtonIndex++;
                if (selectedCategoryButtonIndex >= Catbuttons.Count)
                {
                    selectedCategoryButtonIndex = 0;
                }
                Catbuttons[previouslySelectedButtonIndex].transform.localScale = originalScale; // Reset previous button size
                Catbuttons[selectedCategoryButtonIndex].GetComponent<Button>().Select();
                Catbuttons[selectedCategoryButtonIndex].transform.localScale *= 1.1f; // Increase size by 10%
            }
            else
            {
                previouslySelectedButtonIndex = selectedItemsButtonIndex;
                selectedItemsButtonIndex++;
                if (selectedItemsButtonIndex >= itemButtons.Count)
                {
                    selectedItemsButtonIndex = itemButtons.Count - 1;
                }
                itemButtons[previouslySelectedButtonIndex].transform.localScale = originalScale; // Reset previous button size
                itemButtons[selectedItemsButtonIndex].GetComponent<Button>().Select();
                itemButtons[selectedItemsButtonIndex].transform.localScale *= 1.1f; // Increase size by 10%
            }
        }
        //press escape
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            //if the category menu is open then close it
            if (categoryMenu.activeSelf == true && itemsMenu.activeSelf == false)
            {
                categoryMenu.SetActive(false);
                previouslySelectedButtonIndex = selectedButtonIndex;
                if (selectedCategoryButtonIndex >= 0 && selectedCategoryButtonIndex < Catbuttons.Count)
                {
                    Catbuttons[selectedCategoryButtonIndex].transform.localScale = originalScale;
                }
            }
            //if the items menu is open then close it
            else if (categoryMenu.activeSelf == true && itemsMenu.activeSelf == true)
            {
                itemsMenu.SetActive(false);
                previouslySelectedButtonIndex = selectedCategoryButtonIndex;
                if (selectedItemsButtonIndex >= 0 && selectedItemsButtonIndex < itemButtons.Count)
                {
                    itemButtons[selectedItemsButtonIndex].transform.localScale = originalScale;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            if (categoryMenu.activeSelf == false)
            {
                selectedCategoryButtonIndex = 0;
                previouslySelectedButtonIndex = 0;
                categoryMenu.SetActive(true);

                // Check if Catbuttons exist before setting their scale
                if (Catbuttons != null && Catbuttons.Count > 0 && selectedCategoryButtonIndex < Catbuttons.Count)
                {
                    Catbuttons[selectedCategoryButtonIndex].transform.localScale = originalScale;
                }

                HandleButtonPress();
            }
            else if (categoryMenu.activeSelf == true && itemsMenu.activeSelf == false && collectedItems.Count > 0)
            {
                if (Catbuttons[selectedCategoryButtonIndex].GetComponent<Button>().interactable)
                {
                    selectedItemsButtonIndex = 0;
                    previouslySelectedButtonIndex = 0;
                    itemsMenu.SetActive(true);
                    //divide the size of categoryMenu by 1.1f to get the original size
                    itemButtons[selectedItemsButtonIndex].transform.localScale = originalScale;
                }
            }                                                    
        }
    }

    public void HandleButtonPress()
    {
        //reset the size of the button you're currently on to the original size
        buttons[selectedButtonIndex].transform.localScale = originalScale;

        //check which button you're on and do the appropriate action
        switch (selectedButtonIndex)
        {
            case 0:
                //attack();
                AttacksMenuOpened();
                break;
            case 1:
                //items();
                UpdateItemMenu();
                break;
            case 2:
                //flee();
                FleeButtonClicked();
                break;
        }
    }

    public void FleeButtonClicked()
    {
        categoryMenu.SetActive(false);
        itemsMenu.SetActive(false);
    }   

    public void receiveAtkMoves(List<Attacks> moves)
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in categoryContent)
        {
            Destroy(child.gameObject);
        }

        Catbuttons.Clear();
        itemButtons.Clear();

        // Create a loop to go through each move in the list.
        foreach (Attacks move in moves)
        {
            string moveType = move.type;
            string moveName = move.name;

            // Check if there is already a move with the same name.
            if (collectedMoves.Any(existingMove => existingMove.name == moveName))
            {
                UpdateAttackMenu();
                return;
            }

            // Create a new Move object.
            Attacks newMove = new Attacks
            {
                name = moveName,
                type = moveType,
            };

            collectedMoves.Add(newMove);

            if (newMove.type == "Status")
            {
                statusMoves.Add(newMove);
            }
            else if (newMove.type == "Damaging")
            {
                damagingMoves.Add(newMove);
            }
        }

        UpdateAttackMenu();
    }

    public void AttacksMenuOpened()
    {
        // Set the first button as selected
        Catbuttons[selectedCategoryButtonIndex].GetComponent<Button>().Select();
        originalScale = Catbuttons[0].transform.localScale;
        Catbuttons[selectedCategoryButtonIndex].transform.localScale *= 1.1f;
    }

    private void UpdateItemMenu()
    {
        // Clear the list of buttons in the category menu, and the items in the items menu
        foreach (Transform child in categoryContent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        Catbuttons.Clear();

        // Temporary lists to store newly categorized items
        List<Item> newStatusItems = new List<Item>();
        List<Item> newDamagingItems = new List<Item>();

        // Sort the items in collectedItems into the category list's status and damaging based on their types
        foreach (Item item in collectedItems)
        {
            string itemType = item.itemType;
            string itemName = item.name;

            if (itemType == "Status")
            {
                newStatusItems.Add(item);
            }
            else if (itemType == "Damaging" || itemType == "Attacking")
            {
                newDamagingItems.Add(item);
            }
        }

        // Add the damaging items category to the item menu
        Button damagingButton = Instantiate(categoryPrefab, categoryContent);
        damagingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Damaging Items";
        if (newDamagingItems.Count == 0)
        {
            damagingButton.interactable = false;
            damagingButton.GetComponent<Image>().color = Color.gray;
        }
        else
        {
            damagingButton.onClick.AddListener(() => GenerateItemButtons(newDamagingItems));
        }
        Catbuttons.Add(damagingButton.gameObject);

        // Add the status items category to the item menu
        Button statusButton = Instantiate(categoryPrefab, categoryContent);
        statusButton.GetComponentInChildren<TextMeshProUGUI>().text = "Status Items";
        if (newStatusItems.Count == 0)
        {
            statusButton.interactable = false;
            statusButton.GetComponent<Image>().color = Color.gray;
        }
        else
        {
            statusButton.onClick.AddListener(() => GenerateItemButtons(newStatusItems));
        }
        Catbuttons.Add(statusButton.gameObject);

        Catbuttons[selectedCategoryButtonIndex].GetComponent<Button>().Select();
        originalScale = Catbuttons[0].transform.localScale;
        Catbuttons[selectedCategoryButtonIndex].transform.localScale *= 1.1f;
    }

    // Define the method for generating buttons from a list of items
    private IEnumerator GenerateItemButtons(List<Item> items)
    {
        itemsMenu.SetActive(true);
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        itemButtons.Clear();
        foreach(Item item in items)
        {
            Button buttonObject = Instantiate(itemPrefab, content);
            buttonObject.GetComponentInChildren<TextMeshProUGUI>().text = item.name;
            itemButtons.Add(buttonObject.gameObject);
            buttonObject.onClick.AddListener(() => {
                itemsMenu.SetActive(false);
                categoryMenu.SetActive(false);
                SlideMenuDown();
                //implement something to do with the item when you get the chance.
                player.GetComponent<SpriteMovement>().playerAttackMove(item.name, true, item.itemType, item.damageAmount, item.target);
                playerturn = false;

                //delete the item from the list of items
                collectedItems.Remove(item);
            });
        }
        yield return null;
    }

    private void UpdateAttackMenu()
    {

        foreach (Transform child in categoryContent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Add the damaging moves category to the attack menu
        Button damagingButton = Instantiate(categoryPrefab, categoryContent);
        damagingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Damaging Moves";
        damagingButton.onClick.AddListener(() => {GenerateAttackButtons(damagingMoves); itemsMenu.SetActive(true);});
        Catbuttons.Add(damagingButton.gameObject);

        // Add the status moves category to the attack menu
        Button statusButton = Instantiate(categoryPrefab, categoryContent);
        statusButton.GetComponentInChildren<TextMeshProUGUI>().text = "Status Moves";
        statusButton.onClick.AddListener(() => {GenerateAttackButtons(statusMoves); itemsMenu.SetActive(true);});
        Catbuttons.Add(statusButton.gameObject);
    }

    // Define the method for generating buttons from a list of attacks
    private void GenerateAttackButtons(List<Attacks> attacks)
    {
        // Clear any existing buttons in the ItemsMenu area

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Clear the itemButtons list before generating new buttons
        itemButtons.Clear();

        //check if list is a status list or attacking list
        if (attacks[0].type == "Status")
        {
            //sort the list by name
            attacks = attacks.OrderBy(x => x.name).ToList();
        }
        else
        {
            //sort the list by name
            attacks = attacks.OrderBy(x => x.name).ToList();
        }

        // // Generate a button for each attack in the list
        foreach (Attacks attack in attacks)
        {
            // Create a new button object
            Button buttonObject = Instantiate(itemPrefab, content);

            // Set the button text to the attack name
            buttonObject.GetComponentInChildren<TextMeshProUGUI>().text = attack.name;

            //add the button to the items list
            itemButtons.Add(buttonObject.gameObject);

            // Add an onClick listener to the button
            buttonObject.onClick.AddListener(() => {
                // Handle the button click event
                itemsMenu.SetActive(false);
                SlideMenuDown();
                player.GetComponent<SpriteMovement>().playerAttackMove(attack.name, false, "", 0f, "");
                playerturn = false;
            });
        }

        //Select the first button in the list
        itemsMenu.GetComponentInChildren<Button>().Select();
        itemButtons[0].transform.localScale *= 1.1f;
    }
    public void BattleOver(int EnemyLevel)
    {
        isBattleOver = true;
        playerturn = false;
        //turn the menu on
        menu.SetActive(true);
        categoryMenu.SetActive(false);
        itemsMenu.SetActive(false);
        SlideMenuUp();

        //get the player level
        int playerLevel = player.GetComponent<SpriteMovement>().level;
        //get the player exp
        int playerExp = player.GetComponent<SpriteMovement>().exp;
        string message = "";

        int expGained = EnemyLevel * 5;
        
        int playerExpNeeded = playerLevel * 10;

        //check if the player leveled up
        if (playerExp + expGained >= playerExpNeeded)
        {
            //level up
            player.GetComponent<SpriteMovement>().levelUp();
            message = "You have gained " + expGained + " exp!\n\nYou have leveled up!";
        }
        else
        {
            //add the exp to the player
            player.GetComponent<SpriteMovement>().exp += expGained;
            message = "You have gained " + expGained + " exp!";
        }

        //show the end battle screen
        EndBattleScreen.SetActive(true);

        //hide the menu buttons 
        ButtonHolder.SetActive(false);

        //show the continue button
        Continue.SetActive(true);

        //select the continue button
        Continue.GetComponent<Button>().Select();

        //set the text of the end battle screen
        EndBattleScreen.GetComponentInChildren<TextMeshProUGUI>().text = message;
   }

   //when enter is pressed and the cotinue button is selected
    public void ContinueButtonPressed()
    {
         //hide the end battle screen
         EndBattleScreen.SetActive(false);
    
         //show the menu buttons
         ButtonHolder.SetActive(true);
    
         //hide the continue button
         Continue.SetActive(false);
    
         //reset the menu
         ResetMenu();
    }

    //add items to the list of items
    public void AddItem(ItemCollection itemCollection)
    {
        Item item = new Item();
        item.name = itemCollection.name;
        item.itemType = itemCollection.itemType;
        item.damageAmount = itemCollection.damageAmount;
        item.target = itemCollection.target;
        
        //make sure the item doesn't already exist in the list
        if (collectedItems.Any(existingItem => existingItem.name == item.name))
        {
            return;
        }

        //add the item to the list of items
        collectedItems.Add(item);
    }
}

[System.Serializable]
public class Item
{
    public string name;
    public string itemType;
    public float damageAmount;
    public string target;
}
[System.Serializable]
public class Attacks
{
    public string name;
    public string type;
    public int dmg;
    public int accuracy;
    public int hits;
    public int heal;
}