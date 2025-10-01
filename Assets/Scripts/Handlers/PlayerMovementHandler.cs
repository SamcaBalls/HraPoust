using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

namespace SteamLobbyTutorial
{
    public class PlayerMovementHandler : NetworkBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference move;

        [Header("Movement Settings")]
        [SerializeField] private float normalSpeed = 5f;
        [SerializeField] private float sandSpeed = 2.5f;
        [SerializeField] private float sprintSpeed = 8f;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        [HideInInspector] public float moveSpeed = 0;

        [HideInInspector] public CharacterController controller;
        private bool isInSand = false;

        public override void OnStartLocalPlayer()
        {
            controller = GetComponent<CharacterController>();
            if (controller == null)
            {
                Debug.LogError("⚠️ Player prefab needs a CharacterController component!");
                return;
            }

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            move.action.Enable();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (!isOwned) return;

            // input
            Vector2 input = move.action.ReadValue<Vector2>();
            Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;

            // --- SPRINT ---
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);

            // rychlost podle terénu a sprintu
            if (isInSand)
                moveSpeed = sandSpeed; // sprint v písku ne?
            else
                moveSpeed = isSprinting ? sprintSpeed : normalSpeed;

            // pohyb
            Vector3 moveVector = transform.TransformDirection(direction) * moveSpeed;
            controller.SimpleMove(moveVector);

            // --- ANIMÁTOR ---
            if (animator != null)
            {
                animator.SetFloat("MoveX", input.x, 0.1f, Time.deltaTime);
                animator.SetFloat("MoveY", input.y, 0.1f, Time.deltaTime);
                animator.SetFloat("Speed", direction.magnitude * moveSpeed, 0.02f, Time.deltaTime);

                animator.SetBool("isRunning", isSprinting);
            }
        }

        // detekce kolize pro CharacterController
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider.CompareTag("Sand"))
                isInSand = true;
            else
                isInSand = false;
        }
    }
}
