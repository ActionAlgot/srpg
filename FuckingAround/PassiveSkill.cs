using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class PassiveSkill {
		public Mod Mod { get; protected set; }
		public PassiveSkill(Mod mod) {
			Mod = mod;
		}
	}

	public class Mod {
		public List<string> Tags;	//ENUM list
		public object SomeShit;
	}

	public static class Passives {
		public static PassiveSkill[] All = new PassiveSkill[]{
			new PassiveSkill(new Mod{ Tags = new List<string>{"Stat", "Strength", "Base"}, SomeShit = 10 }),
			new PassiveSkill(new Mod{ Tags = new List<string>{"Stat", "Strength", "Multiply"}, SomeShit = 0.20 })
		};
	}
}