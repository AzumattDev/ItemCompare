using System.Linq;
using System.Text;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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
        // Check if there's an equipped item of the same type
        var equippedItem = FindEquippedItemMatching(item);
        if (equippedItem == null)
        {
            // No matching equipped item, do not create/update cloned tooltip
            return;
        }

        if (clonedTooltip != null)
        {
            Object.Destroy(clonedTooltip);
            clonedTooltip = null;
        }

        GameObject originalPrefab = tooltip.m_tooltipPrefab;
        clonedTooltip = Object.Instantiate(originalPrefab, tooltip.transform.GetComponentInParent<Canvas>().transform);


        RectTransform originalRT = tooltip.GetComponent<RectTransform>();
        RectTransform clonedRT = clonedTooltip.GetComponent<RectTransform>();
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
            AddUseComparison(hoveredItem, equippedItem, comparisonText);
            AddDamageComparison(hoveredItem, equippedItem, comparisonText, player);
            AddArmorComparison(hoveredItem, equippedItem, comparisonText);
            AddValueComparison(hoveredItem, equippedItem, comparisonText);
            AddOtherStatComparison(hoveredItem, equippedItem, comparisonText);
        }

        return Localization.instance.Localize(comparisonText.ToString());
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

        comparisonText.AppendLine($"$item_durability: ({(difference >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{difference}</color>)");
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

        comparisonText.AppendLine($"True Damage: ({(differenceTrue >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceTrue}</color>)");
        comparisonText.AppendLine($"Blunt Damage: ({(differenceBlunt >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceBlunt}</color>)");
        comparisonText.AppendLine($"Slash Damage: ({(differenceSlash >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceSlash}</color>)");
        comparisonText.AppendLine($"Pierce Damage: ({(differencePierce >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differencePierce}</color>)");
        comparisonText.AppendLine($"Chop Damage: ({(differenceChop >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceChop}</color>)");
        comparisonText.AppendLine($"Pickaxe Damage: ({(differencePickaxe >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differencePickaxe}</color>)");
        comparisonText.AppendLine($"Fire Damage: ({(differenceFire >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceFire}</color>)");
        comparisonText.AppendLine($"Frost Damage: ({(differenceFrost >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceFrost}</color>)");
        comparisonText.AppendLine($"Lightning Damage: ({(differenceLightning >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceLightning}</color>)");
        comparisonText.AppendLine($"Poison Damage: ({(differencePoison >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differencePoison}</color>)");
        comparisonText.AppendLine($"Spirit Damage: ({(differenceSpirit >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceSpirit}</color>)");
        comparisonText.AppendLine($"All Damage: ({(differenceTotal >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceTotal}</color>)");
    }

    private static void AddArmorComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredArmor = hoveredItem.GetArmor();
        float equippedArmor = equippedItem.GetArmor();
        float difference = hoveredArmor - equippedArmor;

        comparisonText.AppendLine($"$item_armor: ({(difference >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{difference}</color>)");
    }

    private static void AddWeightComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredWeight = hoveredItem.GetWeight();
        float equippedWeight = equippedItem.GetWeight();
        float difference = hoveredWeight - equippedWeight;

        comparisonText.AppendLine($"Weight: ({(difference >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{difference}</color>)");
    }

    private static void AddValueComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        int hoveredValue = hoveredItem.GetValue();
        int equippedValue = equippedItem.GetValue();
        int difference = hoveredValue - equippedValue;

        comparisonText.AppendLine($"$item_value: ({(difference >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{difference}</color>)");
    }

    private static void AddUseComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredAttackStamina = hoveredItem.m_shared.m_attack.m_attackStamina;
        float equippedAttackStamina = equippedItem.m_shared.m_attack.m_attackStamina;
        float differenceAttackStamina = hoveredAttackStamina - equippedAttackStamina;
        
        float hoveredAttackEitr = hoveredItem.m_shared.m_attack.m_attackEitr;
        float equippedAttackEitr = equippedItem.m_shared.m_attack.m_attackEitr;
        float differenceAttackEitr = hoveredAttackEitr - equippedAttackEitr;
        
        float hoveredHealthUse = hoveredItem.m_shared.m_attack.m_attackEitr;
        float equippedHealthUse = equippedItem.m_shared.m_attack.m_attackEitr;
        float differenceHealthUse = hoveredHealthUse - equippedHealthUse;
        
        comparisonText.AppendLine($"$item_attackstamina: ({(differenceAttackStamina >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceAttackStamina}</color>)");
        comparisonText.AppendLine($"$item_attackeitr: ({(differenceAttackEitr >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceAttackEitr}</color>)");
        comparisonText.AppendLine($"$item_healthuse: ({(differenceHealthUse >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceHealthUse}</color>)");
        
    }

    private static void AddOtherStatComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredBp = hoveredItem.GetBlockPower(hoveredItem.m_quality, Player.m_localPlayer.GetSkillFactor(Skills.SkillType.Blocking));
        float equippedBp = equippedItem.GetBlockPower(hoveredItem.m_quality, Player.m_localPlayer.GetSkillFactor(Skills.SkillType.Blocking));
        float difference = hoveredBp - equippedBp;
        
        float hoveredBlockForce = hoveredItem.GetDeflectionForce(hoveredItem.m_quality);
        float equippedBlockForce = equippedItem.GetDeflectionForce(equippedItem.m_quality);
        float differenceBlockForce = hoveredBlockForce - equippedBlockForce;

        float hoveredStamDrain = hoveredItem.GetDrawStaminaDrain();
        float equippedStamDrain = equippedItem.GetDrawStaminaDrain();
        float differenceStamDrain = hoveredStamDrain - equippedStamDrain;

        comparisonText.AppendLine($"$item_blockarmor: ({(difference >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{difference}</color>)");
        comparisonText.AppendLine($"$item_blockforce: ({(differenceBlockForce >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceBlockForce}</color>)");
        comparisonText.AppendLine($"$item_staminahold: ({(differenceStamDrain >= 0 ? "<color=#00FF00> + " : "<color=#FF0000>")}{differenceStamDrain}</color>)");
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

[HarmonyPatch(typeof(UITooltip), nameof(UITooltip.LateUpdate))]
static class UITooltipLateUpdatePatch
{
    static void Postfix(UITooltip __instance)
    {
        if (InventoryGridCreateItemTooltipPatch.clonedTooltip == null) return;
        RectTransform originalRT = __instance.GetComponent<RectTransform>();
        if (UITooltip.m_current != null && !UITooltip.m_tooltip.activeSelf)
        {
            __instance.m_showTimer += Time.deltaTime;
            if (__instance.m_showTimer > 0.5 || ZInput.IsGamepadActive() && !ZInput.IsMouseActive())
                InventoryGridCreateItemTooltipPatch.clonedTooltip.SetActive(true);
        }

        if (ZInput.IsGamepadActive() && !ZInput.IsMouseActive())
        {
            if (__instance.m_gamepadFocusObject != null)
            {
                if (__instance.m_gamepadFocusObject.activeSelf && UITooltip.m_current != __instance)
                    InventoryGridCreateItemTooltipPatch.clonedTooltip.SetActive(true);
                else if (!__instance.m_gamepadFocusObject.activeSelf && UITooltip.m_current == __instance)
                    DestroyTooltip();
            }
            else if (__instance.m_selectable)
            {
                if (EventSystem.current.currentSelectedGameObject == __instance.m_selectable.gameObject && UITooltip.m_current != __instance)
                    InventoryGridCreateItemTooltipPatch.clonedTooltip.SetActive(true);
                else if (EventSystem.current.currentSelectedGameObject != __instance.m_selectable.gameObject && UITooltip.m_current == __instance)
                    DestroyTooltip();
            }

            if (!(UITooltip.m_current == __instance) || !(UITooltip.m_tooltip != null))
                return;
            if (__instance.m_anchor != null)
            {
                InventoryGridCreateItemTooltipPatch.clonedTooltip.transform.SetParent(__instance.m_anchor);
                InventoryGridCreateItemTooltipPatch.clonedTooltip.transform.localPosition = __instance.m_fixedPosition + new Vector2(originalRT.rect.width + 300, -100);
            }
            else if (__instance.m_fixedPosition != Vector2.zero)
            {
                InventoryGridCreateItemTooltipPatch.clonedTooltip.transform.position = __instance.m_fixedPosition + new Vector2(originalRT.rect.width + 300, -100);
            }
            else
            {
                Player.m_localPlayer.m_nview.GetZDO().GetFloat(ZDOVars.s_stamina, 0.0f);
                RectTransform transform = __instance.gameObject.transform as RectTransform;
                Vector3[] vector3Array = new Vector3[4];
                Vector3[] fourCornersArray = vector3Array;
                transform.GetWorldCorners(fourCornersArray);
                InventoryGridCreateItemTooltipPatch.clonedTooltip.transform.position = ((vector3Array[1] + vector3Array[2]) / 2f) + new Vector3(originalRT.rect.width + 300, -100, 0);
                Utils.ClampUIToScreen(InventoryGridCreateItemTooltipPatch.clonedTooltip.transform as RectTransform);
            }
        }
        else
        {
            if (!(UITooltip.m_current == __instance))
                return;
            if (UITooltip.m_hovered == null)
                DestroyTooltip();
            else if (UITooltip.m_tooltip.activeSelf && !RectTransformUtility.RectangleContainsScreenPoint(UITooltip.m_hovered.transform as RectTransform, ZInput.mousePosition))
            {
                DestroyTooltip();
            }
            else
            {
                InventoryGridCreateItemTooltipPatch.clonedTooltip.transform.position = ZInput.mousePosition + new Vector3(originalRT.rect.width + 300, 0, 0);
                Utils.ClampUIToScreen(InventoryGridCreateItemTooltipPatch.clonedTooltip.transform as RectTransform);
            }
        }
    }

    private static void DestroyTooltip()
    {
        if (InventoryGridCreateItemTooltipPatch.clonedTooltip != null)
        {
            //ItemComparePlugin.ItemCompareLogger.LogInfo("UITooltip.OnPointerExit: Cloned tooltip exists, destroying it");
            Object.Destroy(InventoryGridCreateItemTooltipPatch.clonedTooltip);
            InventoryGridCreateItemTooltipPatch.clonedTooltip = null;
        }
    }
}