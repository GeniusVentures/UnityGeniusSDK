using UnityEngine;
using UnityEngine.UI;

public static class GeniusSDKWalletPanelFactory
{
	private const string PanelName = "GeniusSDK Wallet Panel";

	public static GeniusSDKWalletPanel FindOrCreate(Transform parent)
	{
		GeniusSDKWalletPanel existingPanel = GeniusSDKWalletPanelFactory.FindExisting(parent);
		if (existingPanel != null)
		{
			return existingPanel;
		}

		return GeniusSDKWalletPanelFactory.Create(parent);
	}

	public static GeniusSDKWalletPanel FindExisting(Transform parent)
	{
		if (parent == null)
		{
			return null;
		}

		GeniusSDKWalletPanel[] panels = parent.GetComponentsInChildren<GeniusSDKWalletPanel>(true);
		if (panels.Length > 0)
		{
			return panels[0];
		}

		return null;
	}

	public static GeniusSDKWalletPanel Create(Transform parent)
	{
		Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

		GameObject panelObject = GeniusSDKWalletPanelFactory.CreateRect(GeniusSDKWalletPanelFactory.PanelName, parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(760f, 520f), new Vector2(0f, 116f));
		panelObject.layer = parent.gameObject.layer;
		Image panelImage = panelObject.AddComponent<Image>();
		panelImage.color = new Color(0.035f, 0.055f, 0.085f, 0.96f);

		VerticalLayoutGroup layout = panelObject.AddComponent<VerticalLayoutGroup>();
		layout.padding = new RectOffset(28, 28, 24, 24);
		layout.spacing = 12f;
		layout.childAlignment = TextAnchor.UpperCenter;
		layout.childForceExpandWidth = true;
		layout.childForceExpandHeight = false;

		Text title = GeniusSDKWalletPanelFactory.CreateText("Title", panelObject.transform, font, "GeniusSDK Wallet", 26, Color.white, TextAnchor.MiddleLeft);
		GeniusSDKWalletPanelFactory.SetLayout(title.gameObject, -1f, 34f);

		Text textAddress = GeniusSDKWalletPanelFactory.CreateText("Address", panelObject.transform, font, "Not connected", 22, new Color(0.75f, 0.86f, 1f, 1f), TextAnchor.MiddleLeft);
		textAddress.horizontalOverflow = HorizontalWrapMode.Wrap;
		GeniusSDKWalletPanelFactory.SetLayout(textAddress.gameObject, -1f, 58f);

		Text payoutLabel = GeniusSDKWalletPanelFactory.CreateText("PayoutLabel", panelObject.transform, font, "Set Payout Address", 20, Color.white, TextAnchor.MiddleLeft);
		GeniusSDKWalletPanelFactory.SetLayout(payoutLabel.gameObject, -1f, 28f);

		GameObject payoutRow = GeniusSDKWalletPanelFactory.CreateRow("PayoutRow", panelObject.transform, 58f);
		InputField inputPayout = GeniusSDKWalletPanelFactory.CreateInputField("InputPayoutAddress", payoutRow.transform, font, "0x...");
		Button btnSetPayout = GeniusSDKWalletPanelFactory.CreateButton("BtnSetPayout", payoutRow.transform, font, "Set", new Color(0.05f, 0.39f, 0.75f, 1f), 120f);

		Text mnemonicLabel = GeniusSDKWalletPanelFactory.CreateText("MnemonicLabel", panelObject.transform, font, "Add Account from Mnemonic", 20, Color.white, TextAnchor.MiddleLeft);
		GeniusSDKWalletPanelFactory.SetLayout(mnemonicLabel.gameObject, -1f, 28f);

		InputField inputMnemonic = GeniusSDKWalletPanelFactory.CreateInputField("InputMnemonic", panelObject.transform, font, "Mnemonic phrase");
		inputMnemonic.lineType = InputField.LineType.MultiLineNewline;
		GeniusSDKWalletPanelFactory.SetLayout(inputMnemonic.gameObject, -1f, 92f);

		Button btnAddMnemonic = GeniusSDKWalletPanelFactory.CreateButton("BtnAddMnemonic", panelObject.transform, font, "Add Account", new Color(0.04f, 0.52f, 0.40f, 1f), -1f);
		Text textStatus = GeniusSDKWalletPanelFactory.CreateText("Status", panelObject.transform, font, "", 20, new Color(0.56f, 1f, 0.72f, 1f), TextAnchor.MiddleLeft);
		textStatus.horizontalOverflow = HorizontalWrapMode.Wrap;
		GeniusSDKWalletPanelFactory.SetLayout(textStatus.gameObject, -1f, 42f);

		GeniusSDKWalletPanel panel = panelObject.AddComponent<GeniusSDKWalletPanel>();
		panel.Initialize(textAddress, inputPayout, btnSetPayout, inputMnemonic, btnAddMnemonic, textStatus);
		return panel;
	}

	private static GameObject CreateRow(string name, Transform parent, float preferredHeight)
	{
		GameObject row = GeniusSDKWalletPanelFactory.CreateRect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
		HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
		layout.spacing = 10f;
		layout.childAlignment = TextAnchor.MiddleCenter;
		layout.childForceExpandWidth = false;
		layout.childForceExpandHeight = true;
		GeniusSDKWalletPanelFactory.SetLayout(row, -1f, preferredHeight);
		return row;
	}

	private static Text CreateText(string name, Transform parent, Font font, string value, int size, Color color, TextAnchor alignment)
	{
		GameObject obj = GeniusSDKWalletPanelFactory.CreateRect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
		Text text = obj.AddComponent<Text>();
		text.font = font;
		text.text = value;
		text.fontSize = size;
		text.color = color;
		text.alignment = alignment;
		text.raycastTarget = false;
		return text;
	}

	private static InputField CreateInputField(string name, Transform parent, Font font, string placeholder)
	{
		GameObject obj = GeniusSDKWalletPanelFactory.CreateRect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
		Image image = obj.AddComponent<Image>();
		image.color = Color.white;
		InputField input = obj.AddComponent<InputField>();
		input.textComponent = GeniusSDKWalletPanelFactory.CreateText("Text", obj.transform, font, "", 20, new Color(0.05f, 0.08f, 0.12f, 1f), TextAnchor.MiddleLeft);
		input.placeholder = GeniusSDKWalletPanelFactory.CreateText("Placeholder", obj.transform, font, placeholder, 20, new Color(0.42f, 0.46f, 0.52f, 1f), TextAnchor.MiddleLeft);
		GeniusSDKWalletPanelFactory.InsetText(input.textComponent.GetComponent<RectTransform>(), 16f, 8f);
		GeniusSDKWalletPanelFactory.InsetText(((Text)input.placeholder).GetComponent<RectTransform>(), 16f, 8f);
		GeniusSDKWalletPanelFactory.SetLayout(obj, -1f, 54f);
		return input;
	}

	private static Button CreateButton(string name, Transform parent, Font font, string label, Color color, float preferredWidth)
	{
		GameObject obj = GeniusSDKWalletPanelFactory.CreateRect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
		Image image = obj.AddComponent<Image>();
		image.color = color;
		Button button = obj.AddComponent<Button>();
		Text text = GeniusSDKWalletPanelFactory.CreateText("Text", obj.transform, font, label, 22, Color.white, TextAnchor.MiddleCenter);
		GeniusSDKWalletPanelFactory.InsetText(text.GetComponent<RectTransform>(), 12f, 4f);
		GeniusSDKWalletPanelFactory.SetLayout(obj, preferredWidth, 54f);
		return button;
	}

	private static GameObject CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPosition)
	{
		GameObject obj = new GameObject(name);
		obj.transform.SetParent(parent, false);
		obj.layer = parent.gameObject.layer;
		RectTransform rect = obj.AddComponent<RectTransform>();
		rect.anchorMin = anchorMin;
		rect.anchorMax = anchorMax;
		rect.pivot = pivot;
		rect.sizeDelta = sizeDelta;
		rect.anchoredPosition = anchoredPosition;
		return obj;
	}

	private static void InsetText(RectTransform rect, float horizontal, float vertical)
	{
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = new Vector2(horizontal, vertical);
		rect.offsetMax = new Vector2(-horizontal, -vertical);
	}

	private static void SetLayout(GameObject obj, float preferredWidth, float preferredHeight)
	{
		LayoutElement layoutElement = obj.GetComponent<LayoutElement>();
		if (layoutElement == null)
		{
			layoutElement = obj.AddComponent<LayoutElement>();
		}

		layoutElement.preferredWidth = preferredWidth;
		layoutElement.preferredHeight = preferredHeight;
	}
}
