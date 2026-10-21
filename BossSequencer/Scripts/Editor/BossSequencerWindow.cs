#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;

public class BossSequencerWindow : OdinEditorWindow
{
    [MenuItem("Tools/Boss Sequencer (DAW Timeline)")]
    private static void OpenWindow()
    {
        var window = GetWindow<BossSequencerWindow>();
        window.titleContent = new GUIContent("Boss DAW Sequencer");
        window.minSize = new Vector2(850, 500);
        window.Show();
    }

    [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Foldout)]
    [OnValueChanged(nameof(OnSequenceDataChanged))]
    public BossTimelineSequence targetSequence;

    [ShowInInspector, HideReferenceObjectPicker]
    [BoxGroup("Clip Inspector", CenterLabel = true)]
    private TimelineClip selectedClip;

    private VisualElement timelineContainer;
    private ScrollView timelineScrollView;

    private const float PIXELS_PER_SECOND = 40f;
    private const float TRACK_HEIGHT = 55f;

    protected override void OnEnable()
    {
        base.OnEnable();
        BuildTimelineUI();
    }

    private void OnSequenceDataChanged()
    {
        RefreshTimeline();
    }

    private void BuildTimelineUI()
    {
        var root = rootVisualElement;

        var container = new VisualElement
        {
            style =
            {
                flexGrow = 1,
                marginTop = 10,
                borderTopWidth = 2,
                borderTopColor = new Color(0.12f, 0.12f, 0.12f),
                backgroundColor = new Color(0.18f, 0.18f, 0.18f)
            }
        };

        var toolbar = new VisualElement
        {
            style =
            {
                height = 32,
                flexDirection = FlexDirection.Row,
                backgroundColor = new Color(0.14f, 0.14f, 0.14f),
                alignItems = Align.Center,
                paddingLeft = 8,
                paddingRight = 8
            }
        };

        var addTrackBtn = new Button(() =>
        {
            if (targetSequence == null) return;
            targetSequence.tracks.Add(new TimelineTrack { trackName = $"Track {targetSequence.tracks.Count + 1}" });
            EditorUtility.SetDirty(targetSequence);
            RefreshTimeline();
        }) { text = "+ Ajouter Piste" };

        var helpLabel = new Label("Astuces : Double-clic sur piste = Créer Clip | Glisser = Déplacer | Bord droit = Redimensionner | Clic-droit = Supprimer")
        {
            style =
            {
                color = new Color(0.6f, 0.6f, 0.6f),
                fontSize = 11,
                marginLeft = 15
            }
        };

        toolbar.Add(addTrackBtn);
        toolbar.Add(helpLabel);
        container.Add(toolbar);

        timelineScrollView = new ScrollView(ScrollViewMode.Horizontal)
        {
            style = { flexGrow = 1 }
        };

        timelineContainer = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                minHeight = 250,
                paddingTop = 15
            }
        };

        timelineScrollView.Add(timelineContainer);
        container.Add(timelineScrollView);
        root.Add(container);

        RefreshTimeline();
    }

    public void RefreshTimeline()
    {
        if (timelineContainer == null) return;
        timelineContainer.Clear();

        if (targetSequence == null)
        {
            var label = new Label("Assignez un ScriptableObject 'BossTimelineSequence' pour éditer.")
            {
                style = { color = Color.gray, marginLeft = 20, marginTop = 20 }
            };
            timelineContainer.Add(label);
            return;
        }

        float totalWidth = targetSequence.totalDuration * PIXELS_PER_SECOND;
        timelineContainer.style.width = totalWidth + 250;

        var ruler = CreateTimeRuler(targetSequence.totalDuration);
        timelineContainer.Add(ruler);

        foreach (var track in targetSequence.tracks)
        {
            var trackElement = CreateTrackElement(track);
            timelineContainer.Add(trackElement);
        }
    }

    private VisualElement CreateTimeRuler(float duration)
    {
        var ruler = new VisualElement
        {
            style =
            {
                height = 24,
                flexDirection = FlexDirection.Row,
                borderBottomWidth = 1,
                borderBottomColor = Color.black,
                backgroundColor = new Color(0.12f, 0.12f, 0.12f)
            }
        };

        for (int sec = 0; sec <= duration; sec += 5)
        {
            var mark = new Label($"{sec}s")
            {
                style =
                {
                    position = Position.Absolute,
                    left = sec * PIXELS_PER_SECOND,
                    fontSize = 10,
                    color = new Color(0.7f, 0.7f, 0.7f)
                }
            };
            ruler.Add(mark);
        }

        return ruler;
    }

    private VisualElement CreateTrackElement(TimelineTrack track)
    {
        var trackRow = new VisualElement
        {
            style =
            {
                height = TRACK_HEIGHT,
                borderBottomWidth = 1,
                borderBottomColor = new Color(0.12f, 0.12f, 0.12f),
                flexDirection = FlexDirection.Row,
                backgroundColor = new Color(0.16f, 0.16f, 0.16f)
            }
        };

        trackRow.RegisterCallback<MouseDownEvent>(evt =>
        {
            if (evt.clickCount == 2)
            {
                float clickTime = Mathf.Max(0, evt.localMousePosition.x / PIXELS_PER_SECOND);
                var newClip = new TimelineClip
                {
                    clipName = track.type == TrackType.BossAction ? "Boss Attack" : "Event",
                    startTime = clickTime,
                    duration = 3.0f,
                    trackType = track.type
                };
                track.clips.Add(newClip);
                EditorUtility.SetDirty(targetSequence);
                RefreshTimeline();
            }
        });

        foreach (var clip in track.clips)
        {
            clip.trackType = track.type;
            var clipVisual = CreateClipElement(clip, track);
            trackRow.Add(clipVisual);
        }

        return trackRow;
    }

    private VisualElement CreateClipElement(TimelineClip clip, TimelineTrack parentTrack)
    {
        var clipEl = new VisualElement
        {
            style =
            {
                position = Position.Absolute,
                left = clip.startTime * PIXELS_PER_SECOND,
                width = clip.duration * PIXELS_PER_SECOND,
                height = TRACK_HEIGHT - 6,
                top = 3,
                backgroundColor = clip.clipColor,
                borderTopLeftRadius = 4,
                borderBottomLeftRadius = 4,
                borderTopRightRadius = 4,
                borderBottomRightRadius = 4,
                borderLeftWidth = 1,
                borderRightWidth = 1,
                borderTopWidth = 1,
                borderBottomWidth = 1,
                borderColor = new Color(0, 0, 0, 0.6f)
            }
        };

        var label = new Label(clip.clipName)
        {
            style =
            {
                color = Color.white,
                fontSize = 11,
                unityFontStyleAndWeight = FontStyle.Bold,
                marginTop = 4,
                marginLeft = 6
            }
        };
        clipEl.Add(label);

        var resizeHandle = new VisualElement
        {
            style =
            {
                width = 8,
                position = Position.Absolute,
                right = 0,
                top = 0,
                bottom = 0,
                backgroundColor = new Color(0, 0, 0, 0.3f)
            }
        };
        clipEl.Add(resizeHandle);

        bool isDragging = false;
        bool isResizing = false;
        Vector2 startMousePos = Vector2.zero;
        float originalStart = 0;
        float originalDuration = 0;

        clipEl.RegisterCallback<MouseDownEvent>(evt =>
        {
            selectedClip = clip;
            originalStart = clip.startTime;
            originalDuration = clip.duration;
            startMousePos = evt.mousePosition;

            if (evt.target == resizeHandle)
            {
                isResizing = true;
            }
            else
            {
                isDragging = true;
            }

            clipEl.CaptureMouse();
            evt.StopPropagation();
        });

        clipEl.RegisterCallback<MouseMoveEvent>(evt =>
        {
            if (!isDragging && !isResizing) return;

            float deltaX = evt.mousePosition.x - startMousePos.x;
            float deltaTime = deltaX / PIXELS_PER_SECOND;

            if (isDragging)
            {
                clip.startTime = Mathf.Max(0, originalStart + deltaTime);
                clipEl.style.left = clip.startTime * PIXELS_PER_SECOND;
            }
            else if (isResizing)
            {
                clip.duration = Mathf.Max(0.5f, originalDuration + deltaTime);
                clipEl.style.width = clip.duration * PIXELS_PER_SECOND;
            }
        });

        clipEl.RegisterCallback<MouseUpEvent>(evt =>
        {
            if (isDragging || isResizing)
            {
                isDragging = false;
                isResizing = false;
                clipEl.ReleaseMouse();
                EditorUtility.SetDirty(targetSequence);
            }
        });

        clipEl.RegisterCallback<ContextClickEvent>(evt =>
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Supprimer"), false, () =>
            {
                parentTrack.clips.Remove(clip);
                if (selectedClip == clip) selectedClip = null;
                EditorUtility.SetDirty(targetSequence);
                RefreshTimeline();
            });
            menu.ShowAsContext();
            evt.StopPropagation();
        });

        return clipEl;
    }
}
#endif
