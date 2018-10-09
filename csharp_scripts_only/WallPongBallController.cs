using UnityEngine;

public class WallPongBallController : MonoBehaviour
{
    // Ball Variables
    private Transform trBall;
    private Rigidbody rbBall;

    // Paddle Variables
    public GameObject paddle;
    private Transform trPaddle;
    public float paddleOffSet = 0.5f;

    // Ball variables
    public float ballSpeed = 4;
    private bool ballFired = false;

    //***************************************************************************
    void Start()
    {
        // Assign ball transform & rigidbody variables
        trBall = this.transform;
        rbBall = this.GetComponent<Rigidbody>();

        // Assign Paddle transform variable
        trPaddle = paddle.transform;
    }

    //***************************************************************************
    // Reset ball position in reference to paddle location
    public void ResetBall()
    {
        //Reset ball fired state
        ballFired = false;

        // Initial Velocity equals 0
        rbBall.velocity = new Vector3(0, 0, 0).normalized;
    }

    //***************************************************************************
    private void FixedUpdate()
    {
        // If ball not fired (game not on)
        // then ball tracks paddle position
        if (ballFired == false)
        {
            // Set initial ball position
            trBall.localPosition = new Vector3(trPaddle.localPosition.x, (trBall.localScale.y / 2), trPaddle.localPosition.z + paddleOffSet);

            // add small pentaly for not firing ball
            paddle.GetComponent<WallPongPaddleAgent>().AddReward(-0.01f);
        }
    }

    //***************************************************************************
    // Fire ball to start moving
    public void FireBall()
    {
        if (ballFired == false)
        {
            // Calculate initial direction, make length=1 via .normalized
            Vector3 velocityDirection = new Vector3(Random.Range(-1.0f, 1.0f), 0, 1).normalized;

            // Initial Velocity
            rbBall.velocity = velocityDirection * ballSpeed;

            //Set ball fired
            ballFired = true;
        }
    }

    //***************************************************************************
    // Process Paddle collision
    void OnCollisionEnter(Collision col)
    {
        ///////////////////////////////////////
        // Hit the paddle
        if (col.gameObject.name == "Paddle")
        {
            // Calculate hit Factor as defined in below function
            float x = hitFactor(trBall.localPosition,
                col.transform.localPosition,
                col.collider.bounds.size.x);

            // Calculate direction, make length=1 via .normalized
            Vector3 velocityDirection = new Vector3(x, 0, 1).normalized;

            // Set Velocity with dir * speed
            rbBall.velocity = velocityDirection * ballSpeed;
        }
    }

    //***************************************************************************
    // If ball hit paddle, set new ball direction
    float hitFactor(Vector3 ballPos, Vector3 paddlePos, float paddleSize)
    {
        // ||  1 <- at the right of the paddle
        // ||
        // ||  0 <- at the middle of the paddle
        // ||
        // || -1 <- at the left of the paddle
        return Mathf.Clamp(((ballPos.x - paddlePos.x) / paddleSize), -1.0f, 1.0f);
    }

}
