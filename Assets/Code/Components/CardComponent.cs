using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Assets.Code.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class CardComponent : MonoBehaviour
    {
        [ReadOnly] public int Id;

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

        public List<Sprite> UseOnly = new List<Sprite>();
        private List<string> UseOnlyNames = new List<string>();

        public int LimitForCount;

        public int Order;

        [Button]
        public void ClearUseOnly()
        {
            UseOnly.Clear();
            UseOnlyNames.Clear();
        }

        public UniTask<bool> Prepare()
        {
            if (UseOnly.Count > 0)
                PrepareWithUseOnly();
            else if (LimitForCount > 0)
                PrepareWithLimit();
            else
                PrepareWithUseOnly();

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

        private void PrepareWithLimit()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            var items = Resources.LoadAll(Path, typeof(Sprite));
            Sprites = new List<SpriteWrapper>();

            int counter = 0;
            foreach (var item in items)
            {
                var path = AssetDatabase.GetAssetPath(item);
                FileName file = new FileName(path);

                Sprites.Add(new()
                {
                    FileName = file,
                    Sprite = (Sprite)item
                });

                counter++;

                if (counter == LimitForCount)
                    break;
            }
        }

        private void PrepareWithUseOnly()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            var items = Resources.LoadAll(Path, typeof(Sprite));
            Sprites = new List<SpriteWrapper>();

            foreach (var uo in UseOnly)
            {
                var path = AssetDatabase.GetAssetPath(uo);
                FileName file = new FileName(path);
                UseOnlyNames.Add(file.NameWithoutExt);
            }

            foreach (var item in items)
            {
                var path = AssetDatabase.GetAssetPath(item);
                FileName file = new FileName(path);

                if (UseOnly.Count > 0)
                {
                    if (!UseOnlyNames.Contains(file.NameWithoutExt))
                        continue;
                }

                Sprites.Add(new()
                {
                    FileName = file,
                    Sprite = (Sprite)item
                });
            }
        }

        public void Refresh()
        {
            index = 0;
            UsedSpriteIndex = 0;
        }

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
