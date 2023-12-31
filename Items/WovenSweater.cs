
using CalamityMod.Items;
using CalamityMod;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Rarities;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using Terraria;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;

namespace stormWeaverStaffEX.Items
{
    [AutoloadEquip(EquipType.Body)]
    public class WovenSweater : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Woven Sweater");
            base.Tooltip.SetDefault("Immunity to Electrified\n+1 max minion\n20% damage reduction\n+69 maximum life\nSeems very durable.\n'Baby Weaver loves you!'");
        }

        public override void SetDefaults()
        {
            Item.width = 18; // Width of the item
            Item.height = 18; // Height of the item
            Item.value = Item.sellPrice(platinum: 1); // How many coins the item is worth
            Item.rare = ItemRarityID.Cyan; // The rarity of the item
            Item.defense = 60;
        }

        public override void UpdateEquip(Player player)
        {
            player.buffImmune[BuffID.Electrified] = true; // Make the player immune to Fire
            player.statLifeMax2 += 69;
            player.maxMinions += 1;
            player.endurance += 0.2f;
        }
    }
}