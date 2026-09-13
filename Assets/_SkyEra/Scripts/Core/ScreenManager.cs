using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public GameObject welcomePanel;
    public GameObject instructionsPanel;
    public GameObject explorerPanel;


    private void Start()
    {
        ShowWelcome();
    }


    public void ShowWelcome()
    {
        welcomePanel.SetActive(true);
        instructionsPanel.SetActive(false);
        explorerPanel.SetActive(false);
    }


    public void ShowInstructions()
    {
        welcomePanel.SetActive(false);
        instructionsPanel.SetActive(true);
        explorerPanel.SetActive(false);
    }


    public void ShowExplorer()
    {
        welcomePanel.SetActive(false);
        instructionsPanel.SetActive(false);
        explorerPanel.SetActive(true);
    }


    public void ShowInstructionsFromExplorer()
    {
        welcomePanel.SetActive(false);
        instructionsPanel.SetActive(true);
        explorerPanel.SetActive(false);
    }
}