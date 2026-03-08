using UnityEngine;
using UnityEngine.InputSystem;

namespace Tutorial
{
    /// <summary>
    /// Moves the player discretely across a 3-column × 3-row grid.
    /// Left/Right keys change column; Up/Down keys change row.
    /// Position is smoothly snapped using MoveTowards — no physics involved.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class TutorialPlayerController : MonoBehaviour
    {
        [Header("Grid Positions")]
        [SerializeField] private float[] columnXPositions = { -6f, 0f, 6f };
        [SerializeField] private float[] rowYPositions    = { 0.55f, 3.05f, 5.55f };

        [Header("Snap")]
        [SerializeField] private float snapSpeed = 25f;

        private int _col = 1; // Start centre column.
        private int _row = 0; // Start bottom row.

        private Vector3 _targetPosition;

        private PlayerInput _playerInput;
        private InputAction _moveAction;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _moveAction  = _playerInput.actions["Move"];
        }

        private void OnEnable()
        {
            _moveAction.performed += HandleMovePerformed;
        }

        private void OnDisable()
        {
            _moveAction.performed -= HandleMovePerformed;
        }

        private void Start()
        {
            _targetPosition    = GridToWorld(_col, _row);
            transform.position = _targetPosition;
        }

        private void HandleMovePerformed(InputAction.CallbackContext ctx)
        {
            Vector2 dir = ctx.ReadValue<Vector2>();

            // Use the dominant axis to resolve diagonal inputs.
            if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            {
                if (dir.x >  0.5f) _col = Mathf.Min(_col + 1, columnXPositions.Length - 1);
                if (dir.x < -0.5f) _col = Mathf.Max(_col - 1, 0);
            }
            else
            {
                if (dir.y >  0.5f) _row = Mathf.Min(_row + 1, rowYPositions.Length - 1);
                if (dir.y < -0.5f) _row = Mathf.Max(_row - 1, 0);
            }

            _targetPosition = GridToWorld(_col, _row);
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetPosition,
                snapSpeed * Time.deltaTime);
        }

        private Vector3 GridToWorld(int col, int row)
        {
            float x = columnXPositions[Mathf.Clamp(col, 0, columnXPositions.Length - 1)];
            float y = rowYPositions   [Mathf.Clamp(row, 0, rowYPositions.Length    - 1)];
            return new Vector3(x, y, transform.position.z);
        }

        /// <summary>The player's current grid column (0 = left, 2 = right).</summary>
        public int CurrentColumn => _col;

        /// <summary>The player's current grid row (0 = bottom, 2 = top).</summary>
        public int CurrentRow => _row;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            foreach (float y in rowYPositions)
                foreach (float x in columnXPositions)
                    Gizmos.DrawWireCube(new Vector3(x, y, transform.position.z), Vector3.one * 0.6f);
        }
    }
}
