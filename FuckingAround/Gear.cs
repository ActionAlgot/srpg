using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public abstract class Gear {
		public abstract IEnumerable<Mod> LocalBaseMods { get; }
		public abstract IEnumerable<Mod> GlobalBaseMods { get; }
		public IEnumerable<Mod> BaseMods { get { return LocalBaseMods.Concat(GlobalBaseMods); } }

		public IEnumerable<Mod> LocalEnchantments { get; protected set; }
		public IEnumerable<Mod> GlobalEnchantments { get; protected set; }
		public IEnumerable<Mod> Enchantments { get { return LocalEnchantments.Concat(GlobalEnchantments); } }

		public virtual IEnumerable<Mod> GlobalMods { get {
			return GlobalBaseMods
				.Concat(GlobalEnchantments)
				.Concat(ModdedMods);
		} }
		protected IEnumerable<Mod> PrivMods { get {
			return LocalBaseMods.Concat(LocalEnchantments);
		} }
		protected IEnumerable<Mod> ModdedMods {	//get private mods calculated into global add mods
			get {
				var r = new StatSet();
				var r2 = new List<Mod>();
				foreach (var m in PrivMods)
					if (m is ConversionMod) r2.Add(m);
					else m.Affect(r);
				return r.AsEnumerableStats()
					.Select(stat => new AdditionMod(stat.StatType, stat.LoneValue))
					.Concat(r2);
		} }
	}

	public class ArmourButNotNecessarilyArmour : Gear {
		private AdditionMod BaseArmour;
		public override IEnumerable<Mod> LocalBaseMods { get { yield return BaseArmour; } }
		public override IEnumerable<Mod> GlobalBaseMods { get { yield break; } }
		public ArmourButNotNecessarilyArmour(int armour) {
			BaseArmour = new AdditionMod(StatType.Armour, armour);
			LocalEnchantments = new List<Mod>();
			GlobalEnchantments = new List<Mod>();
		}
	}
	public class Shield : ArmourButNotNecessarilyArmour {
		public Shield(int armour) : base(armour) { }
	}

	public class Weapon : Gear {
		public bool TwoH;
		private Mod BaseDamage;
		public override IEnumerable<Mod> LocalBaseMods { get { yield return BaseDamage; } }
		public override IEnumerable<Mod> GlobalBaseMods { get { yield break; } }
		public virtual void Affect(Skill skill, SkillUser su, Tile target, Action<Skill, SkillUser, Tile> effect) {
			effect(skill.GetModdedInstance(PrivMods), su, target);
		}

		public override IEnumerable<Mod> GlobalMods { get {
				return GlobalBaseMods
					.Concat(GlobalEnchantments);
		}	}

		public virtual IEnumerable<Tile> Range(Skill skill, SkillUser su) {
			return su.Place.Adjacent;
		}
		public virtual IEnumerable<Tile> AoE(Skill skill, SkillUser su, Tile target) {
			yield return target;
		}
		public Weapon(int dmg) {
			BaseDamage = new AdditionMod(StatType.PhysicalDamage|StatType.Weapon, dmg);
			LocalEnchantments = new List<Mod>();
			GlobalEnchantments = new List<Mod>();
		}
	}

	public class ProjectileWeapon : Weapon {
		public ProjectileWeapon(int dmg) : base(dmg) { }
		public override IEnumerable<Tile> Range(Skill skill, SkillUser su) {
			return su.Place.GetArea((int)skill.GetStat(StatType.WeaponRange, su));
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
					mods = PrivMods.Concat(	//halve effect v targets 2 tiles away
						new Mod[] { new MultiplierMod(StatType.Damage, 0.50) });

				effect(skill.GetModdedInstance(mods), su, target);
			}
		} 
	}

	public enum GearType {
		OneHandedWeapon, TwoHandedWeapon, Shield, Helmet, Accessory, Boots, Gloves, Armour
	}
}
