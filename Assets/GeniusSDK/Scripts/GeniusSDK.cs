using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

public class GeniusSDKWrapper : MonoBehaviour
{
    // Struct definitions matching C structures
    [StructLayout(LayoutKind.Sequential)]
    public struct GeniusArray
    {
        public ulong size;
        public IntPtr ptr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GeniusMatrix
    {
        public ulong size;
        public IntPtr ptr; // GeniusArray*
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GeniusAddress
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 67)] // 2 + 256/4 + 1
        public string address;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GeniusTokenValue
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
        public string value;
    }

    // DLL Import declarations for all functions in order

    // Initialization functions
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern IntPtr GeniusSDKInit(StringBuilder base_path, StringBuilder eth_private_key, int autodht, int process, ushort baseport);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern IntPtr GeniusSDKInitSecure(StringBuilder base_path, string dev_config, StringBuilder eth_private_key, int autodht, int process, ushort baseport);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern IntPtr GeniusSDKInitMinimal(StringBuilder base_path, StringBuilder eth_private_key, ushort baseport);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern void GeniusSDKShutdown();

    // Balance and price functions
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern ulong GeniusSDKGetBalance();

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern double GeniusSDKGetGNUSPrice();

    // Token conversion functions
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusTokenValue GeniusSDKToChild(ulong minions, string token_id);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern ulong GeniusSDKFromChild(ref GeniusTokenValue child, string token_id);

    // Balance retrieval functions
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusTokenValue GeniusSDKGetBalanceGNUS();

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern IntPtr GeniusSDKGetBalanceGNUSString();

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern ulong GeniusSDKGetBalanceByToken(string token_id);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern IntPtr GeniusSDKGetBalanceByTokenString(string token_id);

    // Address function
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusAddress GeniusSDKGetAddress();

    // Transaction functions
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusMatrix GeniusSDKGetInTransactions();

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusMatrix GeniusSDKGetOutTransactions();

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern void GeniusSDKFreeTransactions(GeniusMatrix matrix);

    // Transfer functions
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern bool GeniusSDKTransfer(ulong amount, ref GeniusAddress dest);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern bool GeniusSDKTransferGNUS(ref GeniusTokenValue gnus, ref GeniusAddress dest);

    // Pay dev function
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern bool GeniusSDKPayDev(ulong amount, string token_id);

    // Cost calculation functions
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern ulong GeniusSDKGetCost(string jsondata);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusTokenValue GeniusSDKGetCostGNUS(string jsondata);

    // Process function
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern void GeniusSDKProcess(string jsondata);

    // Unit conversion functions
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern ulong GeniusSDKToMinions(ref GeniusTokenValue gnus);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusTokenValue GeniusSDKToGenius(ulong minions);

    // Instance management
    private bool isReady = false;
    private bool isShutdown = false;
    [SerializeField] private string address = "0xcatcatcat";
    [SerializeField][Range(0f, 1f)] private float cut = 0.7f;
    [SerializeField] private float tokenValue = 1.0f;
    [SerializeField] private string tokenID = "0000000000000000000000000000000100000000000000000000000000000002";

    private static GeniusSDKWrapper instance;
    public static GeniusSDKWrapper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("GeniusSDKWrapper").AddComponent<GeniusSDKWrapper>();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(InitGeniusSDK());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator InitGeniusSDK()
    {
        UnityEngine.Debug.Log("Initializing Genius SDK");
        StringBuilder pathBuilder = new StringBuilder(UnityEngine.Application.persistentDataPath + "/", 1024);
        string destinationPath = Path.Combine(UnityEngine.Application.persistentDataPath, "dev_config.json");

        UnityEngine.Debug.Log("dev_config.json not found. Creating a new one...");
        string jsonData = $@"{{
    ""Address"": ""{address}"",
    ""Cut"": ""{cut}"",
    ""TokenValue"": ""{tokenValue:F5}"",
    ""TokenID"": ""{tokenID}"",
    ""WriteDirectory"": """"
}}";

        try
        {
            File.WriteAllText(destinationPath, jsonData);
            UnityEngine.Debug.Log("dev_config.json created successfully.");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error writing dev_config.json: {ex.Message}");
            yield break;
        }

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

        UnityEngine.Debug.Log("Try to init SDK");
        try
        {
            IntPtr resultPtr = GeniusSDKInitSecure(pathBuilder, jsonData, key, 1, 1, 42001);
            string result = Marshal.PtrToStringAnsi(resultPtr);
            UnityEngine.Debug.Log($"GeniusSDKInit returned: {result}");
            isReady = true;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error initializing Genius SDK: {ex.Message}");
        }

        yield return null;
    }

    // Public wrapper methods for all functions

    // Initialization wrappers
    public string InitSDK(string basePath, string privateKey, bool autoDht, bool process, ushort basePort)
    {
        var pathBuilder = new StringBuilder(basePath, 1024);
        var keyBuilder = new StringBuilder(privateKey, 1024);
        IntPtr resultPtr = GeniusSDKInit(pathBuilder, keyBuilder, autoDht ? 1 : 0, process ? 1 : 0, basePort);
        return Marshal.PtrToStringAnsi(resultPtr);
    }

    public string InitMinimalSDK(string basePath, string privateKey, ushort basePort)
    {
        var pathBuilder = new StringBuilder(basePath, 1024);
        var keyBuilder = new StringBuilder(privateKey, 1024);
        IntPtr resultPtr = GeniusSDKInitMinimal(pathBuilder, keyBuilder, basePort);
        return Marshal.PtrToStringAnsi(resultPtr);
    }

    public void Shutdown()
    {
        GeniusSDKShutdown();
        isShutdown = true;
    }

    // Balance and price wrappers
    public ulong GetBalance()
    {
        try
        {
            ulong balance = GeniusSDKGetBalance();
            return balance;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error in GeniusSDKGetBalance: {ex.Message}");
            return 0;
        }
    }

    public double GetGNUSPrice()
    {
        try
        {
            double price = GeniusSDKGetGNUSPrice();
            return price;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error in GetGNUSPrice: {ex.Message}");
            return 0;
        }
    }

    // Token conversion wrappers
    public GeniusTokenValue ToChild(ulong minions, string tokenId)
    {
        return GeniusSDKToChild(minions, tokenId);
    }

    public ulong FromChild(GeniusTokenValue child, string tokenId)
    {
        return GeniusSDKFromChild(ref child, tokenId);
    }

    // Balance retrieval wrappers
    public GeniusTokenValue GetBalanceGNUS()
    {
        return GeniusSDKGetBalanceGNUS();
    }

    public string GetBalanceGNUSString()
    {
        IntPtr resultPtr = GeniusSDKGetBalanceGNUSString();
        return Marshal.PtrToStringAnsi(resultPtr);
    }

    public ulong GetBalanceByToken(string tokenId)
    {
        return GeniusSDKGetBalanceByToken(tokenId);
    }

    public string GetBalanceByTokenString(string tokenId)
    {
        IntPtr resultPtr = GeniusSDKGetBalanceByTokenString(tokenId);
        return Marshal.PtrToStringAnsi(resultPtr);
    }

    // Address wrapper
    public GeniusAddress GetAddress()
    {
        return GeniusSDKGetAddress();
    }

    // Transaction wrappers
    public GeniusMatrix GetInTransactions()
    {
        return GeniusSDKGetInTransactions();
    }

    public GeniusMatrix GetOutTransactions()
    {
        return GeniusSDKGetOutTransactions();
    }

    public void FreeTransactions(GeniusMatrix matrix)
    {
        GeniusSDKFreeTransactions(matrix);
    }

    // Transfer wrappers
    public bool Transfer(ulong amount, GeniusAddress destination)
    {
        return GeniusSDKTransfer(amount, ref destination);
    }

    public bool TransferGNUS(GeniusTokenValue gnus, GeniusAddress destination)
    {
        return GeniusSDKTransferGNUS(ref gnus, ref destination);
    }

    // Pay dev wrapper (updated to match C signature with token_id)
    public bool PayDev(ulong amount, string tokenId = null)
    {
        UnityEngine.Debug.Log($"Attempting to pay developer {amount} tokens");
        try
        {
            bool result = GeniusSDKPayDev(amount, tokenId ?? tokenID);
            UnityEngine.Debug.Log($"GeniusSDKPayDev returned: {result}");
            return result;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error in GeniusSDKPayDev: {ex.Message}");
            return false;
        }
    }

    // Cost calculation wrappers
    public ulong GetCost(string jsonData)
    {
        return GeniusSDKGetCost(jsonData);
    }

    public GeniusTokenValue GetCostGNUS(string jsonData)
    {
        return GeniusSDKGetCostGNUS(jsonData);
    }

    // Process wrapper
    public void Process(string jsonData)
    {
        GeniusSDKProcess(jsonData);
    }

    // Unit conversion wrappers
    public ulong ToMinions(GeniusTokenValue gnus)
    {
        return GeniusSDKToMinions(ref gnus);
    }

    public GeniusTokenValue ToGenius(ulong minions)
    {
        return GeniusSDKToGenius(minions);
    }

    // Properties
    public bool IsReady => isReady;

    // Cleanup
    void OnApplicationQuit()
    {
        if (isShutdown) return;
        isShutdown = true;
        UnityEngine.Debug.Log("Shutting down Genius SDK on application quit.");
        GeniusSDKShutdown();
    }

    void OnDestroy()
    {
        if (isShutdown) return;
        isShutdown = true;
        UnityEngine.Debug.Log("Shutting down Genius SDK on destroy.");
        GeniusSDKShutdown();
    }
}