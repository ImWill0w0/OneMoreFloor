using Sandbox;
using System;
using System.Linq;

namespace OneMoreFloor.Weapons
{
	public class ViewModel : BaseViewModel
	{
		protected float SwingInfluence => 0f;
		protected float ReturnSpeed => 0f;
		protected float MaxOffsetLength => 0f;
		protected float BobCycleTime => 0;
		protected Vector3 BobDirection => new(0.0f, 0.0f, 0.0f);

		private Vector3 swingOffset;
		private float lastPitch;
		private float lastYaw;
		private float bobAnim;

		private bool activated;

		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			base.PostCameraSetup( ref camSetup );

			if ( !Local.Pawn.IsValid() )
			{
				return;
			}

			if ( !this.activated )
			{
				this.lastPitch = camSetup.Rotation.Pitch();
				this.lastYaw = camSetup.Rotation.Yaw();

				this.activated = true;
			}

			Position = camSetup.Position;
			Rotation = camSetup.Rotation;

			camSetup.ViewModel.FieldOfView = FieldOfView;

			var newPitch = Rotation.Pitch();
			var newYaw = Rotation.Yaw();

			var pitchDelta = Angles.NormalizeAngle( newPitch - this.lastPitch );
			var yawDelta = Angles.NormalizeAngle( this.lastYaw - newYaw );

			var playerVelocity = Local.Pawn.Velocity;
			var verticalDelta = playerVelocity.z * Time.Delta;
			var viewDown = Rotation.FromPitch( newPitch ).Up * -1.0f;
			verticalDelta *= 1.0f - MathF.Abs( viewDown.Cross( Vector3.Down ).y );
			pitchDelta -= verticalDelta * 1;

			var offset = this.CalcSwingOffset( pitchDelta, yawDelta );
			offset += this.CalcBobbingOffset( playerVelocity );

			Position += Rotation * offset;

			this.lastPitch = newPitch;
			this.lastYaw = newYaw;
		}

		protected Vector3 CalcSwingOffset( float pitchDelta, float yawDelta )
		{
			var swingVelocity = new Vector3( 0, yawDelta, pitchDelta );

			this.swingOffset -= this.swingOffset * ReturnSpeed * Time.Delta;
			this.swingOffset += swingVelocity * SwingInfluence;

			if ( this.swingOffset.Length > MaxOffsetLength )
			{
				this.swingOffset = this.swingOffset.Normal * MaxOffsetLength;
			}

			return this.swingOffset;
		}

		protected Vector3 CalcBobbingOffset( Vector3 velocity )
		{
			this.bobAnim += Time.Delta * BobCycleTime;

			var twoPI = MathF.PI * 2.0f;

			if ( this.bobAnim > twoPI )
			{
				this.bobAnim -= twoPI;
			}

			var speed = new Vector2( velocity.x, velocity.y ).Length;
			speed = speed > 10.0 ? speed : 0.0f;
			var offset = BobDirection * (speed * 0.005f) * MathF.Cos( this.bobAnim );
			offset = offset.WithZ( -MathF.Abs( offset.z ) );

			return offset;
		}
	}
}
