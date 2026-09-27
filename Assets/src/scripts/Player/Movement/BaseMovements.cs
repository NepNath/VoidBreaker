using System;
using UnityEngine;

public class BaseMovements : MonoBehaviour
{
    public float walkSpeed;
    public float runSpeed;
    public float jumpForce;

    private Rigidbody rb;
    
    public enum MovementStates
    {
        Walk,
        Run,
        Airborne
    }

    public MovementStates currentState;

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

    private void Movement()
    {
        float hrz = Input.GetAxisRaw("Horizontal");
        float vrt = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(hrz, 0f, vrt);

        Vector3 movement = inputDirection.normalized * CurrentMovementSpeed();
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
    }

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