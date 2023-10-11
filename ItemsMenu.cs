using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemsMenu : MonoBehaviour
{
    public GameObject player;
    public Button itemPrefab;
    public Button categoryPrefab;
    public Transform categoryContent;
    public GameObject categoryMenu;
    public GameObject itemsMenu;
    public Transform content;
    private bool itemsShow;
    private List<Move> collectedMoves = new List<Move>();
    private List<Move> statusMoves = new List<Move>();
    private List<Move> damagingMoves = new List<Move>();
    private List<Item> collectedItems = new List<Item>();
    private List<Item> statusItems = new List<Item>();
    private List<Item> damagingItems = new List<Item>();

    // Call this method to pick up an item and add it to the storage system

    //create test of adding items to all the categories
    private void Start()
    {
        //hide both category menu and items menu
        categoryMenu.SetActive(false);
        itemsMenu.SetActive(false);

        //Delete this later
        Item item1 = new Item();
        item1.name = "Item 1";
        item1.itemType = "Status";
        item1.damageAmount = 0;
        PickUpItem(item1);

        Item item2 = new Item();
        item2.name = "Item 2";
        item2.itemType = "Status";
        item2.damageAmount = 0;
        PickUpItem(item2);

        //create more tests and simulate it picking it up in the game
        Item item3 = new Item();
        item3.name = "Item 3";
        item3.itemType = "Damaging";
        item3.damageAmount = 10;
        PickUpItem(item3);
    }

    public void PickUpItem(Item item)
    {
        collectedItems.Add(item);

        if (item.itemType == "Status")
        {
            statusItems.Add(item);
        }
        else if (item.itemType == "Damaging")
        {
            damagingItems.Add(item);
        }

        UpdateItemsMenu();
        UpdateCategoriesMenu();
    }

    // Call this method to remove an item from the storage system
    public void RemoveItem(Item item)
    {
        collectedItems.Remove(item);

        if (item.itemType == "Status")
        {
            statusItems.Remove(item);
        }
        else if (item.itemType == "Damaging")
        {
            damagingItems.Remove(item);
        }

        UpdateItemsMenu();
        UpdateCategoriesMenu();
    }

    // Call this method to check if an item is in the storage system
    public bool HasItem(Item item)
    {
        return collectedItems.Contains(item);
    }

    // Call this method to update the items menu
    private void UpdateItemsMenu()
    {
        // Clear the content of the scroll view
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Sort the collected items in alphabetical order
        collectedItems.Sort((x, y) => x.name.CompareTo(y.name));

        // Add each collected item to the scroll view
        foreach (Item item in collectedItems)
        {
            Button itemButton = Instantiate(itemPrefab, content);
            itemButton.GetComponentInChildren<TextMeshProUGUI>().text = item.name;
        }
    }

    private void UpdateAtkMenu()
    {
        // Clear the content of the scroll view
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Sort the collected items in alphabetical order
        collectedItems.Sort((x, y) => x.name.CompareTo(y.name));

        //add each item in list 
        foreach (Move move in collectedMoves)
        {
            Button moveButton = Instantiate(itemPrefab, content);
            moveButton.GetComponentInChildren<TextMeshProUGUI>().text = move.name;
        }
    }
    
      // Call this method to update the categories menu
    private void UpdateCategoriesMenu()
    {
    
        // Clear the content of the category menu
        foreach (Transform child in categoryContent)
        {
            Destroy(child.gameObject);
        }

        // Add the damaging items category to the category menu
        Button damagingButton = Instantiate(categoryPrefab, categoryContent);
        damagingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Damaging Items (" + damagingItems.Count + ")";
        damagingButton.onClick.AddListener(() => ShowItems(damagingItems));
        // Clear the content of the category menu

        // Add the status items category to the category menu
        Button statusButton = Instantiate(categoryPrefab, categoryContent);
        statusButton.GetComponentInChildren<TextMeshProUGUI>().text = "Status Items (" + statusItems.Count + ")";
        statusButton.onClick.AddListener(() => ShowItems(statusItems));
    }

    private void UpdateAttackMenu()
    {
        Debug.Log("Updating attack menu");
        // Clear the content of the scroll view
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Add the damaging moves category to the attack menu
        Button damagingButton = Instantiate(categoryPrefab, categoryContent);
        damagingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Damaging Moves";
        damagingButton.onClick.AddListener(() => ShowAtk(damagingMoves));

        // Add the status moves category to the attack menu
        Button statusButton = Instantiate(categoryPrefab, categoryContent);
        statusButton.GetComponentInChildren<TextMeshProUGUI>().text = "Status Moves";
        statusButton.onClick.AddListener(() => ShowAtk(statusMoves));
    }

    // Call this method to show the items in a category
    private void ShowItems(List<Item> items)
    {
        itemsMenu.SetActive(true);
        // Clear the content of the scroll view
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Sort the items in alphabetical order
        items.Sort((x, y) => x.name.CompareTo(y.name));

        // Add each item to the scroll view
        foreach (Item item in items)
        {
            Button itemButton = Instantiate(itemPrefab, content);
            itemButton.GetComponentInChildren<TextMeshProUGUI>().text = item.name;
        }


        //In the future add listeners!
    }

    private void ShowAtk(List<Move> moves)
    {
        itemsMenu.SetActive(true);
        // Clear the content of the scroll view
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Sort the moves in alphabetical order
        moves.Sort((x, y) => x.name.CompareTo(y.name));

        // Add each move to the scroll view
        foreach (Move move in moves)
        {
            Button moveButton = Instantiate(itemPrefab, content);
            moveButton.GetComponentInChildren<TextMeshProUGUI>().text = move.name;
            //create a button that will be use to select the move and send it to SpriteMovement script
            moveButton.onClick.AddListener(() => player.GetComponent<SpriteMovement>().playerAttackMove(move.name));
        }
    }

    public void ItemButtonClicked()
    {
        //show the category menu
        categoryMenu.SetActive(true);
        itemsMenu.SetActive(false);
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in categoryContent)
        {
            Destroy(child.gameObject);
        }

        UpdateItemsMenu();
        UpdateCategoriesMenu();
    }

    //if the flee button is clicked, hide everything
    public void FleeButtonClicked()
    {
        categoryMenu.SetActive(false);
        itemsMenu.SetActive(false);
    }   

    public void atkMenuClicked()
    {
        //show the category menu
        itemsMenu.SetActive(false);
        categoryMenu.SetActive(true);
    }

    public void recieveAtkMove(object[] moves)
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in categoryContent)
        {
            Destroy(child.gameObject);
        }

        //create loop to go through each item in moves
        foreach (object moveObj in moves)
        {  
            string movetype;
            string moveName;

            //get the name of the move
            moveName = (string)moveObj.GetType().GetProperty("name").GetValue(moveObj);
            Debug.Log("Received name: " + moveName);

            //check if there is already a move with the same name
            foreach (Move move in collectedMoves)
            {
                if (move.name == moveName)
                {
                    Debug.Log("Already have this move");
                    UpdateAtkMenu();
                    UpdateAttackMenu();
                    return;
                }
            }

            //print the type of the move
            movetype = (string)moveObj.GetType().GetProperty("type").GetValue(moveObj);
            Debug.Log("Received type: " + movetype);

            //create a new move object
            Move newMove = new Move();
            newMove.name = moveName;
            newMove.moveType = movetype;

            collectedMoves.Add(newMove);

            if (newMove.moveType == "Status")
            {
                statusMoves.Add(newMove);
            }
            else if (newMove.moveType == "Damanging")
            {
                damagingMoves.Add(newMove);
            }
        }

        UpdateAtkMenu();
        UpdateAttackMenu();
    }
}

public class Item
{
    public string name;
    public string itemType;
    public float damageAmount;
}

public class Move
{
    public string name;
    public string moveType;
}