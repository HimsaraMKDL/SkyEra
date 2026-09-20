using System;
using UnityEngine;
using SkyEra.Games.TwoDGame.Core;
using SkyEra.Games.TwoDGame.Data;

namespace SkyEra.Games.TwoDGame.Gameplay
{
    [DisallowMultipleComponent]
    public class GameTimerController : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField] private GameSessionBootstrap sessionBootstrap;
        [SerializeField] private StarConnectionGameController connectionController;

        private GameModeData currentMode;
        private float remainingTime;
        private bool timerRunning;
        private bool timeExpired;

        public event Action<float> TimeChanged;
        public event Action TimeExpired;

        public float RemainingTime => remainingTime;

        public bool IsRunning => timerRunning;

        public bool HasExpired => timeExpired;

        public bool UsesTimer =>
            currentMode != null &&
            currentMode.UsesTimer;

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void Start()
        {
            if (sessionBootstrap != null &&
                sessionBootstrap.HasActiveSession)
            {
                InitializeTimer(
                    sessionBootstrap.CurrentSession);
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            if (!timerRunning ||
                timeExpired ||
                currentMode == null)
            {
                return;
            }

            remainingTime -= Time.unscaledDeltaTime;

            if (remainingTime <= 0f)
            {
                ExpireTimer();
                return;
            }

            TimeChanged?.Invoke(
                remainingTime);
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
            }
        }

        private void HandleSessionLoaded(
            GameSessionData session)
        {
            InitializeTimer(session);
        }

        private void HandlePatternCompleted()
        {
            if (!timerRunning)
            {
                return;
            }

            StopTimer();

            Debug.Log(
                $"[GameTimerController] Timer stopped at " +
                $"{remainingTime:0.00} seconds because the pattern " +
                "was completed.",
                this);
        }

        private void InitializeTimer(
            GameSessionData session)
        {
            if (session == null ||
                session.GameMode == null)
            {
                Debug.LogError(
                    "[GameTimerController] Cannot initialize timer " +
                    "because the session or GameModeData is null.",
                    this);

                StopTimer();
                return;
            }

            currentMode = session.GameMode;
            timeExpired = false;

            if (!currentMode.UsesTimer)
            {
                remainingTime = 0f;
                timerRunning = false;

                TimeChanged?.Invoke(
                    remainingTime);

                Debug.Log(
                    $"[GameTimerController] Mode " +
                    $"'{currentMode.DisplayName}' does not use a timer.",
                    this);

                return;
            }

            remainingTime =
                Mathf.Max(
                    0f,
                    currentMode.TimeLimitSeconds);

            timerRunning =
                remainingTime > 0f;

            TimeChanged?.Invoke(
                remainingTime);

            Debug.Log(
                $"[GameTimerController] Timer initialized to " +
                $"{remainingTime:0} seconds for mode " +
                $"'{currentMode.DisplayName}'.",
                this);
        }

        private void ExpireTimer()
        {
            remainingTime = 0f;
            timerRunning = false;
            timeExpired = true;

            TimeChanged?.Invoke(
                remainingTime);

            if (connectionController != null)
            {
                connectionController.EndGameplayByTimeout();
            }
            else
            {
                Debug.LogError(
                    "[GameTimerController] Cannot lock gameplay " +
                    "because Connection Controller is not assigned.",
                    this);
            }

            TimeExpired?.Invoke();

            Debug.Log(
                "[GameTimerController] Time expired.",
                this);
        }

        public void PauseTimer()
        {
            if (timeExpired ||
                !UsesTimer)
            {
                return;
            }

            timerRunning = false;
        }

        public void ResumeTimer()
        {
            if (timeExpired ||
                !UsesTimer ||
                remainingTime <= 0f)
            {
                return;
            }

            timerRunning = true;
        }

        public void StopTimer()
        {
            timerRunning = false;
        }

        public void ResetTimer()
        {
            if (currentMode == null)
            {
                remainingTime = 0f;
                timerRunning = false;
                timeExpired = false;

                TimeChanged?.Invoke(
                    remainingTime);

                return;
            }

            timeExpired = false;

            remainingTime =
                currentMode.UsesTimer
                    ? Mathf.Max(
                        0f,
                        currentMode.TimeLimitSeconds)
                    : 0f;

            timerRunning =
                currentMode.UsesTimer &&
                remainingTime > 0f;

            TimeChanged?.Invoke(
                remainingTime);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sessionBootstrap == null)
            {
                sessionBootstrap =
                    GetComponent<GameSessionBootstrap>();
            }

            if (connectionController == null)
            {
                connectionController =
                    GetComponent<StarConnectionGameController>();
            }
        }
#endif
    }
}