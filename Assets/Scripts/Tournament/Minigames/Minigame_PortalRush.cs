using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Linked teleporter race with safe momentum-preserving movement and arrival-sequence scoring.
    public sealed class Minigame_PortalRush : BaseMinigameManager
    {
        [System.Serializable]
        public sealed class PortalLink
        {
            public TeleportTrigger entrance;
            public Transform destination;
            public int sequenceIndex;
        }

        [SerializeField] private List<PortalLink> portalLinks = new List<PortalLink>();
        [SerializeField] private float baseArrivalScore = 20f;
        [SerializeField] private float orderBonus = 5f;

        private readonly Dictionary<int, int> nextExpectedSequenceByPlayer = new Dictionary<int, int>();
        private int globalArrivalOrder;

        protected override void OnMatchStarted()
        {
            nextExpectedSequenceByPlayer.Clear();
            globalArrivalOrder = 0;

            foreach (TournamentPlayerAgent agent in GetRegisteredPlayers())
            {
                nextExpectedSequenceByPlayer[agent.PlayerID] = 0;
            }
        }

        public bool TryTeleportPlayer(TournamentPlayerAgent agent, Transform destination, int sequenceIndex)
        {
            if (agent == null || destination == null)
            {
                return false;
            }

            if (!nextExpectedSequenceByPlayer.ContainsKey(agent.PlayerID))
            {
                nextExpectedSequenceByPlayer[agent.PlayerID] = 0;
            }

            if (sequenceIndex != nextExpectedSequenceByPlayer[agent.PlayerID])
            {
                return false;
            }

            nextExpectedSequenceByPlayer[agent.PlayerID] = sequenceIndex + 1;

            Rigidbody2D rigidbody2D = agent.CachedRigidbody;
            Vector2 preservedVelocity = rigidbody2D != null ? rigidbody2D.linearVelocity : Vector2.zero;
            float preservedAngularVelocity = rigidbody2D != null ? rigidbody2D.angularVelocity : 0f;

            agent.Teleport(destination.position);

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = preservedVelocity;
                rigidbody2D.angularVelocity = preservedAngularVelocity;
            }

            float sequenceScore = baseArrivalScore + Mathf.Max(0, 10 - globalArrivalOrder) * orderBonus;
            globalArrivalOrder++;
            AddGameplayScore(agent.PlayerID, sequenceScore, $"Portal {sequenceIndex + 1}");
            return true;
        }
    }
}