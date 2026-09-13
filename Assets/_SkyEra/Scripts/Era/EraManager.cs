using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EraManager : MonoBehaviour
{
    public Slider eraSlider;

    public TMP_Text eraTitle;
    public TMP_Text eraYear;
    public TMP_Text eraDescription;

    public EraData[] eras;

    // Aluth reference eka
    public StarRenderer starRenderer;

    private void Start()
    {
        eraSlider.onValueChanged.AddListener(UpdateEra);

        UpdateEra(0);
    }

    public void UpdateEra(float value)
    {
        int index = Mathf.RoundToInt(value);

        if (index >= eras.Length)
            index = eras.Length - 1;

        EraData currentEra = eras[index];

        // StarRenderer eka update kirima
        if (starRenderer != null)
        {
            starRenderer.ChangeEra(index);
        }

        eraTitle.text = currentEra.eraName;
        eraYear.text = currentEra.year;
        eraDescription.text = currentEra.description;
    }
}