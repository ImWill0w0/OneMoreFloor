using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace OneMoreFloor
{
	public static class Util
	{
		public static T Random<T>( this IEnumerable<T> enumerable )
		{
			if ( !enumerable.Any() )
				return default;

			var choice = Rand.Int( 0, enumerable.Count() - 1 );
			return enumerable.ElementAt( choice );
		}
	}
}
