using System;
using System.Linq;
using OpenTK;
using System.Collections.Generic;

namespace MagicCrow
{
	public class InPlayGroup : CardGroup
	{
		public Player Controler;

		public CardLayout LandsLayout = new MagicCrow.LandsLayout();
		public CardLayout CreatureLayout = new CardLayout();
		public CardLayout OtherLayout = new CardLayout();
		public CardLayout CombatingCreature = new CardLayout();

		public InPlayGroup(Player _controler)
			: base(CardGroupEnum.InPlay)
		{
			Controler = _controler;
			y = -4.5f;
			HorizontalSpacing = 1.5f;
			MaxHorizontalSpace = 7f;

			LandsLayout.x = -1f;
			LandsLayout.y = -4.2f;
			LandsLayout.HorizontalSpacing = 1.4f;
			LandsLayout.VerticalSpacing = 0.01f;
			LandsLayout.MaxHorizontalSpace = 5f;

			CreatureLayout.x = -0f;
			CreatureLayout.y = -2.0f;
			CreatureLayout.HorizontalSpacing = 1.5f;
			CreatureLayout.VerticalSpacing = 0.01f;
			CreatureLayout.MaxHorizontalSpace = 8f;

			OtherLayout.x = 4f; 
			OtherLayout.y = -1.5f;
			OtherLayout.HorizontalSpacing = 1.5f;
			OtherLayout.VerticalSpacing = 0.01f;
			OtherLayout.MaxHorizontalSpace = 4f;

			CombatingCreature.x = -0;
			CombatingCreature.y = -0.7f;
			CombatingCreature.HorizontalSpacing = 1.5f;
			CombatingCreature.VerticalSpacing = 0.01f;
			CombatingCreature.MaxHorizontalSpace = 7f;
		}

		public override void AddCard (CardInstance c, bool anim = true)
		{
			base.AddCard (c, anim);

			if (c.HasType (CardTypes.Creature))
				c.CreatePointOverlay ();			
		}
		public override void RemoveCard (CardInstance c, bool anim = true)
		{
			base.RemoveCard (c, anim);
			if (c.HasType (CardTypes.Creature))
				c.RemovePointOverlay ();
		}
		public override void UpdateLayout(bool anim = true)
		{
			IList<CardInstance> uncontroledCards = Cards.Where (c => c.Controler != this.Controler).ToList();
			foreach (CardInstance uc in uncontroledCards) {
				Cards.Remove (uc);
				uc.Controler.InPlay.Cards.Add (uc);
				uc.CurrentGroup = uc.Controler.InPlay;
			}

			LandsLayout.Cards = Cards.Where(c => c.Model.Types == CardTypes.Land && !(c.IsAttached || c.Combating)).ToList();
			CreatureLayout.Cards = Cards.Where(c => c.Model.Types == CardTypes.Creature && !(c.IsAttached || c.Combating)).ToList();
			OtherLayout.Cards = Cards.Where(c => c.Model.Types != CardTypes.Land && c.Model.Types != CardTypes.Creature
				&& !(c.IsAttachedToACardInTheSameCamp || c.Combating)).ToList();

			LandsLayout.UpdateLayout(anim);
			CreatureLayout.UpdateLayout(anim);
			OtherLayout.UpdateLayout(anim);

			if (MagicEngine.CurrentEngine.CurrentPhase > GamePhases.BeforeCombat &&
				MagicEngine.CurrentEngine.CurrentPhase <= GamePhases.EndOfCombat)
			{
				CombatingCreature.Cards = Cards.Where(c => c.Model.Types == CardTypes.Creature && c.Combating).ToList();
				if (CombatingCreature.Cards.Count == 0)
					return;
				if (MagicEngine.CurrentEngine.cp == Cards[0].Controler)
					CombatingCreature.UpdateLayout(anim);
				else
					CombatingCreature.UpdateDefendersLayout();
			}
			//base.UpdateLayout();
		}
	}
}

