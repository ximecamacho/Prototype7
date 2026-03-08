using UnityEngine;

namespace Tutorial
{
    /// <summary>
    /// White canvas quad that accumulates paint splats using a CPU-side Texture2D.
    /// Using Texture2D (not RenderTexture + GL) so PaintCell is safe to call from
    /// physics callbacks such as OnTriggerEnter2D.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class PaintCanvas : MonoBehaviour
    {
        [Header("Canvas Resolution")]
        [SerializeField] private int canvasWidth  = 768;
        [SerializeField] private int canvasHeight = 320;

        [Header("Grid")]
        [SerializeField] private int   gridColumns       = 3;
        [SerializeField] private int   gridRows          = 3;
        [SerializeField] private Color gridLineColor     = new Color(0.15f, 0.15f, 0.15f, 1f);
        [SerializeField] private int   gridLineThickness = 5;

        [Header("Splat")]
        [SerializeField] private Texture2D splatBrush;
        /// <summary>Fraction of the cell's shorter dimension used for splat size.</summary>
        [SerializeField] [Range(0.4f, 1.0f)] private float splatCellFill = 0.88f;

        // CPU pixel buffer holding painted state (no grid lines).
        // Grid lines are composited on top before every GPU upload.
        private Color[]   _painted;
        private Texture2D _canvasTexture;
        private Renderer  _renderer;

        private static readonly int MainTexId = Shader.PropertyToID("_BaseMap");

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();

            _canvasTexture = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode   = TextureWrapMode.Clamp
            };

            ClearCanvas();

            _renderer.material.SetTexture(MainTexId, _canvasTexture);
        }

        private void OnDestroy()
        {
            if (_canvasTexture != null)
                Destroy(_canvasTexture);
        }

        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Draws the splat brush into the specified grid cell tinted with color.
        /// Safe to call from physics callbacks. col/row are 0-based, origin at bottom-left.
        /// </summary>
        public void PaintCell(int col, int row, Color color)
        {
            if (col < 0 || col >= gridColumns || row < 0 || row >= gridRows) return;

            float cellW   = canvasWidth  / (float)gridColumns;
            float cellH   = canvasHeight / (float)gridRows;
            int   splatPx = Mathf.RoundToInt(Mathf.Min(cellW, cellH) * splatCellFill);
            int   cx      = Mathf.RoundToInt((col + 0.5f) * cellW);
            int   cy      = Mathf.RoundToInt((row + 0.5f) * cellH);
            int   half    = splatPx / 2;

            int x0 = Mathf.Max(0, cx - half);
            int y0 = Mathf.Max(0, cy - half);
            int x1 = Mathf.Min(canvasWidth,  cx + half);
            int y1 = Mathf.Min(canvasHeight, cy + half);

            bool hasBrush = splatBrush != null && splatBrush.isReadable;

            for (int py = y0; py < y1; py++)
            {
                for (int px = x0; px < x1; px++)
                {
                    float alpha;
                    Color brushPixel;

                    if (hasBrush)
                    {
                        float u = (float)(px - (cx - half)) / splatPx;
                        float v = (float)(py - (cy - half)) / splatPx;
                        brushPixel = splatBrush.GetPixelBilinear(u, v);
                        alpha      = brushPixel.a;
                    }
                    else
                    {
                        // Solid-square fallback when no brush is assigned.
                        brushPixel = Color.white;
                        alpha      = 1f;
                    }

                    if (alpha < 0.02f) continue;

                    // White brush texel → pure tint color; grey texel → darker shade.
                    Color tinted = new Color(
                        color.r * brushPixel.r,
                        color.g * brushPixel.g,
                        color.b * brushPixel.b,
                        1f);

                    int idx     = py * canvasWidth + px;
                    _painted[idx] = Color.Lerp(_painted[idx], tinted, alpha);
                }
            }

            UploadToGPU();
        }

        /// <summary>Clears all paint splats and restores the white grid.</summary>
        public void ClearCanvas()
        {
            _painted = new Color[canvasWidth * canvasHeight];
            for (int i = 0; i < _painted.Length; i++)
                _painted[i] = Color.white;

            UploadToGPU();
        }

        // ─── Internal ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Composites grid lines on top of _painted and pushes the result to the GPU.
        /// </summary>
        private void UploadToGPU()
        {
            // Clone the painted buffer so grid lines don't corrupt the paint state.
            Color[] upload = (Color[])_painted.Clone();
            DrawGridLines(upload);
            _canvasTexture.SetPixels(upload);
            _canvasTexture.Apply(false);
        }

        private void DrawGridLines(Color[] pixels)
        {
            // Vertical column dividers.
            for (int c = 1; c < gridColumns; c++)
            {
                int x = Mathf.RoundToInt(c / (float)gridColumns * canvasWidth);
                FillRect(pixels, x - gridLineThickness / 2, 0, gridLineThickness, canvasHeight);
            }

            // Horizontal row dividers.
            for (int r = 1; r < gridRows; r++)
            {
                int y = Mathf.RoundToInt(r / (float)gridRows * canvasHeight);
                FillRect(pixels, 0, y - gridLineThickness / 2, canvasWidth, gridLineThickness);
            }

            // Outer border.
            FillRect(pixels, 0,                               0,                               canvasWidth,      gridLineThickness);
            FillRect(pixels, 0,                               canvasHeight - gridLineThickness, canvasWidth,      gridLineThickness);
            FillRect(pixels, 0,                               0,                               gridLineThickness, canvasHeight);
            FillRect(pixels, canvasWidth - gridLineThickness, 0,                               gridLineThickness, canvasHeight);
        }

        private void FillRect(Color[] pixels, int x, int y, int w, int h)
        {
            int x2 = Mathf.Min(canvasWidth,  x + w);
            int y2 = Mathf.Min(canvasHeight, y + h);
            x = Mathf.Max(0, x);
            y = Mathf.Max(0, y);
            for (int py = y; py < y2; py++)
                for (int px = x; px < x2; px++)
                    pixels[py * canvasWidth + px] = gridLineColor;
        }
    }
}
