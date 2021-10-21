using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCommandListener : MonoBehaviour
{
    private string _device;
    AudioClip _clipRecord;
    void Start()
    {
        // foreach (var device in Microphone.devices){
        //     Debug.Log("Mic name: " +device);
        // }

        //  _device = Microphone.devices[1];
        //  Debug.Log("Selected: " + _device.ToString()); 



        //  //https://docs.unity3d.com/ScriptReference/Microphone.Start.html

        //  AudioSource audioSource = GetComponent<AudioSource>();
        //    audioSource.clip = Microphone.Start(_device, false, 10, 44100);
        //    audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
// Debug.Log("Volume is " + MicInput.MicLoudness.ToString("##.#####"));        
    }
}
