using UnityEngine;
using Sirenix.OdinInspector;

public class BossFightDebugTester : MonoBehaviour
{
    [SerializeField] private BossController bossController;

    private void Awake()
    {
        if (bossController == null)
            bossController = GetComponent<BossController>();
    }

    [ButtonGroup("CombatActions")]
    [Button("💥 Attaquer Boss (-150 HP)", ButtonSizes.Medium)]
    [GUIColor(1f, 0.4f, 0.4f)]
    public void DealSmallDamage()
    {
        if (bossController != null)
            bossController.TakeDamage(150f);
    }

    [ButtonGroup("CombatActions")]
    [Button("💣 Coup Critique (-400 HP)", ButtonSizes.Medium)]
    [GUIColor(1f, 0.2f, 0.2f)]
    public void DealHeavyDamage()
    {
        if (bossController != null)
            bossController.TakeDamage(400f);
    }

    [ButtonGroup("CombatActions")]
    [Button("✨ Soigner Boss (+300 HP)", ButtonSizes.Medium)]
    [GUIColor(0.3f, 0.9f, 0.4f)]
    public void HealBoss()
    {
        if (bossController != null)
        {
            bossController.TakeDamage(-300f);
        }
    }

    [ButtonGroup("PhaseForcing")]
    [Button("⏩ Forcer Phase 1", ButtonSizes.Small)]
    public void ForcePhase1() => bossController?.TransitionToPhase(0);

    [ButtonGroup("PhaseForcing")]
    [Button("⏩ Forcer Phase 2", ButtonSizes.Small)]
    public void ForcePhase2() => bossController?.TransitionToPhase(1);

    [ButtonGroup("PhaseForcing")]
    [Button("🔥 Forcer Phase 3 (Enrage)", ButtonSizes.Small)]
    [GUIColor(1f, 0.5f, 0f)]
    public void ForcePhase3() => bossController?.TransitionToPhase(2);

    private void OnGUI()
    {
        // Simple overlay de test en bas à gauche de l'écran en Play Mode
        GUILayout.BeginArea(new Rect(15, Screen.height - 130, 240, 115), GUI.skin.box);
        GUILayout.Label("<b>⚔️ TESTEUR DE COMBAT</b>");

        if (GUILayout.Button("Frapper (-200 HP)"))
        {
            bossController?.TakeDamage(200f);
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("P1")) bossController?.TransitionToPhase(0);
        if (GUILayout.Button("P2")) bossController?.TransitionToPhase(1);
        if (GUILayout.Button("P3 (Enrage)")) bossController?.TransitionToPhase(2);
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }
}
