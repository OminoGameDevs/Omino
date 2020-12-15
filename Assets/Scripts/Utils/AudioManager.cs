using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    private Sound[] sounds;

    private void Awake()
    {
        instance = this;

        var clips = ResourceLoader.GetAll<AudioClip>();
        sounds = new Sound[clips.Length];
        for (int i = 0; i < clips.Length; ++i)
            sounds[i] = new Sound(this, clips[i]);

        //foreach (Sound s in sounds)
        //{
        //    s.source = gameObject.AddComponent<AudioSource>();
        //    s.source.clip = s.clip;
        //    s.source.volume = s.volume;
        //    s.source.pitch = s.pitch;
        //    s.source.loop = s.isLoop;
        //    s.source.panStereo = s.stereo;
        //}
    }

    public static void PlaySound(string name, float minPitch, float maxPitch)
    {
        if (!instance) return;

        Sound s = System.Array.Find(instance.sounds, sound => sound.source.clip.name.Equals(name));
        if (s == null) return;
        s.source.pitch = Random.Range(minPitch, maxPitch);
        s.source.Play();
    }
    public static void PlaySound(string name, float minPitch = 1) => PlaySound(name, minPitch, minPitch);

    public static void StopSound(string name)
    {
        if (!instance) return;

        Sound s = System.Array.Find(instance.sounds, sound => sound.source.clip.name.Equals(name));
        if (s == null) return;
        s.source.Stop();
    }
}
