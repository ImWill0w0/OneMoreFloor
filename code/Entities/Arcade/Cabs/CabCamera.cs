using OneMoreFloor.Player;
using Sandbox;

namespace OneMoreFloor.Entities.Arcade.Cabs
{
	public class CabCamera : Camera
	{
		public override void Activated()
		{
			 var pawn = Local.Pawn as OMFPlayer;
			 if ( pawn == null ) return;

			 var cab = pawn.Minigame;
			 
			 Pos = cab.Position + new Vector3( -35, 0, 65 );
			 FieldOfView = 60;
			 Rot = Rotation.FromPitch( 15 );
			 //	Rot = Rotation.Identity;
		}
		
		public override void Update()
		{
			// ignored
		}
	}
}
