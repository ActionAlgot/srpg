using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace srpg {
	[Serializable()]
	public class SkillNode {

		public void AddMod(Mod mod) { _Mods.Add(mod); }
		public void RemoveMod(Mod mod) { _Mods.Remove(mod); }

		public int X, Y;
		
		private List<Mod> _Mods = new List<Mod>();
		public IEnumerable<Mod> Mods { get { return _Mods.AsEnumerable(); } }
		[NonSerialized]
		private List<SkillTreePath> _Paths = new List<SkillTreePath>();
		public IEnumerable<SkillTreePath> Paths { get { return _Paths.AsEnumerable(); } }

		public SkillNode(int x, int y) { X = x; Y = y; }
		public SkillNode(int x, int y, Mod mod) {
			X = x; Y = y;
			_Mods.Add(mod);
		}
		public SkillNode(int x, int y, IEnumerable<Mod> mods) {
			X = x; Y = y;
			_Mods.AddRange(mods);
		}

		public void AddPath(SkillTreePath path) {
			_Paths.Add(path);
		}

		[OnDeserialized()]
		protected void OnDeserialized(StreamingContext context) {
			_Paths = new List<SkillTreePath>();
		}
	}

	[Serializable]
	public class SkillTreePath {
		[NonSerialized]
		private SkillNode _Node0, _Node1;
		public SkillNode Node0 { get { return _Node0; } private set { _Node0 = value; } }
		public SkillNode Node1 { get { return _Node1; } private set { _Node1 = value; } }

		public void SetNodes(SkillNode node0, SkillNode node1) {
			if (!(Node0 == null && Node1 == null))
				throw new ArgumentException("Nodes already set");

			Node0 = node0;
			Node1 = node1;
		}
		public void FinalizeDeserialization() {
			Node0.AddPath(this);
			Node1.AddPath(this);
		}

		public bool OneWay { get; private set; }
		public SkillTreePath(SkillNode node0, SkillNode node1)
			: this(node0, node1, false) { }
		public SkillTreePath(SkillNode node0, SkillNode node1, bool oneWay) {
			Node0 = node0;
			Node1 = node1;
			OneWay = oneWay;

			Node0.AddPath(this);
			Node1.AddPath(this);
		}
	}

	[Serializable]
	public class SkillTree : ISerializable, IDeserializationCallback {

		protected SkillTree(SerializationInfo info, StreamingContext context) {
			_AllNodes = (List<SkillNode>)info.GetValue("nodes", typeof(List<SkillNode>));
			Start = _AllNodes[info.GetInt32("Start")];
			
			var paths = (List<SkillTreePath>)info.GetValue("paths", typeof(List<SkillTreePath>));
			var nodePairs = (List<int[]>)info.GetValue("pathsNodePairs", typeof(List<int[]>));
			for(int i = 0; i<paths.Count; i++) {
				paths[i].SetNodes(
					_AllNodes[nodePairs[i][0]],
					_AllNodes[nodePairs[i][1]] );
				_AllPaths.Add(paths[i]);
			}
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("Start", _AllNodes.IndexOf(Start));
			info.AddValue("nodes", _AllNodes);

			var paths = _AllPaths.ToList();		//ensure order as hashset enumeration may be poopy
			var nodePairs = new List<int[]>();
			for(int i = 0; i<paths.Count; i++) {
				var p = paths[i];
				nodePairs.Add(new int[]{
					_AllNodes.IndexOf(p.Node0),
					_AllNodes.IndexOf(p.Node1) });
			}

			info.AddValue("paths", paths);
			info.AddValue("pathsNodePairs", nodePairs);

		}

		public SkillNode Start { get; private set; }
		private List<SkillNode> _AllNodes = new List<SkillNode>();
		private HashSet<SkillTreePath> _AllPaths = new HashSet<SkillTreePath>();
		public IEnumerable<SkillTreePath> AllPaths { get { return _AllPaths.AsEnumerable(); } }
		public IEnumerable<SkillNode> AllNodes { get { return _AllNodes.AsEnumerable(); } }

		public void AddNode(SkillNode node) {
			_AllNodes.Add(node);
			foreach (var p in node.Paths)
				if(!_AllPaths.Contains(p)) _AllPaths.Add(p);
		}
		public void AddNodes(IEnumerable<SkillNode> nodes) {
			foreach (var n in nodes)
				AddNode(n);
		}

		public SkillTree(SkillNode start) {
			AddNode(start);
			Start = start;
		}
		public IEnumerable<SkillNode> GetNodes(IEnumerable<int> indexs) {
			return indexs.Select(i => _AllNodes[i]);
		}

		void IDeserializationCallback.OnDeserialization(object sender) {
			foreach (var p in _AllPaths)
				p.FinalizeDeserialization();
		}
	}

	public static class SkillTreeshit {
		private static SkillTree _basic;
		public static SkillTree Basic { get {
				if (_basic == null)
					BuildBasic();
				return _basic;
		}	}
		private static void BuildBasic() {
			_basic = new SkillTree( new SkillNode(-20, -20, new Mod[] {
				new AdditionMod(StatType.Strength, 10),
				new AdditionMod(StatType.Speed, 5),
				new AdditionMod(StatType.HP, 20),
				new AdditionMod(StatType.ChannelingSpeed,  4),
				new ConversionToAdditiveMultiplierMod( StatType.PhysicalDamage, 0.05, StatType.Strength),
				new AdditionMod(StatType.MovementPoints, 5)
			}));
			var nodes = new SkillNode[] {
				/*0*/new SkillNode(20, 60, new AdditionMod(StatType.HP, 5)),
				/*1*/new SkillNode(-60, 60, new AdditionMod(StatType.Speed, 1)),
				/*2*/new SkillNode(-100, -20, new AdditiveMultiplierMod(StatType.Strength, 0.20)),
				/*3*/new SkillNode(-140, -100, new GainMod(StatType.FireDamage, 0.10, StatType.PhysicalDamage)),
				/*4*/new SkillNode(-180, -20, new ConversionMod(StatType.FireDamage, 0.50, StatType.PhysicalDamage)),
				/*5*/new SkillNode(60, 140, new AdditionMod(StatType.FireResistance, 0.50))
			};
			new SkillTreePath(Basic.Start, nodes[0]);
			new SkillTreePath(_basic.Start, nodes[1]);
			new SkillTreePath(_basic.Start, nodes[2]);
			new SkillTreePath(nodes[2], nodes[3]);
			new SkillTreePath(nodes[2], nodes[4]);
			new SkillTreePath(nodes[0], nodes[5]);

			Basic.AddNodes(nodes);
		}
	}

	public class SkillTreeFiller {
		public SkillTree Target { get; private set; }
		private Dictionary<SkillNode, bool> TakenDic = new Dictionary<SkillNode, bool>();
		public IEnumerable<SkillNode> Taken {
			get { return TakenDic.Where(kvp => kvp.Value).Select(kvp => kvp.Key); } }
		private Dictionary<SkillNode, bool> AvailableDic = new Dictionary<SkillNode, bool>();
		public IEnumerable<SkillNode> Available {
			get { return AvailableDic.Where(kvp => kvp.Value).Select(kvp => kvp.Key); } }

		private bool _Taken(SkillNode n) {
			return TakenDic.ContainsKey(n) && TakenDic[n];
		}
		public bool Take(SkillNode skN) {
			if (TakenDic.ContainsKey(skN) && TakenDic[skN] == true)
				return false;
			if (AvailableDic.ContainsKey(skN) == false
				|| AvailableDic[skN] == false)
				return false;

			TakenDic[skN] = true;
			AvailableDic[skN] = false;
			foreach (var p in skN.Paths)
				if (p.Node0 == skN && !_Taken(p.Node1))
					AvailableDic[p.Node1] = true;
				else if (!p.OneWay && !_Taken(p.Node0))
					AvailableDic[p.Node0] = true;
			return true;
		}

		public SkillTreeFiller(SkillTree target) {
			Target = target;
			AvailableDic[Target.Start] = true;
			Take(Target.Start);
		}
	}
}
