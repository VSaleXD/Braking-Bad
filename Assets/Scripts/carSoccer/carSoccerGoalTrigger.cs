using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// <summary>
    /// Put this on left/right goal trigger colliders and point it at the soccer manager.
    /// </summary>
    public sealed class CarSoccerGoalTrigger : MonoBehaviour
    {
        [SerializeField] private Minigame_CarSoccer manager;
        [SerializeField, Range(0, 1)] private int scoringTeamIndex;
        [SerializeField] private string ballTag = "Ball";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (manager == null || other == null)
            {
                return;
            }

            if (!other.CompareTag(ballTag))
            {
                return;
            }

            manager.RegisterGoal(scoringTeamIndex);
            if(other.CompareTag("Player"))
            {
                Debug.Log("Player masuk gawang!");
            }
        }
    }
}