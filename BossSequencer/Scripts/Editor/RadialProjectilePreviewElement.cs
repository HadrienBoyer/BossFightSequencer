#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class RadialProjectilePreviewElement : ImmediateModeElement
{
    private ProjectilePatternDefinition pattern;
    private float simulationTime = 1.0f;
    private bool autoAnimate = true;

    public void SetPattern(ProjectilePatternDefinition newPattern)
    {
        pattern = newPattern;
        MarkDirtyRepaint();
    }

    public void SetSimulationTime(float time)
    {
        simulationTime = time;
        MarkDirtyRepaint();
    }

    public void SetAutoAnimate(bool animate)
    {
        autoAnimate = animate;
    }

    public void Tick(float delta)
    {
        if (autoAnimate && pattern != null)
        {
            simulationTime = Mathf.Repeat(simulationTime + delta * 0.8f, 2.5f);
            MarkDirtyRepaint();
        }
    }

    protected override void ImmediateRepaint()
    {
        Rect r = contentRect;
        if (r.width <= 10 || r.height <= 10) return;

        Vector2 center = r.center;
        float maxRadius = Mathf.Min(r.width, r.height) * 0.44f;

        // 1. Fond du radar
        DrawRadarBackground(center, maxRadius);

        // 2. Boss central
        DrawBossCenter(center);

        if (pattern == null)
        {
            return;
        }

        // 3. Calcul & Rendu des projectiles selon la simulation temporelle
        DrawSimulatedBullets(center, maxRadius);
    }

    private void DrawRadarBackground(Vector2 center, float radius)
    {
        // Cercles concentriques (Distance 10m, 20m, 30m)
        Color gridColor = new Color(0.25f, 0.35f, 0.45f, 0.4f);
        Color axisColor = new Color(0.25f, 0.35f, 0.45f, 0.6f);

        for (int i = 1; i <= 3; i++)
        {
            float ringR = radius * (i / 3f);
            DrawWireCircle(center, ringR, gridColor, 1.0f);
        }

        // Axes orthogonaux
        DrawLine(new Vector2(center.x - radius, center.y), new Vector2(center.x + radius, center.y), axisColor, 1f);
        DrawLine(new Vector2(center.x, center.y - radius), new Vector2(center.x, center.y + radius), axisColor, 1f);

        // Diagonales discrètes
        Color diagColor = new Color(0.2f, 0.3f, 0.4f, 0.25f);
        float d = radius * 0.7071f;
        DrawLine(new Vector2(center.x - d, center.y - d), new Vector2(center.x + d, center.y + d), diagColor, 1f);
        DrawLine(new Vector2(center.x - d, center.y + d), new Vector2(center.x + d, center.y - d), diagColor, 1f);
    }

    private void DrawBossCenter(Vector2 center)
    {
        // Boss icône centrale (Rouge pulsant)
        DrawFilledCircle(center, 7f, new Color(0.9f, 0.25f, 0.25f, 1f));
        DrawWireCircle(center, 10f, new Color(1f, 0.5f, 0.2f, 0.8f), 1.5f);
    }

    private void DrawSimulatedBullets(Vector2 center, float maxRadius)
    {
        Color bulletCol = pattern.bulletColor;
        float baseScale = Mathf.Max(2f, pattern.bulletScale * 6f);
        float speed = pattern.baseSpeed * 8f; // Échelle pixels/seconde

        for (int stepIndex = 0; stepIndex < pattern.waveSteps.Count; stepIndex++)
        {
            var wave = pattern.waveSteps[stepIndex];
            int count = Mathf.Max(1, wave.projectileCount);
            float stepDelay = stepIndex * wave.delayBeforeNextWave;
            float activeTime = simulationTime - stepDelay;

            if (activeTime < 0f) continue; // Pas encore tiré

            float arc = wave.arcAngle;
            float stepAngle = arc / count;
            float startAngle = -arc * 0.5f + wave.angleOffset - 90f; // -90 pour orienter vers le haut

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (i * stepAngle);
                float dist = activeTime * speed;

                switch (pattern.spreadShape)
                {
                    case ProjectileSpreadShape.DoubleSpiralOpposed:
                        float dirSign = (i % 2 == 0) ? 1f : -1f;
                        angle += dirSign * (stepIndex * pattern.spinSpeed * 0.15f + activeTime * pattern.spinSpeed * 0.5f);
                        break;

                    case ProjectileSpreadShape.RosePetals:
                        float rad = (angle + 90f) * Mathf.Deg2Rad;
                        float distMod = 1f + Mathf.Cos(pattern.petalCount * rad) * pattern.petalDistortion;
                        dist *= distMod;
                        break;

                    case ProjectileSpreadShape.DelayedBurst:
                        if (activeTime < 0.6f)
                        {
                            dist = (activeTime / 0.6f) * (speed * 0.25f);
                        }
                        else if (activeTime < 0.6f + pattern.freezeDuration)
                        {
                            dist = speed * 0.25f; // Arrêt
                        }
                        else
                        {
                            float postTime = activeTime - (0.6f + pattern.freezeDuration);
                            dist = (speed * 0.25f) + postTime * speed * pattern.postFreezeSpeedMultiplier;
                        }
                        break;

                    case ProjectileSpreadShape.WaveSine:
                        float radAngle = angle * Mathf.Deg2Rad;
                        Vector2 fwd = new Vector2(Mathf.Cos(radAngle), Mathf.Sin(radAngle));
                        Vector2 perp = new Vector2(-fwd.y, fwd.x);
                        float sine = Mathf.Sin(activeTime * pattern.sineFrequency) * pattern.sineAmplitude * 6f;
                        Vector2 posWithSine = center + (fwd * dist) + (perp * sine);
                        DrawBullet(posWithSine, baseScale, bulletCol);
                        continue;

                    case ProjectileSpreadShape.ClusterSplit:
                        if (activeTime >= pattern.splitDelay)
                        {
                            // Orbe principal éclaté en sous-balles
                            float splitActive = activeTime - pattern.splitDelay;
                            float rootDist = pattern.splitDelay * speed;
                            float rRad = angle * Mathf.Deg2Rad;
                            Vector2 splitOrigin = center + new Vector2(Mathf.Cos(rRad), Mathf.Sin(rRad)) * rootDist;

                            for (int c = 0; c < pattern.splitChildCount; c++)
                            {
                                float cAngle = (c * (360f / pattern.splitChildCount)) * Mathf.Deg2Rad;
                                Vector2 cPos = splitOrigin + new Vector2(Mathf.Cos(cAngle), Mathf.Sin(cAngle)) * (splitActive * speed * 0.9f);
                                DrawBullet(cPos, baseScale * 0.65f, Color.cyan);
                            }
                            continue;
                        }
                        break;
                }

                float finalRad = angle * Mathf.Deg2Rad;
                Vector2 pos = center + new Vector2(Mathf.Cos(finalRad), Mathf.Sin(finalRad)) * dist;

                // Ne dessine que ce qui est dans les limites visibles
                if (Vector2.Distance(center, pos) <= maxRadius * 1.05f)
                {
                    DrawBullet(pos, baseScale, bulletCol);
                }
            }
        }
    }

    private void DrawBullet(Vector2 pos, float size, Color col)
    {
        DrawFilledCircle(pos, size * 0.5f, col);
        DrawWireCircle(pos, size * 0.5f + 1f, new Color(1f, 1f, 1f, 0.7f), 1f);
    }

    private void DrawFilledCircle(Vector2 center, float radius, Color color)
    {
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, radius);
    }

    private void DrawWireCircle(Vector2 center, float radius, Color color, float thickness)
    {
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawWireDisc(center, Vector3.forward, radius);
    }

    private void DrawLine(Vector2 p1, Vector2 p2, Color color, float thickness)
    {
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawLine(p1, p2);
    }
}
#endif
