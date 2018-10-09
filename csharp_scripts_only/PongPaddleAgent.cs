using UnityEngine;
using MLAgents;

public class PongPaddleAgent : Agent
{
    // My Paddle (Agent) Variables
    public int playerNumber = 1;
    private Transform trMyPaddle;

    // Opponent Paddle (Other Agent) Variables
    public GameObject opponentPaddle;
    private Transform trOpPaddle;

    // Paddle Motion and Constraint Variables
    public float translateFactor = 0.02f;
    private float maxPaddlePosition;

    // Ball Variables
    public GameObject Ball;
    private Transform trBall;
    private Rigidbody rbBall;

    // Observation Normalization factors
    public Vector3 normPosFactor = new Vector3(5.0f, 1.0f, 3.5f);
    public float normVelFactor = 3.0f;

    // Inverse vactor normalizes position data as a function of paddle side
    // set as 1 for Paddle 1
    // set as -1 for Paddle 2
    public int inverseFactor = 1;

    //***************************************************************************
    void Start()
    {
        // Assign paddle transform variables
        trMyPaddle = this.transform;
        trOpPaddle = opponentPaddle.transform;

        // Assign ball transform and rigidbody variables
        trBall = Ball.transform;
        rbBall = Ball.GetComponent<Rigidbody>();

        // Set Paddle Max-Position Constraint
        // Equals width of plane arena (from center) minus the width (from center) of the paddle
        maxPaddlePosition = normPosFactor.z; // - (trMyPaddle.localScale.z / 2.0f);
    }


    //***************************************************************************
    public override void CollectObservations()
    {
        // Collect current ball x and z position and velocity
        AddVectorObs((trBall.localPosition.x * inverseFactor) / normPosFactor.x);
        AddVectorObs((trBall.localPosition.z) / normPosFactor.z);
        AddVectorObs((rbBall.velocity.x * inverseFactor) / normVelFactor);
        AddVectorObs((rbBall.velocity.z) / normVelFactor);

        // Collect opponent's current paddle (agent) z position
        AddVectorObs((trOpPaddle.localPosition.z) / normPosFactor.z);

        // Collect my current paddle (agent) z position
        AddVectorObs((trMyPaddle.localPosition.z) / normPosFactor.z);
    }


    //***************************************************************************
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // If this (my) Paddle missed the ball, trigger done and add rewards
        if ((trBall.localPosition.x * inverseFactor) < (trMyPaddle.localPosition.x * inverseFactor))
        {
            // This players lost (done); add a negative reward for this paddle (agent)
            Done();
            AddReward(-1.0f);

            // Other player won (done); add a positive reward for them.
            opponentPaddle.GetComponent<PongPaddleAgent>().Done();
            opponentPaddle.GetComponent<PongPaddleAgent>().AddReward(1.0f);

            //Reset ball position
            Ball.GetComponent<PongBallController>().ResetBall(playerNumber);
        }

        // Act (0=nothing, 1=move left, 2=move right) 
        int action = Mathf.FloorToInt(vectorAction[0]);
        Vector3 dirToGo = Vector3.zero;
        switch (action)
        {
            case 0:
                //Do nothing
                break;
            case 1:
                //Set translate direction to up
                dirToGo = new Vector3(0, 0, 1);
                break;
            case 2:
                //Set translate direction to down
                dirToGo = new Vector3(0, 0, -1);
                break;
            case 3:
                //Fire ball
                Ball.GetComponent<PongBallController>().FireBall(playerNumber);
                break;
        }

        // Update (move) Paddle position, clamping paddle position to max paddle position (arena bounds)
        Vector3 newPosition = trMyPaddle.localPosition + (dirToGo * translateFactor);
        newPosition.z = Mathf.Clamp(newPosition.z, -maxPaddlePosition, maxPaddlePosition);
        trMyPaddle.localPosition = newPosition;
    }


    //***************************************************************************
    void OnCollisionEnter(Collision col)
    {
        // If paddle (agent) hits ball, add small positive reward
        // NOTE: The higher you set this reward (e.g., above .1) the more likely 
        // the Paddles will learn to hit the ball back and forth in a cooperative manner.
        // For more competative game play, set this low (e.g., even euqal to 0), so that max reward 
        // results when a paddle wins. 
        if (col.gameObject.name == "Ball")
        {
            AddReward(0.005f);
        }
    }

    //***************************************************************************
    public override void AgentReset()
    {
        //Rest paddle (agent)to a random start location
        Vector3 resetPosition = trMyPaddle.localPosition;
        resetPosition.z = Random.Range(-maxPaddlePosition, maxPaddlePosition);
        trMyPaddle.localPosition = resetPosition;
    }

}
