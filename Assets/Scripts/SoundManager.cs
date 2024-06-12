using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public AudioClip[] SoundEffects;
    [SerializeField] private AudioSource soundFXObject;

    private void Awake(){
        if (Instance == null){
            Instance = this;
        }
    }
    // what is LLM?
    // Fine-tuning LLM
    public void PlaySoundEffect(int number, Transform transform, float volume){
        AudioSource audioSource = Instantiate(soundFXObject, transform.position, Quaternion.identity);
        audioSource.clip = SoundEffects[number];
        audioSource.volume = volume;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject,clipLength);
    }
}
