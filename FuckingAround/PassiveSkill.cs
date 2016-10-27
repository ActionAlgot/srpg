using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class PassiveSkill {
		private List<Mod> _mods;
		public IEnumerable<Mod> Mods { get { return _mods; } }
		public PassiveSkill(Mod mod) {
			_mods = new List<Mod>{ mod };
		}
		public PassiveSkill(IEnumerable<Mod> mods) {
			_mods = mods.ToList();
		}
	}

	public static class Passives {
		public static IEnumerable<PassiveSkill> Default = new PassiveSkill[]{
			new PassiveSkill(new Mod( StatType.Strength, ModifyingMethod.Add, 10)),
			new PassiveSkill(new Mod( StatType.Speed, ModifyingMethod.Add, 5)),
			new PassiveSkill(new Mod( StatType.HP, ModifyingMethod.Add, 20)),
			new PassiveSkill(new Mod( StatType.ChannelingSpeed, ModifyingMethod.Add, 4)),
			new PassiveSkill(new Mod( StatType.PhysicalDamage, ModifyingMethod.Convert|ModifyingMethod.AdditiveMultiply, 0.05){ ConversionSource = StatType.Strength})	//5% increase in physical damage for each point in strength
		};
		public static PassiveSkill[] All = new PassiveSkill[]{
			new PassiveSkill(new Mod( StatType.Strength, ModifyingMethod.Add, 10 )),	//Increases strength by 10
			new PassiveSkill(new Mod( StatType.Strength, ModifyingMethod.AdditiveMultiply, 0.20 )),	//Increases strength by 20%
			new PassiveSkill(new Mod( StatType.Strength, ModifyingMethod.Convert|ModifyingMethod.Add, 0.10 ){ ConversionSource = StatType.PhysicalDamage}),	//gain 10% of physical damage as bonus fire damage
			new PassiveSkill(new Mod[]{	//Converts 50% of PhysicalDamage to FireDamage
				new Mod( StatType.PhysicalDamage, ModifyingMethod.Multiply, 0.50),	//TODO fix 100% conversion
				new Mod( StatType.FireDamage, ModifyingMethod.Convert|ModifyingMethod.Add, 0.50*(1/0.50)){ ConversionSource = StatType.PhysicalDamage }	//value is 1 rather than 0.5 because mod above is applied first
			}),
			new PassiveSkill(new Mod(StatType.FireResistance,ModifyingMethod.Add, 0.50))	//50% fire resistance
		};

	}
}