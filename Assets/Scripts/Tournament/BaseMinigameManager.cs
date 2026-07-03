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
        [SerializeField] protected float matchDuration = 30f;
        [SerializeField] protected bool autoStartMatch = true;

        [Header("UI Toolkit")]
        [SerializeField] protected UIDocument uiDocument;
        [SerializeField] protected string scoreTextName = "ScoreText";
        [SerializeField] protected string timerTextName = "TimerText";
        [SerializeField] protected string comboContainerName = "ComboContainer";
        [SerializeField] protected string comboTextName = "ComboText";
        [SerializeField] protected string pickupTextName = "PickupText";

        protected float matchTimer = 90f;
        protected bool isMatchStarted = false;
        public bool IsMatchStarted => isMatchStarted;

        private readonly Dictionary<int, float> playerScores = new Dictionary<int, float>(4)
        {
            { 1, 0f },
            { 2, 0f },
            { 3, 0f },
            { 4, 0f }
        };

        private Label scoreLabel;
        private Label timerLabel;
        private VisualElement comboContainer;
        private Label comboLabel;
        private Label pickupLabel;
        private Coroutine comboRoutine;
        private Coroutine pickupRoutine;
        private bool matchComplete;

        protected virtual void Awake()
        {
            CachePlayerAgents();
            ResetScoreState();
        }

        protected virtual void OnEnable()
        {
            PowerUpItem.PowerUpPickedUp += HandlePowerUpPickedUp;
        }

        protected virtual void OnDisable()
        {
            PowerUpItem.PowerUpPickedUp -= HandlePowerUpPickedUp;
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
            if(CheckAllPlayersEliminated())
            {
                matchTimer = 0f;
                UpdateTimerUI();
                CompleteMatch();
                return;
            }   
            
            matchTimer -= Time.deltaTime;
            UpdateTimerUI();

            if (matchTimer <= 0f)
            {
                matchTimer = 0f;
                UpdateTimerUI();
                CompleteMatch();
            }
        }
        private bool CheckAllPlayersEliminated()
        {
            foreach (TournamentPlayerAgent agent in GetRegisteredPlayers())
            {
                if (agent != null && !agent.IsEliminated)
                {
                    return false;
                }
            }
            return true; 
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
            ShowComboMessage(message, Color.white, duration);
        }

        protected void ShowComboMessage(string message, Color accentColor, float duration = 1.5f)
        {
            if (comboLabel == null)
            {
                return;
            }

            if (comboRoutine != null)
            {
                StopCoroutine(comboRoutine);
            }

            comboRoutine = StartCoroutine(ComboMessageRoutine(message, accentColor, duration));
        }

        protected void ShowPickupMessage(PowerUpType type, float duration = 1.5f)
        {
            if (pickupLabel == null)
            {
                return;
            }

            if (pickupRoutine != null)
            {
                StopCoroutine(pickupRoutine);
            }

            string message = GetPickupMessage(type);
            Color color = PowerUpItem.GetPowerUpColor(type);
            pickupRoutine = StartCoroutine(PickupMessageRoutine(message, color, duration));
        }

        private System.Collections.IEnumerator ComboMessageRoutine(string message, Color accentColor, float duration)
        {
            float baseFontSize = comboLabel.resolvedStyle.fontSize;
            float popFontSize = baseFontSize * 1.12f;

            comboLabel.text = message;
            comboLabel.style.color = accentColor;
            comboLabel.style.display = DisplayStyle.Flex;

            if (comboContainer != null)
            {
                comboContainer.style.display = DisplayStyle.Flex;
                comboContainer.style.opacity = 0f;
            }

            comboLabel.style.opacity = 0f;
            comboLabel.style.fontSize = baseFontSize * 0.85f;
            comboLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            comboLabel.style.display = DisplayStyle.Flex;

            const float fadeInDuration = 0.14f;
            const float fadeOutDuration = 0.18f;
            float holdDuration = Mathf.Max(0f, duration - fadeInDuration - fadeOutDuration);

            for (float elapsed = 0f; elapsed < fadeInDuration; elapsed += Time.deltaTime)
            {
                float t = elapsed / fadeInDuration;
                float eased = Mathf.SmoothStep(0f, 1f, t);

                if (comboContainer != null)
                {
                    comboContainer.style.opacity = eased;
                }

                comboLabel.style.opacity = eased;
                comboLabel.style.fontSize = Mathf.Lerp(baseFontSize * 0.85f, popFontSize, eased);
                yield return null;
            }

            if (comboContainer != null)
            {
                comboContainer.style.opacity = 1f;
            }

            comboLabel.style.opacity = 1f;
            comboLabel.style.fontSize = popFontSize;

            if (holdDuration > 0f)
            {
                yield return new WaitForSeconds(holdDuration);
            }

            for (float elapsed = 0f; elapsed < fadeOutDuration; elapsed += Time.deltaTime)
            {
                float t = elapsed / fadeOutDuration;
                float eased = 1f - Mathf.SmoothStep(0f, 1f, t);

                if (comboContainer != null)
                {
                    comboContainer.style.opacity = eased;
                }

                comboLabel.style.opacity = eased;
                comboLabel.style.fontSize = Mathf.Lerp(popFontSize, baseFontSize, t);
                yield return null;
            }

            comboLabel.text = string.Empty;
            comboLabel.style.display = DisplayStyle.None;
            comboLabel.style.opacity = 1f;
            comboLabel.style.fontSize = baseFontSize;
            if (comboContainer != null)
            {
                comboContainer.style.display = DisplayStyle.None;
                comboContainer.style.opacity = 1f;
            }
            comboRoutine = null;
        }

        private System.Collections.IEnumerator PickupMessageRoutine(string message, Color color, float duration)
        {
            float baseFontSize = pickupLabel.resolvedStyle.fontSize;
            float popFontSize = baseFontSize * 1.15f;

            pickupLabel.text = message;
            pickupLabel.style.color = color;
            pickupLabel.style.display = DisplayStyle.Flex;
            pickupLabel.style.opacity = 0f;
            pickupLabel.style.fontSize = baseFontSize * 0.85f;

            const float fadeInDuration = 0.15f;
            const float fadeOutDuration = 0.2f;
            float holdDuration = Mathf.Max(0f, duration - fadeInDuration - fadeOutDuration);

            for (float elapsed = 0f; elapsed < fadeInDuration; elapsed += Time.deltaTime)
            {
                float t = elapsed / fadeInDuration;
                pickupLabel.style.opacity = Mathf.Lerp(0f, 1f, t);
                pickupLabel.style.fontSize = Mathf.Lerp(baseFontSize * 0.85f, popFontSize, t);
                yield return null;
            }

            pickupLabel.style.opacity = 1f;
            pickupLabel.style.fontSize = popFontSize;

            if (holdDuration > 0f)
            {
                yield return new WaitForSeconds(holdDuration);
            }

            for (float elapsed = 0f; elapsed < fadeOutDuration; elapsed += Time.deltaTime)
            {
                float t = elapsed / fadeOutDuration;
                pickupLabel.style.opacity = Mathf.Lerp(1f, 0f, t);
                pickupLabel.style.fontSize = Mathf.Lerp(popFontSize, baseFontSize, t);
                yield return null;
            }

            pickupLabel.text = string.Empty;
            pickupLabel.style.display = DisplayStyle.None;
            pickupLabel.style.opacity = 1f;
            pickupLabel.style.fontSize = baseFontSize;
            pickupRoutine = null;
        }

        private void HandlePowerUpPickedUp(PowerUpType type, Vector3 position)
        {
            ShowPickupMessage(type);
        }

        private string GetPickupMessage(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.SpeedSurge:
                    return "SPEED SURGE!";
                case PowerUpType.Freeze:
                    return "FREEZE!";
                case PowerUpType.Shield:
                    return "SHIELD!";
                default:
                    return "POWER UP!";
            }
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
            comboContainer = root.Q<VisualElement>(comboContainerName);
            comboLabel = root.Q<Label>(comboTextName);
            pickupLabel = root.Q<Label>(pickupTextName);

            if (comboContainer != null)
            {
                comboContainer.style.display = DisplayStyle.None;
            }

            if (comboLabel != null)
            {
                comboLabel.style.display = DisplayStyle.None;
            }

            if (pickupLabel != null)
            {
                pickupLabel.style.display = DisplayStyle.None;
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
            timerLabel.text = $"{seconds:00}";
        }
    }
}