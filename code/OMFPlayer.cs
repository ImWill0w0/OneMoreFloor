using Sandbox;
using System;

namespace OneMoreFloor
{
	[Library]
	public partial class OMFPlayer : Player, ICanRideElevator
	{
		private DamageInfo lastDamage;

		[Net, Predicted] public ICamera MainCamera { get; set; }
		public ICamera LastCamera { get; set; }

		private DateTimeOffset timeSinceBgmStart;
		private Sound bgm;
		private bool isBgmActive = false;
		private int bgmDuration;

		public OMFPlayer()
		{
			Inventory = new Inventory( this );
		}

		public override void Spawn()
		{
			MainCamera = new FirstPersonCamera();
			LastCamera = MainCamera;

			base.Spawn();
		}

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new OMFWalkController();
			Animator = new StandardPlayerAnimator();

			MainCamera = LastCamera;
			Camera = MainCamera;

			if ( DevController is NoclipController )
			{
				DevController = null;
			}

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			this.Dress();

			Inventory.Add( new arm(), true );

			base.Respawn();
		}

		[ClientRpc]
		public void PlayFloorBgm( Vector3 origin, string bgmPath, int durationMs )
		{
			Host.AssertClient();

			Log.Info( "[C] Playing BGM: " + bgmPath );

			this.timeSinceBgmStart = DateTimeOffset.Now;
			this.isBgmActive = true;
			this.bgmDuration = durationMs;

			this.bgm.Stop();

			this.bgm = Sound.FromWorld( bgmPath, origin );
			this.bgm.SetVolume( 1.0f );
		}

		/// <summary>
		/// Called every tick, clientside and serverside.
		/// </summary>
		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			if ( Input.ActiveChild != null )
			{
				ActiveChild = Input.ActiveChild;
			}

			if ( LifeState != LifeState.Alive )
				return;

			this.TickPlayerUse();
			SimulateActiveChild( cl, ActiveChild );

			if ( IsServer && Input.Pressed( InputButton.Attack1 ) )
			{
				// TODO: Watch, cough
			}

			if ( IsClient )
			{
				if ( this.isBgmActive && (DateTimeOffset.Now - this.timeSinceBgmStart).TotalMilliseconds > this.bgmDuration )
				{
					this.isBgmActive = false;
					this.bgm.SetVolume( 0 );
				}

				DebugOverlay.ScreenText( 10, "BGM Active: " + this.isBgmActive );
				DebugOverlay.ScreenText( 11, "For: " + (DateTimeOffset.Now - this.timeSinceBgmStart).TotalMilliseconds + " / " + this.bgmDuration );
			}
		}

		#region Use

		public bool IsUseDisabled()
		{
			return ActiveChild is IUse use && use.IsUsable( this );
		}

		protected override Entity FindUsable()
		{
			if ( IsUseDisabled() )
				return null;

			// First try a direct 0 width line
			var tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * (85 * Scale) )
				.HitLayer( CollisionLayer.Debris )
				.Ignore( this )
				.Run();

			DebugOverlay.TraceResult( tr );

			// Nothing found, try a wider search
			if ( !IsValidUseEntity( tr.Entity ) )
			{
				tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * (85 * Scale) )
					.Radius( 2 )
					.HitLayer( CollisionLayer.Debris )
					.Ignore( this )
					.Run();

				DebugOverlay.TraceResult( tr );
			}

			// Still no good? Bail.
			if ( !IsValidUseEntity( tr.Entity ) ) return null;

			return tr.Entity;
		}

		protected override void UseFail()
		{
			if ( IsUseDisabled() )
				return;

			base.UseFail();
		}

		#endregion

		public override void OnKilled()
		{
			base.OnKilled();

			if ( lastDamage.Flags.HasFlag( DamageFlags.Vehicle ) )
			{
				Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position );
				Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
				PlaySound( "kersplat" );
			}

			BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );
			LastCamera = MainCamera;
			MainCamera = new SpectateRagdollCamera();
			Camera = MainCamera;
			Controller = null;

			EnableAllCollisions = false;
			EnableDrawing = false;

			Inventory.DropActive();
			Inventory.DeleteContents();
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
			{
				info.Damage *= 10.0f;
			}

			lastDamage = info;

			//TookDamage( lastDamage.Flags, lastDamage.Position, lastDamage.Force );

			base.TakeDamage( info );
		}
	}
}
