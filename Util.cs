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
            Object.Destroy(InventoryGridCreateItemTooltipPatch.ClonedTooltip);
            InventoryGridCreateItemTooltipPatch.ClonedTooltip = null!;
        }
    }

    internal static ItemDrop.ItemData? FindEquippedItemMatching(ItemDrop.ItemData hoveredItem)
    {
        Player player = Player.m_localPlayer;
        if (player == null)
            return null;
        return hoveredItem.IsWeapon()
            ? player.GetInventory().GetEquippedItems().FirstOrDefault(i => i.IsWeapon())
            : player.GetInventory().GetEquippedItems().FirstOrDefault(i => i.m_shared.m_itemType == hoveredItem.m_shared.m_itemType);
    }

    public static void AddDurabilityComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredDurability = hoveredItem.GetMaxDurability();
        float equippedDurability = equippedItem.GetMaxDurability();
        float difference = hoveredDurability - equippedDurability;
        if (difference != 0)
            comparisonText.AppendLine($"$item_durability: ({(difference >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{difference:+0;-0}</color>)");
    }

    public static void AddDamageComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText, Player player)
    {
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
            comparisonText.AppendLine($"$inventory_damage: ({(differenceTrue >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceTrue:+0;-0}</color>)");
        if (differenceBlunt != 0)
            comparisonText.AppendLine($"$inventory_blunt: ({(differenceBlunt >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceBlunt:+0;-0}</color>)");
        if (differenceSlash != 0)
            comparisonText.AppendLine($"$inventory_slash: ({(differenceSlash >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceSlash:+0;-0}</color>)");
        if (differencePierce != 0)
            comparisonText.AppendLine($"$inventory_pierce: ({(differencePierce >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differencePierce:+0;-0}</color>)");
        if (differenceChop != 0)
            comparisonText.AppendLine($"$inventory_chop: ({(differenceChop >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceChop:+0;-0}</color>)");
        if (differencePickaxe != 0)
            comparisonText.AppendLine($"$inventory_pickaxe: ({(differencePickaxe >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differencePickaxe:+0;-0}</color>)");
        if (differenceFire != 0)
            comparisonText.AppendLine($"$inventory_fire: ({(differenceFire >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceFire:+0;-0}</color>)");
        if (differenceFrost != 0)
            comparisonText.AppendLine($"$inventory_frost: ({(differenceFrost >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceFrost:+0;-0}</color>)");
        if (differenceLightning != 0)
            comparisonText.AppendLine($"$inventory_lightning: ({(differenceLightning >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceLightning:+0;-0}</color>)");
        if (differencePoison != 0)
            comparisonText.AppendLine($"$inventory_poison: ({(differencePoison >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differencePoison:+0;-0}</color>)");
        if (differenceSpirit != 0)
            comparisonText.AppendLine($"$inventory_spirit: ({(differenceSpirit >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceSpirit:+0;-0}</color>)");
        if (differenceTotal != 0)
            comparisonText.AppendLine($"$item_total $inventory_damage: ({(differenceTotal >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceTotal:+0;-0}</color>)");
    }

    public static void AddArmorComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredArmor = hoveredItem.GetArmor();
        float equippedArmor = equippedItem.GetArmor();
        float difference = hoveredArmor - equippedArmor;
        if (difference != 0)
            comparisonText.AppendLine($"$item_armor: ({(difference >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{difference:+0;-0}</color>)");
    }

    public static void AddWeightComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        float hoveredWeight = hoveredItem.GetWeight();
        float equippedWeight = equippedItem.GetWeight();
        float difference = hoveredWeight - equippedWeight;
        if (difference != 0)
            comparisonText.AppendLine($"$item_weight: ({(difference >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{difference:+0;-0}</color>)");
    }

    public static void AddValueComparison(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        int hoveredValue = hoveredItem.GetValue();
        int equippedValue = equippedItem.GetValue();
        int difference = hoveredValue - equippedValue;
        if (difference != 0)
            comparisonText.AppendLine($"$item_value: ({(difference >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{difference:+0;-0}</color>)");
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
            comparisonText.AppendLine($"$item_staminause: ({(differenceAttackStamina >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceAttackStamina:+0;-0}</color>)");
        if (differenceAttackEitr != 0)
            comparisonText.AppendLine($"$item_eitruse: ({(differenceAttackEitr >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceAttackEitr:+0;-0}</color>)");
        if (differenceHealthUse != 0)
            comparisonText.AppendLine($"$item_healthuse: ({(differenceHealthUse >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceHealthUse:+0;-0}</color>)");
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
            comparisonText.AppendLine($"$item_blockarmor: ({(difference >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{difference:+0;-0}</color>)");
        if (differenceBlockForce != 0)
            comparisonText.AppendLine($"$item_blockforce: ({(differenceBlockForce >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceBlockForce:+0;-0}</color>)");
        if (differenceParryBonus != 0)
            comparisonText.AppendLine($"$item_parrybonus: ({(differenceParryBonus >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceParryBonus:+0;-0}x</color>)");
        if (differenceStamDrain != 0)
            comparisonText.AppendLine($"$item_staminahold: ({(differenceStamDrain >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceStamDrain:+0;-0}</color>)");
        if (differenceKnockback != 0)
            comparisonText.AppendLine($"$item_knockback: ({(differenceKnockback >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceKnockback:+0;-0}</color>)");
        if (differenceBackstab != 0)
            comparisonText.AppendLine($"$item_backstab: ({(differenceBackstab >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceBackstab:+0;-0}x</color>)");

        if (differenceEitrRegenModifier != 0)
            comparisonText.AppendLine($"$item_eitrregen_modifier: ({(differenceEitrRegenModifier >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceEitrRegenModifier:+0;-0}</color>)");
        if (differenceMovementModifier != 0)
            comparisonText.AppendLine($"$item_movement_modifier: ({(differenceMovementModifier >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceMovementModifier:+0;-0}</color>)");
        if (differenceBaseItemStaminaModifier != 0)
            comparisonText.AppendLine($"$base_item_modifier: ({(differenceBaseItemStaminaModifier >= 0 ? "<color=#00FF00>" : "<color=#FF0000>")}{differenceBaseItemStaminaModifier:+0;-0}</color>)");
        // Extract status effect tooltips for hovered and equipped items
        string hoveredStatusEffectTooltip = hoveredItem.GetStatusEffectTooltip(hoveredItem.m_quality, Player.m_localPlayer.GetSkillLevel(hoveredItem.m_shared.m_skillType));
        string equippedStatusEffectTooltip = equippedItem.GetStatusEffectTooltip(equippedItem.m_quality, Player.m_localPlayer.GetSkillLevel(equippedItem.m_shared.m_skillType));

        // Check if the status effects are different
        if (hoveredStatusEffectTooltip != equippedStatusEffectTooltip)
        {
            // Append a header for clarity
            comparisonText.AppendLine("\n\n$inventory_activeeffects:");

            // If the equipped item has a status effect that the hovered item doesn't
            if (!string.IsNullOrEmpty(equippedStatusEffectTooltip) && string.IsNullOrEmpty(hoveredStatusEffectTooltip))
            {
                comparisonText.AppendLine($"<color=#FF0000>- $hud_remove: {equippedStatusEffectTooltip}</color>");
            }
            // If the hovered item has a status effect that the equipped item doesn't
            else if (string.IsNullOrEmpty(equippedStatusEffectTooltip) && !string.IsNullOrEmpty(hoveredStatusEffectTooltip))
            {
                comparisonText.AppendLine($"<color=#00FF00>+ $piece_smelter_add: {hoveredStatusEffectTooltip}</color>");
            }
            // If both items have status effects but they are different
            else
            {
                comparisonText.AppendLine($"<color=#FF0000>- $hud_remove: {equippedStatusEffectTooltip}</color>");
                comparisonText.AppendLine($"<color=#00FF00>+ $piece_smelter_add: {hoveredStatusEffectTooltip}</color>");
            }
        }

        // Determine the set effects for both hovered and equipped items
        string hoveredSetEffectTooltip = hoveredItem.GetSetStatusEffectTooltip(hoveredItem.m_quality, Player.m_localPlayer.GetSkillLevel(hoveredItem.m_shared.m_skillType));
        string equippedSetEffectTooltip = equippedItem.GetSetStatusEffectTooltip(equippedItem.m_quality, Player.m_localPlayer.GetSkillLevel(equippedItem.m_shared.m_skillType));

        // Check if the set effects are different
        if (hoveredSetEffectTooltip != equippedSetEffectTooltip)
        {
            // Append a header to indicate a comparison is being made
            comparisonText.AppendLine("\n\n$item_seteffect:");

            // If the equipped item has a set effect that the hovered item doesn't
            if (!string.IsNullOrEmpty(equippedSetEffectTooltip) && string.IsNullOrEmpty(hoveredSetEffectTooltip))
            {
                comparisonText.AppendLine($"<color=#FF0000>- $hud_remove: <color=orange>{equippedItem.m_shared.m_setStatusEffect.m_name}</color> {equippedSetEffectTooltip}</color>");
            }
            // If the hovered item has a set effect that the equipped item doesn't
            else if (string.IsNullOrEmpty(equippedSetEffectTooltip) && !string.IsNullOrEmpty(hoveredSetEffectTooltip))
            {
                comparisonText.AppendLine($"<color=#00FF00>+ $piece_smelter_add: <color=orange>{hoveredItem.m_shared.m_setStatusEffect.m_name}</color> {hoveredSetEffectTooltip}</color>");
            }
            // If both items have set effects but they are different
            else
            {
                comparisonText.AppendLine($"<color=#FF0000>- $hud_remove: <color=orange>{equippedItem.m_shared.m_setStatusEffect.m_name}</color> {equippedSetEffectTooltip}</color>");
                comparisonText.AppendLine($"<color=#00FF00>+ $piece_smelter_add: <color=orange>{hoveredItem.m_shared.m_setStatusEffect.m_name}</color> {hoveredSetEffectTooltip}</color>");
            }
        }
    }

    public static void AddSeInformation(ItemDrop.ItemData hoveredItem, ItemDrop.ItemData equippedItem, StringBuilder comparisonText)
    {
        
        // Determine the modifiers for both hovered and equipped items
        string hoveredModifiersTooltip = SE_Stats.GetDamageModifiersTooltipString(hoveredItem.m_shared.m_damageModifiers);
        string equippedModifiersTooltip = SE_Stats.GetDamageModifiersTooltipString(equippedItem.m_shared.m_damageModifiers);

        // Check if the modifiers are different
        if (hoveredModifiersTooltip != equippedModifiersTooltip)
        {
            // If the equipped item has a modifier that the hovered item doesn't
            if (!string.IsNullOrEmpty(equippedModifiersTooltip) && string.IsNullOrEmpty(hoveredModifiersTooltip))
            {
                comparisonText.AppendLine($"<color=#FF0000>- $hud_remove: {equippedModifiersTooltip}</color>");
            }
            // If the hovered item has a modifier that the equipped item doesn't
            else if (string.IsNullOrEmpty(equippedModifiersTooltip) && !string.IsNullOrEmpty(hoveredModifiersTooltip))
            {
                comparisonText.AppendLine($"<color=#00FF00>+ $piece_smelter_add: {hoveredModifiersTooltip}</color>");
            }
            // If both items have modifiers but they are different
            else
            {
                comparisonText.AppendLine($"<color=#FF0000>- $hud_remove: {equippedModifiersTooltip}</color>");
                comparisonText.AppendLine($"<color=#00FF00>+ $piece_smelter_add: {hoveredModifiersTooltip}</color>");
            }
        }
    }
}