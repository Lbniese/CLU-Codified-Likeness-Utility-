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

using CLU.Helpers;
using CommonBehaviors.Actions;
using Styx.TreeSharp;
using CLU.Settings;
using CLU.Base;
using Rest = CLU.Base.Rest;


namespace CLU.Classes.Hunter
{
    using global::CLU.Managers;
    using Styx.CommonBot;

    class BeastMastery : RotationBase
    {
        public override string Name
        {
            get {
                return "BeastMastery Hunter";
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
                return "Kill Command";
            }
        }

        public override int KeySpellId
        {
            get { return 34026; }
        }

        public override float CombatMinDistance
        {
            get {
                return 40f;
            }
        }

        public override string Help
        {
            get {
                return
                    @"
----------------------------------------------------------------------
BeastMastery:
[*] Aspect of the Fox/Aspect of the Hawk/Iron Hawk Switching while moving.
[*] Misdirection on best target (tank or pet or focus) when movement enabled.
[*] AutomaticCooldowns now works with Boss's or Mob's (See: General Setting)
[*] Tranquilizing Shot on Enrage.
[*] Level 90 talents supported. Glaive Toss, Barrage, Powershot
[*] Always Camouflage option (see UI setting)
This Rotation will:
1. Heal using Exhilaration, Healthstone, Deterrence.
2. Trap Launcher or Feign Death will halt the rotation
3. AoE with Multishot, Barrage, Explosive Trap, Powershot, Glaive Toss
4. AutomaticCooldowns has:
    ==> UseTrinkets 
    ==> UseRacials 
    ==> UseEngineerGloves
    ==> Rapid Fire, Dire Beast, Bestial Wrath, Fervor, A Murder of Crows
    ==> Readiness, Blink Strike, Stampede, Lynx Rush
5. Will use Heart of the Phoenix, Mend Pet, Call Pet, Revive Pet.
Recommended Pets:
===================
Fox		        -haste
Cat		        +str/agil
Core Hound	    (+haste &-cast speed) exotic
Silithid		(+health) exotic
Wolf		    (+crit)
Shale Spider	(+5% stats) exotic
Raptor		    (-armor)
Carrion Bird	(-physical damage)
Sporebat	    (-cast speed)
Ravager		    (-Phys Armor)
Wind Serpent	(-Spell Armor)
NOTE: PvP rotations have been implemented in the most basic form, once MoP is released I will go back & revise the rotations for optimal functionality 'Dagradt'.
----------------------------------------------------------------------";
            }
        }

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                           // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),

                           //Spell.CastSelfSpell("Call Pet 1", ret => true, "Call Pet 1"),

                           // Camouflage
                           Buff.CastBuff("Camouflage", ret => CLUSettings.Instance.Hunter.EnableAlwaysCamouflage, "Camouflage"),

                           // Trinkets & Cooldowns
                           new Decorator(
                               ret => Me.CurrentTarget != null && (Unit.IsTargetWorthy(Me.CurrentTarget) && !Buff.PlayerHasBuff("Feign Death")),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Racials.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),

                           // Main Rotation
                           new Decorator(
                               ret => !Buff.PlayerHasBuff("Feign Death"),
                               new PrioritySelector(
                                   // HandleMovement? Lets Misdirect to Focus, Pet, RafLeader or Tank
                                   // TODO: Add Binding shot logic..need to see it working well.
                                   Common.HandleMisdirection(),
                                   Buff.CastDebuff("Hunter's Mark",           ret => !TalentManager.HasGlyph("Marked for Death"), "Hunter's Mark"),
                                   Spell.CastSelfSpell("Feign Death",         ret => Me.CurrentTarget != null && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && CLUSettings.Instance.Hunter.UseFeignDeath, "Feign Death Threat"),
                                   Spell.CastSpell("Concussive Shot",         ret => Me.CurrentTarget != null && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && CLUSettings.Instance.Hunter.UseConcussiveShot, "Concussive Shot"),
                                   Spell.CastSpell("Tranquilizing Shot",      ret => Buff.TargetHasBuff("Enrage") && CLUSettings.Instance.Hunter.UseTranquilizingShot, "Tranquilizing Shot"),
                                   Common.HandleAspectSwitching(2),
                                   // Main rotation
                                   Spell.CastSpell("Kill Shot",               ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent < 20, "Kill Shot"),
                                   Buff.CastBuff("Focus Fire",                ret => Buff.PlayerHasBuff("Frenzy"), "Focus Fire"),
                                   Buff.CastDebuff("Serpent Sting",           ret => Me.CurrentTarget != null && Buff.TargetDebuffTimeLeft("Serpent Sting").TotalSeconds <= 0.5 && Unit.TimeToDeath(Me.CurrentTarget) > 10, "Serpent Sting"),
                                   Buff.CastBuff("Fervor",                    ret => Me.CurrentTarget != null && Me.FocusPercent <= CLUSettings.Instance.Hunter.BmFevorFocusPercent && Unit.IsTargetWorthy(Me.CurrentTarget), "Fervor"),
                                   Buff.CastBuff("Bestial Wrath", ret => Me.CurrentTarget != null && Me.FocusPercent > CLUSettings.Instance.Hunter.BestialWrathFocusPercent && Me.Pet.Location.DistanceSqr(Me.CurrentTarget.Location) < Spell.MeleeRange * Spell.MeleeRange, "Bestial Wrath"),
                                   Spell.CastSpell("Multi-Shot",              ret => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 20) >= CLUSettings.Instance.Hunter.BmMultiShotCount, "Multi-Shot"),
                                   //Spell.CastSpell("Steady Shot",              ret => Unit.CountEnnemiesInRange(Me.Location, 30) >= CLUSettings.Instance.Hunter.BmMultiShotCount, "Cobra Shot"),
                                   Spell.HunterTrapBehavior("Explosive Trap", ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && !Lists.BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 10) >= CLUSettings.Instance.Hunter.ExplosiveTrapCount),
                                   Buff.CastBuff("A Murder of Crows",         ret => Unit.IsTargetWorthy(Me.CurrentTarget), "A Murder of Crows"), //reduced to 60sec cooldown if under 20%
                                   Buff.CastBuff("Stampede",                  ret => Unit.IsTargetWorthy(Me.CurrentTarget), "Stampede"),
                                   // Common level 90 ability's
                                   Spell.CastSpell("Glaive Toss",             ret => !Buff.TargetHasDebuff("Glaive Toss") && Unit.CountEnnemiesInRange(Me.Location, 20) > CLUSettings.Instance.Hunter.GlaiveTossCount, "Glaive Toss"), //instant..with no apparent cooldown...needs checking.
                                   Spell.ChannelSpell("Barrage",              ret => Unit.CountEnnemiesInRange(Me.Location, 20) > CLUSettings.Instance.Hunter.BarrageCount, "Barrage"), //AoE? needs testing.
                                   Spell.ChannelSpell("Powershot",            ret => Unit.CountEnnemiesInRange(Me.Location, 20) > CLUSettings.Instance.Hunter.PowershotCount, "Powershot"), //AoE? needs testing also has knockback!
                                   // End Common level 90 ability's
                                   Spell.CastSpell("Blink Strike",            ret => Me.CurrentTarget != null && Me.Pet.Location.DistanceSqr(Me.CurrentTarget.Location) > 10 * 10 && Me.GotAlivePet, "Blink Strike"), // teleports behind target mad damage.
                                   Spell.CastSpell("Lynx Rush", ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.Pet.Location.DistanceSqr(Me.CurrentTarget.Location) < 10 * 10, "Lynx Rush"),
                                   Buff.CastBuff("Rapid Fire",                ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && !Buff.PlayerHasBuff("The Beast Within") && !Buff.UnitHasHasteBuff(Me), "Rapid Fire"),
                                   Spell.CastSpell("Kill Command", ret => Me.CurrentTarget != null && Me.GotAlivePet && Me.Pet.Location.DistanceSqr(Me.CurrentTarget.Location) < 25 * 25 && Spell.SpellCooldown("Kill Command").TotalSeconds < 1 && Me.FocusPercent >= 40, "Kill Command"),
                                   Spell.CastSpell("Dire Beast", ret => Me.CurrentTarget != null && Me.Pet.Location.DistanceSqr(Me.CurrentTarget.Location) < 10 * 10 && Me.FocusPercent <= 90, "Dire Beast"),
                                   Spell.CastSpell("Arcane Shot",             ret => Buff.PlayerHasActiveBuff("Thrill of the Hunt"), "Arcane Shot (Thrill of the Hunt)"),
                                   Buff.CastBuff("Readiness",                 ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Buff.PlayerHasActiveBuff("Rapid Fire"), "Readiness"),
                                   Spell.CastSpell("Arcane Shot",             ret => (Me.FocusPercent >= CLUSettings.Instance.Hunter.BmArcaneShotFocusPercent || Buff.PlayerHasBuff("The Beast Within")), "Arcane Shot"),
                                   Buff.CastBuff("Focus Fire",                ret => Buff.PlayerHasBuff("Frenzy") && !Buff.PlayerHasBuff("The Beast Within"), "Focus Fire"),
                                   Spell.CastSpell("Steady Shot",              ret => true, "Cobra Shot"))));
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
                        Spell.CastSelfSpell("Feign Death",             ret => Me.HealthPercent <= 25, "Feign Death"),
                        Spell.CastSelfSpell("Deterrence",              ret => Me.HealthPercent <= 75, "Deterrence"),
                        Spell.CastSelfSpell("Disengage",               ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange, "Disengage"),
                        Spell.CastSpell("Concussive Shot",             ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr <= 26 * 26, "Concussive Shot"),
                        Spell.CastSpell("Widow Venom",                 ret => !Buff.TargetHasDebuff("Widow Venom"), "Widow Venom"),
                        Spell.CastSpell("Tranquilizing Shot",          ret => Buff.TargetHasBuff("Enrage"), "Tranquilizing Shot"),
                        Buff.CastBuff("Mend Pet",                      ret => Me.Pet != null && Me.Pet.DistanceSqr <= 45 * 45 && Me.Pet.HealthPercent <= 90 && !Buff.TargetHasBuff("MendPet", Me.Pet), "Mend Pet"),
                        Buff.CastBuff("Camouflage",                    ret => true, "Camouflage"),

                        //Rotation
                        //virmens_bite_potion,if=buff.bloodlust.react|target.time_to_die<=60
                        
                        // Dagradt: Test this just give it a time
                        Common.HandleAspectSwitching(0),

                        Spell.HunterTrapBehavior("Explosive Trap",     ret => Me.CurrentTarget, ret => Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 10) > 0),
                        Buff.CastBuff("Focus Fire",                    ret => Buff.PlayerHasActiveBuff("Frenzy"), "Focus Fire"), //Me.ActiveAuras["Frenzy"].StackCount == 5 << you cannot attempt to access a key if it dosnt exist it will spam nullreferences.!!! --wulf
                        Spell.CastSpell("Serpent Sting",               ret => !Buff.TargetHasDebuff("Serpent Sting"), "Serpent Sting"),
                        Racials.UseRacials(),
                        Buff.CastBuff("Fervor",                        ret => TalentManager.HasTalent(10) && !Buff.PlayerHasActiveBuff("Fervor") && Me.CurrentFocus <= 65, "Fervor"),
                        Buff.CastBuff("Bestial Wrath",                 ret => Me.CurrentTarget != null && Me.Pet.Location.DistanceSqr(Me.CurrentTarget.Location) <= Spell.MeleeRange * Spell.MeleeRange && Me.GotAlivePet && Me.CurrentFocus > 60 && !Buff.PlayerHasActiveBuff("Beast Within"), "Bestial Wrath"),
                        Spell.CastSpell("Multi Shot",                  ret => Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 8) > 5, "Multi Shot"),
                        Spell.CastSpell("Steady Shot",                 ret => Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 0) > 5, "Cobra Shot"),
                        Buff.CastBuff("Rapid Fire",                    ret => !Buff.PlayerHasActiveBuff("Rapid Fire"), "Rapid Fire"),
                        Spell.CastSpell("Stampede",                    ret => true, "Stampede"),
                        Spell.CastSpell("Kill Shot",                   ret => true, "Kill Shot"),
                        Spell.CastSpell("Kill Command",                ret => Me.CurrentTarget != null && Me.Pet.Location.DistanceSqr(Me.CurrentTarget.Location) <= 25 * 25 && Me.GotAlivePet, "Kill Command"),
                        Spell.CastSpell("A Murder of Crows",           ret => TalentManager.HasTalent(13) && !Buff.TargetHasDebuff("A Murder of Crows"), "A Murder of Crows"),
                        Spell.CastSpell("Glaive Toss",                 ret => TalentManager.HasTalent(16), "Glaive Toss"),
                        Spell.CastSpell("Lynx Rush",                   ret => Me.CurrentTarget != null && Me.Pet.Location.DistanceSqr(Me.CurrentTarget.Location) <= 10 * 10 && Me.GotAlivePet && TalentManager.HasTalent(15) && !SpellManager.Spells["Lynx Rush"].Cooldown, "Lynx Rush"),
                        Spell.CastSpell("Dire Beast",                  ret => TalentManager.HasTalent(11) && Me.CurrentFocus <= 90, "Dire Beast"),
                        Spell.CastSpell("Barrage",                     ret => TalentManager.HasTalent(18), "Barage"),
                        Spell.CastSpell("Powershot",                   ret => TalentManager.HasTalent(17), "Powershot"),
                        Spell.CastSpell("Blink Strike",                ret => Me.CurrentTarget != null && Me.Pet.Location.DistanceSqr(Me.CurrentTarget.Location) <= 40 * 40 && Me.GotAlivePet && TalentManager.HasTalent(14), "Blink Strike"),
                        Spell.CastSelfSpell("Readiness",               ret => Buff.PlayerHasActiveBuff("Rapid Fire"), "Readiness"),
                        Spell.CastSpell("Arcane Shot",                 ret => Buff.PlayerHasActiveBuff("Thrill of the Hunt"), "Arcane Shot"),
                        Buff.CastBuff("Focus Fire",                    ret => Buff.PlayerHasActiveBuff("Frenzy") && !Buff.PlayerHasActiveBuff("Focus Fire") && !Buff.PlayerHasActiveBuff("Beast Within"), "Focus Fire"), //Me.ActiveAuras["Frenzy"].StackCount == 5 << you cannot attempt to access a key if it dosnt exist it will spam nullreferences.!!! --wulf
                        Spell.CastSpell("Steady Shot",                 ret => Buff.TargetDebuffTimeLeft("Serpent Sting").Seconds < 6, "Cobra Shot"),
                        Spell.CastSpell("Arcane Shot",                 ret => Me.CurrentFocus >= 61 || Buff.PlayerHasActiveBuff("Beast Within"), "Arcane Shot"),
                        Spell.CastSpell("Steady Shot",                 ret => true, "Cobra Shot")
                ));
            }
        }

        public override Composite Pull
        {
             get { return new PrioritySelector(); }
        }

        public override Composite Medic
        {
            get {
                return new PrioritySelector(
                           // Make sure we go our pet.
                           Common.HunterCallPetBehavior(CLUSettings.Instance.Hunter.ReviveInCombat),
                           new Decorator(
                               ret => Me.HealthPercent < 100 && !Buff.PlayerHasBuff("Feign Death") && CLUSettings.Instance.EnableSelfHealing,
                               new PrioritySelector(
                                   Spell.CastSelfSpell("Exhilaration",            ret => Me.HealthPercent < CLUSettings.Instance.Hunter.ExhilarationPercent, "Exhilaration"),
                                   Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < CLUSettings.Instance.Hunter.HealthstonePercent, "Healthstone"),
                                   Spell.CastSelfSpell("Deterrence",              ret => Me.HealthPercent < CLUSettings.Instance.Hunter.DeterrencePercent && Me.HealthPercent > 1, "Deterrence"))),
                           // Heart of the Phoenix, Mend Pet, etc
                           Common.HandlePetHelpers());
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return (
                    new Decorator(ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink") && !Buff.PlayerHasBuff("Feign Death"),
                        new PrioritySelector(
                            //flask,type=spring_blossoms
                            //food,type=sea_mist_rice_noodles
                            Buff.CastDebuff("Hunter's Mark", ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) >= 21 && (!TalentManager.HasGlyph("Marked for Death") || CLU.LocationContext == GroupLogic.Battleground && Me.CurrentTarget.DistanceSqr > 40 * 40), "Hunter's Mark"),
                            Common.HunterCallPetBehavior(CLUSettings.Instance.Hunter.ReviveInCombat),
                            //virmens_bite_potion
                            Buff.CastBuff("Camouflage", ret => CLU.LocationContext == GroupLogic.Battleground && !Me.Mounted, "Camouflage"),
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
            get
            {
                return Base.Rest.CreateDefaultRestBehaviour();
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
                            new Decorator(ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                new PrioritySelector(
                                    Item.UseTrinkets(),
                                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                    Item.UseEngineerGloves(),
                                    new Action(delegate
                                    {
                                        Macro.isMultiCastMacroInUse();
                                        return RunStatus.Failure;
                                    }),
                                    new Decorator(ret => Macro.Burst && !Buff.PlayerHasBuff("Feign Death"), burstRotation),
                                    new Decorator(ret => (!Macro.Burst || BotChecker.BotBaseInUse("BGBuddy")) && !Buff.PlayerHasBuff("Feign Death"), baseRotation)))
                )));
            }
        }

        public override Composite PVERotation
        {
            get
            {
                return this.SingleRotation;
            }
        }
    }
}