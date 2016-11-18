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
			new PassiveSkill(new AdditionMod(StatType.Strength, 10)),
			new PassiveSkill(new AdditionMod(StatType.Speed, 5)),
			new PassiveSkill(new AdditionMod(StatType.HP, 20)),
			new PassiveSkill(new AdditionMod(StatType.ChannelingSpeed,  4)),
			new PassiveSkill(new ConversionToAdditiveMultiplierMod( StatType.PhysicalDamage, 0.05, StatType.Strength))	//5% increase in physical damage for each point in strength
		};
		public static PassiveSkill[] All = new PassiveSkill[]{
			/*0*/
			new PassiveSkill(new AdditionMod(StatType.Strength,  10)),	//Increases strength by 10
			new PassiveSkill(new AdditiveMultiplierMod(StatType.Strength, 0.20)),	//Increases strength by 20%
			new PassiveSkill(new GainMod(StatType.FireDamage, 0.10, StatType.PhysicalDamage)),	//gain 10% of physical damage as bonus fire damage
			new PassiveSkill(new ConversionMod(StatType.FireDamage, 0.50, StatType.PhysicalDamage)),	//Converts 50% of PhysicalDamage to FireDamage
			new PassiveSkill(new ConversionMod(StatType.PhysicalDamage, 0.50, StatType.FireDamage)),	//Converts 50% of FireDamage to PhysicalDamage
			/*5*/
			new PassiveSkill(new AdditionMod(StatType.FireResistance, 0.50))	//50% fire resistance
		};

	}
}