/*************************************************************************
 *
 *   file		: QuestTemplate.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-06 19:13:00 +0100 (st, 06 II 2008) $
 *   last author	: $LastChangedBy: Nivelo $
 *   revision		: $Rev: 112 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.Quests;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Lang;
using WCell.Util;
using WCell.Util.Data;
using WCell.Constants.NPCs;
using WCell.Constants.GameObjects;
using WCell.RealmServer.AreaTriggers;
using WCell.RealmServer.Content;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Quests
{
	/// <summary>
	/// Quest Templates represent all information that is associated with a possible ingame Quest.
	/// TODO: AreaTrigger relations
	/// </summary>
	[DataHolder]
	public partial class QuestTemplate : IDataHolder
	{
		private const uint MaxId = 200000;

		/// <summary>
		/// The list of all entities that can start this Quest
		/// </summary>
		public readonly List<IQuestHolderEntry> Starters = new List<IQuestHolderEntry>(3);

		/// <summary>
		/// The List of all entities that can finish this Quest
		/// </summary>
		public readonly List<IQuestHolderEntry> Finishers = new List<IQuestHolderEntry>(3);

		#region QuestRoot

		/// <summary>
		/// Unique identifier of quest.
		/// </summary>
		public uint Id;

		/// <summary>
		/// Determines whether quest is active or not. 
		/// </summary>
		public QuestTemplateStatus IsActive = QuestTemplateStatus.Active;

		/// <summary>
		/// Level of given quest, for which the quest is optimized. (If FFFFFFFF, then it's level independent or special)
		/// </summary>
		public uint Level, MinLevel;

		/// <summary>
		/// 
		/// </summary>
		public int Category;

		/// <summary>
		/// Restricted to this Zone
		/// </summary>
		[NotPersistent]
		public ZoneTemplate ZoneTemplate;

		/// <summary>
		/// Id for QuestSort.dbc
		/// </summary>
		//[NotPersistent]
		//public ZoneId ZoneId;

		/// <summary>
		/// QuestType, for more detailed description of type look at <seealso cref="Constants.Quests.QuestType"/>
		/// </summary>
		public QuestType QuestType;

		/// <summary>
		/// Number of players quest is optimized for.
		/// </summary>
		public uint SuggestedPlayers;

		/// <summary>
		/// 
		/// </summary>
		public FactionReputationEntry ObjectiveMinReputation;

		/// <summary>
		/// Player cannot have more than this in reputation
		/// </summary>
		public FactionReputationEntry ObjectiveMaxReputation;

		/// <summary>
		/// Gets or sets the reward money in copper, if it's negative,
		/// money will be required for quest completition and deducted
		/// from player's money after completition.
		/// 1     = 1 copper
		/// 10    = 10 copper
		/// 100   = 1 silver
		/// 1000  = 10 silver
		/// 10000 = 1 gold 
		/// </summary>
		public int RewMoney;

		/// <summary>
		/// Money gained instead of RewMoney at level 70.
		/// </summary>
		public uint MoneyAtMaxLevel;

		/// <summary>
		/// Given spell id, which is added to character's spell book when finishing the quest.
		/// </summary>
		public SpellId RewSpell;

		/// <summary>
		/// Cast spell id of spell which is casted on character when finishing the quest.
		/// </summary>
		public SpellId CastSpell;

		/// <summary>
		/// 
		/// </summary>
		[NotPersistent]
		public uint BonusHonor;

		/// <summary>
		/// An Quest-starting Item
		/// </summary>
		public ItemId SrcItemId;

		/// <summary>
		/// QuestFlags, for more detailed description of flags look at <see cref="QuestFlags"/>
		/// </summary>
		public QuestFlags Flags;

		/// <summary>
		/// 
		/// </summary>
		public TitleId RewardTitleId;

		/* 
		public uint PlayerKillCount;
		*/

		public uint RewardTalents;

		/// <summary>
		/// The id of the loot to be sent via mail to the finisher after completion
		/// </summary>
		public uint RewardMailTemplateId;

		/// <summary>
		/// The delay after which the Reward Mail should be sent
		/// </summary>
		public uint RewardMailDelaySeconds;

		/// <summary>
		/// Array of items containing item ID, index and quantity of items.
		/// </summary>
		[Persistent(QuestConstants.MaxRewardItems)]
		public ItemStackDescription[] RewardItems;

		/// <summary>
		/// Array of items containing item ID, index and quantity of items.
		/// </summary>
		[Persistent(QuestConstants.MaxRewardChoiceItems)]
		public ItemStackDescription[] RewardChoiceItems;

		/// <summary>
		/// Map Id of point showing something.
		/// </summary>
		public MapId MapId;

		/// <summary>
		/// X-coordinate of point showing something.
		/// </summary>
		public float PointX;

		/// <summary>
		/// Y-coordinate of point showing something.
		/// </summary>
		public float PointY;

		/// <summary>
		/// Options of point showing something.
		/// </summary>
		public uint PointOpt;

		[Persistent((int)ClientLocale.End)]
		public string[] Titles;

		/// <summary>
		/// Title (name) of the quest to be shown in <see cref="QuestLog"/> in the server's default language.
		/// </summary>
		public string DefaultTitle
		{
			get { return Titles != null ? Titles.LocalizeWithDefaultLocale() : "[unknown Quest]"; }
		}

		/// <summary>
		/// Text sumarizing the objectives of quest.
		/// </summary>
		[Persistent((int)ClientLocale.End)]
		public string[] Instructions;

		/// <summary>
		/// Objective of the quest to be shown in <see cref="QuestLog"/> in the server's default language.
		/// </summary>
		[NotPersistent]
		public string DefaultObjective
		{
			get { return Instructions.LocalizeWithDefaultLocale(); }
		}

		/// <summary>
		/// Detailed quest descriptions shown in <see cref="QuestLog"/>
		/// </summary>
		[Persistent((int)ClientLocale.End)]
		public string[] Details;

		[NotPersistent]
		public string DefaultDetailText
		{
			get { return Details.LocalizeWithDefaultLocale(); }
		}

		/// <summary>
		/// Text which is QuestGiver going to say upon quest finishing.
		/// </summary>
		[Persistent((int)ClientLocale.End)]
		public string[] EndTexts;

		[NotPersistent]
		public string DefaultEndText
		{
			get { return EndTexts.LocalizeWithDefaultLocale(); }
		}

		/// <summary>
		/// Array of interactions containing ID, index and quantity.
		/// </summary>
		[Persistent(QuestConstants.MaxObjectInteractions)]
		public QuestInteractionTemplate[] ObjectInteractions = new QuestInteractionTemplate[QuestConstants.MaxObjectInteractions];

		[NotPersistent]
		public QuestInteractionTemplate[] GOInteractions;

		public bool HasGOEvent
		{
			get { return GOInteractions != null || GOInteraction != null; }
		}

		[NotPersistent]
		public QuestInteractionTemplate[] NPCInteractions;

		public bool HasNPCInteractionEvent
		{
			get
			{
				return NPCInteracted != null ||
				(NPCInteractions != null && NPCInteractions.Length > 0);
			}
		}

		/// <summary>
		/// Array of items you need to collect.
		/// If the items are quest-only,
		/// they will be deleted upon canceling quest.
		/// </summary>
		[Persistent(QuestConstants.MaxObjectInteractions)]
		public ItemStackDescription[] CollectableItems;

		/// <summary>
		/// Array of quest objectives text, every value is a short note that is shown
		/// once all objectives of the corresponding slot have been fullfilled.
		/// </summary>
		[Persistent((int)ClientLocale.End)]
		public QuestObjectiveSet[] ObjectiveTexts = new QuestObjectiveSet[(int)ClientLocale.End];

		#endregion

		#region QuestSettings

		/// <summary>
		/// Special Quest flags, unknown purpose.
		/// </summary>
		public QuestSpecialFlags SpecialFlags;

		/// <summary>
		/// Time limit for timed Quest. It's not taken into account
		/// if there's no Timed flag set in QuestFlags
		/// </summary>
		public uint TimeLimit;

		/// <summary>
		/// Text which will be shown when the objectives are done. In the
		/// offering rewards window.
		/// </summary>
		[Persistent((int)ClientLocale.End)]
		public string[] OfferRewardTexts;

		[NotPersistent]
		public string DefaultOfferRewardText
		{
			get { return OfferRewardTexts.LocalizeWithDefaultLocale(); }
		}

		/// <summary>
		/// Text which will be shown when the objectives aren't done yet. In the 
		/// window where you have to have items.
		/// </summary>
		[Persistent((int)ClientLocale.End)]
		public string[] ProgressTexts;

		[NotPersistent]
		public string DefaultProgressText
		{
			get { return ProgressTexts.LocalizeWithDefaultLocale(); }
		}

		/// <summary>
		/// Value indicating whether this <see cref="QuestTemplate"/> is repeatable.
		/// </summary>
		public bool Repeatable;

		/// <summary>
		/// Value indicating whether this <see cref="QuestTemplate"/> is available only for clients
		/// with expansion.
		/// probably obsolete, there is QuestFlag for this
		/// </summary>
		public ClientId RequiredClientId;

		/// <summary>
		/// Array of Items to be given upon accepting the quest. These items will be destroyed when the Quest is solved or canceled.
		/// </summary>
		[NotPersistent]
		public List<ItemStackDescription> InitialItems = new List<ItemStackDescription>(1);

		#endregion

		#region QuestRequirements

		/// <summary>
		/// Required minimal level to be able to see this quest.
		/// </summary>
		public uint RequiredLevel;

		/// <summary>
		/// Required race mask to check availability to player.
		/// </summary>
		public RaceMask RequiredRaces;

		/// <summary>
		/// Required class mask to check availability to player.
		/// </summary>
		public ClassId RequiredClass;

		public int ReqSkillOrClass;

		public SkillId RequiredSkill;

		/// <summary>
		/// Tradeskill level which is required to accept this quest.
		/// </summary>
		public uint RequiredSkillValue;
		
		// Represents the Quest graph
		public int PreviousQuestId, NextQuestId, ExclusiveGroup;
		public uint FollowupQuestId;

        /// <summary>
        /// Represents the Reward XP column id.
        /// </summary>
	    public int RewXPId;

		/// <summary>
		/// Quests that may must all be active in order to get this Quest
		/// </summary>
		[NotPersistent]
		public readonly List<uint> ReqAllActiveQuests = new List<uint>(2);

		/// <summary>
		/// Quests that must all be finished in order to get this Quest
		/// </summary>
		[NotPersistent]
		public readonly List<uint> ReqAllFinishedQuests = new List<uint>(2);

		/// <summary>
		/// Quests of which at least one must be active to get this Quest
		/// </summary>
		[NotPersistent]
		public readonly List<uint> ReqAnyActiveQuests = new List<uint>(2);

		/// <summary>
		/// Quests of which at least one must be finished to get this Quest
		/// </summary>
		[NotPersistent]
		public readonly List<uint> ReqAnyFinishedQuests = new List<uint>(2);

		/// <summary>
		/// Quests of which
		/// </summary>
		[NotPersistent]
		public readonly List<uint> ReqUndoneQuests = new List<uint>(2);
		#endregion

		#region QuestObjectives

		/// <summary>
		/// Triggers or areas which are objective to be explored as requirements.
		/// </summary>
		[NotPersistent]
		public uint[] AreaTriggerObjectives = new uint[0];

		/// <summary>
		/// Spell which are needed to be cast as a requirement.
		/// </summary>
		[Persistent(QuestConstants.MaxObjectInteractions)]
		public SpellId[] SpellCastObjectives = new SpellId[4];
		#endregion

		#region QuestRewards
		/// <summary>
		/// Array of <see href="ReputationReward">ReputationRewards</see>
		/// </summary>
		[Persistent(QuestConstants.MaxReputations)]
		public ReputationReward[] RewardReputations = new ReputationReward[5];

		#endregion

		#region QuestEmotes
		public uint OfferRewardEmoteDelay;
		public EmoteType OfferRewardEmoteType;

		public uint RequestItemsEmoteDelay;
		public EmoteType RequestItemsEmoteType;

		public EmoteType RequestEmoteType;

		[Persistent(QuestConstants.MaxEmotes)]
		public EmoteTemplate[] QuestDetailedEmotes = new EmoteTemplate[4];

		[Persistent(QuestConstants.MaxEmotes)]
		public EmoteTemplate[] OfferRewardEmotes = new EmoteTemplate[4];

		#endregion

		/// <summary>
		/// Value indicating whether this <see cref="QuestTemplate"/> is shareable.
		/// </summary>
		public bool Sharable
		{
			get { return Flags.HasFlag(QuestFlags.Sharable); }
		}

		/// <summary>
		/// Value indicating whether this <see cref="QuestTemplate"/> is daily.
		/// </summary>
		public bool IsDaily
		{
			get { return Flags.HasFlag(QuestFlags.Daily); }
		}

		#region Modify Templates
		/// <summary>
		/// To finish this Quest the Character has to interact with the given
		/// kind of GO the given amount of times.
		/// </summary>
		/// <param name="goId"></param>
		/// <param name="amount"></param>
		public void AddGOInteraction(GOEntryId goId, int amount)
		{
			int goIndex;
			if (GOInteractions == null)
			{
				goIndex = 0;
				GOInteractions = new QuestInteractionTemplate[1];
			}
			else
			{
				goIndex = GOInteractions.Length;
				Array.Resize(ref GOInteractions, goIndex + 1);
			}

			var index = ObjectInteractions.GetFreeIndex();

			var templ = new QuestInteractionTemplate
			{
				Index = index,
				Amount = amount,
				TemplateId = (uint)goId,
				Type = ObjectTypeId.GameObject
			};
			ArrayUtil.Set(ref ObjectInteractions, index, templ);
			GOInteractions[goIndex] = templ;
		}

		public void AddInitialItem(ItemId id, int amount)
		{
			InitialItems.Add(new ItemStackDescription(id, amount));
		}

		public void AddAreaTriggerObjective(uint id)
		{
			ArrayUtil.AddOnlyOne(ref AreaTriggerObjectives, id);
		}

		/// <summary>
		/// Used by SpellEffectType.QuestComplete
		/// </summary>
		/// <param name="id"></param>
		internal void AddSpellCastObjective(SpellId id)
		{
			ArrayUtil.AddOnlyOne(ref SpellCastObjectives, id);
		}

		/// <summary>
		/// To finish this Quest the Character has to interact with the given
		/// kind of NPC the given amount of times.
		/// </summary>
		/// <param name="npcid"></param>
		/// <param name="amount"></param>
		public void AddNPCInteraction(NPCId npcid, int amount)
		{
			int npcIndex;
			if (NPCInteractions == null)
			{
				npcIndex = 0;
				NPCInteractions = new QuestInteractionTemplate[1];
			}
			else
			{
				npcIndex = NPCInteractions.Length;
				Array.Resize(ref NPCInteractions, npcIndex + 1);
			}

			var index = ObjectInteractions.GetFreeIndex();

			var templ = new QuestInteractionTemplate
			{
				Index = index,
				Amount = amount,
				TemplateId = (uint)npcid,
				Type = ObjectTypeId.Unit
			};
			ArrayUtil.Set(ref ObjectInteractions, index, templ);
			NPCInteractions[npcIndex] = templ;
		}
		#endregion

		#region Requirements
		/// <summary>
		/// Checks whether the given Character may do this Quest
		/// </summary>
		public QuestInvalidReason CheckBasicRequirements(Character chr)
		{
			if (RequiredRaces != 0 && !RequiredRaces.HasAnyFlag(chr.RaceMask))
			{
				return QuestInvalidReason.WrongRace;
			}
			if (RequiredClass != 0 && RequiredClass != chr.Class)
			{
				return QuestInvalidReason.WrongClass;
			}

			if (RequiredSkill != SkillId.None && RequiredSkill > 0 &&
				!chr.Skills.CheckSkill(RequiredSkill, (int)RequiredSkillValue))
			{
				return QuestInvalidReason.NoRequirements;

			}

			var err = CheckActiveQuests(chr.QuestLog);
			if (err != QuestInvalidReason.Ok)
			{
				return err;
			}

			err = CheckFinishedQuests(chr.QuestLog);
			if (err != QuestInvalidReason.Ok)
			{
				return err;
			}

			if (IsDaily && (chr.QuestLog.CurrentDailyCount >= QuestLog.MaxDailyQuestCount))
			{
				return QuestInvalidReason.TooManyDailys;
			}

			if (chr.Account.ClientId < RequiredClientId)
			{
				return QuestInvalidReason.NoExpansionAccount;
			}

			if (chr.QuestLog.TimedQuestSlot != null)
			{
				return QuestInvalidReason.AlreadyOnTimedQuest;
			}
			if (chr.Level < RequiredLevel)
			{
				return QuestInvalidReason.LowLevel;
			}
			if (RewMoney < 0 && chr.Money < -RewMoney)
			{
				return QuestInvalidReason.NotEnoughMoney;
			}
			//TimeOut = 27 how the heck to work with this one?

			return QuestInvalidReason.Ok;
		}

		private QuestInvalidReason CheckActiveQuests(QuestLog log)
		{
			for (int i = 0; i < ReqAllActiveQuests.Count; i++)
			{
				var preqId = ReqAllActiveQuests[i];
				if (!log.HasActiveQuest(preqId))
				{
					return QuestInvalidReason.NoRequirements;
				}
			}

			if (ReqAnyActiveQuests.Count > 0)
			{
				var found = false;
				for (int i = 0; i < ReqAnyActiveQuests.Count; i++)
				{
					var preqId = ReqAnyActiveQuests[i];
					if (log.HasActiveQuest(preqId))
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					return QuestInvalidReason.NoRequirements;
				}
			}
			return QuestInvalidReason.Ok;
		}

		private QuestInvalidReason CheckFinishedQuests(QuestLog log)
		{
			for (int i = 0; i < ReqAllFinishedQuests.Count; i++)
			{
				var preqId = ReqAllFinishedQuests[i];
				if (!log.FinishedQuests.Contains(preqId))
				{
					return QuestInvalidReason.NoRequirements;
				}
			}

			if (ReqAnyActiveQuests.Count > 0)
			{
				var found = false;
				for (int i = 0; i < ReqAnyFinishedQuests.Count; i++)
				{
					var preqId = ReqAnyFinishedQuests[i];
					if (log.FinishedQuests.Contains(preqId))
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					return QuestInvalidReason.NoRequirements;
				}
			}

			if (ReqUndoneQuests.Count > 0)
			{
				for (var i = 0; i < ReqUndoneQuests.Count; i++)
				{
					var preqId = ReqUndoneQuests[i];
					if (log.FinishedQuests.Contains(preqId))
					{
						return QuestInvalidReason.NoRequirements;
					}
				}
			}
			return QuestInvalidReason.Ok;
		}

		/// <summary>
		/// Determines whether is quest obsolete for given character.
		/// </summary>
		/// <param name="chr">The character.</param>
		/// <returns>
		/// 	<c>true</c> if [is quest obsolete] [the specified qt]; otherwise, <c>false</c>.
		/// </returns>
		public bool IsObsolete(Character chr)
		{
			return chr.Level >= RequiredLevel + QuestMgr.LevelObsoleteOffset;
		}

		/// <summary>
		/// Determines whether [is quest too high level] [the specified qt].
		/// </summary>
		/// <param name="chr">The CHR.</param>
		/// <returns>
		/// 	<c>true</c> if [is quest too high level] [the specified qt]; otherwise, <c>false</c>.
		/// </returns>
		public bool IsTooHighLevel(Character chr)
		{
			return chr.Level + QuestMgr.LevelRequirementOffset < RequiredLevel;
		}
		#endregion

		#region Status
		/// <summary>
		/// Checks the requirements and returns the QuestStatus for ending a Quest.
		/// </summary>
		public QuestStatus GetStartStatus(QuestHolderInfo qh, Character chr)
		{
			var quest = chr.QuestLog.GetActiveQuest(Id);
			if (quest != null)
			{
				return QuestStatus.NotAvailable;
			}

			if (!Repeatable && chr.QuestLog.FinishedQuests.Contains(Id))
			{
				return QuestStatus.NotAvailable;
			}

			var status = CheckBasicRequirements(chr);
			if (status == QuestInvalidReason.LowLevel)
			{
				return QuestStatus.TooHighLevel;
			}

			if (status != QuestInvalidReason.Ok)
			{
				return QuestStatus.NotAvailable;
			}

			if (Repeatable)
			{
				return QuestStatus.Repeatable;
			}

			return IsObsolete(chr) ? QuestStatus.Obsolete : QuestStatus.Available;
		}

		/// <summary>
		/// </summary>
		public QuestStatus GetAvailability(Character chr)
		{
			var status = CheckBasicRequirements(chr);
			if (status == QuestInvalidReason.LowLevel)
			{
				return QuestStatus.TooHighLevel;
			}

			if (IsObsolete(chr))
			{
				return Repeatable ? QuestStatus.Repeatable : QuestStatus.Obsolete;
			}

			return Repeatable ? QuestStatus.Repeatable : QuestStatus.Available;
		}

		/// <summary>
		/// Checks the requirements and returns the QuestStatus for ending a Quest.
		/// </summary>
		/// <param name="chr">The client.</param>
		/// <returns></returns>
		public QuestStatus GetEndStatus(Character chr)
		{
			var quest = chr.QuestLog.GetActiveQuest(Id);
			if (quest == null)
			{
				return QuestStatus.NotAvailable;
			}

			return quest.Status;
		}

		#endregion

		#region QuestScripts

		// nothing yet

		#endregion

		#region Starters and Finishers
		/// <summary>
		/// Returns the GOEntry with the given id or null
		/// </summary>
		public GOEntry GetStarter(GOEntryId id)
		{
			for (var i = 0; i < Starters.Count; i++)
			{
				var starter = Starters[i];
				if (starter is GOEntry && starter.Id == (uint)id)
				{
					return (GOEntry)starter;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the NPCEntry with the given id or null
		/// </summary>
		public NPCEntry GetStarter(NPCId id)
		{
			for (var i = 0; i < Starters.Count; i++)
			{
				var starter = Starters[i];
				if (starter is NPCEntry && starter.Id == (uint)id)
				{
					return (NPCEntry)starter;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the ItemTemplate with the given id or null
		/// </summary>
		public ItemTemplate GetStarter(ItemId id)
		{
			for (var i = 0; i < Starters.Count; i++)
			{
				var starter = Starters[i];
				if (starter is ItemTemplate && starter.Id == (uint)id)
				{
					return (ItemTemplate)starter;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the Starter of the given Type which has the given Id
		/// </summary>
		public T GetStarter<T>(uint id)
			where T : IQuestHolderEntry
		{
			for (var i = 0; i < Starters.Count; i++)
			{
				var starter = Starters[i];
				if (starter is T && starter.Id == id)
				{
					return (T)starter;
				}
			}
			return default(T);
		}

		/// <summary>
		/// Returns the GOEntry with the given id or null
		/// </summary>
		public GOEntry GetFinisher(GOEntryId id)
		{
			for (var i = 0; i < Finishers.Count; i++)
			{
				var finisher = Finishers[i];
				if (finisher is GOEntry && finisher.Id == (uint)id)
				{
					return (GOEntry)finisher;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the NPCEntry with the given id or null
		/// </summary>
		public NPCEntry GetFinisher(NPCId id)
		{
			for (var i = 0; i < Finishers.Count; i++)
			{
				var finisher = Finishers[i];
				if (finisher is NPCEntry && finisher.Id == (uint)id)
				{
					return (NPCEntry)finisher;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the Finisher of the given Type which has the given Id
		/// </summary>
		public T GetFinisher<T>(uint id)
			where T : IQuestHolderEntry
		{
			for (var i = 0; i < Finishers.Count; i++)
			{
				var finisher = Finishers[i];
				if (finisher is T && finisher.Id == id)
				{
					return (T)finisher;
				}
			}
			return default(T);
		}
		#endregion

		#region Interactions
		/// <summary>
		/// Tries to give all Initial Items (or none at all).
		/// </summary>
		/// <remarks>If not all Initial Items could be given, the Quest cannot be started.</remarks>
		/// <param name="receiver"></param>
		/// <returns>Whether initial Items were given.</returns>
		public bool GiveInitialItems(Character receiver)
		{
			if (InitialItems.Count > 0)
			{
				var err = receiver.Inventory.TryAddAll(InitialItems.ToArray());
				if (err != InventoryError.OK)
				{
					ItemHandler.SendInventoryError(receiver.Client, null, null, err);
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Tries to give all Rewards (or none at all).
		/// </summary>
		/// <remarks>If not all Rewards could be given, the Quest remains completable.</remarks>
		/// <param name="receiver"></param>
		/// <param name="qHolder"></param>
		/// <param name="rewardSlot">The slot of choosable items</param>
		/// <returns>Whether Rewards were given.</returns>
		public bool TryGiveRewards(Character receiver, IQuestHolder qHolder, uint rewardSlot)
		{
			if (RewMoney < 0 && receiver.Money - RewMoney < 0)
			{
				QuestHandler.SendRequestItems(qHolder, this, receiver, true);
				return false;
			}

			return GiveRewards(receiver, rewardSlot);
		}

		public bool GiveRewards(Character receiver, uint rewardSlot)
		{
			ItemStackDescription[] items;
			var rewardItemCount = RewardItems.Length;
			if (rewardSlot < RewardChoiceItems.Length)
			{
				items = new ItemStackDescription[rewardItemCount + 1];
				Array.Copy(RewardItems, items, rewardItemCount);
				items[rewardItemCount] = RewardChoiceItems[rewardSlot];
			}
			else
			{
				items = RewardItems;
			}

			var err = receiver.Inventory.TryAddAll(items);
			if (err != InventoryError.OK)
			{
				ItemHandler.SendInventoryError(receiver.Client, null, null, err);
				return false;
			}

			if (!Repeatable)
			{
				if (receiver.Level >= RealmServerConfiguration.MaxCharacterLevel)
				{
					receiver.Money += MoneyAtMaxLevel;
				}
				else
				{
					receiver.GainXp(CalcRewardXp(receiver), false);
				}
			}

			if (RewMoney > 0)
			{
				receiver.Money = (uint)(receiver.Money + RewMoney);
			}

			for (var i = 0; i < QuestConstants.MaxReputations; i++)
			{
				if (RewardReputations[i].Faction != 0)
				{
				    var value = CalcRewRep(RewardReputations[i].ValueId, RewardReputations[i].Value);
					receiver.Reputations.GainReputation(RewardReputations[i].Faction, value);
				}
			}
            if (RewardTitleId != TitleId.None)
            {
                receiver.SetTitle(RewardTitleId, false);
            }
		    return true;
		}

        public int CalcRewRep(int valueId, int value)
        {
            if (value != 0)
                return value*100;

            var index = (valueId > 0) ? 0 : 1; 
            return QuestMgr.QuestRewRepInfos[index].RewRep[valueId-1];
        }

        public int CalcRewardXp(Character character)
        {
            var info = QuestMgr.QuestXpInfos.Get(Level);
        	int fullxp;
			if (info != null)
			{
				fullxp = info.RewXP.Get((uint)RewXPId - 1u);
			}
			else
			{
				// TODO: What to do with quests with funky levels
				fullxp = (int) (MinLevel*100);
			}
            fullxp = (fullxp*character.QuestExperienceGainModifierPercent/100);

            int playerLevel = character.Level;
            
            if (playerLevel <= Level + 5)
            {
            	return fullxp;
            }
            if (playerLevel == Level + 6)
            {
            	return (fullxp*8)/10;
            }
            if (playerLevel == Level + 7)
            {
            	return (fullxp*6)/10;
            }
            if (playerLevel == Level + 8)
            {
            	return (fullxp *4)/10;		// 0.4f
            }
            if (playerLevel == Level + 9)
            {
                return fullxp / 5;
            }
            return fullxp / 10;
        }

		#endregion

		#region Dump
		public void Dump(IndentTextWriter writer)
		{
			//writer.WriteLine(this);
			writer.WriteLineNotDefault(QuestType, "Type: " + QuestType);
			writer.WriteLineNotDefault(Flags, "Flags: " + Flags);
			writer.WriteLineNotDefault(RequiredLevel, "Required Level: " + RequiredLevel);
			writer.WriteLineNotDefault(RequiredRaces, "Races: " + RequiredRaces);
			writer.WriteLineNotDefault(RequiredClass, "Class: " + RequiredClass);
			writer.WriteLineNotDefault(InitialItems.Count, "Provided Items: " + InitialItems.ToString(", "));
			writer.WriteLineNotDefault(Starters.Count, "Starts at: " + Starters.ToString(", "));
			writer.WriteLineNotDefault(Finishers.Count, "Ends at: " + Finishers.ToString(", "));

			var interactions = ObjectInteractions.Where(action => action != null && action.TemplateId > 0);
			writer.WriteLineNotDefault(interactions.Count(), "Interactions: " + interactions.ToString(", "));

			if (CollectableItems != null && CollectableItems.Length > 0)
			{
				writer.WriteLine("Collectables: " + CollectableItems.ToString(", "));
			}

			writer.WriteLineNotDefault(SpellCastObjectives.Length,
				"Req Spells: " + SpellCastObjectives.TransformArray(id => (SpellId) id + " (" + id + ")").ToString(", "));

			writer.WriteLineNotDefault(AreaTriggerObjectives.Length, 
				"Req AreaTriggers: " + AreaTriggerObjectives.TransformArray(id => AreaTriggerMgr.GetTrigger(id)).ToString(", "));

			if (Instructions != null)
			{
				var ins = Instructions.Where(obj => !string.IsNullOrEmpty(obj));
				writer.WriteLineNotDefault(ins.Count(), "Instructions: " + ins.ToString(" / ") + "");
			}
			writer.WriteLineNotDefault(FollowupQuestId, "Next quest: " + QuestMgr.GetTemplate(FollowupQuestId));
		}

		#endregion

		public override string ToString()
		{
			return DefaultTitle + " (Id: " + Id + ")";
		}

		#region Events
		internal void NotifyStarted(Quest quest)
		{
			var evt = QuestStarted;
			if (evt != null)
			{
				evt(quest);
			}
		}

		internal void NotifyFinished(Quest quest)
		{
			var evt = QuestFinished;
			if (evt != null)
			{
				evt(quest);
			}
		}

		internal void NotifyCancelled(Quest quest, bool failed)
		{
			var evt = QuestCancelled;
			if (evt != null)
			{
				evt(quest, failed);
			}
		}

		internal void NotifyNPCInteracted(Quest quest, NPC npc)
		{
			var evt = NPCInteracted;
			if (evt != null)
			{
				evt(quest, npc);
			}
		}

		internal void NotifyGOUsed(Quest quest, GameObject go)
		{
			var evt = GOInteraction;
			if (evt != null)
			{
				evt(quest, go);
			}
		}
		#endregion

		public static IEnumerable<QuestTemplate> GetAllDataHolders()
		{
			return QuestMgr.Templates;
		}

		#region Deserialization
		public void FinalizeDataHolder()
		{
			if (ReqSkillOrClass > 0)
			{
				// skill
				RequiredSkill = (SkillId)ReqSkillOrClass;
			}
			else
			{
				// class
				RequiredClass = (ClassId)ReqSkillOrClass;
			}

			if (Category < 0)
			{
				// QuestSort
				var clss = ((QuestSort)(-Category)).GetClassId();
				if (clss != ClassId.End)
				{
					RequiredClass = clss;
				}
			}
			else if (Category > 0)
			{
				ZoneTemplate = World.GetZoneInfo((ZoneId)Category);
			}

			List<QuestInteractionTemplate> goInteractions = null;
			List<QuestInteractionTemplate> npcInteractions = null;
			for (uint i = 0; i < ObjectInteractions.Length; i++)
			{
				if (ObjectInteractions[i].TemplateId == 0 || ObjectInteractions[i].Amount == 0)
				{
					// AllInteractions[i] = default(QuestInteractionTemplate);
				}
				else
				{
					if ((ObjectInteractions[i].TemplateId & QuestConstants.GOIndicator) != 0)
					{
						ObjectInteractions[i].TemplateId &= ~QuestConstants.GOIndicator;
						ObjectInteractions[i].Type = ObjectTypeId.GameObject;
						(goInteractions = goInteractions.NotNull()).Add(ObjectInteractions[i]);
					}
					else
					{
						ObjectInteractions[i].Type = ObjectTypeId.Unit;
						(npcInteractions = npcInteractions.NotNull()).Add(ObjectInteractions[i]);
					}
					ObjectInteractions[i].Index = i;
				}
			}

			//if (AreaTriggerObjectiveIds == null)
			//{
			//    AreaTriggerObjectiveIds = new uint[0];
			//}
			//else
			//{
			//    ArrayUtil.PruneVals(ref AreaTriggerObjectiveIds);
			//    for (var i = 0; i < AreaTriggerObjectiveIds.Length; i++)
			//    {
			//        var triggerId = AreaTriggerObjectiveIds[i];
			//        var trigger = AreaTriggerMgr.GetTrigger(triggerId);
			//        if (trigger == null)
			//        {
			//            ContentHandler.OnInvalidDBData("Invalid AreaTrigger {0} in Quest: " + this, triggerId);
			//        }
			//        else
			//        {
			//            trigger.TriggerQuest = this;
			//        }
			//    }
			//}

			if (SrcItemId != 0)
			{
				InitialItems.Add(new ItemStackDescription(SrcItemId, 1));
			}

			// make sure that provided items are not required
			var colItems = new List<ItemStackDescription>(4);
			for (var i = 0; i < CollectableItems.Length; i++)
			{
				var item = CollectableItems[i];
				if (item.ItemId == 0 || InitialItems.Find(stack => stack.ItemId == item.ItemId).ItemId != 0)
				{
					continue;
				}
				colItems.Add(item);
			}
			CollectableItems = colItems.ToArray();

			if (goInteractions != null)
			{
				GOInteractions = goInteractions.ToArray();
			}
			if (npcInteractions != null)
			{
				NPCInteractions = npcInteractions.ToArray();
			}

			if (SpellCastObjectives != null)
			{
				ArrayUtil.PruneVals(ref SpellCastObjectives);
			}

			ArrayUtil.PruneVals(ref AreaTriggerObjectives);
			ArrayUtil.PruneVals(ref RewardChoiceItems);
			ArrayUtil.PruneVals(ref RewardItems);

			QuestMgr.AddQuest(this);
		}
		#endregion
	}

	/// <summary>
	/// Consists of a Type of objects, an id of the object's Template and
	/// amount of objects to be searched for in order to complete a <see cref="Quest"/>.
	/// </summary>
	public class QuestInteractionTemplate
	{
		public uint TemplateId;
		public int Amount;

		/// <summary>
		/// Either <see cref="ObjectTypeId.Unit"/> or <see cref="ObjectTypeId.GameObject"/>
		/// </summary>
		[NotPersistent]
		public ObjectTypeId Type;

		[NotPersistent]
		public uint Index;

		public override string ToString()
		{
			return (Amount != 1 ? Amount + "x " : "") + Type + " " + Type.ToString(TemplateId);
		}

		/// <summary>
		/// The RawId is used in certain Packets
		/// </summary>
		public uint RawId
		{
			get
			{
				return Type == ObjectTypeId.GameObject ?
					//(uint)-(int)(TemplateId | QuestConstants.GOIndicator) : TemplateId;
					uint.MaxValue-(TemplateId) : TemplateId;
			}
		}
	}

	public struct EmoteTemplate
	{
		public uint Count;
		public uint Delay;
		public EmoteType Type;
	}

	public struct ReputationReward
	{
		//public FactionReputationIndex Faction;
		public FactionId Faction;
		public int ValueId;
	    public int Value;
	}
}