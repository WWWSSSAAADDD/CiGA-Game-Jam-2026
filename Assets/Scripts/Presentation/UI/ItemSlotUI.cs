using System;
using System.Collections;
using System.Collections.Generic;
using Game.Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.UI
{
    public class ItemSlotUI : MonoBehaviour
    {
        [SerializeField] private ItemData item;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text itemNameText;
        
        // Start is called before the first frame update
        void Start()
        {
            if (Inventory.Instance != null)
            {
                Inventory.Instance.OnItemChanged += HandleOnItemChanged;
                itemNameText.text = item != null ? item.DisplayName : "No Item";
                Refresh();
            }
            
        }

        private void OnDestroy()
        {
            if (Inventory.Instance != null)
                Inventory.Instance.OnItemChanged -= HandleOnItemChanged;
        }

        private void HandleOnItemChanged(ItemData changedItem, int newAmount)
        {
            if (changedItem == item)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            if (countText == null || iconImage == null || Inventory.Instance == null) return;
            int count = Inventory.Instance.GetItemCount(item);
            countText.text = count.ToString();
            iconImage.enabled = count > 0;
        }
    }
}
