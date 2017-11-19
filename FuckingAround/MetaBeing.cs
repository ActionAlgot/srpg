using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace srpg {
	public class MetaBeing {
		public string Name { get; private set; }

		public SkillTree SkillTree { get { return SkillTreeshit.Basic; } }
		public SkillTreeFiller SkillTreeFilling;

		public Weapon Fist = new Weapon(2);
		public PersonalInventory Inventory { get; private set; }
		public Weapon MainHand { get { return Inventory.MainHand; } }
		public Gear OffHand { get { return Inventory.OffHand; } }

		public StatSet Stats { get; protected set; }
		public Stat this[StatType st] {
			get {
				if(Stats == null) {
					Stats = new StatSet();
					foreach (var m in Mods)
						m.Affect(Stats);
				}
				return Stats.GetStat(st);
		}	}
		public IEnumerable<Mod> Mods {
			get {
				return SkillTreeFilling.Taken
					.SelectMany(node => node.Mods)
					.Concat(Inventory
						.Where(g => g != null)
						.SelectMany(g => g.GlobalMods));
			}
		}

		private List<Skill> _skills = new List<Skill>();
		public IEnumerable<Skill> Skills {
			get { return _skills; } }
		public void AddSkill(Skill skill) {
			_skills.Add(skill); }
		public void AddSkills(IEnumerable<Skill> skills) {
			_skills.AddRange(skills); }
		public void RemoveSkill(Skill skill) {
			_skills.Remove(skill); }

		public MetaBeing(string name) {
			Name = name;
			SkillTreeFilling = new SkillTreeFiller(SkillTree);
			Inventory = new PersonalInventory(Fist);
		}
	}
}
