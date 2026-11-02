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

    [Header("unsorted")]
    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 input;
    private bool isGrounded;
    
    
    public Vector3 Velocity => velocity;
    public bool IsGrounded => isGrounded;

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

    private void ReadInput()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void Tick(float dt)
    {
        isGrounded = controller.isGrounded;

        ApplyGravity(dt);

        controller.Move(velocity * dt);
        
    }

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
