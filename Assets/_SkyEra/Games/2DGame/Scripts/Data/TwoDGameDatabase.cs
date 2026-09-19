using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkyEra.Games.TwoDGame.Data
{
    [CreateAssetMenu(
        fileName = "TwoDGameDatabase",
        menuName = "SkyEra/2D Game/2D Game Database",
        order = 3)]
    public class TwoDGameDatabase : ScriptableObject
    {
        [Header("Star Patterns")]
        [SerializeField]
        private List<StarPatternData> starPatterns =
            new List<StarPatternData>();

        [Header("Game Modes")]
        [SerializeField]
        private List<GameModeData> gameModes =
            new List<GameModeData>();

        [Header("Playable Sessions")]
        [SerializeField]
        private List<GameSessionData> sessions =
            new List<GameSessionData>();

        public IReadOnlyList<StarPatternData> StarPatterns => starPatterns;
        public IReadOnlyList<GameModeData> GameModes => gameModes;
        public IReadOnlyList<GameSessionData> Sessions => sessions;

        public int StarPatternCount =>
            starPatterns != null ? starPatterns.Count : 0;

        public int GameModeCount =>
            gameModes != null ? gameModes.Count : 0;

        public int SessionCount =>
            sessions != null ? sessions.Count : 0;

        public bool TryGetStarPattern(
            string patternId,
            out StarPatternData starPattern)
        {
            starPattern = null;

            if (string.IsNullOrWhiteSpace(patternId) ||
                starPatterns == null)
            {
                return false;
            }

            for (int i = 0; i < starPatterns.Count; i++)
            {
                StarPatternData candidate = starPatterns[i];

                if (candidate == null)
                {
                    continue;
                }

                if (string.Equals(
                        candidate.PatternId,
                        patternId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    starPattern = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetGameMode(
            string modeId,
            out GameModeData gameMode)
        {
            gameMode = null;

            if (string.IsNullOrWhiteSpace(modeId) ||
                gameModes == null)
            {
                return false;
            }

            for (int i = 0; i < gameModes.Count; i++)
            {
                GameModeData candidate = gameModes[i];

                if (candidate == null)
                {
                    continue;
                }

                if (string.Equals(
                        candidate.ModeId,
                        modeId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    gameMode = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetSession(
            string sessionId,
            out GameSessionData session)
        {
            session = null;

            if (string.IsNullOrWhiteSpace(sessionId) ||
                sessions == null)
            {
                return false;
            }

            for (int i = 0; i < sessions.Count; i++)
            {
                GameSessionData candidate = sessions[i];

                if (candidate == null)
                {
                    continue;
                }

                if (string.Equals(
                        candidate.SessionId,
                        sessionId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    session = candidate;
                    return true;
                }
            }

            return false;
        }

        public List<GameSessionData> GetSessionsForPattern(
            StarPatternData starPattern,
            bool availableOnly = true)
        {
            List<GameSessionData> results =
                new List<GameSessionData>();

            if (starPattern == null || sessions == null)
            {
                return results;
            }

            for (int i = 0; i < sessions.Count; i++)
            {
                GameSessionData session = sessions[i];

                if (session == null)
                {
                    continue;
                }

                if (session.StarPattern != starPattern)
                {
                    continue;
                }

                if (availableOnly && !session.Available)
                {
                    continue;
                }

                results.Add(session);
            }

            results.Sort(
                (first, second) =>
                    first.DisplayOrder.CompareTo(second.DisplayOrder));

            return results;
        }

        public List<GameSessionData> GetSessionsForMode(
            GameModeData gameMode,
            bool availableOnly = true)
        {
            List<GameSessionData> results =
                new List<GameSessionData>();

            if (gameMode == null || sessions == null)
            {
                return results;
            }

            for (int i = 0; i < sessions.Count; i++)
            {
                GameSessionData session = sessions[i];

                if (session == null)
                {
                    continue;
                }

                if (session.GameMode != gameMode)
                {
                    continue;
                }

                if (availableOnly && !session.Available)
                {
                    continue;
                }

                results.Add(session);
            }

            results.Sort(
                (first, second) =>
                    first.DisplayOrder.CompareTo(second.DisplayOrder));

            return results;
        }

        public bool ContainsSession(
            StarPatternData starPattern,
            GameModeData gameMode)
        {
            if (starPattern == null ||
                gameMode == null ||
                sessions == null)
            {
                return false;
            }

            for (int i = 0; i < sessions.Count; i++)
            {
                GameSessionData session = sessions[i];

                if (session == null)
                {
                    continue;
                }

                if (session.StarPattern == starPattern &&
                    session.GameMode == gameMode)
                {
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (starPatterns == null)
            {
                starPatterns = new List<StarPatternData>();
            }

            if (gameModes == null)
            {
                gameModes = new List<GameModeData>();
            }

            if (sessions == null)
            {
                sessions = new List<GameSessionData>();
            }
        }
#endif
    }
}