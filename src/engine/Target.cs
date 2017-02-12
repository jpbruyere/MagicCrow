using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace MagicCrow
{
	//TODO: permanent, self and enchanted by should go to CardTarget
	//		possible target should be player or card
    public enum TargetType
    {
        Opponent,
        Player,
        Card,
        Permanent,
		Self,
		EnchantedBy,
		EquipedBy,
		Attached,
		Kicked,
		Spell,
		Activated,
		Triggered,
		Charge
    }
    public enum ControlerType
    {
        All,
        You,
        Opponent,
		Targeted,
		ChosenAndYou,
		TargetedController,
		TriggeredCardController,
		TriggeredTargetController,
		TriggeredSourceController,
		TriggeredAttackingPlayer,
		TriggeredActivator,
		TriggeredPlayer,
		IsNotRemembered,
    }
    public enum CombatImplication
    {
        Unset,
        Attacking,
        Blocking
    }
    public enum NumericRelations
    {
        Equal,
        Greater,
        Less,
        LessOrEqual,
        GreaterOrEqual,
        NotEqual
    }
	[Serializable]
	public class NumericConstrain
    {
        public NumericRelations Relation;
        public int Value;
    }
	[Serializable]	
    public class Target
    {
        public TargetType TypeOfTarget;
		//should have defining source to know opponent...

		public virtual bool Accept(object _target, CardInstance _source){
			switch (TypeOfTarget) {
			case TargetType.Spell:
				return _target is Spell;
			case TargetType.Activated:
				AbilityActivation aa = _target as AbilityActivation;
				return aa == null ? false : aa.Source.IsActivatedAbility;
			case TargetType.Triggered:
				AbilityActivation aaa = _target as AbilityActivation;
				return aaa == null ? false : aaa.Source.IsTriggeredAbility;
			case TargetType.Opponent:
				Player p = _target as Player;
				if (p == null)
					return false;				
				break;
			case TargetType.Player:
				return (_target is Player);
			case TargetType.Card:
				return (_target is CardInstance);
			case TargetType.Permanent:
				CardInstance c = _target as CardInstance;
				if (c == null)
					return false;
				return (c.CurrentGroup.GroupName == CardGroupEnum.InPlay);
			case TargetType.Self:
				return _target == _source;
			case TargetType.EquipedBy:
				if (_target is CardInstance)
					return  (_target as CardInstance).AttachedTo == _source;
				return false;
			case TargetType.EnchantedBy:
				if (_target is CardInstance)
					return  (_target as CardInstance).AttachedTo == _source;
				return false;
			}
			return false;
		}
//		static List<string> list = new List<String> ();

        public static MultiformAttribut<Target> ParseTargets(string str)
        {
            MultiformAttribut<Target> result = new MultiformAttribut<Target>(AttributeType.Choice);

			string[] tmp = str.Trim().Split(new char[] { ',' });
			foreach (string t in tmp)
			{
				if (string.IsNullOrWhiteSpace(t))
					continue;

				string[] cardTypes = t.Trim().Split(new char[] { '.', '+' });

				switch (cardTypes[0].Trim())
				{
				case "Opponent":
					result |= TargetType.Opponent;
					break;
				case "Player":
					result |= TargetType.Player;
					break;
				case "CHARGE":
					result |= TargetType.Charge;
					break;
				default:
					CardTarget ctar = new CardTarget();

					foreach (string ct in cardTypes)
					{
						switch (ct) {
						case "Card":
							break;
						case "Permanent":
							ctar.TypeOfTarget = TargetType.Permanent;
							break;
						case "YouCtrl":
							ctar.Controler = ControlerType.You;
							break;
						case "YouDontCtrl":
							ctar.Controler = ControlerType.Opponent;
							break;
						case "OppCtrl":
							ctar.Controler = ControlerType.Opponent;
							break;
						case "Other":
							ctar.CanBeTargetted = false;
							break;
						case "TypeYouCtrl":
							ctar.TypeOfTarget = TargetType.Permanent;
							ctar.Controler = ControlerType.You;
							break;
						case "attacking":
							ctar.CombatState = CombatImplication.Attacking;
							break;
						case "blocking":
							ctar.CombatState = CombatImplication.Blocking;
							break;
						case "CARDNAME":
						case "Self":
							ctar.TypeOfTarget = TargetType.Self;
							break;
						case "EnchantedBy":
							ctar.TypeOfTarget = TargetType.EnchantedBy;
							break;
						case "equipped":
							ctar.HavingAttachedCards += CardTypes.Equipment;
							break;
						case "kicked":
							ctar.TypeOfTarget = TargetType.Kicked;
							break;
						case "Attached":
							ctar.TypeOfTarget = TargetType.Attached;
							break;
						case "EquippedBy":
							ctar.TypeOfTarget = TargetType.EquipedBy;
							break;
						case "White":
							ctar.ValidCardColors += ManaTypes.White;
							break;
						default:							
							#region ability inclusion/exclusion
							if (ct.StartsWith("without")){
								ctar.WithoutAbilities += Ability.ParseKeyword(ct.Substring(7));
								break;
							}
							if (ct.StartsWith ("with")){
								ctar.HavingAbilities += Ability.ParseKeyword(ct.Substring(4));
								break;
							}
							#endregion
							#region numeric contrain
							NumericConstrain nc = null;
							string strTmp = "";
							if (ct.ToLower().StartsWith("power"))
							{
								ctar.PowerConstrain = new NumericConstrain();
								nc = ctar.PowerConstrain;
								strTmp = ct.Substring(5);
							}
							else if (ct.ToLower().StartsWith("toughness"))
							{
								ctar.ToughnessConstrain = new NumericConstrain();
								nc = ctar.ToughnessConstrain;
								strTmp = ct.Substring(9);
							}

							if (nc != null)
							{
								string strRelation = strTmp.Substring(0, 2);
								switch (strRelation)
								{
								case "EQ":
									nc.Relation = NumericRelations.Equal;
									break;
								case "LT":
									nc.Relation = NumericRelations.Less;
									break;
								case "LE":
									nc.Relation = NumericRelations.LessOrEqual;
									break;
								case "GT":
									nc.Relation = NumericRelations.Greater;
									break;
								case "GE":
									nc.Relation = NumericRelations.GreaterOrEqual;
									break;
								case "NE":
									nc.Relation = NumericRelations.NotEqual;
									break;
								default:
									break;
								}
								strTmp = strTmp.Substring(2);

								if (strTmp != "X" && !string.IsNullOrWhiteSpace(strTmp))
									nc.Value = int.Parse(strTmp);

								break;
							}
							#endregion
							#region card types constrains
							CardTypes cts;
							if (Enum.TryParse<CardTypes>(ct, true, out cts))
								ctar.ValidCardTypes += (CardTypes)Enum.Parse(typeof(CardTypes), ct, true);
							else
								Debug.WriteLine ("Unknow card type: " + ct);
							#endregion
							break;
						}							
					}
					result |= ctar;
					break;
				}
            }
				
            return result;
        }

		#region Operators
        public static implicit operator Target(TargetType tt)
        {
            return tt == TargetType.Card || tt == TargetType.Permanent ?
                new Target { TypeOfTarget = tt } :
                new CardTarget { TypeOfTarget = tt };
        }
        public static bool operator ==(Target ct, CardTypes t)
        {
            return ct is CardTarget ? (ct as CardTarget) == t : false;
        }
        public static bool operator !=(Target ct, CardTypes t)
        {
            return ct is CardTarget ? (ct as CardTarget) != t : true;
        }
		#endregion

		public override string ToString()
        {
            return TypeOfTarget.ToString();
        }
    }
			
}
