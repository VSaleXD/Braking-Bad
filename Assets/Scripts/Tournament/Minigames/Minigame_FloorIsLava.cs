using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// <summary>
    /// Tile-collapse survival mode with crack feedback and time-based scoring.
    /// </summary>
    public sealed class Minigame_FloorIsLava : BaseMinigameManager
    {
        [SerializeField] private float survivalPointsPerSecond = 2f;
        [SerializeField] private float tileTriggerBonus = 5f;

        private readonly HashSet<int> fallenPlayers = new HashSet<int>();
        private Coroutine survivalRoutine;

        protected override void OnMatchStarted()
        {
            fallenPlayers.Clear();
            survivalRoutine = StartCoroutine(SurvivalScoreRoutine());
        }

        protected override void OnMatchEnded()
        {
            if (survivalRoutine != null)
            {
                StopCoroutine(survivalRoutine);
                survivalRoutine = null;
            }
        }

        public void RegisterTileTrigger(FloorIsLavaTile tile, TournamentPlayerAgent agent)
        {
            if (tile == null || agent == null)
            {
                return;
            }

            if (!fallenPlayers.Contains(agent.PlayerID))
            {
                AddGameplayScore(agent.PlayerID, tileTriggerBonus, "Crack!");
            }

            tile.BeginCrackSequence();
        }

        public void RegisterFallenPlayer(TournamentPlayerAgent agent)
        {
            if (agent == null)
            {
                return;
            }

            fallenPlayers.Add(agent.PlayerID);
            agent.SetEliminated(true);
        }

        private IEnumerator SurvivalScoreRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                foreach (TournamentPlayerAgent agent in GetRegisteredPlayers())
                {
                    if (!fallenPlayers.Contains(agent.PlayerID))
                    {
                        AddGameplayScore(agent.PlayerID, survivalPointsPerSecond);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Attach this to each tile in the grid. The tile cracks, then disappears, after a short delay.
    /// </summary>
    public sealed class FloorIsLavaTile : MonoBehaviour
    {
        [SerializeField] private Minigame_FloorIsLava manager;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D tileCollider;
        [SerializeField] private Color crackedTint = new Color(0.8f, 0.5f, 0.2f, 1f);
        [SerializeField] private float crackDelay = 0.35f;
        [SerializeField] private float collapseDelay = 0.45f;

        private bool isCracking;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (tileCollider == null)
            {
                tileCollider = GetComponent<Collider2D>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (manager == null || isCracking)
            {
                return;
            }

            TournamentPlayerAgent agent = other.GetComponentInParent<TournamentPlayerAgent>();
            if (agent != null)
            {
                manager.RegisterTileTrigger(this, agent);
            }
        }

        public void BeginCrackSequence()
        {
            if (isCracking)
            {
                return;
            }

            StartCoroutine(CrackRoutine());
        }

        private IEnumerator CrackRoutine()
        {
            isCracking = true;
            yield return new WaitForSeconds(crackDelay);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = crackedTint;
            }

            yield return new WaitForSeconds(collapseDelay);

            if (tileCollider != null)
            {
                tileCollider.enabled = false;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }
    }
}