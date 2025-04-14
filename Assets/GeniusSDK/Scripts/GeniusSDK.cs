using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using UnityEngine;
using System.Collections;

public class GeniusSDKWrapper : MonoBehaviour
{
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern IntPtr GeniusSDKInit(StringBuilder path, StringBuilder key, int autodht, int process, int baseport);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern void GeniusSDKShutdown();

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern bool GeniusSDKPayDev(ulong amount);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern double GeniusSDKGetGNUSPrice();

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern ulong GeniusSDKGetBalance();

    private bool isReady = false;
    [SerializeField] private string address = "0xcatcatcat";
    [SerializeField][Range(0f, 1f)] private float cut = 0.7f;
    [SerializeField] private float tokenValue = 1.0f;
    [SerializeField] private int tokenID = 1;


    private static GeniusSDKWrapper instance;
    public static GeniusSDKWrapper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("GeniusSDKWrapper").AddComponent<GeniusSDKWrapper>();
                DontDestroyOnLoad(instance.gameObject); // Persist across scene loads
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scene loads
            StartCoroutine(InitGeniusSDK());
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance
        }
    }

    private IEnumerator InitGeniusSDK()
    {
        Debug.Log("Initializing Genius SDK");
        // Define the path for persistent data
        StringBuilder pathBuilder = new StringBuilder(Application.persistentDataPath + "/", 1024);
        string destinationPath = Path.Combine(Application.persistentDataPath, "dev_config.json");
        //if (!File.Exists(destinationPath))
        //{
        Debug.Log("dev_config.json not found. Creating a new one...");
        // JSON data to write
        string jsonData = $@"{{
    ""Address"": ""{address}"",
    ""Cut"": ""{cut}"",
    ""TokenValue"": {tokenValue:F5},
    ""TokenID"": {tokenID},
    ""WriteDirectory"": """"
}}";
        //string jsonData = @"{
        //    ""Address"": ""0xcatcatcat"",
        //    ""Cut"": ""0.7"",
        //    ""TokenValue"": 1.0,
        //    ""TokenID"": 1,
        //    ""WriteDirectory"": """"
        //}";
        try
        {
            File.WriteAllText(destinationPath, jsonData);
            Debug.Log("dev_config.json created successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error writing dev_config.json: {ex.Message}");
            yield break; // Exit the coroutine if writing fails
        }
        //}
        //else
        //{
        //    Debug.Log("dev_config.json already exists. Skipping creation.");
        //}

        // Generate a cryptographic key
        byte[] keyBytes = new byte[32];
        using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
        {
            rng.GetBytes(keyBytes);
        }
        StringBuilder keyBuilder = new StringBuilder(64);
        foreach (byte b in keyBytes)
        {
            keyBuilder.Append(b.ToString("x2"));
        }
        StringBuilder key = new StringBuilder(keyBuilder.ToString(), 1024);

        Debug.Log("Try to init SDK");
        try
        {
            IntPtr resultPtr = GeniusSDKInit(pathBuilder, key, 1, 1, 42001);
            string result = Marshal.PtrToStringAnsi(resultPtr);
            Debug.Log($"GeniusSDKInit returned: {result}");
            isReady = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error initializing Genius SDK: {ex.Message}");
        }

        // Simulating async initialization
        yield return null; // Ensure the coroutine has at least one yield
    }

    // Public method to pay developer 
    public bool PayDev(ulong amount)
    {
        Debug.Log($"Attempting to pay developer {amount} tokens");
        try
        {
            bool result = GeniusSDKPayDev(amount);
            Debug.Log($"GeniusSDKPayDev returned: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in GeniusSDKPayDev: {ex.Message}");
            return false;
        }
    }

    // Public method to get current balance
    public ulong GetBalance()
    {
        Debug.Log("Getting current balance");
        try
        {
            ulong balance = GeniusSDKGetBalance();
            Debug.Log($"Current balance: {balance}");
            return balance;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in GeniusSDKGetBalance: {ex.Message}");
            return 0;
        }
    }

    public double GetGNUSPrice()
    {
        Debug.Log("Getting current GNUS price");
        try
        {
            double price = GeniusSDKGetGNUSPrice();
            Debug.Log($"Current Prive: {price}");
            return price;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in GetGNUSPrice: {ex.Message}");
            return 0;
        }
    }

    public bool IsReady => isReady;


    void OnApplicationQuit()
    {
        Debug.unityLogger.logEnabled = true;
        Debug.Log("Shutting down Genius SDK on application quit.");
        GeniusSDKShutdown();
        Debug.unityLogger.logEnabled = false;
    }

    void OnDestroy()
    {
        Debug.unityLogger.logEnabled = true;
        Debug.Log("Shutting down Genius SDK on destroy.");
        GeniusSDKShutdown();
        Debug.unityLogger.logEnabled = false;
    }
}