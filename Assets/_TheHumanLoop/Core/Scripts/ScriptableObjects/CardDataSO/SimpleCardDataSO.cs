using UnityEngine;

namespace HumanLoop.Data
{
    [CreateAssetMenu(fileName = "NewCard", menuName = "The Human Loop/Cards/SimpleCard")]
    public class SimpleCardData : CardDataSO
    {
        // You don't need to override ApplyEffect unless you want extra logic.
        // The base.ApplyEffect() will handle the stats.
        [Header("Localizations IDs")]
        [SerializeField] private string titleID;
        [SerializeField] private string descriptionID;
        [SerializeField] private string leftChoiceID;
        [SerializeField] private string rightChoiceID;

        public string TitleID => titleID;
        public string DescriptionID => descriptionID;
        public string LeftChoiceID => leftChoiceID;
        public string RightChoiceID => rightChoiceID;
    }
}

