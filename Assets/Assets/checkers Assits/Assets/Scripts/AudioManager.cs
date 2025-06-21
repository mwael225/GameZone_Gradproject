using UnityEngine;
using Unity.Netcode;

public class AudioManager : NetworkBehaviour
{
    [Header("Audio Sources")]
    public AudioSource effectsSource;

    [Header("Sound Effects")]
    public AudioClip pieceMove;
    public AudioClip pieceCapture;
    public AudioClip kingPromotion;
    public AudioClip gameWin;

    // Singleton pattern
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this from any client, will be executed on all clients
    [ClientRpc]
    public void PlaySoundClientRpc(SoundType soundType)
    {
        AudioClip clipToPlay = null;
        
        switch (soundType)
        {
            case SoundType.Move:
                clipToPlay = pieceMove;
                break;
            case SoundType.Capture:
                clipToPlay = pieceCapture;
                break;
            case SoundType.KingPromotion:
                clipToPlay = kingPromotion;
                break;
            case SoundType.Win:
                clipToPlay = gameWin;
                break;
        }

        if (clipToPlay != null && effectsSource != null)
        {
            effectsSource.PlayOneShot(clipToPlay);
        }
    }
}

public enum SoundType
{
    Move,
    Capture,
    KingPromotion,
    Win
} 