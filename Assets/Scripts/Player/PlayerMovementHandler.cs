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
        public float normalSpeed = 5f;
        [SerializeField] private float sandSpeed = 2.5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float sandSprintSpeed = 4.5f;

        [Header("Animation")]
        // Ujisti se, že tato reference vede na Animator
        [SerializeField] private Animator animator;
        [SerializeField] private PauseMenu pauseMenu;
        [SerializeField] private PlayerStats playerStats;

        [HideInInspector] public float moveSpeed = 0;
        [HideInInspector] public CharacterController controller;
        private bool isInSand = false;

        private void OnEnable() => move.action.Enable();
        private void OnDisable() => move.action.Disable();

        public override void OnStartLocalPlayer()
        {
            controller = GetComponent<CharacterController>();
            if (controller == null)
                Debug.LogError("⚠️ Player prefab needs a CharacterController component!");

            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            // Pouze lokální hráč zpracovává input, hýbe se a nastavuje parametry Animatoru!
            if (!isLocalPlayer) return;

            Vector2 input = move.action.ReadValue<Vector2>();
            Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);

            moveSpeed = isInSand ? (isSprinting ? sandSprintSpeed : sandSpeed)
                                 : (isSprinting ? sprintSpeed : normalSpeed);

            Vector3 moveVector = transform.TransformDirection(direction) * moveSpeed;

            // Poznámka: CharacterController.SimpleMove() aplikuje gravitaci, což je super.
            controller.SimpleMove(moveVector);

            // --- Lokální aktualizace animátoru ---
            // NetworkAnimator automaticky synchronizuje tyto změny na všech vzdálených klientech.
            if (animator != null)
            {
                animator.SetFloat("MoveX", input.x, 0.1f, Time.deltaTime);
                animator.SetFloat("MoveY", input.y, 0.1f, Time.deltaTime);
                // "Speed" je dobrý pro blend tree pro chůzi/běh, pokud je potřeba.
                animator.SetFloat("Speed", direction.magnitude * moveSpeed, 0.02f, Time.deltaTime);
                animator.SetBool("isRunning", isSprinting);
            }
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Toto je v pořádku, ale dej si pozor, že se volá pouze na lokálním klientovi.
            // Pokud by 'isInSand' mělo ovlivňovat logiku, kterou vidí ostatní, 
            // muselo by se to synchronizovat přes [SyncVar].
            isInSand = hit.collider.CompareTag("Sand");
        }
    }
}