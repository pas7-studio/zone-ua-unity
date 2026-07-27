using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZoneUA.Combat
{
    [CreateAssetMenu(fileName = "DamageResistanceProfile", menuName = "Zone UA/Combat/Damage Resistance Profile")]
    public sealed class DamageResistanceProfile : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            [SerializeField] private DamageType damageType;
            [SerializeField, Range(-1f, 1f), Tooltip("Positive values reduce damage. Negative values increase it.")]
            private float resistance;

            public DamageType DamageType => damageType;
            public float Resistance => Mathf.Clamp(resistance, -1f, 1f);
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public float GetResistance(DamageType damageType)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                if (entry != null && entry.DamageType == damageType)
                {
                    return entry.Resistance;
                }
            }

            return 0f;
        }

        private void OnValidate()
        {
            var seen = new HashSet<DamageType>();
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                Entry entry = entries[i];
                if (entry == null || !seen.Add(entry.DamageType))
                {
                    entries.RemoveAt(i);
                }
            }
        }
    }
}
