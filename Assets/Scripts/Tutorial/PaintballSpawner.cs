using UnityEngine;

namespace Tutorial
{
    /// <summary>
    /// Spawns paintballs above the screen in random columns so they fall downward.
    /// Each ball carries a column index used to determine which canvas cell to paint on hit.
    /// </summary>
    public class PaintballSpawner : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject paintballPrefab;

        [Header("Grid Reference — must match TutorialPlayerController")]
        [SerializeField] private float[] columnXPositions = { -6f, 0f, 6f };

        [Header("Spawn")]
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float spawnY        = 9f; // above visible screen top

        [Header("Colors")]
        [SerializeField] private Color[] availableColors = new[]
        {
            Color.red,
            new Color(1f, 0.5f, 0f),   // orange
            Color.yellow,
            Color.green,
            Color.blue,
            new Color(0.55f, 0f, 1f)   // purple
        };

        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= spawnInterval)
            {
                _timer = 0f;
                SpawnPaintball();
            }
        }

        private void SpawnPaintball()
        {
            if (paintballPrefab == null)
            {
                Debug.LogWarning("[PaintballSpawner] Paintball prefab not assigned.", this);
                return;
            }

            int col = Random.Range(0, columnXPositions.Length);
            Vector3 spawnPos = new Vector3(columnXPositions[col], spawnY, 0f);

            GameObject ball = Instantiate(paintballPrefab, spawnPos, Quaternion.identity);

            Paintball paintball = ball.GetComponent<Paintball>();
            if (paintball != null)
            {
                Color color = availableColors.Length > 0
                    ? availableColors[Random.Range(0, availableColors.Length)]
                    : Color.red;
                paintball.Initialize(col, color);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (columnXPositions == null) return;
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
            foreach (float x in columnXPositions)
                Gizmos.DrawWireSphere(new Vector3(x, spawnY, 0f), 0.35f);
        }
    }
}
