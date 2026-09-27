using System;
using UnityEngine;

public class BaseMovements : MonoBehaviour
{
    //----------- Variables --------------//
    public float walkSpeed;
    public float runSpeed;
    public float jumpForce;

    public static Rigidbody rb;
    
    public MovementStates currentState;

    
    //----------- Unity built in --------------//
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate() 
    {
        Movement();
    }
    private void Update() 
    {
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

        Vector3 inputDirection = new Vector3(hrz, 0f, vrt);

        Vector3 movement = inputDirection.normalized * CurrentMovementSpeed();
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
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
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentState = MovementStates.Run;
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
            default:
                return 0f;
        }
    }
}