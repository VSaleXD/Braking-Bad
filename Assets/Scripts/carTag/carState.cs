using UnityEngine;

namespace BrakingBad.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class carState : MonoBehaviour
    {
        [SerializeField] private GameObject itIndicatorVFX; 
        private TournamentPlayerAgent agent;
        private Minigame_CarTag manager;

        private void Awake()
        {
            agent = GetComponentInParent<TournamentPlayerAgent>();
            manager = FindFirstObjectByType<Minigame_CarTag>();

            if (itIndicatorVFX == null)
            {
                Transform indicatorTransform = transform.Find("ItIndicator");
                if (indicatorTransform != null)
                {
                    itIndicatorVFX = indicatorTransform.gameObject;
                }
            }

            if (itIndicatorVFX != null)
            {
                itIndicatorVFX.SetActive(false);
            }
        }

        public void SetItVisual(bool isIt)
        {
            if (itIndicatorVFX != null) itIndicatorVFX.SetActive(isIt);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (manager == null || agent == null || !manager.IsMatchStarted) return;

            var otherAgent = collision.collider.GetComponentInParent<TournamentPlayerAgent>();
            if (otherAgent == null) return;

            manager.TryTag(agent.PlayerID, otherAgent.PlayerID);
        }
    }
}