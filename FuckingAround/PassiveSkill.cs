using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class PassiveSkill {
		public List<Mod> _mods;
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
			new PassiveSkill(new Mod(StatType.Strength, ModifyingMethod.Add, 10)),
			new PassiveSkill(new Mod(StatType.Speed, ModifyingMethod.Add, 5)),
			new PassiveSkill(new Mod(StatType.HP, ModifyingMethod.Add, 20)),
			new PassiveSkill(new Mod(StatType.ChannelingSpeed, ModifyingMethod.Add, 4)),
			new PassiveSkill(new Mod(StatType.PhysicalDamage, ModifyingMethod.Convert|ModifyingMethod.Multiply, 0.1/2){ ConversionSource = StatType.Strength})	//10% increase in physical damage per 2 strength
		};
		public static PassiveSkill[] All = new PassiveSkill[]{
			new PassiveSkill(new Mod( StatType.Strength, ModifyingMethod.Add, 10 )),	//Increases strength by 10
			new PassiveSkill(new Mod( StatType.Strength, ModifyingMethod.Multiply, 0.20 )),	//Increases strength by 20%
			new PassiveSkill(new Mod( StatType.Strength, ModifyingMethod.Convert, 0.1 ){ ConversionSource = StatType.PhysicalDamage}),	//gain 10% of physical damage as bonus fire damage
			new PassiveSkill(new Mod[]{	//Converts 50% of PhysicalDamage to FireDamage
				new Mod( StatType.PhysicalDamage, ModifyingMethod.Multiply, -0.5),	//TODO Should be applied after regular Multiplying stuff
				new Mod( StatType.FireDamage, ModifyingMethod.Convert | ModifyingMethod.Add, /*1 instead of 0.5 because mod above is applied first*/1){ ConversionSource = StatType.PhysicalDamage }
			})
		};
	}
}