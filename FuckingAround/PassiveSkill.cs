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
			new PassiveSkill(new Mod(Stat.Strength, ModifyingMethod.Add, 10)),
			new PassiveSkill(new Mod(Stat.Speed, ModifyingMethod.Add, 5)),
			new PassiveSkill(new Mod(Stat.HP, ModifyingMethod.Add, 20)),
			new PassiveSkill(new Mod(Stat.ChannelingSpeed, ModifyingMethod.Add, 4))
		};
		public static PassiveSkill[] All = new PassiveSkill[]{
			new PassiveSkill(new Mod( Stat.Strength, ModifyingMethod.Add, 10 )),	//Increases strength by 10
			new PassiveSkill(new Mod( Stat.Strength, ModifyingMethod.Multiply, 0.20 )),	//Increases strength by 20%
			new PassiveSkill(new Mod[]{	//Converts 50% of PhysicalDamage to FireDamage
				new Mod( Stat.PhysicalDamage, ModifyingMethod.Multiply, -0.5),	//Should be applied after regular Multiplying stuff?
				new Mod( Stat.FireDamage, ModifyingMethod.Add, 0)	//Func<(Damage? Skill?), double>
			})
		};
	}
}