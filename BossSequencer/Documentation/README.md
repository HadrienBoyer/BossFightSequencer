# Boss Sequencer & Timeline Tool (Unity + Odin Inspector + UI Toolkit)

Ce package fournit un séquenceur no-code sous forme de timeline type DAW (Ableton Live) pour concevoir et rythmer des boss fights en arène avec vagues d'ennemis et patterns d'attaque.

## Prérequis
- Unity 2021.3+ / 2022+ / 6+
- **Odin Inspector & Serializer** (Sirenix) installé dans le projet.

## Contenu du package
- `Runtime/` :
  - `BossPatternData.cs` : ScriptableObject pour configurer les attaques du boss.
  - `BossTimelineSequence.cs` : Structure de données des pistes et des clips temporels.
  - `BossController.cs` : Gestionnaire de boss & santé.
  - `BossFightPhaseState.cs` & `BossWaveInterludeState.cs` : États de combat FSM.
  - `BossSequencerPlayer.cs` : Composant Runtime pour jouer la timeline en temps réel.
- `Editor/` :
  - `BossSequencerWindow.cs` : Fenêtre DAW interactive (UI Toolkit + Odin).

## Utilisation
1. Ouvrez Unity et cliquez sur **Tools > Boss Sequencer (DAW Timeline)**.
2. Créez un asset `BossTimelineSequence` via le menu `Assets > Create > Boss > Timeline Sequence`.
3. Glissez l'asset dans la fenêtre du séquenceur :
   - Cliquez sur **+ Ajouter Piste**.
   - **Double-cliquez** sur une piste pour insérer un clip d'action.
   - **Glissez** pour modifier le timing.
   - **Tirez le bord droit** pour changer la durée.
   - **Sélectionnez le clip** pour l'éditer dans l'inspecteur Odin.
