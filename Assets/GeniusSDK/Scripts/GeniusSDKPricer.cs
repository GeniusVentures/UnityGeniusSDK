using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using TMPro;

public class GeniusSDKPricer : MonoBehaviour
{
    [SerializeField] private float updateInterval = 60f; // How often to update price display
    [SerializeField] public float basePriceUSD = 1.0f;  // What the item should cost in USD
    [SerializeField] private string labelPrefix = ""; // e.g. "Price: "

    [HideInInspector]
    public double lastPrice = 0;
    private Text uiText;
    private TMP_Text tmpText;


    private void Awake()
    {
        Debug.unityLogger.logEnabled = true;
        uiText = GetComponent<Text>();
        tmpText = GetComponent<TMP_Text>();

        if (uiText == null && tmpText == null)
        {
            Debug.LogWarning("GeniusSDKPricer: No Text or TMP_Text component found on this GameObject.");
        }
        Debug.unityLogger.logEnabled = false;
    }


    private IEnumerator Start()
    {
        while (!GeniusSDKWrapper.Instance.IsReady)
            yield return null;
        ForceUpdatePrice();
        //if (priceText != null)
        //{
        StartCoroutine(UpdatePriceLoop());
        //}
    }

    public void ForceUpdatePrice() 
    {
        UpdatePriceDisplay();
    }

    private IEnumerator UpdatePriceLoop()
    {
        while (true)
        {
            UpdatePriceDisplay();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void UpdatePriceDisplay()
    {
        Debug.unityLogger.logEnabled = true;
        double gnusPriceUSD = GeniusSDKWrapper.Instance.GetGNUSPrice();
        if (gnusPriceUSD <= 0 && lastPrice <= 0)
        {
            if (uiText != null)
            {
                uiText.text = labelPrefix + "N/A";
            }
            else if (tmpText != null)
            {
                tmpText.text = labelPrefix + "N/A";
            }
            return;
        }
        double minions = 0;
        ulong roundedMinions = 0;
        if (gnusPriceUSD <= 0 && lastPrice > 0)
        {
            minions = (basePriceUSD / lastPrice) * 1_000_000;
            roundedMinions = (ulong)Mathf.Ceil((float)minions);
            if (uiText != null)
            {
                uiText.text = labelPrefix + roundedMinions.ToString("N0");
            }
            else if (tmpText != null)
            {
                tmpText.text = labelPrefix + roundedMinions.ToString("N0");
            }
            return;
        }
        lastPrice = gnusPriceUSD;

        minions = (basePriceUSD / gnusPriceUSD) * 1_000_000; 
        roundedMinions = (ulong)Mathf.Ceil((float)minions);

        if (uiText != null)
        {
            uiText.text = labelPrefix + roundedMinions.ToString("N0");
        }
        else if (tmpText != null)
        {
            tmpText.text = labelPrefix + roundedMinions.ToString("N0");
        }
        Debug.unityLogger.logEnabled = false;
    }
}
