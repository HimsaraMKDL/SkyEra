using TMPro;
using UnityEngine;
using SkyEra.Games.TwoDGame.Gameplay;

namespace SkyEra.Games.TwoDGame.UI
{
    [DisallowMultipleComponent]
    public class TimerDisplayView : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField] private GameTimerController timerController;

        [Header("UI References")]
        [SerializeField] private TMP_Text timerText;

        [Header("Display")]
        [SerializeField] private string timerFormat = "Time: {0:00}:{1:00}";

        private int lastDisplayedSeconds = -1;

        private void OnEnable()
        {
            SubscribeToEvents();
            RefreshCurrentTime();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (timerController == null)
            {
                return;
            }

            timerController.TimeChanged -= HandleTimeChanged;
            timerController.TimeChanged += HandleTimeChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (timerController == null)
            {
                return;
            }

            timerController.TimeChanged -= HandleTimeChanged;
        }

        private void HandleTimeChanged(float remainingTime)
        {
            RefreshTime(remainingTime);
        }

        public void RefreshCurrentTime()
        {
            if (timerController == null)
            {
                RefreshTime(0f);
                return;
            }

            RefreshTime(timerController.RemainingTime);
        }

        public void RefreshTime(float remainingTime)
        {
            int totalSeconds =
                Mathf.Max(
                    0,
                    Mathf.CeilToInt(remainingTime));

            if (totalSeconds == lastDisplayedSeconds)
            {
                return;
            }

            lastDisplayedSeconds = totalSeconds;

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            if (timerText != null)
            {
                timerText.text =
                    string.Format(
                        timerFormat,
                        minutes,
                        seconds);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (timerText == null)
            {
                timerText = GetComponent<TMP_Text>();
            }

            if (string.IsNullOrWhiteSpace(timerFormat))
            {
                timerFormat = "Time: {0:00}:{1:00}";
            }

            lastDisplayedSeconds = -1;
        }
#endif
    }
}