using UnityEngine;

namespace BrakingBad.Gameplay
{
    public sealed class ArenaBoundaryTrigger : MonoBehaviour
    {
        [SerializeField] private Minigame_CarSumo manager;

        [Header("Splash Sound")]
        [SerializeField] private AudioClip splashSound;
        [SerializeField, Range(0f, 1f)] private float splashVolume = 1f;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D sound
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (manager == null || other == null)
            {
                return;
            }

            TournamentPlayerAgent agent = other.GetComponentInParent<TournamentPlayerAgent>();
            if (agent != null)
            {
                // ── Splash Sound ──────────────────────────────────────────
                if (splashSound != null)
                {
                    audioSource.PlayOneShot(splashSound, splashVolume);
                }
                // ─────────────────────────────────────────────────────────

                manager.HandleArenaExit(agent);
            }
        }
    }
}