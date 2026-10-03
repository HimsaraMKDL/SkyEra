
using UnityEngine;
using UnityEngine.UI;
using SkyEra.Games.TwoDGame.Core;
using SkyEra.Games.TwoDGame.Gameplay;

namespace SkyEra.Games.TwoDGame.UI
{
    [DisallowMultipleComponent]
    public class BeginnerIntroController : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField]
        private GameSessionBootstrap sessionBootstrap;

        [SerializeField]
        private GameTimerController timerController;

        [Header("Navigation")]
        [SerializeField]
        private GameModeSelectionController
            modeSelectionController;

        [Header("UI References")]
        [SerializeField]
        private CanvasGroup introCanvasGroup;

        [SerializeField]
        private Button startButton;

        [SerializeField]
        private Button backButton;

        private bool hasStarted;

        private void Awake()
        {
            if (introCanvasGroup == null)
            {
                introCanvasGroup =
                    GetComponent<CanvasGroup>();
            }

            hasStarted = false;
            SetIntroVisible(true);
        }

        private void OnEnable()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(
                    HandleStartClicked);

                startButton.onClick.AddListener(
                    HandleStartClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(
                    HandleBackClicked);

                backButton.onClick.AddListener(
                    HandleBackClicked);
            }
        }

        private void OnDisable()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(
                    HandleStartClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(
                    HandleBackClicked);
            }
        }

        private void Start()
        {
            if (sessionBootstrap != null &&
                sessionBootstrap.HasActiveSession)
            {
                if (timerController != null)
                {
                    timerController.PauseTimer();
                }

                Debug.LogWarning(
                    "[BeginnerIntroController] A session " +
                    "loaded before Start was pressed. " +
                    "Disable Auto Start On Play on " +
                    "GameSessionBootstrap.",
                    this);
            }
        }

        private void HandleStartClicked()
        {
            if (hasStarted)
            {
                return;
            }

            if (sessionBootstrap == null)
            {
                Debug.LogError(
                    "[BeginnerIntroController] Session " +
                    "Bootstrap is not assigned.",
                    this);

                return;
            }

            if (timerController == null)
            {
                Debug.LogError(
                    "[BeginnerIntroController] Timer " +
                    "Controller is not assigned.",
                    this);

                return;
            }

            sessionBootstrap.LoadStartupSession();

            if (!sessionBootstrap.HasActiveSession)
            {
                Debug.LogError(
                    "[BeginnerIntroController] Startup " +
                    "session failed to load.",
                    this);

                return;
            }

            if (!timerController.UsesTimer ||
                !timerController.IsRunning)
            {
                Debug.LogError(
                    "[BeginnerIntroController] Beginner " +
                    "session loaded, but the timer " +
                    "did not start.",
                    this);

                return;
            }

            hasStarted = true;

            SetIntroVisible(false);

            Debug.Log(
                "[BeginnerIntroController] Beginner " +
                "gameplay started.",
                this);
        }

        private void HandleBackClicked()
        {
            // Back is available only before Start.
            // It returns to the temporary game-only
            // mode selector, not the kiosk scene.
            if (hasStarted)
            {
                return;
            }

            if (modeSelectionController == null)
            {
                Debug.LogError(
                    "[BeginnerIntroController] Mode " +
                    "Selection Controller is not assigned.",
                    this);

                return;
            }

            modeSelectionController.ReturnToModeSelection();

            Debug.Log(
                "[BeginnerIntroController] Returned " +
                "to the game mode selection screen.",
                this);
        }

        private void SetIntroVisible(bool visible)
        {
            if (introCanvasGroup == null)
            {
                Debug.LogError(
                    "[BeginnerIntroController] Intro " +
                    "Canvas Group is not assigned.",
                    this);

                return;
            }

            introCanvasGroup.alpha =
                visible ? 1f : 0f;

            introCanvasGroup.interactable =
                visible;

            introCanvasGroup.blocksRaycasts =
                visible;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (introCanvasGroup == null)
            {
                introCanvasGroup =
                    GetComponent<CanvasGroup>();
            }
        }
#endif
    }
}
