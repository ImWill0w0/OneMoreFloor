using OneMoreFloor.Player;
using Sandbox;

namespace OneMoreFloor.Entities.Arcade.Cabs
{
	[Library]
	public class CabController : PawnController
	{
		public override void FrameSimulate()
		{
			base.FrameSimulate();

			Simulate();
		}

		// TODO: Make a generic minigame base entity
		public override void Simulate()
		{
			var player = Pawn as OMFPlayer;
			if ( !player.IsValid() ) return;

			var game = player.Minigame as BaseCabEntity;
			if ( !game.IsValid() ) return;

			game.Simulate( Client );
			
			/*

			if ( player.Vehicle == null )
			{
				Position = game.Position + game.Rotation.Up * (100 * game.Scale);
				Velocity += game.Rotation.Right * (200 * game.Scale);
				return;
			}

			EyeRot = Input.Rotation;
			EyePosLocal = Vector3.Up * (64 - 10) * game.Scale;
			Velocity = game.Velocity;

			SetTag( "noclip" );
			SetTag( "sitting" );
			*/
		}
	}
}
