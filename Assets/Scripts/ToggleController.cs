using UnityEngine;
using UnityEngine.UI;

public class MoneyToggleController : MonoBehaviour
{
    public Toggle lowToggle;
    public Toggle medToggle;
    public Toggle highToggle;

    private bool isChangingToggles = false;

    private void OnEnable()
    {
        lowToggle.onValueChanged.AddListener(OnLowToggled);
        medToggle.onValueChanged.AddListener(OnMedToggled);
        highToggle.onValueChanged.AddListener(OnHighToggled);
    }

    private void OnDisable()
    {
        lowToggle.onValueChanged.RemoveListener(OnLowToggled);
        medToggle.onValueChanged.RemoveListener(OnMedToggled);
        highToggle.onValueChanged.RemoveListener(OnHighToggled);
    }

    private void OnLowToggled(bool isOn)
    {
        if (isChangingToggles) return;
        isChangingToggles = true;

        if (isOn)
        {
            medToggle.isOn = false;
            highToggle.isOn = false;
        }
        else
        {
            lowToggle.isOn = true;
        }

        isChangingToggles = false;
    }

    private void OnMedToggled(bool isOn)
    {
        if (isChangingToggles) return;
        isChangingToggles = true;

        if (isOn)
        {
            lowToggle.isOn = false;
            highToggle.isOn = false;
        }
        else
        {
            medToggle.isOn = true;
        }

        isChangingToggles = false;
    }

    private void OnHighToggled(bool isOn)
    {
        if (isChangingToggles) return;
        isChangingToggles = true;

        if (isOn)
        {
            lowToggle.isOn = false;
            medToggle.isOn = false;
        }
        else
        {
            highToggle.isOn = true;
        }

        isChangingToggles = false;
    }
}

