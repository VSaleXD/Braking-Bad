using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace BrakingBad.Gameplay
{
    /// Shared minigame loop: local countdown timer, UI Toolkit score/timer display, and result handoff.
    public abstract class BaseMinigameManager : MonoBehaviour
    {
        [Header("Match Clock")]
        [SerializeField] protected float matchDuration = 90f;
        [SerializeField] protected bool autoStartMatch = true;

        [Header("UI Toolkit")]
        [SerializeField] protected UIDocument uiDocument;
        [SerializeField] protected string scoreTextName = "ScoreText";
        [SerializeField] protected string timerTextName = "TimerText";
        [SerializeField] protected string comboTextName = "ComboText";

        protected float matchTimer = 90f;
        private bool isMatchStarted = false;

        private readonly Dictionary<int, float> playerScores = new Dictionary<int, float>(4)
        {
            { 1, 0f },
            { 2, 0f },
            { 3, 0f },
            { 4, 0f }
        };

        private Label scoreLabel;
        private Label timerLabel;
        private Label comboLabel;
        private Coroutine comboRoutine;
        private bool matchComplete;

        protected virtual void Awake()
        {
            CachePlayerAgents();
            ResetScoreState();
        }

        protected virtual void Start()
        {
            CacheUI();

            matchTimer = matchDuration;
            UpdateTimerUI();
            isMatchStarted = false;
        }

        protected virtual void Update()
        {
            if (matchComplete || !isMatchStarted)
            {
                return;
            }

            matchTimer -= Time.deltaTime;
            UpdateTimerUI();

            if (matchTimer <= 0f)
            {
                CompleteMatch();
            }
        }

        public void StartMinigameMatch()
        {
            isMatchStarted = true;
            BeginMatch();
        }
        public void BeginMatch()
        {
            CachePlayerAgents();
            ResetScoreState();
            matchComplete = false;
            matchTimer = matchDuration;

            OnMatchStarted();
            RefreshScoreboardUI();
            UpdateTimerUI();
        }

        protected virtual void OnMatchStarted()
        {
        }

        protected virtual void OnMatchEnded()
        {
        }

        protected void AddGameplayScore(int playerID, float delta, string comboMessage = null)
        {
            if (!playerScores.ContainsKey(playerID))
            {
                playerScores[playerID] = 0f;
            }

            playerScores[playerID] += delta;
            RefreshScoreboardUI();

            if (!string.IsNullOrWhiteSpace(comboMessage))
            {
                ShowComboMessage(comboMessage);
            }
        }

        protected void SetGameplayScore(int playerID, float score)
        {
            playerScores[playerID] = score;
            RefreshScoreboardUI();
        }

        protected float GetGameplayScore(int playerID)
        {
            return playerScores.TryGetValue(playerID, out float score) ? score : 0f;
        }

        protected TournamentPlayerAgent GetPlayerAgent(int playerID)
        {
            return FindObjectsByType<TournamentPlayerAgent>(FindObjectsSortMode.None)
                .FirstOrDefault(agent => agent != null && agent.PlayerID == playerID);
        }

        protected IEnumerable<TournamentPlayerAgent> GetRegisteredPlayers()
        {
            return FindObjectsByType<TournamentPlayerAgent>(FindObjectsSortMode.None)
                .Where(agent => agent != null)
                .OrderBy(agent => agent.PlayerID);
        }

        protected virtual List<PlayerMatchResult> CollectFinalScores()
        {
            return playerScores
                .OrderBy(pair => pair.Key)
                .Select(pair => new PlayerMatchResult(pair.Key, pair.Value))
                .ToList();
        }

        protected void CompleteMatch()
        {
            if (matchComplete)
            {
                return;
            }

            matchComplete = true;
            matchTimer = 0f;
            UpdateTimerUI();
            OnMatchEnded();

            if (TournamentManager.Instance != null)
            {
                TournamentManager.Instance.ResolveMinigame(CollectFinalScores());
            }
            else
            {
                Debug.LogError($"{name} cannot resolve match because TournamentManager.Instance is missing.");
            }
        }

        protected void ResetScoreState()
        {
            var playerIds = playerScores.Keys.ToList();
            foreach (int playerID in playerIds)
            {
                playerScores[playerID] = 0f;
            }

            RefreshScoreboardUI();
        }

        protected void CachePlayerAgents()
        {
            foreach (TournamentPlayerAgent agent in GetRegisteredPlayers())
            {
                if (!playerScores.ContainsKey(agent.PlayerID))
                {
                    playerScores.Add(agent.PlayerID, 0f);
                }
            }

            for (int playerID = 1; playerID <= 4; playerID++)
            {
                if (!playerScores.ContainsKey(playerID))
                {
                    playerScores.Add(playerID, 0f);
                }
            }
        }

        protected void ShowComboMessage(string message, float duration = 1.5f)
        {
            if (comboLabel == null)
            {
                return;
            }

            if (comboRoutine != null)
            {
                StopCoroutine(comboRoutine);
            }

            comboRoutine = StartCoroutine(ComboMessageRoutine(message, duration));
        }

        private System.Collections.IEnumerator ComboMessageRoutine(string message, float duration)
        {
            comboLabel.text = message;
            comboLabel.style.display = DisplayStyle.Flex;

            yield return new WaitForSeconds(duration);

            comboLabel.text = string.Empty;
            comboLabel.style.display = DisplayStyle.None;
            comboRoutine = null;
        }

        private void CacheUI()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument == null)
            {
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;
            scoreLabel = root.Q<Label>(scoreTextName);
            timerLabel = root.Q<Label>(timerTextName);
            comboLabel = root.Q<Label>(comboTextName);

            if (comboLabel != null)
            {
                comboLabel.style.display = DisplayStyle.None;
            }

            RefreshScoreboardUI();
            UpdateTimerUI();
        }

        private void RefreshScoreboardUI()
        {
            if (scoreLabel == null)
            {
                return;
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendLine("Score");

            foreach (var entry in playerScores.OrderBy(pair => pair.Key))
            {
                builder.Append("P").Append(entry.Key).Append(": ").Append(entry.Value.ToString("0")).AppendLine();
            }

            scoreLabel.text = builder.ToString().TrimEnd();
        }

        private void UpdateTimerUI()
        {
            if (timerLabel == null)
            {
                return;
            }

            int seconds = Mathf.CeilToInt(Mathf.Max(0f, matchTimer));
            timerLabel.text = $"Time: {seconds:00}";
        }
    }
}