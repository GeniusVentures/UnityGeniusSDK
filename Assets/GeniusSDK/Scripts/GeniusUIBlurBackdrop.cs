using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A frosted-glass backdrop for a modal: snapshots the screen, blurs it, and displays the
/// result behind the dialog.
///
/// A snapshot rather than a live effect. The game's canvases are Screen Space - Overlay,
/// which is composited after every camera finishes, so the usual GrabPass trick has nothing
/// to grab and renders as garbage on most mobile GPUs. And a modal's background does not need
/// to animate -- blurring once when the dialog opens costs a single frame instead of one blur
/// per frame forever.
///
/// The capture is DRIVEN BY THE CALLER, before the modal is shown, and it never touches the
/// dialog's active state. An earlier version hid the dialog, captured, then un-hid it -- so a
/// capture that failed or stalled left the dialog hidden permanently while the modal's input
/// blocker went on swallowing every click. A cosmetic effect must never be able to take the UI
/// down with it. Capture() is best-effort by construction: it swallows its own failures, and
/// the caller shows the modal either way.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class GeniusUIBlurBackdrop : MonoBehaviour
{
	[Header("Blur")]
	[SerializeField] private Material blurMaterial;

	// Halving the resolution before blurring is most of the cheapness, and costs nothing
	// visually: the detail being thrown away was about to be destroyed by the blur anyway.
	// 2 == quarter width and height.
	[SerializeField] private int downsample = 2;

	private RenderTexture blurred;

	/// <summary>
	/// Snapshots the screen and blurs it into this RawImage.
	///
	/// Call this while the modal is still HIDDEN -- it captures whatever is on screen, so a
	/// visible dialog would be baked into its own backdrop. Never throws: a failure logs and
	/// leaves a flat tinted backdrop, so the caller can show the modal unconditionally.
	/// </summary>
	public IEnumerator Capture()
	{
		// The screen is only complete at the end of the frame. Capturing earlier yields a
		// partially drawn frame.
		yield return new WaitForEndOfFrame();

		// No yields below, so this can be a real try/catch.
		try
		{
			RawImage image = this.GetComponent<RawImage>();

			int width = Mathf.Max(1, Screen.width >> this.downsample);
			int height = Mathf.Max(1, Screen.height >> this.downsample);

			RenderTexture screen = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
			ScreenCapture.CaptureScreenshotIntoRenderTexture(screen);

			RenderTexture horizontal = RenderTexture.GetTemporary(width, height, 0);

			this.Release();
			this.blurred = new RenderTexture(width, height, 0);

			if (this.blurMaterial == null)
			{
				Debug.LogWarning("GeniusUIBlurBackdrop: no blur material assigned; the backdrop will not be blurred.");
				Graphics.Blit(screen, this.blurred);
			}
			else
			{
				// Separable: horizontal pass into one target, vertical into the next.
				Graphics.Blit(screen, horizontal, this.blurMaterial, 0);
				Graphics.Blit(horizontal, this.blurred, this.blurMaterial, 1);
			}

			RenderTexture.ReleaseTemporary(horizontal);
			RenderTexture.ReleaseTemporary(screen);

			image.texture = this.blurred;

			// CaptureScreenshotIntoRenderTexture writes in the graphics API's native
			// orientation: bottom-up on OpenGL/Vulkan, top-down on D3D/Metal. Unhandled, the
			// backdrop is upside down on roughly half the platforms the game ships to -- and,
			// being a blur, it looks merely "wrong" rather than obviously flipped, so it is
			// the kind of bug that ships.
			image.uvRect = SystemInfo.graphicsUVStartsAtTop
				? new Rect(0f, 1f, 1f, -1f)
				: new Rect(0f, 0f, 1f, 1f);

			Debug.Log("GeniusUIBlurBackdrop: captured " + Screen.width + "x" + Screen.height +
				", blurred at " + width + "x" + height);
		}
		catch (System.Exception ex)
		{
			// Swallowed on purpose. The modal must open whether or not it is pretty.
			Debug.LogError("GeniusUIBlurBackdrop: capture failed; falling back to a flat backdrop. " + ex);
		}
	}

	private void OnDisable()
	{
		this.Release();
	}

	/// <summary>
	/// RenderTextures are unmanaged. Left to the GC these leak a screen-sized buffer every
	/// time the modal is reopened.
	/// </summary>
	private void Release()
	{
		if (this.blurred == null)
		{
			return;
		}

		this.blurred.Release();
		Destroy(this.blurred);
		this.blurred = null;
	}
}
