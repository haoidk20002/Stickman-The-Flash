using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMixer : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    // Start is called before the first frame update
    public void SetMusicVolume(float level){
        audioMixer.SetFloat("Music", Mathf.Log10(level)*20);
    }
    public void SetSFXVolume(float level){
        audioMixer.SetFloat("SFX", Mathf.Log10(level)*20);
    }
}
