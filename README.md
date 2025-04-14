# 🧠 GeniusSDK for Unity

**GeniusSDK** is a lightweight Unity plugin that integrates the GNUS decentralized compute network into your game or app. It allows players to earn *Minions* (GNUS tokens) by contributing compute power — which they can then spend on in-game rewards or purchases.

---

## 🚀 Features

- 🔌 One-line SDK initialization
- 🪙 Real-time Minion balance display
- 💸 Automatic GNUS-to-USD price conversion
- 🎯 Compatible with both TextMeshPro and UGUI
- 📱 Cross-platform native support: iOS, Android, macOS, Windows, Linux
- ⚡ Drag-and-drop purchase buttons with UnityEvent support

---

## 📦 Installation

### ✅ Option A: Import from GitHub Release

1. Download the latest `.unitypackage` from the [Releases tab](https://github.com/GeniusVentures/UnityGeniusSDK/releases)
2. In Unity:  
   `Assets → Import Package → Custom Package…`
3. Select the downloaded file and click **Import**

### 🧑‍💻 Option B: Clone into your project

```bash
git clone https://github.com/your-username/GeniusSDK.git
```

Make sure to run:

```bash
git lfs install
```

---

## 🧩 Getting Started

### 1. Add the SDK to your scene

Attach `GeniusSDKWrapper` to any persistent GameObject (e.g. your game manager). It initializes automatically in `Awake()` and persists across scenes.

---

### 2. Display Minion Balance

Attach `GeniusSDKBalanceDisplay` to a `Text` or `TextMeshProUGUI` element.

- Automatically refreshes every X seconds (configurable)
- Or call manually:
  ```csharp
  balanceDisplay.ForceUpdateBalance();
  ```

---

### 3. Show Dynamic Minion Pricing

Attach `GeniusSDKPricer` to a `Text` or `TextMeshProUGUI`.

- Set the **USD base price** in the Inspector (e.g., `$1.00`)
- It calculates how many Minions the player must pay based on live GNUS token price
- Auto-refreshes every X seconds (or manually):

  ```csharp
  pricer.ForceUpdatePrice();
  ```

---

### 4. Add a Buy Button

Attach `GeniusSDKPurchase` to a Unity UI `Button`.

- Assign the `GeniusSDKPricer` reference
- Use the `onPurchaseSuccess` UnityEvent to trigger in-game rewards
- Purchase will use the latest Minion price calculated from the `Pricer`

---

## 🧪 Example Setup

```plaintext
[Canvas]
 ├── [Text]      MinionBalanceText + GeniusSDKBalanceDisplay
 ├── [Text]      ItemPriceText + GeniusSDKPricer
 ├── [Button]    PurchaseButton + GeniusSDKPurchase
 │      └── onPurchaseSuccess → your custom success handler (e.g. add gems)
```

---

## 🛠 Requirements

- Unity 2021.3 or newer (tested on 2022.3 LTS)
- Git LFS installed (for native plugin files >100MB)
- Platform-dependent libraries:
  - `libGeniusSDK.so` for Android/Linux
  - `GeniusSDK.framework` for iOS
  - `GeniusSDK.bundle` for macOS
  - `GeniusSDK.dll` for Windows

---

## 🧠 Notes

- You only need **one instance** of `GeniusSDKWrapper` per game
- All pricing and balance logic is async and runs on coroutines
- Purchase buttons automatically disable duplicate presses during execution

---

## 📣 Support

- [Open an issue](https://github.com/GeniusVentures/UnityGeniusSDK/issues) for bugs or feature requests
- PRs welcome!

---

## 🔗 About GNUS / Minions

The GeniusSDK connects your Unity app to the [GNUS Network](https://gnus.ai), where players earn *Minions* (GNUS tokens) by contributing device GPU cycles. Developers receive a portion of the tokens, allowing a monetization model without ads or upfront purchases.
