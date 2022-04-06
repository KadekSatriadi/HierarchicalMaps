using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    AudioSource audioSource;
    // Start is called before the first frame update
    public List<AudioClip> sounds;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(int idx)
    {
        audioSource.Stop();
        audioSource.clip = sounds[idx];
        audioSource.Play();
    }
}
