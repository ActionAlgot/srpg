using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class ChannelingInstance : ITurnHaver, SkillUser {
		#region ITurnHaver
		public event EventHandler TurnFinished;
		protected double _awaited;
		public double Speed { get { return Mods.GetStat(StatType.ChannelingSpeed); } }
		public double Awaited { get { return _awaited; } }
		public void Await(double time) {
			_awaited += Speed * time;
		}
		#endregion
		public Weapon Weapon { get { return null; } }	//Kill me

		public void AddMod(Mod mod) {
			_mods.Add(mod);
		}
		private List<Mod> _mods;
		public IEnumerable<Mod> Mods { get { return _mods; } }

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
		protected Skill Skill;
		protected Func<Tile> TargetSelector;

		public Action<System.Drawing.Graphics> Draw { get; protected set; }

		public void Do() {
			if (!Skill.Do(this, TargetSelector()))
				ConsoleLoggerHandlerOrWhatever.Log(this.ToString() + " cast " + Skill.Name + " and hit nothing.");
			_awaited = 0;	//should just kill self
			_mods = new List<Mod>();
			if (TurnFinished != null) TurnFinished(this, EventArgs.Empty);
			Place = null;
		}

		public ChannelingInstance(IEnumerable<Mod> mods, Skill skill, Tile place, Func<Tile> targetSelector) {
			_mods = mods.ToList();

			Skill = skill;
			Place = place;
			TargetSelector = targetSelector;

			Draw = g => g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.Pink), Place.Rectangle);
			TurnFinished += (s, e) => TurnTracker.Remove(this);
		}
		public ChannelingInstance(IEnumerable<Mod> mods, Skill skill, Tile place) : this(mods, skill, place, () => place) { }
	}
}