using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public enum AudioValue : int
    {
        Shoot = 0,
        Hit = 1,
        Get = 2,
        BGM = 3
    };
    public static SoundManager instance;
    AudioSource source;
    public AudioClip[] audios;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        source = GetComponent<AudioSource>();
        source.Play();
        source.loop = true;
    }


    void Update()
    {

    }

    public void AudioStart(AudioValue value)
    {
        source.PlayOneShot(audios[(int)value]);
    }

    public void PlayAudio(AudioClip clip)
    {
        source.PlayOneShot(clip);
    }
    public void LoopMusic()
    {
        source.Play();
        source.loop = true;
    }
}
