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
            [Tooltip("Transform yang akan dipindah/dinaikkan untuk slot rank ini (podium pillar).")]
            public Transform standTransform;

            [Tooltip("Posisi awal (di bawah/tersembunyi) sebelum animasi mulai.")]
            public Vector3 hiddenLocalPosition;

            [Tooltip("Posisi akhir (di atas podium) setelah animasi selesai.")]
            public Vector3 revealedLocalPosition;

            [Tooltip("Label nama/ID player, contoh: 'P1'.")]
            public TMPro.TextMeshProUGUI playerLabel;

            [Tooltip("Label skor tournament, contoh: '7 pts'.")]
            public TMPro.TextMeshProUGUI pointsLabel;

            [Header("Car On Podium")]
            [Tooltip("SpriteRenderer mobil yang ditaruh di atas podium ini.")]
            public SpriteRenderer carRenderer;

            [Tooltip("Local position kosong/idle si car renderer sebelum dimunculkan (opsional, biasanya sama dengan posisi akhirnya, hanya alpha 0).")]
            public bool bobCarWhileIdle = true;
        }

        [Header("Podium Slots (urut: Rank 1, Rank 2, Rank 3, Rank 4)")]
        [SerializeField] private List<PodiumSlot> podiumSlots = new List<PodiumSlot>(4);

        [Header("Sprite Per Player (opsional, index 0 = Player 1)")]
        [Tooltip("Kalau diisi, carRenderer.sprite akan diganti sesuai playerID pemenang slot ini.")]
        [SerializeField] private Sprite[] carSpriteByPlayerIndex;

        [Header("Animation - Rise")]
        [SerializeField] private float delayBeforeStart = 0.5f;
        [SerializeField] private float riseDuration = 0.6f;
        [SerializeField] private float delayBetweenSlots = 0.4f;
        [SerializeField] private AnimationCurve riseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Animation - Car Idle (muter/bobbing pelan setelah naik)")]
        [SerializeField] private bool enableCarIdleAnimation = true;
        [SerializeField] private float carBobAmplitude = 0.08f;
        [SerializeField] private float carBobSpeed = 2f;
        [SerializeField] private float carSpinDegreesPerSecond = 40f;

        [Header("Navigation")]
        [SerializeField] private GameObject continueButtonRoot;

        private readonly List<Coroutine> idleRoutines = new List<Coroutine>();

        private void Start()
        {
           
            if (continueButtonRoot != null)
            {
                continueButtonRoot.SetActive(true);
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

            foreach (PodiumSlot slot in podiumSlots)
            {
                if (slot.carRenderer != null)
                {
                    slot.carRenderer.enabled = false;
                }

                if (slot.playerLabel != null)
                {
                    slot.playerLabel.text = string.Empty;
                }

                if (slot.pointsLabel != null)
                {
                    slot.pointsLabel.text = string.Empty;
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

                RevealLabels(slot, entry.playerID, entry.points);

                if (slot.carRenderer != null)
                {
                    slot.carRenderer.enabled = true;
                }

                if (enableCarIdleAnimation && slot.carRenderer != null && slot.bobCarWhileIdle)
                {
                    Coroutine idle = StartCoroutine(CarIdleRoutine(slot.carRenderer));
                    idleRoutines.Add(idle);
                }

                yield return new WaitForSeconds(delayBetweenSlots);
            }
        }

        private void ApplySlotData(PodiumSlot slot, int playerID, int points)
        {
            if (slot.standTransform != null)
            {
                slot.standTransform.localPosition = slot.hiddenLocalPosition;
            }

            if (slot.carRenderer != null)
            {
                if (carSpriteByPlayerIndex != null
                    && playerID - 1 >= 0
                    && playerID - 1 < carSpriteByPlayerIndex.Length
                    && carSpriteByPlayerIndex[playerID - 1] != null)
                {
                    slot.carRenderer.sprite = carSpriteByPlayerIndex[playerID - 1];
                }
            }
        }

        private void RevealLabels(PodiumSlot slot, int playerID, int points)
        {
            if (slot.playerLabel != null)
            {
                slot.playerLabel.text = $"P{playerID}";
            }

            if (slot.pointsLabel != null)
            {
                PointsLabel_SetText(slot.pointsLabel, points);
            }
        }

        private void PointsLabel_SetText(TMPro.TextMeshProUGUI label, int points)
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

        private IEnumerator CarIdleRoutine(SpriteRenderer carRenderer)
        {
            if (carRenderer == null)
            {
                yield break;
            }

            Transform carTransform = carRenderer.transform;
            Vector3 basePosition = carTransform.localPosition;
            float timeOffset = Random.Range(0f, 10f); // biar tiap mobil tidak persis sinkron

            while (true)
            {
                float t = (Time.time + timeOffset) * carBobSpeed;

                float bobOffset = Mathf.Sin(t) * carBobAmplitude;
                carTransform.localPosition = basePosition + new Vector3(0f, bobOffset, 0f);

                carTransform.Rotate(0f, 0f, carSpinDegreesPerSecond * Time.deltaTime);

                yield return null;
            }
        }

        public void ReturnToMenu()
        {
            if (TournamentManager.Instance != null)
            {
                TournamentManager.Instance.ResetTournamentPoints();
            }

            SceneManager.LoadScene("Menu");
        }
    }
}