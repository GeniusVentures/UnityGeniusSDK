using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GeniusSDKWalletPanel : MonoBehaviour
{
	[Header("Address Display")]
	[SerializeField] private Text textAddress;

	[Header("Set Payout Address")]
	[SerializeField] private InputField inputPayoutAddress;
	[SerializeField] private Button btnSetPayout;

	[Header("Add Account from Mnemonic")]
	[SerializeField] private InputField inputMnemonic;
	[SerializeField] private Button btnAddMnemonic;

	[Header("Status")]
	[SerializeField] private Text textStatus;

	private bool listenersBound;

	public void Initialize(Text textAddress, InputField inputPayoutAddress, Button btnSetPayout, InputField inputMnemonic, Button btnAddMnemonic, Text textStatus)
	{
		this.textAddress = textAddress;
		this.inputPayoutAddress = inputPayoutAddress;
		this.btnSetPayout = btnSetPayout;
		this.inputMnemonic = inputMnemonic;
		this.btnAddMnemonic = btnAddMnemonic;
		this.textStatus = textStatus;
		this.BindButtons();
	}

	public void Show()
	{
		this.gameObject.SetActive(true);
		this.Refresh();
	}

	public void Hide()
	{
		this.gameObject.SetActive(false);
	}

	private void Start()
	{
		this.BindButtons();
	}

	private void BindButtons()
	{
		if (this.listenersBound || this.btnSetPayout == null || this.btnAddMnemonic == null)
		{
			return;
		}

		this.listenersBound = true;
		this.btnSetPayout.onClick.AddListener(this.OnSetPayout);
		this.btnAddMnemonic.onClick.AddListener(this.OnAddMnemonic);
	}

	private void Refresh()
	{
		if (this.textAddress == null)
		{
			return;
		}

		try
		{
			GeniusSDKWrapper.GeniusAddress addr = GeniusSDKWrapper.Instance.GetAddress();
			if (!string.IsNullOrEmpty(addr.address) && addr.address.StartsWith("0x"))
			{
				this.textAddress.text = addr.address;
			}
			else
			{
				this.textAddress.text = "Not connected";
			}
		}
		catch (Exception)
		{
			this.textAddress.text = "Not connected";
		}
	}

	/// <summary>
	/// The payout address is 128 hex characters with NO "0x" prefix -- a 64-byte public key.
	/// It is NOT an Ethereum address, and it is NOT the node's own address, which GetAddress()
	/// returns WITH a "0x" prefix. Established by driving the shipped DLL directly:
	///
	///   128 hex, no prefix (any case) -> GENIUS_NODE_RET_OK
	///   128 hex WITH "0x"             -> GENIUS_NODE_INVALID_ARGUMENT
	///   Ethereum "0x" + 40 hex        -> GENIUS_NODE_INVALID_ARGUMENT
	///   40 hex, no prefix             -> GENIUS_NODE_INVALID_ARGUMENT
	///
	/// Native performs NO checksum and NO account-existence check -- it accepts any 128-hex
	/// string, including all zeroes -- so a caller that does not validate will happily register
	/// a typo as a payout destination and pay nobody.
	/// </summary>
	private static bool IsValidPayoutAddress(string address)
	{
		if (string.IsNullOrEmpty(address) || address.Length != 128)
		{
			return false;
		}

		for (int i = 0; i < address.Length; i++)
		{
			char c = address[i];
			bool isHex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
			if (!isHex)
			{
				return false;
			}
		}

		return true;
	}

	private void OnSetPayout()
	{
		if (this.inputPayoutAddress == null || this.textStatus == null)
		{
			return;
		}

		string address = this.inputPayoutAddress.text.Trim();

		// The player's wallet may display the key with a "0x" prefix. Native rejects it, so
		// strip it here rather than making the caller know that.
		if (address.Length > 2 && address[0] == '0' && (address[1] == 'x' || address[1] == 'X'))
		{
			address = address.Substring(2);
		}

		if (!IsValidPayoutAddress(address))
		{
			this.textStatus.text = "Not a valid payout address";
			return;
		}

		// MUST NOT be called earlier. SetPayoutAddress returns GENIUS_NODE_RET_OK while the node
		// is still in MIGRATING_DATABASE, and then the process dies of an access violation on an
		// SDK background thread a few seconds later. The return code gives no warning, so this
		// gate is the only thing standing between a user pressing this button too early and a
		// hard crash. IsSafeForAccountCalls -- not IsReady, which may never arrive on a node
		// that cannot reach a full node.
		if (GeniusSDKWrapper.Instance == null || !GeniusSDKWrapper.Instance.IsSafeForAccountCalls)
		{
			this.textStatus.text = "SDK is still starting up. Try again in a moment.";
			return;
		}

		GeniusSDKWrapper.GeniusNodeReturnValue result;

		try
		{
			result = GeniusSDKWrapper.Instance.SetPayoutAddress(address);
		}
		catch (Exception ex)
		{
			// The Android, iOS and Linux native binaries predate this entry point and do not
			// export it, so the P/Invoke throws there. Unhandled, Unity swallows it out of a
			// Button callback and the user sees nothing happen at all.
			Debug.LogError("GeniusSDK: SetPayoutAddress threw: " + ex.Message);
			this.textStatus.text = "Failed: SetPayoutAddress is not available in this native build";
			return;
		}

		this.textStatus.text = result == GeniusSDKWrapper.GeniusNodeReturnValue.GENIUS_NODE_RET_OK
			? "Payout address set"
			: "Failed: " + result;
	}

	private void OnAddMnemonic()
	{
		if (this.inputMnemonic == null || this.textStatus == null)
		{
			return;
		}

		string mnemonic = this.inputMnemonic.text.Trim();
		if (string.IsNullOrEmpty(mnemonic))
		{
			this.textStatus.text = "Enter a mnemonic phrase";
			return;
		}

		GeniusSDKWrapper.GeniusNodeReturnValue result = GeniusSDKWrapper.Instance.AddAccountWithMnemonic(mnemonic);
		if (result == GeniusSDKWrapper.GeniusNodeReturnValue.GENIUS_NODE_RET_OK)
		{
			this.textStatus.text = "Account added";
			this.inputMnemonic.text = "";
			this.Refresh();
		}
		else
		{
			this.textStatus.text = "Failed: " + result;
		}
	}
}
