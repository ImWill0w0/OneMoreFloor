using OneMoreFloor.Player;
using Sandbox;

namespace OneMoreFloor.Entities.Arcade
{
	[Library("ent_omf_minigame", Spawnable = false)]
	public partial class MinigameEntity : ModelEntity, IUse
	{
		public Entity MinigameOwner { get; set; }

		private TimeSince timeSinceOwnerLeft;

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Static );

			MoveType = MoveType.None;
			CollisionGroup = CollisionGroup.Interactive;
			PhysicsEnabled = true;
			UsePhysicsCollision = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			EnableHitboxes = true;
		}

		public override void Simulate( Client owner )
		{
			if ( owner == null ) return;
			if ( !IsServer ) return;

			using ( Prediction.Off() )
			{
				//currentInput.Reset();

				if ( Input.Pressed( InputButton.Use ) )
				{
					if ( owner.Pawn is OMFPlayer player && !player.IsUseDisabled() )
					{
						RemoveOwner( player );

						return;
					}
				}

				/*
				currentInput.throttle = (Input.Down( InputButton.Forward ) ? 1 : 0) + (Input.Down( InputButton.Back ) ? -1 : 0);
				currentInput.turning = (Input.Down( InputButton.Left ) ? 1 : 0) + (Input.Down( InputButton.Right ) ? -1 : 0);
				currentInput.breaking = (Input.Down( InputButton.Jump ) ? 1 : 0);
				currentInput.tilt = (Input.Down( InputButton.Run ) ? 1 : 0) + (Input.Down( InputButton.Duck ) ? -1 : 0);
				currentInput.roll = (Input.Down( InputButton.Left ) ? 1 : 0) + (Input.Down( InputButton.Right ) ? -1 : 0);
				*/
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if ( this.MinigameOwner is OMFPlayer player )
			{
				RemoveOwner( player );
			}
		}

		private void RemoveOwner( OMFPlayer player )
		{
			OnStopPlaying();

			this.MinigameOwner = null;
			this.timeSinceOwnerLeft = 0;

			//ResetInput();

			if ( !player.IsValid() )
				return;

			player.Minigame = null;
			player.MinigameController = null;
			player.MinigameAnimator = null;
			player.MinigameCamera = null;
			player.Parent = null;

			if ( player.PhysicsBody.IsValid() )
			{
				player.PhysicsBody.Enabled = true;
				player.PhysicsBody.Position = player.Position;
			}
		}

		public bool OnUse( Entity user )
		{
			if ( user is OMFPlayer player && player.Minigame == null && this.timeSinceOwnerLeft > 1.0f )
			{
				player.Minigame = this;
				player.MinigameController = SetupController();
				player.MinigameCamera = SetupCamera();
				player.MinigameAnimator = SetupAnimator();

				this.MinigameOwner = user;

				OnStartPlaying();
			}

			return true;
		}

		public bool IsUsable( Entity user )
		{
			return this.MinigameOwner == null;
		}

		protected virtual PawnController SetupController() => null;

		protected virtual ICamera SetupCamera() => null;

		protected virtual PawnAnimator SetupAnimator() => null;

		protected virtual void OnStartPlaying()
		{
			// ignored
		}

		protected virtual void OnStopPlaying()
		{
			// ignored
		}
	}
}
