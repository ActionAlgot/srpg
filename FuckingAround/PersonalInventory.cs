using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace srpg{
	public class PersonalInventory : IEnumerable<Gear> {
		private Being Owner;
		private Gear[] gear = new Gear[5];

		public class WeaponSetEventArgs : EventArgs {
			public Weapon Previous;
			public Weapon New;
			public WeaponSetEventArgs(Gear previous, Gear newg){
				Previous = previous as Weapon;	//autonull if shield
				New = newg as Weapon;
			}
		}
		public event EventHandler<WeaponSetEventArgs> MainHandSet;
		public event EventHandler<WeaponSetEventArgs> OffHandSet;

		private Weapon _mainHand;
		public Weapon MainHand { //set should never be used outside of Equip/Unequip
			get { return _mainHand ?? Owner.Fist; }
			private set {
				var pre = MainHand;
				_mainHand = value;
				if(MainHandSet != null) MainHandSet(this, new WeaponSetEventArgs(pre, MainHand));
			}
		}
		private Gear _offHand;
		public Gear OffHand {
			get { return _offHand ?? (MainHand.TwoH ? null : Owner.Fist); }
			private set {
				var pre = OffHand;
				_offHand = value;
				if (OffHandSet != null) OffHandSet(this, new WeaponSetEventArgs(pre, OffHand));
			}
		}

		private void EquipS(Shield s) {
			if (MainHand.TwoH) throw new ArgumentException("Can't shield while TWoH");
			else if (OffHand != Owner.Fist) throw new ArgumentException("Can't triple wield");
			else OffHand = s;
		}
		private void EquipW(Weapon w) {
			if (w == null) MainHand = w;
			else if (MainHand == Owner.Fist) {
				if (!w.TwoH || OffHand == Owner.Fist) MainHand = w;
				else throw new ArgumentException("Can't wield 2H in 1H"); }
			else if (MainHand.TwoH) throw new ArgumentException("Can't shit in offhand while wielding 2H");
			else if (OffHand != Owner.Fist) throw new ArgumentException("Can't triplewield");
			else if (w.TwoH) throw new ArgumentException("Can't 2H in 1H");
			else OffHand = w;
		}
		private void Equip(Gear g) {
			if (g is Shield) EquipS(g as Shield);
			else if (g is Weapon) EquipW(g as Weapon);
		}

		private void Unequip(Gear g) {
			if (MainHand == g) {
				MainHand = null;
				if (OffHand is Weapon && OffHand != Owner.Fist) {
					MainHand = (Weapon)OffHand;
					OffHand = null;
				}
			} else if(OffHand == g){
				OffHand = null;
			}
		}

		public Gear this[int index] {
			get { return gear[index]; }
			set {
				if (gear[index] != null) {
					Unequip(gear[index]);
					gear[index] = null;
				}
				try {
					Equip(value);
					gear[index] = value;
				}
				catch (ArgumentException) { }
			}
		}
		public PersonalInventory(Being owner) {
			Owner = owner;
		}

		#region IEnumerable
		public IEnumerator<Gear> GetEnumerator() {
			return ((IEnumerable<Gear>)gear).GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return gear.GetEnumerator();
		}
		#endregion
	}
}