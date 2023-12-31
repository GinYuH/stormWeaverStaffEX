using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Items.Materials;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Typeless;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using rail;
using ReLogic.Content;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Security.Permissions;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace stormWeaverStaffEX.Items
{
    public class ArtificerTempestStaff : ModItem
	{
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Artificer Tempest Staff");
            Tooltip.SetDefault("Summons a baby Storm Weaver to protect you\nAdditional summons add more segments\nWhen not targeting something, the Weaver has armor and provides defense to the player.\nRight click or use a Whip to target an enemy");
        }
        public override void SetDefaults()
		{
			    Item.damage = 131;
                Item.DamageType = DamageClass.Summon;
			    Item.useTime = 5;
			    Item.useAnimation = Item.useTime;
			    Item.useStyle = ItemUseStyleID.Swing;
			    Item.noMelee	 = true;
                Item.shoot = ModContent.ProjectileType<ArtificerTempestHead>();
                Item.autoReuse = true;
                Item.useTurn = false;
                Item.UseSound= SoundID.Item1;
            }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            bool Exists = false;
            int tailID = 0;
            foreach (var projectile in Main.projectile)
            {
                if (projectile.type == type && projectile.owner == player.whoAmI && projectile.active)
                {
                    Exists = true;
                }

                if (projectile.type == ModContent.ProjectileType<ArtificerTempestTail>() && projectile.owner == player.whoAmI && projectile.active)
                {
                    tailID = projectile.whoAmI;
                }
            }
            float foundSlotsCount = 0f;
            for (int i = 0; i < 1000; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.minion && p.owner == player.whoAmI)
                {
                    foundSlotsCount += p.minionSlots;
                }
            }
            if (Exists)
            {
                if (!(foundSlotsCount + 0.5f > (float)player.maxMinions)) { 
                Projectile.NewProjectile(source, Main.projectile[tailID].Center, Vector2.Zero, ModContent.ProjectileType<ArtificerTempestBody>(), damage, knockback, player.whoAmI);
                }
            } else
            {
                if (!(foundSlotsCount + 1f > (float)player.maxMinions))
                {
                    Projectile.NewProjectile(source, Main.MouseWorld, player.DirectionTo(Main.MouseWorld) * 3, type, damage, knockback, player.whoAmI);
                    Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero * 3, ModContent.ProjectileType<ArtificerTempestTail>(), damage, knockback, player.whoAmI);
                }
            }
            return false;
        }
        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(Item.type);
            recipe.AddIngredient<ArmoredShell>(3);
            recipe.AddIngredient(ItemID.StardustDragonStaff);
            recipe.AddIngredient(ItemID.MagnetSphere);
            recipe.Register();

        }

    }

    public class ArtificerBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Mini Storm Weaver");
            base.Description.SetDefault("The baby Weaver will protect you");
            Main.buffNoTimeDisplay[base.Type] = true;
            Main.buffNoSave[base.Type] = true;
        }
            public override void Update(Player player, ref int buffIndex)
        { 
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ArtificerTempestHead>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            } else
            {
                player.DelBuff(buffIndex);
            }
        }
    }
	public class ArtificerTempestHead : ModProjectile
	{
        public override string Texture => "CalamityMod/NPCs/StormWeaver/StormWeaverHead";
        public bool armored = true;
        public int timer = 0;
        Dictionary<int, Projectile> segments = new Dictionary<int, Projectile>();
        public override void SetStaticDefaults()
        {

            ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
            /*CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);*/
        }

        public override void SetDefaults()
        {
			Projectile.timeLeft = 10;
            Projectile.width = 24;
			Projectile.height = 24;
            Projectile.friendly = true;
			Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
			Projectile.localNPCHitCooldown = 30;
			Projectile.usesLocalNPCImmunity = true;
            Projectile.tileCollide= false;
			Projectile.extraUpdates = 0;
			Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.minionSlots = 0.5f;
            Projectile.netImportant = true;

            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 100;
        }
        public override void AI()
        {

            //Main.NewText("head " + Projectile.owner);
            Player player = Main.player[Projectile.owner];
            armored = !player.HasMinionAttackTargetNPC;
            if (timer == 0)
            {
                player.AddBuff(ModContent.BuffType<ArtificerBuff>(), 60);
            }
            if (player.HasBuff<ArtificerBuff>())
            {
                Projectile.timeLeft = 10;
            } else
            {
                Projectile.Kill();
                return;
            }
            if (armored) ArmorAI(); else NakedAI();
            timer++;
            //Projectile.Center -= Projectile.velocity;
            segments.Clear();
            if (!Main.dedServ) { 
            foreach (var projectile in Main.projectile)
            {

                if (projectile.type == ModContent.ProjectileType<ArtificerTempestBody>() && projectile.owner == Projectile.owner && projectile.active)
                {
                        if (projectile.ModProjectile<ArtificerTempestBody>().segmentIndex != -1) { 
                    segments.Add(projectile.ModProjectile<ArtificerTempestBody>().segmentIndex, projectile);
                        }
                    }
                if (projectile.type == ModContent.ProjectileType <ArtificerTempestTail>() && projectile.owner == Projectile.owner && projectile.active)
                {
                    segments.Add(projectile.ModProjectile<ArtificerTempestTail>().segmentIndex, projectile);
                }
            }
            }
            for (var i = 1; i <= segments.Count; i++)
            {
                if (i < segments.Count)
                {
                    segments[i].ModProjectile<ArtificerTempestBody>().SegmentMove();
                } else
                {
                    segments[i].ModProjectile<ArtificerTempestTail>().SegmentMove();
                }
            }
        }
        private void ArmorAI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 targetPos = player.Center + new Vector2(200, 0).RotatedBy(MathHelper.ToRadians(timer*4));
            Projectile.velocity = Projectile.DirectionTo(targetPos)*10f;
            if (Projectile.Center.Distance(targetPos) > 160) Projectile.velocity *= Projectile.Center.Distance(targetPos)/100f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (timer % 60 == 30)
            {
                NPC targetNPC = Projectile.FindTargetWithinRange(5000);
                if (targetNPC != null) {
                    var spawnpoint = segments[segments.Count].Center;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnpoint, spawnpoint.DirectionTo(targetNPC.Center)*10,ModContent.ProjectileType<ArtificerTempestShot>(),(int)(Projectile.damage* (0.95f + 0.05f * segments.Count)), Projectile.knockBack,Projectile.owner); 
                }
            }
            player.statDefense += (int)Math.Ceiling(1f + 1f * segments.Count);
            player.lifeRegenTime += (int)Math.Ceiling(0.5f + 0.5f * segments.Count);
            player.lifeRegen += (int)Math.Ceiling(0.5f + 0.5f * segments.Count);
            player.endurance += (0.05f + 0.05f * segments.Count);
        }

        private void NakedAI()
        {
            Player player = Main.player[Projectile.owner];
            var target = Main.npc[player.MinionAttackTargetNPC];
            Vector2 targetPos = target.Center;

            if (timer % 60 == 0)
            {
                Projectile.velocity = Projectile.DirectionTo(targetPos) * 40f;
            }
            if (Projectile.velocity.Length() > 20) Projectile.velocity *= 0.98f;
            TurnTowards(targetPos);
            Projectile.rotation = MathHelper.WrapAngle(Projectile.velocity.ToRotation());
        }
        private void TurnTowards(Vector2 pos)
        {
            var rotAmount = Math.Clamp(MathHelper.WrapAngle(Projectile.rotation - Projectile.DirectionTo(pos).ToRotation()),MathHelper.ToRadians(-10), MathHelper.ToRadians(10));
           Projectile.velocity = Projectile.velocity.RotatedBy(-rotAmount);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("stormWeaverStaffEX/Items/StormWeaverHead" + (armored ? "_Armor" : "_Naked"), (AssetRequestMode)2).Value;
            Texture2D texBody = ModContent.Request<Texture2D>("stormWeaverStaffEX/Items/StormWeaverBody" + (armored ? "_Armor" : "_Naked"), (AssetRequestMode)2).Value;
            Texture2D texTail = ModContent.Request<Texture2D>("stormWeaverStaffEX/Items/StormWeaverTail" + (armored ? "_Armor" : "_Naked"), (AssetRequestMode)2).Value;
            for (var i = segments.Count; i > 0; i--)
            {
                if (i < segments.Count)
                {
                    Main.EntitySpriteDraw(texBody, segments[i].Center - Main.screenPosition, null, segments[i].GetAlpha(Lighting.GetColor((segments[i].Center / 16).ToPoint())), segments[i].rotation + MathHelper.Pi, texBody.Size() / 2f, segments[i].scale, SpriteEffects.None, 0);
                } else
                {
                    Main.EntitySpriteDraw(texTail, segments[i].Center - Main.screenPosition-new Vector2(10, 0).RotatedBy(segments[i].rotation), null, segments[i].GetAlpha(Lighting.GetColor((segments[i].Center / 16).ToPoint())), segments[i].rotation + MathHelper.Pi, texTail.Size() / 2f, segments[i].scale, SpriteEffects.None, 0);

                }
            }
            Main.EntitySpriteDraw(tex, base.Projectile.Center - Main.screenPosition, null, base.Projectile.GetAlpha(lightColor), base.Projectile.rotation+MathHelper.Pi, tex.Size() / 2f, base.Projectile.scale, SpriteEffects.None, 0);
            return false;

        }
    }

    public class ArtificerTempestBody : ModProjectile
    {
        public override string Texture => "CalamityMod/NPCs/StormWeaver/StormWeaverBody";
        public bool armored = true;
        public int segmentIndex = -1;

        public override void SetStaticDefaults()
        {

            ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
            /*CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);*/
        }

        public override void SetDefaults()
        {
            Projectile.timeLeft = 10;
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.idStaticNPCHitCooldown = 4;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.minionSlots = 0.5f;
            Projectile.netImportant = true;

        }

        public override void OnSpawn(IEntitySource source)
        {
            foreach (var projectile in Main.projectile)
            {
                if (projectile.type == ModContent.ProjectileType<ArtificerTempestTail>() && projectile.owner == Projectile.owner && projectile.active)
                {
                    segmentIndex = projectile.ModProjectile<ArtificerTempestTail>().segmentIndex;
                    projectile.ModProjectile<ArtificerTempestTail>().segmentIndex++;
                }
            }
        }
        public override void AI()
        {
            if (segmentIndex == -1)
            {
                foreach (var projectile in Main.projectile)
                {
                    if (projectile.type == ModContent.ProjectileType<ArtificerTempestTail>() && projectile.owner == Projectile.owner && projectile.active)
                    {
                        segmentIndex = projectile.ModProjectile<ArtificerTempestTail>().segmentIndex;
                        projectile.ModProjectile<ArtificerTempestTail>().segmentIndex++;
                    }
                }
            }
            Player player = Main.player[Projectile.owner];
            if (player.HasBuff<ArtificerBuff>())
            {
                Projectile.timeLeft = 10;
            }
            else
            {
                Projectile.Kill();
            }
        }

        internal void SegmentMove()
        {
            Player player = Main.player[Projectile.owner];
            armored = !player.HasMinionAttackTargetNPC;
            var live = false;
            Projectile nextSegment = new Projectile();
            ArtificerTempestHead head = new ArtificerTempestHead();
            for (int i = 0; i < 1000; i++)
            {
                var projectile = Main.projectile[i];
                if (projectile.type == ModContent.ProjectileType<ArtificerTempestBody>() && projectile.owner == Projectile.owner && projectile.active)
                {
                    if (projectile.ModProjectile<ArtificerTempestBody>().segmentIndex == segmentIndex - 1)
                    {
                        live = true;
                        nextSegment = projectile;
                    }
                }
                if (projectile.type == ModContent.ProjectileType<ArtificerTempestHead>() && projectile.owner == Projectile.owner && projectile.active)
                {
                    if (segmentIndex == 1)
                    {
                        live = true;
                        nextSegment = projectile;
                    }
                    head = projectile.ModProjectile<ArtificerTempestHead>();
                }
            }
            if (!live) Projectile.Kill();
            Vector2 destinationOffset = nextSegment.Center+nextSegment.velocity - Projectile.Center;
            if (nextSegment.rotation != Projectile.rotation)
            {
                float angle = MathHelper.WrapAngle(nextSegment.rotation - Projectile.rotation);
                //destinationOffset = Projectile.Center.DirectionTo(nextSegment.Center);
                destinationOffset = destinationOffset.RotatedBy(angle * 0.1f);
            }
            Projectile.rotation = destinationOffset.ToRotation();
            if (destinationOffset != Vector2.Zero)
            {
                Projectile.Center = nextSegment.Center+nextSegment.velocity - destinationOffset.SafeNormalize(Vector2.Zero) * 19;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
        public override void Kill(int timeLeft)
        {
            if (Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<ArtificerTempestHead>()] > 0)
            {
                for (int i = 0; i < 1000; i++)
                {
                    var projectile = Main.projectile[i];
                    if (projectile.type == ModContent.ProjectileType<ArtificerTempestHead>() && projectile.owner == Projectile.owner && projectile.active)
                    {
                        projectile.Kill();
                    }
                }
            }
        }
    }

    public class ArtificerTempestTail : ModProjectile
    {
        public override string Texture => "CalamityMod/NPCs/StormWeaver/StormWeaverTail";
        public bool armored = true;
        public int segmentIndex = 1;

        public override void SetStaticDefaults()
        {

            ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
            /*CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);*/
        }

        public override void SetDefaults()
        {
            Projectile.timeLeft = 10;
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.localNPCHitCooldown = 30;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.minionSlots = 0.5f;
            Projectile.netImportant = true;

        }
        public override void AI()
        {
            //Main.NewText("tail " +Projectile.owner);
            Player player = Main.player[Projectile.owner];
            if (player.HasBuff<ArtificerBuff>())
            {
                Projectile.timeLeft = 10;
            }
            else
            {
                Projectile.Kill();
            }
        }
        internal void SegmentMove()
        {
            Player player = Main.player[Projectile.owner];
            armored = !player.HasMinionAttackTargetNPC;
            var live = false;
            Projectile nextSegment = new Projectile();
            ArtificerTempestHead head = new ArtificerTempestHead();

            for (int i = 0; i < 1000; i++)
            {
                var projectile = Main.projectile[i];
                if (projectile.type == ModContent.ProjectileType<ArtificerTempestBody>() && projectile.owner == Projectile.owner && projectile.active)
                {
                    if (projectile.ModProjectile<ArtificerTempestBody>().segmentIndex == segmentIndex - 1)
                    {
                        live = true;
                        nextSegment = projectile;
                    }
                }
                if (projectile.type == ModContent.ProjectileType<ArtificerTempestHead>() && projectile.owner == Projectile.owner && projectile.active)
                {
                    if (segmentIndex == 1)
                    {
                        live = true;
                        nextSegment = projectile;
                    }
                    head = projectile.ModProjectile<ArtificerTempestHead>();
                }
            }
            if (!live) Projectile.Kill();
            Vector2 destinationOffset = nextSegment.Center - Projectile.Center;
            if (nextSegment.rotation != Projectile.rotation)
            {
                float angle = MathHelper.WrapAngle(nextSegment.rotation - Projectile.rotation);
                //destinationOffset = Projectile.Center.DirectionTo(nextSegment.Center);
                destinationOffset = destinationOffset.RotatedBy(angle * 0.1f);
            }
            Projectile.rotation = destinationOffset.ToRotation();
            if (destinationOffset != Vector2.Zero)
            {
                Projectile.Center = nextSegment.Center - destinationOffset.SafeNormalize(Vector2.Zero) * 19;
            }
            Projectile.velocity = Vector2.Zero;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void Kill(int timeLeft)
        {
            if (Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<ArtificerTempestHead>()] > 0)
            {
                for (int i = 0; i < 1000; i++)
                {
                    var projectile = Main.projectile[i];
                    if (projectile.type == ModContent.ProjectileType<ArtificerTempestHead>() && projectile.owner == Projectile.owner && projectile.active)
                    {
                        projectile.Kill();
                    }
                }
            }
        }
    }

    public class ArtificerTempestShot : ModProjectile
    {
        public override string Texture => ModContent.GetModProjectile(ModContent.ProjectileType<VoidVortexProj>()).Texture;
        public bool armored = true;
        public int segmentIndex = 1;

        public override void SetStaticDefaults()
        {

            Main.projFrames[base.Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.localNPCHitCooldown = 30;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = false;

        }
        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 4)
                {
                    Projectile.frame = 0;
                }
            }
        }
       
    }

}