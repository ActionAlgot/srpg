using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class Gear {
		public IEnumerable<EquipmentMod> BaseMods { get; protected set; }
		public IEnumerable<EquipmentMod> Enchantments { get; protected set; }
		public IEnumerable<Mod> GlobalMods { get { return BaseMods.Concat(Enchantments).Where(em => em.Global); } }
		
	}

	

	public enum GearType {
		OneHandedWeapon, TwoHandedWeapon, Shield, Helmet, Accessory, Boots, Gloves, Armour
	}
}
