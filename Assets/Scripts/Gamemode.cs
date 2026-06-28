using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Ditempel di scene Gamemode. Dipanggil oleh tombol "2 PLAYER MODE" dan
    /// "4 PLAYER MODE" lewat Button.OnClick di Inspector.
    public sealed class Gamemode : MonoBehaviour
    {
        public void StartTwoPlayerMode()
        {
            StartTournamentWithPlayerCount(2);
        }

        public void StartFourPlayerMode()
        {
            StartTournamentWithPlayerCount(4);
        }
        public void BackToMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
        private void StartTournamentWithPlayerCount(int playerCount)
        {
            if (TournamentManager.Instance == null)
            {
                Debug.LogError("ModeGameButtons: TournamentManager.Instance tidak ditemukan. " +
                                "Pastikan scene awal (MenuBaru) sudah punya GameObject dengan TournamentManager.");
                return;
            }

            TournamentManager.Instance.SetPlayerCountAndBegin(playerCount);
        }
    }
}