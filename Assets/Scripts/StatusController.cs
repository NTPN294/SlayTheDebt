using UnityEngine;
using UnityEngine.UI;

public class StatusController : MonoBehaviour
{
    public Toggle lowToggle;
    public Toggle midToggle;
    public Toggle highToggle;
    public Toggle homeToggle;
    public Toggle studioToggle;
    public Toggle sharedToggle;
    public Toggle jobToggle;
    public Toggle loanToggle;

    public Slider sliderHappiness;
    public Slider sliderHealth;
    public Slider sliderCareer;
    public Slider sliderRelationship;

    [Header("Step (use multiples of 5)")]
    public int step = 5;

    void Start()
    {
        // Als er al een speler is (vanuit StartScene meegenomen), laad die stats direct.
        // Zo worden de keuzes uit StartScene altijd correct doorgegeven aan Phase1Scene.
        if (GameSession.Player != null)
        {
            LoadFromGameSession();
            return;
        }

        // Geen speler nog (we zijn in StartScene vóór de klik) — luister naar toggles.
        if (lowToggle != null)  lowToggle.onValueChanged.AddListener(_  => ApplySituation());
        if (midToggle != null)  midToggle.onValueChanged.AddListener(_  => ApplySituation());
        if (highToggle != null) highToggle.onValueChanged.AddListener(_ => ApplySituation());

        if (homeToggle != null)   homeToggle.onValueChanged.AddListener(_   => ApplySituation());
        if (studioToggle != null) studioToggle.onValueChanged.AddListener(_ => ApplySituation());
        if (sharedToggle != null) sharedToggle.onValueChanged.AddListener(_ => ApplySituation());

        if (jobToggle != null)  jobToggle.onValueChanged.AddListener(_  => ApplySituation());
        if (loanToggle != null) loanToggle.onValueChanged.AddListener(_ => ApplySituation());

        ApplySituation();
    }

    /// <summary>
    /// Laad stats direct uit GameSession.Player (Phase1Scene of na herstart).
    /// Wordt ook aangeroepen vanuit CardController na het spelen van een kaart.
    /// </summary>
    public void LoadFromGameSession()
    {
        if (GameSession.Player == null) return;

        int h  = Mathf.Clamp(GameSession.Player.Happiness,    0, 100);
        int he = Mathf.Clamp(GameSession.Player.Health,       0, 100);
        int c  = Mathf.Clamp(GameSession.Player.Career,       0, 100);
        int r  = Mathf.Clamp(GameSession.Player.Relationships, 0, 100);

        if (sliderHappiness    != null) sliderHappiness.SetValueWithoutNotify(h);
        if (sliderHealth       != null) sliderHealth.SetValueWithoutNotify(he);
        if (sliderCareer       != null) sliderCareer.SetValueWithoutNotify(c);
        if (sliderRelationship != null) sliderRelationship.SetValueWithoutNotify(r);
    }

    void ApplySituation()
    {
        // financial tier base: 0=low, 1=mid, 2=high
        int tier = 1;
        if      (highToggle != null && highToggle.isOn) tier = 2;
        else if (midToggle  != null && midToggle.isOn)  tier = 1;
        else                                             tier = 0;

        // living situation: 0=home, 1=studio, 2=shared
        int living = 0;
        if      (studioToggle != null && studioToggle.isOn) living = 1;
        else if (sharedToggle != null && sharedToggle.isOn) living = 2;

        int baseHappiness = 50, baseHealth = 50, baseCareer = 50, baseRelationship = 50;
        switch (tier)
        {
            case 0:
                baseHappiness = 20; baseHealth = 30; baseCareer = 10; baseRelationship = 40;
                break;
            case 1:
                baseHappiness = 50; baseHealth = 50; baseCareer = 50; baseRelationship = 50;
                break;
            case 2:
                baseHappiness = 80; baseHealth = 70; baseCareer = 80; baseRelationship = 60;
                break;
        }

        int modHappiness = 0, modHealth = 0, modCareer = 0, modRelationship = 0;
        switch (living)
        {
            case 0: modHappiness = 20; modHealth =  20; modRelationship =  10; break;
            case 1: modHappiness = 20; modHealth = -10; modRelationship = -10; break;
            case 2: modHappiness = -10; modHealth = -10; modRelationship = 10; break;
        }

        int jobModCareer     = (jobToggle  != null && jobToggle.isOn)  ? 30  : 0;
        int loanModHappiness = (loanToggle != null && loanToggle.isOn) ? -20 : 0;

        int finalHappiness    = Mathf.Clamp(RoundToStep(baseHappiness    + modHappiness    + loanModHappiness), 0, 100);
        int finalHealth       = Mathf.Clamp(RoundToStep(baseHealth       + modHealth),                          0, 100);
        int finalCareer       = Mathf.Clamp(RoundToStep(baseCareer       + modCareer       + jobModCareer),     0, 100);
        int finalRelationship = Mathf.Clamp(RoundToStep(baseRelationship + modRelationship),                    0, 100);

        if (sliderHappiness    != null) sliderHappiness.SetValueWithoutNotify(finalHappiness);
        if (sliderHealth       != null) sliderHealth.SetValueWithoutNotify(finalHealth);
        if (sliderCareer       != null) sliderCareer.SetValueWithoutNotify(finalCareer);
        if (sliderRelationship != null) sliderRelationship.SetValueWithoutNotify(finalRelationship);
    }

    int RoundToStep(int value)
    {
        if (step <= 1) return value;
        return Mathf.RoundToInt(value / (float)step) * step;
    }
}
