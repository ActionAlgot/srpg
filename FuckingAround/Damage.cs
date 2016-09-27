using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class Damage {
		public int PhysDmg;
		//public int FireDamage;	//All of this shit in same object?
		//public double ArmourPenetration;
		//etc
		public Damage(IEnumerable<Mod> mods) {
			PhysDmg = (int)mods.GetStat(StatType.PhysicalDamage);
			ConsoleLoggerHandlerOrWhatever.Log(PhysDmg + " Damage");
		}
	}
}
