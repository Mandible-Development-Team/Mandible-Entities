using UnityEngine;
using TMPro;
using System.Collections;

namespace Mandible.Entities
{
    public class DamageNumber : MonoBehaviour
    {
        public enum HorizontalDirectionMode
        {
            Left,
            Right,
            Random,     // any float between -1 and 1
            RandomInt,  // -1 or 1 only
            Custom      // set manually at runtime
        }

        [Header("Damage Settings")]
        public float damage = 0f;
        public Color color = Color.white;
        public float fontSize = 36f;

        [Header("Display Settings")]
        public TMP_FontAsset font;
        public float floatDistance = 2f;      // vertical float distance
        public float horizontalDistance = 1f; // max horizontal float
        public float lifetime = 1f;           // how long it stays
        public float floatSpeed = 1f;         // speed multiplier for float
        public float startScale = 1.5f;       // initial pop scale
        public float endScale = 0.8f;         // final shrink scale
        public HorizontalDirectionMode horizontalMode = HorizontalDirectionMode.Random;

        [Tooltip("Used if horizontalMode is Custom.")]
        public float customHorizontalDirection = 0f; 

        private TextMeshProUGUI tmp;
        private Vector3 startPos;
        private float elapsed = 0f;
        private float horizontalDirection;     // final horizontal multiplier

        void Awake()
        {
            tmp = GetComponent<TextMeshProUGUI>();
            if(tmp == null) tmp = gameObject.AddComponent<TextMeshProUGUI>();
        }

        void Start()
        {
            if(tmp == null) return;
            
            tmp.text = damage.ToString("F0");
            tmp.fontSize = fontSize;
            tmp.color = color;
            if (font != null) tmp.font = font;

            tmp.alignment = TextAlignmentOptions.Center;
            startPos = transform.position;
        
            transform.localScale = Vector3.one * startScale;

            SetHorizontalDirection();
        }

        public void SetHorizontalDirection()
        {
            switch (horizontalMode)
            {
                case HorizontalDirectionMode.Left:
                    horizontalDirection = -1f;
                    break;
                case HorizontalDirectionMode.Right:
                    horizontalDirection = 1f;
                    break;
                case HorizontalDirectionMode.Random:
                    horizontalDirection = Random.Range(-1f, 1f);
                    break;
                case HorizontalDirectionMode.RandomInt:
                    horizontalDirection = Random.value < 0.5f ? -1f : 1f;
                    break;
                case HorizontalDirectionMode.Custom:
                    horizontalDirection = customHorizontalDirection;
                    break;
            }
        }

        void Update()
        {
            elapsed += Time.deltaTime * floatSpeed;
            float t = Mathf.Clamp01(elapsed / lifetime);

            // Smooth vertical float
            float smoothY = Mathf.SmoothStep(0f, floatDistance, t);

            // Smooth horizontal float
            float smoothX = Mathf.SmoothStep(0f, horizontalDistance, t) * horizontalDirection;

            transform.position = startPos + new Vector3(smoothX, smoothY, 0f);

            // Smooth fade out
            float alpha = Mathf.SmoothStep(1f, 0f, t);
            tmp.color = new Color(color.r, color.g, color.b, alpha);

            // Pop / Scale effect
            float scale = Mathf.SmoothStep(startScale, endScale, t);
            transform.localScale = Vector3.one * scale;

            // Destroy after lifetime
            if (elapsed >= lifetime)
                Destroy(gameObject);
        }
    }
}