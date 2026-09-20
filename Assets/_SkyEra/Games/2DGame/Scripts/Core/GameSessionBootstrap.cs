using System;
using UnityEngine;
using SkyEra.Games.TwoDGame.Data;
using SkyEra.Games.TwoDGame.Gameplay;

namespace SkyEra.Games.TwoDGame.Core
{
    [DisallowMultipleComponent]
    public class GameSessionBootstrap : MonoBehaviour
    {
        [Header("Startup Session")]
        [SerializeField] private GameSessionData startupSession;
        [SerializeField] private bool autoStartOnPlay = true;

        [Header("Runtime References")]
        [SerializeField] private StarPatternView starPatternView;

        private GameSessionData currentSession;

        public event Action<GameSessionData> SessionLoaded;

        public GameSessionData StartupSession => startupSession;
        public GameSessionData CurrentSession => currentSession;

        public bool HasActiveSession => currentSession != null;

        private void Start()
        {
            if (autoStartOnPlay)
            {
                LoadStartupSession();
            }
        }

        public void LoadStartupSession()
        {
            if (startupSession == null)
            {
                Debug.LogError(
                    "[GameSessionBootstrap] Startup Session is not assigned.",
                    this);

                return;
            }

            LoadSession(startupSession);
        }

        public void LoadSession(GameSessionData session)
        {
            if (session == null)
            {
                Debug.LogError(
                    "[GameSessionBootstrap] Cannot load a null GameSessionData.",
                    this);

                return;
            }

            if (!session.IsConfigured)
            {
                Debug.LogError(
                    $"[GameSessionBootstrap] Session '{session.name}' " +
                    "is not fully configured.",
                    this);

                return;
            }

            if (!session.Available)
            {
                Debug.LogWarning(
                    $"[GameSessionBootstrap] Session '{session.DisplayName}' " +
                    "is currently unavailable.",
                    this);

                return;
            }

            if (starPatternView == null)
            {
                Debug.LogError(
                    "[GameSessionBootstrap] Star Pattern View is not assigned.",
                    this);

                return;
            }

            StarPatternData pattern = session.StarPattern;
            GameModeData mode = session.GameMode;

            currentSession = session;

            starPatternView.BuildPattern(
                pattern,
                mode.ShowStarLabels,
                true,
                mode.ShowGuideLines);

            Debug.Log(
                $"[GameSessionBootstrap] Loaded session " +
                $"'{session.DisplayName}' | " +
                $"Pattern: '{pattern.DisplayName}' | " +
                $"Mode: '{mode.DisplayName}' | " +
                $"Stars: {pattern.StarCount} | " +
                $"Guide Lines: {mode.ShowGuideLines}.",
                this);

            SessionLoaded?.Invoke(currentSession);
        }

        public void ClearSession()
        {
            if (starPatternView != null)
            {
                starPatternView.ClearPattern();
            }

            currentSession = null;
        }

        public void ReloadCurrentSession()
        {
            if (currentSession == null)
            {
                LoadStartupSession();
                return;
            }

            LoadSession(currentSession);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (startupSession != null && !startupSession.IsConfigured)
            {
                Debug.LogWarning(
                    $"[GameSessionBootstrap] Startup session " +
                    $"'{startupSession.name}' is not fully configured.",
                    this);
            }
        }
#endif
    }
}