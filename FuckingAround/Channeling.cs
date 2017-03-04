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
		public double Speed { get { return Stats[StatType.ChannelingSpeed]; } }
		public double Awaited { get { return _awaited; } }
		public void Await(double time) {
			_awaited += Speed * time;
		}
		#endregion
		public Weapon Weapon { get { return null; } }   //TODO Kill me

		public Battle Battle { get; private set; }
		public StatSet Stats { get; protected set; }
		public Dictionary<object, StatSet> SkillUsageStats { get; protected set; }

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

		public void OnTurnStarted(object s, EventArgs e) {
			if (!Skill.Do(this, TargetSelector()))
				ConsoleLoggerHandlerOrWhatever.Log(this.ToString() + " cast " + Skill.Name + " and hit nothing.");
			_awaited = 0;	//should just kill self
			if (TurnFinished != null) TurnFinished(this, EventArgs.Empty);
			Place = null;
		}

		public ChannelingInstance(Battle battle, IEnumerable<Mod> mods, Skill skill, Tile place, Func<Tile> targetSelector) {
			Stats = new StatSet();
			SkillUsageStats = new Dictionary<object, StatSet>();
			Battle = battle;
			Battle.Add(this);

			Skill = skill;
			Place = place;
			TargetSelector = targetSelector;

			foreach (var m in mods)
				m.Affect(Stats);
			
			TurnFinished += (s, e) => Battle.Remove(this);
			TurnStarted += OnTurnStarted;
		}
		public ChannelingInstance(Battle battle, IEnumerable<Mod> mods, Skill skill, Tile place)
			: this(battle, mods, skill, place, () => place) { }

		public event EventHandler TurnStarted;

		public void StartTurn() {
			TurnStarted(this, EventArgs.Empty);
		}
	}
	public static class SkillUserChannelingExtensions {
		public static IEnumerable<Mod> GetChannelingMods(this SkillUser su) {
			yield return new AdditionMod(StatType.ChannelingSpeed, su.Stats[StatType.ChannelingSpeed]);
		}
	}
}