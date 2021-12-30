using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI
{
    public class LoadingBar : MonoBehaviour
    {
        public Color32 ForegroundBarColor
        {
            get => _foregroundBarColor;
            set
            {
                _foregroundBarColor = value;
                _refreshVisuals = true;
            }
        }
        public Color32 BackgroundBarColor
        {
            get => _backgroundBarColor;
            set
            {
                _backgroundBarColor = value;
                _refreshVisuals = true;
            }
        }
        public Color32 ForegroundTextColor
        {
            get => _barText.color;
            set => _barText.color = value;
        }
        public Color32 BackgroundColor
        {
            get => _backgroundImg.color;
            set => _backgroundImg.color = value;
        }

        public float Max
        {
            get => _max;
            set
            {
                _max = value;
                _refreshVisuals = true;
            }
        }
        public float Min
        {
            get => _min;
            set
            {
                _min = value;
                _refreshVisuals = true;
            }
        }
        public float Current
        {
            get => _current;
            set
            {
                if (value > Max)
                    value = Max;
                else if (value < Min)
                    value = Min;

                _current = value;
                _refreshVisuals = true;
            }
        }

        public string Text
        {
            get => _barText.text;
            set
            {
                if (value == null)
                    value = string.Empty;

                _barText.text = value;
            }
        }

        [SerializeField] Image _backgroundImg;
        [SerializeField] Text _barText;
        [SerializeField] RawImage _barImage;
        [SerializeField] RectTransform _barTransform;

        [SerializeField] float _min;
        [SerializeField] float _max;
        [SerializeField] float _current;
        [SerializeField] Color _foregroundBarColor = Color.cyan;
        [SerializeField] Color _backgroundBarColor = Color.red;

        [SerializeField] protected bool _refreshVisuals;

        public void Increment()
        {
            Current++;
        }

        public void Increment(string text)
        {
            Text = text;
            Increment();
        }

        public void Decrement()
        {
            Current--;
        }

        public void Decrement(string text)
        {
            Text = text;
            Decrement();
        }

        public void Setup(int min, int max, int current, string text)
        {
            Max = max;
            Min = min;
            Current = current;
            Text = text;
        }

        void Awake()
        {
            Max = 100;
            Current = 0;

            if (_max != 0 && _current > 0)
                _refreshVisuals = true;
            else
            {
                Texture2D tex = GetCurrentBarTexture();
                Color32[] colors = tex.GetPixels32();

                for (int i = 0; i < colors.Length; i++)
                    colors[i] = BackgroundBarColor;

                tex.SetPixels32(colors);
                tex.Apply();
            }
        }

        protected virtual void FixedUpdate()
        {
            if (!_refreshVisuals)
                return;

            _refreshVisuals = false;

            if (!InvalidateValues())
                return;

            Texture2D tex = GetCurrentBarTexture();
            Vector2 size = _barTransform.sizeDelta;

            if (tex.width != (int)size.x || tex.height != (int)size.y)
                tex.Resize((int)size.x, (int)size.y);

            // foreground width
            float perc = 100f / Max * Current;
            int widthForeground = (int)(size.x / 100f * perc);
            Color32[] colors = tex.GetPixels32();
            int index = 0;

            int swidth = (int)size.x;
            int sheight = (int)size.y;

            for (int y = 0; y < sheight; y++)
            {
                for (int x = 0; x < widthForeground; x++)
                {
                    colors[index++] = ForegroundBarColor;
                }

                for (int x = widthForeground; x < swidth; x++)
                {
                    colors[index++] = BackgroundBarColor;
                }
            }

            tex.SetPixels32(colors);
            tex.Apply();
        }

        protected virtual bool InvalidateValues()
        {
            if (Max <= Min)
                return false;

            if (Current > Max)
                Current = Max;
            else if (Current < Min)
                Current = Min;

            return true;
        }

        Texture2D GetCurrentBarTexture()
        {
            Texture2D tex = _barImage.texture as Texture2D;

            if (tex == null)
                _barImage.texture = tex = new Texture2D((int)_barTransform.sizeDelta.x, (int)_barTransform.sizeDelta.y);

            return tex;
        }
    }
}
