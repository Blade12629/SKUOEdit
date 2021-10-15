using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Source.StaticsBuilder
{
    public class OnClickEvent : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            ((IPointerClickHandler)GetComponentInParent<RowPanel>()).OnPointerClick(eventData);
        }
    }
}
