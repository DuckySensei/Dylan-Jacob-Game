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
    public List<Move> moves;
    public GameObject[] enemiesToFollow;
    public bool itemActive;
    public float nextAttackMultiplier;
    private int selectedEnemyIndex = 0;
    public Color selectedColor;
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();
    private bool selectingEnemy = false;
    private float playeratkdmg = 0f;
    private string playermovename = "";
    private bool playeritemMove = false;
    private string playeritemType = "";
    private string playeritemtarget = "";

    private int enemyLevelStacker = 0;

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
    private GameObject currentEnemy;
    public ScreenFader screenFader;

    public MenuController menuController;
    public bool inbattle;

    //get script BattleMenu

    private PlayerPrefs playerPrefs;
    public float detectionRange = 1f;
    private string enemyTag = "Enemy";
    private string saves = "Saves";
    private PlayerData playerData; // Reference to store the player data.

    private void Start()
    {
        playerData = new PlayerData(); // Initialize the player data.

        //if this is a new game...
        if (PlayerPrefs.GetInt("firstTime") == 0)
        {
            currentHealth = HealthCap;
            exp = 0;
            expGained = 0; 
            level = 1;
            speed = 5f;
            List<Move> moves = new List<Move>
            {
                new Move { name = "Nibble", type = "Damaging", dmg = 100, accuracy = 100, hits = 3 },
                new Move { name = "Pee On", type = "Damaging", dmg = 50, accuracy = 40, hits = 1 },
                new Move { name = "Masturbate", type = "Status", heal = 10, accuracy = 100 }
            };
            //set the player firstime to 1
            PlayerPrefs.SetInt("firstTime", 1);
            PlayerPrefs.Save();
            saveData();
        }
        else
        {
            LoadData();
        }
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
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerData.dat");

        //print the save file location
        //Debug.Log(Application.persistentDataPath);

        // Assign the current player data to the PlayerData object.
        playerData.moves = moves.ToList();
        playerData.HealthCap = HealthCap;
        playerData.currentHealth = currentHealth;
        playerData.exp = exp;
        playerData.expGained = expGained;
        playerData.level = level;
        playerData.speed = speed;
        playerData.strength = strength;
        //players current position
        playerData.playerX = transform.position.x;
        playerData.playerY = transform.position.y;

        // Serialize and save the PlayerData object to a file.
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

    //create function to get the enemiesToFollow gameObject array
    public GameObject[] getEnemiesToFollow()
    {
        return enemiesToFollow;
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        //make it so you can only move cardinal directions
        if (Mathf.Abs(horizontalInput) > Mathf.Abs(verticalInput))
        {
            verticalInput = 0;
        }
        else
        {
            horizontalInput = 0;
        }

        // get all game objects with the enemy tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        GameObject[] enemiesGetter = enemies
            .Where(enemy => enemy.GetComponent<EnemyBehaviour>().follow == true)
            .OrderBy(enemy => int.Parse(enemy.name.Split('(', ')')[1]))
            .ToArray();

        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f).normalized * speed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;

        // Check for collisions before moving
        Collider2D collider = Physics2D.OverlapBox(newPosition, transform.localScale, 0f);

        //check if the collider is not null and im not colliding with myself

        if (collider != null && disableMovement == false)
        {
            //check if the gameobject is an enemy
            if (collider.gameObject.CompareTag(enemyTag))
            {
                //startbattle
                currentEnemy = collider.gameObject;
                bool enemyFollow = false;

                //Save the player's current position
                PlayerPrefs.SetFloat("PlayerX", transform.position.x);
                PlayerPrefs.SetFloat("PlayerY", transform.position.y);

                //print the total number of enemies

                if (enemiesGetter.Length <= 1){
                    //save the position of the enemy
                    PlayerPrefs.SetFloat("EnemyX", collider.gameObject.transform.position.x);
                    PlayerPrefs.SetFloat("EnemyY", collider.gameObject.transform.position.y);

                    if (enemiesGetter.Length == 1)
                    {
                        enemyFollow = true;
                    }
                    else 
                    {
                        collider.gameObject.GetComponent<EnemyBehaviour>().follow = false;
                        enemyFollow = false;
                    }

                    PlayerPrefs.SetInt("totalEnemies", 1);
                    PlayerPrefs.Save();
                }
                else if (enemiesGetter.Length == 2)
                {
                    //save the position of the enemy
                    PlayerPrefs.SetFloat("EnemyX", enemiesGetter[0].transform.position.x);
                    PlayerPrefs.SetFloat("EnemyY", enemiesGetter[0].transform.position.y);

                    PlayerPrefs.SetFloat("EnemyX2", enemiesGetter[1].transform.position.x);
                    PlayerPrefs.SetFloat("EnemyY2", enemiesGetter[1].transform.position.y);

                    enemyFollow = true;
                    PlayerPrefs.SetInt("totalEnemies", 2);
                    PlayerPrefs.Save();
                }
                else
                {
                    //save the position of the enemy
                    PlayerPrefs.SetFloat("EnemyX", enemiesGetter[0].transform.position.x);
                    PlayerPrefs.SetFloat("EnemyY", enemiesGetter[0].transform.position.y);

                    PlayerPrefs.SetFloat("EnemyX2", enemiesGetter[1].transform.position.x);
                    PlayerPrefs.SetFloat("EnemyY2", enemiesGetter[1].transform.position.y);

                    PlayerPrefs.SetFloat("EnemyX3", enemiesGetter[2].transform.position.x);
                    PlayerPrefs.SetFloat("EnemyY3", enemiesGetter[2].transform.position.y);

                    enemyFollow = true;
                    PlayerPrefs.SetInt("totalEnemies", 3);
                    PlayerPrefs.Save();
                }
                //check to see if the enemy follow was true

                if (enemyFollow && !disableMovement)
                {
                    disableMovement = true;
                    enemiesToFollow = enemiesGetter;
                    foreach (GameObject enemyToFollow in enemiesToFollow)
                    {
                        //activate function enemy disableMovement in enemy
                        enemyToFollow.GetComponent<EnemyBehaviour>().disableMovement();
                    }
                    StartCoroutine(HandleEnemyHit(collider.gameObject, true));
                }
                else if (!disableMovement)
                {
                    disableMovement = true;
                    StartCoroutine(HandleEnemyHit(collider.gameObject, false));
                }
                //HERE FIX HERE
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                //check if 'e' was pressed 
                if (collider.gameObject.CompareTag(saves))
                {
                    //save the game
                    saveData();
                    return;
                }
                else if (collider.gameObject.CompareTag("Items"))
                {
                    //get the itemattribute script from the object i just hit
                    ItemAttributes itemAttributes = collider.gameObject.GetComponent<ItemAttributes>();
                    //activate get item script
                    itemAttributes.GetItem();
                    return;
                }
                else if (collider.gameObject.CompareTag("Room_Trigger"))
                {
                    string name = collider.gameObject.name;
                    string numberString = name.Substring(name.Length - 1);
                    int roomNumber;
                    if (int.TryParse(numberString, out roomNumber))
                    {
                        PlayerPrefs.SetInt("RoomNumber", roomNumber);
                        PlayerPrefs.Save();

                        saveData();
                        //save the items data 
                        menuController.SaveData();

                        //fade to white 
                        screenFader.FadeToColor(Color.white, 1f);
                        StartCoroutine(loadRoom());
                    }
                    else
                    {
                        Debug.LogError("Invalid room number: " + numberString);
                    }
                }
            }
            else
            {
                // If the collider is not a trigger, stop the movement
                if (!collider.isTrigger)
                {
                    //print what the player is colliding with 
                    return;
                }
            }
        }

        //check if enemy is in detectionRange
        Collider2D[] collidersInRange = Physics2D.OverlapCircleAll(transform.position, detectionRange);

        //check if enemy is in range, and has the tag "Enemy" then continue through
        foreach (Collider2D colliderInRange in collidersInRange)
        {
            if (colliderInRange.gameObject.CompareTag(enemyTag) & disableMovement == false && colliderInRange.gameObject.GetComponent<EnemyBehaviour>().follow == false)
            {
                //check to see if you hit the enemy and enemy isn't following
                if (Input.GetKeyDown(KeyCode.E) || (Input.GetKeyDown(KeyCode.E) && Input.anyKeyDown))
                {   
                    PlayerPrefs.SetInt("totalEnemies", 1);
                    currentEnemy = colliderInRange.gameObject;
                    //disable movement
                    disableMovement = true;
                    StartCoroutine(HandleEnemyHit(colliderInRange.gameObject, false));

                    //Save the player's current position
                    PlayerPrefs.SetFloat("PlayerX", transform.position.x);
                    PlayerPrefs.SetFloat("PlayerY", transform.position.y);

                    //save the position of the enemy
                    PlayerPrefs.SetFloat("EnemyX", colliderInRange.gameObject.transform.position.x);
                    PlayerPrefs.SetFloat("EnemyY", colliderInRange.gameObject.transform.position.y);
                }
            }
        }
        //if player collides with the enemy, start the battle
        // Move the sprite if there are no collisions and movement is not disabled
        if (!disableMovement)
        {
            transform.position = newPosition;
        }

        if (selectingEnemy == true)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                // Move the selection up
                GameObject previousEnemy = enemiesToFollow[selectedEnemyIndex];
                previousEnemy.GetComponent<Renderer>().material.color = originalColors[previousEnemy];
                selectedEnemyIndex--;
                if (selectedEnemyIndex < 0)
                {
                    selectedEnemyIndex = enemiesToFollow.Length - 1;
                }

                // Update the selected enemy
                GameObject selectedEnemy = enemiesToFollow[selectedEnemyIndex];
                selectedEnemy.GetComponent<Renderer>().material.color = selectedColor;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                // Move the selection down
                GameObject previousEnemy = enemiesToFollow[selectedEnemyIndex];
                previousEnemy.GetComponent<Renderer>().material.color = originalColors[previousEnemy];
                selectedEnemyIndex++;
                if (selectedEnemyIndex >= enemiesToFollow.Length)
                {
                    selectedEnemyIndex = 0;
                }

                // Update the selected enemy
                GameObject selectedEnemy = enemiesToFollow[selectedEnemyIndex];
                selectedEnemy.GetComponent<Renderer>().material.color = selectedColor;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                // Attack the selected enemy
                GameObject selectedEnemy = enemiesToFollow[selectedEnemyIndex];
                selectedEnemy.GetComponent<Renderer>().material.color = selectedColor;
                currentEnemy = selectedEnemy;
                selectedEnemyIndex = 0;
                if (playeritemMove == true)
                {
                    itemActive = true;
                }
                else
                {
                    itemActive = false;
                }

                StartCoroutine(StartPlayerAtk(playermovename, playeritemMove, playeritemType, playeratkdmg, playeritemtarget));

                selectingEnemy = false;
            }
            
        }
    }

    //disable 'e' for a certain amount of time

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

    IEnumerator HandleEnemyHit(GameObject enemy, bool gothit)
    {
        inbattle = true;

        //remove every enemiesToFollow from the list other then the first 3
        if (enemiesToFollow.Length > 3)
        {
            for (int i = 3; i < enemiesToFollow.Length; i++)
            {
                //remove the enemy from the list
                Array.Resize(ref enemiesToFollow, enemiesToFollow.Length - 1);
            }
        }

        //disable menu
        screenFader.FadeToColor(Color.white, 0.5f);
        //wait for 0.5 seconds
        yield return new WaitForSeconds(0.5f);

        //stop persuing
        EnemyBehaviour enemyBehavior = enemy.GetComponent<EnemyBehaviour>();
        enemyBehavior.stopPerusing();
                                    
        //move the character sprite to x=-31.5 and y = 2
        transform.position = new Vector3(-29.5f, 2f, 0f);

        Vector3 startPosition = new Vector3(-18f, 2f, 0f);
        Vector3 endPosition = new Vector3(-24.5f, 2f, 0f);
        Vector3 startPosition2 = new Vector3(-18f, 0f, 0f);
        Vector3 endPosition2 = new Vector3(-23.5f, 0f, 0f);
        Vector3 startPosition3 = new Vector3(-18f, 4f, 0f);
        Vector3 endPosition3 = new Vector3(-23.5f, 4f, 0f);
        if (enemiesToFollow.Length == 0)
        {
            enemy.gameObject.transform.position = startPosition;
        }
        else if (PlayerPrefs.GetInt("totalEnemies") == 1 && enemiesToFollow.Length == 1)
        {
            enemiesToFollow[0].gameObject.transform.position = startPosition;
        }
        else if(PlayerPrefs.GetInt("totalEnemies") == 2)
        {
            enemiesToFollow[0].gameObject.transform.position = startPosition;
            enemiesToFollow[1].gameObject.transform.position = startPosition;
        }
        else if (PlayerPrefs.GetInt("totalEnemies") == 3)
        {
            enemiesToFollow[0].gameObject.transform.position = startPosition;
            enemiesToFollow[1].gameObject.transform.position = startPosition2;
            enemiesToFollow[2].gameObject.transform.position = startPosition3;
        }
        //send a signal to the camera to Target itself
        cameraMovements.FollowOnOff(false);
        Camera.main.transform.position = new Vector3(-27f, 0f, -30f);

        //fade back to the game
        screenFader.FadeToColor(Color.clear, 0.5f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.5f; // Move over 0.5 seconds
            if (PlayerPrefs.GetInt("totalEnemies") == 1 && enemiesToFollow.Length == 0)
            {
                enemy.gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            }
            else if (PlayerPrefs.GetInt("totalEnemies") == 1 && enemiesToFollow.Length == 1)
            {
                enemiesToFollow[0].gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            }
            else if (PlayerPrefs.GetInt("totalEnemies") == 2)
            {
                enemiesToFollow[0].gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, t);
                enemiesToFollow[1].gameObject.transform.position = Vector3.Lerp(startPosition2, endPosition2, t);
            }
            else if (PlayerPrefs.GetInt("totalEnemies") == 3)
            {
                enemiesToFollow[0].gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, t);
                enemiesToFollow[1].gameObject.transform.position = Vector3.Lerp(startPosition2, endPosition2, t);
                enemiesToFollow[2].gameObject.transform.position = Vector3.Lerp(startPosition3, endPosition3, t);
            }
            yield return null;
        }

        if (!gothit){
            // Show the menu when the enemy is hit
            menuController.ShowMenu();
        }
        else
        {
            yield return new WaitForSeconds(1f);
            EnemyTurn();
        }
    }

    public int getEnemyLevel()
    {
        //get the level of the enemy im in battle with
        EnemyBehaviour enemyBehavior = currentEnemy.GetComponent<EnemyBehaviour>();
        int enemyLevel = enemyBehavior.level;
        return enemyLevel;
    }

    //flee Successfully!
    public void flee()
    {
        //get the level of the enemy im in battle with
        if (enemiesToFollow.Length == 0)
        {
            currentEnemy = getCurrentEnemy();
        }
        else
        {
            currentEnemy = enemiesToFollow[0];
        }
        EnemyBehaviour enemyBehavior = currentEnemy.GetComponent<EnemyBehaviour>();
        int enemyLevel = enemyBehavior.level;

        //two numbers that will be compared
        int playerRoll = UnityEngine.Random.Range(enemyLevel, 101);
        int enemyRoll = UnityEngine.Random.Range(enemyLevel, 101);

        //role the DICE
        if (playerRoll == enemyRoll)
        {
            //Flee Unsuccessful
            StartCoroutine(fleeUnSuccess());
        }
        else
        {
            //Flee Successfully!
            StartCoroutine(fleeSuccess());
        }
    }

    IEnumerator fleeUnSuccess()
    {
        //disable menu
        menuController.SlideMenuDown();

        //Player slide to the left off screen
        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(-33.5f, 2f, 0f);
        float distance = Vector3.Distance(startPosition, endPosition);
        float speed = distance / 0.5f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed / distance;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }
        transform.position = endPosition;

        while (transform.position.x < endPosition.x)
        {
            transform.position += new Vector3(5f * Time.deltaTime, 0f, 0f);
            yield return null;
        }

        //*Trip* animation, for now just have the player sprite rotate 90 degrees counterclockwise
        transform.Rotate(0, 0, 90);

        //wait for 0.5 seconds
        yield return new WaitForSeconds(1f);

        //stand back up
        transform.Rotate(0, 0, -90);

        //wait for 0.5 seconds
        yield return new WaitForSeconds(0.5f);

        //get current position
        startPosition = transform.position;
        endPosition = new Vector3(-29.5f, 2f, 0f);

        //move the player back to the start position
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed / distance;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }
        transform.position = endPosition;

        //wait 0.5 seconds
        yield return new WaitForSeconds(0.5f);

        //enemy turn
        EnemyTurn();
    }

    IEnumerator fleeSuccess()
    {
        inbattle = false;
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
        float enemyX = 0f;
        float enemyY = 0f;
        float enemyX2 = 0f;
        float enemyY2 = 0f;  
        float enemyX3 = 0f;
        float enemyY3 = 0f;

        if (PlayerPrefs.GetInt("totalEnemies") == 1){
            //save the position of the enemy
            enemyX = PlayerPrefs.GetFloat("EnemyX", currentEnemy.transform.position.x);
            enemyY = PlayerPrefs.GetFloat("EnemyY", currentEnemy.transform.position.y);
        }
        else if (PlayerPrefs.GetInt("totalEnemies") == 2)
        {
            //save the position of the enemy
            enemyX = PlayerPrefs.GetFloat("EnemyX", enemiesToFollow[0].transform.position.x);
            enemyY = PlayerPrefs.GetFloat("EnemyY", enemiesToFollow[0].transform.position.y);

            enemyX2 = PlayerPrefs.GetFloat("EnemyX2", enemiesToFollow[1].transform.position.x);
            enemyY2 = PlayerPrefs.GetFloat("EnemyY2", enemiesToFollow[1].transform.position.y);
        }
        else
        {
            //save the position of the enemy
            enemyX = PlayerPrefs.GetFloat("EnemyX", enemiesToFollow[0].transform.position.x);
            enemyY = PlayerPrefs.GetFloat("EnemyY", enemiesToFollow[0].transform.position.y);

            enemyX2 = PlayerPrefs.GetFloat("EnemyX2", enemiesToFollow[1].transform.position.x);
            enemyY2 = PlayerPrefs.GetFloat("EnemyY2", enemiesToFollow[1].transform.position.y);

            enemyX3 = PlayerPrefs.GetFloat("EnemyX3", enemiesToFollow[2].transform.position.x);
            enemyY3 = PlayerPrefs.GetFloat("EnemyY3", enemiesToFollow[2].transform.position.y);
        }

        //clear the enemy position player prefs 
        PlayerPrefs.DeleteKey("EnemyX");
        PlayerPrefs.DeleteKey("EnemyY");
        PlayerPrefs.DeleteKey("EnemyX2");
        PlayerPrefs.DeleteKey("EnemyY2");
        PlayerPrefs.DeleteKey("EnemyX3");
        PlayerPrefs.DeleteKey("EnemyY3");
        PlayerPrefs.Save();

        //send a signal to the camera to Target itself
        cameraMovements.FollowOnOff(true);

        //fade back to the game
        screenFader.FadeToColor(Color.clear, 0.5f);

        //enable movement
        disableMovement = false;

        if (PlayerPrefs.GetInt("totalEnemies") == 1)
        {
            //move the player back to the start position
            transform.position = new Vector3(playerX, playerY, 0f);
            //move the enemy back to the start position
            currentEnemy.transform.position = new Vector3(enemyX, enemyY, 0f);
            StartCoroutine(EnemyTimeout(currentEnemy));
        }
        else if (PlayerPrefs.GetInt("totalEnemies") == 2)
        {
            //move the player back to the start position
            transform.position = new Vector3(playerX, playerY, 0f);
            //move the enemy back to the start position
            enemiesToFollow[0].transform.position = new Vector3(enemyX, enemyY, 0f);
            enemiesToFollow[1].transform.position = new Vector3(enemyX2, enemyY2, 0f);

            StartCoroutine(EnemyTimeout(enemiesToFollow[0]));
            StartCoroutine(EnemyTimeout(enemiesToFollow[1]));
        }
        else
        {
            //move the player back to the start position
            transform.position = new Vector3(playerX, playerY, 0f);
            //move the enemy back to the start position
            enemiesToFollow[0].transform.position = new Vector3(enemyX, enemyY, 0f);
            enemiesToFollow[1].transform.position = new Vector3(enemyX2, enemyY2, 0f);
            enemiesToFollow[2].transform.position = new Vector3(enemyX3, enemyY3, 0f);

            StartCoroutine(EnemyTimeout(enemiesToFollow[0]));
            StartCoroutine(EnemyTimeout(enemiesToFollow[1]));
            StartCoroutine(EnemyTimeout(enemiesToFollow[2]));
        }
        //remove current enemy
        currentEnemy = null;
    }

    //make a a request that will return the current enemy in battle
    public GameObject getCurrentEnemy()
    {
        return currentEnemy;
    }

    IEnumerator EnemyTimeout(GameObject Enemy)
    {
        //enemy can't be interacted with
        Enemy.GetComponent<BoxCollider2D>().enabled = false;

        //enemy starts blinking for 3 seconds
        for (int i = 0; i < 6; i++)
        {
            Enemy.GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSeconds(0.25f);
            Enemy.GetComponent<SpriteRenderer>().enabled = true;
            yield return new WaitForSeconds(0.25f);
        }
        Enemy.GetComponent<BoxCollider2D>().enabled = true;
        
        //no longer in battle mode
        Enemy.GetComponent<EnemyBehaviour>().notInBattle();
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

        //Initiate turn based combat : EnemyTurn
    public void EnemyTurn()
    {
        GameObject enemy = null;
        //get the current enemy from the player Object
        if (PlayerPrefs.GetInt("totalEnemies") == 1 && enemiesToFollow.Length == 0){
            enemy = getCurrentEnemy();
        }
        else
        {
            enemy = enemiesToFollow[0];
        }
        
        PlayerPrefs.SetInt("EnemyTurnCounter", 1);
        PlayerPrefs.Save();
        //get the enemy script
        EnemyBehaviour enemyScript = enemy.GetComponent<EnemyBehaviour>();

        StartCoroutine(enemyScript.DoAttackPhase());
    }

    public void NextEnemyTurn()
    {
        GameObject enemy = null;

        enemy = enemiesToFollow[PlayerPrefs.GetInt("EnemyTurnCounter") - 1];


        //get the enemy script
        EnemyBehaviour enemyScript = enemy.GetComponent<EnemyBehaviour>();

        StartCoroutine(enemyScript.DoAttackPhase());
    }

    
    public void PlayerTurn()
    {
        //pull up the menu
        menuController.ShowMenu();
    }

    public void sendAtkMoves()
    {
        // Get a reference to the ItemsMenu script

        // Convert the moves list to a list of Attacks objects
        List<Attacks> attacks = moves.Select(move => new Attacks
        {
            name = move.name,
            type = move.type,
            dmg = move.dmg,
            accuracy = move.accuracy,
            hits = move.hits,
            heal = move.heal
        }).ToList();

        // send list to 'receiveAtkMoves' in ItemsMenu
        menuController.receiveAtkMoves(attacks);
    }

    public void playerAttackMove(string name, bool itemMove, string itemType, float itemDmg, string target)
    {
        if (PlayerPrefs.GetInt("totalEnemies") > 1 && ((itemType == "Attacking" || itemType == "") && target != "All"))
        {
            foreach (GameObject enemy in enemiesToFollow)
            {
                originalColors[enemy] = enemy.GetComponent<SpriteRenderer>().material.color;
            }

            GameObject selectedEnemy = enemiesToFollow[selectedEnemyIndex];
            selectedEnemy.GetComponent<SpriteRenderer>().material.color = selectedColor;
            selectingEnemy = true;
            playeratkdmg = itemDmg;
            playermovename = name;
            playeritemMove = itemMove;
            playeritemType = itemType;
            playeritemtarget = target;
        }
        else if (itemMove == true)
        {
            itemActive = true;
            currentEnemy = getCurrentEnemy();
            StartCoroutine(StartPlayerAtk(name, itemActive, itemType, itemDmg, target));
        }
        else
        {
            //start the player attack
            itemActive = false; 
            currentEnemy = enemiesToFollow[0];
            StartCoroutine(StartPlayerAtk(name, itemActive, itemType, itemDmg, target));
        }
    }

    IEnumerator StartPlayerAtk(string moveName, bool itemActive, string itemType, float itemDmg, string target)
    {
        Vector3 currentPos;
        Vector3 endposition;
        float t;

        // Get the current enemy from the player Object
        GameObject enemy = getCurrentEnemy();

        // Get the enemy health
        EnemyBehaviour enemyScript = enemy.GetComponent<EnemyBehaviour>();
        int enemyHealth = enemyScript.currentHealth;

        // Get the move from the moves list
        Move move = moves.Find(m => m.name == moveName);


        if (itemActive == true){
            //wait 0.5 seconds
            yield return new WaitForSeconds(0.5f);

            if (itemType == "Status")
            {
                //heal the player (without the strength effect)
                currentHealth += (int)itemDmg;

                if (currentHealth > HealthCap)
                {
                    // Set the current health to the max health
                    currentHealth = HealthCap;
                }
            }
            else if (itemType == "Damaging")
            {
                nextAttackMultiplier = itemDmg; 
            }
            else
            {
                if (target == "All")
                {
                    //damage all enemies in enemiesToFollow array
                    foreach (GameObject enemyToFollow in enemiesToFollow)
                    {
                        // Get the enemy health
                        EnemyBehaviour enemyToFollowScript = enemyToFollow.GetComponent<EnemyBehaviour>();
                        int enemyToFollowHealth = enemyToFollowScript.currentHealth;

                        // Get the damage value from the move
                        float damage = itemDmg;

                        enemyToFollowScript.TakeDamage((int)(damage + (damage * strength) + (damage * nextAttackMultiplier)));

                        // Wait for 1 second
                        yield return new WaitForSeconds(0.25f);
                    }

                    foreach (GameObject enemyToFollow in enemiesToFollow.ToArray())
                    {
                        if (enemyScript.currentHealth <= 0)
                        {
                            //enemies dies 
                            PlayerPrefs.SetInt("totalEnemies", PlayerPrefs.GetInt("totalEnemies") - 1);
                            PlayerPrefs.Save();

                            //get the enemy level
                            int enemyLevel = enemyToFollow.GetComponent<EnemyBehaviour>().level;    
                            enemyLevelStacker += enemyLevel;

                            //get the index of the GameObject enemiesToFollow that is equal to the enemyToFollow
                            int indexToRemove = Array.IndexOf(enemiesToFollow, Array.Find(enemiesToFollow, enemy => enemy.name == enemyToFollow.name));

                            if (indexToRemove != -1)
                            {
                                for (int i = indexToRemove; i < enemiesToFollow.Length - 1; i++)
                                {
                                    enemiesToFollow[i] = enemiesToFollow[i + 1];
                                }
                                Array.Resize(ref enemiesToFollow, enemiesToFollow.Length - 1);
                            }

                            StartCoroutine(enemyToFollow.GetComponent<EnemyBehaviour>().Die());
                        }
                    }

                    //check if the enemies to follow list is empty 
                    if (PlayerPrefs.GetInt("totalEnemies") == 0)
                    {
                        //end enemy turn and end function 
                        menuController.BattleOver(enemyLevelStacker);
                        yield break;
                    }

                    //start enemy turn and end function 
                    nextAttackMultiplier = 0;
                    EnemyTurn();
                    yield break;
                }
            }
            if (target == "One")
            {
                // Get the damage value from the move
                int damage = (int)itemDmg;

                // Apply damage to the enemy
                enemyScript.TakeDamage(damage + (int)(damage * nextAttackMultiplier));

                // Reset the next attack multiplier
                nextAttackMultiplier = 0;
            }
        }
        //check if the move is a healing more or attacking move
        else if (move.type == "Status")
        {
            int heal = move.heal;
            // Heal the player
            currentHealth += (int)((float)heal + ((float)heal * strength));

            // Check if the heal is greater than the max health
            if (currentHealth > HealthCap)
            {
                // Set the current health to the max health
                currentHealth = HealthCap;
            }

            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            // Get the damage value from the move
            int damage = move.dmg;

            // Get the accuracy value from the move
            int accuracy = move.accuracy;

            // Get the number of hits from the move
            int hits = move.hits;

            // Get the player object
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            // Get the player script
            SpriteMovement playerScript = player.GetComponent<SpriteMovement>();
            //slide the character to the middle
            
            //get the current position of the player
            currentPos = transform.position;
            endposition = new Vector3(-27f, 2, 0);

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.5f; // Move over 0.5 seconds
                transform.position = Vector3.Lerp(currentPos, endposition, t);
                yield return null;
            }

            //execute some code after the 0.5 seconds have passed

            // Attack name
            string name = move.name;

            // Attack as many times as the move hits
            for (int i = 0; i < hits; i++)
            {
                // Check if the attack hits
                if (UnityEngine.Random.Range(0, 101) <= accuracy)
                {
                    // Attack hits
                    // Deal damage to the enemy
                    enemyScript.TakeDamage((int)((float)damage + ((float)damage * strength) + ((float)damage * nextAttackMultiplier)));
                }
                else
                {
                    // Attack misses
                    Debug.Log("Attack misses");
                }

                // Wait for 1 second
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
            //wait for 0.5 seconds
            yield return new WaitForSeconds(0.5f);
        }

        //wait for 0.5 seconds
        yield return new WaitForSeconds(0.5f);

        //enemy turn
        if (enemyScript.currentHealth <= 0)
        {
            PlayerPrefs.SetInt("totalEnemies", PlayerPrefs.GetInt("totalEnemies") - 1);
            PlayerPrefs.Save();

            Debug.Log(PlayerPrefs.GetInt("totalEnemies"));

            //get the enemy level 
            int enemyLevel = enemyScript.level;

            enemyLevelStacker += enemyLevel;

            if (PlayerPrefs.GetInt("totalEnemies") == 0)
            {
                //enemy dies
                StartCoroutine(enemyScript.Die());
                menuController.BattleOver(enemyLevelStacker);
            }
            else
            {
                int indexToRemove = Array.IndexOf(enemiesToFollow, Array.Find(enemiesToFollow, enemy => enemy.name == currentEnemy.name));

                if (indexToRemove != -1)
                {
                    for (int i = indexToRemove; i < enemiesToFollow.Length - 1; i++)
                    {
                        enemiesToFollow[i] = enemiesToFollow[i + 1];
                    }
                    Array.Resize(ref enemiesToFollow, enemiesToFollow.Length - 1);
                }

                StartCoroutine(enemyScript.Die());
                EnemyTurn();
            }
        }
        else
        {
            EnemyTurn();
        }
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

        currentEnemy = null;
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
    public int dmg;
    public int accuracy;
    public int hits;
    public int heal;
}
