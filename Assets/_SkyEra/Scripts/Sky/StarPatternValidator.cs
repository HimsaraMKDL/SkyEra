using System.Collections.Generic;
using UnityEngine;

public class StarPatternValidator : MonoBehaviour
{
    [Header("References")]
    public StarDatabase starDatabase;
    public ConstellationData[] constellations;

    [Header("Settings")]
    public int modernDatasetIndex = 2;

    private void Start()
    {
        Validate();
    }

    [ContextMenu("Validate Star Patterns")]
    public void Validate()
    {
        if (starDatabase == null ||
            starDatabase.datasets == null ||
            modernDatasetIndex < 0 ||
            modernDatasetIndex >= starDatabase.datasets.Length)
        {
            Debug.LogError(
                "Validator: Star Database / dataset is invalid."
            );

            return;
        }

        StarDataset dataset =
            starDatabase.datasets[modernDatasetIndex];

        if (dataset == null ||
            dataset.stars == null)
        {
            Debug.LogError(
                "Validator: Modern dataset is empty."
            );

            return;
        }


        // =====================================================
        // DATABASE STAR NAMES
        // =====================================================

        HashSet<string> databaseNames =
            new HashSet<string>(
                System.StringComparer.OrdinalIgnoreCase
            );

        HashSet<string> duplicates =
            new HashSet<string>(
                System.StringComparer.OrdinalIgnoreCase
            );


        foreach (StarData star in dataset.stars)
        {
            if (star == null)
                continue;

            string name =
                StarRenderer.NormalizeStarName(
                    star.starName
                );

            if (string.IsNullOrEmpty(name))
                continue;


            if (!databaseNames.Add(name))
            {
                duplicates.Add(name);
            }
        }


        // =====================================================
        // CONSTELLATION REQUIRED NAMES
        // =====================================================

        HashSet<string> requiredNames =
            new HashSet<string>(
                System.StringComparer.OrdinalIgnoreCase
            );


        if (constellations != null)
        {
            foreach (
                ConstellationData constellation
                in constellations
            )
            {
                if (constellation == null ||
                    constellation.starNames == null)
                {
                    continue;
                }


                foreach (
                    string starName
                    in constellation.starNames
                )
                {
                    string cleanName =
                        StarRenderer.NormalizeStarName(
                            starName
                        );


                    if (!string.IsNullOrEmpty(cleanName))
                    {
                        requiredNames.Add(
                            cleanName
                        );
                    }
                }
            }
        }


        // =====================================================
        // REPORT
        // =====================================================

        Debug.Log(
            "VALIDATOR | Database entries = " +
            dataset.stars.Length +
            " | Unique names = " +
            databaseNames.Count +
            " | Required by patterns = " +
            requiredNames.Count
        );


        foreach (string duplicate in duplicates)
        {
            Debug.LogWarning(
                "DUPLICATE STAR: " +
                duplicate
            );
        }


        int missingCount = 0;


        foreach (string required in requiredNames)
        {
            if (!databaseNames.Contains(required))
            {
                Debug.LogError(
                    "MISSING STAR: " +
                    required
                );

                missingCount++;
            }
        }


        if (duplicates.Count == 0 &&
            missingCount == 0)
        {
            Debug.Log(
                "VALIDATOR: All constellation stars are present and no duplicate names were found."
            );
        }
        else
        {
            Debug.Log(
                "VALIDATOR COMPLETE | Missing = " +
                missingCount +
                " | Duplicates = " +
                duplicates.Count
            );
        }
    }
}