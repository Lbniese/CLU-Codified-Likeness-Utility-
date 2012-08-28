﻿using Clu.Helpers;
using TreeSharp;
using CommonBehaviors.Actions;
using Styx.Logic.Combat;
using Styx.WoWInternals;
using Styx;
using Clu.Settings;

namespace Clu.Classes.Paladin
{
	class Holy : HealerRotationBase
	{
		public override string Name
		{
			get {
				return "Holy Paladin";
			}
		}

		public override string KeySpell
		{
			get {
				return "Holy Shock";
			}
		}

		public override float CombatMaxDistance
		{
			get {
				return 34f;
			}
		}

		public override float CombatMinDistance
		{
			get {
				return 28f;
			}
		}

		// adding some help about cooldown management
		public override string Help
		{
			get {
				return "\n" +
					"----------------------------------------------------------------------\n" +
					"This Rotation will:\n" +
					"1. Has Sacred Cleansing? " + TalentManager.HasTalent(1, 14) + " \n" +
					"2. \n" +
					"3. AutomaticCooldowns has: \n" +
					"==> UseTrinkets \n" +
					"==> UseRacials \n" +
					"==> UseEngineerGloves \n" +
					"==>  \n" +
					"4.  \n" +
					"5. Best Suited for end game raiding\n" +
					"NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
					"Credits to cowdude\n" +
					"----------------------------------------------------------------------\n";
			}
		}

		public override Composite SingleRotation
		{
			get {
				return new PrioritySelector(
					// Pause Rotation
					new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

					// Spell.WaitForCast(false),

					// if someone dies make sure we retarget.
					TargetBase.EnsureTarget(ret => ObjectManager.Me.CurrentTarget == null),

					// For DS Encounters.
					EncounterSpecific.ExtraActionButton(),

					// emergency heals on me
					Spell.HealMe("Divine Protection", 	ret => Me.HealthPercent < 40 && CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat, "Divine Protection"),
					Spell.HealMe("Divine Shield", 		ret => Me.HealthPercent < 40 && !Buff.PlayerHasBuff("Forbearance") && CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat, "Divine Shield"),
					Spell.HealMe("Flash Heal", 			ret => Me.HealthPercent < 40, "flash heal on me, emergency"),

					// don't break PlayerIsChanneling
					new Decorator(x => Spell.PlayerIsChanneling, new Action()),

					// Ensure we have Seal of Insight during battle
					Buff.CastBuff("Seal of Insight", ret => true, "Seal of Insight"),

					// Mana regen
					Buff.CastBuff("Divine Plea", ret => Me.ManaPercent < 75 && StyxWoW.Me.Combat, "Divine Plea"),

					// emergency Cooldowns
					Healer.FindRaidMember(a => CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat && Spell.SpellCooldown("Hand of Sacrifice").TotalSeconds < 0.5, x => x.ToUnit().InLineOfSight && !x.ToUnit().Dead && x.HealthPercent < 45, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "emergency Cooldowns",
					                      Spell.CastSpell("Hand of Sacrifice", ret => (IsTank || IsHealer) && !IsMe, "Hand of Sacrifice")
					                     ),

					// emergency Cooldowns that cause Forbearance
					Healer.FindRaidMember(a => CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat && (Spell.SpellCooldown("Lay on Hands").TotalSeconds < 0.5 || Spell.SpellCooldown("Hand of Protection").TotalSeconds < 0.5), x => x.ToUnit().InLineOfSight && !x.ToUnit().Dead && x.HealthPercent < 35 && !x.ToUnit().HasAura("Forbearance"), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "emergency Cooldowns that cause Forbearance",
					                      Spell.CastSpell("Lay on Hands", ret => IsTank || IsHealer || IsMe, "Lay on Hands"),
					                      Spell.CastSpell("Hand of Protection", ret => !IsTank, "Hand of Protection")
					                     ),

					// emergency heals on most injured tank
					Healer.FindTank(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().Dead && x.HealthPercent < 50, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "emergency heals on most injured tank",
					                Spell.CastSpell("Holy Shock", a => HolyPower < 3 || Buff.PlayerHasActiveBuff("Daybreak"), "Holy Shock before WoG (emergency)"),
					                Spell.CastSpell("Word of Glory", a => HolyPower == 3, "Word of Glory (emergency)"),
					                Spell.CastSpell("Holy Shock", a => true, "Holy Shock"),
					                Spell.CastSpell("Divine Light", a => Buff.PlayerHasActiveBuff("Infusion of Light"), "Divine Light"),
					                Spell.CastSpell("Flash of Light", a => true, "Flash of Light (emergency)")
					               ),

					// Urgent Cleanse
					Healer.FindRaidMember(a => CLUSettings.Instance.EnableDispel, x => x.ToUnit().InLineOfSight && !x.ToUnit().Dead && x.ToUnit().HasAuraToDispel(true), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Cleanse (Urgent)",
					                      Spell.CastSpell("Cleanse", a => true, "Cleanse (Urgent)")
					                     ),

					// I'm fine and tanks are not dying => ensure nobody is REALLY low life
					Healer.FindRaidMember(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().Dead && x.HealthPercent < 45, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "I'm fine and tanks are not dying => ensure nobody is REALLY low life",
					                      Spell.CastSpell("Holy Shock", a => HolyPower < 3 || Buff.PlayerHasActiveBuff("Daybreak"), "Holy Shock before WoG (emergency)"),
					                      Spell.CastSpell("Word of Glory", a => HolyPower == 3, "Word of Glory (emergency)"),
					                      Spell.CastSpell("Holy Shock", a => true, "Holy Shock"),
					                      Spell.CastSpell("Divine Light", a => Buff.PlayerHasActiveBuff("Infusion of Light"), "Divine Light"),
					                      Spell.CastSpell("Flash of Light", a => true, "Flash of Light (emergency)")
					                     ),

					////// Cooldowns
					//Healer.FindAreaHeal(a => CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat && PaladinCooldownsOK, 10, 65, 30f, (Me.IsInRaid ? 6 : 4), "Cooldowns: Avg: 10-65, 38yrds, count: 6 or 3",
					// // Item.UseTrinkets(),
					// // Spell.UseRacials(),
					//                    Spell.CastSelfSpell("Divine Favor", ret => !Buff.PlayerHasActiveBuff("Guardian of Ancient Kings") && !Buff.PlayerHasActiveBuff("Avenging Wrath"), "Divine Favor"),
					//                    Buff.CastBuff("Guardian of Ancient Kings", ret => !Buff.PlayerHasActiveBuff("Avenging Wrath") && !Buff.PlayerHasActiveBuff("Divine Favour"), "Guardian of Ancient Kings"),
					//                    Buff.CastBuff("Avenging Wrath", ret => !Buff.PlayerHasActiveBuff("Guardian of Ancient Kings") && !Buff.PlayerHasActiveBuff("Divine Favour"), "Avenging Wrath")
					// // Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
					// // Item.UseEngineerGloves()
					//),

					// Beacon of Light on tank
					Healer.FindTank(a => true, x => !x.ToUnit().Dead && x.ToUnit().InLineOfSight && !x.ToUnit().HasMyAura("Beacon of Light") && x.Beacon, (a, b) => (int)(a.MaxHealth - b.MaxHealth), "Beacon of Light on tank",
					                Buff.CastTargetBuff("Beacon of Light", a => true, "Beacon of Light")
					               ),

					// cast Judgement for Judgements of the Pure
					Spell.CastSpellOnMostFocusedTarget("Judgement", u => Unit.MostFocusedUnit.Unit, ret => !Buff.PlayerHasActiveBuff("Judgements of the Pure") && StyxWoW.Me.Combat, "Judgement for (Judgements of the Pure)", false),

					// Holy Radiance
					Healer.FindAreaHeal(a => true, 10, 93, 12f, 4, "Holy Radiance party healing: Avg: 10-98 or 85, 11yrds, count: 5 or 4",
					                    Spell.CastFacingSpellOnTarget("Light of Dawn", ret => Me.CurrentTarget, a => HolyPower == 3, "Light of Dawn", true),
					                    Spell.CastSpell("Holy Radiance", a => Me.ManaPercent > 15, "Holy Radiance")),

					// single target healing not the tank
					Healer.FindRaidMember(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().Dead && x.HealthPercent < 92 && !x.ToUnit().HasMyAura("Beacon of Light"), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Single target healing",
					                      Spell.CastSpell("Holy Shock", a => HolyPower < 3 || Buff.PlayerHasActiveBuff("Daybreak"), "Holy Shock before WoG "),
					                      Spell.CastSpell("Holy Shock", a => true, "Holy Shock"),
					                      Spell.CastSpell("Word of Glory", a => HolyPower == 3, "Word of Glory"),
					                      Spell.CastSpell("Divine Light", a => Me.CurrentTarget != null && Buff.PlayerHasActiveBuff("Infusion of Light") && CurrentTargetHealthPercent < 80, "Divine Light"),
					                      Spell.CastSpell("Divine Light", a => Me.CurrentTarget != null && CurrentTargetHealthPercent < 80, "Divine Light"),
					                      Spell.CastSpell("Holy Light", a => true, "Holy Light")
					                     ),

					// single target healing with the tank
					Healer.FindTank(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().Dead && x.HealthPercent < 92, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Single target Tank healing",
					                Spell.CastSpell("Holy Shock", a => HolyPower < 3 || Buff.PlayerHasActiveBuff("Daybreak"), "Holy Shock before WoG "),
					                Spell.CastSpell("Word of Glory", a => HolyPower == 3, "Word of Glory"),
					                Spell.CastSpell("Holy Shock", a => true, "Holy Shock"),
					                Spell.CastSpell("Divine Light", a => Me.CurrentTarget != null && Buff.PlayerHasActiveBuff("Infusion of Light") && CurrentTargetHealthPercent < 80, "Divine Light"),
					                Spell.CastSpell("Divine Light", a => Me.CurrentTarget != null && CurrentTargetHealthPercent < 80, "Divine Light"),
					                Spell.CastSpell("Holy Light", a => true, "Holy Light")
					               ),

					// Cleanse
					Healer.FindRaidMember(a => CLUSettings.Instance.EnableDispel, x => x.ToUnit().InLineOfSight && !x.ToUnit().Dead && x.ToUnit().HasAuraToDispel(false), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Cleanse",
					                      Spell.CastSpell("Cleanse", a => true, "Cleanse ")
					                     ),

					//// Nub DPS
					//Healer.FindDPS(a => Spell.SpellCooldown("Hand of Salvation").TotalSeconds < 0.3 && CLUSettings.Instance.UseCooldowns, x => x.InLineOfSight && x.Dead && x.CurrentTarget.ThreatInfo.RawPercent > 90, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Nub DPS Salvation",
					//        Spell.CastSpell("Hand of Salvation", a => true, "Hand of Salvation (Threat)")
					//),

					// cast Holy Shock while moving
					Healer.FindRaidMember(a => Me.IsMoving && SpellManager.CanCast("Holy Shock"), x => x.ToUnit().InLineOfSight && !x.ToUnit().Dead && x.HealthPercent < 90, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "cast Holy Shock while moving",
					                      Spell.CastSpell("Holy Shock", a => true, "Holy Shock while moving")
					                     ),

					// If no one to heal make sure we use the free Holy Shock with Daybreak for Holy power generation.
					Spell.HealMe("Holy Shock", a => Buff.PlayerHasActiveBuff("Daybreak") && Buff.PlayerActiveBuffTimeLeft("Daybreak").TotalSeconds < 2 && HolyPower < 3, "Holy Shock (Holy power generation)")

				);
			}
		}

		public override Composite Medic
		{
			get {
				return new Decorator(
					ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
					new PrioritySelector(
						Buff.CastBuff("Hand of Freedom", ret => Me.MovementInfo.ForwardSpeed < 8.05, "Hand of Freedom"),
						Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 40, "Healthstone")
					));
			}
		}

		public override Composite PreCombat
		{
			get {
				return new Decorator(
					ret => !Me.Mounted && !Me.Dead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
					new PrioritySelector(
						// Beacon of Light on tank
						Healer.FindTank(a => true, x => !x.ToUnit().Dead && x.ToUnit().InLineOfSight && !x.ToUnit().HasMyAura("Beacon of Light") && x.Beacon, (a, b) => (int)(a.MaxHealth - b.MaxHealth), "Beacon of Light on tank",
						                Buff.CastTargetBuff("Beacon of Light", a => true, "Beacon of Light")
						               ),
						Buff.CastBuff("Seal of Insight", ret => true, "Seal of Insight"),
						Buff.CastRaidBuff("Blessing of Kings", ret => !Buff.PlayerHasBuff("Mark of the Wild"), "[Blessing] of Kings"),
						Buff.CastRaidBuff("Blessing of Might", ret => Buff.PlayerHasBuff("Mark of the Wild"), "[Blessing] of Might")));
			}
		}

		public override Composite Resting
		{
			get { return this.SingleRotation; }
		}

		public override Composite PVPRotation
		{
			get { return this.SingleRotation; }
		}

		public override Composite PVERotation
		{
			get { return this.SingleRotation; }
		}
	}
}
