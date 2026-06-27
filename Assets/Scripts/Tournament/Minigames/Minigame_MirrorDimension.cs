using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Drift Madness variant that inverts steering control by applying a -1 multiplier to player agents.
    public sealed class Minigame_MirrorDimension : Minigame_DriftMadness
    {
        protected override void OnMatchStarted()
        {
            base.OnMatchStarted();

            foreach (TournamentPlayerAgent agent in GetRegisteredPlayers())
            {
                agent.SetControlMultipliers(-1f, agent.ThrottleMultiplier);
            }
        }

        protected override void OnMatchEnded()
        {
            foreach (TournamentPlayerAgent agent in GetRegisteredPlayers())
            {
                agent.SetControlMultipliers(1f, agent.ThrottleMultiplier);
            }
        }
    }
}