namespace srpg {
	public abstract class WorldIO {
		private Game Game;

		internal void SetGame(Game game) {
			Game = game;
		}
		public void TEMP_ForceEnterBattle() {
			Game.TEMP_ForceEnterBattle();
		}
		
		public abstract void EnterBattle();
	}
}