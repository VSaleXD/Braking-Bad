using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class lava : MonoBehaviour
    {
        [SerializeField] private Minigame_FloorIsLava manager;
        [SerializeField] private AudioClip lavaFallSound;
        [SerializeField, Range(0f, 1f)] private float lavaFallVolume = 1f;

        private AudioSource audioSource;
        private Collider2D lavaCollider;
        private readonly HashSet<int> triggeredPlayers = new HashSet<int>();

        private void Awake()
        {
            if (manager == null)
            {
                manager = FindAnyObjectByType<Minigame_FloorIsLava>();
            }

            lavaCollider = GetComponent<Collider2D>();
            if (lavaCollider != null)
            {
                lavaCollider.isTrigger = true;
                lavaCollider.enabled = false;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }
        }

        public void Activate()
        {
            triggeredPlayers.Clear();

            if (lavaCollider != null)
            {
                lavaCollider.enabled = true;
            }

            Physics2D.SyncTransforms();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TournamentPlayerAgent agent = other.GetComponentInParent<TournamentPlayerAgent>();
            if (agent == null || manager == null)
            {
                return;
            }

            if (triggeredPlayers.Add(agent.PlayerID))
            {
                if (lavaFallSound != null)
                {
                    audioSource.PlayOneShot(lavaFallSound, lavaFallVolume);
                }

                manager.RegisterFallenPlayer(agent);
            }
        }
    }
}
