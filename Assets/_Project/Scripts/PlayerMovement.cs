using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerMovement : MonoBehaviour
{
    [Header("Forward Movement")]
    [SerializeField] private float forwardSpeed = 8f;

    [Header("Lane Movement")]
    [SerializeField] private float laneDistance = 2f;
    [SerializeField] private float laneChangeSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float groundY = 1.1f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpDuration = 0.6f;

    private int currentLane = 0;

    private bool isJumping;
    private float jumpTimer;

    private void Update()
    {
        HandleInput();
        MovePlayer();
    }

    private void HandleInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
        {
            currentLane--;
        }

        if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
        {
            currentLane++;
        }

        if (keyboard.spaceKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
        {
            TryStartJump();
        }

        currentLane = Mathf.Clamp(currentLane, -1, 1);
    }

    private void TryStartJump()
    {
        if (isJumping)
        {
            return;
        }

        isJumping = true;
        jumpTimer = 0f;
    }

    private void MovePlayer()
    {
        Vector3 currentPosition = transform.position;

        float targetX = currentLane * laneDistance;

        float newX = Mathf.MoveTowards(
            currentPosition.x,
            targetX,
            laneChangeSpeed * Time.deltaTime
        );

        float newY = CalculateYPosition();

        float newZ = currentPosition.z + forwardSpeed * Time.deltaTime;

        transform.position = new Vector3(
            newX,
            newY,
            newZ
        );
    }

    private float CalculateYPosition()
    {
        if (!isJumping)
        {
            return groundY;
        }

        jumpTimer += Time.deltaTime;

        float progress = jumpTimer / jumpDuration;

        if (progress >= 1f)
        {
            isJumping = false;
            jumpTimer = 0f;
            return groundY;
        }

        float jumpOffset = 4f * jumpHeight * progress * (1f - progress);

        return groundY + jumpOffset;
    }
    public void SetForwardSpeed(float speed)
    {
        forwardSpeed = Mathf.Max(0f, speed);
    }
}