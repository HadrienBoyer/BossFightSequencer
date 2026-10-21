using System.Collections;
using UnityEngine;

public class BossWaveInterludeState : IBossState
{
    private BossController boss;
    private BossPhaseConfig nextPhase;
    private int aliveMinionsCount = 0;

    public BossWaveInterludeState(BossController boss, BossPhaseConfig nextPhase)
    {
        this.boss = boss;
        this.nextPhase = nextPhase;
    }

    public void Enter()
    {
        Debug.Log("[Boss] Début de l'Interlude / Wave d'adds !");
        boss.IsInvulnerable = true;

        if (boss.arenaCenter != null)
            boss.transform.position = boss.arenaCenter.position + Vector3.up * 5f;

        SpawnWave(4);
    }

    public void Update()
    {
        if (aliveMinionsCount <= 0)
        {
            boss.ChangeState(new BossFightPhaseState(boss, nextPhase));
        }
    }

    public void Exit()
    {
        boss.IsInvulnerable = false;
        if (boss.arenaCenter != null)
            boss.transform.position = boss.arenaCenter.position;
        
        Debug.Log("[Boss] Fin de l'interlude, retour au combat !");
    }

    private void SpawnWave(int count)
    {
        aliveMinionsCount = count;
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = boss.transform.position + new Vector3(Random.Range(-5f, 5f), -5f, Random.Range(-5f, 5f));
            GameObject minion = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            minion.name = $"Minion_Wave_{i + 1}";
            minion.transform.position = spawnPos;

            boss.StartCoroutine(SimulateMinionDeath(minion, 3.5f + i));
        }
    }

    private IEnumerator SimulateMinionDeath(GameObject minion, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (minion != null)
        {
            GameObject.Destroy(minion);
            aliveMinionsCount--;
        }
    }
}
