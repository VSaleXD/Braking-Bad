using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Central flag capture mode with pick up, carry, drop, and delivery states.
    public sealed class Minigame_CaptureTheFlag : BaseMinigameManager
    {
        [SerializeField] private float captureBonus = 200f;
        [SerializeField] private Transform flagResetPoint;
        [SerializeField] private GameObject flagPrefab;
        
        private Item activeFlag;

        protected override void OnMatchStarted()
        {
            if (flagPrefab != null && activeFlag == null)
            {
                GameObject flagInstance = Instantiate(flagPrefab, flagResetPoint != null ? flagResetPoint.position : Vector3.zero, Quaternion.identity);
                activeFlag = flagInstance.GetComponent<Item>();

                if (activeFlag != null)
                {
                    activeFlag.Initialize(this);
                }
            }
        }

        public bool TryPickupFlag(TournamentPlayerAgent agent)
        {
            if (agent == null || activeFlag == null || activeFlag.IsHeld)
            {
                return false;
            }

            activeFlag.Pickup(agent);
            ShowComboMessage($"P{agent.PlayerID} picked up the flag");
            return true;
        }

        public void DropFlag(Vector3 worldPosition)
        {
            if (activeFlag == null)
            {
                return;
            }

            activeFlag.Drop(worldPosition);
        }

        public void CaptureFlag(TournamentPlayerAgent carrier)
        {
            if (carrier == null || activeFlag == null)
            {
                return;
            }

            AddGameplayScore(carrier.PlayerID, captureBonus, "Flag captured!");
            activeFlag.ResetFlag(flagResetPoint != null ? flagResetPoint.position : Vector3.zero);
        }

        public void NotifyFlagCarrierHit(TournamentPlayerAgent attacker)
        {
            if (activeFlag == null || !activeFlag.IsHeld)
            {
                return;
            }

            if (attacker != null && activeFlag.Carrier != null && attacker.PlayerID != activeFlag.Carrier.PlayerID)
            {
                DropFlag(activeFlag.transform.position);
            }
        }
    }
}