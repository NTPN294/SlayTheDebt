using UnityEngine;

public class MusicScript : MonoBehaviour
{
    public AudioClip audioClip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not found! Add an AudioSource to this GameObject.");
            return;
        }
        if (audioClip == null)
        {
            Debug.LogError("No audio clip assigned!");
            return;
        }
        
        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.Play();
        Debug.Log("Audio playing: " + audioClip.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
