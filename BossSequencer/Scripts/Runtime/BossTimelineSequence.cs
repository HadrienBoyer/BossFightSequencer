using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum TrackType
{
    BossAction,
    MinionWave,
    ArenaHazard
}

[Serializable]
public class TimelineClip
{
    [HideInInspector] public string id = Guid.NewGuid().ToString();

    [HorizontalGroup("Header", Width = 0.6f)]
    public string clipName = "New Clip";

    [HorizontalGroup("Header", Width = 0.4f)]
    public Color clipColor = new Color(0.2f, 0.6f, 0.85f, 0.9f);

    [BoxGroup("Timing")]
    [MinValue(0f)] public float startTime = 0f;

    [BoxGroup("Timing")]
    [MinValue(0.2f)] public float duration = 2.0f;

    public float EndTime => startTime + duration;

    [ShowIf("@trackType == TrackType.BossAction")]
    public BossPatternData patternData;

    [ShowIf("@trackType == TrackType.MinionWave")]
    public GameObject minionPrefab;

    [ShowIf("@trackType == TrackType.MinionWave")]
    public int spawnCount = 3;

    [HideInInspector] public TrackType trackType;
}

[Serializable]
public class TimelineTrack
{
    [HorizontalGroup("TrackInfo", Width = 0.7f)]
    public string trackName = "Piste";

    [HorizontalGroup("TrackInfo", Width = 0.3f)]
    public TrackType type = TrackType.BossAction;

    [ListDrawerSettings(Expanded = false)]
    public List<TimelineClip> clips = new List<TimelineClip>();
}

[CreateAssetMenu(fileName = "NewBossSequence", menuName = "Boss/Timeline Sequence")]
public class BossTimelineSequence : SerializedScriptableObject
{
    [Title("Propriétés de la Wave")]
    [MinValue(5f), MaxValue(300f)]
    public float totalDuration = 60f;

    [TableList]
    public List<TimelineTrack> tracks = new List<TimelineTrack>();
}
