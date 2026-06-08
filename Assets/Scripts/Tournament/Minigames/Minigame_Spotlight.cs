using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BrakingBad.Gameplay
{
    /// <summary>
    /// Dark map, glowing target, and score-over-time until the target is bumped.
    /// </summary>
    public sealed class Minigame_Spotlight : BaseMinigameManager
    {
        [SerializeField] private float targetScorePerSecond = 5f;
        [SerializeField] private Light2D ambientLight;
        [SerializeField] private Light2D targetLight;
        [SerializeField] private GameObject targetAuraPrefab;

        private int currentTargetPlayerID = 1;
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

            currentTargetPlayerID = attacker.PlayerID;
            ApplyLightingState();
            ShowComboMessage($"Target changed to P{currentTargetPlayerID}");
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

            if (targetLight != null)
            {
                targetLight.intensity = 1.5f;
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

    /// <summary>
    /// Put this on player vehicles so contact with the target can swap the spotlight.
    /// </summary>
    public sealed class SpotlightContactRelay : MonoBehaviour
    {
        [SerializeField] private Minigame_Spotlight manager;
        private TournamentPlayerAgent cachedAgent;

        private void Awake()
        {
            cachedAgent = GetComponentInParent<TournamentPlayerAgent>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (manager == null || cachedAgent == null)
            {
                return;
            }

            TournamentPlayerAgent otherAgent = collision.collider.GetComponentInParent<TournamentPlayerAgent>();
            if (otherAgent != null && otherAgent != cachedAgent)
            {
                manager.TrySwapTarget(cachedAgent);
            }
        }
    }
}