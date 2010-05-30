﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.Spells.Priest
{
	public static class PriestFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixPriest()
		{
			// only proc on kill that rewards xp or honor
			SpellLineId.PriestShadowSpiritTap.Apply(spell => spell.ProcTriggerFlags |= ProcTriggerFlags.GainExperience);

			// Holy Inspiration can be proced when priest casts the given spells
			// TODO: Only cast on crit
			SpellLineId.PriestHolyInspiration.Apply(spell => spell.AddCasterProcSpells(
				SpellLineId.PriestFlashHeal,
				SpellLineId.PriestHeal,
				SpellLineId.PriestGreaterHeal,
				SpellLineId.PriestBindingHeal,
				SpellLineId.PriestDisciplinePenance,
				SpellLineId.PriestPrayerOfMending,
				SpellLineId.PriestPrayerOfHealing,
				SpellLineId.PriestHolyCircleOfHealing));

			//
			SpellLineId.PriestShadowMindFlay.Apply(spell =>
			{
				var effect = spell.AddAuraEffect(AuraType.PeriodicDamage, ImplicitTargetType.SingleEnemy);
				effect.BasePoints = spell.Effects[2].BasePoints * 3;
				effect.Amplitude = spell.Effects[2].Amplitude;
			});

			SpellLineId.PriestShadowShadowWeaving.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.AddTargetTrigger);
				effect.ImplicitTargetA = ImplicitTargetType.Self;
				effect.AddToEffectMask(SpellLineId.PriestShadowMindFlay);
			});

            // Mana regen on Priest Dispersion Talant
            SpellLineId.PriestShadowDispersion.Apply(spell =>
            {
                var effect = spell.AddPeriodicTriggerSpellEffect(SpellId.Dispersion_2, ImplicitTargetType.Self);
                effect.Amplitude = 1000;
            });
		}
	}
}
