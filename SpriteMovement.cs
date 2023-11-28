using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SpriteMovement : MonoBehaviour
{
    //new list of things I need
    public battleController battleController;
    public List<Move> moves = null;
    private float nextAttackMultiplier = 0f;
    public Items itemObject;

    //Player Variables
    public int HealthCap = 100;
    public int currentHealth = 100;
    public int exp = 0;
    public int expGained = 0; 
    public int level = 1;
    public float speed = 5f;
    public float strength = 0f;


    //configurable variables
    bool disableMovement = false;
    public CameraMovements cameraMovements;
    public ScreenFader screenFader;

    public MenuController menuController;
    public bool inbattle;

    //get script BattleMenu
    public float detectionRange = 1f;
    private PlayerData playerData; // Reference to store the player data.

    //player movement controller
    public Rigidbody2D rb; 

    private void Start()
    {   
        rb = GetComponent<Rigidbody2D>();
        //if this is a new game...
        if (PlayerPrefs.GetInt("firstTime") == 1)
        {
            currentHealth = HealthCap;

            moves = null;

            menuController.collectedItems.Clear();
            menuController.collectedItems.Clear();

            //set the player moves
            moves = new List<Move>();
            moves.Add(new Move { name = "Punch", type = "Damaging", target = "One", dmg = 10, accuracy = 100, hits = 3});
            moves.Add(new Move { name = "Kick", type = "Damaging", target = "All", dmg = 15, accuracy = 80, hits = 1});
            moves.Add(new Move { name = "Heal", type = "Status", target = "Self", dmg = 10, accuracy = 100, hits = 1});

            //FOR TESTING
            moves.Add(new Move { name = "Kick 2", type = "Damaging", target = "All", dmg = 1000, accuracy = 100, hits = 1});
            moves.Add(new Move { name = "punch", type = "Damaging", target = "One", dmg = 1000, accuracy = 100, hits = 2});

            exp = 0;
            expGained = 0; 
            level = 1;
            speed = 5f;

            //set the player firstime to 1
            PlayerPrefs.SetInt("firstTime", 0);
            PlayerPrefs.Save();
            saveData();
            menuController.SaveData();
        }
        else
        {
            foreach (Item item in menuController.collectedItems)
            {
                //find the script Items.cs
                itemObject.removeItem(item.name);
            }
            LoadData();
            menuController.LoadData();
        }

        foreach (Item item in menuController.collectedItems)
        {
            //find the script Items.cs
            itemObject.removeItem(item.name);
        }
    }

    public void removeDeplicateMoves()
    {
        moves = moves.Distinct().ToList();
    }

    public void sendAtkMoves()
    {
        menuController.receiveAtkMoves(moves);
    }

    public bool getinBattle()
    {
        return inbattle;
    }

    private void OnEnable()
    {
        // Re-enable the script
        enabled = true;
    }

    public void saveData()
    {
        removeDeplicateMoves();
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(Application.persistentDataPath + "/playerData.dat"))
        {
            File.Delete(Application.persistentDataPath + "/playerData.dat");
        }

        FileStream file = File.Create(Application.persistentDataPath + "/playerData.dat");

        // Initialize a new PlayerData object
        PlayerData playerData = new PlayerData();

        // Assign the current player data to the new PlayerData object.
        playerData.moves = moves.ToList();

        playerData.HealthCap = HealthCap;
        playerData.currentHealth = currentHealth;
        playerData.exp = exp;
        playerData.expGained = expGained;
        playerData.level = level;
        playerData.speed = speed;
        playerData.strength = strength;
        // Player's current position
        playerData.playerX = transform.position.x;
        playerData.playerY = transform.position.y;

        // Serialize and save the new PlayerData object to a file.
        bf.Serialize(file, playerData);
        file.Close();
    }

    public void LoadData()
    {
        if (File.Exists(Application.persistentDataPath + "/playerData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerData.dat", FileMode.Open);

            // Deserialize the PlayerData object from the file.
            playerData = (PlayerData)bf.Deserialize(file);
            file.Close();
            
            removeDeplicateMoves();

            // Update the player's data with the loaded data.
            moves = playerData.moves.ToList();
            HealthCap = playerData.HealthCap;
            currentHealth = playerData.currentHealth;
            exp = playerData.exp;
            expGained = playerData.expGained;
            level = playerData.level;
            speed = playerData.speed;
            strength = playerData.strength;
            //put the player in there last known position 
            transform.position = new Vector3(playerData.playerX, playerData.playerY, 0f);
        }
    }

    private void Update()
    {  
        move();

        if (Input.GetKeyDown(KeyCode.E) || (Input.GetKeyDown(KeyCode.E) && Input.anyKeyDown))
        {
            tryInteractions();
        }
    }

    private void move()
    {
        if (!disableMovement)
        {
            // Input for movement
            float horizontal = Input.GetAxisRaw("Horizontal"); // Raw input to get -1, 0, or 1
            float vertical = Input.GetAxisRaw("Vertical");     // Raw input to get -1, 0, or 1

            // Normalize the input vector to ensure constant speed in all directions
            Vector3 movement = new Vector3(horizontal, vertical, 0.0f).normalized;

            // Move the character
            rb.velocity = movement * speed;
        }
    }


    void tryInteractions()
    {
        int layerMask = LayerMask.GetMask("Interactables");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange, layerMask);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                Debug.Log("Touched enemy");
                cameraMovements.FollowOnOff(false);
                battleController.setBattlePositions(collider.gameObject);
                disableMovement = true;
            }
            else if (collider.CompareTag("Saves"))
            {
                saveData();
                menuController.SaveData();
                Debug.Log("Saving");
            }
            else if (collider.CompareTag("Items"))
            {
                // Get the ItemAttributes script from the object
                ItemAttributes itemAttributes = collider.GetComponent<ItemAttributes>();
                if (itemAttributes != null)
                {
                    // Activate getItem script
                    itemAttributes.GetItem();
                }
            }
            else if (collider.CompareTag("Room_Trigger"))
            {
                string name = collider.gameObject.name;
                string numberString = name.Substring(name.Length - 1);
                if (int.TryParse(numberString, out int roomNumber))
                {
                    PlayerPrefs.SetInt("RoomNumber", roomNumber);
                    PlayerPrefs.Save();

                    saveData();
                    menuController.SaveData();

                    screenFader.FadeToColor(Color.white, 1f);
                    StartCoroutine(loadRoom());
                }
                else
                {
                    Debug.LogError("Invalid room number: " + numberString);
                }
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            battleController.setBattlePositions(collision.gameObject);
            disableMovement = true;
        }
    }

    public void levelUp()
    {
        //increase the level by 1
        level += 1;

        HealthCap += 10;

        //increase the max health by 10
        currentHealth += 10;

        //increase the speed by 1
        speed += 0.25f;

        //increase the exp needed by 10
        exp += level * 10;

        //get the overflow exp amount and add it to expGained
        int newEXP = exp - expGained;

        strength += 0.1f;

        //reset the exp gained
        expGained = newEXP;
    }

    IEnumerator loadRoom()
    {
        //wait 1 second
        yield return new WaitForSeconds(1f);

        //load the room
        SceneManager.LoadScene("Room");
    }

    //Take damage from the enemy
    public void TakeDamage(int damage)
    {
        //subtract the damage from the current health
        currentHealth -= damage;

        //check if the player is dead
        if (currentHealth <= 0)
        {
            //player is dead
            StartCoroutine(Die());
        }
    }

    public void setdisableMovement(bool disable)
    {
        disableMovement = disable;
    }

    public void startAttack(GameObject enemy, string atkName)
    {
        StartCoroutine(StartPlayerAtk(enemy, atkName));
    }

    IEnumerator StartPlayerAtk(GameObject enemy, string atkName)
    {
        EnemyBehaviour enemyScript = enemy.GetComponent<EnemyBehaviour>();
        int enemyHealth = enemyScript.currentHealth;

        Move move = moves.Find(m => m.name == atkName);
        Item item = menuController.collectedItems.Find(i => i.name == atkName);
        

        yield return new WaitForSeconds(0.5f);

        if (move == null)
        {
            if (item.itemType == "Status")
            {
                currentHealth += (int)item.damageAmount;
                if (currentHealth > HealthCap)
                {
                    currentHealth = HealthCap;
                }
            } 
            else if (item.itemType == "Damaging")
            {
                nextAttackMultiplier = item.damageAmount;
            }
            else // can continue this for different types of attacks
            {
                int damage = (int)item.damageAmount;
                enemyScript.TakeDamage(damage + (int)(damage * nextAttackMultiplier));
                nextAttackMultiplier = 0;
            }
        }
        else
        {
            if (move.type == "Status")
            {
                int heal = move.dmg;
                currentHealth += (int)((float)heal + ((float)heal * strength));
                if (currentHealth > HealthCap)
                {
                    currentHealth = HealthCap;
                }
            }
            else
            {
                int damage = move.dmg;
                int accuracy = move.accuracy;
                int hits = move.hits;
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                SpriteMovement playerScript = player.GetComponent<SpriteMovement>();
                Vector3 currentPos = transform.position;
                Vector3 endposition = new Vector3(-27f, 2, 0);

                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / 0.5f; // Move over 0.5 seconds
                    transform.position = Vector3.Lerp(currentPos, endposition, t);
                    yield return null;
                }

                string name = move.name;

                for (int i = 0; i < hits; i++)
                {
                    if (UnityEngine.Random.Range(0, 101) <= accuracy)
                    {
                        enemyScript.TakeDamage((int)((float)damage + ((float)damage * strength) + ((float)damage * nextAttackMultiplier)));
                    }
                    else
                    {
                        Debug.Log("Attack misses");
                    }

                    yield return new WaitForSeconds(0.25f);
                    nextAttackMultiplier = 0;
                }
                currentPos = transform.position;
                endposition = new Vector3(-29.5f, 2, 0);
                t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / 0.5f; // Move over 0.5 seconds
                    transform.position = Vector3.Lerp(currentPos, endposition, t);
                    yield return null;
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        //wait for 0.5 seconds
        yield return new WaitForSeconds(0.5f);

        battleController.PlayerTurnOver();
    }

    public void sendit()
    {
        StartCoroutine(sendEmHome());
    }

    IEnumerator sendEmHome()
    {
        //disable menu
        menuController.menu.SetActive(false);

        //set the menu back to original position
        menuController.ResetMenu();

        //screen fade to white
        screenFader.FadeToColor(Color.white, 1f);
    
        //Player slide to the left off screen
        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(-50f, 2f, 0f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 1.5f; // Move over 0.5 seconds
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        //wait for 0.5 seconds
        yield return new WaitForSeconds(0.5f);

        //load the players saved position
        float playerX = PlayerPrefs.GetFloat("PlayerX");
        float playerY = PlayerPrefs.GetFloat("PlayerY");

        //clear PlayerPrefs
        PlayerPrefs.DeleteAll();

        //send a signal to the camera to Target itself
        cameraMovements.FollowOnOff(true);

        //fade back to the game
        screenFader.FadeToColor(Color.clear, 0.5f);

        //enable movement
        disableMovement = false;

        //move the player to the saved position
        transform.position = new Vector3(playerX, playerY, 0f);

        inbattle = false;
    }

    IEnumerator Die()
    {
        //wait 2 seconds
        yield return new WaitForSeconds(2f);
        //white
        screenFader.FadeToColor(Color.white, 1f);
        //wait 1 second 
        yield return new WaitForSeconds(2f);
        //reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

[System.Serializable]
public class PlayerData
{
    public List<Move> moves;
    public int HealthCap;
    public int currentHealth;
    public int exp;
    public int expGained;
    public int level;
    public float speed;
    public float strength;
    public float playerX;
    public float playerY;

}

[System.Serializable]
public class Move
{
    public string name;
    public string type;
    public string target;
    public int dmg;
    public int accuracy;
    public int hits;
}

[System.Serializable]
public class Item
{
    public string name;
    public string itemType;
    public float damageAmount;
    public string target;
}