using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicCrow
{
    public interface IDamagable
    {
		bool HasCombatDamage { get; set; }
        void AddDamages(Damage d);

    }
	public class Damage : MagicStackElement
    {
		#region implemented abstract members of MagicStackElement
		public override string Title {
			get { return "Damages"; }
		}
		public override string Message {
			get { return string.Format(
					"{0} deals {1} Damage(s) to {2}", Source.Model.Name, Amount, Target.ToString()); }
		}
		public override string[] MSECostElements {get { return null; }}
		public override string[] MSEOtherCostElements {get { return null; }}
		public override Player Player {
			get {
				return Source.Controler;
			}
			set {
				throw new NotImplementedException ();
			}
		}
		#endregion

        public IDamagable Target;
        public CardInstance Source;
        public int Amount;
		public bool IsCombatDamage;

		public Damage(IDamagable _target, CardInstance _source, int _amount, bool isCombatDamage = false)
        {
            Target = _target;
            Source = _source;
            Amount = _amount;
			IsCombatDamage = isCombatDamage;
        }

        public void Deal()
        {
			if (Source.HasAbility(AbilityEnum.Lifelink)){
				Source.Controler.LifePoints += Amount;
			}
            if (Source.HasAbility(AbilityEnum.Trample) && Target is CardInstance)
            {
                CardInstance t = Target as CardInstance;
				Target.AddDamages (new Damage (t, Source, t.Toughness, IsCombatDamage));
                Amount -= t.Toughness;
                t.Controler.AddDamages(this);
            }else{
                Target.AddDamages(this);
            }
			if (IsCombatDamage && !Source.HasDealtCombatDamages)
				Source.HasDealtCombatDamages = true;
        }
    	public override string ToString ()
		{
			return string.Format ("{0} deals {1} damage to {2}", Source.Model.Name,Amount,Target.ToString());
		}
	}

    //public class DamageList : List<Damage>
    //{
    //    public Damage AddDamage(Damage d)
    //    {
            
    //        this.Add(d);



    //        return 
    //    }
    //}
}
