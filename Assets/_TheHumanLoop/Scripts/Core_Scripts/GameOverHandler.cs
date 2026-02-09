using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HumanLoop.Core
{
    public class GameOverHandler : MonoBehaviour
    {
        [SerializeField] private CanvasGroup gameOverPanel;
        [SerializeField] private TextMeshProUGUI statsSummaryText;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private string fireMessage = "¡Ve ha hablar con recursos humanos! ¡No has conseguido pasar el corte! Semanas en la empresa:";


        private void Start()
        {
            // Ensure the Game Over panel is hidden at the start
            gameOverPanel.gameObject.SetActive(false);
            gameOverPanel.alpha = 0f;
        }

        /// <summary>
        /// This method will be linked to the GameEventListener's UnityEvent in the Inspector.
        /// </summary>
        ///       
        public void HandleGameOver()
        {
            Debug.Log("GAME OVER: Displaying UI...");

            gameOverPanel.gameObject.SetActive(true);
            gameOverPanel.DOFade(1f, 1f).SetUpdate(true); // SetUpdate(true) allows animation even if timeScale is 0

            if (statsSummaryText != null)
            {
                statsSummaryText.text = fireMessage + timeManager.CurrentWeek;
            }

            // Optional: Pause the game logic
            // Time.timeScale = 0; 
        }

        public void RestartGame()
        {
            // Reset time and reload scene
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
