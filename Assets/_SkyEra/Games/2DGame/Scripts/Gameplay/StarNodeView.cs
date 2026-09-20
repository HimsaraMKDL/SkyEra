using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SkyEra.Games.TwoDGame.Data;

namespace SkyEra.Games.TwoDGame.Gameplay
{
    public class StarNodeView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Button button;
        [SerializeField] private Image starImage;
        [SerializeField] private Image glowImage;
        [SerializeField] private TMP_Text labelText;

        [Header("Visual Settings")]
        [SerializeField] private float normalScale = 1f;
        [SerializeField] private float importantScale = 1.15f;
        [SerializeField] private float selectedScale = 1.2f;

        private StarNodeDefinition starDefinition;
        private bool selected;
        private bool interactable = true;

        public event Action<StarNodeView> Clicked;

        public string StarId =>
            starDefinition != null ? starDefinition.StarId : string.Empty;

        public string DisplayName =>
            starDefinition != null ? starDefinition.DisplayName : string.Empty;

        public Vector2 NormalizedPosition =>
            starDefinition != null
                ? starDefinition.NormalizedPosition
                : new Vector2(0.5f, 0.5f);

        public bool ImportantStar =>
            starDefinition != null && starDefinition.ImportantStar;

        public StarNodeDefinition Definition => starDefinition;

        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                {
                    rectTransform = transform as RectTransform;
                }

                return rectTransform;
            }
        }

        public bool IsSelected => selected;

        public bool IsInteractable => interactable;

        private void Awake()
        {
            CacheReferences();

            if (button != null)
            {
                button.onClick.RemoveListener(HandleButtonClicked);
                button.onClick.AddListener(HandleButtonClicked);
            }

            RefreshVisualState();
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleButtonClicked);
            }
        }

        public void Configure(
            StarNodeDefinition definition,
            bool showLabel,
            bool enableInteraction = true)
        {
            starDefinition = definition;
            selected = false;
            interactable = enableInteraction;

            CacheReferences();

            if (labelText != null)
            {
                labelText.text = definition != null
                    ? definition.DisplayName
                    : string.Empty;

                labelText.gameObject.SetActive(showLabel);
            }

            RefreshVisualState();
        }

        public void SetSelected(bool value)
        {
            selected = value;
            RefreshVisualState();
        }

        public void SetInteractable(bool value)
        {
            interactable = value;
            RefreshVisualState();
        }

        public void SetLabelVisible(bool visible)
        {
            if (labelText != null)
            {
                labelText.gameObject.SetActive(visible);
            }
        }

        public void SetGlowVisible(bool visible)
        {
            if (glowImage != null)
            {
                glowImage.gameObject.SetActive(visible);
            }
        }

        public void ResetState()
        {
            selected = false;
            RefreshVisualState();
        }

        private void HandleButtonClicked()
        {
            if (!interactable || starDefinition == null)
            {
                return;
            }

            Clicked?.Invoke(this);
        }

        private void RefreshVisualState()
        {
            if (button != null)
            {
                button.interactable = interactable;
            }

            float targetScale = normalScale;

            if (starDefinition != null && starDefinition.ImportantStar)
            {
                targetScale = importantScale;
            }

            if (selected)
            {
                targetScale = selectedScale;
            }

            if (RectTransform != null)
            {
                RectTransform.localScale =
                    new Vector3(targetScale, targetScale, 1f);
            }

            if (glowImage != null)
            {
                glowImage.gameObject.SetActive(selected);
            }

            if (starImage != null)
            {
                starImage.raycastTarget = false;
            }

            if (glowImage != null)
            {
                glowImage.raycastTarget = false;
            }

            if (labelText != null)
            {
                labelText.raycastTarget = false;
            }
        }

        private void CacheReferences()
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (starImage == null)
            {
                starImage = GetComponent<Image>();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            normalScale = Mathf.Max(0.01f, normalScale);
            importantScale = Mathf.Max(0.01f, importantScale);
            selectedScale = Mathf.Max(0.01f, selectedScale);

            CacheReferences();
        }
#endif
    }
}