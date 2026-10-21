using System.Collections.Generic;
using UnityEngine;

public class BossSequencerPlayer : MonoBehaviour
{
    [SerializeField] private BossTimelineSequence sequence;
    [SerializeField] private BossController boss;

    private float playhead = 0f;
    private bool isPlaying = false;
    private HashSet<string> executedClips = new HashSet<string>();

    public float CurrentPlayhead => playhead;
    public bool IsPlaying => isPlaying;

    public void PlayWave(BossTimelineSequence newSequence)
    {
        sequence = newSequence;
        playhead = 0f;
        executedClips.Clear();
        isPlaying = true;
    }

    public void StopWave()
    {
        isPlaying = false;
        playhead = 0f;
        executedClips.Clear();
    }

    private void Update()
    {
        if (!isPlaying || sequence == null) return;

        playhead += Time.deltaTime;

        foreach (var track in sequence.tracks)
        {
            if (track == null || track.clips == null) continue;

            foreach (var clip in track.clips)
            {
                if (clip == null) continue;

                if (playhead >= clip.startTime && !executedClips.Contains(clip.id))
                {
                    executedClips.Add(clip.id);
                    ExecuteClip(clip);
                }
            }
        }

        if (playhead >= sequence.totalDuration)
        {
            isPlaying = false;
            Debug.Log("[BossSequencerPlayer] Wave terminée !");
        }
    }

    private void ExecuteClip(TimelineClip clip)
    {
        switch (clip.trackType)
        {
            case TrackType.BossAction:
                if (clip.patternData != null && boss != null)
                {
                    Debug.Log($"[Sequencer] Boss exécute pattern : {clip.patternData.patternName}");
                    boss.animator?.SetTrigger(clip.patternData.animationTrigger);

                    var demoBoss = boss.GetComponent<DemoBossController>();
                    if (demoBoss != null) demoBoss.OnPatternExecute(clip.patternData);
                }
                break;

            case TrackType.MinionWave:
                Debug.Log($"[Sequencer] Vague d'adds : {clip.spawnCount} ennemis");
                if (clip.minionPrefab != null && boss != null)
                {
                    for (int i = 0; i < clip.spawnCount; i++)
                    {
                        Vector3 offset = new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
                        Instantiate(clip.minionPrefab, boss.transform.position + offset, Quaternion.identity);
                    }
                }
                break;

            case TrackType.ArenaHazard:
                Debug.Log($"[Sequencer] Piège/Danger d'arène : {clip.clipName} actif pendant {clip.duration}s");
                break;
        }
    }
}
