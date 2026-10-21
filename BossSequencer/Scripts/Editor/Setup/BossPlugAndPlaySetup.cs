#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public static class BossPlugAndPlaySetup
{
    private const string ASSET_ROOT = "Assets/BossSequencer_Demo";

    [MenuItem("Tools/Boss Sequencer/⚡ 1-Click Plug & Play Boss Setup", false, 0)]
    public static void GeneratePlugAndPlayBossFight()
    {
        EnsureDirectories();

        // 1. Création des Feedbacks Audio & Visuels
        var sfxLaunch = CreateOrLoadAsset<AudioFeedbackDefinition>($"{ASSET_ROOT}/Feedbacks/SFX_Boss_Launch.asset");
        var sfxPhaseChange = CreateOrLoadAsset<AudioFeedbackDefinition>($"{ASSET_ROOT}/Feedbacks/SFX_PhaseTransition.asset");

        var vfxLaunch = CreateOrLoadAsset<VisualFeedbackDefinition>($"{ASSET_ROOT}/Feedbacks/VFX_Launch_Tween.asset");
        vfxLaunch.triggerCameraShake = true;
        vfxLaunch.cameraShakeIntensity = 0.5f;

        var vfxImpact = CreateOrLoadAsset<VisualFeedbackDefinition>($"{ASSET_ROOT}/Feedbacks/VFX_Impact_Flash.asset");
        EditorUtility.SetDirty(vfxLaunch);
        EditorUtility.SetDirty(vfxImpact);

        // 2. Création des Patterns de Projectiles
        var projPattern1 = CreateOrLoadAsset<ProjectilePatternDefinition>($"{ASSET_ROOT}/Projectiles/ProjPattern_RadialBurst.asset");
        projPattern1.patternIdentifier = "Radial Energy Burst";
        projPattern1.spreadShape = ProjectileSpreadShape.RadialCircle;
        projPattern1.baseSpeed = 10f;
        projPattern1.damage = 25f;
        projPattern1.waveSteps = new List<ProjectileWaveStep>
        {
            new ProjectileWaveStep { projectileCount = 12, delayBeforeNextWave = 0.25f, arcAngle = 360f },
            new ProjectileWaveStep { projectileCount = 16, delayBeforeNextWave = 0.3f, arcAngle = 360f, angleOffset = 15f }
        };
        projPattern1.launchAudioFeedback = sfxLaunch;
        projPattern1.launchVisualFeedback = vfxLaunch;
        EditorUtility.SetDirty(projPattern1);

        var projPattern2 = CreateOrLoadAsset<ProjectilePatternDefinition>($"{ASSET_ROOT}/Projectiles/ProjPattern_SpiralHell.asset");
        projPattern2.patternIdentifier = "Spiral Bullet Storm";
        projPattern2.spreadShape = ProjectileSpreadShape.Spiral;
        projPattern2.baseSpeed = 14f;
        projPattern2.damage = 40f;
        projPattern2.isHoming = true;
        projPattern2.homingTurnSpeed = 120f;
        projPattern2.launchAudioFeedback = sfxLaunch;
        EditorUtility.SetDirty(projPattern2);

        // 3. Création des BossPatternData
        var bossPattern1 = CreateOrLoadAsset<BossPatternData>($"{ASSET_ROOT}/Patterns/BossPattern_Nova.asset");
        bossPattern1.patternName = "Nova Explosion";
        bossPattern1.animationTrigger = "Attack_Nova";
        bossPattern1.projectilePattern = projPattern1;
        bossPattern1.telegraphDuration = 1.0f;
        bossPattern1.attackDuration = 1.5f;
        bossPattern1.recoveryDuration = 2.0f;
        EditorUtility.SetDirty(bossPattern1);

        var bossPattern2 = CreateOrLoadAsset<BossPatternData>($"{ASSET_ROOT}/Patterns/BossPattern_Spiral.asset");
        bossPattern2.patternName = "Spiral Barrage";
        bossPattern2.animationTrigger = "Attack_Spiral";
        bossPattern2.projectilePattern = projPattern2;
        bossPattern2.telegraphDuration = 0.8f;
        bossPattern2.attackDuration = 2.5f;
        bossPattern2.recoveryDuration = 1.0f;
        EditorUtility.SetDirty(bossPattern2);

        // 4. Création des Timelines (Waves)
        var timelineP1 = CreateOrLoadAsset<BossTimelineSequence>($"{ASSET_ROOT}/Sequences/Timeline_Phase1_Intro.asset");
        timelineP1.totalDuration = 30f;
        timelineP1.tracks = new List<TimelineTrack>
        {
            new TimelineTrack
            {
                trackName = "Boss Pattern Track",
                type = TrackType.BossAction,
                clips = new List<TimelineClip>
                {
                    new TimelineClip { clipName = "Nova Attack", startTime = 2.0f, duration = 4.5f, patternData = bossPattern1, clipColor = new Color(0.2f, 0.7f, 1f) },
                    new TimelineClip { clipName = "Nova Burst 2", startTime = 12.0f, duration = 4.5f, patternData = bossPattern1, clipColor = new Color(0.2f, 0.7f, 1f) }
                }
            },
            new TimelineTrack
            {
                trackName = "Minion Wave Track",
                type = TrackType.MinionWave,
                clips = new List<TimelineClip>
                {
                    new TimelineClip { clipName = "Add Wave A", startTime = 18.0f, duration = 6f, spawnCount = 4, clipColor = new Color(0.9f, 0.6f, 0.2f) }
                }
            }
        };
        EditorUtility.SetDirty(timelineP1);

        var timelineP2 = CreateOrLoadAsset<BossTimelineSequence>($"{ASSET_ROOT}/Sequences/Timeline_Phase2_Enrage.asset");
        timelineP2.totalDuration = 45f;
        timelineP2.tracks = new List<TimelineTrack>
        {
            new TimelineTrack
            {
                trackName = "Frantic Boss Actions",
                type = TrackType.BossAction,
                clips = new List<TimelineClip>
                {
                    new TimelineClip { clipName = "Spiral Barrage", startTime = 1.0f, duration = 4.0f, patternData = bossPattern2, clipColor = new Color(1f, 0.3f, 0.3f) },
                    new TimelineClip { clipName = "Double Nova", startTime = 8.0f, duration = 5.0f, patternData = bossPattern1, clipColor = new Color(1f, 0.3f, 0.3f) },
                    new TimelineClip { clipName = "Death Spiral", startTime = 18.0f, duration = 6.0f, patternData = bossPattern2, clipColor = new Color(0.8f, 0.1f, 0.1f) }
                }
            },
            new TimelineTrack
            {
                trackName = "Arena Hazards",
                type = TrackType.ArenaHazard,
                clips = new List<TimelineClip>
                {
                    new TimelineClip { clipName = "Lava Floor Danger", startTime = 12.0f, duration = 8f, clipColor = new Color(0.9f, 0.2f, 0.8f) }
                }
            }
        };
        EditorUtility.SetDirty(timelineP2);

        // 5. Création des Stats & Profil Personnage
        var stats = CreateOrLoadAsset<BossStatsData>($"{ASSET_ROOT}/Data/Stats_Malakor.asset");
        stats.maxHealth = 2000f;
        stats.armor = 25f;
        stats.poiseMeter = 120f;
        stats.baseMoveSpeed = 4.5f;
        stats.enrageSpeedMultiplier = 1.5f;
        EditorUtility.SetDirty(stats);

        var anims = CreateOrLoadAsset<BossAnimationData>($"{ASSET_ROOT}/Data/Anims_Malakor.asset");
        EditorUtility.SetDirty(anims);

        var profile = CreateOrLoadAsset<BossCharacterProfile>($"{ASSET_ROOT}/Data/Profile_Malakor.asset");
        profile.bossName = "Malakor, The Void Sovereign";
        profile.title = "Ancient Arch-Demon of the Arena";
        profile.stats = stats;
        profile.animations = anims;
        profile.allPatterns = new List<BossPatternData> { bossPattern1, bossPattern2 };
        EditorUtility.SetDirty(profile);

        AssetDatabase.SaveAssets();

        // 6. Instanciation dans la scène active (Arène NavMesh + Prefabs DemoPlayer & DemoBoss + HUD Canvas)
        SetupSceneGameObjects(profile, timelineP1, timelineP2, sfxPhaseChange);

        EditorUtility.DisplayDialog(
            "⚡ Plug & Play Boss Setup Terminé !",
            "Le setup 100% fonctionnel a été généré :\n\n" +
            "• Arène avec sol NavMesh (Static)\n" +
            "• Prefab 'DemoPlayerController' (Déplacement ZQSD/Flèches + Visée souris + Clic gauche pour attaquer)\n" +
            "• Prefab 'DemoBossController' (Navigation NavMeshAgent vers le joueur + vitesse dynamique selon les phases + tirs de projectiles synchronisés avec la Timeline)\n" +
            "• Canvas HUD réactif (PV, Poise, Timer, Flash, Bannières)\n\n" +
            "N'oubliez pas de générer le NavMesh (Window > AI > Navigation > Bake) si ce n'est pas déjà fait !",
            "Compris !"
        );
    }

    private static void SetupSceneGameObjects(BossCharacterProfile profile, BossTimelineSequence p1, BossTimelineSequence p2, AudioFeedbackDefinition phaseSfx)
    {
        // 1. Sol d'Arène (NavMesh Static)
        var floor = GameObject.Find("Arena_Floor");
        if (floor == null)
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Arena_Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(5, 1, 5); // 50m x 50m
            floor.GetComponent<Renderer>().sharedMaterial.color = new Color(0.18f, 0.2f, 0.22f);
            GameObjectUtility.SetStaticEditorFlags(floor, StaticEditorFlags.NavigationStatic);
        }

        // 2. HUD Canvas
        var hudObj = GameObject.Find("BossFight_HUD");
        if (hudObj == null)
        {
            hudObj = new GameObject("BossFight_HUD");
            var canvas = hudObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            hudObj.AddComponent<GraphicRaycaster>();
            BuildHUDUI(hudObj);
        }

        // 3. Joueur : DemoPlayerController (Prefab)
        var playerObj = GameObject.Find("DemoPlayer");
        if (playerObj == null)
        {
            playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "DemoPlayer";
            playerObj.tag = "Player";
            playerObj.transform.position = new Vector3(0, 1f, -12f);
            playerObj.GetComponent<Renderer>().sharedMaterial.color = new Color(0.2f, 0.8f, 0.4f); // Vert

            // Remplacement du CapsuleCollider par CharacterController
            var col = playerObj.GetComponent<CapsuleCollider>();
            if (col != null) Object.DestroyImmediate(col);

            var cc = playerObj.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.center = Vector3.up;

            playerObj.AddComponent<DemoPlayerController>();

            // Sauvegarde en Prefab
            string playerPrefabPath = $"{ASSET_ROOT}/DemoPlayerController.prefab";
            PrefabUtility.SaveAsPrefabAsset(playerObj, playerPrefabPath);
        }

        // 4. Boss : DemoBossController (Prefab + NavMeshAgent)
        var bossObj = GameObject.Find("DemoBoss");
        if (bossObj == null)
        {
            bossObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bossObj.name = "DemoBoss";
            bossObj.transform.position = new Vector3(0, 1.5f, 8f);
            bossObj.transform.localScale = new Vector3(2.5f, 3f, 2.5f);
            var bossRenderer = bossObj.GetComponent<Renderer>();
            bossRenderer.sharedMaterial.color = new Color(0.85f, 0.2f, 0.2f); // Rouge

            // NavMeshAgent
            var agent = bossObj.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.speed = profile.stats.baseMoveSpeed;
            agent.acceleration = 16f;
            agent.angularSpeed = 240f;
            agent.stoppingDistance = 3.5f;

            var controller = bossObj.AddComponent<BossController>();
            var sequencer = bossObj.AddComponent<BossSequencerPlayer>();
            var tester = bossObj.AddComponent<BossFightDebugTester>();
            var demoBoss = bossObj.AddComponent<DemoBossController>();

            controller.characterProfile = profile;
            controller.sequencerPlayer = sequencer;
            controller.player = playerObj.transform;
            controller.fightPhases = new List<BossFightPhaseDefinition>
            {
                new BossFightPhaseDefinition
                {
                    phaseName = "Phase 1 : Awakening",
                    phaseThemeColor = new Color(0.2f, 0.7f, 1f),
                    healthThreshold = 1.0f,
                    timelineSequence = p1,
                    bannerAlertText = "PHASE 1 : LE SOUVERAIN S'ÉVEILLE",
                    damageMultiplier = 1.0f,
                    speedMultiplier = 1.0f,
                    defenseMultiplier = 1.0f,
                    transitionSFX = phaseSfx
                },
                new BossFightPhaseDefinition
                {
                    phaseName = "Phase 2 : Void Fury (Enrage)",
                    phaseThemeColor = new Color(1f, 0.25f, 0.2f),
                    healthThreshold = 0.5f,
                    timelineSequence = p2,
                    bannerAlertText = "PHASE 2 : RAGE DU NÉANT !",
                    damageMultiplier = 1.5f,
                    speedMultiplier = 1.5f, // Boss accélère avec le NavMesh en P2
                    defenseMultiplier = 1.3f,
                    transitionSFX = phaseSfx
                }
            };

            // Sauvegarde en Prefab
            string bossPrefabPath = $"{ASSET_ROOT}/DemoBossController.prefab";
            PrefabUtility.SaveAsPrefabAsset(bossObj, bossPrefabPath);
        }

        // Camera setup si besoin
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(0, 18, -22);
            Camera.main.transform.rotation = Quaternion.Euler(45, 0, 0);
        }

        Selection.activeGameObject = bossObj;
    }

    private static void BuildHUDUI(GameObject hudRoot)
    {
        var hudScript = hudRoot.AddComponent<BossFightHUD>();

        // Overlay Dégâts (Screen Flash)
        var flashObj = new GameObject("DamageFlashOverlay");
        flashObj.transform.SetParent(hudRoot.transform, false);
        var flashImg = flashObj.AddComponent<Image>();
        flashImg.rectTransform.anchorMin = Vector2.zero;
        flashImg.rectTransform.anchorMax = Vector2.one;
        flashImg.rectTransform.sizeDelta = Vector2.zero;
        flashImg.color = new Color(1, 0, 0, 0);
        flashImg.raycastTarget = false;

        // Container Top HUD
        var topContainer = new GameObject("TopHUD_Container");
        topContainer.transform.SetParent(hudRoot.transform, false);
        var topRect = topContainer.AddComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0.2f, 0.88f);
        topRect.anchorMax = new Vector2(0.8f, 0.98f);
        topRect.offsetMin = Vector2.zero;
        topRect.offsetMax = Vector2.zero;

        // Boss Name
        var nameObj = new GameObject("BossNameText");
        nameObj.transform.SetParent(topContainer.transform, false);
        var nameText = nameObj.AddComponent<Text>();
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        nameText.text = "MALAKOR, THE VOID SOVEREIGN";
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.fontSize = 20;
        nameText.fontStyle = FontStyle.Bold;
        nameText.color = Color.white;
        var nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.6f);
        nameRect.anchorMax = new Vector2(1, 1f);
        nameRect.sizeDelta = Vector2.zero;

        // Health Bar (Ghost & Main)
        var healthBarObj = CreateUISlider(topContainer.transform, "HealthBar", new Color(0.85f, 0.15f, 0.15f), new Vector2(0, 0.25f), new Vector2(1, 0.55f));
        var ghostBarObj = CreateUISlider(topContainer.transform, "GhostHealthBar", new Color(1f, 0.8f, 0.2f, 0.6f), new Vector2(0, 0.25f), new Vector2(1, 0.55f));
        ghostBarObj.transform.SetSiblingIndex(healthBarObj.transform.GetSiblingIndex()); // En dessous

        // Poise Bar
        var poiseBarObj = CreateUISlider(topContainer.transform, "PoiseBar", new Color(0.2f, 0.6f, 1f), new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.2f));

        // Timeline Progress & Phase Indicator
        var timeObj = new GameObject("TimelineTimerText");
        timeObj.transform.SetParent(topContainer.transform, false);
        var timeText = timeObj.AddComponent<Text>();
        timeText.font = nameText.font;
        timeText.text = "0.0s / 30.0s";
        timeText.alignment = TextAnchor.MiddleRight;
        timeText.fontSize = 13;
        timeText.color = new Color(0.8f, 0.8f, 0.8f);
        var timeRect = timeObj.GetComponent<RectTransform>();
        timeRect.anchorMin = new Vector2(0.6f, 0f);
        timeRect.anchorMax = new Vector2(1f, 0.3f);
        timeRect.sizeDelta = Vector2.zero;

        // Phase Alert Banner (Au centre de l'écran)
        var bannerObj = new GameObject("PhaseAlertBanner");
        bannerObj.transform.SetParent(hudRoot.transform, false);
        var bannerText = bannerObj.AddComponent<Text>();
        bannerText.font = nameText.font;
        bannerText.alignment = TextAnchor.MiddleCenter;
        bannerText.fontSize = 32;
        bannerText.fontStyle = FontStyle.Bold;
        bannerText.color = Color.yellow;
        var bannerRect = bannerObj.GetComponent<RectTransform>();
        bannerRect.anchorMin = new Vector2(0.1f, 0.5f);
        bannerRect.anchorMax = new Vector2(0.9f, 0.7f);
        bannerRect.sizeDelta = Vector2.zero;

        // Assignations reflection/champs
        SetField(hudScript, "damageFlashOverlay", flashImg);
        SetField(hudScript, "bossNameText", nameText);
        SetField(hudScript, "healthBar", healthBarObj.GetComponent<Slider>());
        SetField(hudScript, "healthGhostBar", ghostBarObj.GetComponent<Slider>());
        SetField(hudScript, "poiseBar", poiseBarObj.GetComponent<Slider>());
        SetField(hudScript, "timelineTimerText", timeText);
        SetField(hudScript, "phaseAlertBanner", bannerText);
    }

    private static GameObject CreateUISlider(Transform parent, string name, Color fillColor, Vector2 anchorMin, Vector2 anchorMax)
    {
        var sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);
        var slider = sliderObj.AddComponent<Slider>();
        var rect = sliderObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = Vector2.zero;

        var bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        var faRect = fillArea.AddComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero;
        faRect.anchorMax = Vector2.one;
        faRect.sizeDelta = Vector2.zero;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = fillColor;
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImg;
        slider.value = 1f;

        return sliderObj;
    }

    private static void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (field != null) field.SetValue(target, value);
    }

    private static void EnsureDirectories()
    {
        string[] dirs = {
            ASSET_ROOT,
            $"{ASSET_ROOT}/Feedbacks",
            $"{ASSET_ROOT}/Projectiles",
            $"{ASSET_ROOT}/Patterns",
            $"{ASSET_ROOT}/Sequences",
            $"{ASSET_ROOT}/Data"
        };
        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        AssetDatabase.Refresh();
    }

    private static T CreateOrLoadAsset<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var newAsset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(newAsset, path);
        return newAsset;
    }
}
#endif
