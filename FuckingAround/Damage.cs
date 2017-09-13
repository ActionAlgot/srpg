using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace srpg {
	public class Damage {
		public int Value { get; private set; }

		public class StatDamage {
			public StatType Type { get; private set; }
			public double Damage { get; private set; }
			public StatDamage(double damage, StatType type) {
				Type = type; Damage = damage;
			} }
		public IEnumerable<StatDamage> statDamages { get; private set; }
		public Damage(Being defender, StatSet damages) {
			var statdmg = new List<StatDamage>();
			double total = 0.0;
			foreach (StatType dmgType in StatTypeStuff.DirectDamageTypeApplicationTypes) {
				double dmg = damages.GetStat(dmgType).Value;
				if (dmg != 0) {
					double resist = defender[dmgType.AsResistance()].Value;
					double penetration = damages[dmgType.AsPenetration()];
					double threshold = defender[dmgType.AsThreshold()].Value;
					dmg *= (1 - (resist - penetration));
					if (Math.Abs(dmg) < threshold) dmg = 0; //don't negate more than absolute damage
					else dmg -= (dmg < 0 ? -1 : 1) * threshold; //negate flat amount regardless of negative or positive damage
					ConsoleLoggerHandlerOrWhatever.Log(dmg + " " + dmgType);
					statdmg.Add(new StatDamage(dmg, dmgType));
					total += dmg;
				}
			}

			Value = (int)(total + 0.5);
		}

		public override string ToString() {
			return Value.ToString();
		}
	}
}
