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
        if (clonedTooltip != null)
        {
            Object.Destroy(clonedTooltip);
            clonedTooltip = null;
        }

        GameObject originalPrefab = tooltip.m_tooltipPrefab;
        clonedTooltip = Object.Instantiate(originalPrefab, tooltip.transform.parent);


        RectTransform originalRT = tooltip.GetComponent<RectTransform>();
        RectTransform clonedRT = clonedTooltip.GetComponent<RectTransform>();
        // After cloning the tooltip and setting it up
        TooltipFollower follower = clonedTooltip.AddComponent<TooltipFollower>();
        follower.target = originalRT; // Assuming originalRT is the RectTransform of the original tooltip
        follower.offset = new Vector2(originalRT.rect.width + 50, 0); // Customize this offset as needed

        /*Vector2 offset = new Vector2(originalRT.rect.width + 50, 0);
        clonedRT.anchoredPosition = originalRT.anchoredPosition + offset;

        // The cloned tooltip is always behind the original and isn't to the right of the original. This is likely due to the fact the original can move around
        // Fix it
        clonedRT.position = originalRT.position + new Vector3(offset.x, -offset.y, 0);*/


        clonedRT.sizeDelta = originalRT.sizeDelta;
        clonedRT.anchorMin = originalRT.anchorMin;
        clonedRT.anchorMax = originalRT.anchorMax;

        Utils.ClampUIToScreen(clonedRT);

        UpdateClonedTooltipText(clonedTooltip, item);
    }

    private static void UpdateClonedTooltipText(GameObject clonedTooltip, ItemDrop.ItemData hoveredItem)
    {
        string comparisonText = GenerateComparisonText(hoveredItem);
        string comparisonTopic = Localization.instance.Localize(FindEquippedItemMatching(hoveredItem)?.m_shared.m_name) + " (Equipped)";

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
            AddDurabilityComparison(hoveredItem, equippedItem, comparisonText);
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

        comparisonText.AppendLine($"Durability: {hoveredDurability} vs. {equippedDurability} ({(difference >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{difference}</color>)");
    }

    private static void AddDamageComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText, Player player)
    {
        // Simplified; actual damage may depend on skills, item quality, etc.
        HitData.DamageTypes hoveredDamage = hoveredItem.GetDamage();
        HitData.DamageTypes equippedDamage = equippedItem.GetDamage();

        // Calculate differences
        float differenceTrue = hoveredDamage.m_damage - equippedDamage.m_damage;
        float differenceBlunt = hoveredDamage.m_blunt - equippedDamage.m_blunt;
        float differenceSlash = hoveredDamage.m_slash - equippedDamage.m_slash;
        float differencePierce = hoveredDamage.m_pierce - equippedDamage.m_pierce;
        float differenceChop = hoveredDamage.m_chop - equippedDamage.m_chop;
        float differencePickaxe = hoveredDamage.m_pickaxe - equippedDamage.m_pickaxe;
        float differenceFire = hoveredDamage.m_fire - equippedDamage.m_fire;
        float differenceFrost = hoveredDamage.m_frost - equippedDamage.m_frost;
        float differenceLightning = hoveredDamage.m_lightning - equippedDamage.m_lightning;
        float differencePoison = hoveredDamage.m_poison - equippedDamage.m_poison;
        float differenceSpirit = hoveredDamage.m_spirit - equippedDamage.m_spirit;
        float differenceTotal = hoveredDamage.GetTotalDamage() - equippedDamage.GetTotalDamage();

        comparisonText.AppendLine($"True Damage: {hoveredDamage.m_damage} vs. {equippedDamage.m_damage} ({(differenceTrue >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceTrue}</color>)");
        comparisonText.AppendLine($"Blunt Damage: {hoveredDamage.m_blunt} vs. {equippedDamage.m_blunt} ({(differenceBlunt >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceBlunt}</color>)");
        comparisonText.AppendLine($"Slash Damage: {hoveredDamage.m_slash} vs. {equippedDamage.m_slash} ({(differenceSlash >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceSlash}</color>)");
        comparisonText.AppendLine($"Pierce Damage: {hoveredDamage.m_pierce} vs. {equippedDamage.m_pierce} ({(differencePierce >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differencePierce}</color>)");
        comparisonText.AppendLine($"Chop Damage: {hoveredDamage.m_chop} vs. {equippedDamage.m_chop} ({(differenceChop >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceChop}</color>)");
        comparisonText.AppendLine($"Pickaxe Damage: {hoveredDamage.m_pickaxe} vs. {equippedDamage.m_pickaxe} ({(differencePickaxe >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differencePickaxe}</color>)");
        comparisonText.AppendLine($"Fire Damage: {hoveredDamage.m_fire} vs. {equippedDamage.m_fire} ({(differenceFire >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceFire}</color>)");
        comparisonText.AppendLine($"Frost Damage: {hoveredDamage.m_frost} vs. {equippedDamage.m_frost} ({(differenceFrost >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceFrost}</color>)");
        comparisonText.AppendLine($"Lightning Damage: {hoveredDamage.m_lightning} vs. {equippedDamage.m_lightning} ({(differenceLightning >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceLightning}</color>)");
        comparisonText.AppendLine($"Poison Damage: {hoveredDamage.m_poison} vs. {equippedDamage.m_poison} ({(differencePoison >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differencePoison}</color>)");
        comparisonText.AppendLine($"Spirit Damage: {hoveredDamage.m_spirit} vs. {equippedDamage.m_spirit} ({(differenceSpirit >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceSpirit}</color>)");
        comparisonText.AppendLine($"All Damage: {hoveredDamage.GetTotalDamage()} vs. {equippedDamage.GetTotalDamage()} ({(differenceTotal >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceTotal}</color>)");
    }

    private static void AddArmorComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredArmor = hoveredItem.GetArmor();
        float equippedArmor = equippedItem.GetArmor();
        float difference = hoveredArmor - equippedArmor;

        comparisonText.AppendLine($"Armor: {hoveredArmor} vs. {equippedArmor} ({(difference >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{difference}</color>)");
    }

    private static void AddWeightComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredWeight = hoveredItem.GetWeight();
        float equippedWeight = equippedItem.GetWeight();
        float difference = hoveredWeight - equippedWeight;

        comparisonText.AppendLine($"Weight: {hoveredWeight} vs. {equippedWeight} ({(difference >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{difference}</color>)");
    }

    private static void AddValueComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        int hoveredValue = hoveredItem.GetValue();
        int equippedValue = equippedItem.GetValue();
        int difference = hoveredValue - equippedValue;

        comparisonText.AppendLine($"Value: {hoveredValue} vs. {equippedValue} ({(difference >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{difference}</color>)");
    }

    private static void AddOtherStatComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredBp = hoveredItem.GetBlockPower(hoveredItem.m_quality, Player.m_localPlayer.GetSkillFactor(Skills.SkillType.Blocking));
        float equippedBp = equippedItem.GetBlockPower(hoveredItem.m_quality, Player.m_localPlayer.GetSkillFactor(Skills.SkillType.Blocking));
        float difference = hoveredBp - equippedBp;

        float hoveredStamDrain = hoveredItem.GetDrawStaminaDrain();
        float equippedStamDrain = equippedItem.GetDrawStaminaDrain();
        float differenceStamDrain = hoveredStamDrain - equippedStamDrain;

        comparisonText.AppendLine($"Block Power: {hoveredBp} vs. {equippedBp} ({(difference >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{difference}</color>)");
        comparisonText.AppendLine($"$item_staminahold: {hoveredStamDrain} vs. {equippedStamDrain} ({(differenceStamDrain >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceStamDrain}</color>)");
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
            // ItemComparePlugin.ItemCompareLogger.LogInfo("UITooltip.OnHoverStart: Cloned tooltip exists, making it active");
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
            //ItemComparePlugin.ItemCompareLogger.LogInfo("UITooltip.OnPointerExit: Cloned tooltip exists, destroying it");
            Object.Destroy(InventoryGridCreateItemTooltipPatch.clonedTooltip);
            InventoryGridCreateItemTooltipPatch.clonedTooltip = null;
        }
    }
}