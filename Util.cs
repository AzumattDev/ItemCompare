using System.Linq;
using System.Text;
using ItemCompare.Patches;
using UnityEngine;

namespace ItemCompare;

public class Util
{
    public static string ColorToHexString(Color color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }

    internal static void DestroyTooltip()
    {
        if (InventoryGridCreateItemTooltipPatch.ClonedTooltip != null)
        {
            //ItemComparePlugin.ItemCompareLogger.LogInfo("UITooltip.OnPointerExit: Cloned tooltip exists, destroying it");
            Object.Destroy(InventoryGridCreateItemTooltipPatch.ClonedTooltip);
            InventoryGridCreateItemTooltipPatch.ClonedTooltip = null!;
        }
    }

    internal static ItemDrop.ItemData? FindEquippedItemMatching(ItemDrop.ItemData hoveredItem)
    {
        Player player = Player.m_localPlayer;
        return player.GetInventory().GetEquippedItems().FirstOrDefault(i => i.m_shared.m_itemType == hoveredItem.m_shared.m_itemType);
    }

    public static void AddDurabilityComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredDurability = hoveredItem.GetMaxDurability();
        float equippedDurability = equippedItem.GetMaxDurability();
        float difference = hoveredDurability - equippedDurability;
        if (difference != 0)
            comparisonText.AppendLine($"$item_durability: ({(difference >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{difference}</color>)");
    }

    public static void AddDamageComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText, Player player)
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

        if (differenceTrue != 0)
            comparisonText.AppendLine($"$inventory_damage: ({(differenceTrue >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceTrue}</color>)");
        if (differenceBlunt != 0)
            comparisonText.AppendLine($"$inventory_blunt: ({(differenceBlunt >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceBlunt}</color>)");
        if (differenceSlash != 0)
            comparisonText.AppendLine($"$inventory_slash: ({(differenceSlash >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceSlash}</color>)");
        if (differencePierce != 0)
            comparisonText.AppendLine($"$inventory_pierce: ({(differencePierce >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differencePierce}</color>)");
        if (differenceChop != 0)
            comparisonText.AppendLine($"$inventory_chop: ({(differenceChop >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceChop}</color>)");
        if (differencePickaxe != 0)
            comparisonText.AppendLine($"$inventory_pickaxe: ({(differencePickaxe >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differencePickaxe}</color>)");
        if (differenceFire != 0)
            comparisonText.AppendLine($"$inventory_fire: ({(differenceFire >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceFire}</color>)");
        if (differenceFrost != 0)
            comparisonText.AppendLine($"$inventory_frost: ({(differenceFrost >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceFrost}</color>)");
        if (differenceLightning != 0)
            comparisonText.AppendLine($"$inventory_lightning: ({(differenceLightning >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceLightning}</color>)");
        if (differencePoison != 0)
            comparisonText.AppendLine($"$inventory_poison: ({(differencePoison >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differencePoison}</color>)");
        if (differenceSpirit != 0)
            comparisonText.AppendLine($"$inventory_spirit: ({(differenceSpirit >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceSpirit}</color>)");
        if (differenceTotal != 0)
            comparisonText.AppendLine($"$item_total $inventory_damage: ({(differenceTotal >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceTotal}</color>)");
    }

    public static void AddArmorComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredArmor = hoveredItem.GetArmor();
        float equippedArmor = equippedItem.GetArmor();
        float difference = hoveredArmor - equippedArmor;
        if (difference != 0)
            comparisonText.AppendLine($"$item_armor: ({(difference >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{difference}</color>)");
    }

    public static void AddWeightComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredWeight = hoveredItem.GetWeight();
        float equippedWeight = equippedItem.GetWeight();
        float difference = hoveredWeight - equippedWeight;
        if (difference != 0)
            comparisonText.AppendLine($"$item_weight: ({(difference >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{difference}</color>)");
    }

    public static void AddValueComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        int hoveredValue = hoveredItem.GetValue();
        int equippedValue = equippedItem.GetValue();
        int difference = hoveredValue - equippedValue;
        if (difference != 0)
            comparisonText.AppendLine($"$item_value: ({(difference >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{difference}</color>)");
    }

    public static void AddUseComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
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

        if (differenceAttackStamina != 0)
            comparisonText.AppendLine($"$item_staminause: ({(differenceAttackStamina >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceAttackStamina}</color>)");
        if (differenceAttackEitr != 0)
            comparisonText.AppendLine($"$item_eitruse: ({(differenceAttackEitr >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceAttackEitr}</color>)");
        if (differenceHealthUse != 0)
            comparisonText.AppendLine($"$item_healthuse: ({(differenceHealthUse >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceHealthUse}</color>)");
    }

    public static void AddOtherStatComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
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

        float hoveredKnockback = hoveredItem.m_shared.m_attackForce;
        float equippedKnockback = equippedItem.m_shared.m_attackForce;
        float differenceKnockback = hoveredKnockback - equippedKnockback;

        float hoveredBackstab = hoveredItem.m_shared.m_backstabBonus;
        float equippedBackstab = equippedItem.m_shared.m_backstabBonus;
        float differenceBackstab = hoveredBackstab - equippedBackstab;

        float eitrRegenModifier = hoveredItem.m_shared.m_eitrRegenModifier;
        float equippedEitrRegenModifier = equippedItem.m_shared.m_eitrRegenModifier;
        float differenceEitrRegenModifier = eitrRegenModifier - equippedEitrRegenModifier;

        float hoveredMovementModifier = hoveredItem.m_shared.m_movementModifier;
        float equippedMovementModifier = equippedItem.m_shared.m_movementModifier;
        float differenceMovementModifier = hoveredMovementModifier - equippedMovementModifier;

        float hoveredBaseItemStaminaModifier = hoveredItem.m_shared.m_baseItemsStaminaModifier;
        float equippedBaseItemStaminaModifier = equippedItem.m_shared.m_baseItemsStaminaModifier;
        float differenceBaseItemStaminaModifier = hoveredBaseItemStaminaModifier - equippedBaseItemStaminaModifier;

        float hoveredParryBonus = hoveredItem.m_shared.m_timedBlockBonus;
        float equippedParryBonus = equippedItem.m_shared.m_timedBlockBonus;
        float differenceParryBonus = hoveredParryBonus - equippedParryBonus;

        if (difference != 0)
            comparisonText.AppendLine($"$item_blockarmor: ({(difference >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{difference}</color>)");
        if (differenceBlockForce != 0)
            comparisonText.AppendLine($"$item_blockforce: ({(differenceBlockForce >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceBlockForce}</color>)");
        if (differenceParryBonus != 0)
            comparisonText.AppendLine($"$item_parrybonus: ({(differenceParryBonus >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceParryBonus}x</color>)");
        if (differenceStamDrain != 0)
            comparisonText.AppendLine($"$item_staminahold: ({(differenceStamDrain >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceStamDrain}</color>)");
        if (differenceKnockback != 0)
            comparisonText.AppendLine($"$item_knockback: ({(differenceKnockback >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceKnockback}</color>)");
        if (differenceBackstab != 0)
            comparisonText.AppendLine($"$item_backstab: ({(differenceBackstab >= 0 ? "<color=#00FF00>+" : "<color=#FF0000>")}{differenceBackstab}x</color>)");

        if (differenceEitrRegenModifier != 0)
            comparisonText.AppendLine($"$item_eitrregen_modifier: ({(differenceEitrRegenModifier >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceEitrRegenModifier:+0;-0}</color>)");
        if (differenceMovementModifier != 0)
            comparisonText.AppendLine($"$item_movement_modifier: ({(differenceMovementModifier >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceMovementModifier:+0;-0}</color>)");
        if (differenceBaseItemStaminaModifier != 0)
            comparisonText.AppendLine($"$base_item_modifier: ({(differenceBaseItemStaminaModifier >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceBaseItemStaminaModifier:+0;-0}</color>)");

        if (hoveredItem.GetSetStatusEffectTooltip(hoveredItem.m_quality, Player.m_localPlayer.GetSkillLevel(hoveredItem.m_shared.m_skillType)) != equippedItem.GetSetStatusEffectTooltip(equippedItem.m_quality, Player.m_localPlayer.GetSkillLevel(equippedItem.m_shared.m_skillType)))
            comparisonText.AppendLine($"\n\n$item_seteffect: ({hoveredItem.GetSetStatusEffectTooltip(hoveredItem.m_quality, Player.m_localPlayer.GetSkillLevel(hoveredItem.m_shared.m_skillType))})");
    }

    public static void AddSeInformation(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        string modifiersTooltipString = SE_Stats.GetDamageModifiersTooltipString(equippedItem.m_shared.m_damageModifiers);
        if (modifiersTooltipString.Length > 0)
        {
            comparisonText.Append(modifiersTooltipString);
        }
    }
}