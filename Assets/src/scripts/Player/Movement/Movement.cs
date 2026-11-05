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

    [Header("Jump")]
    public float jumpForce;
    public float jumpBufferTime;
    
    private float jumpBufferTimer;
    
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
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Speed : {HorizontalSpeed:F2} m/s");
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y > 0.7f)
        {
            return;
        }

        if (Vector3.Dot(velocity, hit.normal) < 0f)
        {
            velocity = Vector3.ProjectOnPlane(velocity, hit.normal);
        }
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
            return;
        }

        float control = Mathf.Max(speed, stopSpeed);
        float drop = control * damping * dt;
        float newSpeed = Mathf.Max(speed - drop, 0f);

        horizontal *= newSpeed / speed;
        velocity.x = horizontal.x;
        velocity.z = horizontal.z;
    }
    private void Acccelerate(Vector3 dir, float speed, float accel, float dt)
    {
        float currentSpeed = Vector3.Dot(velocity, dir);
        float addSpeed = speed - currentSpeed;

        if (addSpeed <= 0f)
        {
            return;
        }

        float accelSpeed = Math.Min(accel * speed * dt, addSpeed);
        velocity += dir * accelSpeed;
    }
    private void Jump()
    {
        velocity.y = jumpForce;
        jumpBufferTimer = 0f;
    }
    
    // ------ Game Logic ------ //
    private void Tick(float dt)
    {
        isGrounded = controller.isGrounded;
        Vector3 dir = getDirection();
        bool isJumping = jumpBufferTimer > 0f;
        
        if (isGrounded)
        {
            if (!isJumping)
            {
                ApplyFriction(dt);
            }
            ApplyFriction(dt);
            Acccelerate(dir, walkSpeed, groundAcceleration, dt);
            if (isJumping)
            {
                Jump();
            }
        }

     

        jumpBufferTimer = Mathf.Max(jumpBufferTimer - dt, 0f);
        ApplyGravity(dt);
        controller.Move(velocity * dt);
        
    }
    private void ReadInput()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime;
        }
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
