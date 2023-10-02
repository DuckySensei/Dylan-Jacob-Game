using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SpriteMovement : MonoBehaviour
{

    //configurable variables
    public float speed = 5f;
    bool disableMovement = false;
    public CameraMovements cameraMovements;
    public ScreenFader screenFader;

    //get script BattleMenu
    public BattleMenu atkmenu;
    public float detectionRange = 1f;
    public string enemyTag = "Enemy";

    private void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f).normalized * speed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;

        // Check for collisions before moving
        Collider2D[] colliders = Physics2D.OverlapBoxAll(newPosition, transform.localScale, 0f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                return;
            }
        }

        //check if enemy is in detectionRange
        Collider2D[] collidersInRange = Physics2D.OverlapCircleAll(transform.position, detectionRange);

        //check if enemy is in range, and has the tag "Enemy" then continue through
        foreach (Collider2D collider in collidersInRange)
        {
            if (collider.gameObject.CompareTag(enemyTag) & disableMovement == false)
            {
                //check to see if you hit the enemy
                if (Input.GetKeyDown(KeyCode.E) || (Input.GetKeyDown(KeyCode.E) && Input.anyKeyDown))
                {   
                    
                    //disable movement
                    disableMovement = true;
                    StartCoroutine(HandleEnemyHit(collider.gameObject));
                }
            }
        }

        // Move the sprite if there are no collisions and movement is not disabled
        if (!disableMovement)
        {
            transform.position = newPosition;
        }
    }

    //disable 'e' for a certain amount of time

    IEnumerator HandleEnemyHit(GameObject enemy)
    {
        screenFader.FadeToColor(Color.white, 0.5f);

        //wait for 0.5 seconds
        yield return new WaitForSeconds(0.5f);
                                    
        //move the character sprite to x=-31.5 and y = 2
        transform.position = new Vector3(-29.5f, 2f, 0f);
                                    
        Vector3 startPosition = new Vector3(-18f, 2f, 0f);
        Vector3 endPosition = new Vector3(-24.5f, 2f, 0f);
        enemy.gameObject.transform.position = startPosition;

        //send a signal to the camera to Target itself
        cameraMovements.FollowOnOff(false);
        Camera.main.transform.position = new Vector3(-27f, 0f, -30f);

        //fade back to the game
        screenFader.FadeToColor(Color.clear, 0.5f);

        // Move enemy sprite to end position using a coroutine

        // Show the menu when the enemy is hit
        atkmenu.ShowMenu();

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.5f; // Move over 0.5 seconds
            enemy.gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

    }
}


