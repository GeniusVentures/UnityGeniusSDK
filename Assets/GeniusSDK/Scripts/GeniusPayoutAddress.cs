using System.Text.RegularExpressions;

/// <summary>
/// The single validation gate for the player's payout address.
///
/// FORMAT: exactly 128 hex characters -- a 64-byte public key -- with no "0x" prefix.
///
/// This is not a guess. It was established by calling the shipped native DLL directly and
/// recording what GeniusSDKSetPayoutAddress returns:
///
///   128 hex, no prefix        -> GENIUS_NODE_RET_OK          (any case; any 128-hex value)
///   128 hex WITH "0x"         -> GENIUS_NODE_INVALID_ARGUMENT
///   Ethereum "0x" + 40 hex    -> GENIUS_NODE_INVALID_ARGUMENT
///   40 hex, no prefix         -> GENIUS_NODE_INVALID_ARGUMENT
///   127 hex / non-hex / empty -> GENIUS_NODE_INVALID_ARGUMENT
///
/// An earlier revision of this file enforced "0x" + 40 hex (an Ethereum address). That
/// rejected every address the SDK accepts and accepted only ones it refuses, so the feature
/// could not have worked on any platform. Do not "fix" this back without re-running that probe.
///
/// The player's own GeniusWallet displays this key; whether it shows it with a leading "0x" is
/// outside our control, so Normalize() strips one if present. The SDK is handed the bare hex.
///
/// The native side performs NO checksum and NO account-existence check -- it accepts any
/// 128-hex string, including all zeroes. This validator is therefore the only thing standing
/// between a typo and a payout address that silently pays nobody. It runs before every SDK
/// call, and again on every value read back out of CacheGame.PayoutAddress, because
/// PlayerPrefs is unencrypted and writable by the user and by any local process.
/// </summary>
public static class GeniusPayoutAddress
{
	// Built once and reused. RegexOptions.Compiled is deliberately NOT set: it emits a
	// DynamicMethod via System.Reflection.Emit, which IL2CPP (iOS/Android) does not support,
	// and in a static readonly field a throw would surface as a TypeInitializationException
	// the first time anything touched the validator on device.
	//
	// \A and \z (not ^ and $) are load-bearing: in .NET, "$" also matches immediately before a
	// trailing newline, so an anchored-with-$ pattern would accept "…hex\n" and forward the
	// newline to the SDK. Values read out of PlayerPrefs are not trimmed by anyone.
	private static readonly Regex PayoutKeyPattern = new Regex(@"\A[0-9a-fA-F]{128}\z");

	/// <summary>
	/// User-facing copy shown in the status Text when validation rejects an address.
	/// Deliberately says nothing about the format: a player who mistypes does not need a spec,
	/// and one being socially engineered into pasting the wrong thing is not helped by us
	/// teaching them what a valid one looks like.
	/// </summary>
	public const string InvalidMessage = "That doesn't look like a wallet address. Copy it from your GeniusWallet and paste it here.";

	/// <summary>
	/// Strips a leading "0x" and surrounding whitespace, producing the exact string the SDK
	/// expects. Returns an empty string for null input. Does NOT validate -- call IsValid on
	/// the result before handing it to native.
	/// </summary>
	public static string Normalize(string address)
	{
		if (string.IsNullOrEmpty(address))
		{
			return string.Empty;
		}

		string trimmed = address.Trim();

		if (trimmed.Length > 2 && (trimmed[0] == '0') && (trimmed[1] == 'x' || trimmed[1] == 'X'))
		{
			trimmed = trimmed.Substring(2);
		}

		return trimmed;
	}

	/// <summary>
	/// True only for exactly 128 hex characters, case-insensitive, with no prefix. Null, empty,
	/// whitespace, a wrong length, a leftover "0x", and any non-hex character all return false.
	/// Never throws.
	///
	/// Pass it a Normalize()d value: this checks the SDK's form, not the player's paste.
	/// </summary>
	public static bool IsValid(string address)
	{
		if (string.IsNullOrEmpty(address))
		{
			return false;
		}

		return PayoutKeyPattern.IsMatch(address);
	}
}
