using OneMoreFloor.Player;
using Sandbox;

namespace OneMoreFloor.Entities.Arcade.Cabs
{
	[Library("ent_omf_arcade_basecab", Title = "Base Arcade Cabinet", Spawnable = true, Description = "Base arcade cabinet for One More Floor.")]
	[Hammer.Model]
	public partial class BaseCabEntity : MinigameEntity
	{
		protected override PawnController SetupController()
		{
			return new CabController();
		}

		protected override ICamera SetupCamera()
		{
			return new CabCamera();
		}

		protected override PawnAnimator SetupAnimator()
		{
			return new CabAnimator();
		}

		protected override void OnStartPlaying()
		{
			base.OnStartPlaying();

			// TODO: We should only do this on the client
			Owner.EnableDrawing = false;
		}

		protected override void OnStopPlaying()
		{
			base.OnStopPlaying();

			// TODO: We should only do this on the client
			Owner.EnableDrawing = true;
		}
	}
}
