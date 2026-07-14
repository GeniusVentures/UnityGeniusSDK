using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
        // 131 = GENIUS_SDK_ADDRESS_SIZE + 1, where the header defines
        // GENIUS_SDK_ADDRESS_SIZE = 2 + 128 ("0x" + 128 hex characters).
        //
        // This was 67, which is half the size native actually returns. The struct is
        // marshaled by value, so an undersized buffer silently truncates GetAddress() and
        // corrupts both Transfer overloads -- and it does so quietly, producing an address
        // that still looks like an address. Do not "simplify" this back.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 131)]
        public string address;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GeniusTokenValue
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
        public string value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GeniusTokenID
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] data;
    }

    // Enum types matching C API
    public enum GeniusNodeReturnValue : int
    {
        GENIUS_NODE_RET_OK = 0,
        GENIUS_NODE_ERROR_NOT_INITIALIZED,
        GENIUS_NODE_ERROR_PROCESS_IMAGE,
        GENIUS_NODE_ERROR_MINT,
        GENIUS_NODE_INVALID_ARGUMENT,
        GENIUS_NODE_ERROR_TRANSFER,
        GENIUS_NODE_ERROR_PAY_DEV
    }

    public enum GeniusNodeState : int
    {
        GENIUS_NODE_CREATING = 0,
        GENIUS_NODE_MIGRATING_DATABASE,
        GENIUS_NODE_INITIALIZING_DATABASE,
        GENIUS_NODE_INITIALIZING_PROCESSING,
        GENIUS_NODE_INITIALIZING_BLOCKCHAIN,
        GENIUS_NODE_INITIALIZING_TRANSACTIONS,
        GENIUS_NODE_INITIALIZING_DHT,
        GENIUS_NODE_READY
    }

    public enum GeniusTransactionManagerState : int
    {
        GENIUS_TM_STATE_CREATING = 0,
        GENIUS_TM_STATE_INITIALIZING = 1,
        GENIUS_TM_STATE_SYNCHING = 2,
        GENIUS_TM_STATE_READY = 3
    }

    public enum GeniusTransactionStatus : int
    {
        GENIUS_TX_STATUS_CREATED = 0,
        GENIUS_TX_STATUS_SENDING = 1,
        GENIUS_TX_STATUS_CONFIRMED = 2,
        GENIUS_TX_STATUS_VERIFYING = 3,
        GENIUS_TX_STATUS_FAILED = 4,
        GENIUS_TX_STATUS_INVALID = 5
    }

    public enum GeniusProcessingStatus : int
    {
        GENIUS_PR_STATUS_DISABLED = 0,
        GENIUS_PR_STATUS_IDLE = 1,
        GENIUS_PR_STATUS_PROCESSING = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GeniusProcessingStatusInfo
    {
        public GeniusProcessingStatus status;
        public float percentage;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GeniusStatusInfo
    {
        public float percentage;
        public IntPtr message;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GeniusMnemonic
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 216)]
        public string mnemonic;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GeniusMnemonicAndStatus
    {
        public int status;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 216)]
        public string mnemonic;
    }

    // DLL Import declarations for all functions in order

    // Initialization functions
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern IntPtr GeniusSDKInit(string basePath, string devConfig);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern IntPtr GeniusSDKInitWithKey(string basePath, string devConfig, string ethPrivateKey);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern IntPtr GeniusSDKInitWithMnemonic(string basePath, string devConfig, string mnemonic);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusStatusInfo GeniusSDKGetInitializationStatus();

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern void GeniusSDKFree(IntPtr ptr);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern void GeniusSDKLoadLogConfig();

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusNodeReturnValue GeniusSDKShutdown();

    // Account management functions
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern IntPtr GeniusSDKGetAvailableAccounts();

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusNodeReturnValue GeniusSDKAddAccountWithMnemonic(string mnemonic);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusNodeReturnValue GeniusSDKSelectGeniusAccount(string publicAddress);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusNodeReturnValue GeniusSDKSetPayoutAddress(string publicAddress);

    // Balance and price functions
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern ulong GeniusSDKGetBalance(GeniusTokenID token_id);

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
    private static extern IntPtr GeniusSDKGetVersion();

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
    private static extern GeniusNodeReturnValue GeniusSDKTransfer(ulong amount, ref GeniusAddress dest, GeniusTokenID token_id);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusNodeReturnValue GeniusSDKTransferGNUS(ref GeniusTokenValue gnus, ref GeniusAddress dest);

    // Pay dev function
#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusNodeReturnValue GeniusSDKPayDev(ulong amount, GeniusTokenID token_id);

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
    private static extern GeniusNodeReturnValue GeniusSDKProcess(string jsondata);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern bool GeniusSDKCheckJobValidity(string jsondata);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusTransactionManagerState GeniusSDKGetTransactionManagerState();

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusNodeState GeniusSDKGetNodeState();

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusTransactionStatus GeniusSDKGetTransactionStatus(string tx_id);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusProcessingStatusInfo GeniusSDKGetProcessingStatus();

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusNodeReturnValue GeniusSDKMint(ulong amount, string transaction_hash, string chain_id, GeniusTokenID token_id);

#if UNITY_IOS
    [DllImport("__Internal")]
#else
    [DllImport("GeniusSDK")]
#endif
    private static extern GeniusNodeReturnValue GeniusSDKMintGNUS(ref GeniusTokenValue amount, string transaction_hash, string chain_id);

    // Instance management
    private bool isReady = false;
    private bool isInitializing = false;

    // True as soon as GeniusSDKInit has returned an init path -- i.e. the native node object
    // exists, and the account with it. This is NOT the same thing as isReady, which only flips
    // at 100%.
    //
    // The distinction matters. GetInitializationStatus tracks node bring-up, most of which is
    // blockchain sync, and a node that cannot reach a full node parks partway through it
    // indefinitely. Calls that only need an account must not wait for a readiness that may
    // never arrive.
    private bool isInitialized = false;

    private bool isShutdown = false;
    private static bool androidKeyStoreInitialized = false;
    [SerializeField] private string address = "0xcatcatcat";
    [SerializeField][Range(0f, 1f)] private float cut = 0.7f;
    [SerializeField] private float tokenValue = 1.0f;
    [SerializeField] private string tokenID = "0000000000000000000000000000000100000000000000000000000000000002";
    [SerializeField] private string pubsubPort = "";
    [SerializeField] private string pubsubBindAddress = "";
    [SerializeField] private string[] bootstrapAddresses = new string[]
    {
        "/ip4/104.131.131.82/tcp/4001/ipfs/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ",
        "/ip4/104.236.179.241/tcp/4001/ipfs/QmSoLPppuBtQSGwKDZT2M73ULpjvfd3aZ6ha4oFGL1KrGM",
        "/ip4/128.199.219.111/tcp/4001/ipfs/QmSoLSafTMBsPKadTEgaXctDQVcqN88CNLHXMkTNwMKPnu",
        "/ip4/104.236.76.40/tcp/4001/ipfs/QmSoLV4Bbm51jM9C4gDYZQ9Cy3U6aXMJDAbzgu2fzaDs64",
        "/ip4/178.62.158.247/tcp/4001/ipfs/QmSoLer265NRgSp2LA3dPaeykiS1J6DifTC88f5uVQKNAd",
        "/ip6/2604:a880:1:20::203:d001/tcp/4001/ipfs/QmSoLPppuBtQSGwKDZT2M73ULpjvfd3aZ6ha4oFGL1KrGM",
        "/ip6/2400:6180:0:d0::151:6001/tcp/4001/ipfs/QmSoLSafTMBsPKadTEgaXctDQVcqN88CNLHXMkTNwMKPnu",
        "/ip6/2604:a880:800:10::4a:5001/tcp/4001/ipfs/QmSoLV4Bbm51jM9C4gDYZQ9Cy3U6aXMJDAbzgu2fzaDs64",
        "/ip6/2a03:b0c0:0:1010::23:1001/tcp/4001/ipfs/QmSoLer265NRgSp2LA3dPaeykiS1J6DifTC88f5uVQKNAd"
    };

    [SerializeField] private string[] bootstrapFullnodes = new string[]
    {
        "/dns4/sg-fullnode-1.gnus.ai/tcp/40102/ipfs/12D3KooWRqFHPFz6YptGnt4wLEGsNuWuv5TLN7rdQ9CFJcbHCWZC"
    };
    [SerializeField] private bool upnpEnabled = true;
    [SerializeField] private int highWater = 300;
    [SerializeField] private int lowWater = 150;
    [SerializeField] private string authorizedFullNode = "8a33bdf1445a68736429d1773be8682362753a0efc6fb9d8b3e8dffe3b74fc91e26b203fd521547a5219eddf1d3ac51fd17a7646c9bca5ef065da131add4e5a2";
    [SerializeField] private bool crdtBackupEnabled = true;
    [SerializeField] private int crdtBackupIntervalMinutes = 15;
    [SerializeField] private int crdtBackupKeepCount = 12;
    [SerializeField] private bool crdtBackupAutoRestoreOnRepairFailure = true;

    public enum LogLevel
    {
        trace,
        debug,
        info,
        warn,
        err,
        critical,
        off
    }

    [Serializable]
    public struct LoggerEntry
    {
        [HideInInspector] public string name;
        public LogLevel level;
    }

    [SerializeField] private LoggerEntry[] loggerConfigs = new LoggerEntry[]
    {
        new LoggerEntry { name = "SuperGeniusNode",                   level = LogLevel.trace },
        new LoggerEntry { name = "GeniusNode",                        level = LogLevel.err },
        new LoggerEntry { name = "GlobalDB",                          level = LogLevel.err },
        new LoggerEntry { name = "GraphsyncDAGSyncer",                level = LogLevel.err },
        new LoggerEntry { name = "graphsync",                         level = LogLevel.err },
        new LoggerEntry { name = "PubSubBroadcasterExt",              level = LogLevel.err },
        new LoggerEntry { name = "CrdtDatastore",                     level = LogLevel.err },
        new LoggerEntry { name = "CrdtHeads",                         level = LogLevel.err },
        new LoggerEntry { name = "TransactionManager",                level = LogLevel.err },
        new LoggerEntry { name = "MigrationManager",                  level = LogLevel.err },
        new LoggerEntry { name = "MigrationStep",                     level = LogLevel.err },
        new LoggerEntry { name = "ProcessingTaskQueueImpl",           level = LogLevel.err },
        new LoggerEntry { name = "rocksdb",                           level = LogLevel.err },
        new LoggerEntry { name = "Kademlia",                          level = LogLevel.err },
        new LoggerEntry { name = "Noise",                             level = LogLevel.err },
        new LoggerEntry { name = "ProcessingEngine",                  level = LogLevel.err },
        new LoggerEntry { name = "ProcessingSubTaskQueueAccessorImpl",level = LogLevel.err },
        new LoggerEntry { name = "ProcessingService",                 level = LogLevel.err },
        new LoggerEntry { name = "ProcessingSubTaskQueueManager",     level = LogLevel.err },
        new LoggerEntry { name = "UPNP",                              level = LogLevel.err },
        new LoggerEntry { name = "ProcessingNode",                    level = LogLevel.err },
        new LoggerEntry { name = "GossipPubSub",                      level = LogLevel.err },
        new LoggerEntry { name = "AccountMessenger",                  level = LogLevel.err },
        new LoggerEntry { name = "GeniusAccount",                     level = LogLevel.err },
        new LoggerEntry { name = "KeyPairFileStorage",                level = LogLevel.err },
        new LoggerEntry { name = "Blockchain",                        level = LogLevel.err },
        new LoggerEntry { name = "ValidatorRegistry",                 level = LogLevel.err },
        new LoggerEntry { name = "SGProcessingManager",               level = LogLevel.err },
        new LoggerEntry { name = "SGProcessor",                       level = LogLevel.err },
        new LoggerEntry { name = "CRDTCallbackManager",               level = LogLevel.err },
        new LoggerEntry { name = "CoinPrices",                        level = LogLevel.err },
        new LoggerEntry { name = "FILECommon",                        level = LogLevel.err },
        new LoggerEntry { name = "FileManager",                       level = LogLevel.err },
        new LoggerEntry { name = "HTTPCommon",                        level = LogLevel.err },
        new LoggerEntry { name = "IPFSCommon",                        level = LogLevel.err },
        new LoggerEntry { name = "IPFSLoader",                        level = LogLevel.err },
        new LoggerEntry { name = "MNNLoader",                         level = LogLevel.err },
        new LoggerEntry { name = "WSCommon",                          level = LogLevel.err }
    };

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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private bool IsActiveSingleton()
    {
        return instance == this;
    }

    private class NativeInitResult
    {
        public IntPtr resultPtr;
        public string errorMessage;
    }

    private IEnumerator InitGeniusSDK(string basePath, string devConfigJson, string mnemonic)
    {
        if (isReady)
        {
            yield break;
        }

        if (isInitializing)
        {
            UnityEngine.Debug.Log("Genius SDK initialization is already running.");
            yield break;
        }

        isInitializing = true;
        UnityEngine.Debug.Log("Initializing Genius SDK");
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!InitializeAndroidKeyStore())
        {
            isInitializing = false;
            yield break;
        }
#endif
        string sgnsConfigPath = Path.Combine(basePath, "sgns_config.json");
        string networkConfigPath = Path.Combine(basePath, "network_config.json");
        string crdtConfigPath = Path.Combine(basePath, "crdt_config.json");
        string logConfigPath = Path.Combine(basePath, "log_config.json");

        string sgnsJsonData = BuildSgnsConfigJson();
        string networkJsonData = BuildNetworkConfigJson();
        string crdtJsonData = BuildCrdtConfigJson();
        string logJsonData = BuildLogConfigJson();

        UnityEngine.Debug.Log("Starting Genius SDK native init in background.");

        string inlineDevConfig = $@"{{
    ""Address"": ""{address}"",
    ""Cut"": ""{cut}"",
    ""TokenValue"": ""{tokenValue:F5}"",
    ""TokenID"": ""{tokenID}"",
    ""WriteDirectory"": """"
}}";

        string configJson = string.IsNullOrEmpty(devConfigJson) ? inlineDevConfig : devConfigJson;
        Task<NativeInitResult> initTask = Task.Run(() => RunNativeInit(
            basePath,
            configJson,
            mnemonic,
            sgnsConfigPath,
            sgnsJsonData,
            networkConfigPath,
            networkJsonData,
            crdtConfigPath,
            crdtJsonData,
            logConfigPath,
            logJsonData));

        while (!initTask.IsCompleted)
        {
            yield return null;
        }

        if (initTask.IsFaulted)
        {
            isInitializing = false;
            UnityEngine.Debug.LogError("GeniusSDK native init task failed: " + initTask.Exception);
            yield break;
        }

        NativeInitResult initResult = initTask.Result;
        if (!string.IsNullOrEmpty(initResult.errorMessage))
        {
            isInitializing = false;
            UnityEngine.Debug.LogError(initResult.errorMessage);
            yield break;
        }

        if (initResult.resultPtr == IntPtr.Zero)
        {
            isInitializing = false;
            UnityEngine.Debug.LogError("GeniusSDK init returned null — initialization failed");
            yield break;
        }

        string initPath = Marshal.PtrToStringAnsi(initResult.resultPtr);
        UnityEngine.Debug.Log($"GeniusSDK init returned: {initPath}");

        // The node object exists from here on, and the account with it. Everything below is
        // bring-up progress, not existence.
        isInitialized = true;

        while (!isReady)
        {
            GeniusStatusInfo status = GeniusSDKGetInitializationStatus();
            if (status.percentage >= 1.0f)
            {
                isReady = true;
            }
            if (status.message != IntPtr.Zero)
            {
                string msg = Marshal.PtrToStringAnsi(status.message);
                UnityEngine.Debug.Log("SDK Init: " + msg + " (" + (status.percentage * 100f) + "%)");
                GeniusSDKFree(status.message);
            }
            if (!isReady)
            {
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        isInitializing = false;
    }

    private static NativeInitResult RunNativeInit(
        string basePath,
        string configJson,
        string mnemonic,
        string sgnsConfigPath,
        string sgnsJsonData,
        string networkConfigPath,
        string networkJsonData,
        string crdtConfigPath,
        string crdtJsonData,
        string logConfigPath,
        string logJsonData)
    {
        try
        {
            File.WriteAllText(sgnsConfigPath, sgnsJsonData);
            File.WriteAllText(networkConfigPath, networkJsonData);
            File.WriteAllText(crdtConfigPath, crdtJsonData);
            File.WriteAllText(logConfigPath, logJsonData);

            IntPtr resultPtr = string.IsNullOrEmpty(mnemonic)
                ? GeniusSDKInit(basePath, configJson)
                : GeniusSDKInitWithMnemonic(basePath, configJson, mnemonic);

            return new NativeInitResult { resultPtr = resultPtr };
        }
        catch (Exception ex)
        {
            return new NativeInitResult { errorMessage = "GeniusSDK native init failed: " + ex.Message };
        }
    }

    private static string EscapeJsonString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\b", "\\b")
            .Replace("\f", "\\f")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    private string BuildNetworkConfigJson()
    {
        var builder = new StringBuilder(256);
        builder.AppendLine("{");
        builder.AppendLine("  \"port_seed\": 42001,");
        builder.AppendLine("  \"auto_dht\": true");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private string BuildSgnsConfigJson()
    {
        var builder = new StringBuilder(1024);
        builder.AppendLine("{");
        builder.AppendLine("  \"is_processor\": true,");
        builder.AppendLine("  \"node_type\": \"Light\",");
        builder.AppendLine($"  \"authorized_full_node\": \"{EscapeJsonString(authorizedFullNode)}\",");
        builder.AppendLine("  \"bootstrap_fullnodes\": [");

        for (int i = 0; i < bootstrapFullnodes.Length; i++)
        {
            string entry = EscapeJsonString(bootstrapFullnodes[i] ?? string.Empty);
            string suffix = i < bootstrapFullnodes.Length - 1 ? "," : string.Empty;
            builder.AppendLine($"    \"{entry}\"{suffix}");
        }

        builder.AppendLine("  ]");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private string BuildCrdtConfigJson()
    {
        var builder = new StringBuilder(512);
        builder.AppendLine("{");
        builder.AppendLine($"  \"backup_enabled\": {crdtBackupEnabled.ToString().ToLowerInvariant()},");
        builder.AppendLine($"  \"backup_interval_minutes\": {crdtBackupIntervalMinutes},");
        builder.AppendLine($"  \"backup_keep_count\": {crdtBackupKeepCount},");
        builder.AppendLine($"  \"backup_auto_restore_on_repair_failure\": {crdtBackupAutoRestoreOnRepairFailure.ToString().ToLowerInvariant()}");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private string BuildLogConfigJson()
    {
        var builder = new StringBuilder(2048);
        builder.AppendLine("{");
        builder.AppendLine("    \"loggers\": {");

        for (int i = 0; i < loggerConfigs.Length; i++)
        {
            string name = EscapeJsonString(loggerConfigs[i].name);
            string level = loggerConfigs[i].level.ToString().ToLowerInvariant();
            string suffix = i < loggerConfigs.Length - 1 ? "," : string.Empty;
            builder.AppendLine($"        \"{name}\": \"{level}\"{suffix}");
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    // Public wrapper methods for all functions
    //Convert 0x token
    private GeniusTokenID ParseTokenId(string tokenid)
    {
        if (string.IsNullOrEmpty(tokenid))
            throw new ArgumentException("TokenID is null or empty");

        if (tokenid.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            tokenid = tokenid.Substring(2);

        if (tokenid.Length != 64)
            throw new ArgumentException($"TokenID should be 64 hex characters, got {tokenid.Length}");

        byte[] tokenBytes = new byte[32];
        for (int i = 0; i < 32; i++)
            tokenBytes[i] = Convert.ToByte(tokenid.Substring(i * 2, 2), 16);

        return new GeniusTokenID { data = tokenBytes };
    }

    //Get Token ID
    public string TokenID
    {
        get { return tokenID; }
        set { tokenID = value; }
    }

    /// <summary>
    /// Starts the SDK, at most once for the lifetime of the process.
    ///
    /// The once-only rule lives HERE rather than in each caller. This wrapper is a
    /// DontDestroyOnLoad singleton, but its callers are ordinary scene components that are
    /// recreated on every scene load, so each of them believes it is the first -- and a second
    /// GeniusSDKInit against a live node is not something to discover the hard way.
    ///
    /// Returns immediately: the blocking native call runs on a thread-pool thread and the
    /// coroutine polls the Task, so the game keeps rendering throughout.
    /// </summary>
    public void BeginInitialize(string basePath)
    {
        if (isInitialized || isInitializing)
        {
            return;
        }

        StartCoroutine(InitGeniusSDK(basePath, null, null));
    }

    public void BeginInitializeWithMnemonic(string basePath, string mnemonic)
    {
        StartCoroutine(InitGeniusSDK(basePath, null, mnemonic));
    }

    public void Free(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            GeniusSDKFree(ptr);
        }
    }

    public void LoadLogConfig()
    {
        GeniusSDKLoadLogConfig();
    }

    public GeniusStatusInfo GetInitializationStatus()
    {
        return GeniusSDKGetInitializationStatus();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private bool InitializeAndroidKeyStore()
    {
        if (androidKeyStoreInitialized)
            return true;

        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            using (var keyStoreHelper = new AndroidJavaClass("ai.gnus.sdk.KeyStoreHelper"))
            {
                keyStoreHelper.CallStatic("initialize", context);
                androidKeyStoreInitialized = true;
                UnityEngine.Debug.Log("KeyStoreHelper initialized.");
                return true;
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to initialize KeyStoreHelper: {ex.Message}");
            return false;
        }
    }
#endif

    public GeniusNodeReturnValue Shutdown()
    {
        var result = GeniusSDKShutdown();
        isShutdown = true;
        return result;
    }

    public string GetAvailableAccounts()
    {
        IntPtr resultPtr = GeniusSDKGetAvailableAccounts();
        if (resultPtr == IntPtr.Zero)
        {
            return null;
        }
        return Marshal.PtrToStringAnsi(resultPtr);
    }

    public GeniusNodeReturnValue AddAccountWithMnemonic(string mnemonic)
    {
        return GeniusSDKAddAccountWithMnemonic(mnemonic);
    }

    public GeniusNodeReturnValue SelectGeniusAccount(string publicAddress)
    {
        return GeniusSDKSelectGeniusAccount(publicAddress);
    }

    public GeniusNodeReturnValue SetPayoutAddress(string publicAddress)
    {
        return GeniusSDKSetPayoutAddress(publicAddress);
    }

    // Balance and price wrappers
    public ulong GetBalance(string tokenid)
    {
        try
        {
            GeniusTokenID tokenId = ParseTokenId(tokenid);
            ulong balance = GeniusSDKGetBalance(tokenId);
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

    public string GetVersion()
    {
        try
        {
            IntPtr resultPtr = GeniusSDKGetVersion();
            return Marshal.PtrToStringAnsi(resultPtr);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error in GetVersion: {ex.Message}");
            return "Unknown";
        }
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
    public GeniusNodeReturnValue Transfer(ulong amount, GeniusAddress destination, string tokenid = null)
    {
        try
        {
            GeniusTokenID tokenId = ParseTokenId(tokenid ?? tokenID);
            return GeniusSDKTransfer(amount, ref destination, tokenId);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error in Transfer: {ex.Message}");
            return GeniusNodeReturnValue.GENIUS_NODE_INVALID_ARGUMENT;
        }
    }

    public GeniusNodeReturnValue TransferGNUS(GeniusTokenValue gnus, GeniusAddress destination)
    {
        return GeniusSDKTransferGNUS(ref gnus, ref destination);
    }

    // Pay dev wrapper (updated to match C signature with token_id)
    public GeniusNodeReturnValue PayDev(ulong amount, string tokenId = null)
    {
        UnityEngine.Debug.Log($"Attempting to pay developer {amount} tokens");
        try
        {
            GeniusTokenID tid = ParseTokenId(tokenId ?? tokenID);
            GeniusNodeReturnValue result = GeniusSDKPayDev(amount, tid);
            UnityEngine.Debug.Log($"GeniusSDKPayDev returned: {result}");
            return result;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error in GeniusSDKPayDev: {ex.Message}");
            return GeniusNodeReturnValue.GENIUS_NODE_INVALID_ARGUMENT;
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
    public GeniusNodeReturnValue Process(string jsonData)
    {
        return GeniusSDKProcess(jsonData);
    }

    public bool CheckJobValidity(string jsonData)
    {
        return GeniusSDKCheckJobValidity(jsonData);
    }

    // State query wrappers
    public GeniusTransactionManagerState GetTransactionManagerState()
    {
        return GeniusSDKGetTransactionManagerState();
    }

    public GeniusNodeState GetNodeState()
    {
        return GeniusSDKGetNodeState();
    }

    public GeniusTransactionStatus GetTransactionStatus(string txId)
    {
        return GeniusSDKGetTransactionStatus(txId);
    }

    public GeniusProcessingStatusInfo GetProcessingStatus()
    {
        return GeniusSDKGetProcessingStatus();
    }

    // Mint wrappers
    public GeniusNodeReturnValue Mint(ulong amount, string transactionHash, string chainId, string tokenId = null)
    {
        try
        {
            GeniusTokenID tid = ParseTokenId(tokenId ?? tokenID);
            return GeniusSDKMint(amount, transactionHash, chainId, tid);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error in Mint: {ex.Message}");
            return GeniusNodeReturnValue.GENIUS_NODE_INVALID_ARGUMENT;
        }
    }

    public GeniusNodeReturnValue MintGNUS(GeniusTokenValue amount, string transactionHash, string chainId)
    {
        return GeniusSDKMintGNUS(ref amount, transactionHash, chainId);
    }

    // Properties
    public bool IsReady => isReady;

    /// <summary>
    /// True once GeniusSDKInit has returned, i.e. the node and its account exist. Reached long
    /// before <see cref="IsReady"/>, which requires 100% and therefore a synced blockchain.
    /// </summary>
    public bool IsInitialized => isInitialized;

    /// <summary>
    /// True once the node is past database migration and database init — the point at which
    /// account-scoped calls such as SetPayoutAddress become SAFE TO CALL AT ALL.
    ///
    /// This is not a convenience. Calling SetPayoutAddress while the node is in
    /// MIGRATING_DATABASE returns GENIUS_NODE_RET_OK and then kills the process with an access
    /// violation on one of the SDK's own background threads, seconds later. The return code
    /// gives no warning whatsoever. Established by driving the shipped DLL directly:
    ///
    ///   state 1 MIGRATING_DATABASE      (10%)  -> returns OK, then CRASHES within 5s
    ///   state 3 INITIALIZING_PROCESSING (52%)  -> returns OK, node stays healthy
    ///   never called (control)                 -> healthy
    ///
    /// Gate on this, NOT on the return code and NOT on IsReady: a node that cannot reach a
    /// full node parks at INITIALIZING_PROCESSING and may never reach READY, so waiting for
    /// readiness makes these calls impossible in exactly the environment we develop in.
    /// </summary>
    public bool IsSafeForAccountCalls
    {
        get
        {
            if (!isInitialized)
            {
                return false;
            }

            return GeniusSDKGetNodeState() >= GeniusNodeState.GENIUS_NODE_INITIALIZING_PROCESSING;
        }
    }

    // Cleanup
    void OnApplicationQuit()
    {
        if (!IsActiveSingleton()) return;
        if (isShutdown) return;
        isShutdown = true;
        UnityEngine.Debug.Log("Shutting down Genius SDK on application quit.");
        var result = GeniusSDKShutdown();
        if (result != GeniusNodeReturnValue.GENIUS_NODE_RET_OK)
        {
            UnityEngine.Debug.LogWarning($"Shutdown returned: {result}");
        }
    }

    void OnDestroy()
    {
        if (!IsActiveSingleton()) return;
        if (isShutdown) return;
        isShutdown = true;
        UnityEngine.Debug.Log("Shutting down Genius SDK on destroy.");
        var result = GeniusSDKShutdown();
        if (result != GeniusNodeReturnValue.GENIUS_NODE_RET_OK)
        {
            UnityEngine.Debug.LogWarning($"Shutdown returned: {result}");
        }
    }
}
