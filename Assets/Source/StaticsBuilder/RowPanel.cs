using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Assets.Source.StaticsBuilder
{
    public class RowPanel : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] InputField _xInput;
        [SerializeField] InputField _yInput;
        [SerializeField] InputField _zInput;

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public event Action<float> OnXUpdated;
        public event Action<float> OnYUpdated;
        public event Action<float> OnZUpdated;

        int _row;
        StaticBuilder _staticBuilder;

        public void SetRow(int row, StaticBuilder builder)
        {
            _row = row;
            _staticBuilder = builder;
        }

        public void UpdateFields()
        {
            _xInput.SetTextWithoutNotify(X.ToString());
            _yInput.SetTextWithoutNotify(Y.ToString());
            _zInput.SetTextWithoutNotify(Z.ToString());
        }

        public void SetFields(Vector3 v)
        {
            SetFields(v.x, v.y, v.z);
        }

        public void SetFields(float? x = null, float? y = null, float? z = null)
        {
            if (x.HasValue)
                X = x.Value;
            
            if (y.HasValue)
                Y = y.Value;

            if (z.HasValue)
                Z = z.Value;

            UpdateFields();

            ((IPointerClickHandler)this).OnPointerClick(null);
        }

        public void OnXUpdate()
        {
            if (!float.TryParse(_xInput.text, out float x))
                return;

            X = x;
            OnXUpdated?.Invoke(x);
        }

        public void OnYUpdate()
        {
            if (!float.TryParse(_yInput.text, out float y))
                return;

            Y = y;
            OnYUpdated?.Invoke(y);
        }

        public void OnZUpdate()
        {
            if (!float.TryParse(_zInput.text, out float z))
                return;

            Z = z;
            OnZUpdated?.Invoke(z);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            _staticBuilder.OnRowClick(_row);
        }
    }
}
