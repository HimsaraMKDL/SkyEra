using System;
using System.Collections.Generic;
using UnityEngine;
using SkyEra.Games.TwoDGame.Data;

namespace SkyEra.Games.TwoDGame.Core
{
    [DisallowMultipleComponent]
    public class GameSessionProgressionController : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField]
        private GameSessionBootstrap sessionBootstrap;

        [Header("Ordered Session Sequence")]
        [SerializeField]
        private List<GameSessionData> sessions =
            new List<GameSessionData>();

        private int currentSessionIndex = -1;

        public event Action<GameSessionData, int> ProgressionSessionChanged;

        public event Action SessionSequenceCompleted;

        public IReadOnlyList<GameSessionData> Sessions =>
            sessions;

        public int SessionCount =>
            sessions.Count;

        public int CurrentSessionIndex =>
            currentSessionIndex;

        public bool HasValidCurrentSession =>
            currentSessionIndex >= 0 &&
            currentSessionIndex < sessions.Count;

        public bool HasNextSession =>
            HasValidCurrentSession &&
            currentSessionIndex + 1 < sessions.Count;

        public GameSessionData CurrentProgressionSession =>
            HasValidCurrentSession
                ? sessions[currentSessionIndex]
                : null;

        public GameSessionData NextSession =>
            HasNextSession
                ? sessions[currentSessionIndex + 1]
                : null;

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void Start()
        {
            if (sessionBootstrap != null &&
                sessionBootstrap.HasActiveSession)
            {
                SyncWithSession(
                    sessionBootstrap.CurrentSession);
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (sessionBootstrap == null)
            {
                return;
            }

            sessionBootstrap.SessionLoaded -=
                HandleSessionLoaded;

            sessionBootstrap.SessionLoaded +=
                HandleSessionLoaded;
        }

        private void UnsubscribeFromEvents()
        {
            if (sessionBootstrap == null)
            {
                return;
            }

            sessionBootstrap.SessionLoaded -=
                HandleSessionLoaded;
        }

        private void HandleSessionLoaded(
            GameSessionData session)
        {
            SyncWithSession(session);
        }

        private void SyncWithSession(
            GameSessionData session)
        {
            currentSessionIndex =
                FindSessionIndex(session);

            if (!HasValidCurrentSession)
            {
                Debug.LogWarning(
                    $"[GameSessionProgressionController] Loaded session " +
                    $"'{GetSessionName(session)}' is not present in the " +
                    "configured progression sequence.",
                    this);

                return;
            }

            ProgressionSessionChanged?.Invoke(
                CurrentProgressionSession,
                currentSessionIndex);

            Debug.Log(
                $"[GameSessionProgressionController] Progression synced " +
                $"to session {currentSessionIndex + 1}/{sessions.Count}: " +
                $"'{GetSessionName(CurrentProgressionSession)}'.",
                this);
        }

        public void LoadNextSession()
        {
            if (sessionBootstrap == null)
            {
                Debug.LogError(
                    "[GameSessionProgressionController] Session Bootstrap " +
                    "is not assigned.",
                    this);

                return;
            }

            if (!HasValidCurrentSession)
            {
                Debug.LogWarning(
                    "[GameSessionProgressionController] Cannot load next " +
                    "session because the current progression session is invalid.",
                    this);

                return;
            }

            if (!HasNextSession)
            {
                SessionSequenceCompleted?.Invoke();

                Debug.Log(
                    "[GameSessionProgressionController] Session sequence " +
                    "completed. There is no next session.",
                    this);

                return;
            }

            GameSessionData nextSession =
                sessions[currentSessionIndex + 1];

            if (nextSession == null)
            {
                Debug.LogError(
                    $"[GameSessionProgressionController] Session at index " +
                    $"{currentSessionIndex + 1} is null.",
                    this);

                return;
            }

            if (!nextSession.IsConfigured)
            {
                Debug.LogError(
                    $"[GameSessionProgressionController] Next session " +
                    $"'{nextSession.name}' is not fully configured.",
                    this);

                return;
            }

            if (!nextSession.Available)
            {
                Debug.LogWarning(
                    $"[GameSessionProgressionController] Next session " +
                    $"'{nextSession.DisplayName}' is unavailable.",
                    this);

                return;
            }

            sessionBootstrap.LoadSession(
                nextSession);
        }

        public void LoadFirstSession()
        {
            if (sessionBootstrap == null)
            {
                Debug.LogError(
                    "[GameSessionProgressionController] Session Bootstrap " +
                    "is not assigned.",
                    this);

                return;
            }

            if (sessions.Count == 0 ||
                sessions[0] == null)
            {
                Debug.LogWarning(
                    "[GameSessionProgressionController] No valid first " +
                    "session is configured.",
                    this);

                return;
            }

            sessionBootstrap.LoadSession(
                sessions[0]);
        }

        private int FindSessionIndex(
            GameSessionData session)
        {
            if (session == null)
            {
                return -1;
            }

            for (int i = 0;
                 i < sessions.Count;
                 i++)
            {
                if (sessions[i] == session)
                {
                    return i;
                }
            }

            return -1;
        }

        private static string GetSessionName(
            GameSessionData session)
        {
            if (session == null)
            {
                return "None";
            }

            if (!string.IsNullOrWhiteSpace(
                    session.DisplayName))
            {
                return session.DisplayName;
            }

            return session.name;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sessionBootstrap == null)
            {
                sessionBootstrap =
                    GetComponent<GameSessionBootstrap>();
            }
        }
#endif
    }
}