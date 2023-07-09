using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAudioPlay : MonoBehaviour
{
    public int soundIndex;

    public static List<int> currentlyPlaying;

    void Awake()
    {
        if (currentlyPlaying == null) currentlyPlaying = new List<int>();
    }
    
    void Start()
    {
        StartCoroutine(RandomPlay());
    }
    
    void Update()
    {
        
    }

    IEnumerator RandomPlay(){
        while(true){
            yield return new WaitForSeconds(Random.Range(5f, 60f));
            if (!currentlyPlaying.Contains(soundIndex)){
                currentlyPlaying.Add(soundIndex);
                GetComponent<AudioSource>().Play();
                yield return new WaitForSeconds(GetComponent<AudioSource>().clip.length);
                currentlyPlaying.Remove(soundIndex);
            }
        }
    }
}
