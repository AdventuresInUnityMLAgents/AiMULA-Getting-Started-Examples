using UnityEngine;
using MLAgents;

public class WallPongPaddleAgent : Agent
{
    // Paddle (Agent) Variables
    private Transform trPaddle;
    public float translateFactor = 0.1f;
    private float maxPaddlePosition;

    // Ball Variables
    public GameObject Ball;
    private Transform trBall;
    private Rigidbody rbBall;

    // Observation Normalization factors
    public Vector3 normPosFactor = new Vector3(5.0f, 1.0f, 3.5f);
    public float normVelFactor = 5.0f;

    //***************************************************************************
    void Start()
    {
        // Assign paddle transform variable
        trPaddle = this.transform;

        // Assign ball transform and rigidbody variables
        trBall = Ball.transform;
        rbBall = Ball.GetComponent<Rigidbody>();

        // Set Paddle Max-Position Constraint
        // Equals width of plane arena (from center) minus the width (from center) of the paddle
        maxPaddlePosition = normPosFactor.x - (trPaddle.localScale.x / 2);
    }


    //***************************************************************************
    public override void CollectObservations()
    {
        // Collect current ball x and z position and velocity
        AddVectorObs(trBall.localPosition.x / normPosFactor.x);
        AddVectorObs(trBall.localPosition.z / normPosFactor.z);
        AddVectorObs(rbBall.velocity.x / normVelFactor);
        AddVectorObs(rbBall.velocity.z / normVelFactor);

        // Collect current paddle (agent) x position
        AddVectorObs(trPaddle.localPosition.x / normPosFactor.x);
    }


    //***************************************************************************
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // If ball missed, add negative reward
        if (trBall.localPosition.z < trPaddle.localPosition.z)
        {
            Done();
            AddReward(-1.0f);
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
                //Set translate direction to left
                dirToGo = Vector3.left;
                break;
            case 2:
                //Set translate direction to right
                dirToGo = Vector3.right;
                break;
            case 3:
                //Fire ball
                Ball.GetComponent<WallPongBallController>().FireBall();
                break;
        }

        // Update (move) Paddle position, clamping paddle position to max paddle position (arena bounds)
        Vector3 newPosition = trPaddle.localPosition + (dirToGo * translateFactor);
        newPosition.x = Mathf.Clamp(newPosition.x, -maxPaddlePosition, maxPaddlePosition);
        trPaddle.localPosition = newPosition;
    }


    //***************************************************************************
    public override void AgentReset()
    {
        //Leave paddle (agent) where they are
        //Reset ball position and velocity
        Ball.GetComponent<WallPongBallController>().ResetBall();
    }


    //***************************************************************************
    void OnCollisionEnter(Collision col)
    {
        // If paddle (agent) hits ball, add positive reward
        if (col.gameObject.name == "Ball")
        {
            AddReward(1.0f);
        }
    }
}
