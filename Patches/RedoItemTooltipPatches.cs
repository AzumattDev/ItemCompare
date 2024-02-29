using System.Linq;
using System.Text;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace ItemCompare.Patches;
// Not the best approach, but since my other attempts have failed, I say fuck it. Try anyways.
[HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.CreateItemTooltip))]
static class InventoryGridCreateItemTooltipPatch
{
    public static GameObject clonedTooltip = null;

    [HarmonyPriority(Priority.Last)]
    [HarmonyAfter("org.bepinex.plugins.jewelcrafting")]
    public static void Postfix(ItemDrop.ItemData item, UITooltip tooltip, InventoryGrid __instance)
    {
        // Ensure the cloned tooltip is destroyed if it already exists (to handle dynamic updates)
        if (clonedTooltip != null)
        {
            ItemComparePlugin.ItemCompareLogger.LogInfo("InventoryGrid.CreateItemTooltip: Cloned tooltip exists, destroying it");
            Object.Destroy(clonedTooltip);
            clonedTooltip = null;
        }

        ItemComparePlugin.ItemCompareLogger.LogInfo($"InventoryGrid.CreateItemTooltip: {item.m_shared.m_name}");
        // Clone the tooltip prefab
        GameObject originalPrefab = tooltip.m_tooltipPrefab;
        clonedTooltip = Object.Instantiate(originalPrefab, originalPrefab.transform.parent);

        // Adjust position to display to the right of the original tooltip
        RectTransform originalRT = tooltip.GetComponent<RectTransform>();
        RectTransform clonedRT = clonedTooltip.GetComponent<RectTransform>();
        
        Vector3 offset = new Vector3(originalRT.rect.width, 0, 0);
        clonedRT.anchoredPosition = originalRT.position + offset;

        // Update the cloned tooltip's text with comparison data
        UpdateClonedTooltipText(clonedTooltip, item);
    }

    private static void UpdateClonedTooltipText(GameObject clonedTooltip, ItemDrop.ItemData hoveredItem)
    {
        ItemComparePlugin.ItemCompareLogger.LogInfo($"UpdateClonedTooltipText: {hoveredItem.m_shared.m_name}");
        // Generate the comparison text and topic
        string comparisonText = GenerateComparisonText(hoveredItem);
        string comparisonTopic = "Comparison"; // Example topic, change as needed

        // Find and update the "Text" component with comparison data
        Transform textTransform = Utils.FindChild(clonedTooltip.transform, "Text");
        if (textTransform != null)
        {
            TMP_Text textComponent = textTransform.GetComponent<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = comparisonText; // Set or append comparison text
            }
        }

        // Find and update the "Topic" component with the new topic
        Transform topicTransform = Utils.FindChild(clonedTooltip.transform, "Topic");
        if (topicTransform != null)
        {
            TMP_Text topicComponent = topicTransform.GetComponent<TMP_Text>();
            if (topicComponent != null)
            {
                topicComponent.text = comparisonTopic; // Set the new topic
            }
        }
    }


    private static string GenerateComparisonText(ItemDrop.ItemData hoveredItem)
    {
        StringBuilder comparisonText = new();
        Player player = Player.m_localPlayer;
        ItemDrop.ItemData? equippedItem = FindEquippedItemMatching(hoveredItem);

        //if (equippedItem != null && ItemComparePlugin.HoverKeybind.Value.IsKeyHeld())
        if (equippedItem != null)
        {
            // Durability comparison
            AddDurabilityComparison(hoveredItem, equippedItem, comparisonText);

            // Weight comparison
            AddWeightComparison(hoveredItem, equippedItem, comparisonText);
            AddDamageComparison(hoveredItem, equippedItem, comparisonText, player);
            AddArmorComparison(hoveredItem, equippedItem, comparisonText);
            AddValueComparison(hoveredItem, equippedItem, comparisonText);
            AddOtherStatComparison(hoveredItem, equippedItem, comparisonText);
        }

        return comparisonText.ToString();
    }

    private static ItemDrop.ItemData? FindEquippedItemMatching(ItemDrop.ItemData hoveredItem)
    {
        Player player = Player.m_localPlayer;
        return player.GetInventory().GetEquippedItems().FirstOrDefault(i => i.m_shared.m_itemType == hoveredItem.m_shared.m_itemType);
    }

    private static void AddDurabilityComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredDurability = hoveredItem.GetMaxDurability();
        float equippedDurability = equippedItem.GetMaxDurability();
        float difference = hoveredDurability - equippedDurability;

        comparisonText.AppendLine($"Durability: {hoveredDurability} vs. {equippedDurability} ({(difference >= 0 ? "+" : "")}{difference})");
    }

    private static void AddDamageComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText, Player player)
    {
        // Simplified; actual damage may depend on skills, item quality, etc.
        var hoveredDamage = hoveredItem.GetDamage();
        var equippedDamage = equippedItem.GetDamage();

        comparisonText.AppendLine($"Damage: {hoveredDamage} vs. {equippedDamage}");
    }

    private static void AddArmorComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredArmor = hoveredItem.GetArmor();
        float equippedArmor = equippedItem.GetArmor();

        comparisonText.AppendLine($"Armor: {hoveredArmor} vs. {equippedArmor}");
    }

    private static void AddWeightComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredWeight = hoveredItem.GetWeight();
        float equippedWeight = equippedItem.GetWeight();
        float difference = hoveredWeight - equippedWeight;

        comparisonText.AppendLine($"Weight: {hoveredWeight} vs. {equippedWeight} ({(difference >= 0 ? "+" : "")}{difference})");
    }

    private static void AddValueComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        int hoveredValue = hoveredItem.GetValue();
        int equippedValue = equippedItem.GetValue();

        comparisonText.AppendLine($"Value: {hoveredValue} vs. {equippedValue}");
    }

    private static void AddOtherStatComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredBp = hoveredItem.GetBlockPower(hoveredItem.m_quality, Player.m_localPlayer.GetSkillFactor(Skills.SkillType.Blocking));
        float equippedBp = equippedItem.GetBlockPower(hoveredItem.m_quality, Player.m_localPlayer.GetSkillFactor(Skills.SkillType.Blocking));

        float hoveredStamDrain = hoveredItem.GetDrawStaminaDrain();
        float equippedStamDrain = equippedItem.GetDrawStaminaDrain();

        comparisonText.AppendLine($"Block Power: {hoveredBp} vs. {equippedBp}");
        comparisonText.AppendLine($"$item_staminahold: {hoveredStamDrain} vs. {equippedStamDrain}");
    }
}

[HarmonyPatch(typeof(UITooltip), nameof(UITooltip.OnHoverStart))]
static class UITooltipOnHoverStartPatch
{
    public static void Postfix()
    {
        // Check if the cloned tooltip exists and make it active
        if (InventoryGridCreateItemTooltipPatch.clonedTooltip != null)
        {
            ItemComparePlugin.ItemCompareLogger.LogInfo("UITooltip.OnHoverStart: Cloned tooltip exists, making it active");
            InventoryGridCreateItemTooltipPatch.clonedTooltip.SetActive(true);
        }
    }
}

[HarmonyPatch(typeof(UITooltip), nameof(UITooltip.OnPointerExit))]
public static class UITooltipOnPointerExitPatch
{
    public static void Prefix()
    {
        // Destroy the cloned tooltip when the pointer exits the original tooltip
        if (InventoryGridCreateItemTooltipPatch.clonedTooltip != null)
        {
            ItemComparePlugin.ItemCompareLogger.LogInfo("UITooltip.OnPointerExit: Cloned tooltip exists, destroying it");
            Object.Destroy(InventoryGridCreateItemTooltipPatch.clonedTooltip);
            InventoryGridCreateItemTooltipPatch.clonedTooltip = null;
        }
    }
}