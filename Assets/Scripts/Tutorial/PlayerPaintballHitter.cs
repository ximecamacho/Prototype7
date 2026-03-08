using UnityEngine;

namespace Tutorial
{
    /// <summary>
    /// Detects when a falling Paintball's 2D trigger overlaps the player.
    /// Paints the canvas cell at (ball.Column, player.CurrentRow) and destroys the ball.
    /// </summary>
    public class PlayerPaintballHitter : MonoBehaviour
    {
        [SerializeField] private PaintCanvas paintCanvas;

        private TutorialPlayerController _controller;

        private void Awake()
        {
            _controller = GetComponent<TutorialPlayerController>();

            if (paintCanvas == null)
                paintCanvas = FindFirstObjectByType<PaintCanvas>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Paintball ball = other.GetComponent<Paintball>();
            if (ball == null) return;

            if (paintCanvas != null)
                paintCanvas.PaintCell(ball.Column, _controller.CurrentRow, ball.PaintColor);

            Destroy(other.gameObject);
        }
    }
}
