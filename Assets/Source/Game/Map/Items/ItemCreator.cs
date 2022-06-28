using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.Source.Game.Map.Items
{
    public class ItemCreator : MonoBehaviour
    {
        [SerializeField] Material _itemMaterial;
        [SerializeField] bool _spawnNewItem;

        [SerializeField] Item _selectedItem;
        [SerializeField] uint _selectedItemId;

        void Start()
        {
        }

        void Update()
        {
            if (_selectedItem != null && _selectedItemId != _selectedItem.ItemId)
                _selectedItem.ItemId = _selectedItemId;

            if (!_spawnNewItem)
                return;

            _spawnNewItem = false;
            _selectedItemId = 0;

            GameObject obj = new GameObject();
            Item item = obj.AddComponent<Item>();
            item.Initialize(_itemMaterial);

            _selectedItem = item;
        }
    }
}
