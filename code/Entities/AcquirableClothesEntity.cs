using OneMoreFloor.Player;
using Sandbox;

namespace OneMoreFloor.Entities
{
	[Library("ent_omf_acquirable", Spawnable = true, Description = "An acquirable clothes model that can be worn by the player when used.", Editable = true, Title = "Acquirable")]
	[Hammer.Model]
	public class AcquirableClothesEntity : Prop, IUse, ICanRideElevator
	{
		/// <summary>
		/// The body group this entity is to be attached to when picked up.
		/// </summary>
		[Property( "bodygroup_type", Title = "Target Body Group" )]
		public OMFPlayer.BodyGroup TargetBodyGroup { get; set; } = OMFPlayer.BodyGroup.Head;

		/// <summary>
		/// The model to be used when attached to the player body.
		/// </summary>
		[Property("bodymodel", Title = "Body Model")]
		[FGDType("studio")]
		public string BodyModel { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

			EnableHitboxes = true;
			EnableAllCollisions = true;
			EnableShadowCasting = true;
			EnableDrawing = true;
		}

		public bool OnUse( Entity user )
		{
			if ( IsServer )
			{
				if ( user is OMFPlayer player )
				{
					player.SetBodyModel( TargetBodyGroup, BodyModel );
				}

				this.Delete();
			}

			return true;
		}

		public bool IsUsable( Entity user )
		{
			return user is OMFPlayer;
		}
	}
}
