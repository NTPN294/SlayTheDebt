using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoneySliderScript : MonoBehaviour
{
    public Slider slider;
    public TMP_Text valueText;

    private void OnEnable()
    {
        if (slider != null)
            slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDisable()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void Start()
    {
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 5000f;
            // initialize display
            UpdateDisplay(slider.value);
        }
    }

    private void OnSliderChanged(float rawValue)
    {
        // round to nearest hundred
        int rounded = Mathf.RoundToInt(rawValue / 100f) * 100;

        // update slider without invoking callbacks to avoid feedback loops
        if (slider != null)
            slider.SetValueWithoutNotify(rounded);

        UpdateDisplay(rounded);
    }

    private void UpdateDisplay(float value)
    {
        if (valueText == null) return;

        int intVal = Mathf.Clamp(Mathf.RoundToInt(value), 0, 5000);
        // show value like 4100, 2300 (hundreds)
        valueText.text = "€" + intVal.ToString();
    }
}
