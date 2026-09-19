using System;
using System.Collections.Generic;

namespace SkyEra.Games.TwoDGame.Data
{
    public static class GameDataValidator
    {
        public static List<string> ValidateDatabase(
            TwoDGameDatabase database)
        {
            List<string> errors = new List<string>();

            if (database == null)
            {
                errors.Add("2D Game Database reference is null.");
                return errors;
            }

            ValidateStarPatterns(database, errors);
            ValidateGameModes(database, errors);
            ValidateSessions(database, errors);

            return errors;
        }

        public static List<string> ValidateStarPattern(
            StarPatternData pattern)
        {
            List<string> errors = new List<string>();

            if (pattern == null)
            {
                errors.Add("Star Pattern reference is null.");
                return errors;
            }

            ValidateSingleStarPattern(pattern, errors);

            return errors;
        }

        public static List<string> ValidateGameMode(
            GameModeData gameMode)
        {
            List<string> errors = new List<string>();

            if (gameMode == null)
            {
                errors.Add("Game Mode reference is null.");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(gameMode.ModeId))
            {
                errors.Add(
                    $"Game Mode '{gameMode.name}' has an empty Mode ID.");
            }

            if (string.IsNullOrWhiteSpace(gameMode.DisplayName))
            {
                errors.Add(
                    $"Game Mode '{gameMode.name}' has an empty Display Name.");
            }

            return errors;
        }

        public static List<string> ValidateSession(
            GameSessionData session)
        {
            List<string> errors = new List<string>();

            if (session == null)
            {
                errors.Add("Game Session reference is null.");
                return errors;
            }

            ValidateSingleSession(session, errors);

            return errors;
        }

        private static void ValidateStarPatterns(
            TwoDGameDatabase database,
            List<string> errors)
        {
            HashSet<string> patternIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < database.StarPatternCount; i++)
            {
                StarPatternData pattern =
                    database.StarPatterns[i];

                if (pattern == null)
                {
                    errors.Add(
                        $"Star Patterns entry {i} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(pattern.PatternId))
                {
                    errors.Add(
                        $"Star Pattern '{pattern.name}' has an empty Pattern ID.");
                }
                else if (!patternIds.Add(pattern.PatternId))
                {
                    errors.Add(
                        $"Duplicate Pattern ID found: '{pattern.PatternId}'.");
                }

                ValidateSingleStarPattern(pattern, errors);
            }
        }

        private static void ValidateSingleStarPattern(
            StarPatternData pattern,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(pattern.DisplayName))
            {
                errors.Add(
                    $"Star Pattern '{pattern.name}' has an empty Display Name.");
            }

            if (pattern.StarCount == 0)
            {
                errors.Add(
                    $"Star Pattern '{pattern.name}' contains no stars.");
            }

            HashSet<string> starIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < pattern.StarCount; i++)
            {
                StarNodeDefinition star =
                    pattern.Stars[i];

                if (star == null)
                {
                    errors.Add(
                        $"Star Pattern '{pattern.name}' has a null star at index {i}.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(star.StarId))
                {
                    errors.Add(
                        $"Star Pattern '{pattern.name}' has a star with an empty Star ID at index {i}.");
                    continue;
                }

                if (!starIds.Add(star.StarId))
                {
                    errors.Add(
                        $"Star Pattern '{pattern.name}' contains duplicate Star ID '{star.StarId}'.");
                }
            }

            HashSet<string> connectionKeys =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < pattern.ConnectionCount; i++)
            {
                StarConnectionDefinition connection =
                    pattern.Connections[i];

                if (connection == null)
                {
                    errors.Add(
                        $"Star Pattern '{pattern.name}' has a null connection at index {i}.");
                    continue;
                }

                string fromId = connection.FromStarId;
                string toId = connection.ToStarId;

                if (string.IsNullOrWhiteSpace(fromId) ||
                    string.IsNullOrWhiteSpace(toId))
                {
                    errors.Add(
                        $"Star Pattern '{pattern.name}' has a connection with an empty Star ID at index {i}.");
                    continue;
                }

                if (!pattern.ContainsStar(fromId))
                {
                    errors.Add(
                        $"Star Pattern '{pattern.name}' connection references missing Star ID '{fromId}'.");
                }

                if (!pattern.ContainsStar(toId))
                {
                    errors.Add(
                        $"Star Pattern '{pattern.name}' connection references missing Star ID '{toId}'.");
                }

                if (string.Equals(
                        fromId,
                        toId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        $"Star Pattern '{pattern.name}' contains a self-connection on Star ID '{fromId}'.");
                }

                string connectionKey =
                    CreateUndirectedConnectionKey(
                        fromId,
                        toId);

                if (!connectionKeys.Add(connectionKey))
                {
                    errors.Add(
                        $"Star Pattern '{pattern.name}' contains duplicate connection '{fromId}' <-> '{toId}'.");
                }
            }
        }

        private static void ValidateGameModes(
            TwoDGameDatabase database,
            List<string> errors)
        {
            HashSet<string> modeIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < database.GameModeCount; i++)
            {
                GameModeData mode =
                    database.GameModes[i];

                if (mode == null)
                {
                    errors.Add(
                        $"Game Modes entry {i} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(mode.ModeId))
                {
                    errors.Add(
                        $"Game Mode '{mode.name}' has an empty Mode ID.");
                }
                else if (!modeIds.Add(mode.ModeId))
                {
                    errors.Add(
                        $"Duplicate Mode ID found: '{mode.ModeId}'.");
                }

                if (string.IsNullOrWhiteSpace(mode.DisplayName))
                {
                    errors.Add(
                        $"Game Mode '{mode.name}' has an empty Display Name.");
                }
            }
        }

        private static void ValidateSessions(
            TwoDGameDatabase database,
            List<string> errors)
        {
            HashSet<string> sessionIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            HashSet<string> sessionCombinations =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < database.SessionCount; i++)
            {
                GameSessionData session =
                    database.Sessions[i];

                if (session == null)
                {
                    errors.Add(
                        $"Playable Sessions entry {i} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(session.SessionId))
                {
                    errors.Add(
                        $"Game Session '{session.name}' has an empty Session ID.");
                }
                else if (!sessionIds.Add(session.SessionId))
                {
                    errors.Add(
                        $"Duplicate Session ID found: '{session.SessionId}'.");
                }

                ValidateSingleSession(session, errors);

                if (session.StarPattern != null &&
                    session.GameMode != null)
                {
                    string combinationKey =
                        $"{session.StarPattern.PatternId}::{session.GameMode.ModeId}";

                    if (!sessionCombinations.Add(combinationKey))
                    {
                        errors.Add(
                            $"Duplicate session combination found for Pattern '{session.StarPattern.PatternId}' and Mode '{session.GameMode.ModeId}'.");
                    }
                }
            }
        }

        private static void ValidateSingleSession(
            GameSessionData session,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(session.DisplayName))
            {
                errors.Add(
                    $"Game Session '{session.name}' has an empty Display Name.");
            }

            if (session.StarPattern == null)
            {
                errors.Add(
                    $"Game Session '{session.name}' has no Star Pattern assigned.");
            }

            if (session.GameMode == null)
            {
                errors.Add(
                    $"Game Session '{session.name}' has no Game Mode assigned.");
            }
        }

        private static string CreateUndirectedConnectionKey(
            string firstId,
            string secondId)
        {
            if (string.Compare(
                    firstId,
                    secondId,
                    StringComparison.OrdinalIgnoreCase) <= 0)
            {
                return $"{firstId}::{secondId}";
            }

            return $"{secondId}::{firstId}";
        }
    }
}