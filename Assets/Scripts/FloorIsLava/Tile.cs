using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    public sealed class Tile : MonoBehaviour
    {
        public enum TileState
        {
            Safe,
            Crack1,
            Crack2,
            Lava
        }

        [SerializeField] private Minigame_FloorIsLava manager;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D tileCollider;

        [Header("State Sprites")]
        [SerializeField] private Sprite safeSprite;
        [SerializeField] private Sprite crack1Sprite;
        [SerializeField] private Sprite crack2Sprite;
        [SerializeField] private Sprite lavaSprite;

        [Header("Timing")]
        [Tooltip("Durasi dari Safe ke Crack1, sejak pertama diinjak")]
        [SerializeField] private float timeToReachCrack1 = 0.5f;
        [Tooltip("Durasi dari Crack1 ke Crack2")]
        [SerializeField] private float timeToReachCrack2 = 0.4f;
        [Tooltip("Durasi dari Crack2 ke Lava (collapse)")]
        [SerializeField] private float timeToReachLava = 0.35f;

        private TileState currentState = TileState.Safe;
        private bool sequenceStarted;

        private readonly HashSet<TournamentPlayerAgent> agentsOnTile = new HashSet<TournamentPlayerAgent>();

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

            ApplyStateVisual(TileState.Safe);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (manager == null)
            {
                return;
            }

            TournamentPlayerAgent agent = other.GetComponentInParent<TournamentPlayerAgent>();
            if (agent == null)
            {
                return;
            }

            agentsOnTile.Add(agent);
            manager.RegisterTileTrigger(this, agent);

            if (!sequenceStarted)
            {
                sequenceStarted = true;
                StartCoroutine(StateSequenceRoutine());
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TournamentPlayerAgent agent = other.GetComponentInParent<TournamentPlayerAgent>();
            if (agent != null)
            {
                agentsOnTile.Remove(agent);
            }
        }

        private IEnumerator StateSequenceRoutine()
        {
            yield return new WaitForSeconds(timeToReachCrack1);
            SetState(TileState.Crack1);

            yield return new WaitForSeconds(timeToReachCrack2);
            SetState(TileState.Crack2);

            yield return new WaitForSeconds(timeToReachLava);
            SetState(TileState.Lava);

            CollapseTile();
        }

        private void SetState(TileState newState)
        {
            currentState = newState;
            ApplyStateVisual(newState);
        }

        private void ApplyStateVisual(TileState state)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            switch (state)
            {
                case TileState.Safe:
                    if (safeSprite != null) spriteRenderer.sprite = safeSprite;
                    break;
                case TileState.Crack1:
                    if (crack1Sprite != null) spriteRenderer.sprite = crack1Sprite;
                    break;
                case TileState.Crack2:
                    if (crack2Sprite != null) spriteRenderer.sprite = crack2Sprite;
                    break;
                case TileState.Lava:
                    if (lavaSprite != null) spriteRenderer.sprite = lavaSprite;
                    break;
            }
        }

        private void CollapseTile()
        {
            if (manager != null)
            {
                foreach (TournamentPlayerAgent agent in agentsOnTile)
                {
                    if (agent != null)
                    {
                        manager.RegisterFallenPlayer(agent);
                    }
                }
            }

            agentsOnTile.Clear();

            if (tileCollider != null)
            {
                tileCollider.enabled = false;
            }

            // if (spriteRenderer != null) spriteRenderer.enabled = false;
        }
    }
}