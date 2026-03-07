using UnityEngine;
using System.Collections.Generic;
using HumanLoop.Data;

namespace HumanLoop.Visuals
{
    [System.Serializable]
    public struct CategoryStyle
    {
        public CardCategory category;
        public Color themeColor;
        public Sprite categoryIcon;
    }

    [CreateAssetMenu(fileName = "CategorySettings", menuName = "The Human Loop/UI/Category Settings")]
    public class CardCategorySettingsSO : ScriptableObject
    {
        public List<CategoryStyle> styles;

        public CategoryStyle GetStyle(CardCategory category)
        {
            return styles.Find(s => s.category == category);
        }
    }
}