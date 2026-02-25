using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

namespace HumanLoop.Core
{
    public class VictoryUIHandler : MonoBehaviour
    {
        [SerializeField] private CanvasGroup victoryPanel;
        [SerializeField] private TextMeshProUGUI statsSummaryText;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private string victoryMessage = "¡You've won! You've evolved into CEO at Human Loop! You've managed to survive these past few weeks at the company!";

        

        private void Start()
        {
            // Ensure the Victory panel is hidden at the start
            victoryPanel.gameObject.SetActive(false);
            victoryPanel.alpha = 0f;
        }

        public void HandleVictory()
        {
             

            victoryPanel.gameObject.SetActive(true);

            // Animación de entrada
            victoryPanel.DOFade(1f, 1f).SetUpdate(true); // SetUpdate(true) allows animation even if timeScale is 0

            if (statsSummaryText != null)
            {
                statsSummaryText.text = victoryMessage + timeManager.CurrentWeek; 
            }

            // Opcional: Detener el spawn de cartas
            // FindObjectOfType<Core.DeckManager>().enabled = false;
        }

        public void BackToMenu()
        {
            SceneManager.LoadScene(0); // O la escena que desees
        }
    }
}