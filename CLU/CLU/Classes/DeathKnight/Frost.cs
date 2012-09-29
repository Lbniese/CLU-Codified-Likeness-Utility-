#region Revision info
/*
 * $Author$
 * $Date$
 * $ID$
 * $Revision$
 * $URL$
 * $LastChangedBy$
 * $ChangesMade$
 */
#endregion

using System.Linq;
using CLU.Helpers;
using CLU.Lists;
using CLU.Settings;
using CommonBehaviors.Actions;
using Styx;
using Styx.TreeSharp;
using CLU.Base;
using Rest = CLU.Base.Rest;
using Styx.CommonBot;
using global::CLU.Managers;

namespace CLU.Classes.DeathKnight
{
    class Frost : RotationBase
    {
        public override string Name
        {
            get {
                return "Frost Deathknight";
            }
        }

        public override float CombatMaxDistance
        {
            get {
                return 3.2f;
            }
        }

        public override string Help
        {
            get {
                return "\n" +
                        "----------------------------------------------------------------------\n" +
                        "This Rotation will:\n" +
                        "1. Heal using AMS, IBF, Healthstone, Deathstrike\n" +
                        "2. AutomaticCooldowns has:\n" +
                            "==> UseTrinkets\n" + 
                            "==> UseRacials\n" +
                            "==> UseEngineerGloves\n" +
                            "==> Pillar of Frost & Raise Dead & Death and Decay & Empower Rune Weapon\n" + 
                        "NOTE: PvP rotations have been implemented in the most basic form, once MoP is released I will go back & revise the rotations for optimal functionality 'Dagradt'.\n" +
                        "[*] Handles Killing Machine differently; Dual Wield (Frost Strike); 2Handed (Obliterate)\n" +
                        "[*] Unholy runes are gamed to force RE procs or Blood Tap or Plague Leech on blood/frost\n" +
                        "[*] AutomaticCooldowns now works with Boss's or Mob's (See: General Setting)\n" +
                        "[*] Death Siphon (only if movement enabled.)\n" +
                        "Credits to Weischbier, because he owns the buisness and I want him to have my babys! -- Sincerely Wulf (Bahahaha :P)\n" +
                        "----------------------------------------------------------------------\n";
            }
        }

        public override string Revision
        {
            get
            {
                return "$Rev$";
            }
        }

        public override string KeySpell
        {
            get {
                return "Howling Blast";
            }
        }

        public override int KeySpellId
        {
            get { return 49184; }
        }

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                           // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),
                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),
                           // Items
                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Racials.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),
                           
                           //Interrupts
                           Spell.CastInterupt("Mind Freeze", 		ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange, "Mind Freeze"), //Why does nobody check for the range of melee kicks? // CurrentTarget Null check, we are accessing the objects property ;) --wulf
                           Spell.CastInterupt("Strangulate", 		ret => true, "Strangulate"),
                           Spell.CastInterupt("Asphyxiate", 		ret => true, "Asphyxiate"),// Replaces Strangulate -- Darth Vader like ability
                           //Diseases
                           Common.ApplyDiseases(ret => Me.CurrentTarget),
                           //Cooldowns
                           new Decorator(ret => Unit.IsTargetWorthy(Me.CurrentTarget) && Me.IsWithinMeleeRange,//Check for the damn range, we don't want to pop anything when the destination is shit away
                                         new PrioritySelector(
                                             Buff.CastBuff("Raise Dead", 				ret => Me.CurrentTarget != null && Buff.PlayerHasBuff("Pillar of Frost") && Buff.PlayerBuffTimeLeft("Pillar of Frost") <= 10 && Buff.PlayerHasBuff("Unholy Strength"), "Raise Dead"),
                                             Buff.CastBuff("Pillar of Frost", 			ret => Me.CurrentTarget != null, "Pillar of Frost"),
                                             Spell.CastSelfSpell("Empower Rune Weapon", ret => Me.CurrentTarget != null && CLUSettings.Instance.DeathKnight.UseEmpowerRuneWeapon && StyxWoW.Me.FrostRuneCount < 1 && StyxWoW.Me.UnholyRuneCount < 2 && StyxWoW.Me.DeathRuneCount < 1 && !Buff.UnitHasHasteBuff(Me), "Empower Rune Weapon")
                                         )
                                        ),
                           //Aoe
                           new Decorator(ret => CLUSettings.Instance.UseAoEAbilities && Unit.EnemyUnits.Count() >= 3,
                               new PrioritySelector(
                                    Spell.CastSpell("Howling Blast",ret => (Me.FrostRuneCount >= 1 || Me.DeathRuneCount >= 1),"Howling Blast [Aoe]"),
                                    Spell.CastAreaSpell("Death and Decay", 10, true, 3, 0.0, 0.0, 	ret => Me.CurrentTarget != null && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.UnholyRuneCount == 2 && Unit.EnemyUnits.Count() >= 3 && !Me.IsMoving && !Me.CurrentTarget.IsMoving && Unit.IsTargetWorthy(Me.CurrentTarget), "Death and Decay"),
                                    Spell.CastAreaSpell("Death and Decay", 10, true, 3, 0.0, 0.0, 	ret => Me.CurrentTarget != null && (!BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && (Spell.RuneCooldown(4) == 0 && Spell.RuneCooldown(3) <= 1) || (Spell.RuneCooldown(3) == 0 && Spell.RuneCooldown(4) <= 1) && Unit.EnemyUnits.Count() >= 3 && !Me.IsMoving && !Me.CurrentTarget.IsMoving && Unit.IsTargetWorthy(Me.CurrentTarget)), "Death and Decay"),
                                    Common.SpreadDiseasesBehavior(ret => Me.CurrentTarget), // Used to spread your Diseases based upon your Tier one Talent. -- wulf
                                    Spell.CastSpell("Plague Strike", ret => Spell.SpellCooldown("Death and Decay").TotalSeconds > 6 && Me.UnholyRuneCount == 2 && Unit.EnemyUnits.Count() >= 3, "Plague Strike [Aoe]")
                                   )
                               ),
                           //Operation: Do Damage[Eyes only]
                           new Decorator(ret => Common.IsWieldingTwoHandedWeapon(),
                               new PrioritySelector(
                                    Spell.CastSpell("Soul Reaper", ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent < 35, "Soul Reaping"),
                                    Spell.CastSpell("Howling Blast", ret => Buff.PlayerHasBuff("Freezing Fog"), "Howling Blast (Rime)"),
                                    Spell.CastSpell("Obliterate", ret => Me.CurrentTarget, ret => Buff.PlayerHasBuff("Killing Machine"), "Obliterate (2 Hand Killing Machine)"),
                                    Spell.CastSpell("Obliterate", ret => Me.CurrentTarget, ret => Me.CurrentRunicPower < 85, "Obliterate (Utilize Runes)"),
                                    Spell.CastSpell("Blood Strike", ret => Me.CurrentTarget, ret => Me.RunicPowerPercent >= 90, "Frost Strike (Dumping Runic Power) => BS"),
                                    Spell.CastSpell("Blood Strike", ret => Me.CurrentTarget, ret => true, "Frost Strike (Because we can) => BS"),
                                    Spell.CastSpell("Blood Tap", ret => Me.CurrentTarget, ret =>  Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap (Refreshed a depleted Rune)"), //Don't waste it on Unholy Runes
                                    Buff.CastBuff("Horn of Winter", ret => CLUSettings.Instance.DeathKnight.UseHornofWinter, "Horn of Winter (Because we can)")
                                   )
                               ),
                           //Operation: Do Damage[Eyes only]
                           new Decorator(ret => !Common.IsWieldingTwoHandedWeapon(),
                               new PrioritySelector(
                                    Spell.CastSpell("Soul Reaper", ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent < 35, "Soul Reaping"),
                                    Spell.CastSpell("Howling Blast", ret => Buff.PlayerHasBuff("Freezing Fog"), "Howling Blast (Rime)"),
                                    Spell.CastSpell("Blood Strike", ret => Me.CurrentTarget, ret => Buff.PlayerHasBuff("Killing Machine"), "Frost Strike (Dual Wield Killing Machine)"),
                                    Spell.CastSpell("Blood Strike", ret => Me.CurrentTarget, ret => Me.RunicPowerPercent > 84, "Frost Strike (Dumping Runic Power)"),
                                    Spell.CastSpell("Blood Tap", ret => Me.CurrentTarget, ret =>  Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap (Refreshed a depleted Rune)"), //Don't waste it on Unholy Runes
                                    Spell.CastSpell("Obliterate", ret => (Me.FrostRuneCount == 2 && Me.UnholyRuneCount == 2) || Me.DeathRuneCount == 2, "Obliterate (Utilizing Killing Machine)"),
                                    Spell.CastSpell("Obliterate", ret => Buff.PlayerHasBuff("Killing Machine") && Me.CurrentRunicPower <= 10, "Obliterate (Utilizing Killing Machine)"),
                                    Spell.CastSpell("Howling Blast", ret => true, "Howling Blast (Because we can)"),
                                    Spell.CastSpell("Blood Tap", ret => Me.CurrentTarget, ret =>  Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap (Refreshed a depleted Rune)"),  //Don't waste it on Unholy Runes
                                    Spell.CastSpell("Blood Strike", ret => true, "Frost Strike (Because we can)"),
                                    Spell.CastSpell("Horn of Winter", ret => Me, ret => CLUSettings.Instance.DeathKnight.UseHornofWinter, "Horn of Winter for RP")
                                   )
                               )
                       );
            }
        }

        public Composite burstRotation
        {
            get
            {
                return (
                    new PrioritySelector(
                ));
            }
        }

        public Composite baseRotation
        {
            get
            {
                return (
                    new PrioritySelector(
                        //new Action(a => { CLU.Log("I am the start of public Composite baseRotation"); return RunStatus.Failure; }),
                        //PvP Utilities
                        Spell.CastSpell("Chains of Ice",                        ret => Me.CurrentTarget != null && !Me.CurrentTarget.IsWithinMeleeRange && Me.CurrentTarget.DistanceSqr <= 30 * 30 && !Buff.TargetHasDebuff("Chains of Ice"), "Chains of Ice"),
                        Spell.CastSpell("Death Grip",                           ret => Me.CurrentTarget != null && !Me.CurrentTarget.IsWithinMeleeRange && Me.CurrentTarget.DistanceSqr <= 30 * 30 && !Buff.TargetHasDebuff("Chains of Ice") && !SpellManager.CanCast("Chains of Ice"), "Death Grip"),

                        //Rotation
                        Racials.UseRacials(),
                        //mogu_power_potion,if=target.time_to_die<=60&buff.pillar_of_frost.up
                        Item.UseEngineerGloves(),//~> use_item,name=gauntlets_of_the_lost_catacomb,if=(frost>=1|death>=1)
                        Buff.CastBuff("Pillar of Frost",                        ret => Me.CurrentTarget != null && Me.IsWithinMeleeRange, "Pillar of Frost"),
                        //raise_dead
                        Spell.CastSpell("Outbreak",                             ret => Buff.TargetDebuffTimeLeft("Frost Fever").Seconds < 3 || Buff.TargetDebuffTimeLeft("Blood Plague").Seconds < 3, "Outbreak"),
                        Spell.CastSpell("Soul Reaper",                          ret => Me.CurrentTarget.HealthPercent <= 35, "Soul Reaping"),//~> soul_reaper,if=target.health.pct<=35|((target.health.pct-3*(target.health.pct%target.time_to_die))<=35)
                        Spell.CastSelfSpell("Unholy Blight",                    ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr <= 10 * 10 && TalentManager.HasTalent(3) && (Buff.TargetDebuffTimeLeft("Frost Fever").Seconds < 3 || Buff.TargetDebuffTimeLeft("Blood Plague").Seconds < 3), "Unholy Blight"),
                        Spell.CastSpell("Howling Blast",                        ret => !Buff.TargetHasDebuff("Frost Fever"), "Howling Blast"),
                        Spell.CastSpell("Plague Strike",                        ret => !Buff.TargetHasDebuff("Blood Plague"), "Plague Strike"),

                        //2H
                        new Decorator(ret => Common.IsWieldingTwoHandedWeapon(),
                            new PrioritySelector(
                                Spell.CastSpell("Plague Leech",                 ret => TalentManager.HasTalent(2) && ((SpellManager.Spells["Outbreak"].CooldownTimeLeft.Seconds < 1) || (Buff.PlayerHasBuff("Freezing Fog") && Buff.TargetDebuffTimeLeft("Blood Plague").Seconds < 3 && (Me.UnholyRuneCount >= 1 || Me.DeathRuneCount >= 1))), "Plague Leech"),
                                Spell.CastSpell("Necrotic Strike",              ret => !Macro.rotationSwap, "Necrotic Strike"),
                                Spell.CastSpell("Howling Blast",                ret => Buff.PlayerHasBuff("Freezing Fog"), "Howling Blast"),
                                Spell.CastSpell("Obliterate",                   ret => Macro.rotationSwap && Me.CurrentRunicPower <= 76, "Obliterate"),
                                Spell.CastSpell("Obliterate",                   ret => !Macro.rotationSwap && Me.CurrentRunicPower <= 76 && Me.FrostRuneCount >= 1 && Me.UnholyRuneCount >= 1, "Obliterate"),
                                //empower_rune_weapon,if=target.time_to_die<=60&buff.mogu_power_potion.up
                                Spell.CastSpell("Blood Strike",                 ret => !Buff.PlayerHasBuff("Killing Machine"), "Frost Strike"),
                                Spell.CastSpell("Obliterate",                   ret => Macro.rotationSwap && Buff.PlayerHasBuff("Killing Machine"), "Obliterate"),
                                Spell.CastSpell("Obliterate",                   ret => !Macro.rotationSwap && Buff.PlayerHasBuff("Killing Machine") && Me.FrostRuneCount >= 1 && Me.UnholyRuneCount >= 1, "Obliterate"),
                                Spell.CastSpell("Blood Tap",                    ret => TalentManager.HasTalent(13) && Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap"),
                                Spell.CastSpell("Blood Strike",                 ret => true, "Frost Strike"),
                                Buff.CastRaidBuff("Horn of Winter", ret => CLUSettings.Instance.DeathKnight.UseHornofWinter, "Horn of Winter"),
                                Spell.CastSpell("Empower Rune Weapon",          ret => true, "Empower Rune Weapon"))),

                        //DW
                        new Decorator(ret => !Common.IsWieldingTwoHandedWeapon(),
                            new PrioritySelector(
                                Spell.CastSpell("Plague Leech",                 ret => TalentManager.HasTalent(2) && !((Buff.PlayerHasBuff("Killing Machine") && Me.CurrentRunicPower < 10) || (Me.UnholyRuneCount == 2 || Me.FrostRuneCount == 2 || Me.DeathRuneCount == 2)), "Plague Leech"),
                                Spell.CastSpell("Necrotic Strike",              ret => !Macro.rotationSwap, "Necrotic Strike"),
                                Spell.CastSpell("Howling Blast",                ret => Buff.PlayerHasBuff("Freezing Fog"), "Howling Blast"),
                                Spell.CastSpell("Blood Strike",                 ret => Me.CurrentRunicPower >= 88, "Frost Strike"),
                                //empower_rune_weapon,if=target.time_to_die<=60&buff.mogu_power_potion.up
                                Spell.CastSpell("Blood Strike",                 ret => Buff.PlayerHasBuff("Killing Machine"), "Frost Strike"),
                                Spell.CastSpell("Obliterate",                   ret => Macro.rotationSwap && Buff.PlayerHasBuff("Killing Machine") && Me.CurrentRunicPower < 10, "Obliterate"),
                                Spell.CastSpell("Obliterate",                   ret => !Macro.rotationSwap && Buff.PlayerHasBuff("Killing Machine") && Me.CurrentRunicPower < 10 && Me.FrostRuneCount >= 1 && Me.UnholyRuneCount >= 1, "Obliterate"),
                                Spell.CastSpell("Obliterate",                   ret => Macro.rotationSwap && (Me.UnholyRuneCount == 2 || Me.FrostRuneCount == 2 || Me.DeathRuneCount == 2), "Obliterate"),
                                Spell.CastSpell("Obliterate",                   ret => !Macro.rotationSwap && (Me.UnholyRuneCount == 2 || Me.FrostRuneCount == 2), "Obliterate"),
                                Spell.CastSpell("Howling Blast",                ret => true, "Howling Blast"),
                                Spell.CastSpell("Blood Strike",                 ret => true, "Frost Strike"),
                                Spell.CastOnUnitLocation("Death and Decay",     ret => Me.CurrentTarget, ret => true, "Death and Decay"),
                                Spell.CastSpell("Plague Strike",                ret => true, "Plague Strike"),
                                Spell.CastSpell("Blood Tap",                    ret => TalentManager.HasTalent(13) && Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap"),
                                Buff.CastRaidBuff("Horn of Winter", ret => CLUSettings.Instance.DeathKnight.UseHornofWinter, "Horn of Winter"),
                                Spell.CastSpell("Empower Rune Weapon",          ret => true, "Empower Rune Weapon")))
                ));
            }
        }

        public override Composite Medic
        {
            get {
                return
                    new PrioritySelector(
                        Buff.CastBuff("Anti-Magic Shell", ret => Me.CurrentTarget != null && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.DeathKnight.UseAntiMagicShell && (Me.CurrentTarget.IsCasting || Me.CurrentTarget.ChanneledCastingSpellId != 0), "AMS"), // TODO: Put this back in when its fixed. && Me.CurrentTarget.IsTargetingMeOrPet
                        new Decorator(
                            ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                            new PrioritySelector(
                                Spell.CastSelfSpell("Death Pact",			ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.FrostPetSacrificePercent && Me.Minions.FirstOrDefault(q => q.CreatureType == WoWCreatureType.Undead || q.CreatureType == WoWCreatureType.Totem) != null && CLUSettings.Instance.DeathKnight.FrostUsePetSacrifice, "Death Pact"),
                                Spell.CastSelfSpell("Raise Dead",			ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.FrostPetSacrificePercent && !Buff.PlayerHasBuff("Icebound Fortitude") && CLUSettings.Instance.DeathKnight.FrostUsePetSacrifice, "Raise Dead"),
                                Spell.CastSelfSpell("Icebound Fortitude",	ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.FrostIceboundFortitudePercent && CLUSettings.Instance.DeathKnight.UseIceboundFortitude, "Icebound Fortitude "),
                                Spell.CastSpell("Death Strike",				ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathStrikeEmergencyPercent, "Death Strike"),
                                Spell.CastSpell("Death Siphon",             ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathSiphonPercent && CLUSettings.Instance.EnableMovement, "Death Siphon"), // Only if movement is enabled..i.e. we are questing, gathering, etc...
                                // Conversion lasts until canceled!!..I am reluctant to put this in atm as its so underpowered
                                // Buff.CastBuff("Conversion",                 ret => (Buff.PlayerHasBuff("Unholy Presence") && Buff.PlayerHasBuff("Anti-Magic Shell")/*Since conversion stops extra rune regen from Frost Presence but not from AMS we will go this way only for Unholy Dks*/), "Conversion (Restoring 3% HP every 1s for 10RP"),//Tricky One
                                Item.UseBagItem("Healthstone",				ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.HealthstonePercent, "Healthstone"))
                        )
                    );
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return (
                    new Decorator(ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                        new PrioritySelector(
                            //flask,type=winters_bite
                            //food,type=black_pepper_ribs_and_shrimp
                            Buff.CastBuff("Frost Presence",         ret => !Me.HasMyAura("Frost Presence"), "Frost Presence"),
                            Buff.CastRaidBuff("Horn of Winter",     ret => CLUSettings.Instance.DeathKnight.UseHornofWinter && Me.CurrentTarget != null && !Me.CurrentTarget.IsFriendly, "Horn of Winter"),
                            //army_of_the_dead
                            //mogu_power_potion
                            Spell.CastSpell("Chains of Ice",        ret => Me.CurrentTarget != null && Macro.Manual && (CLU.LocationContext == GroupLogic.Battleground || Unit.IsTrainingDummy(Me.CurrentTarget)) && !Me.CurrentTarget.IsWithinMeleeRange && Me.CurrentTarget.DistanceSqr <= 30 * 30 && !Buff.TargetHasDebuff("Chains of Ice"), "Chains of Ice"),
                            Spell.CastSpell("Death Grip",           ret => Me.CurrentTarget != null && Macro.Manual && (CLU.LocationContext == GroupLogic.Battleground || Unit.IsTrainingDummy(Me.CurrentTarget)) && !Me.CurrentTarget.IsWithinMeleeRange && Me.CurrentTarget.DistanceSqr <= 30 * 30 && !Buff.TargetHasDebuff("Chains of Ice") && !SpellManager.CanCast("Chains of Ice"), "Death Grip"),
                            new Action(delegate
                            {
                                Macro.isMultiCastMacroInUse();
                                return RunStatus.Failure;
                            })
                )));
            }
        }

        public override Composite Resting
        {
            get {
                return Rest.CreateDefaultRestBehaviour();
            }
        }

        public override Composite PVPRotation
        {
            get
            {
                return (
                    new PrioritySelector(
                        //new Action(a => { CLU.Log("I am the start of public override Composite PVPRotation"); return RunStatus.Failure; }),
                        CrowdControl.freeMe(),
                        new Decorator(ret => Macro.Manual || BotChecker.BotBaseInUse("BGBuddy"),
                            new Decorator(ret => StyxWoW.Me.CurrentTarget != null && Unit.IsTargetWorthy(StyxWoW.Me.CurrentTarget),
                                new PrioritySelector(
                                    Item.UseTrinkets(),
                                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                    new Action(delegate
                                    {
                                        Macro.isMultiCastMacroInUse();
                                        return RunStatus.Failure;
                                    }),
                                    new Decorator(ret => Macro.Burst, burstRotation),
                                    new Decorator(ret => !Macro.Burst || BotChecker.BotBaseInUse("BGBuddy"), baseRotation)))
                )));
            }
        }

        public override Composite PVERotation
        {
            get {
                return this.SingleRotation;
            }
        }
    }
}