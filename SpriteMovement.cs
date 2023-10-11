using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SpriteMovement : MonoBehaviour
{
    public object[] moves = {
        new { name = "Nibble", type = "Damanging", dmg = 10, accuracy = 60, hits = 3},
        new { name = "Pee On", type = "Damanging", dmg = 50, accuracy = 40, hits = 1 },
        new { name = "Masterbate", type = "Status", heal = 10 , accuracy = 100 }
    };

    //configurable variables
    public float speed = 5f;
    bool disableMovement = false;
    public CameraMovements cameraMovements;
    private GameObject currentEnemy;
    public ScreenFader screenFader;

    public ItemsMenu itemMenu;

    //get script BattleMenu
    public BattleMenu atkmenu;

    private PlayerPrefs playerPrefs;
    public float detectionRange = 1f;
    public string enemyTag = "Enemy";

    public int currentHealth = 100;

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

        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f).normalized * speed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;

        // Check for collisions before moving
        Collider2D[] colliders = Physics2D.OverlapBoxAll(newPosition, transform.localScale, 0f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                //check if the gameobject is an enemy
                if (collider.gameObject.CompareTag(enemyTag))
                {
                    //startbattle
                    currentEnemy = collider.gameObject;

                    //Save the player's current position
                    PlayerPrefs.SetFloat("PlayerX", transform.position.x);
                    PlayerPrefs.SetFloat("PlayerY", transform.position.y);

                    //save the position of the enemy
                    PlayerPrefs.SetFloat("EnemyX", collider.gameObject.transform.position.x);
                    PlayerPrefs.SetFloat("EnemyY", collider.gameObject.transform.position.y);

                    //check to see if the enemy follow was true
                    EnemyBehaviour enemyBehavior = currentEnemy.GetComponent<EnemyBehaviour>();
                    bool enemyFollow = enemyBehavior.follow;

                    if (enemyFollow && !disableMovement)
                    {
                        StartCoroutine(HandleEnemyHit(collider.gameObject, true));

                    }
                    else if (!disableMovement)
                    {
                        StartCoroutine(HandleEnemyHit(collider.gameObject, false));
                    }

                    disableMovement = true;
                }
                else {return;}
            }
        }

        //check if enemy is in detectionRange
        Collider2D[] collidersInRange = Physics2D.OverlapCircleAll(transform.position, detectionRange);

        //check if enemy is in range, and has the tag "Enemy" then continue through
        foreach (Collider2D collider in collidersInRange)
        {
            if (collider.gameObject.CompareTag(enemyTag) & disableMovement == false && collider.gameObject.GetComponent<EnemyBehaviour>().follow == false)
            {
                //check to see if you hit the enemy and enemy isn't following
                if (Input.GetKeyDown(KeyCode.E) || (Input.GetKeyDown(KeyCode.E) && Input.anyKeyDown))
                {   
                    currentEnemy = collider.gameObject;
                    //disable movement
                    disableMovement = true;
                    StartCoroutine(HandleEnemyHit(collider.gameObject, false));

                    //Save the player's current position
                    PlayerPrefs.SetFloat("PlayerX", transform.position.x);
                    PlayerPrefs.SetFloat("PlayerY", transform.position.y);

                    //save the position of the enemy
                    PlayerPrefs.SetFloat("EnemyX", collider.gameObject.transform.position.x);
                    PlayerPrefs.SetFloat("EnemyY", collider.gameObject.transform.position.y);
                }
            }
        }

        //if player collides with the enemy, start the battle


        // Move the sprite if there are no collisions and movement is not disabled
        if (!disableMovement)
        {
            transform.position = newPosition;
        }
    }

    //disable 'e' for a certain amount of time

    IEnumerator HandleEnemyHit(GameObject enemy, bool gothit)
    {
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
        enemy.gameObject.transform.position = startPosition;

        // Get the enemy's level and health values
        int enemyLevel = enemyBehavior.level;
        int enemyHealth = enemyBehavior.currentHealth;

        Debug.Log("Enemy level: " + enemyLevel);
        Debug.Log("Enemy health: " + enemyHealth);

        //send a signal to the camera to Target itself
        cameraMovements.FollowOnOff(false);
        Camera.main.transform.position = new Vector3(-27f, 0f, -30f);

        //fade back to the game
        screenFader.FadeToColor(Color.clear, 0.5f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.5f; // Move over 0.5 seconds
            enemy.gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        if (!gothit){
            // Show the menu when the enemy is hit
            atkmenu.ShowMenu();
        }
        else
        {
            Debug.Log("Enemy hit ME!");
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
        EnemyBehaviour enemyBehavior = currentEnemy.GetComponent<EnemyBehaviour>();
        int enemyLevel = enemyBehavior.level;

        //two numbers that will be compared
        int playerRoll = Random.Range(enemyLevel, 101);
        int enemyRoll = Random.Range(enemyLevel, 101);

        //role the DICE
        if (playerRoll == enemyRoll)
        {
            //Flee Unsuccessful
            Debug.Log("Flee Unsuccessful");
            StartCoroutine(fleeUnSuccess());
        }
        else
        {
            //Flee Successfully!
            Debug.Log("Flee Successfully!");
            StartCoroutine(fleeSuccess());
        }
    }

    IEnumerator fleeUnSuccess()
    {
        //disable menu
        atkmenu.menu.SetActive(false);

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

        atkmenu.menu.SetActive(true);
    }

    IEnumerator fleeSuccess()
    {
        //disable menu
        atkmenu.menu.SetActive(false);

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

        //load the enemy saved position
        float enemyX = PlayerPrefs.GetFloat("EnemyX");
        float enemyY = PlayerPrefs.GetFloat("EnemyY");

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

        //move the enemy to the saved position
        currentEnemy.transform.position = new Vector3(enemyX, enemyY, 0f);

        //enemy starts blinking for 3 seconds
        StartCoroutine(EnemyTimeout(currentEnemy));

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
    }

        //Initiate turn based combat : EnemyTurn
    public void EnemyTurn()
    {
        //get the current enemy from the player Object
        GameObject enemy = getCurrentEnemy();
        
        //get the enemy script
        EnemyBehaviour enemyScript = enemy.GetComponent<EnemyBehaviour>();

        StartCoroutine(enemyScript.DoAttackPhase());
    }

    
    public void PlayerTurn()
    {
        //pull up the menu
        atkmenu.ShowMenu();
    }

    public void sendAtkMoves()
    {
        //send moves to items menu via the items menu script
        itemMenu.recieveAtkMove(moves);
    }

    public void playerAttackMove(string moveName)
    {
        //get the current enemy from the player Object
        GameObject enemy = getCurrentEnemy();

        //get the enemy health
        EnemyBehaviour enemyScript = enemy.GetComponent<EnemyBehaviour>();
        int enemyHealth = enemyScript.currentHealth;

        //get the move object that has the same name as the moveName
        object move = System.Array.Find(moves, x => x.GetType().GetProperty("name").GetValue(x, null).ToString() == moveName);

        //print the move name
        Debug.Log(move.GetType().GetProperty("name").GetValue(move, null).ToString());
    }
}