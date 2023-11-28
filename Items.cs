using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{
    public MenuController menuController;
    public List<ItemCollection> items = new List<ItemCollection>
    {
        //THIS IS WHERE THE ITEMS ARE STORED
        new ItemCollection("Potion", "Status", 10, ""), //1
        new ItemCollection("Damage Booster", "Damaging", 5, ""), //2
        new ItemCollection("Bomb", "Attacking", 100, "All"), //3
        new ItemCollection("Tiny Bomb", "Attacking", 100, "One") //4
    };

    // Get a specific item from the list based on its index position
    public ItemCollection GetItem(int index)
    {
        //add the item to menuController 
        menuController.AddItem(items[index]);
        return null;
    }

    public void removeItem(string name)
    {
        //get the ItemCollection name from the list that matches the name
        ItemCollection item = items.Find(x => x.name == name);

        if (item != null)
        {
            //get the item index
            int index = items.IndexOf(item);

            string itemBoxName = "ItemBox(" + index + ")";

            GameObject itemBox = GameObject.Find(itemBoxName);

            Destroy(itemBox);
        }
    }
}

public class ItemCollection
{
    public string name;
    public string itemType;
    public float damageAmount;
    public string target;

    public ItemCollection(string name, string itemType, float damageAmount, string target)
    {
        this.name = name;
        this.itemType = itemType;
        this.damageAmount = damageAmount;
        this.target = target;
    }
}