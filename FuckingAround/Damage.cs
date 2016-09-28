using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class Damage {
		public int PhysDmg;
		public int FireDamage;
		//public double ArmourPenetration;
		//etc
		public Damage(IEnumerable<Mod> mods) {
			PhysDmg = (int)mods.GetStat(StatType.PhysicalDamage);
			ConsoleLoggerHandlerOrWhatever.Log(PhysDmg + " Damage");
			FireDamage = (int)mods.GetStat(StatType.FireDamage);
			ConsoleLoggerHandlerOrWhatever.Log(FireDamage + " FireDamage");

			foreach (StatType dmg in ((IEnumerable<StatType>)Enum.GetValues(typeof(StatType)))){
				if((dmg & StatType.Damage) == StatType.Damage && dmg != StatType.Damage){
					ConsoleLoggerHandlerOrWhatever.Log(dmg.ToString());
				}
			}
		}
	}
}
