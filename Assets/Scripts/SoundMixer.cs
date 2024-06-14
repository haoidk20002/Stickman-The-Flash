using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMixer : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    public void SetMusicVolume(float level){
        GameManager.Instance._settings.MusicLevel = level;
        audioMixer.SetFloat("Music", Mathf.Log10(level)*20);
    }
    public void SetSFXVolume(float level){
        GameManager.Instance._settings.SFXLevel = level;
        audioMixer.SetFloat("SFX", Mathf.Log10(level)*20);
    }
}
