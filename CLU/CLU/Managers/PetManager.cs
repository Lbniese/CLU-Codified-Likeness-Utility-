﻿namespace CLU.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommonBehaviors.Actions;
    using Styx;
    using Styx.Combat.CombatRoutine;
    using Styx.Common.Helpers;
    using Styx.CommonBot;
    //using Styx.Logic.Combat;
    //using Styx.Logic.Pathing;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;
    using System.Drawing;

    using Styx.TreeSharp;
    using global::CLU.Base;
    using global::CLU.Settings;
    using Action = Styx.TreeSharp.Action;

    internal class PetManager
    {
        /* putting all the Pets Spell/Buff/Debuff logic here */

        private static readonly List<WoWPetSpell> PetSpells = new List<WoWPetSpell>();

        private static readonly PetManager PetsInstance = new PetManager();

        private static ulong petGuid;

        private static LocalPlayer Me
        {
            get {
                return StyxWoW.Me;
            }
        }

        private static readonly WaitTimer PetTimer = new WaitTimer(TimeSpan.FromSeconds(2));

        /// <summary>
        /// An instance of the Pets Class
        /// </summary>
        public static PetManager Instance
        {
            get {
                return PetsInstance;
            }
        }

        internal static void Pulse()
        {
            if (!StyxWoW.Me.GotAlivePet) {
                PetSpells.Clear();
                return;
            }

            if (StyxWoW.Me.Pet != null && petGuid != StyxWoW.Me.Pet.Guid) {
                petGuid = StyxWoW.Me.Pet.Guid;
                PetSpells.Clear();
                PetSpells.AddRange(StyxWoW.Me.PetSpells);
            }
        }

        /*--------------------------------------------------------
         * Spells
         *--------------------------------------------------------
         */

        /// <summary>Returns the Pet spell cooldown using Timespan (00:00:00.0000000)
        /// gtfo if the Pet dosn't have the spell.</summary>
        /// <param name="name">the name of the spell to check for</param>
        /// <returns>The spell cooldown.</returns>
        public static TimeSpan PetSpellCooldown(string name)
        {
            WoWPetSpell petAction = PetSpells.FirstOrDefault(p => p.ToString() == name);
            if (petAction == null || petAction.Spell == null) {
                return TimeSpan.Zero;
            }

            CLU.DebugLog(Color.ForestGreen, " [PetSpellCooldown] {0} : {1}", name, petAction.Spell.CooldownTimeLeft);
            return petAction.Spell.CooldownTimeLeft;
        }

        /// <summary>Returns a summoned pet</summary>
        /// <param name="name">the name of the spell</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast pet summon spell.</returns>
        public static Composite CastPetSummonSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
            var isWarlock = Me.Class == WoWClass.Warlock && SpellManager.Spells[name].Name.StartsWith("Summon ");
            if (isWarlock) {
                Spell.KnownChanneledSpells.Add(name);
            }

            return new Decorator(
            	delegate(object a) {
            		if (!cond(a))
            			return false;

            		if (!Spell.CanCast(name, Me))
            			return false;

            		return true;
            },
            new Decorator( ret => !Me.GotAlivePet && PetTimer.IsFinished,
                           new Sequence(
                               new Action(a => CLU.Log(" {0}", label)),
                               new Action(a => Spell.CastMySpell(name)),
                               Spell.CreateWaitForLagDuration(),
                               new Wait(5, a => Me.GotAlivePet || !Me.IsCasting,
                                        new PrioritySelector(
                                            new Decorator(a => StyxWoW.Me.IsCasting, new Action(ret => SpellManager.StopCasting())),
                                            new ActionAlwaysSucceed())))));
        }


        /// <summary>Cast's a pet spell</summary>
        /// <param name="name">the pet spell to cast</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast pet spell.</returns>
        public static Composite CastPetSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                       a => cond(a) && CanCastPetSpell(name),
                       new Sequence(
                           new Action(a => CLU.Log(" [Pet Spell] {0} ", label)), new Action(a => CastMyPetSpell(name))));
        }

        /// <summary>Returns true if the pet spell can be cast</summary>
        /// <param name="name">the name of the spell to check</param>
        /// <returns>The can cast pet spell.</returns>
        public static bool CanCastPetSpell(string name)
        {
            WoWPetSpell petAction = Me.PetSpells.FirstOrDefault(p => p.ToString() == name);
            if (petAction == null || petAction.Spell == null) {
                CLU.DebugLog(Color.ForestGreen, String.Format("[PetManager] Pet does not have the spell {0}", name));
                return false;
            }
            CLU.DebugLog(Color.ForestGreen, String.Format("[PetManager] Spell {0}, Cooldown left {1}", petAction.Spell.Name, petAction.Spell.CooldownTimeLeft));
            return !petAction.Spell.Cooldown;
        }

        /// <summary>
        /// Check to see if our pet has a spell
        /// </summary>
        /// <param name="name">the spell name in english to check for</param>
        /// <returns>true if the pet has the spell</returns>
        public static bool HasSpellPet(string name)
        {
            WoWPetSpell petAction = Me.PetSpells.FirstOrDefault(p => p.ToString() == name);
            if (petAction == null || petAction.Spell == null) {
                CLU.DebugLog(Color.ForestGreen, String.Format("[PetManager] Pet does not have the spell {0}", name));
                return false;
            }
            return true;
        }

        /// <summary>
        /// the main cast pet spell method (uses Lua)
        /// </summary>
        /// <param name="name">the name of the spell to cast</param>
        private static void CastMyPetSpell(string name)
        {
            var spell = Me.PetSpells.FirstOrDefault(p => p.ToString() == name);
            if (spell == null)
                return;

            Lua.DoString("CastPetAction({0})", spell.ActionBarIndex + 1);
        }

        /// <summary>Casts a Pet spell at the units location</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The on Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell at location.</returns>
        public static Composite CastPetSpellAtLocation(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            // CLU.DebugLog(" [CastSpellAtLocation] name = {0} location = {1} and can we cast it? {2}", name, CLU.SafeName(unit), CanCast(name));
            return new Decorator(
            	delegate(object a) {
            		if (!CLUSettings.Instance.UseAoEAbilities)
            			return false;

            		if (Me.CurrentTarget == null)
            			return false;

            		WoWPoint currTargetLocation = Me.CurrentTarget.Location;
            		if (Unit.NearbyControlledUnits(currTargetLocation, 20, false).Any())
            			return false;

            		if (!cond(a))
            			return false;

            		if (!CanCastPetSpell(name))
            			return false;

            		return onUnit != null;
            },
            new Sequence(
                new Action(a => CLU.Log(" [Pet Casting at location] {0} ", label)),
                new Action(a => CastMyPetSpell(name)),
                // new WaitContinue(
                //    0,
                //    ret => StyxWoW.Me.CurrentPendingCursorSpell != null &&
                //           StyxWoW.Me.CurrentPendingCursorSpell.Name == name,
                //    new ActionAlwaysSucceed()),
                new Action(a => SpellManager.ClickRemoteLocation(onUnit(a).Location))));
        }

        /*--------------------------------------------------------
         * Buffs
         *--------------------------------------------------------
         */

        /// <summary>Returns true if the Pet has the buff</summary>
        /// <param name="name">the name of the buff to check for</param>
        /// <returns>The pet has buff.</returns>
        public static bool PetHasBuff(string name)
        {
            // Me.Pet.ActiveAuras.ContainsKey(name);
            return Me.GotAlivePet && Buff.HasAura(Me.Pet, name, Me.Pet);
        }

        /// <summary>Returns true if the Pet has the buff</summary>
        /// <param name="name">the name of the buff to check for</param>
        /// <returns>The pet has buff.</returns>
        public static bool PetHasActiveBuff(string name)
        {
            return Me.GotAlivePet && Me.Pet.ActiveAuras.ContainsKey(name);
        }

        /// <summary>Returns the buff count on the Pet</summary>
        /// <param name="name">the name of the buff to check</param>
        /// <returns>The pet count buff.</returns>
        public static uint PetCountBuff(string name)
        {
            return !Me.GotAlivePet ? 0 : Buff.GetAuraStack(Me.Pet, name, false);
        }

        /// <summary>Returns the buff time left on the Pet</summary>
        /// <param name="name">the name of the buff to check for</param>
        /// <returns>The pet buff time left.</returns>
        public static TimeSpan PetBuffTimeLeft(string name)
        {
            return !Me.GotAlivePet ? TimeSpan.Zero : Buff.GetAuraTimeLeft(Me.Pet, name, false);
        }

        /// <summary>Returns true if the Pet has the buff</summary>
        /// <param name="id">The id.</param>
        /// <returns>The pet has buff.</returns>
        public static bool PetAuraByIdActive(int id)
        {
            // Me.Pet.ActiveAuras.ContainsKey(name);
            return Me.Pet != null && StyxWoW.Me.Pet.GetAuraById(id).IsActive; // !Me.Pet.Stunned ||  PetAuraByIdActive(32752) //Summoning Disorientation
        }
    }
}
