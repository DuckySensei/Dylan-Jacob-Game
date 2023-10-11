using UnityEngine;
using System.Collections;

public class EnemyBehaviour : MonoBehaviour
{
    //add moves to enemy
    public object[] moves = {
        new { name = "Nibble", dmg = 10, accuracy = 60, hits = 3},
        new { name = "Pee On", dmg = 50, accuracy = 40, hits = 1 },
        new { name = "Masterbate", heal = 10 , accuracy = 100 }
    };

    private int hitcounter = 0;
    private bool inBattle = false;
    public int range = 10;
    public int currentHealth;
    public int maxHealth = 100;
    public int level = 1;
    public int attack = 10;
    public int speed = 4;
    public bool follow = false;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        playerInRange();
    }

    private void playerInRange()
    {
        // Get the player object
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null || inBattle == true)
        {
            return;
        }

        // Get the distance between the player and the enemy
        float distance = Vector3.Distance(transform.position, player.transform.position);

        // Get the direction from the enemy to the player
        Vector3 direction = player.transform.position - transform.position;

        // Check if the player is within the range south of the enemy
        if (distance < range && direction.y < 0 && !follow)
        {
            follow = true;
        }
        else 
        {
            float chase = range * 3f;
            if (distance < chase && direction.y < chase && follow){
                // Round the direction to the nearest cardinal direction
                int x = Mathf.RoundToInt(direction.x);
                int y = Mathf.RoundToInt(direction.y);
                if (Mathf.Abs(x) > Mathf.Abs(y))
                {
                    y = 0;
                }
                else
                {
                    x = 0;
                }

                // Move the enemy in the rounded direction
                Vector3 newPosition = transform.position + new Vector3(x, y, 0);
                transform.position = Vector3.MoveTowards(transform.position, newPosition, speed * Time.deltaTime);
            }
        }
    }

    public void notInBattle(){inBattle = false;}

    public void stopPerusing()
    {
        follow = false;
        inBattle = true;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public IEnumerator DoAttackPhase()
    {
        Debug.Log("DoAttackPhase");
        //pick a move randomly from the list of moves
        int moveIndex = Random.Range(0, moves.Length);
        //get the move from the list
        object move = moves[moveIndex];

        //check if the move is a healing move
        if (move.GetType().GetProperty("heal") != null)
        {
            Debug.Log("Healing");
            //get the heal value from the move
            int heal = (int)move.GetType().GetProperty("heal").GetValue(move, null);
            //heal the enemy
            currentHealth += heal;
            //check if the heal is greater than the max health
            if (currentHealth > maxHealth)
            {
                //set the current health to the max health
                currentHealth = maxHealth;
            }
        }
        else
        {
            //get the damage value from the move
            int damage = (int)move.GetType().GetProperty("dmg").GetValue(move, null);
            //get the accuracy value from the move
            int accuracy = (int)move.GetType().GetProperty("accuracy").GetValue(move, null);

            //get the player object
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            //get the player script
            SpriteMovement playerScript = player.GetComponent<SpriteMovement>();

            //slide the character to the middle
            StartCoroutine(SlideToMiddle());

            //Attack name
            string name = (string)move.GetType().GetProperty("name").GetValue(move, null);
            Debug.Log("Enemy used " + name);
            

            //attack as many time as the move hits
            for (int i = 0; i < (int)move.GetType().GetProperty("hits").GetValue(move, null); i++)
            {
                //check if the attack hits
                if (Random.Range(0, 100) < accuracy)
                {
                    //deal damage to the player
                    playerScript.TakeDamage(damage);
                    hitcounter++;
                }
                else
                {
                    //attack missed
                    Debug.Log("Attack Missed");
                }
                //wait for 1 second
                yield return new WaitForSeconds(1);
            }

            Debug.Log("Enemy hit " + hitcounter + " times");

            //This is where you'd wanna see if the player is DEAD

            //wait for 1 second
            yield return new WaitForSeconds(1);

            StartCoroutine(SlideToStarting());

            yield return new WaitForSeconds(0.5f);

            //players turn
            playerScript.PlayerTurn();
        }
    }

    //slide the enemy to the middle of the screen (27, 2, 0)
    IEnumerator SlideToMiddle()
    {
        //get the current position of the enemy
        Vector3 startPos = transform.localPosition;
        Vector3 endPos = new Vector3(-27, 2, 0);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.5f; // Move over 0.5 seconds
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }

    IEnumerator SlideToStarting()
    {
        //get the current position of the enemy
        Vector3 startPos = transform.localPosition;
        Vector3 endPos = new Vector3(-24.5f, 2, 0);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.5f; // Move over 0.5 seconds
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }

    private void Die()
    {
        // ...
    }
}