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
			 
			 Pos = cab.Transform.PointToWorld(new Vector3( -35, 0, 65 ));
			 FieldOfView = 60;
			 
			 // TODO: This should be a fixed point looking at the screen, but I can't maths
			 Rot = pawn.EyeRot;
		}
		
		public override void Update()
		{
			// ignored
		}
	}
}
