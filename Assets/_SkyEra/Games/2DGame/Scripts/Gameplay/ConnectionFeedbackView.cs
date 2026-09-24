using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SkyEra.Games.TwoDGame.Gameplay;

namespace SkyEra.Games.TwoDGame.UI
{
    [DisallowMultipleComponent]
    public class ConnectionFeedbackView : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField]
        private StarConnectionGameController connectionController;

        [Header("UI References")]
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private Image feedbackIcon;

        [SerializeField]
        private TMP_Text feedbackText;

        [Header("Feedback Icons")]
        [SerializeField]
        private Sprite correctIcon;

        [SerializeField]
        private Sprite retryIcon;

        [Header("Feedback Text")]
        [SerializeField]
        private string correctMessage = "Correct!";

        [SerializeField]
        private string retryMessage = "Try Again";

        [Header("Feedback Colors")]
        [SerializeField]
        private Color correctTextColor =
            new Color(0.45f, 1.00f, 0.75f, 1.00f);

        [SerializeField]
        private Color retryTextColor =
            new Color(1.00f, 0.65f, 0.35f, 1.00f);

        [Header("Timing")]
        [Min(0.1f)]
        [SerializeField]
        private float displayDuration = 1.0f;

        private Coroutine hideCoroutine;

        private void OnEnable()
        {
            SubscribeToEvents();
            HideImmediately();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
        }

        private void SubscribeToEvents()
        {
            if (connectionController == null)
            {
                return;
            }

            connectionController.ValidConnectionCreated -=
                HandleValidConnection;

            connectionController.ValidConnectionCreated +=
                HandleValidConnection;

            connectionController.InvalidConnectionAttempted -=
                HandleInvalidConnection;

            connectionController.InvalidConnectionAttempted +=
                HandleInvalidConnection;
        }

        private void UnsubscribeFromEvents()
        {
            if (connectionController == null)
            {
                return;
            }

            connectionController.ValidConnectionCreated -=
                HandleValidConnection;

            connectionController.InvalidConnectionAttempted -=
                HandleInvalidConnection;
        }

        private void HandleValidConnection(
            StarNodeView fromStar,
            StarNodeView toStar)
        {
            ShowFeedback(
                correctIcon,
                correctMessage,
                correctTextColor);
        }

        private void HandleInvalidConnection(
            StarNodeView fromStar,
            StarNodeView toStar)
        {
            ShowFeedback(
                retryIcon,
                retryMessage,
                retryTextColor);
        }

        private void ShowFeedback(
            Sprite icon,
            string message,
            Color textColor)
        {
            if (canvasGroup == null)
            {
                Debug.LogError(
                    "[ConnectionFeedbackView] Canvas Group " +
                    "is not assigned.",
                    this);

                return;
            }

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }

            if (feedbackIcon != null)
            {
                feedbackIcon.sprite = icon;
                feedbackIcon.enabled = icon != null;
            }

            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = textColor;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            hideCoroutine =
                StartCoroutine(
                    HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return
                new WaitForSecondsRealtime(
                    displayDuration);

            HideImmediately();

            hideCoroutine = null;
        }

        public void HideImmediately()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            displayDuration =
                Mathf.Max(
                    0.1f,
                    displayDuration);

            if (canvasGroup == null)
            {
                canvasGroup =
                    GetComponent<CanvasGroup>();
            }
        }
#endif
    }
}