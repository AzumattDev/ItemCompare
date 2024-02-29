/*using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace ItemCompare.Patches;

[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
static class InventoryGuiAwakePatch
{
    static void Postfix(InventoryGui __instance)
    {
        GameObject myPrefab = ItemComparePlugin.LoadPrefabByName("inventorytooltip");
        if (myPrefab != null)
        {
            CustomUITooltip.m_tooltipPrefab = Object.Instantiate(myPrefab);
            ItemComparePlugin.ItemCompareLogger.LogDebug($"Custom tooltip prefab: {CustomUITooltip.m_tooltipPrefab}");
        }
    }
}

[HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.CreateItemTooltip))]
static class InventoryGridCreateItemTooltipPatch
{
    [HarmonyPriority(Priority.Last)]
    static void Postfix(ItemDrop.ItemData item, UITooltip tooltip, InventoryGrid __instance)
    {
        UpdateCustomTooltipWithComparison(item, tooltip);
        EnsureCustomTooltipInstance(tooltip);
    }

    internal static void EnsureCustomTooltipInstance(UITooltip HoveredTooltip)
    {
        if (HoveredTooltip == null || CustomUITooltip.m_current == null) return;
        RectTransform originalRT = HoveredTooltip.GetComponent<RectTransform>();
        RectTransform customRT = CustomUITooltip.m_current.GetComponent<RectTransform>();
        // Adjust custom tooltip position here, this is a simple example, adjust as necessary
        customRT.anchoredPosition = new Vector2(originalRT.anchoredPosition.x + originalRT.rect.width, originalRT.anchoredPosition.y);
    }


    private static void UpdateCustomTooltipWithComparison(ItemDrop.ItemData hoveredItem, UITooltip tooltip)
    {
        Player player = Player.m_localPlayer;
        ItemDrop.ItemData? equippedItem = FindEquippedItemMatching(hoveredItem);

        //if (equippedItem != null && ItemComparePlugin.HoverKeybind.Value.IsKeyHeld())
        if (equippedItem != null)
        {
            StringBuilder comparisonHeaderText = new();
            StringBuilder comparisonText = new();
            comparisonHeaderText.AppendLine("<color=yellow>Comparison:</color>");
            // Start comparison text with the item name for clarity
            comparisonHeaderText.AppendLine($"<color=#00afd4>{hoveredItem.m_shared.m_name} vs. {equippedItem.m_shared.m_name}</color>");


            // Durability comparison
            AddDurabilityComparison(hoveredItem, equippedItem, comparisonText);

            // Weight comparison
            AddWeightComparison(hoveredItem, equippedItem, comparisonText);
            AddDamageComparison(hoveredItem, equippedItem, comparisonText, player);
            AddArmorComparison(hoveredItem, equippedItem, comparisonText);
            AddValueComparison(hoveredItem, equippedItem, comparisonText);
            AddOtherStatComparison(hoveredItem, equippedItem, comparisonText);
            if (tooltip.gameObject.GetComponent<CustomUITooltip>() is { } customUITooltip)
            {
                //ItemComparePlugin.ItemCompareLogger.LogDebug($"Updating existing custom tooltip");
                // Set the content of the custom tooltip
                customUITooltip.Set(comparisonText.ToString(), comparisonText.ToString(), tooltip.m_anchor);
            }
            else
            {
                //ItemComparePlugin.ItemCompareLogger.LogDebug($"Creating new custom tooltip");
                tooltip.gameObject.AddComponent<CustomUITooltip>();
                customUITooltip = tooltip.gameObject.GetComponent<CustomUITooltip>();
                customUITooltip.Set(comparisonHeaderText.ToString(), comparisonText.ToString(), tooltip.m_anchor);
            }
        }
    }

    private static ItemDrop.ItemData? FindEquippedItemMatching(ItemDrop.ItemData hoveredItem)
    {
        Player player = Player.m_localPlayer;
        // Your existing logic to find the matching equipped item, simplified for demonstration
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

/*using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace ItemCompare.Patches;

[HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.CreateItemTooltip))]
static class InventoryGridCreateItemTooltipPatch
{
    internal static UITooltip HoveredTooltip = null!;
    internal static ItemDrop.ItemData HoveredItem = null!;
    internal static InventoryGrid Grid = null!;

    [HarmonyPriority(Priority.Last)]
    static void Postfix(ItemDrop.ItemData item, UITooltip tooltip, InventoryGrid __instance)
    {
        HoveredTooltip = tooltip;
        HoveredItem = item;
        Grid = __instance;

        CreateAndShowComparisonTooltip(HoveredItem, HoveredTooltip, Grid);
    }

    internal static UITooltip? CreateAndShowComparisonTooltip(ItemDrop.ItemData hoveredItem, UITooltip originalTooltip, InventoryGrid grid)
    {
        Player player = Player.m_localPlayer;
        ItemDrop.ItemData? equippedItem = null;

        // Example: Determine the equipped item based on the hovered item's type
        // This logic needs to be adjusted based on actual slot matching logic
        switch (hoveredItem.m_shared.m_itemType)
        {
            case ItemDrop.ItemData.ItemType.OneHandedWeapon or ItemDrop.ItemData.ItemType.TwoHandedWeapon or ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                equippedItem = player.GetInventory().GetEquippedItems().FirstOrDefault(x => x.m_shared.m_itemType is ItemDrop.ItemData.ItemType.OneHandedWeapon or ItemDrop.ItemData.ItemType.TwoHandedWeapon or ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft);
                break;
            default:
                equippedItem = player.GetInventory().GetEquippedItems().FirstOrDefault(i => i.m_shared.m_itemType == hoveredItem.m_shared.m_itemType);
                break;
        }

        if (equippedItem != null)
        {
            ItemComparePlugin.ItemCompareLogger.LogDebug($"Hovered item: {hoveredItem.m_shared.m_name}, Equipped item: {equippedItem.m_shared.m_name}");
            StringBuilder comparisonText = new StringBuilder();
            comparisonText.AppendLine("<color=yellow>Comparison:</color>");
            // Start comparison text with the item name for clarity
            comparisonText.AppendLine($"<color=#00afd4>{hoveredItem.m_shared.m_name} vs. {equippedItem.m_shared.m_name}</color>");


            // Durability comparison
            AddDurabilityComparison(hoveredItem, equippedItem, comparisonText);

            // Weight comparison
            AddWeightComparison(hoveredItem, equippedItem, comparisonText);

            // Add more comparisons here (Damage, Armor, etc.)
            // For example:
            AddDamageComparison(hoveredItem, equippedItem, comparisonText, player);
            AddArmorComparison(hoveredItem, equippedItem, comparisonText);
            AddValueComparison(hoveredItem, equippedItem, comparisonText);


            // Add other stat comparisons here

            string comparisonTooltipText = comparisonText.ToString();

            UITooltip comparisonTooltip = null!;
            // Instantiate a new UITooltip for the comparison
            if (UITooltipOnHoverStartPatch.createdTooltip == null)
            {
                comparisonTooltip = Object.Instantiate(originalTooltip, originalTooltip.transform.parent);
                comparisonTooltip.Set("Comparison", comparisonTooltipText);
                ItemComparePlugin.ItemCompareLogger.LogDebug($"Comparison tooltip text: {comparisonTooltipText}");
                ItemComparePlugin.ItemCompareLogger.LogDebug($"Comparison tooltip: {comparisonTooltip} {originalTooltip.transform.parent.name}");

                // Adjust position to be next to the original tooltip
                // // This is an example; you'll need to adjust based on the actual UI layout
                RectTransform originalTooltipRT = originalTooltip.GetComponent<RectTransform>();
                Vector2 newPosition = originalTooltipRT.anchoredPosition;
                newPosition.x += originalTooltipRT.rect.width;
                comparisonTooltip.GetComponent<RectTransform>().anchoredPosition = newPosition;
            }
            else
            {
                comparisonTooltip = UITooltipOnHoverStartPatch.createdTooltip;
            }

            return comparisonTooltip;
        }

        return null;
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
}

[HarmonyPatch(typeof(UITooltip), nameof(UITooltip.OnHoverStart))]
static class UITooltipOnHoverStartPatch
{
    internal static UITooltip? createdTooltip = null!;

    static void Postfix(UITooltip __instance)
    {
        if (InventoryGridCreateItemTooltipPatch.HoveredTooltip != null && InventoryGridCreateItemTooltipPatch.Grid != null)
        {
            ItemComparePlugin.ItemCompareLogger.LogDebug($"Creating comparison tooltip {__instance == InventoryGridCreateItemTooltipPatch.HoveredTooltip} {__instance.m_text} {InventoryGridCreateItemTooltipPatch.HoveredTooltip.m_text}");
            if (createdTooltip == null)
            {
                createdTooltip = InventoryGridCreateItemTooltipPatch.CreateAndShowComparisonTooltip(InventoryGridCreateItemTooltipPatch.HoveredItem, InventoryGridCreateItemTooltipPatch.HoveredTooltip, InventoryGridCreateItemTooltipPatch.Grid);
            }
        }
        else
        {
            ItemComparePlugin.ItemCompareLogger.LogDebug("No tooltip to compare");
        }
    }
}

[HarmonyPatch(typeof(UITooltip), nameof(UITooltip.OnPointerExit))]
static class UITooltipOnPointerExitPatch
{
    static void Prefix(UITooltip __instance)
    {
        if (__instance == InventoryGridCreateItemTooltipPatch.HoveredTooltip)
        {
            // Destroy the comparison tooltip if it was created
            if (UITooltipOnHoverStartPatch.createdTooltip != null)
            {
                if (!(bool)(Object)UITooltip.m_tooltip)
                    return;
                Object.Destroy(UITooltipOnHoverStartPatch.createdTooltip.gameObject);
                UITooltip.m_current = (UITooltip)null;
                UITooltip.m_tooltip = (GameObject)null;
                UITooltip.m_hovered = (GameObject)null;
                UITooltipOnHoverStartPatch.createdTooltip = null!;
            }
            else
            {
                ItemComparePlugin.ItemCompareLogger.LogDebug("No comparison tooltip to destroy");
            }

            InventoryGridCreateItemTooltipPatch.HoveredTooltip = null!;
            InventoryGridCreateItemTooltipPatch.HoveredItem = null!;
            InventoryGridCreateItemTooltipPatch.Grid = null!;
        }
    }
}#1#

[HarmonyPatch(typeof(UITooltip), nameof(UITooltip.OnPointerExit))]
static class UITooltipOnPointerExitPatch
{
    static void Prefix(UITooltip __instance)
    {
        if (CustomUITooltip.m_current != null)
        {
            Object.Destroy(CustomUITooltip.m_current.gameObject);
            CustomUITooltip.m_current = null;
            CustomUITooltip.m_hovered = null;
        }
    }
}*/