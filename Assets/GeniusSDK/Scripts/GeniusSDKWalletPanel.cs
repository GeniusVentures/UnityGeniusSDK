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

	private void OnSetPayout()
	{
		if (this.inputPayoutAddress == null || this.textStatus == null)
		{
			return;
		}

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
