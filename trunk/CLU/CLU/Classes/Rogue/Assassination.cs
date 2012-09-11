﻿using System.Linq;

using CLU.Base;
using CLU.Helpers;
using CLU.Settings;

using CommonBehaviors.Actions;

using Styx.TreeSharp;

using Rest = CLU.Base.Rest;

//using Styx.Logic.Combat;

namespace CLU.Classes.Rogue
{
    public class Assassination : RotationBase
    {
        #region Public Properties

        public override string Help
        {
            get
            {
                return "\n" + "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" + "1. use defensive moves when holding aggro.\n" + "2. Pop cooldowns: \n" +
                       "==> UseTrinkets \n" + "==> UseRacials \n" + "==> UseEngineerGloves \n" +
                       "3. Kick for interrupts.\n" + "4. Tricks of the Trade on the best target (tank, then class)\n" +
                       "5. Maintain Slice and Dice and Rupture.\n" +
                       "6. Use Dispatch when Blindside is up and under 5 combo points.\n" +
                       "7. Dispatch when target is under 35% HP.\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits for this rotation: Weischbier, Wulf, Singularity team, LaoArchAngel\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override string KeySpell
        {
            get { return "Mutilate"; }
        }

        /// <summary>
        /// Gets the healing rotation.
        /// Rotation created by wulf.
        /// </summary>
        public override Composite Medic
        {
            get
            {
                return new Decorator
                    (ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                     new PrioritySelector
                         (Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 40, "Healthstone"),
                          Spell.CastSelfSpell
                              ("Smoke Bomb",
                               ret =>
                               Me.CurrentTarget != null && Me.HealthPercent < 30 && Me.CurrentTarget.IsTargetingMeOrPet,
                               "Smoke Bomb"),
                          Spell.CastSelfSpell
                              ("Combat Readiness",
                               ret =>
                               Me.CurrentTarget != null && Me.HealthPercent < 40 && Me.CurrentTarget.IsTargetingMeOrPet,
                               "Combat Readiness"),
                          Spell.CastSelfSpell
                              ("Evasion",
                               ret =>
                               Me.HealthPercent < 35 &&
                               Unit.EnemyUnits.Count(u => u.DistanceSqr < 6 * 6 && u.IsTargetingMeOrPet) >= 1, "Evasion"),
                          Spell.CastSelfSpell
                              ("Cloak of Shadows",
                               ret => Unit.EnemyUnits.Count(u => u.IsTargetingMeOrPet && u.IsCasting) >= 1,
                               "Cloak of Shadows"), Poisons.CreateApplyPoisons()));
            }
        }

        public override string Name
        {
            get { return "Assassination Rotation"; }
        }

        public override Composite PVERotation
        {
            get { return this.SingleRotation; }
        }

        public override Composite PVPRotation
        {
            get { return this.SingleRotation; }
        }

        /// <summary>
        /// Gets the combat preperation rotation.
        /// Rotation by wulf.
        /// </summary>
        public override Composite PreCombat
        {
            get
            {
                return new Decorator
                    (ret =>
                     !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") &&
                     !Me.HasAura("Drink"), new PrioritySelector
                                               ( // Stealth
                                               Spell.CastSelfSpell
                                                   ("Stealth",
                                                    ret =>
                                                    !Buff.PlayerHasBuff("Stealth") &&
                                                    CLUSettings.Instance.Rogue.EnableAlwaysStealth, "Stealth"),
                                               Poisons.CreateApplyPoisons()));
            }
        }

        /// <summary>
        /// Gets the rest rotation.
        /// Rotation created by the Singular devs.
        /// </summary>
        public override Composite Resting
        {
            get { return Rest.CreateDefaultRestBehaviour(); }
        }

        public override Composite SingleRotation
        {
            get
            {
                return new PrioritySelector
                    (new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),
                     EncounterSpecific.ExtraActionButton(),
                     Buff.CastBuff("Stealth", ret => CLUSettings.Instance.Rogue.EnableAlwaysStealth, "Stealth"),
                     Cooldowns,
                     Spell.CastSelfSpell("Feint",ret =>Me.CurrentTarget != null &&(/*Me.CurrentTarget.ThreatInfo.RawPercent > 80 ||*/ EncounterSpecific.IsMorchokStomp()), "Feint"),
                     Spell.CastSpell("Tricks of the Trade", u => Unit.BestTricksTarget, ret => CLUSettings.Instance.Rogue.UseTricksOfTheTrade && Unit.BestTricksTarget != null,"Tricks of the Trade"), 
                     Spell.CastInterupt("Kick", ret => Me.IsWithinMeleeRange, "Kick"),
                     Spell.CastSpell("Redirect", ret => Me.RawComboPoints > 0 && Me.ComboPoints < 1, "Redirect"), AoE,
                     Spell.CastSelfSpell("Slice and Dice", ret => !Buff.PlayerHasActiveBuff("Slice and Dice"), "Slice and Dice"), 
                     //Vanish,
                     Rupture,
                     Spell.CastSpell("Vendetta",ret =>Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds > 6, "Vendetta"),
                     //Spell.CastSpell("Preparation",ret =>SpellManager.HasSpell(14185) && Me.CurrentTarget != null &&Unit.IsTargetWorthy(Me.CurrentTarget) && SpellManager.Spells["Vanish"].Cooldown, "Preparation"),
                     Spell.CastSpell("Dispatch", ret => Me.ComboPoints < 5 && Buff.PlayerHasBuff("Blindside"), "Dispatch @ Blindside"),
                     Envenom,
                     Spell.CastSpell("Dispatch", ret => Me.ComboPoints < ReqCmbPts && Me.CurrentTarget.HealthPercent < 35, "Dispatch"),
                     Spell.CastSpell("Mutilate", ret => Me.ComboPoints < ReqCmbPts && Me.CurrentTarget.HealthPercent >= 35,"Mutilate"));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the area of effect rotaion.
        /// Rotation by Wulf.
        /// </summary>
        private static PrioritySelector AoE
        {
            get
            {
                return new PrioritySelector
                    (Spell.CastAreaSpell("Crimson Tempest", 8, false, 4, 0, 0,ret => Unit.EnemyUnits.Any(a => !a.HasMyAura("Crimson Tempest") && Me.ComboPoints > 3),"Crimson Tempest"),
                     Spell.CastAreaSpell("Fan of Knives", 8, false, 4, 0.0, 0.0, ret => Me.ComboPoints < 5, "Fan of Knives"));
            }
        }

        private static Decorator Cooldowns
        {
            get
            {
                return new Decorator
                    (ret =>
                     Me.CurrentTarget != null &&
                     ((Unit.IsTargetWorthy(Me.CurrentTarget) || Buff.TargetHasDebuff("Vendetta"))),//Switched to || instead of &&, we want to use trinkets on Cd and not every 2min
                     new PrioritySelector
                         (Item.UseTrinkets(), Spell.UseRacials(), Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),// Thanks Kink
                          Item.UseEngineerGloves()));
            }
        }

        private static Composite Envenom
        {
            get
            {
                return new Decorator(cond => Buff.PlayerHasActiveBuff("Slice and Dice"),
                    new PrioritySelector(
                        Spell.CastSpell("Envenom", ret => Me.ComboPoints >= ReqCmbPts && !Buff.TargetHasDebuff("Envenom") && Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds > 5, "Envenom @ First"),// Envenom if we have enough combo points, Envenom debuff is down and Rupture is safe.
                        Spell.CastSpell("Envenom", ret => Me.ComboPoints >= ReqCmbPts && Me.CurrentEnergy > 90 && Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds > 2, "Envenom @ Second"),// Envenom if we have enough combo points, Rupture is safe and we're about to cap.
                        Spell.CastSpell("Envenom", ret => Me.ComboPoints >= 2 && Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds < 2, "Envenom @ Third (WARNING!)")// Envenom if SnD is about to fall off. This should never happen.
                    ));
            }
        }

        private static int ReqCmbPts
        {
            get { return (Me.CurrentTarget != null) && (Me.CurrentTarget.HealthPercent < 35) ? 5 : 4; }
        }

        private static Decorator Rupture
        {
            get
            {
                return new Decorator
                    (x =>
                     Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds > 6 &&
                     Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds <= 2,
                    // Do not rupture if SnD is about to come down.
                     new PrioritySelector
                         (Spell.CastSpell("Rupture", ret => !Buff.TargetHasDebuff("Rupture"), "Rupture @ Down"),// Rupture if it's down.
                          Spell.CastSpell("Rupture", ret => Me.ComboPoints >= ReqCmbPts, "Rupture @ Low")));// Rupture if it's about to fall off and we have 4 or 5 combo points.
            }
        }

        /// <summary>
        /// Gets the vanish rotation
        /// Rotation by Weischbier.
        /// Logic by LaoArchAngel
        /// </summary>
        private static Composite Vanish
        {
            get
            {
                return new Decorator
                    (x => // Only Do this if SnD is up, Rupture is up, Target is CD-worthy and we've got spare points.
                     Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds > 6 &&
                     Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds > 4 && Unit.IsTargetWorthy(Me.CurrentTarget) &&
                     Me.ComboPoints < 4 && Me.CurrentTarget.IsWithinMeleeRange,
                     new Sequence(
                         Spell.CastSelfSpell("Vanish", x => true, "Vanish"),
                    //new WaitContinue(1, ret => Me.HasMyAura("Stealth") || Me.HasMyAura("Vanish"), new ActionAlwaysSucceed()),
                    //Spell.CastSpell("Shadowstep", ret => SpellManager.HasSpell("Shadowstep"), "Shadowstep"),
                          Spell.CastSpellByID(8676, ret => Me.CurrentTarget != null && IsBehind, "Ambush")));
            }
        }

        #endregion

        #region Methods

        private static bool IsBehind
        {
            get
            {
                return Me.CurrentTarget != null && Me.IsBehind(Me.CurrentTarget);
            }
        }

        #endregion
    }
}