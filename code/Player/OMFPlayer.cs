using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using OneMoreFloor.Weapons;

namespace OneMoreFloor.Player
{
	[Library]
	public partial class OMFPlayer : Sandbox.Player, ICanRideElevator
	{
		private DamageInfo lastDamage;

		[Net] public PawnController MinigameController { get; set; }
		[Net] public PawnAnimator MinigameAnimator { get; set; }
		[Net, Predicted] public ICamera MinigameCamera { get; set; }
		[Net, Predicted] public Entity Minigame { get; set; }
		
		[Net, Predicted] public ICamera MainCamera { get; set; }
		public ICamera LastCamera { get; set; }

		private Sound bgm;

		public static IEnumerable<OMFPlayer> AllPlayers => Client.All.Select(x => x.Pawn).OfType<OMFPlayer>();
		public static OMFPlayer LocalPlayer => Local.Pawn as OMFPlayer;

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

			// This is the only way I found to no-collide players for now
			// Might have to change something in WalkController (the TraceBBox stuff) to actually do it properly
			EnableAllCollisions = false;
			
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			this.Dress();

			Inventory.Add( new ArmWeapon(), true );

			base.Respawn();
		}

		[ClientRpc]
		public void PlayFloorBgm( Vector3 origin, string bgmPath )
		{
			Host.AssertClient();

			Log.Info( "[C] Playing BGM: " + bgmPath );

			this.bgm.Stop();

			this.bgm = Sound.FromWorld( bgmPath, origin );
			this.bgm.SetVolume( 0.5f );

			DebugOverlay.Sphere( origin, 5, Color.Green, false, 2000 );
		}

		[ClientRpc]
		public void StopFloorBgm()
		{
			this.bgm.SetVolume( 0 );
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
			
			if ( MinigameController != null && DevController is NoclipController )
			{
				DevController = null;
			}

			this.TickPlayerUse();

			if ( Minigame == null )
			{
				SimulateActiveChild( cl, ActiveChild );
			}
			

			Camera = GetActiveCamera();

			if ( IsClient )
			{
				PlayerGhosting();
			}
		}

		private void PlayerGhosting()
		{
			foreach (OMFPlayer player in AllPlayers)
			{
				if (player == Local.Pawn) continue;

				float distance = player.Position.Distance(Position);

				player.SetAlpha(MathX.Clamp((distance - 25) / 10, 0, 1));
			}
		}

		#region Use

		public bool IsUseDisabled()
		{
			return ActiveChild is IUse use && use.IsUsable( this );
		}

		public Entity GetUsableTrace()
		{
			if ( IsUseDisabled() )
				return null;

			// First try a direct 0 width line
			var tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * (85 * Scale) )
				.HitLayer( CollisionLayer.Debris )
				.Ignore( this )
				.Run();

			// DebugOverlay.TraceResult( tr );

			// Nothing found, try a wider search
			if ( !IsValidUseEntity( tr.Entity ) )
			{
				tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * (85 * Scale) )
					.Radius( 2 )
					.HitLayer( CollisionLayer.Debris )
					.Ignore( this )
					.Run();

				// DebugOverlay.TraceResult( tr );
			}

			// Still no good? Bail.
			if ( !IsValidUseEntity( tr.Entity ) ) return null;

			return tr.Entity;
		}

		protected override Entity FindUsable()
		{
			return GetUsableTrace();
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
			
			MinigameController = null;
			MinigameAnimator = null;
			MinigameCamera = null;
			Minigame = null;

			EnableAllCollisions = false;
			EnableDrawing = false;

			Inventory.DropActive();
			Inventory.DeleteContents();
		}

		public override PawnController GetActiveController()
		{
			if ( this.MinigameController != null )
			{
				return this.MinigameController;
			}

			if ( this.DevController != null )
			{
				return this.DevController;
			}

			return base.GetActiveController();
		}
		
		public override PawnAnimator GetActiveAnimator()
		{
			if ( this.MinigameAnimator != null )
			{
				return this.MinigameAnimator;
			}

			return base.GetActiveAnimator();
		}
		
		public ICamera GetActiveCamera()
		{
			if ( this.MinigameCamera != null )
			{
				return this.MinigameCamera;
			}

			return this.MainCamera;
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

		private void SetAlpha(float alpha)
		{
			RenderAlpha = alpha;

			foreach (ModelEntity ent in Children.OfType<ModelEntity>())
			{
				ent.RenderAlpha = alpha;
			}
		}
	}
}
