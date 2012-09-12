﻿#region Revision Info

// This file was part of Singular - A community driven Honorbuddy CC
// $Author$
// $LastChangedBy$
#endregion

using System.ComponentModel;
using Styx.Helpers;
using CLU.Base;
using DefaultValue = Styx.Helpers.DefaultValueAttribute;

namespace CLU.Settings
{
    internal class WarriorSettings : Styx.Helpers.Settings
    {
        public WarriorSettings()
        : base(CLUSettings.SettingsPath + "_Warrior.xml")
        {
        }

        #region Common

        [Setting]
        [DefaultValue(WarriorShout.Battle)]
        [Category("Common")]
        [DisplayName("Warrior Shout Selector")]
        [Description("Choose a Warrior Shout. This is on applicable to solo play (ie: not in party or raid.)")]
        public WarriorShout ShoutSelection
        {
            get;
            set;
        }


        [Setting]
        [DefaultValue(40)]
        [Category("Common")]
        [DisplayName("Healthstone Percent")]
        [Description("Will use a Healthstone for self heal at this healthpercent. (Self Healing (General Tab) must be enabled as well.)")]
        public int HealthstonePercent
        {
            get;
            set;
        }



        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Use Shockwave to interupt")]
        [Description("When enabled CLU will use Shockwave (shared across all warrior rotations)")]
        public bool UseShockwave { get; set; }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Use Dragon Roar")]
        [Description("When enabled CLU will use Dragon Roar. (shared across all warrior rotations)")]
        public bool UseDragonRoar { get; set; }

        #endregion

        #region Protection

        [Setting]
        [DefaultValue(true)]
        [Category("Protection")]
        [DisplayName("Use Deadly Calm on CD")]
        [Description("When enabled CLU will use deadly calm on CD.")]
        public bool UseDeadlyCalm { get; set; }

        [Setting]
        [DefaultValue(true)]
        [Category("Protection")]
        [DisplayName("Use Recklessness on bosses")]
        [Description("When enabled CLU will use Recklessness on bosses.")]
        public bool UseRecklessness { get; set; }

        [Setting]
        [DefaultValue(true)]
        [Category("Protection")]
        [DisplayName("Use Shattering Throw on bosses")]
        [Description("When enabled CLU will use Shattering Throw on bosses.")]
        public bool UseShatteringThrow { get; set; }

        [Setting]
        [DefaultValue(true)]
        [Category("Protection")]
        [DisplayName("Use Pummel")]
        [Description("When enabled CLU will use Pummel.")]
        public bool UsePummel { get; set; }


        [Setting]
        [DefaultValue(true)]
        [Category("Protection")]
        [DisplayName("Use Spell Reflection")]
        [Description("When enabled CLU will use Spell Reflection")]
        public bool UseSpellReflection { get; set; }

        [Setting]
        [DefaultValue(true)]
        [Category("Protection")]
        [DisplayName("Use Intimidating Shout ")]
        [Description("When enabled CLU will Intimidating Shout")]
        public bool UseIntimidatingShout { get; set; }

        
        [Setting]
        [DefaultValue(75)]
        [Category("Protection")]
        [DisplayName("Cleave Percent")]
        [Description("Will use Cleave when Rage percent is grater than or equal to the set value")]
        public int ProtAoECleaveRagePercent { get; set; }


        [Setting]
        [DefaultValue(3)]
        [Category("Protection")]
        [DisplayName("Prot AoE Count")]
        [Description("Will use AoE abillites (Thunder Clap,Cleave,ShockWave) when agro mob count is equal to or higher then this value.")]
        public int ProtAoECount { get; set; }

        [Setting]
        [DefaultValue(75)]
        [Category("Protection")]
        [DisplayName("Heroic Strike Percent")]
        [Description("Will use Heroic Strike when Rage percent is greater than or equal to the set value")]
        public int ProtHeroicStrikeRagePercent { get; set; }


        [Setting]
        [DefaultValue(true)]
        [Category("Protection")]
        [DisplayName("Use Berserker Rage ")]
        [Description("When enabled CLU will Berserker Rage")]
        public bool UseBerserkerRage { get; set; }


        [Setting]
        [DefaultValue(true)]
        [Category("Protection")]
        [DisplayName("Demoralizing Shout")]
        [Description("Will use Demoralizing Shout  if enabled and target does not have Weakened blows.")]
        public bool UseDemoralizingShout { get; set; }


        [Setting]
        [DefaultValue(80)]
        [Category("Protection")]
        [DisplayName("Shield Block Percent")]
        [Description("Will use Shield Block when health percent is less than or equal to the set value")]
        public int ShieldBlockPercent { get; set; }

        [Setting]
        [DefaultValue(80)]
        [Category("Protection")]
        [DisplayName("Shield Barrier Percent")]
        [Description("Will use Shield Barrier when health percent is less than or equal to the set value")]
        public int ShieldBarrierPercent { get; set; }

        [Setting]
        [DefaultValue(80)]
        [Category("Protection")]
        [DisplayName("Impending Victory Percent")]
        [Description("Will use Impending Victory when health percent is less than or equal to the set value")]
        public int ImpendingVictoryPercent { get; set; }

        [Setting]
        [DefaultValue(40)]
        [Category("Protection")]
        [DisplayName("Shield Wall Percent")]
        [Description("Will use Shield Wall when health percent is less than or equal to the set value")]
        public int ShieldWallPercent { get; set; }

        [Setting]
        [DefaultValue(60)]
        [Category("Protection")]
        [DisplayName("Rallying Cry Percent")]
        [Description("Will use Rallying Cry when my health percent is greater than or equal to the set value")]
        public int RallyingCryPercent { get; set; }

        [Setting]
        [DefaultValue(40)]
        [Category("Protection")]
        [DisplayName("Last Stand Percent")]
        [Description("Will use Last Stand  when health percent is less than or equal to the set value")]
        public int LastStandPercent { get; set; }
        
       

        #endregion

        #region DPS

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("Use Damage Cooldowns")]
        //[Description("True / False if you would like the cc to use damage cooldowns")]
        //public bool UseWarriorDpsCooldowns { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("Use Interupts")]
        //[Description("True / False if you would like the cc to use Interupts")]
        //public bool UseWarriorInterupts { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("true for Battle Shout, false for Commanding")]
        //[Description("True / False if you would like the cc to use Battleshout/Commanding")]
        //public bool UseWarriorShouts { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("Slows")]
        //[Description("True / False if you would like the cc to use slows ie. Hammstring, Piercing Howl")]
        //public bool UseWarriorSlows { get; set; }

        //[Setting]
        //[DefaultValue(false)]
        //[Category("DPS")]
        //[DisplayName("Basic Rotation Only")]
        //[Description("True / False if you would like the cc to use just the basic DPS rotation only")]
        //public bool UseWarriorBasicRotation { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("Use AOE")]
        //[Description("True / False if you would like the cc to use AOE with more than 3 mobs")]
        //public bool UseWarriorAOE { get; set; }

        //[Setting]
        //[DefaultValue(false)]
        //[Category("DPS")]
        //[DisplayName("T12 2-Piece")]
        //[Description("True / False if you have the T12 2-piece set bonus")]
        //public bool UseWarriorT12 { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("Force proper stance?")]
        //[Description("True / False on whether you would like the cc to keep the toon in the proper stance for the spec. Arms:Battle, Fury:Berserker")]
        //public bool UseWarriorKeepStance { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("Use Charge/Intercept/Heroic Leap?")]
        //[Description("True / False if you would like the cc to use any gap closers")]
        //public bool UseWarriorCloser { get; set; }

        #endregion

        #region Arms


        [Setting]
        [DefaultValue(true)]
        [Category("Arms Spec")]
        [DisplayName("Enable Stance Dance")]
        [Description("When this is set to true, CLU will automaticly switch between Stances")]
        public bool EnableStanceDance
        {
            get;
            set;
        }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Arms")]
        //[DisplayName("Improved Slam Talented?")]
        //[Description("True / False if you have Improved Slam Talented")]
        //public bool UseWarriorSlamTalent { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Arms")]
        //[DisplayName("Bladestorm?")]
        //[Description("True / False if you would like the cc to use bladestorm")]
        //public bool UseWarriorBladestorm { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Arms")]
        //[DisplayName("Throwdown?")]
        //[Description("True / False if you would like the cc to use Throwdown")]
        //public bool UseWarriorThrowdown { get; set; }

        #endregion

        #region Fury

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Fury")]
        //[DisplayName("Use SMF Rotation")]
        //[Description("True / False if you would like the cc to use a SMF rotation")]
        //public bool UseWarriorSMF { get; set; }

        #endregion

    }
}