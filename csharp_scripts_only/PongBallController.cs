using UnityEngine;
using System.Collections;

public class PongBallController : MonoBehaviour
{
    // Ball Variables
    private Transform trBall;
    private Rigidbody rbBall;

    // Paddle1 Variables
    public GameObject paddle1;
    private Transform trPaddle1;

    // Paddle2 Variables
    public GameObject paddle2;
    private Transform trPaddle2;

    // Paddle offset for ball reset
    public float paddleOffSet = 0.5f;

    // Player to Fire Ball
    private int playerToFire = 1;

    // Ball variables
    private bool ballFired = false;
    private bool ballReset = false;
    public float maxBallPosition;
    private Vector3 velocityDirection = Vector3.zero;
    public float ballSpeed = 3;
    

    //***************************************************************************
    void Start()
    {
        // Assign ball transform & rigidbody variables
        trBall = this.transform;
        rbBall = this.GetComponent<Rigidbody>();

        // Assign Paddle transform variables
        trPaddle1 = paddle1.transform;
        trPaddle2 = paddle2.transform;

        ResetBall(playerToFire);
    }

    //***************************************************************************
    // Reset ball position in reference to paddle location
    public void ResetBall(int player)
    {
        //Reset ball fired state
        ballFired = false;

        // Player who resets ball is the player who lost, 
        // so is also the player who will fire the ball
        playerToFire = player;

        // Initial Velocity equals 0
        rbBall.velocity = new Vector3(0, 0, 0).normalized;

        // Wait for a small moment before ball can be fired
        StartCoroutine(BallSet());
    }

    //***************************************************************************
    private IEnumerator BallSet()
    {
        yield return new WaitForSeconds(1.0f);
        ballReset = true;
    }

    //***************************************************************************
    private void FixedUpdate()
    {
        // If ball not fired (game not on)
        // then ball tracks paddle position
        if (ballFired == false)
        {
            // Track paddle position depeding on ball location
            // if ball.x < 0 then on left, or Paddle 1 side
            // if ball.x > 0 then on right, or Paddle 2 side
            if (trBall.localPosition.x < 0)
            {
                // Set ball position relative to paddle 1
                trBall.localPosition = new Vector3(trPaddle1.localPosition.x + paddleOffSet, (trBall.localScale.y / 2), 
                    Mathf.Clamp(trPaddle1.localPosition.z, -maxBallPosition, maxBallPosition));

                // add small pentaly to paddle 2 agent for not firing ball
                paddle1.GetComponent<PongPaddleAgent>().AddReward(-0.01f);
            }
            else
            {
                // Set ball position relative to paddle 2
                trBall.localPosition = new Vector3(trPaddle2.localPosition.x - paddleOffSet, (trBall.localScale.y / 2),
                    Mathf.Clamp(trPaddle2.localPosition.z, -maxBallPosition, maxBallPosition));

                // add small pentaly to paddle 2 agent for not firing ball
                paddle2.GetComponent<PongPaddleAgent>().AddReward(-0.01f);
            }
        }
    }

    //***************************************************************************
    // Fire ball to start moving
    public void FireBall(int player)
    {
        if (ballFired == false && ballReset == true)
        {
            // Check to see if fire command came from correct player
            if (player == playerToFire)
            {
                // Fire ball using:
                // positive velocity for paddle 1
                // negative velocity for paddle 2
                if (trBall.localPosition.x < 0)
                {
                    velocityDirection = new Vector3(1, 0, 0).normalized;
                }
                else
                {
                    velocityDirection = new Vector3(-1, 0, 0).normalized;
                }

                // Initial Velocity
                rbBall.velocity = velocityDirection * ballSpeed;

                //Set ball fired
                ballFired = true;
                ballReset = false;
            }
        }
    }

    //***************************************************************************
    // Process Paddle collisions and update score
    void OnCollisionEnter(Collision col)
    {
        ///////////////////////////////////////
        // Hit Paddle 1 (Player 1)
        if (col.gameObject.name == "Paddle1")
        {
            // Calculate hit Factor as defined in below function
            float z = hitFactor(trBall.localPosition,
                col.transform.localPosition,
                col.collider.bounds.size.z);

            // Calculate direction, make length=1 via .normalized
            velocityDirection = new Vector3(1, 0, z).normalized;

            // Set Velocity with dir * speed
            rbBall.velocity = velocityDirection * ballSpeed;
        }

        ///////////////////////////////////////
        // Hit Paddle 2 (Player 2)
        if (col.gameObject.name == "Paddle2")
        {
            // Calculate hit Factor as defined in above function
            float z = hitFactor(transform.localPosition,
                col.transform.localPosition,
                col.collider.bounds.size.z);

            // Calculate direction, make length=1 via .normalized
            velocityDirection = new Vector3(-1, 0, z).normalized;

            // Set Velocity with dir * speed
            GetComponent<Rigidbody>().velocity = velocityDirection * ballSpeed;
        }

        // Reflect (Bounce Ball) off Bounds
        if (col.gameObject.name == "TopBound" || col.gameObject.name == "BottomBound")
        {
            // Get Reflection Velocity on Collision
            Vector3 direction = Vector3.Reflect(velocityDirection, col.contacts[0].normal);

            // Update normlaized velocity direction of ball of ball
            velocityDirection = new Vector3(velocityDirection.x, 0, direction.z).normalized;

            // Se new velocity
            GetComponent<Rigidbody>().velocity = velocityDirection * ballSpeed;
        }
    }

    //***************************************************************************
    // If ball hit paddle, set new ball direction
    float hitFactor(Vector3 ballPos, Vector3 paddlePos, float paddleSize)
    {
        // ||  1 <- at the top of the paddle
        // ||
        // ||  0 <- at the middle of the paddle
        // ||
        // || -1 <- at the bottom of the paddle
        return Mathf.Clamp(((ballPos.z - paddlePos.z) / paddleSize), -1.0f, 1.0f);
    }

}
