using UnityEngine;

/// <summary>
/// Attach to the ImageIcon GameObject under GamUIManager/SafeArea/UICoin/ContentsDiamond.
/// Plays the Animator only while GeniusSDK is actively processing; otherwise freezes on frame 0.
/// Polls the SDK status at a fixed interval rather than every frame.
/// </summary>
[RequireComponent(typeof(Animator))]
public class GeniusProcessingAnimator : MonoBehaviour
{
    [SerializeField] private float pollInterval = 0.5f;

    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        InvokeRepeating(nameof(CheckProcessingStatus), 0f, pollInterval);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(CheckProcessingStatus));
    }

    private void CheckProcessingStatus()
    {
        if (GeniusSDKWrapper.Instance == null)
            return;

        bool isProcessing =
            GeniusSDKWrapper.Instance.GetProcessingStatus().status ==
            GeniusSDKWrapper.GeniusProcessingStatus.GENIUS_PR_STATUS_PROCESSING;

        if (isProcessing)
        {
            if (_animator.speed == 0f)
                _animator.speed = 1f;
        }
        else
        {
            if (_animator.speed != 0f)
            {
                _animator.speed = 0f;
                _animator.Play(0, 0, 0f); // snap to first frame
            }
        }
    }
}
