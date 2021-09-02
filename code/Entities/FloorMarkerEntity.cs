﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace OneMoreFloor.Entities
{
    [Library("ent_omf_floormarker", Title = "OMF Floor Marker", Description = "Marks an elevator floor for One More Floor.", Editable = true, Spawnable = true, Icon = "attribution")]
    [Hammer.EditorSprite("editor/omf_floormarker.vmat")]
    public partial class FloorMarkerEntity : Entity
    {
	    [Net]
	    public bool IsOccupied { get; private set; }

	    [Net]
	    public DoorEntity Door { get; private set; }

	    private static Vector3 TeleportExtents => new( 120, 120, 120 );

	    #region Hammer Properties

	    /// <summary>
	    /// Fires when a teleport to this floor is activated.
	    /// </summary>
	    private Output OnArrival { get; set; }

	    /// <summary>
	    /// Whether or not this floor is the lobby.
	    /// </summary>
	    [Property( "islobby", Title = "Is Lobby floor" )]
	    public bool IsLobby { get; set; }

	    /// <summary>
	    /// Whether or not this floor is the lobby.
	    /// </summary>
	    [Property( "istop", Title = "Is Top floor" )]
	    public bool IsTop { get; set; }

	    /// <summary>
	    /// This floor's door entity.
	    /// </summary>
	    [Property( "door_target", FGDType = "target_destination", Title = "Door Entity")]
	    public string DoorEntityName { get; set; }

	    /// <summary>
	    /// BGM for this floor. Will be faded in and out for each player in the floor.
	    /// </summary>
	    [Property( "floor_bgm", Group = "Sounds", FGDType = "sound", Title = "Floor BGM" )]
	    public string FloorBgm { get; set; } = "";

	    /// <summary>
	    /// The origin of the BGM.
	    /// </summary>
	    [Property( "bgm_origin_target", Group = "Sounds", FGDType = "target_destination", Title = "BGM Origin Entity")]
	    public string BgmOriginName { get; set; }

	    #endregion

	    public override void Spawn()
	    {
		    base.Spawn();

		    if ( IsServer )
		    {
			    if ( string.IsNullOrWhiteSpace( DoorEntityName ) )
			    {
				    Log.Error( $"Door entity for {EntityName} not set!" );
				    return;
			    }

			    var ent = FindByName( DoorEntityName );
			    if ( ent is not DoorEntity doorEnt )
			    {
				    Log.Error( $"Could not find door entity for {EntityName}!" );
				    return;
			    }

			    Door = doorEnt;
			    doorEnt.AddOutputEvent( "OnFullyOpen", this.OnDoorOpen );
			    doorEnt.AddOutputEvent( "OnFullyClosed", this.OnDoorClosed );
		    }
	    }

	    private ValueTask OnDoorOpen( Entity activator, float delay )
	    {
		    Log.Info( $"{EntityName} OnDoorOpen!" );

		    var eligible = this.GetEntsInReach().OfType<OMFPlayer>();
		    var bgmOrigin = FindByName( BgmOriginName, this );

		    foreach (var entity in eligible)
		    {
			    if ( !string.IsNullOrWhiteSpace( FloorBgm ) )
			    {
				    entity.PlayFloorBgm( To.Single( entity ), bgmOrigin.Position, FloorBgm );
			    }
		    }

		    return ValueTask.CompletedTask;
	    }

	    private ValueTask OnDoorClosed( Entity activator, float delay )
	    {
		    Log.Info( $"{EntityName} OnDoorClosed!" );

		    var eligible = this.GetEntsInReach().OfType<OMFPlayer>();

		    foreach (var entity in eligible)
		    {
			    entity.StopFloorBgm( To.Single( entity ) );
		    }

		    return ValueTask.CompletedTask;
	    }

	    // TODO: Use Physics and a BBox?
	    private IList<Entity> GetEntsInReach()
	    {
		    // Get ICanRideElevator entities in range to teleport
		    return All.Where( x =>
		    {
			    var localTransform = Transform.ToLocal( x.Transform );
			    if ( localTransform.Position.x > TeleportExtents.x ||
			         localTransform.Position.y > TeleportExtents.y ||
			         localTransform.Position.z > TeleportExtents.z ||
			         localTransform.Position.x < -TeleportExtents.x ||
			         localTransform.Position.y < -TeleportExtents.y ||
			         localTransform.Position.z < -TeleportExtents.z )
			    {
				    return false;
			    }

			    return true;
		    } ).Where(x => x is ICanRideElevator).ToList();
	    }

	    [Event.Tick]
	    private void Tick()
	    {
		    DebugOverlay.Box( Position, -TeleportExtents, TeleportExtents, Color.White, depthTest: false);
		    DebugOverlay.Text( Position, $"Floor: {EntityName}\nOccupied: {IsOccupied}", Color.White );
	    }

	    /// <summary>
        /// Activates teleport to an in this session unseen floor.
        /// </summary>
        [Input]
        public void Teleport( Entity activator = null )
        {
	        if ( !IsServer )
		        return;

	        // Get ICanRideElevator entities in range to teleport
	        var eligibleToTeleport = this.GetEntsInReach();
	        var nextFloor = OneMoreFloorGame.Instance.GetNextFloor( this );

	        Log.Info( $"[S] Teleporting {eligibleToTeleport.Count} entities to {nextFloor.EntityName}" );

	        foreach ( var teleEnt in eligibleToTeleport )
	        {
		        var localTransform = Transform.ToLocal( teleEnt.Transform );
		        var newTransform = nextFloor.Transform.ToWorld( localTransform );

		        teleEnt.Position = newTransform.Position;
	        }

	        if (!nextFloor.IsLobby)
				nextFloor.IsOccupied = true;

	        if ( eligibleToTeleport.Count > 0 )
	        {
		        var _ = nextFloor.OnArrival.Fire( this );
	        }

	        IsOccupied = false;
        }
    }
}
