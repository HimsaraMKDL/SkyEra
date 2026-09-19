using System.Collections.Generic;
using SkyEra.Games.TwoDGame.Data;
using UnityEditor;
using UnityEngine;

namespace SkyEra.Games.TwoDGame.Editor
{
    public static class GameDataValidationMenu
    {
        private const string SelectedDatabaseMenuPath =
            "SkyEra/2D Game/Validate Selected Database";

        private const string AllDatabasesMenuPath =
            "SkyEra/2D Game/Validate All Databases";

        [MenuItem(SelectedDatabaseMenuPath)]
        private static void ValidateSelectedDatabase()
        {
            TwoDGameDatabase database =
                Selection.activeObject as TwoDGameDatabase;

            if (database == null)
            {
                Debug.LogWarning(
                    "Select a TwoDGameDatabase asset before running validation.");
                return;
            }

            ValidateAndReport(database);
        }

        [MenuItem(SelectedDatabaseMenuPath, true)]
        private static bool ValidateSelectedDatabaseMenu()
        {
            return Selection.activeObject is TwoDGameDatabase;
        }

        [MenuItem(AllDatabasesMenuPath)]
        private static void ValidateAllDatabases()
        {
            string[] databaseGuids =
                AssetDatabase.FindAssets("t:TwoDGameDatabase");

            if (databaseGuids.Length == 0)
            {
                Debug.LogWarning(
                    "No TwoDGameDatabase assets were found in the project.");
                return;
            }

            int totalErrorCount = 0;

            for (int i = 0; i < databaseGuids.Length; i++)
            {
                string assetPath =
                    AssetDatabase.GUIDToAssetPath(databaseGuids[i]);

                TwoDGameDatabase database =
                    AssetDatabase.LoadAssetAtPath<TwoDGameDatabase>(
                        assetPath);

                if (database == null)
                {
                    Debug.LogError(
                        $"Could not load TwoDGameDatabase at '{assetPath}'.");
                    totalErrorCount++;
                    continue;
                }

                totalErrorCount += ValidateAndReport(database);
            }

            if (totalErrorCount == 0)
            {
                Debug.Log(
                    $"2D Game data validation completed successfully. " +
                    $"{databaseGuids.Length} database asset(s) checked.");
            }
            else
            {
                Debug.LogError(
                    $"2D Game data validation completed with " +
                    $"{totalErrorCount} error(s).");
            }
        }

        private static int ValidateAndReport(
            TwoDGameDatabase database)
        {
            List<string> errors =
                GameDataValidator.ValidateDatabase(database);

            if (errors.Count == 0)
            {
                Debug.Log(
                    $"2D Game Database '{database.name}' is valid.",
                    database);

                return 0;
            }

            Debug.LogError(
                $"2D Game Database '{database.name}' contains " +
                $"{errors.Count} validation error(s):",
                database);

            for (int i = 0; i < errors.Count; i++)
            {
                Debug.LogError(
                    $"[{i + 1}] {errors[i]}",
                    database);
            }

            return errors.Count;
        }
    }
}