using System;
using System.Collections.Generic;
using System.Linq;

namespace srpg {
	public class Battle {

		private Game Game;
		public IEnumerable<MetaBeing> GetRoster() {
			return Game.Roster;
		}

		public TileSet TileSet { get; private set; }
		private TurnTracker _TurnTracker = new TurnTracker();
		private ITurnHaver CurrentTurnHaver { get { return _TurnTracker.CurrentTurnHaver; } }
		public Being activeBeing { get { return CurrentTurnHaver as Being; } } //return null if current turn does not belong to a being

		private List<Being> _Beings = new List<Being>();
		public IEnumerable<Being> Beings { get { return _Beings.AsEnumerable(); } }

		private List<ChannelingInstance> _ChannelingInstances = new List<ChannelingInstance>();
		public IEnumerable<ChannelingInstance> ChannelingInstances { get { return _ChannelingInstances.AsEnumerable(); } }

		public event EventHandler<Being.MovedArgs> BeingMoved;
		private void OnBeingMoved(object s, Being.MovedArgs e) {
			if (BeingMoved != null) BeingMoved(s, e);
		}

		public event EventHandler<EventArgs> BeingTurnStarted;
		private void OnBeingTurnStarted(object s, EventArgs e) {
			if (s is Being && BeingTurnStarted != null) BeingTurnStarted(s, e);
		}

		public event EventHandler<EventArgs> BeingTurnFinished;
		private void OnBeingTurnFinished(object s, EventArgs e) {
			if (s is Being && BeingTurnFinished != null) BeingTurnFinished(s, e);
		}

		public void Add(ITurnHaver ith) {
			if (ith is Being) AddB(ith as Being);
			else if (ith is ChannelingInstance) AddCI(ith as ChannelingInstance);

			_TurnTracker.Add(ith);
		}
		public void Remove(ITurnHaver ith) {
			if (ith is ChannelingInstance) RemoveCI(ith as ChannelingInstance);
			else if (ith is Being) RemoveB(ith as Being);

			_TurnTracker.Remove(ith);
		}

		public class BeingAddedArgs : EventArgs {
			public Being Being;
			public BeingAddedArgs(Being being) {
				Being = being;
			}
		}
		public event EventHandler<BeingAddedArgs> BeingAdded;
		private void AddB(Being being) {
			_Beings.Add(being);
			being.MoveStarted += OnBeingMoved;
			being.TurnStarted += OnBeingTurnStarted;
			being.TurnFinished += OnBeingTurnFinished;

			if (BeingAdded != null) BeingAdded(this, new BeingAddedArgs(being));
		}
		private void RemoveB(Being being) {
			_Beings.Remove(being);
			being.MoveStarted -= OnBeingMoved;
		}

		private void AddCI(ChannelingInstance ci) {
			_ChannelingInstances.Add(ci);
		}
		private void RemoveCI(ChannelingInstance ci) {
			_ChannelingInstances.Remove(ci);
			_TurnTracker.Remove(ci);
		}
		
		public Battle(Game game) {
			TileSet = new TileSet(30, 30, 8);
			Game = game;

			_TurnTracker.TurnStarted += OnBeingTurnStarted;
			_TurnTracker.TurnFinished += OnBeingTurnFinished;
		}

		public bool AddMetaBeing(MetaBeing mb, int team, int x, int y) {

			new Being(mb, team, this, x, y);

			return true;
		}

		public bool Paused {
			get { return _TurnTracker.Paused; }
			set { _TurnTracker.Paused = value; }
		}
	}
}
