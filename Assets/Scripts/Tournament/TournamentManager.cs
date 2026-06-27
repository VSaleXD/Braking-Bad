using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BrakingBad.Gameplay
{
    /// Persistent tournament state machine that selects the 3 minigames, tracks tournament points,
    /// and advances the scene flow after each match resolves.
    public sealed class TournamentManager : MonoBehaviour
    {
        public static TournamentManager Instance { get; private set; }

        [Header("Tournament Setup")]
        [SerializeField] private List<string> minigamePool = new List<string>
        {
            "Minigame_CarSoccer",
            "Minigame_DriftMadness",
            "Minigame_ObstacleSurvival",
            "Minigame_CarSumo",
            "Minigame_ChaseTheUFO",
            "Minigame_PortalRush",
            "Minigame_FloorIsLava",
            "Minigame_Spotlight",
            "Minigame_CaptureTheFlag",
            "Minigame_MirrorDimension"
        };

        [SerializeField] private string finalPodiumSceneName = "FinalPodiumScene";
        [SerializeField] private bool autoBeginTournament = false;

        private readonly List<string> selectedMinigames = new List<string>(3);
        private readonly int[] tournamentPoints = new int[4];
        private int currentMatchIndex;
        private bool selectionReady;

        public IReadOnlyList<string> SelectedMinigames => selectedMinigames;
        public IReadOnlyList<int> TournamentPoints => tournamentPoints;
        public int CurrentMatchIndex => currentMatchIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsurePoolHasDefaults();
            if (selectedMinigames.Count == 0)
            {
                ShuffleAndSelectMinigames();
            }
        }

        private void Start()
        {
            if (autoBeginTournament && selectedMinigames.Count == 3)
            {
                BeginTournament();
            }
        }

        public void EnsurePoolHasDefaults()
        {
            if (minigamePool == null)
            {
                minigamePool = new List<string>();
            }

            if (minigamePool.Count == 0)
            {
                minigamePool.AddRange(new[]
                {
                    "Minigame_CarSoccer",
                    "Minigame_DriftMadness",
                    "Minigame_ObstacleSurvival",
                    "Minigame_CarSumo",
                    "Minigame_ChaseTheUFO",
                    "Minigame_PortalRush",
                    "Minigame_FloorIsLava",
                    "Minigame_Spotlight",
                    "Minigame_CaptureTheFlag",
                    "Minigame_MirrorDimension"
                });
            }
        }

        public List<string> ShuffleAndSelectMinigames()
        {
            EnsurePoolHasDefaults();

            if (minigamePool.Count < 3)
            {
                Debug.LogError("TournamentManager requires at least 3 minigames in the pool.");
                selectedMinigames.Clear();
                selectionReady = false;
                return new List<string>();
            }

            var shuffled = minigamePool.Where(sceneName => !string.IsNullOrWhiteSpace(sceneName)).ToList();
            for (int i = 0; i < shuffled.Count; i++)
            {
                int swapIndex = Random.Range(i, shuffled.Count);
                (shuffled[i], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[i]);
            }

            selectedMinigames.Clear();
            selectedMinigames.AddRange(shuffled.Take(3));
            currentMatchIndex = 0;
            selectionReady = true;
            return new List<string>(selectedMinigames);
        }

        public void BeginTournament()
        {
            if (!selectionReady || selectedMinigames.Count != 3)
            {
                ShuffleAndSelectMinigames();
            }

            currentMatchIndex = 0;
            LoadSceneSafe(selectedMinigames[currentMatchIndex]);
        }

        public void ResetTournamentPoints()
        {
            for (int i = 0; i < tournamentPoints.Length; i++)
            {
                tournamentPoints[i] = 0;
            }
        }

        public void ResolveMinigame(List<PlayerMatchResult> results)
        {
            if (results == null || results.Count == 0)
            {
                results = new List<PlayerMatchResult>
                {
                    new PlayerMatchResult(1, 0f),
                    new PlayerMatchResult(2, 0f),
                    new PlayerMatchResult(3, 0f),
                    new PlayerMatchResult(4, 0f)
                };
            }

            var normalizedResults = NormalizeResults(results);
            ApplyTournamentPoints(normalizedResults);

            currentMatchIndex++;

            string nextSceneName = currentMatchIndex < selectedMinigames.Count
                ? selectedMinigames[currentMatchIndex]
                : finalPodiumSceneName;

            LoadSceneSafe(nextSceneName);
        }

        private List<PlayerMatchResult> NormalizeResults(IEnumerable<PlayerMatchResult> results)
        {
            var scoreByPlayer = new Dictionary<int, float>
            {
                { 1, 0f },
                { 2, 0f },
                { 3, 0f },
                { 4, 0f }
            };

            foreach (var result in results)
            {
                if (result == null)
                {
                    continue;
                }

                if (result.playerID < 1 || result.playerID > 4)
                {
                    continue;
                }

                scoreByPlayer[result.playerID] = result.gameplayScore;
            }

            return scoreByPlayer
                .Select(pair => new PlayerMatchResult(pair.Key, pair.Value))
                .OrderByDescending(result => result.gameplayScore)
                .ThenBy(result => result.playerID)
                .ToList();
        }

        private void ApplyTournamentPoints(IReadOnlyList<PlayerMatchResult> sortedResults)
        {
            const float tieEpsilon = 0.0001f;
            int resultIndex = 0;

            while (resultIndex < sortedResults.Count)
            {
                int groupStart = resultIndex;
                float groupScore = sortedResults[groupStart].gameplayScore;

                while (resultIndex < sortedResults.Count && Mathf.Abs(sortedResults[resultIndex].gameplayScore - groupScore) <= tieEpsilon)
                {
                    resultIndex++;
                }

                int placement = groupStart + 1;
                int awardedPoints = PlacementToTournamentPoints(placement);

                for (int i = groupStart; i < resultIndex; i++)
                {
                    int playerIndex = Mathf.Clamp(sortedResults[i].playerID - 1, 0, tournamentPoints.Length - 1);
                    tournamentPoints[playerIndex] += awardedPoints;
                }
            }
        }

        private int PlacementToTournamentPoints(int placement)
        {
            switch (placement)
            {
                case 1:
                    return 3;
                case 2:
                    return 2;
                case 3:
                    return 1;
                default:
                    return 0;
            }
        }

        private void LoadSceneSafe(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("TournamentManager cannot load an empty scene name.");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"Scene '{sceneName}' is not present in Build Settings.");
            }

            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
    }
}