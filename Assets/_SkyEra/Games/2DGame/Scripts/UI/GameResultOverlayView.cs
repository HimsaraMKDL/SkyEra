using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SkyEra.Games.TwoDGame.Core;
using SkyEra.Games.TwoDGame.Data;
using SkyEra.Games.TwoDGame.Gameplay;

namespace SkyEra.Games.TwoDGame.UI
{
    [DisallowMultipleComponent]
    public class GameResultOverlayView : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField]
        private GameSessionBootstrap sessionBootstrap;

        [SerializeField]
        private StarConnectionGameController connectionController;

        [SerializeField]
        private GameScoreController scoreController;

        [SerializeField]
        private GameTimerController timerController;

        [Header("UI References")]
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private Image resultIcon;

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text messageText;

        [SerializeField]
        private TMP_Text scoreText;

        [SerializeField]
        private TMP_Text timeText;

        [SerializeField]
        private Button retryButton;

        [Header("Result Icons")]
        [SerializeField]
        private Sprite completionIcon;

        [SerializeField]
        private Sprite timeoutIcon;

        [Header("Success Content")]
        [SerializeField]
        private string successTitle =
            "Constellation Complete!";

        [SerializeField]
        private Color successTitleColor =
            new Color(0.45f, 1.00f, 0.75f, 1.00f);

        [Header("Timeout Content")]
        [SerializeField]
        private string timeoutTitle =
            "Time's Up!";

        [SerializeField]
        private Color timeoutTitleColor =
            new Color(1.00f, 0.65f, 0.35f, 1.00f);

        private Coroutine showCoroutine;

        public bool IsVisible =>
            canvasGroup != null &&
            canvasGroup.alpha > 0.001f;

        private void OnEnable()
        {
            SubscribeToEvents();
            SubscribeToButtons();

            HideImmediately();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            UnsubscribeFromButtons();

            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
                showCoroutine = null;
            }
        }

        private void SubscribeToEvents()
        {
            if (sessionBootstrap != null)
            {
                sessionBootstrap.SessionLoaded -=
                    HandleSessionLoaded;

                sessionBootstrap.SessionLoaded +=
                    HandleSessionLoaded;
            }

            if (connectionController != null)
            {
                connectionController.PatternCompleted -=
                    HandlePatternCompleted;

                connectionController.PatternCompleted +=
                    HandlePatternCompleted;

                connectionController.GameplayTimedOut -=
                    HandleGameplayTimedOut;

                connectionController.GameplayTimedOut +=
                    HandleGameplayTimedOut;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (sessionBootstrap != null)
            {
                sessionBootstrap.SessionLoaded -=
                    HandleSessionLoaded;
            }

            if (connectionController != null)
            {
                connectionController.PatternCompleted -=
                    HandlePatternCompleted;

                connectionController.GameplayTimedOut -=
                    HandleGameplayTimedOut;
            }
        }

        private void SubscribeToButtons()
        {
            if (retryButton == null)
            {
                return;
            }

            retryButton.onClick.RemoveListener(
                HandleRetryClicked);

            retryButton.onClick.AddListener(
                HandleRetryClicked);
        }

        private void UnsubscribeFromButtons()
        {
            if (retryButton == null)
            {
                return;
            }

            retryButton.onClick.RemoveListener(
                HandleRetryClicked);
        }

        private void HandleSessionLoaded(
            GameSessionData session)
        {
            HideImmediately();
        }

        private void HandlePatternCompleted()
        {
            QueueResultDisplay(true);
        }

        private void HandleGameplayTimedOut()
        {
            QueueResultDisplay(false);
        }

        private void HandleRetryClicked()
        {
            if (sessionBootstrap == null)
            {
                Debug.LogError(
                    "[GameResultOverlayView] Cannot retry because " +
                    "Session Bootstrap is not assigned.",
                    this);

                return;
            }

            if (retryButton != null)
            {
                retryButton.interactable = false;
            }

            Debug.Log(
                "[GameResultOverlayView] Retrying current session.",
                this);

            sessionBootstrap.ReloadCurrentSession();
        }

        private void QueueResultDisplay(
            bool completedSuccessfully)
        {
            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
            }

            showCoroutine =
                StartCoroutine(
                    ShowResultNextFrame(
                        completedSuccessfully));
        }

        private IEnumerator ShowResultNextFrame(
            bool completedSuccessfully)
        {
            // Wait one frame so score and timer controllers can
            // finish processing the same gameplay event first.
            yield return null;

            if (completedSuccessfully)
            {
                ShowSuccessResult();
            }
            else
            {
                ShowTimeoutResult();
            }

            showCoroutine = null;
        }

        private void ShowSuccessResult()
        {
            string patternName =
                GetCurrentPatternName();

            if (resultIcon != null)
            {
                resultIcon.sprite = completionIcon;
                resultIcon.enabled =
                    completionIcon != null;
            }

            if (titleText != null)
            {
                titleText.text = successTitle;
                titleText.color =
                    successTitleColor;
            }

            if (messageText != null)
            {
                messageText.text =
                    $"{patternName} traced successfully!";
            }

            RefreshScoreText();
            RefreshTimeText(true);

            SetVisible(true);
        }

        private void ShowTimeoutResult()
        {
            string patternName =
                GetCurrentPatternName();

            int completedConnections =
                connectionController != null
                    ? connectionController
                        .CompletedConnectionCount
                    : 0;

            int totalConnections =
                GetCurrentConnectionCount();

            if (resultIcon != null)
            {
                resultIcon.sprite = timeoutIcon;
                resultIcon.enabled =
                    timeoutIcon != null;
            }

            if (titleText != null)
            {
                titleText.text = timeoutTitle;
                titleText.color =
                    timeoutTitleColor;
            }

            if (messageText != null)
            {
                messageText.text =
                    $"{patternName}: " +
                    $"{completedConnections}/" +
                    $"{totalConnections} connections completed.";
            }

            RefreshScoreText();
            RefreshTimeText(false);

            SetVisible(true);
        }

        private void RefreshScoreText()
        {
            if (scoreText == null)
            {
                return;
            }

            int score =
                scoreController != null
                    ? scoreController.CurrentScore
                    : 0;

            scoreText.text =
                $"Score: {score}";
        }

        private void RefreshTimeText(
            bool completedSuccessfully)
        {
            if (timeText == null)
            {
                return;
            }

            if (!completedSuccessfully)
            {
                timeText.text = "Time: 00:00";
                return;
            }

            float remainingTime =
                timerController != null
                    ? timerController.RemainingTime
                    : 0f;

            timeText.text =
                $"Time Left: {FormatTime(remainingTime)}";
        }

        private string GetCurrentPatternName()
        {
            if (sessionBootstrap == null ||
                sessionBootstrap.CurrentSession == null ||
                sessionBootstrap.CurrentSession.StarPattern == null)
            {
                return "Constellation";
            }

            return sessionBootstrap
                .CurrentSession
                .StarPattern
                .DisplayName;
        }

        private int GetCurrentConnectionCount()
        {
            if (sessionBootstrap == null ||
                sessionBootstrap.CurrentSession == null ||
                sessionBootstrap.CurrentSession.StarPattern == null)
            {
                return 0;
            }

            return sessionBootstrap
                .CurrentSession
                .StarPattern
                .ConnectionCount;
        }

        private static string FormatTime(
            float remainingTime)
        {
            int totalSeconds =
                Mathf.Max(
                    0,
                    Mathf.CeilToInt(
                        remainingTime));

            int minutes =
                totalSeconds / 60;

            int seconds =
                totalSeconds % 60;

            return
                $"{minutes:00}:{seconds:00}";
        }

        private void SetVisible(
            bool visible)
        {
            if (canvasGroup == null)
            {
                Debug.LogError(
                    "[GameResultOverlayView] Canvas Group " +
                    "is not assigned.",
                    this);

                return;
            }

            canvasGroup.alpha =
                visible ? 1f : 0f;

            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;

            if (retryButton != null)
            {
                retryButton.interactable = visible;
            }
        }

        public void HideImmediately()
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (retryButton != null)
            {
                retryButton.interactable = false;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (canvasGroup == null)
            {
                canvasGroup =
                    GetComponent<CanvasGroup>();
            }
        }
#endif
    }
}