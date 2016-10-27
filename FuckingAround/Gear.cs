using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public abstract class Gear {
		public abstract IEnumerable<EquipmentMod> BaseMods { get; }
		public IEnumerable<EquipmentMod> Enchantments { get; protected set; }
		public virtual IEnumerable<Mod> GlobalMods { get {
			return BaseMods
				.Concat(Enchantments)
				.Where(em => em.Global)
				.Concat(ModdedMods);
		} }
		protected IEnumerable<Mod> PrivMods { get {
			return BaseMods
				.Concat(Enchantments)
				.Where(em => !em.Global);
		} }
		protected IEnumerable<Mod> ModdedMods {	//get private mods calculated into global add mods
			get {
				foreach (StatType stat in Enum.GetValues(typeof(StatType))) {
					var val = PrivMods.GetStat(stat);
					if (val != 0) yield return new Mod(stat, ModifyingMethod.Add, val);
		} } }
	}

	public class ArmourButNotNecessarilyArmour : Gear {
		private EquipmentMod BaseArmour;
		public override IEnumerable<EquipmentMod> BaseMods { get { yield return BaseArmour; } }
		public ArmourButNotNecessarilyArmour(int armour) {
			BaseArmour = new EquipmentMod(StatType.Armour, ModifyingMethod.Add, armour, false);
			Enchantments = new EquipmentMod[0];
		}
	}
	public class Shield : ArmourButNotNecessarilyArmour {
		public Shield(int armour) : base(armour) { }
	}

	public class Weapon : Gear {
		public bool TwoH;
		private EquipmentMod BaseDamage;
		public override IEnumerable<EquipmentMod> BaseMods { get { yield return BaseDamage; } }
		public virtual void Affect(Skill skill, SkillUser su, Tile target, Action<Skill, SkillUser, Tile> effect) {
			effect(skill.GetModdedInstance(PrivMods), su, target);
		}
		public virtual IEnumerable<Tile> Range(Skill skill, SkillUser su) {
			return su.Place.Adjacent;
		}
		public virtual IEnumerable<Tile> AoE(Skill skill, SkillUser su, Tile target) {
			yield return target;
		}
		public Weapon(int dmg) {
			BaseDamage = new EquipmentMod(StatType.PhysicalDamage, ModifyingMethod.Add, dmg, false);
			Enchantments = new EquipmentMod[0];
		}
	}

	public class ProjectileWeapon : Weapon {
		public ProjectileWeapon(int dmg) : base(dmg) { }
		public override IEnumerable<Tile> Range(Skill skill, SkillUser su) {
			return su.Place.GetArea((int)skill.Mods.Concat(su.Mods).Concat(PrivMods).GetStat(StatType.WeaponRange));
		}
	}

	public class Spear : Weapon {
		public Spear(int dmg) : base(dmg) { }

		public override IEnumerable<Tile> Range(Skill skill, SkillUser su) {
			if (su.Place == null) throw new ArgumentException("SkillUser must be placed.");
			Tile t;
			t = su.Place.North;
			if (t != null) {
				yield return t;
				t = t.North;
				if (t != null) yield return t;
			}
			t = su.Place.East;
			if (t != null) {
				yield return t;
				t = t.East;
				if (t != null) yield return t;
			}
			t = su.Place.West;
			if (t != null) {
				yield return t;
				t = t.West;
				if (t != null) yield return t;
			}
			t = su.Place.South;
			if (t != null) {
				yield return t;
				t = t.South;
				if (t != null) yield return t;
			}
		}
		public override IEnumerable<Tile> AoE(Skill skill, SkillUser su, Tile target) {
			if (su == null) throw new Exception("fcdasgdscfvdfshdsgvdfskjbvndkljs vdsb viudsvjdsnyibedi bvd");
			Tile place = su.Place;
			if (place == null) throw new ArgumentException("SkillUser must be placed.");
			if (target == null) throw new ArgumentNullException("Fuck you");
			if (place == target) throw new ArgumentException("vkjifbdhsb gfknbrbvdfhdrtbcx gfd ");

			int Dif = target.X - place.X;
			if (Dif != 0) {
				if (place.Y - target.Y != 0) yield break;
				if (Math.Abs(Dif) > 2) yield break;
				if (Dif > 0) {
					yield return place.East;
					if (place.East.East != null) yield return place.East.East;
				} else {
					yield return place.West;
					if (place.West.West != null) yield return place.West.West;
				}
			} else {
				Dif = target.Y - place.Y;
				if (Math.Abs(Dif) > 2) yield break;
				if (Dif > 0) {
					yield return place.North;
					if (place.North.North != null) yield return place.North.North;
				} else {
					yield return place.South;
					if (place.South.South != null) yield return place.South.South;
				}
			}
		}

		public override void Affect(Skill skill, SkillUser su, Tile target, Action<Skill, SkillUser, Tile> effect) {
			if (target.Inhabitant != null) {
				int dif = su.Place.X - target.X;
				if (dif == 0) dif = su.Place.Y - target.Y;
				var mods = PrivMods;
				if (Math.Abs(dif) != 1)
					mods = mods.Concat(	//halve effect v targets 2 tiles away
						new Mod[] { new Mod(StatType.None, ModifyingMethod.Multiply, 0.50) });

				effect(skill.GetModdedInstance(mods), su, target);
			}
		} 
	}

	public enum GearType {
		OneHandedWeapon, TwoHandedWeapon, Shield, Helmet, Accessory, Boots, Gloves, Armour
	}
}
