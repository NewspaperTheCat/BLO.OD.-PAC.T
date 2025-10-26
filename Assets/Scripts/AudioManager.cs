using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton Design Pattern (to a degree)
    public static AudioManager inst;
    void OnEnable()
    {
        // ensures there is only ever one Selector
        if (inst == null)
        {
            inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] GameObject sfxPrefab;
    String basePath = "Audio/";

    // Audio Lists
    String keyPressPath = "Keys/";
    List<AudioClip> keyPressClips = new List<AudioClip>();

    // reads all paths into list
    void Awake() {
        keyPressClips.AddRange(Resources.LoadAll(basePath + keyPressPath, typeof(AudioClip)).Cast<AudioClip>());
        Debug.Log(keyPressClips.Count());
    }

    private void PlayRandomSoundFromList(List<AudioClip> list)
    {
        AudioSource aus = Instantiate(sfxPrefab).GetComponent<AudioSource>();
        aus.clip = list[UnityEngine.Random.Range(0, list.Count())];
        aus.pitch = UnityEngine.Random.Range(0.75f, 1.25f);
        aus.Play();
        Destroy(aus.gameObject, aus.clip.length + .1f);
    }

    public void PlayRandomKeyPress() { PlayRandomSoundFromList(keyPressClips); }
}
