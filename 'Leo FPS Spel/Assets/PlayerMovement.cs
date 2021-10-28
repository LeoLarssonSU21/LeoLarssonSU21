using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerStats playerStats;

    public bool isRunning { get; private set; } = false;
    public bool isJumping { get; private set; } = false;
    public bool isGrounded { get; private set; } = false;
    public bool isCrouching { get; private set; } = false;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [System.NonSerialized] public Vector3 jumpVelocity = Vector3.zero;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        HandleMovement();
        HandleJumpInput();

        void HandleJumpInput()
        {
            bool isTryingToJump = Input.GetKeyDown(KeyCode.Space);

            if (isTryingToJump && isGrounded)
            {
                isJumping = true;
            }
            else
            {
                isJumping = false;
            }

            if (isGrounded && jumpVelocity.y < 0)
            {
                jumpVelocity.y = -2f;
            }

            if (isJumping)
            {
                jumpVelocity.y = Mathf.Sqrt(playerStats.jumpHeight * -2f * playerStats.gravity);
            }

            jumpVelocity.y += playerStats.gravity * Time.deltaTime * 2;

            characterController.Move(jumpVelocity * Time.deltaTime * 2);
        }

        void HandleMovement()
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            isRunning = Input.GetKey(KeyCode.LeftShift);
            isCrouching = Input.GetKey(KeyCode.LeftControl);

            if (isCrouching)
            {
                HandleCrouch();
            }
            else
            {
                HandleStand();
            }

            Vector3 move = Vector3.ClampMagnitude(transform.right * x + transform.forward * z, 1.0f);

            if (isCrouching)
            {
                characterController.Move(move * playerStats.crouchingMovementSpeed * Time.deltaTime);
            }
            else if (isRunning)
            {
                characterController.Move(move * playerStats.runningMovementSpeed * Time.deltaTime);
            }
            else
            {
                characterController.Move(move * playerStats.walkingMovementSpeed * Time.deltaTime);
            }
        }

        void HandleCrouch()
        {
            if (characterController.height > playerStats.crouchHeightY)
            {
                UpdateCharacterHeight(playerStats.crouchHeightY);

                if (characterController.height - 0.05f <= playerStats.crouchHeightY)
                {
                    characterController.height = playerStats.crouchHeightY;
                }
            }
        }

        void HandleStand()
        {
            if (characterController.height < playerStats.standingHeightY)
            {
                float lastHeight = characterController.height;

                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.up, out hit, playerStats.standingHeightY))
                {
                    if (hit.distance < playerStats.standingHeightY - playerStats.crouchHeightY)
                    {
                        UpdateCharacterHeight(playerStats.crouchHeightY + hit.distance);
                        return;
                    }
                    else
                    {
                        UpdateCharacterHeight(playerStats.standingHeightY);
                    }
                }
                else
                {
                    UpdateCharacterHeight(playerStats.standingHeightY);
                }

                UpdateCharacterHeight(playerStats.standingHeightY);

                if (characterController.height + 0.05f >= playerStats.standingHeightY)
                {
                    characterController.height = playerStats.standingHeightY;
                }

                transform.position += new Vector3(0, (characterController.height - lastHeight) / 2, 0);
            }
        }

        void UpdateCharacterHeight(float newHeight)
        {
            characterController.height = Mathf.Lerp(characterController.height, newHeight, playerStats.crouchSpeed * Time.deltaTime);
        }
    }
}