﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class ChannelingInstance : ITurnHaver, SkillUser {
		#region ITurnHaver
		public event EventHandler TurnFinished;
		protected double _speed;
		protected double _awaited;
		public double Speed {
			get {
				var s = _speed;
				if (Mods.ContainsKey("Speed"))
					foreach (var f in Mods["Speed"])
						s += f(_speed);
				return s;
			}
		}
		public double Awaited { get { return _awaited; } }
		public void Await(double time) {
			_awaited += Speed * time;
		}
		#endregion
		public Weapon Weapon { get { return null; } }	//Kill me
		private Dictionary<string, List<Func<double, double>>> _mods;
		public Dictionary<string, List<Func<double, double>>> Mods { get { return _mods; } }

		private Tile _place;
		public Tile Place {
			get { return _place; }
			set {
				if (value != null && value.ChannelingInstance != null) {
					if (value == _place) return;
					else throw new ArgumentException("Tile is occupied.");
				}
				if (_place != null) _place.ChannelingInstance = null;
				_place = value;
				if (value != null) value.ChannelingInstance = this;
			}
		}
		protected Spell Spell;
		protected Func<Tile> TargetSelector;

		public Action<System.Drawing.Graphics> Draw { get; protected set; }

		public void Do() {
			if (!Spell.Do(TargetSelector()))
				ConsoleLoggerHandlerOrWhatever.Log(this.ToString() + " cast " + Spell.Name + " and hit nothing.");
			_awaited = 0;	//should just kill self
			_speed = 0;
			if (TurnFinished != null) TurnFinished(this, EventArgs.Empty);
			Place = null;
		}

		public ChannelingInstance(SkillUser caster, Spell spell, Tile place, Func<Tile> targetSelector) {
			_mods = new Dictionary<string, List<Func<double, double>>>();
			foreach (var fuck in caster.Mods) {
				Mods[fuck.Key] = fuck.Value.Select(f => f).ToList();
			}
			_speed = 5;
			Spell = spell.GetAsChanneled(this);
			Place = place;
			TargetSelector = targetSelector;

			Draw = g => g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.Pink), Place.Rectangle);
		}
	}

	public class ChannelingSpell : Spell {
		protected Spell Spell;
		protected Func<SkillUser, Spell> SpellMaker;
		protected Func<Tile, Func<Tile>> TargetSelector;
		private TurnFuckYouFuckThatFuckEverything ShitTracker;

		protected override void TileEffect(Tile t) {
			var piss = new ChannelingInstance(Doer, Spell, t, TargetSelector(t));
			piss.TurnFinished += (s, e) => ShitTracker.Remove(piss);
			ShitTracker.Add(piss);
		}

		public ChannelingSpell(SkillUser doer, Spell spell, Func<Tile, Func<Tile>> targetSelector, TurnFuckYouFuckThatFuckEverything shitTracker)
			: base(doer, 6, "Channel" + " channeling") {
			ShitTracker = shitTracker;
			Spell = spell;
			//Spell.Doer = this;
			TargetSelector = targetSelector;
			TargetTileAllowed = true;

			GetAreaOfEffect = GetGetAreOfEffect(1);
		}
	}

	public class SpeedupChanneling : Spell {
		protected override void ChannelingEffect(ChannelingInstance ci) {
			if (ci != null) {
				var mods = ci.Mods;
				if (!mods.ContainsKey("Speed")) mods["Speed"] = new List<Func<double, double>>();
				mods["Speed"].Add(s => s);	//100% increase
			}
			base.ChannelingEffect(ci);
		}

		public SpeedupChanneling(SkillUser caster) : base(caster, 10, "Channeling speedup") {
			TargetTileAllowed = true;
			MustTargetChannelingInstance = true;
			GetAreaOfEffect = GetGetAreOfEffect(1);
		}
	}
}