using TMPro;
using UnityEngine;
using Yarn.Unity;

/**
 * Show a countdown progress on the screen.
 */
[RequireComponent(typeof(TextMeshProUGUI))]
public class ETAIndicator : MonoBehaviour
{
    [SerializeField] string prefix = "ETA:";
    [SerializeField] int value;
    [SerializeField] string units;
    [SerializeField] int fluctuationAmount;
    [SerializeField] bool display = true;
    [SerializeField] float repeatRate = 2f;

    TextMeshProUGUI tmp;
    int displayedValue;

    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        if (display)
        {
            DisplayTime(true);
        }
    }

    [YarnCommand("displayTime")]
    public void DisplayTime(bool flag)
    {
        display = flag;
        if (flag)
        {
            InvokeRepeating("UpdateText", 0f, repeatRate);
        }
        else
        {
            CancelInvoke();
            UpdateText();
        }
        
    }

    [YarnCommand("setTime")]
    public void SetTime(int value, string units, int fluctuationAmount)
    {
        this.value = value;
        this.units = units;
        this.fluctuationAmount= fluctuationAmount;
    }

    void FluctuateValue()
    {
        displayedValue = value + Mathf.RoundToInt((Random.value - 0.5f) * 2f * fluctuationAmount);
    }

    void UpdateText()
    {
        FluctuateValue();
        if (tmp == null)
        {
            return;
        }
        if (display)
        {
            tmp.text = $"{prefix} {displayedValue} {units}";
        }
        else
        {
            tmp.text = "";
        }
    }

}
