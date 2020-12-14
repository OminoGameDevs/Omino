using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Sound {
    public AudioSource source;

    public Sound(AudioManager mgr, AudioClip clip)
    {
        source = mgr.gameObject.AddComponent<AudioSource>();
        source.clip = clip;
    }
}
