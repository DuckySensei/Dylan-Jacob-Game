using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAttributes : MonoBehaviour
{
    public string itemName;
    private SpriteRenderer itemSpriteRenderer;
    private int index;

    // Reference to the player game object's SpriteRenderer component
    public SpriteRenderer playerSpriteRenderer;

    // Reference to the Items script
    public Items items;

    // Start is called before the first frame update
    void Start()
    {
        itemSpriteRenderer = GetComponent<SpriteRenderer>();

        // Get the index number from the game object name
        string objectName = gameObject.name;
        int startIndex = objectName.IndexOf("(") + 1;
        int endIndex = objectName.IndexOf(")");
        string indexString = objectName.Substring(startIndex, endIndex - startIndex);
        index = int.Parse(indexString);
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player is in front of or behind the item
        if (transform.position.y < playerSpriteRenderer.transform.position.y)
        {
            // Set the item's SortingOrder to be higher than the player's SortingOrder
            itemSpriteRenderer.sortingOrder = playerSpriteRenderer.sortingOrder + 1;
        }
        else
        {
            // Set the item's SortingOrder to be lower than the player's SortingOrder
            itemSpriteRenderer.sortingOrder = playerSpriteRenderer.sortingOrder - 1;
        }
    }

    public GrabItems GetItem()
    {
        items.GetItem(index);

        // Destroy the item game object
        Destroy(gameObject);
        return null;
    }
}

public class GrabItems
{
    public string name;
    public string itemType;
    public float damageAmount;

    public GrabItems(string name, string itemType, float damageAmount)
    {
        this.name = name;
        this.itemType = itemType;
        this.damageAmount = damageAmount;
    }
}