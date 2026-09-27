using System;
using UnityEngine;

public class BaseMovements : MonoBehaviour
{
    //----------- Variables --------------//
    public float walkSpeed;
    public float runSpeed;
    public float airBorneSpeed;
    public float jumpForce;

    public static Rigidbody rb;
    
    public MovementStates currentState;
    
    private GroundDetector GroundDetector;
    
    //----------- Unity built in --------------//
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        GroundDetector = GetComponent<GroundDetector>();
    }
    private void FixedUpdate() 
    {
        Movement();
    }
    private void Update() 
    {
        Jump();
        UpdateMovementState();
    }
    
    //----------- Movement logic --------------//
    /// <summary>
    /// Function to execute base movement on a component.
    /// doesn't do it on IA controlled entity, only players
    ///
    /// don't manage complexe movements like slides, wall jumps, and other advanced mechanics.
    /// </summary>
    private void Movement()
    {
        float hrz = Input.GetAxisRaw("Horizontal");
        float vrt = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = transform.TransformDirection(new Vector3(hrz, 0f, vrt));

        Vector3 movement = inputDirection.normalized * CurrentMovementSpeed();
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
        
    }
    /// <summary>
    /// Executes the logic for the jump action of the player character.
    /// </summary>
    private void Jump()
    {
        if (GroundDetector.IsGrounded() && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
    }
    //----------- Movement state management --------------//
    /// <summary>
    /// Movement states for the player character.
    /// 
    /// To use to manage different movement variables like speed, damping etc...
    /// </summary>
    public enum MovementStates
    {
        Walk,
        Run,
        Airborne
    }
    /// <summary>
    /// Updates the movement state of the player character depending on its current situation
    /// </summary>
    /// <returns>
    /// The updated movement state of the player character.
    /// </returns>
    private MovementStates UpdateMovementState()
    {
        if (Input.GetKey(KeyCode.LeftShift) && GroundDetector.IsGrounded())
        {
            currentState = MovementStates.Run;
        }
        else if(!GroundDetector.IsGrounded())
        {
            currentState = MovementStates.Airborne;
        }
        else
        {
            currentState = MovementStates.Walk;
        }
        
        return currentState;
    }
    /// <summary>
    /// assign the speed value of the player depending on its current movement state
    /// </summary>
    /// <returns>
    /// current speed float value of the player
    /// </returns>
    public float CurrentMovementSpeed()
    {
        switch(currentState)
        {
            case MovementStates.Walk:
                return walkSpeed;
            case MovementStates.Run:
                return runSpeed;
            case MovementStates.Airborne:
                return airBorneSpeed;
            default:
                return 0f;
        }
    }
}