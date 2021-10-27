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

			 var cab = pawn.Minigame as BaseCabEntity;
			 
			 Position = cab.Transform.PointToWorld(new Vector3( -35, 0, 65 ));
			 FieldOfView = 60;
			 
			 // TODO: This should be a fixed point looking at the screen, but I can't maths
			 Rotation = Rotation.LookAt( cab.Position - Position.WithZ( Position.z - cab.ScreenHeight ) );
		}
		
		public override void Update()
		{
			// ignored
		}
	}
}
