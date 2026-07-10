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
		this.btnSetPayout.onClick.AddListener(this.OnSetPayout);
		this.btnAddMnemonic.onClick.AddListener(this.OnAddMnemonic);
	}

	private void Refresh()
	{
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

	private void OnSetPayout()
	{
		string address = this.inputPayoutAddress.text.Trim();
		if (string.IsNullOrEmpty(address))
		{
			this.textStatus.text = "Enter an address";
			return;
		}

		GeniusSDKWrapper.GeniusNodeReturnValue result = GeniusSDKWrapper.Instance.SetPayoutAddress(address);
		this.textStatus.text = result == GeniusSDKWrapper.GeniusNodeReturnValue.GENIUS_NODE_RET_OK
			? "Payout address set"
			: "Failed: " + result;
	}

	private void OnAddMnemonic()
	{
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
