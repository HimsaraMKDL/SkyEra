using UnityEngine;
using UnityEngine.UI;

namespace SkyEra.Games.TwoDGame.Gameplay
{
    [DisallowMultipleComponent]
    public class ConnectionLineView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image lineImage;

        [Header("Line Settings")]
        [Min(1f)]
        [SerializeField] private float lineThickness = 5f;

        private StarNodeView fromStar;
        private StarNodeView toStar;

        public StarNodeView FromStar => fromStar;
        public StarNodeView ToStar => toStar;

        public string FromStarId =>
            fromStar != null ? fromStar.StarId : string.Empty;

        public string ToStarId =>
            toStar != null ? toStar.StarId : string.Empty;

        private void Awake()
        {
            CacheReferences();
        }

        public void Configure(
            StarNodeView from,
            StarNodeView to)
        {
            fromStar = from;
            toStar = to;

            CacheReferences();
            RefreshLine();
        }

        public void RefreshLine()
        {
            if (fromStar == null ||
                toStar == null ||
                RectTransform == null)
            {
                return;
            }

            RectTransform parentRect =
                RectTransform.parent as RectTransform;

            if (parentRect == null)
            {
                return;
            }

            Vector2 fromPosition =
                GetLocalPosition(parentRect, fromStar.RectTransform);

            Vector2 toPosition =
                GetLocalPosition(parentRect, toStar.RectTransform);

            Vector2 direction = toPosition - fromPosition;
            float distance = direction.magnitude;

            Vector2 midpoint =
                (fromPosition + toPosition) * 0.5f;

            float angle =
                Mathf.Atan2(direction.y, direction.x) *
                Mathf.Rad2Deg;

            RectTransform.anchorMin =
                new Vector2(0.5f, 0.5f);

            RectTransform.anchorMax =
                new Vector2(0.5f, 0.5f);

            RectTransform.pivot =
                new Vector2(0.5f, 0.5f);

            RectTransform.anchoredPosition = midpoint;

            RectTransform.sizeDelta =
                new Vector2(distance, lineThickness);

            RectTransform.localRotation =
                Quaternion.Euler(0f, 0f, angle);

            RectTransform.localScale = Vector3.one;
        }

        public void SetThickness(float thickness)
        {
            lineThickness = Mathf.Max(1f, thickness);
            RefreshLine();
        }

        public void SetColor(Color color)
        {
            if (lineImage != null)
            {
                lineImage.color = color;
            }
        }

        private static Vector2 GetLocalPosition(
            RectTransform parentRect,
            RectTransform targetRect)
        {
            if (targetRect == null)
            {
                return Vector2.zero;
            }

            Vector3 worldPosition = targetRect.position;

            Vector3 localPosition =
                parentRect.InverseTransformPoint(worldPosition);

            return new Vector2(
                localPosition.x,
                localPosition.y);
        }

        private void CacheReferences()
        {
            if (rectTransform == null)
            {
                rectTransform =
                    transform as RectTransform;
            }

            if (lineImage == null)
            {
                lineImage = GetComponent<Image>();
            }

            if (lineImage != null)
            {
                lineImage.raycastTarget = false;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            lineThickness =
                Mathf.Max(1f, lineThickness);

            CacheReferences();
        }
#endif

        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                {
                    rectTransform =
                        transform as RectTransform;
                }

                return rectTransform;
            }
        }
    }
}