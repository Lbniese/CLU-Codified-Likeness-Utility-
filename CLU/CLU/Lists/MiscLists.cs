﻿#region Revision info
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

namespace CLU.Lists
{
    using System.Collections.Generic;

    public static class MiscLists
    {
        public static HashSet<string> GCDFreeAbilities
        {
            get {
                return _gcdFreeAbilities;
            }
        }

        private static readonly HashSet<string> _gcdFreeAbilities = new HashSet<string> {
            "Alter Time",
            "Ancient Guardian",
            "Annihilate",
            "Anti-Magic Shell",
            "Anti-Magic Shield",
            "Anti-Magic Zone",
            "Arcane Power",
            "Avenging Wrath",
            "Barkskin",
            "Blessing of Protection",
            "Blood Tap",
            "Blood and Thunder",
            "Bloodlust",
            "Bloodrage",
            "Charge",
            "Cleave",
            "Cold Blood",
            "Cold Snap",
            "Combustion",
            "Counterspell",
            "Create Soulwell",
            "Dark Command",
            "Dark Soul",
            "Death Grip",
            "Demon Soul",
            "Demonic Sacrifice",
            "Deterrence",
            "Devour Magic",
            "Disengage",
            "Divine Favor",
            "Divine Sacrifice",
            "Elemental Mastery",
            "Empower Rune Weapon",
            "Evasion",
            "Feign Death",
            "Fel Domination",
            "Feral Charge",
            "Frenzied Regeneration",
            "Guardian Spirit",
            "Hammer of Wrath",
            "Hand of Reckoning",
            "Heroic Strike",
            "Hysteria",
            "Icebound Fortitude",
            "Icy Veins",
            "Inner Focus",
            "Intercept",
            "Kick",
            "Kill Command",
            "Leap of Faith",
            "Lichborne",
            "Masters Call",
            "Maul",
            "Metamorphosis",
            "Mind Freeze",
            "Nature's Grasp",
            "Nature's Swiftness",
            "Pillar of Frost",
            "Power Infusion",
            "Premeditation",
            "Preparation",
            "Presence of Mind",
            "Pummel",
            "Rapid Fire",
            "Rune Tap",
            "Seduction",
            "Shadow Infusion",
            "Shadowfury",
            "Shadowstep",
            "Shield Bash",
            "Shield Block",
            "Shield of the Righteous",
            "Shock Blast",
            "Silence",
            "Silencing Shot",
            "Solar Beam",
            "Soulburn",
            "Spell Lock",
            "Spell Reflection",
            "Spirit Mend",
            "Spiritwalker's Grace",
            "Sprint",
            "Starfall",
            "Survival Instincts",
            "Sweeping Strikes",
            "Time Warp",
            "Unending Resolve",
            "Vampiric Blood",
            "Vanish",
            "Whiplash",
            "Wild Polymornph",
            "Wind Shear",
            "Zealotry",
        };
    }
}