using System;
using UnityEngine;

namespace Studio.Core
{
    [Serializable]
    public struct TagPair
    {
        public string a;
        public string b;
    }

    [CreateAssetMenu(menuName = "Studio/ContentDB", fileName = "ContentDB")]
    public class ContentDB : ScriptableObject
    {
        public MessageSO brief;
        public CardSO[] cards = new CardSO[0];
        public ComboSO[] combos = new ComboSO[0];
        public MessageSO[] messages = new MessageSO[0];
        public QuoteSO[] quotes = new QuoteSO[0];
        public TagPair[] tagConflicts = new TagPair[0];
        public int startBudget;
        public int deadline;
    }
}
