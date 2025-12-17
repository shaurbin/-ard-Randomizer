using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Code.Components
{
    [DisallowMultipleComponent]
    public class Card : MonoBehaviour
    {
        [HideInInspector] public List<CardComponent> Components;
        
        [HideInInspector] public bool Prepared;

        private Dictionary<string, bool> Hashes = new Dictionary<string, bool>();

        private async void Awake()
        {
            Components = GetComponentsInChildren<CardComponent>(includeInactive: true).ToList();

            foreach (var component in Components) 
            {
                await component.Prepare();
            }

            Prepared = true;
        }

        public bool UniqueView()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var component in Components)
            {
                int arg = component.UsedSpriteIndex;
                sb.Append(arg.ToString());
            }

            //Debug.Log(sb.ToString());
            var hash = sb.ToString();
            
            if (Hashes.ContainsKey(hash))
                return false;

            Hashes.Add(hash, true);

            return true;
        }

        public void Build()
        {
            foreach (var component in Components)
            {
                component.Build();
            }
        }
    }
}
