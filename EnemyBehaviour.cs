using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    public object[] moves = {
        new { name = "Nibble", dmg =12, accuracy = 40, hits = 3},
        new { name = "Pee On", dmg = 25, accuracy = 80, hits = 1 },
        new { name = "Masterbate", heal = 10 , accuracy = 100 }
    };
    public battleController battleController;
    private int hitcounter = 0;
    public int currentHealth;
    public int maxHealth = 100;
    public int level = 1;
    public int attack = 10;
    public bool inBattle;
    [SerializeField] LayerMask playerLayer;
    private Rigidbody2D rb;
    Transform target;
    float range = 4f;
    private Vector3 startPos;
    private Vector3 endPos;
    bool wandering = true;
    NavMeshAgent agent;
    public bool followingPoints;
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    // Adjust this speed value as needed
    [SerializeField] float movementSpeed = 3f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.acceleration = 10000f; // High value to make the agent reach its max speed instantly
        agent.autoBraking = false; // Prevents the agent from slowing down before reaching the destination

        target = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true;

        inBattle = false;

        if(followingPoints)
        {
            moveTowardsWaypoint();
            agent.enabled = false;
        }
        else
        {
            StartCoroutine(Wander());
            agent.enabled = true;
        }
    }

    void Update()
    {
        if (inBattle || followingPoints)
        {
            if (followingPoints == true)
            {
                moveTowardsWaypoint();
            }
            agent.enabled = false;
        }
        else if (IsPlayerInFieldOfView(target.position) && IsPlayerInRange(range) && wandering == true)
        {
            StopAllCoroutines();
            followPlayer();
            wandering = false;
        }
        else if (wandering == false && IsPlayerInRange(range) == true && IsPlayerInFieldOfView(target.position) == true)
        {
            followPlayer();
        }
        else if (wandering == false && IsPlayerInRange(range * 2) == false)
        {
            wandering = true;
            StartCoroutine(Wander());
        }
    }

    public void moveTowardsWaypoint()
    {
        if (currentWaypointIndex < waypoints.Length)
        {
            Vector3 targetPosition = waypoints[currentWaypointIndex].position;
            float step = movementSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            if (transform.position == targetPosition)
            {
                currentWaypointIndex++;
            }
        }
        else
        {
            // Reset to the first waypoint to create a looping path
            currentWaypointIndex = 0;
        }
    }

    void followPlayer()
    {
        // Calculate the direction to the player
        Vector2 directionToPlayer = (target.position - transform.position).normalized;

        // Define the eight directions
        Vector2[] directions = new Vector2[]
        {
            (Vector2.up + Vector2.right).normalized, // North-East
            Vector2.right, // East
            (Vector2.down + Vector2.right).normalized, // South-East
            (Vector2.down + Vector2.left).normalized, // South-West
            Vector2.left, // West
            (Vector2.up + Vector2.left).normalized // North-West
        };

        // Find the direction closest to directionToPlayer
        Vector2 closestDirection = directions[0];
        float closestDot = Vector2.Dot(directions[0], directionToPlayer);

        for (int i = 1; i < directions.Length; i++)
        {
            float dot = Vector2.Dot(directions[i], directionToPlayer);
            if (dot > closestDot)
            {
                closestDirection = directions[i];
                closestDot = dot;
            }
        }

        // Set the destination
        Vector3 currentPos = transform.position;
        Vector3 direction3D = new Vector3(closestDirection.x, closestDirection.y, currentPos.z);
        agent.SetDestination(currentPos + direction3D * 3); // Multiply by 3 to control the distance
    }

    IEnumerator Wander()
    {
        while (true)
        {
            // Get a random cardinal or intercardinal direction in 2D
            Vector2 wanderDirection = GetRandomCardinalDirection();

            // Set the destination for NavMeshAgent
            agent.SetDestination(transform.position + new Vector3(wanderDirection.x, wanderDirection.y, 0) * 3);

            // Wait for a random amount of time before changing direction
            yield return new WaitForSeconds(Random.Range(2f, 4f));
        }
    }


    Vector2 GetRandomCardinalDirection()
    {
        int randomIndex = Random.Range(0, 8); // 0: North, 1: East, 2: South, 3: West
        Vector2[] cardinalDirections = new Vector2[]
            {
                Vector2.up, // North
                (Vector2.up + Vector2.right).normalized, // North-East
                Vector2.right, // East
                (Vector2.down + Vector2.right).normalized, // South-East
                Vector2.down, // South
                (Vector2.down + Vector2.left).normalized, // South-West
                Vector2.left, // West
                (Vector2.up + Vector2.left).normalized // North-West
            };
        return cardinalDirections[randomIndex];
    }

    bool IsPlayerInFieldOfView(Vector2 direction)
    {
        float angleToDirection = Vector2.Angle(rb.velocity.normalized, direction);

        // Adjust the field of view angle as needed
        float fieldOfView = 15f;

        // Check if the angle to the direction is within the field of view
        return angleToDirection < fieldOfView * 0.5f;
    }

    bool IsPlayerInRange(float range)
    {
        // Check if the player is within the range
        return Vector2.Distance(transform.position, target.position) <= range;
    }

    public void EnemyTimeout()
    {
        StartCoroutine(animEnemyTimeout(gameObject));
    }

    public IEnumerator animEnemyTimeout(GameObject Enemy)
    {
        //enemy can't be interacted with (circle collider)
        Enemy.GetComponent<CircleCollider2D>().enabled = false;

        //enemy starts blinking for 3 seconds
        for (int i = 0; i < 4; i++)
        {
            Enemy.GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSeconds(0.2f);
            Enemy.GetComponent<SpriteRenderer>().enabled = true;
            yield return new WaitForSeconds(0.2f);
        }
        Enemy.GetComponent<CircleCollider2D>().enabled = true;
        inBattle = false;
        agent.enabled = true;
        if (waypoints.Length > 0)
        {
            followingPoints = true;
        }
        else
        {
            StartCoroutine(Wander());
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }

    public void Attack()
    {
        StartCoroutine(DoAttackPhase());
    }
    public IEnumerator DoAttackPhase()
    {
        yield return new WaitForSeconds(1);
        //get the player object
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        //get the player script
        SpriteMovement playerScript = player.GetComponent<SpriteMovement>();
        //pick a move randomly from the list of moves
        int moveIndex = Random.Range(0, moves.Length);
        //get the move from the list
        object move = moves[moveIndex];

        //check if the move is a healing move
        if (move.GetType().GetProperty("heal") != null)
        {
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

            yield return new WaitForSeconds(1);
        }
        else
        {
            //get the damage value from the move
            int damage = (int)move.GetType().GetProperty("dmg").GetValue(move, null);
            //get the accuracy value from the move
            int accuracy = (int)move.GetType().GetProperty("accuracy").GetValue(move, null);

            //slide the character to the middle
            StartCoroutine(SlideToMiddle());

            //Attack name
            string name = (string)move.GetType().GetProperty("name").GetValue(move, null);

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
                }
                //wait for 1 second
                yield return new WaitForSeconds(0.25f);
            }

            //This is where you'd wanna see if the player is DEAD

            //wait for 1 second
            yield return new WaitForSeconds(1);

            StartCoroutine(SlideToStarting());

            yield return new WaitForSeconds(0.5f);
        }

        //check if the player is dead
        if (playerScript.currentHealth <= 0)
        {
            Debug.Log("Player is dead");
        }
        else
        {
            battleController.turnChecker();
        }
    }

    public void setInBattle(bool inBattle)
    {
        this.inBattle = inBattle;
        followingPoints = false;
    }

    //slide the enemy to the middle of the screen (27, 2, 0)
    IEnumerator SlideToMiddle()
    {
        //get the current position of the enemy
        startPos = transform.localPosition;
        endPos = new Vector3(-27, 2, 0);

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
        endPos = startPos;
        startPos = transform.localPosition;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.5f; // Move over 0.5 seconds
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }

    public IEnumerator Die()
    {
        //fade out enemy in 3 steps. like transparency 1, 0.66, 0.33, 0
        Renderer renderer = GetComponent<Renderer>();
        Color color = renderer.material.color;

        float fadeStep = 0.33f;
        for (float alpha = 1f; alpha >= 0f; alpha -= fadeStep)
        {
            color.a = alpha;
            renderer.material.color = color;
            yield return new WaitForSeconds(0.5f);
        }

        //destroy the enemy
        Destroy(gameObject);
    }
}
