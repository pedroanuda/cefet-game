using Game.Items;
using Godot;
using System.Collections.Generic;
using System;

namespace Game
{
    public partial class Inventory : RefCounted
    {
        [Signal]
        public delegate void UpdatedEventHandler();

        //private readonly List<Dictionary<string, object>> _content = new();
        private readonly List<Item> _content = new();
        public int Capacity { get; set; } = 8;

        private void Update() => EmitSignal(SignalName.Updated);

        public Inventory(int? capacity = 8)
        {
            if (capacity.HasValue) Capacity = capacity.Value;
        }

        public void Add(Item item, int quantity = 1, Action onSucess = null)
        {
            // var nowSlot = _content.Find(slot => (int) slot["slotId"] == item.Id);
            // 
            // if (nowSlot is not null)
            // {
            // }
            if (_content.Count >= Capacity) return;

            _content.Add(item);
            onSucess?.Invoke();
            Update();
        }

        public void Remove(Item item, int quantity = 1)
        {
            _content.Remove(item);
            Update();
        }

        public Item[] GetItems()
        {
            return _content.ToArray();
        }

        public bool Exists(Item item)
        {
            return _content.Contains(item);
        }
        
        public bool Exists(int itemId)
        {
            return _content.Exists(item => itemId.Equals(item.Id));
        }
        
        public void Clear()
        {
            _content.Clear();
            Update();
        }
    }
}
