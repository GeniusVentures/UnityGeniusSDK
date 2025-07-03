using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using TMPro;

public class GeniusSDKBalanceDisplay : MonoBehaviour
{

    [SerializeField] private string labelPrefix = "Minions: ";
    [SerializeField] private float updateInterval = 10f;

    private Text uiText;
    private TMP_Text tmpText;

    private void Awake()
    {
        uiText = GetComponent<Text>();
        tmpText = GetComponent<TMP_Text>();

        if (uiText == null && tmpText == null)
        {
            Debug.LogWarning("BalanceDisplay: No Text or TMP_Text component found on this GameObject.");
        }
    }

    private IEnumerator Start()
    {
        while (!GeniusSDKWrapper.Instance.IsReady)
            yield return null;

        ForceUpdateBalance();
        if (uiText != null || tmpText != null)
        {
            StartCoroutine(UpdateBalanceLoop());
        }
    }

    public void ForceUpdateBalance()
    {
        ulong balance = GeniusSDKWrapper.Instance.GetBalance(GeniusSDKWrapper.Instance.TokenID);
            if (uiText != null)
            {  
                uiText.text = labelPrefix + balance.ToString();
            }
            else if (tmpText != null)
            {
                tmpText.text = labelPrefix + balance.ToString();
            }
    }

    private IEnumerator UpdateBalanceLoop()
    {
        while (true)
        {
            UnityEngine.Debug.unityLogger.logEnabled = true;
            ulong balance = GeniusSDKWrapper.Instance.GetBalance(GeniusSDKWrapper.Instance.TokenID);
            UnityEngine.Debug.LogWarning($"Balance: {balance.ToString()}");
            if (uiText != null)
            {  
                uiText.text = labelPrefix + balance.ToString();
            }
            else if (tmpText != null)
            {
                tmpText.text = labelPrefix + balance.ToString();
            }

            yield return new WaitForSeconds(updateInterval);
            UnityEngine.Debug.unityLogger.logEnabled = false;
        }
    }
}
