using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject welcomePanel;
    public GameObject instructionsPanel;
    public GameObject explorerPanel;


    [Header("Sky")]
    public StarRenderer starRenderer;


    private Coroutine explorerRefreshCoroutine;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        ShowWelcome();
    }


    // =========================================================
    // WELCOME
    // =========================================================

    public void ShowWelcome()
    {
        welcomePanel.SetActive(true);
        instructionsPanel.SetActive(false);
        explorerPanel.SetActive(false);
    }


    // =========================================================
    // INSTRUCTIONS
    // =========================================================

    public void ShowInstructions()
    {
        welcomePanel.SetActive(false);
        instructionsPanel.SetActive(true);
        explorerPanel.SetActive(false);
    }


    // =========================================================
    // EXPLORER
    // =========================================================

    public void ShowExplorer()
    {
        welcomePanel.SetActive(false);
        instructionsPanel.SetActive(false);
        explorerPanel.SetActive(true);


        if (explorerRefreshCoroutine != null)
        {
            StopCoroutine(
                explorerRefreshCoroutine
            );
        }


        explorerRefreshCoroutine =
            StartCoroutine(
                RefreshExplorerAfterActivation()
            );
    }


    // =========================================================
    // WAIT FOR UI THEN RENDER SKY
    // =========================================================

    private IEnumerator RefreshExplorerAfterActivation()
    {
        // Explorer was activated this frame.
        // Give Unity one frame to calculate RectTransforms.
        yield return null;


        Canvas.ForceUpdateCanvases();


        RectTransform explorerRect =
            explorerPanel.GetComponent<RectTransform>();


        if (explorerRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                explorerRect
            );
        }


        Canvas.ForceUpdateCanvases();


        // One more frame guarantees the final layout
        yield return new WaitForEndOfFrame();


        if (starRenderer == null)
        {
            Debug.LogError(
                "ScreenManager: StarRenderer reference is missing."
            );

            yield break;
        }


        Debug.Log(
            "ScreenManager: Explorer ready - rendering sky."
        );


        starRenderer.RefreshCurrentView();


        explorerRefreshCoroutine = null;
    }


    // =========================================================
    // BACK TO INSTRUCTIONS
    // =========================================================

    public void ShowInstructionsFromExplorer()
    {
        welcomePanel.SetActive(false);
        instructionsPanel.SetActive(true);
        explorerPanel.SetActive(false);
    }
}