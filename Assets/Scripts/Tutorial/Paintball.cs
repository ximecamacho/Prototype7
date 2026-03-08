using UnityEngine;

namespace Tutorial
{
    /// <summary>
    /// A 2D paintball that falls straight down under gravity in its assigned column.
    /// Carries a column index and color so PlayerPaintballHitter knows which cell to paint.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class Paintball : MonoBehaviour
    {
        [SerializeField] private float gravityScale = 3f;
        [SerializeField] private float lifetime     = 6f;

        private Color _paintColor;
        private int   _column;

        private void Awake()
        {
            Rigidbody2D rb   = GetComponent<Rigidbody2D>();
            rb.gravityScale  = gravityScale;
            rb.linearDamping = 0f;
            // Freeze X so the ball drops straight down its column without drifting.
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

            // Trigger so the ball passes through platforms; only the player overlap matters.
            GetComponent<CircleCollider2D>().isTrigger = true;

            Destroy(gameObject, lifetime);
        }

        /// <summary>Called by PaintballSpawner immediately after Instantiate.</summary>
        public void Initialize(int column, Color color)
        {
            _column     = column;
            _paintColor = color;

            Renderer rend = GetComponentInChildren<Renderer>();
            if (rend != null)
                rend.material.color = color;
        }

        /// <summary>Grid column this ball occupies (0 = left, 1 = centre, 2 = right).</summary>
        public int Column => _column;

        /// <summary>Paint color carried by this ball.</summary>
        public Color PaintColor => _paintColor;
    }
}
