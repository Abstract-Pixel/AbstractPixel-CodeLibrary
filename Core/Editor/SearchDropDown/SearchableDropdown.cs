using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AbstractPixel.Core.Editor
{
    public class SearchableDropdown<T> : AdvancedDropdown
    {
        private class DropdownItem<TItem> : AdvancedDropdownItem
        {
            public TItem Value { get; }

            public DropdownItem(string name, TItem value) : base(name)
            {
                Value = value;
            }
        }

        private readonly IEnumerable<T> _items;
        private readonly Func<T, string> _nameSelector;
        private readonly Func<T, string> _pathSelector;
        private readonly Action<T> _onItemSelected;
        private readonly string _title;

        public SearchableDropdown(
            IEnumerable<T> items, 
            Func<T, string> nameSelector, 
            Func<T, string> pathSelector, 
            Action<T> onItemSelected, 
            AdvancedDropdownState state = null,
            string title = "Select Item") 
            : base(state ?? new AdvancedDropdownState())
        {
            _items = items;
            _nameSelector = nameSelector;
            _pathSelector = pathSelector;
            _onItemSelected = onItemSelected;
            _title = title;
            
            // Replicates the native search window size
            minimumSize = new Vector2(250, 300);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(_title);

            foreach (var item in _items)
            {
                string name = _nameSelector(item);
                string path = _pathSelector != null ? _pathSelector(item) : "";
                
                AdvancedDropdownItem parent = root;

                // Build the folder hierarchy based on the path
                if (!string.IsNullOrEmpty(path))
                {
                    string[] folders = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var folder in folders)
                    {
                        var found = parent.children.FirstOrDefault(c => c.name == folder);
                        if (found == null)
                        {
                            found = new AdvancedDropdownItem(folder)
                            {
                                id = folder.GetHashCode()
                            };
                            parent.AddChild(found);
                        }
                        parent = found;
                    }
                }

                // Add the item to its designated folder
                var dropdownItem = new DropdownItem<T>(name, item)
                {
                    id = item.GetHashCode() // Keeps Unity happy by ensuring distinct internal IDs
                };
                
                parent.AddChild(dropdownItem);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is DropdownItem<T> typedItem)
            {
                _onItemSelected?.Invoke(typedItem.Value);
            }
        }
    }
}