using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public enum AudioPlaybackMode
{
    RandomOneShot,
    Sequential,
    Simultaneous
}

[System.Serializable]
public class AudioFeedbackEntry
{
    [HorizontalGroup("ClipGroup", Width = 0.6f)]
    public AudioClip clip;

    [HorizontalGroup("ClipGroup", Width = 0.4f)]
    [Range(0f, 2f)]
    public float volumeMultiplier = 1.0f;

    [MinMaxSlider(0.5f, 2.0f, ShowFields = true)]
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    [MinValue(0f)]
    public float delay = 0f;
}

[CreateAssetMenu(fileName = "AudioFeedback_New", menuName = "Boss/Feedbacks/Audio Feedback Definition")]
public class AudioFeedbackDefinition : SerializedScriptableObject
{
    [Title("Audio Settings")]
    public AudioPlaybackMode playbackMode = AudioPlaybackMode.RandomOneShot;

    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Range(0f, 1f)]
    public float spatialBlend = 1f; // 3D sound

    [TableList(ShowPaging = true)]
    public List<AudioFeedbackEntry> audioClips = new List<AudioFeedbackEntry>();

    public AudioClip GetClip(ref int sequenceIndex)
    {
        if (audioClips == null || audioClips.Count == 0) return null;

        switch (playbackMode)
        {
            case AudioPlaybackMode.RandomOneShot:
                return audioClips[UnityEngine.Random.Range(0, audioClips.Count)].clip;

            case AudioPlaybackMode.Sequential:
                var clip = audioClips[sequenceIndex % audioClips.Count].clip;
                sequenceIndex++;
                return clip;

            default:
                return audioClips[0].clip;
        }
    }
}
