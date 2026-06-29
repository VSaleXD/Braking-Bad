using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BrakingBad.Gameplay
{

    public sealed class FinalPodiumController : MonoBehaviour
    {
        [System.Serializable]
        public sealed class PodiumSlot
        {
            [Tooltip("Transform yang akan dipindah/dinaikkan untuk slot rank ini.")]
            public Transform standTransform;

            [Tooltip("Posisi awal (di bawah/tersembunyi) sebelum animasi mulai.")]
            public Vector3 hiddenLocalPosition;

            [Tooltip("Posisi akhir (di atas podium) setelah animasi selesai.")]
            public Vector3 revealedLocalPosition;

            [Tooltip("Label nama/ID player, contoh: 'P1'.")]
            public TMPro.TextMeshProUGUI playerLabel;

            [Tooltip("Label skor tournament, contoh: '7 pts'.")]
            public TMPro.TextMeshProUGUI pointsLabel;

            [Tooltip("Opsional: ganti warna/sprite mobil sesuai playerID.")]
            public SpriteRenderer carRenderer;
        }

        [Header("Podium Slots (urut: Rank 1, Rank 2, Rank 3, Rank 4)")]
        [SerializeField] private List<PodiumSlot> podiumSlots = new List<PodiumSlot>(4);

        [Header("Animation")]
        [SerializeField] private float delayBeforeStart = 0.5f;
        [SerializeField] private float riseDuration = 0.6f;
        [SerializeField] private float delayBetweenSlots = 0.4f;
        [SerializeField] private AnimationCurve riseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Navigation")]
        [SerializeField] private string menuSceneName = "MenuBaru";
        [SerializeField] private GameObject continueButtonRoot;

private void Start()
{
    if (continueButtonRoot != null)
    {
        continueButtonRoot.SetActive(false);
    }

    int activeCount = 4;
    if (TournamentManager.Instance != null)
    {
        activeCount = TournamentManager.Instance.ActivePlayerCount;
    }

    for (int i = 0; i < podiumSlots.Count; i++)
    {
        if (i >= activeCount && podiumSlots[i].standTransform != null)
        {
            podiumSlots[i].standTransform.gameObject.SetActive(false); 
        }
    }

    List<(int playerID, int points)> ranking = BuildRanking();
    StartCoroutine(RevealRoutine(ranking));
}

        private List<(int playerID, int points)> BuildRanking()
        {
            if (TournamentManager.Instance == null)
            {
                Debug.LogWarning("FinalPodiumController: TournamentManager.Instance tidak ditemukan, memakai data kosong.");
                return new List<(int, int)>
                {
                    (1, 0), (2, 0), (3, 0), (4, 0)
                };
            }

            IReadOnlyList<int> points = TournamentManager.Instance.TournamentPoints;
            int activeCount = TournamentManager.Instance.ActivePlayerCount;

            return Enumerable.Range(1, 4)
                .Where(playerID => playerID <= activeCount)
                .Select(playerID => (playerID: playerID, points: points[playerID - 1]))
                .OrderByDescending(entry => entry.points)
                .ThenBy(entry => entry.playerID)
                .ToList();
        }

        private IEnumerator RevealRoutine(List<(int playerID, int points)> ranking)
        {
            yield return new WaitForSeconds(delayBeforeStart);

            for (int rankIndex = 0; rankIndex < ranking.Count; rankIndex++)
            {
                if (rankIndex >= podiumSlots.Count)
                {
                    continue;
                }

                PodiumSlot slot = podiumSlots[rankIndex];
                (int playerID, int points) entry = ranking[rankIndex];

                ApplySlotData(slot, entry.playerID, entry.points);
                yield return StartCoroutine(RiseRoutine(slot));
                yield return new WaitForSeconds(delayBetweenSlots);
            }

            if (continueButtonRoot != null)
            {
                continueButtonRoot.SetActive(true);
            }
        }

        private void ApplySlotData(PodiumSlot slot, int playerID, int points)
        {
            if (slot.playerLabel != null)
            {
                slot.playerLabel.text = $"P{playerID}";
            }

            if (slot.pointsLabel != null)
            {
                pointsLabel_SetText(slot.pointsLabel, points);
            }

            if (slot.standTransform != null)
            {
                slot.standTransform.localPosition = slot.hiddenLocalPosition;
            }
        }

        private void pointsLabel_SetText(TMPro.TextMeshProUGUI label, int points)
        {
            label.text = points == 1 ? "1 pt" : $"{points} pts";
        }

        private IEnumerator RiseRoutine(PodiumSlot slot)
        {
            if (slot.standTransform == null)
            {
                yield break;
            }

            Vector3 start = slot.hiddenLocalPosition;
            Vector3 end = slot.revealedLocalPosition;
            float elapsed = 0f;

            while (elapsed < riseDuration)
            {
                elapsed += Time.deltaTime;
                float t = riseCurve.Evaluate(Mathf.Clamp01(elapsed / riseDuration));
                slot.standTransform.localPosition = Vector3.LerpUnclamped(start, end, t);
                yield return null;
            }

            slot.standTransform.localPosition = end;
        }

        public void ReturnToMenu()
        {
            if (TournamentManager.Instance != null)
            {
                TournamentManager.Instance.ResetTournamentPoints();
            }

            SceneManager.LoadScene(menuSceneName);
        }
    }
}