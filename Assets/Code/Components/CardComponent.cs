using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private List<SpriteWrapper> Sprites;
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

                if (file.Path.Equals($"Assets/Resources/{Path}"))
                {
                    Sprites.Add(new()
                    {
                        FileName = file,
                        Sprite = (Sprite)item
                    });
                }
                else
                {
                    string innerFolderName = file.Directories[^1];
                    
                    //if (!sp.ContainsKey(innerFolderName))
                    //{
                    //    sp.Add(innerFolderName, new List<SpriteWrapper>());
                    //}

                    //sp[innerFolderName].Add(new()
                    //{
                    //    FileName = file,
                    //    Sprite = (Sprite)item
                    //});

                    if (dependents.Count > 0)
                    {
                        var d = dependents.First(v => v.Key.Equals(file.Postfix));
                        
                        if (!d.Sprites.ContainsKey(file.NameOfPostfix))
                            d.Sprites.Add(file.NameOfPostfix, new List<SpriteWrapper>());
                        d.Sprites[file.NameOfPostfix].Add(new()
                        {
                            FileName = file,
                            Sprite = (Sprite)item
                        });
                        
                    }
                }
            }

            prepared = true;

            return UniTask.FromResult(true);
        }

        public string DependentsIndexes()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var dependent in dependents)
            {
                sb.Append(dependent.UsedIndex);
            }
            return sb.ToString();
        }

        public bool ShowDependencies()
        {
            UsedSpriteIndex = 0;
            if (dependents.Count > 0)
            {
                foreach (var dependent in dependents)
                {
                    if (dependent.Index > 0)
                        dependent.UsedIndex++;

                    dependent.Index++;
                    if (dependent.Index > dependent.Sprites[Sprites[UsedSpriteIndex].FileName.NameWithoutExt].Count)
                    {
                        dependent.UsedIndex = 0;
                        dependent.Index = 0;
                        Build();
                        return true;
                    }
                    Build();
                    return true;
                }
            }
            Build();
            return true;
        }

        public bool NextSprite()
        {
            if (finished) return false;

            if (dependents.Count > 0)
            {
                bool nextSprite = false;
                foreach (var dependent in dependents)
                {
                    if (dependent.Index > 0)
                        dependent.UsedIndex++;
                    
                    dependent.Index++;
                    if (dependent.Index > dependent.Sprites[Sprites[UsedSpriteIndex].FileName.NameWithoutExt].Count)
                    {
                        nextSprite = true;
                    }
                }

                if (nextSprite)
                {
                    UsedSpriteIndex++;
                    index++;
                    foreach (var dependent in dependents)
                    {
                        dependent.UsedIndex = 0;
                        dependent.Index = 0;
                    }
                    if (index >= Sprites.Count)
                    {
                        finished = true;
                        UsedSpriteIndex--;
                        return false;
                    }
                }
            }
            else
            {
                if (index > 0) UsedSpriteIndex++;

                index++;
                if (index >= Sprites.Count)
                    finished = true;
            }

            return true;
        }

        public void Build()
        {
            SpriteRenderer.sprite = Sprites[UsedSpriteIndex].Sprite;
            if (dependents.Count > 0)
            {
                foreach (var dependent in dependents)
                {
                    dependent.SpriteRenderer.sprite = dependent.Sprites[Sprites[UsedSpriteIndex].FileName.NameWithoutExt][dependent.UsedIndex].Sprite;
                }
            }
        }

        [Serializable]
        public class DependentElements
        {
            [HideInInspector] public int Index;
            [HideInInspector] public int UsedIndex;
            public SpriteRenderer SpriteRenderer;
            public string Key;
            [HideInInspector] public Dictionary<string, List<SpriteWrapper>> Sprites = new Dictionary<string, List<SpriteWrapper>>();
        }
    }
}
