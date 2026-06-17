using UnityEngine;

namespace BrakingBad.Gameplay
{
    public sealed class CarSoccerGoalTrigger : MonoBehaviour
    {
        [SerializeField] private Minigame_CarSoccer manager;
        [SerializeField, Range(0, 1)] private int scoringTeamIndex;
        [SerializeField] private string ballTag = "Ball";

        public GameObject ball;

        // FIX: drag titik tengah arena ke field ini di Inspector
        // supaya posisi reset tidak hardcode dan bisa disesuaikan per scene
        [SerializeField] private Transform ballResetPoint;

        void ResetBall()
        {
            if (ball == null) return;

            // FIX: pakai posisi dari ballResetPoint jika di-assign,
            // fallback ke Vector3.zero jika tidak
            Vector3 resetPos = ballResetPoint != null
                ? ballResetPoint.position
                : Vector3.zero;

            ball.transform.position = resetPos;
            ball.transform.rotation = Quaternion.identity;

            // FIX: reset velocity supaya bola tidak melaju setelah dipindah
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