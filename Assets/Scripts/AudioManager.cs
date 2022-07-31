using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioClip> songs;

    public static AudioManager instance { get; private set; }

    private AudioSource _source;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }

    private void Start()
    {
        _source = GetComponent<AudioSource>();
    }

    public void PlaySongsRandomly()
    {
        if (_source.isPlaying) return;
        
        Random random = new Random();
        List<AudioClip> random_songs = songs.OrderBy(item => random.Next()).ToList();

        StartCoroutine(PlayAllContinuously(random_songs));
    }

    private IEnumerator PlayAllContinuously(List<AudioClip> songs)
    {
        int song_index = 0;

        while (true)
        {
            if (song_index >= songs.Count) song_index = 0;
            AudioClip song = songs[song_index++];
            _source.clip = song;
            _source.Play();

            yield return new WaitForSeconds(song.length);
        }
    }
}
