using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFightPhaseState : IBossState
{
    private BossController boss;
    private BossPhaseConfig config;
    private Dictionary<BossPatternData, float> cooldownTrackers = new Dictionary<BossPatternData, float>();
    private Coroutine currentRoutine;

    public BossFightPhaseState(BossController boss, BossPhaseConfig config)
    {
        this.boss = boss;
        this.config = config;

        if (config != null && config.availablePatterns != null)
        {
            foreach (var pattern in config.availablePatterns)
            {
                if (pattern != null)
                    cooldownTrackers[pattern] = 0f;
            }
        }
    }

    public void Enter()
    {
        Debug.Log($"[Boss] Entrée en {config?.phaseName}");
        currentRoutine = boss.StartCoroutine(PatternLoop());
    }

    public void Update()
    {
        var keys = new List<BossPatternData>(cooldownTrackers.Keys);
        foreach (var key in keys)
        {
            if (cooldownTrackers[key] > 0)
                cooldownTrackers[key] -= Time.deltaTime;
        }
    }

    public void Exit()
    {
        if (currentRoutine != null)
            boss.StopCoroutine(currentRoutine);
    }

    private IEnumerator PatternLoop()
    {
        while (true)
        {
            BossPatternData selected = SelectBestPattern();

            if (selected == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            cooldownTrackers[selected] = selected.cooldown;
            boss.animator?.SetTrigger(selected.animationTrigger);
            yield return new WaitForSeconds(selected.telegraphDuration);

            // Phase d'attaque active
            yield return new WaitForSeconds(selected.attackDuration);

            // Phase de recovery
            yield return new WaitForSeconds(selected.recoveryDuration);
        }
    }

    private BossPatternData SelectBestPattern()
    {
        if (config == null || config.availablePatterns == null) return null;

        float dist = boss.DistanceToPlayer();
        List<BossPatternData> validPatterns = new List<BossPatternData>();
        int totalWeight = 0;

        foreach (var p in config.availablePatterns)
        {
            if (p == null) continue;

            if (cooldownTrackers.ContainsKey(p) && cooldownTrackers[p] <= 0 && dist >= p.minDistance && dist <= p.maxDistance)
            {
                validPatterns.Add(p);
                totalWeight += p.weight;
            }
        }

        if (validPatterns.Count == 0) return null;

        int roll = Random.Range(0, totalWeight);
        int currentSum = 0;
        foreach (var p in validPatterns)
        {
            currentSum += p.weight;
            if (roll < currentSum) return p;
        }

        return validPatterns[0];
    }
}
