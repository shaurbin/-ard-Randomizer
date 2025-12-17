using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class CardComponent : MonoBehaviour
    {
        public string Path;
        [HideInInspector] public SpriteRenderer SpriteRenderer;
        [SerializeField] private List<Sprite> Sprites;
        private bool prepared;
        
        private int index;
        private bool finished;

        public bool Finished => finished;

        public bool Prepared => prepared;

        public int Index => index;

        public int Size => Sprites.Count;

        public bool Worked;

        public int UsedSpriteIndex;

        public UniTask<bool> Prepare()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            var items = Resources.LoadAll(Path, typeof(Sprite));
            Sprites = new List<Sprite>();
            
            foreach (var item in items)
                Sprites.Add((Sprite)item);

            prepared = true;

            return UniTask.FromResult(true);
        }

        

        public bool NextSprite()
        {
            if (finished) return false;
            if (index > 0) UsedSpriteIndex++;
            //SpriteRenderer.sprite = Sprites[UsedSpriteIndex];
            index++;
            if (index >= Sprites.Count)
                finished = true;

            return true;
        }

        public void Build()
        {
            SpriteRenderer.sprite = Sprites[UsedSpriteIndex];
        }
    }
}
