using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Effects;

namespace WCell.RealmServer.Spells
{
	public partial class SpellCast
	{
		/// <summary>
		/// Checks whether the given target resisted the debuff, represented through the given spell
		/// </summary>
		public static CastMissReason CheckDebuffResist(Unit target, Spell spell, int casterLevel, bool hostile)
		{
			var missReason = CastMissReason.None;
			if (hostile && target.CheckDebuffResist(casterLevel, target.GetLeastResistantSchool(spell)))
			{
				missReason = CastMissReason.Resist;
			}
			return missReason;
		}

		private SpellFailedReason PrepAuras()
		{
			// create Auras
			//m_auraApplicationInfos = AuraAppListPool.Obtain();
			m_auraApplicationInfos = new List<AuraApplicationInfo>(4);

			// check stacking
			SpellEffectHandler lastHandler = null;
			for (var i = 0; i < m_handlers.Length; i++)
			{
				var spellHandler = m_handlers[i];
				if (spellHandler.Effect.IsAuraEffect)
				{
					if (lastHandler != null && lastHandler.Effect.ImplicitTargetA == spellHandler.Effect.ImplicitTargetA)
					{
						continue;
					}
					lastHandler = spellHandler;

					var doubleTarget = false;
					if (spellHandler.m_targets != null)
					{
						foreach (var target in spellHandler.m_targets)
						{
							if (target is Unit)
							{
								foreach (var info in m_auraApplicationInfos)
								{
									if (info.Target == target)
									{
										doubleTarget = true;
										break;
									}
								}

								if (doubleTarget)
								{
									doubleTarget = false;
									continue;
								}

								var id = m_spell.GetAuraUID(CasterReference, target);
								var failReason = SpellFailedReason.Ok;
								if (((Unit)target).Auras.CheckStackOrOverride(CasterReference, id, m_spell, ref failReason, this))
								{
									m_auraApplicationInfos.Add(new AuraApplicationInfo((Unit)target));
								}
								else if (failReason != SpellFailedReason.Ok && !IsAoE)
								{
									// spell fails
									// m_auraApplicationInfos.Clear();
									// AuraAppListPool.Recycle(m_auraApplicationInfos);
									return failReason;
								}
							}
						}
					}
				}
			}
			return SpellFailedReason.Ok;
		}

		void CreateAuras(ref List<CastMiss> missedTargets, ref List<IAura> auras, DynamicObject dynObj)
		{
			auras = AuraListPool.Obtain();

			var allowDead = m_spell.PersistsThroughDeath;

			// create AreaAura
			if (m_spell.IsAreaAura)
			{
				if (dynObj != null || (CasterObject != null && (allowDead || !(CasterObject is Unit) || ((Unit)CasterObject).IsAlive)))
				{
					// AreaAura is created at the target location if it is a DynamicObject, else its applied to the caster
					var aaura = new AreaAura(dynObj ?? CasterObject, m_spell);
					if (dynObj != null)
					{
						// also start the area aura
						auras.Add(aaura);
					}
					// else: Is coupled to an Aura instance
				}
				else
				{
					LogManager.GetCurrentClassLogger().Warn(
						"Tried to cast Spell {0} with invalid dynObj or Caster - dynObj: {1}, CasterObject: {2}, CasterUnit: {3}",
						m_spell, dynObj, CasterObject, CasterUnit);
				}
			}

			// remove missed targets
			for (var i = m_auraApplicationInfos.Count - 1; i >= 0; i--)
			{
				var app = m_auraApplicationInfos[i];
				if (!m_targets.Contains(app.Target))
				{
					m_auraApplicationInfos.RemoveAt(i);
				}
			}
			if (m_auraApplicationInfos.Count == 0)
			{
				return;
			}

			// create Aura-Handlers
			for (var i = 0; i < m_handlers.Length; i++)
			{
				var spellHandler = m_handlers[i];
				if (spellHandler is ApplyAuraEffectHandler)
				{
					((ApplyAuraEffectHandler)spellHandler).AddAuraHandlers(m_auraApplicationInfos);
				}
			}
			if (missedTargets == null)
			{
				missedTargets = CastMissListPool.Obtain();
			}

			// apply all new Auras
			for (var i = 0; i < m_auraApplicationInfos.Count; i++)
			{
				var info = m_auraApplicationInfos[i];
				var target = info.Target;

				if (!target.IsInContext)
				{
					continue;
				}

				if (info.Handlers == null || (!allowDead && !target.IsAlive))
				{
					continue;
				}

				// check for immunities and resistances
				CastMissReason missReason;
				var hostile = m_spell.IsHarmfulFor(CasterReference, target);

				if (!IsPassive && !m_spell.IsPreventionDebuff &&
					(missReason = CheckDebuffResist(target, m_spell, CasterReference.Level, hostile)) != CastMissReason.None)
				{
					// debuff miss
					missedTargets.Add(new CastMiss(target, missReason));
				}
				else
				{
					// create aura
					var newAura = target.Auras.CreateAura(CasterReference, m_spell, info.Handlers, UsedItem, !m_spell.IsPreventionDebuff && !hostile);
					if (newAura != null)
					{
						// check for debuff
						if (!m_spell.IsPreventionDebuff && hostile && target.IsInWorld && target.IsAlive)
						{
							// force combat mode
							target.IsInCombat = true;
						}
						// add Aura now
						auras.Add(newAura);
					}
				}
			}

			//m_auraApplicationInfos.Clear();
			//AuraAppListPool.Recycle(m_auraApplicationInfos);
			m_auraApplicationInfos = null;
		}
	}
}