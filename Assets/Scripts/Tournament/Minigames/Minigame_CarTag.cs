using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BrakingBad.Gameplay
{
    public class Minigame_CarTag : BaseMinigameManager
    {
        [Header("Car Tag Settings")]
        [SerializeField] private float tagCooldown = 1.0f;
        [SerializeField] private float pointsPerSecondNotIt = 1f;

        private int currentItPlayerID;
        private float lastTagTime;
        private Dictionary<int, bool> isItMap = new Dictionary<int, bool>();

        protected override void OnMatchStarted()
        {
            var players = GetRegisteredPlayers().ToList();
            currentItPlayerID = players[Random.Range(0, players.Count)].PlayerID;
            lastTagTime = -tagCooldown;

            foreach (var p in players)
                isItMap[p.PlayerID] = (p.PlayerID == currentItPlayerID);

            UpdateItVisuals();
        }

        protected override void Update()
        {
            base.Update();
            if (!IsMatchStarted) return;

            foreach (var player in GetRegisteredPlayers())
            {
                if (player.PlayerID != currentItPlayerID)
                {
                    AddGameplayScore(player.PlayerID, pointsPerSecondNotIt * Time.deltaTime);
                }
            }
        }

        public void TryTag(int taggerID, int taggedID)
        {
            if (taggerID != currentItPlayerID) return; // hanya IT yang bisa nge-tag
            if (Time.time - lastTagTime < tagCooldown) return;
            if (taggedID == currentItPlayerID) return;

            lastTagTime = Time.time;
            currentItPlayerID = taggedID;

            foreach (var id in isItMap.Keys.ToList())
                isItMap[id] = (id == currentItPlayerID);

            ShowComboMessage($"P{taggedID} is IT!");
            UpdateItVisuals();
        }

        public bool IsPlayerIt(int playerID) => currentItPlayerID == playerID;

        private void UpdateItVisuals()
        {
            foreach (var player in GetRegisteredPlayers())
            {
                var state = player.GetComponent<carState>();
                if (state != null) state.SetItVisual(player.PlayerID == currentItPlayerID);
            }
        }
    }
}