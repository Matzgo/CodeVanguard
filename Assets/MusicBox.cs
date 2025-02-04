using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBox : MonoBehaviour
{
    [SerializeField]
    public List<AudioClip> musicTracks; // List of music tracks
    [SerializeField]
    private AudioSource audioSource;
    private int currentTrackIndex = 0;

    void Start()
    {
        audioSource.loop = false;
        if (musicTracks.Count > 0)
        {
            StartCoroutine(PlayMusicSequence());
        }
    }

    private IEnumerator PlayMusicSequence()
    {
        while (true)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = musicTracks[currentTrackIndex];
                audioSource.Play();
                currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Count;
            }
            yield return null;
        }
    }
}
