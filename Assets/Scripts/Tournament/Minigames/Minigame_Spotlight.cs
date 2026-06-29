using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BrakingBad.Gameplay
{
    /// Dark map, glowing target, dan score-over-time sampai target ditabrak.
    /// Setiap mobil punya headlight individual (PlayerHeadlight); mobil target
    /// dapat headlight yang lebih besar/terang.
    public sealed class Minigame_Spotlight : BaseMinigameManager
    {
        [SerializeField] private float targetScorePerSecond = 5f;
        [SerializeField] private Light2D ambientLight;
        [SerializeField] private GameObject targetAuraPrefab;

        private int currentTargetPlayerID = 1;
        public int CurrentTargetPlayerID => currentTargetPlayerID;
        private GameObject currentAuraInstance;

        protected override void OnMatchStarted()
        {
            ChooseInitialTarget();
            ApplyLightingState();
        }

        protected override void OnMatchEnded()
        {
            if (currentAuraInstance != null)
            {
                Destroy(currentAuraInstance);
                currentAuraInstance = null;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (matchTimer <= 0f)
            {
                return;
            }

            AddGameplayScore(currentTargetPlayerID, targetScorePerSecond * Time.deltaTime);
        }

        public void TrySwapTarget(TournamentPlayerAgent attacker)
        {
            if (attacker == null || attacker.PlayerID == currentTargetPlayerID)
            {
                return;
            }
            int oldTargetID = currentTargetPlayerID;
            currentTargetPlayerID = attacker.PlayerID;
            ApplyLightingState();
            ShowComboMessage($"P{attacker.PlayerID} STOLE THE SPOTLIGHT FROM P{oldTargetID}!");
        }

        private void ChooseInitialTarget()
        {
            TournamentPlayerAgent[] agents = FindObjectsByType<TournamentPlayerAgent>(FindObjectsSortMode.None);
            if (agents == null || agents.Length == 0)
            {
                currentTargetPlayerID = 1;
                return;
            }

            currentTargetPlayerID = agents[Random.Range(0, agents.Length)].PlayerID;
        }

        private void ApplyLightingState()
        {
            if (ambientLight != null)
            {
                ambientLight.intensity = 0.15f;
            }

            foreach (TournamentPlayerAgent agent in FindObjectsByType<TournamentPlayerAgent>(FindObjectsSortMode.None))
            {
                if (agent == null) continue;

                PlayerHeadlight headlight = agent.GetComponent<PlayerHeadlight>();
                if (headlight != null)
                {
                    headlight.SetAsTarget(agent.PlayerID == currentTargetPlayerID);
                }
            }

            if (currentAuraInstance != null)
            {
                Destroy(currentAuraInstance);
            }

            if (targetAuraPrefab == null)
            {
                return;
            }

            TournamentPlayerAgent targetAgent = GetPlayerAgent(currentTargetPlayerID);
            if (targetAgent != null)
            {
                currentAuraInstance = Instantiate(targetAuraPrefab, targetAgent.transform);
                currentAuraInstance.transform.localPosition = Vector3.zero;
            }
        }
    }
}