using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Code.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class CardComponent : MonoBehaviour
    {
        public string Path;
        [HideInInspector] public SpriteRenderer SpriteRenderer;
        public List<SpriteWrapper> Sprites;
        private bool prepared;
        
        private int index;
        private bool finished;

        public bool Finished => finished;

        public bool Prepared => prepared;

        public int Index => index;

        public int Size => Sprites.Count;

        [HideInInspector] public int UsedSpriteIndex;

        [SerializeField] private List<DependentElements> dependents = new List<DependentElements>();

        public List<DependentElements> Dependents => dependents;

        //private Dictionary<string, List<SpriteWrapper>> sp = new Dictionary<string, List<SpriteWrapper>>();

        public UniTask<bool> Prepare()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            var items = Resources.LoadAll(Path, typeof(Sprite));
            Sprites = new List<SpriteWrapper>();

            foreach (var item in items)
            {
                var path = AssetDatabase.GetAssetPath(item);
                FileName file = new FileName(path);
                Sprites.Add(new()
                {
                    FileName = file,
                    Sprite = (Sprite)item
                });
            }

            if (dependents.Count > 0)
            {
                foreach (var dependent in dependents)
                {
                    var dependentSpriteObjs = Resources.LoadAll(dependent.Path, typeof(Sprite));
                    foreach (var dso in dependentSpriteObjs)
                    {
                        var dpath = AssetDatabase.GetAssetPath(dso);
                        FileName dfile = new FileName(dpath);
                        dependent.Sprites.Add(dfile.Number, new()
                        {
                            FileName = dfile,
                            Sprite = (Sprite)dso
                        });
                    }
                }
            }

            prepared = true;

            return UniTask.FromResult(true);
        }

        public void Refresh()
        {
            index = 0;
            UsedSpriteIndex = 0;
        }

        //public bool NextSprite()
        //{
        //    if (finished) return false;

        //    if (index > 0) UsedSpriteIndex++;

        //    index++;
        //    if (index >= Sprites.Count)
        //        finished = true;
            
        //    return true;
        //}

        public void Build()
        {
            SpriteRenderer.sprite = Sprites[UsedSpriteIndex].Sprite;
            if (dependents.Count > 0)
            {
                foreach (var dependent in dependents)
                {
                    dependent.SpriteRenderer.sprite = dependent.Sprites[Sprites[UsedSpriteIndex].FileName.Number].Sprite;
                }
            }
        }

        public List<Action> Setters()
        {
            List<Action> list = new List<Action>();

            foreach (var s in Sprites)
            {
                Action a = () => SpriteRenderer.sprite = s.Sprite;
                foreach (var dependent in dependents)
                {
                    a += () => dependent.SpriteRenderer.sprite = dependent.Sprites[s.FileName.Number].Sprite;
                }

                list.Add(a);
            }

            return list;
        }

        [Serializable]
        public class DependentElements
        {
            public SpriteRenderer SpriteRenderer;
            public string Path;
            [HideInInspector] public Dictionary<int, SpriteWrapper> Sprites = new Dictionary<int, SpriteWrapper>();
        }
    }
}
