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
        [SerializeField] private string victoryMessage = "¡Has ganado! ¡Has evoluvionado hasta CEO en Human Loop! Has conseguido sobrevir en la empresa estas semanas:";

        

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