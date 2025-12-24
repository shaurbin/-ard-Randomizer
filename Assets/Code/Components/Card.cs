using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NaughtyAttributes;
using Newtonsoft.Json;
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

        public string Hash()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var component in Components)
            {
                int arg = component.UsedSpriteIndex;
                sb.Append(arg.ToString());
            }

            var hash = sb.ToString();

            return hash;
        }

        public bool UniqueView()
        {
            var hash = Hash();   
            
            if (Hashes.ContainsKey(hash))
                return false;

            Hashes.Add(hash, true);

            return true;
        }

        public List < List < Action>> Setters()
        {
            List< List < Action >> setters = new List <List <Action>> ();

            Components.Sort((a, b) =>
            {
                if (a.Order > b.Order)
                    return 1;
                else
                    return -1;
            });

            foreach (var component in Components)
            {
                setters.Add(component.Setters());
            }

            return setters;
        }

        [Button]
        public void Dispose()
        {
            var components = GetComponentsInChildren<CardComponent>(includeInactive: true);
            foreach (var component in components)
            {
                component.ClearUseOnly();
                component.Order = 0;
                component.LimitForCount = 0;
            }
        }

        [Button]
        public void SaveStates()
        {
            var components = GetComponentsInChildren<CardComponent>(includeInactive: true);

            List<StateWrapper> states = new List<StateWrapper>();

            foreach (var component in components)
            {
                StateWrapper state = new StateWrapper
                    (
                    component.Id,
                    component.LimitForCount,
                    component.Order
                    );

                states.Add( state );
            }

            var json = JsonConvert.SerializeObject(states);
            PlayerPrefs.SetString(COMPONENT_STATES, json);
            PlayerPrefs.Save();
        }

        private const string COMPONENT_STATES = "component_states";
        
        public class StateWrapper
        {
            public int Id;
            public int LimitForCount;
            public int Order;            
            
            public StateWrapper(int Id, int limitForCount, int order)
            {
                this.Id = Id;
                this.LimitForCount = limitForCount;
                this.Order = order;
            }
        }

        [Button]
        public void LoadStates()
        {
            if (!PlayerPrefs.HasKey(COMPONENT_STATES)) return;

            var json = PlayerPrefs.GetString(COMPONENT_STATES);
            var obj = JsonConvert.DeserializeObject<List<StateWrapper>>(json);

            var components = GetComponentsInChildren<CardComponent>(includeInactive: true);
            Dictionary<int, StateWrapper> states = new Dictionary<int, StateWrapper>();

            foreach (var o in obj)
            {
                states.Add(o.Id, o);
            }
            foreach (var component in components)
            {
                var state = states[component.Id];
                component.LimitForCount = state.LimitForCount;
                component.Order = state.Order;
            }
        }

        //[Button]
        //public void PrepareIDS()
        //{
        //    var components = GetComponentsInChildren<CardComponent>(includeInactive: true);
        //    int seqNumber = 0;
        //    foreach ( var component in components)
        //    {
        //        component.Id = seqNumber++;
        //    }
        //}
    }
}
