using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace TheHumanLoop.UI
{
    public class FloatingMenu : MonoBehaviour
    {
        [Header("Configuración de Lista")]
        [SerializeField] private RectTransform container; // El objeto con Vertical Layout Group
        [SerializeField] private List<RectTransform> menuButtons;
        [SerializeField] private float staggerDelay = 0.05f;
        [SerializeField] private float animationDuration = 0.4f;

        [Header("Botón Principal")]
        [SerializeField] private RectTransform mainButton;

        private bool isOpen = false;

        void Start()
        {
            // Inicializamos: Lista invisible y botones a escala 0
            container.gameObject.SetActive(false);
            foreach (var btn in menuButtons)
            {
                btn.localScale = Vector3.zero;
            }
        }

        public void ToggleMenu()
        {
            // Important: Stop any running tween on the container and buttons 
            // to prevent "stuttering" if the user clicks too fast.
            container.DOKill();
            foreach (var btn in menuButtons) btn.DOKill();

            isOpen = !isOpen;

            if (isOpen)
                OpenMenu();
            else
                CloseMenu();

            // Animación de feedback para el botón principal
            mainButton.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 1);
            // Opcional: Rotar el botón principal 45 grados si es un "+"
            mainButton.DORotate(new Vector3(0, 0, isOpen ? 45f : 0f), animationDuration, RotateMode.Fast);
        }

        private void OpenMenu()
        {
            container.gameObject.SetActive(true);

            for (int i = 0; i < menuButtons.Count; i++)
            {
                menuButtons[i].DOKill(); // Limpieza
                                         // Efecto Cascada (Stagger)
                menuButtons[i].DOScale(1f, animationDuration)
                    .SetEase(Ease.OutBack)
                    .SetDelay(i * staggerDelay);
            }
        }

        private void CloseMenu()
        {
            // Al cerrar, invertimos el orden (el de arriba primero)
            for (int i = 0; i < menuButtons.Count; i++)
            {
                int index = (menuButtons.Count - 1) - i; // Invertir índice
                menuButtons[index].DOKill();

                menuButtons[index].DOScale(0f, animationDuration * 0.7f)
                    .SetEase(Ease.InBack)
                    .SetDelay(i * staggerDelay)
                    .OnComplete(() => {
                        if (index == 0) container.gameObject.SetActive(false);
                    });
            }
        }
    }
}
