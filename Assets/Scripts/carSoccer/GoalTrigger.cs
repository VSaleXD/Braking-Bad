using UnityEngine;

namespace BrakingBad.Gameplay
{
    public sealed class GoalTrigger : MonoBehaviour
    {
        [SerializeField] private Minigame_CarSoccer manager;
        [SerializeField, Range(0, 1)] private int scoringTeamIndex;
        [SerializeField] private string ballTag = "Ball";

        public GameObject ball;
        [SerializeField] private Transform ballResetPoint;

        void ResetBall()
        {
            if (ball == null) return;
            Vector3 resetPos = ballResetPoint != null
                ? ballResetPoint.position
                : Vector3.zero;

            ball.transform.position = resetPos;
            ball.transform.rotation = Quaternion.identity;

            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (manager == null || other == null) return;
            if (!other.CompareTag(ballTag)) return;

            manager.RegisterGoal(scoringTeamIndex);
            ResetBall();
        }
    }
}