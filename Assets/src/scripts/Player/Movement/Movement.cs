using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    
    [Header("Gravity")]
    public float gravity = -100f;
    public float gravityFallMult;
    public float groundStickGravity = -2f;
    public float maxFallSpeed;

    [Header("Ground")] 
    public float walkSpeed;
    public float groundAcceleration;
    public float damping;
    public float stopSpeed;
    
    [Header("unsorted")]
    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 input;
    private bool isGrounded;
    
    
    public Vector3 Velocity => velocity;
    public float HorizontalSpeed => new Vector3(velocity.x, 0f, velocity.z).magnitude;
    public bool IsGrounded => isGrounded;

    // ------ built in Unity ------ //
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    private void Update()
    {
        ReadInput();
    }
    private void FixedUpdate()
    {
        Tick(Time.fixedDeltaTime);
    }

    // ------ Movements ------ //
    private void ApplyFriction(float dt)
    {
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        float speed = horizontal.magnitude;

        if (speed < .1f)
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }
    }
    
    // ------ Game Logic ------ //
    private void Tick(float dt)
    {
        isGrounded = controller.isGrounded;

        ApplyGravity(dt);

        controller.Move(velocity * dt);
        
    }
    private void ReadInput()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
    
    // ------ Getters ------ //
    private Vector3 getDirection()
    {
        Vector3 Direction = transform.right * input.x + transform.forward * input.y;
        Direction.y = 0f;
        return Direction.normalized;
    }
    
    // ------ Gravity ------ //
    private void ApplyGravity(float dt)
    {
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = groundStickGravity;
            return;
        }

        float currentGravity = gravity;

        if (velocity.y < 0f)
        {
            currentGravity *= gravityFallMult;
        }

        velocity.y += currentGravity * dt;
        velocity.y = Mathf.Max(velocity.y, maxFallSpeed);
    }
}
