
using UnityEngine;
using UnityEngine.UI;

namespace SkyEra.Games.TwoDGame.UI
{
    [DisallowMultipleComponent]
    public class GameModeSelectionController : MonoBehaviour
    {
        [Header("Screen References")]
        [SerializeField]
        private CanvasGroup modeSelectionCanvasGroup;

        [SerializeField]
        private CanvasGroup beginnerIntroCanvasGroup;

        [Header("Mode Buttons")]
        [SerializeField]
        private Button beginnerButton;

        [SerializeField]
        private Button intermediateButton;

        [SerializeField]
        private Button advancedButton;

        [Header("Introduction References")]
        [SerializeField]
        private Button introBackButton;

        private bool beginnerSelected;

        private void Awake()
        {
            beginnerSelected = false;
            ShowModeSelection();
        }

        private void OnEnable()
        {
            if (beginnerButton != null)
            {
                beginnerButton.onClick.RemoveListener(
                    HandleBeginnerClicked);

                beginnerButton.onClick.AddListener(
                    HandleBeginnerClicked);
            }
        }

        private void Start()
        {
            // Ensure Mode Selection is the first screen
            // after all other components have initialized.
            ShowModeSelection();
        }

        private void OnDisable()
        {
            if (beginnerButton != null)
            {
                beginnerButton.onClick.RemoveListener(
                    HandleBeginnerClicked);
            }
        }

        private void ShowModeSelection()
        {
            SetCanvasVisible(
                modeSelectionCanvasGroup,
                true);

            SetCanvasVisible(
                beginnerIntroCanvasGroup,
                false);

            if (beginnerButton != null)
            {
                beginnerButton.interactable = true;
            }

            if (intermediateButton != null)
            {
                intermediateButton.interactable = false;
            }

            if (advancedButton != null)
            {
                advancedButton.interactable = false;
            }

            if (introBackButton != null)
            {
                introBackButton.interactable = false;
            }
        }

        private void HandleBeginnerClicked()
        {
            if (beginnerSelected)
            {
                return;
            }

            if (modeSelectionCanvasGroup == null ||
                beginnerIntroCanvasGroup == null)
            {
                Debug.LogError(
                    "[GameModeSelectionController] " +
                    "Screen references are missing.",
                    this);

                return;
            }

            beginnerSelected = true;

            SetCanvasVisible(
                modeSelectionCanvasGroup,
                false);

            SetCanvasVisible(
                beginnerIntroCanvasGroup,
                true);

            // Enable Back now that the introduction
            // is the active screen.
            if (introBackButton != null)
            {
                introBackButton.interactable = true;
            }

            Debug.Log(
                "[GameModeSelectionController] " +
                "Beginner mode selected.",
                this);
        }

        public void ReturnToModeSelection()
        {
            if (!beginnerSelected)
            {
                return;
            }

            beginnerSelected = false;

            ShowModeSelection();

            Debug.Log(
                "[GameModeSelectionController] " +
                "Returned to Mode Selection.",
                this);
        }

        private static void SetCanvasVisible(
            CanvasGroup canvasGroup,
            bool visible)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha =
                visible ? 1f : 0f;

            canvasGroup.interactable =
                visible;

            canvasGroup.blocksRaycasts =
                visible;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (modeSelectionCanvasGroup == null)
            {
                modeSelectionCanvasGroup =
                    GetComponent<CanvasGroup>();
            }
        }
#endif
    }
}
