using System.Linq;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class battleController : MonoBehaviour
{
    //public variables
    public GameObject player;
    public SettingsMenu settingsMenu;
    public SpriteMovement spriteMovement;
    public ScreenFader screenFader;
    public CameraMovements cameraMovements;
    public MenuController menuController;
    private bool multipleAttacks = false;
    private int currAtkNum = 0;
    public int enemyExp = 0;

    //private variables
    public GameObject[] enemiesToFollow;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject[] getEnemyArray()
    {
        updateFollowingEnemies();
        return enemiesToFollow;
    }

    public void getFollowingEnemies()
    {
        // get all game objects with the enemy tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        enemiesToFollow = enemies
            .Where(enemy => enemy.GetComponent<EnemyBehaviour>().inBattle == true)
            .OrderBy(enemy => int.Parse(enemy.name.Split('(', ')')[1]))
            .ToArray();
    }

    private void updateFollowingEnemies()
    {
        //update the enemiesToFollow array, if an enemy doesn't exist anymore, remove it from the list
        for (int i = 0; i < enemiesToFollow.Length; i++)
        {
            if (enemiesToFollow[i] == null)
            {
                enemiesToFollow = enemiesToFollow.Where((source, index) => index != i).ToArray();
            }
        }
    }

    public void setBattlePositions(GameObject enemy)
    {
        settingsMenu.turnOffOn(true);
        //startbattle
        bool playerTurn = false;
        getFollowingEnemies();

        PlayerPrefs.SetInt("EnemyTurnCounter", 0);

        if (enemiesToFollow.Length == 0){enemiesToFollow = new GameObject[] { enemy }; playerTurn = true;}
        else if (!enemiesToFollow.Contains(enemy))
        {
            Array.Resize(ref enemiesToFollow, enemiesToFollow.Length + 1);
            for (int i = enemiesToFollow.Length - 1; i > 0; i--)
            {
                enemiesToFollow[i] = enemiesToFollow[i - 1];
            }
            enemiesToFollow[0] = enemy;
            playerTurn = false;
        } else {playerTurn = false;}

        //Save the player's current position
        PlayerPrefs.SetFloat("PlayerX", transform.position.x);
        PlayerPrefs.SetFloat("PlayerY", transform.position.y);

        //print the total number of enemies
        int totalEnemies = enemiesToFollow.Length;
        PlayerPrefs.SetInt("totalEnemies", totalEnemies);

        if (enemiesToFollow.Length > 3)
        {
            Array.Resize(ref enemiesToFollow, 3);
        }

        for (int i = 0; i < totalEnemies; i++)
        {
            //save the position of the enemy
            PlayerPrefs.SetFloat($"EnemyX{i}", enemiesToFollow[i].transform.position.x);
            PlayerPrefs.SetFloat($"EnemyY{i}", enemiesToFollow[i].transform.position.y);

            enemiesToFollow[i].GetComponent<EnemyBehaviour>().setInBattle(true);
            //set the rigidbody velocity to 0
            enemiesToFollow[i].GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
        }

        //set the player rigidbody velocity to 0
        player.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
        PlayerPrefs.Save();
        StartCoroutine(HandleEnemyHit(playerTurn));
    }

    //--------------------------------------------------
    // Handle Enemy Hit
    //--------------------------------------------------

    IEnumerator HandleEnemyHit(bool playerTurn)
    {
        //fade to white
        screenFader.FadeToColor(Color.white, 0.5f);
        yield return new WaitForSeconds(0.5f);

        //move the character sprite to x=-31.5 and y = 2
        transform.position = new Vector3(-29.5f, 2f, 0f);

        //define the start and end positions for the enemies
        Vector3[] startPositions = new Vector3[] {
            new Vector3(-18f, 2f, 0f),
            new Vector3(-18f, 0f, 0f),
            new Vector3(-18f, 4f, 0f)
        };
        Vector3[] endPositions = new Vector3[] {
            new Vector3(-24.5f, 2f, 0f),
            new Vector3(-23.5f, 0f, 0f),
            new Vector3(-23.5f, 4f, 0f)
        };

        int totalEnemies = PlayerPrefs.GetInt("totalEnemies");
        for (int i = 0; i < totalEnemies; i++)
        {
            enemiesToFollow[i].gameObject.transform.position = startPositions[i];
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
            for (int i = 0; i < totalEnemies; i++)
            {
                enemiesToFollow[i].gameObject.transform.position = Vector3.Lerp(startPositions[i], endPositions[i], t);
            }
            yield return null;
        }

        if (playerTurn == true)
        {
            menuController.ShowMenu();
        }
        else
        {
            yield return new WaitForSeconds(0.25f);
            turnChecker();
        }
    }

    //--------------------------------------------------
    // Turn functions
    //--------------------------------------------------

    public void turnChecker()
    {
        updateFollowingEnemies();
        //get the total number of enemies
        int totalEnemies = PlayerPrefs.GetInt("totalEnemies");

        //get the current enemy turn
        int currentEnemyTurn = PlayerPrefs.GetInt("EnemyTurnCounter");

        if (totalEnemies == 0)
        {
            BattleOver();
            return;
        }

        //check if the enemy is the last one
        if (currentEnemyTurn == totalEnemies)
        {
            //reset the enemy turn counter
            PlayerPrefs.SetInt("EnemyTurnCounter", 0);

            //start the player's turn
            PlayerTurn();
        }
        else
        {
            menuController.updateDictionary();
            EnemyTurn(enemiesToFollow[currentEnemyTurn]);
            //increment the enemy turn counter
            PlayerPrefs.SetInt("EnemyTurnCounter", currentEnemyTurn + 1);
        }
    }

    private void PlayerTurn()
    {
        menuController.ShowMenu();
    }

    private void EnemyTurn(GameObject enemy)
    {
        enemy.GetComponent<EnemyBehaviour>().Attack();

    }

    //--------------------------------------------------
    //Flee Button
    //--------------------------------------------------

    public void flee()
    {
        int enemyRoller = enemiesToFollow[0].GetComponent<EnemyBehaviour>().level;
        int enemyRoll = UnityEngine.Random.Range(enemyRoller, 101);

        if (enemyRoll > 91) {StartCoroutine(fleeUnSuccess());}
        else {StartCoroutine(fleeSuccess());}
    }

    IEnumerator fleeUnSuccess()
    {
        //disable menu
        menuController.SlideMenuDown();

        //get the player gameobject position 
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
        turnChecker();
    }

    IEnumerator fleeSuccess()
    {
        updateFollowingEnemies();
        menuController.SlideMenuDown();
        menuController.ResetMenu();
        screenFader.FadeToColor(Color.white, 0.5f);

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

        yield return new WaitForSeconds(0.5f);
        menuController.menu.SetActive(false);

        float playerX = PlayerPrefs.GetFloat("PlayerX");
        float playerY = PlayerPrefs.GetFloat("PlayerY");
        int totalEnemies = PlayerPrefs.GetInt("totalEnemies");
        float[] enemyX = new float[totalEnemies];
        float[] enemyY = new float[totalEnemies];
        GameObject[] enemyTransforms = new GameObject[totalEnemies];

        // Loop to load enemy positions
        for (int i = 0; i < totalEnemies; i++)
        {
            enemyX[i] = PlayerPrefs.GetFloat($"EnemyX{i}", enemiesToFollow[i].transform.position.x);
            enemyY[i] = PlayerPrefs.GetFloat($"EnemyY{i}", enemiesToFollow[i].transform.position.y);
            enemyTransforms[i] = enemiesToFollow[i];
            PlayerPrefs.DeleteKey($"EnemyX{i}");
            PlayerPrefs.DeleteKey($"EnemyY{i}");
        }

        PlayerPrefs.Save();
        cameraMovements.FollowOnOff(true);

        for (int i = 0; i < totalEnemies; i++)
        {
            enemyTransforms[i].transform.position = new Vector3(enemyX[i], enemyY[i], 0f);
        }

        screenFader.FadeToColor(Color.clear, 0.5f);

        transform.position = new Vector3(playerX, playerY, 0f);
        // Loop to move player and enemies back to start position
        for (int i = 0; i < totalEnemies; i++)
        {
            enemyTransforms[i].GetComponent<EnemyBehaviour>().EnemyTimeout();
        }

        spriteMovement.setdisableMovement(false);
        settingsMenu.turnOffOn(false);
    }

    public void setAttackUp(int enemyID, string atkName, bool allOrOne)
    {
        multipleAttacks = allOrOne;
        if (allOrOne == true)
        {
            for (int i = 0; i < enemiesToFollow.Length; i++)
            {
                spriteMovement.startAttack(enemiesToFollow[i], atkName);
            }
        }
        else
        {
            spriteMovement.startAttack(enemiesToFollow[enemyID], atkName);
        }
    }

    public void PlayerTurnOver()
    {

        //get all the enemies with a health of 0
        GameObject[] deadEnemies = enemiesToFollow.Where(enemy => enemy.GetComponent<EnemyBehaviour>().currentHealth <= 0).ToArray();
        
        foreach (GameObject enemy in deadEnemies)
        {
            enemyExp += enemy.GetComponent<EnemyBehaviour>().level;
            StartCoroutine(enemy.GetComponent<EnemyBehaviour>().Die());
            PlayerPrefs.SetInt("totalEnemies", PlayerPrefs.GetInt("totalEnemies") - 1);
            removeDeadEnemy(enemy);
        }

        if (multipleAttacks == true)
        {
            currAtkNum++;
            if (currAtkNum == PlayerPrefs.GetInt("totalEnemies") || PlayerPrefs.GetInt("totalEnemies") == 0)
            {
                PlayerPrefs.SetInt("EnemyTurnCounter", 0);
                currAtkNum = 0;
                multipleAttacks = false;
                turnChecker();
            }
        }
        else
        {
            turnChecker();
        }
    }

    public void removeDeadEnemy(GameObject enemy)
    {
        //remove the enemy from the list
        enemiesToFollow = enemiesToFollow.Where(val => val != enemy).ToArray();
    }

    public void BattleOver()
    {
        settingsMenu.turnOffOn(true);
        menuController.BattleOver(enemyExp);
        enemiesToFollow = new GameObject[] { };
    }
}