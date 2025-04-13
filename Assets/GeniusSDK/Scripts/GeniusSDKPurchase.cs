using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

//public enum RewardType
//{
//    Gems,
//    Coins,
//    UltraStarShip,
//    UltraDrone,
//    OneTimeOffer
//}
[Serializable]
public class PurchaseSuccessEvent : UnityEvent { }

public class GeniusSDKPurchase : MonoBehaviour
{
    private Button button;
    private bool isProcessing = false;

    //[SerializeField] private int currencyAmount = 100;
    //[SerializeField] private ulong tokenCost = 10;
    //[UnityEngine.SerializeField, Sirenix.OdinInspector.DrawWithUnity]
    //private RewardType rewardType = RewardType.Gems;
    [SerializeField]
    private PurchaseSuccessEvent onPurchaseSuccess;
    [SerializeField] private GeniusSDKPricer pricer;

    private void Awake()
    {
        Debug.unityLogger.logEnabled = true;
        Debug.Log("GeniusSDKPurchase Awake");
        button = GetComponent<Button>();
        Debug.unityLogger.logEnabled = false;
    }

    private void Start()
    {
        Debug.unityLogger.logEnabled = true;
        if (button != null)
        {
            button.onClick.AddListener(HandlePurchase);
            Debug.Log("Button listener added");
        }
        else
        {
            Debug.LogWarning("Button not found on this GameObject");
        }
        Debug.unityLogger.logEnabled = false;
    }

    private void HandlePurchase()
    {
        Debug.unityLogger.logEnabled = true;
        if (pricer == null || pricer.lastPrice <= 0)
        {
            Debug.LogWarning("Cannot calculate price: Pricer not set or GNUS price unavailable.");
            return;
        }

        if (isProcessing)
        {
            Debug.LogWarning("Purchase already in progress — ignoring duplicate click");
            Debug.unityLogger.logEnabled = false;
            return;
        }

        isProcessing = true;
        Debug.Log("HandlePurchase clicked");
        ulong minionCost = (ulong)Mathf.Ceil((float)((pricer.basePriceUSD / pricer.lastPrice) * 1_000_000));

        bool success = GeniusSDKWrapper.Instance.PayDev(minionCost);

        if (success)
        {
            Debug.Log($"Purchase successful — awarded");

            onPurchaseSuccess?.Invoke();
            // Refresh all balance displays
            var balanceDisplays = FindObjectsOfType<GeniusSDKBalanceDisplay>();
            foreach (var display in balanceDisplays)
            {
                display.ForceUpdateBalance();
            }
        }
        else
        {
            Debug.LogError("Purchase failed");
        }

        // Reset the flag (or delay it if you want a cooldown)
        isProcessing = false;
        Debug.unityLogger.logEnabled = false;
    }
}
