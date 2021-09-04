using Sandbox;

namespace OneMoreFloor.Player
{
	public partial class OMFWalkController : WalkController
	{
		public OMFWalkController()
		{
			WalkSpeed = 140;
			SprintSpeed = WalkSpeed;
			DefaultSpeed = WalkSpeed;
		}

		public override void CheckJumpButton()
		{
			// ignored
		}
	}
}
