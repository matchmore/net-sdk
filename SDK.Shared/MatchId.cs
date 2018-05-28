using System;
namespace Matchmore.SDK
{
    public class MatchId
    {
		private readonly Guid _id;

		MatchId(Guid id) => _id = id;

		public static MatchId Make(Guid id) => new MatchId(id);
		public static MatchId Make(string id){
			if(Guid.TryParse(id, out Guid result)){
				return new MatchId(result);
			}
			throw new ArgumentException("Invalid Match id");
		}

		public static bool TryParse(string str, out MatchId result) {
			if (Guid.TryParse(str, out Guid guid))
            {
				result = new MatchId(guid);
				return true;
            }
			result = null;
			return false;
		}

		public override string ToString() => _id.ToString();
	}
}
