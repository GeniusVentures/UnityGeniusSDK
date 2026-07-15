using System.Collections;
using UnityEngine;

/// <summary>
/// Re-registers the saved payout address with the native SDK on every launch.
///
/// This exists because SetPayoutAddress is write-only: the native API has no
/// GetPayoutAddress, so the game can never read back what native holds and cannot know
/// whether native persisted the address across sessions. Re-applying unconditionally is
/// idempotent and correct either way (D-05).
///
/// Drop it on a GameObject next to the GeniusSDKWrapper. It has no inspector surface and
/// no public API.
///
/// This is the upstreamable SDK base. It references no specific game's types: the payout
/// address is reached only through the protected virtual persistence hooks below, whose
/// defaults read and write the same PlayerPrefs key a host game's accessor wraps. A game
/// overrides them to route through its own persistence.
/// </summary>
public class GeniusPayoutBootstrap : MonoBehaviour
{
	// Generous: a cold first init on a slow device legitimately takes tens of seconds. This
	// exists to catch a node that is wedged, not one that is merely slow.
	private const float InitTimeoutSeconds = 90f;

	// ---- extensibility hooks --------------------------------------------------------
	//
	// The only coupling this component has to a host game is persistence. These hooks abstract
	// it so the base references no game type; the DEFAULTS below are byte-identical to the
	// game's own accessor -- the same PlayerPrefs key, and no Save() -- so behavior is preserved
	// whether or not a subclass overrides them.

	/// <summary>
	/// Reads the stored payout address. Default: the game's persistence key in PlayerPrefs.
	/// </summary>
	protected virtual string LoadPayoutAddress()
	{
		return PlayerPrefs.GetString("GeniusPayoutAddress", "");
	}

	/// <summary>
	/// Persists a payout address. Default: the game's persistence key in PlayerPrefs, with no
	/// PlayerPrefs.Save() -- identical to the game's own accessor, which does not call Save().
	/// </summary>
	protected virtual void SavePayoutAddress(string address)
	{
		PlayerPrefs.SetString("GeniusPayoutAddress", address);
	}

	private IEnumerator Start()
	{
		// Start the SDK HERE, at boot, rather than leaving it to the popup.
		//
		// The native init now runs on a background thread, so this no longer freezes anything --
		// but the node still takes tens of seconds to come up, and the popup cannot register a
		// payout address until it is past database migration. Starting at boot means that clock
		// is already running while the player reads the popup, instead of starting when they
		// press a button and making them wait for it.
		//
		// BeginInitialize is idempotent, so the popup's own call is a no-op.
		GeniusSDKWrapper.Instance.BeginInitialize(Application.persistentDataPath + "/");


		// Read once, up front, before any waiting. That is what makes this component a
		// re-applier and not a second, racing writer: on a first-connect launch the value
		// is still empty at this moment, so we no-op and GeniusSDKConnectScreen owns the
		// registration. On every later launch the value is present and we own it.
		// Normalized on read as well as on write: a value stored by an older build (or edited
		// by hand) may carry a "0x" the SDK will not take.
		string saved = GeniusPayoutAddress.Normalize(this.LoadPayoutAddress());
		bool hasPayoutAddress = GeniusPayoutAddress.IsValid(saved);

		// The stored value is untrusted input. PlayerPrefs is unencrypted and writable by the
		// user and by any local process, so a value coming out of it gets exactly the same
		// validation as a value coming out of an InputField.
		if (!hasPayoutAddress && !string.IsNullOrEmpty(saved))
		{
			// Non-empty but malformed: the store was tampered with or corrupted. Clear it so
			// the player is prompted again rather than silently paying out nowhere. Log the
			// length, never the value.
			Debug.LogError("GeniusSDK: the stored payout address was rejected by validation (length " + saved.Length + ") and has been cleared.");
			this.SavePayoutAddress(string.Empty);
		}

		// Waited for even when there is no payout address to re-apply. The SDK creates the
		// game's own wallet on init whether or not the player linked a destination, and this
		// is the only place that reports it -- without which "did a wallet get created when I
		// skipped?" is unanswerable from inside the game.
		//
		// Wait until the node is SAFE to call. Calling SetPayoutAddress during the SDK's database
		// migration returns GENIUS_NODE_RET_OK and then kills the process with an access
		// violation on an SDK background thread, seconds later -- the return code gives no
		// warning. Verified against the shipped DLL: fatal in MIGRATING_DATABASE, healthy from
		// INITIALIZING_PROCESSING onward. Gate on the node's state, not on IsReady: a node that
		// cannot reach a full node may never become ready at all.
		//
		// Bounded and slow-polled: unbounded, this coroutine outlives the app against a node
		// whose data directory is corrupt and never finishes; at one tick per frame it would
		// check sixty times a second for a flag that changes at most once.
		float deadline = Time.realtimeSinceStartup + InitTimeoutSeconds;

		while (GeniusSDKWrapper.Instance == null || !GeniusSDKWrapper.Instance.IsSafeForAccountCalls)
		{
			if (Time.realtimeSinceStartup > deadline)
			{
				Debug.LogError("GeniusSDK: the node never got past database initialization within " +
					InitTimeoutSeconds + "s, so the saved payout address was NOT re-applied. Earnings may not " +
					"reach the linked wallet this session. Its data directory may be corrupt.");
				yield break;
			}

			yield return new WaitForSecondsRealtime(0.5f);
		}

		this.ReportNodeWallet();

		if (!hasPayoutAddress)
		{
			Debug.LogWarning("GeniusSDK: no payout wallet is linked. The game's own wallet exists and will accrue earnings, but there is no destination to pay them out to.");
			yield break;
		}

		GeniusSDKWrapper.GeniusNodeReturnValue result;

		try
		{
			result = GeniusSDKWrapper.Instance.SetPayoutAddress(saved);
		}
		catch (System.Exception ex)
		{
			// Android, iOS and Linux still ship native binaries with no such export, so the
			// P/Invoke throws there. Unhandled, it would kill this coroutine silently.
			Debug.LogError("GeniusSDK: SetPayoutAddress threw while re-applying the saved address: " + ex.Message);
			yield break;
		}

		if (result == GeniusSDKWrapper.GeniusNodeReturnValue.GENIUS_NODE_RET_OK)
		{
			Debug.Log("GeniusSDK: saved payout address re-applied -- earnings pay out to " + saved);
		}
		else
		{
			// Never swallow this. A silent failure leaves the player believing their previous
			// session's wallet is still linked when it is not.
			Debug.LogError("GeniusSDK: failed to re-apply the saved payout address: " + result);
		}
	}

	/// <summary>
	/// Logs the game's OWN wallet -- the Genius node address the SDK creates on init, with a
	/// random mnemonic, whether or not the player ever linked a payout destination.
	///
	/// This is the answer to "is there actually a wallet in there when I skip?". It is a
	/// read-only diagnostic: the node address is not a payout address and is never a valid
	/// value for one.
	/// </summary>
	private void ReportNodeWallet()
	{
		try
		{
			GeniusSDKWrapper.GeniusAddress address = GeniusSDKWrapper.Instance.GetAddress();

			if (string.IsNullOrEmpty(address.address) || !address.address.StartsWith("0x"))
			{
				Debug.LogError("GeniusSDK: the SDK is ready but reports no wallet address. Expected one to be created on init.");
				return;
			}

			Debug.Log("GeniusSDK: game wallet (this device) = " + address.address);
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("GeniusSDK: could not read the game's wallet address: " + ex.Message);
		}
	}
}
