using UnityEngine;

namespace HumanLoop.Data
{
    [CreateAssetMenu(fileName = "NewCard", menuName = "The Human Loop/Cards/SimpleCard")]
    public class SimpleCardData : CardDataSO
    {
        // You don't need to override ApplyEffect unless you want extra logic.
        // The base.ApplyEffect() will handle the stats.
    }
}

