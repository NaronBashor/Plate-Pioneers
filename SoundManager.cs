using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public List<AudioSource> audioSources; // List of audio sources

    private void Awake()
    {
        // Ensure this is the only instance of SoundManager
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep across scenes
        } else {
            Destroy(gameObject); // Destroy if there's a duplicate
        }
    }

    public void OnButtonClick()
    {
        PlaySound(0, false);
    }

    // Play sound by index (e.g., from audioSources list)
    public void PlaySound(int index, bool loop = false)
    {
        if (index >= 0 && index < audioSources.Count) {
            audioSources[index].loop = loop;  // Set the loop option
            audioSources[index].Play();
        } else {
            Debug.LogWarning("Invalid audio source index: " + index);
        }
    }

    // Stop sound by index
    public void StopSound(int index)
    {
        if (index >= 0 && index < audioSources.Count) {
            audioSources[index].Stop();
            audioSources[index].loop = false;  // Ensure the loop is disabled after stopping
        } else {
            Debug.LogWarning("Invalid audio source index: " + index);
        }
    }

    // Play sound by name (using a dictionary) with an optional loop parameter
    public void PlaySoundByName(string soundName, bool loop = false)
    {
        foreach (AudioSource source in audioSources) {
            if (source.clip.name == soundName) {
                source.loop = loop;  // Set the loop option
                source.Play();
                return;
            }
        }
        Debug.LogWarning("Sound with name " + soundName + " not found.");
    }

    // Example to stop a sound by name
    public void StopSoundByName(string soundName)
    {
        foreach (AudioSource source in audioSources) {
            if (source.clip.name == soundName) {
                source.Stop();
                source.loop = false; // Ensure loop is disabled after stopping
                return;
            }
        }
        Debug.LogWarning("Sound with name " + soundName + " not found.");
    }
}
