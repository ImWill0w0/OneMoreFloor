using System.Collections.Generic;
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
	    /// BGM for this floor. Will be faded in and out for each player in the floor.
	    /// </summary>
	    [Property( "floor_bgm", Group = "Sounds", FGDType = "sound", Title = "Floor BGM" )]
	    public string FloorBgm { get; set; } = "";

	    #endregion

	    /// <summary>
	    /// Stop the elevator BGM.
	    /// </summary>
	    [Input]
	    public void StopElevatorBgm( Entity activator = null )
	    {
		    if ( !IsServer )
			    return;

		    Log.Info( $"StopElevatorBgm in {EntityName}!" );

		    var eligible = this.GetEntsInReach().OfType<OMFPlayer>();

		    foreach (var entity in eligible)
		    {
			    entity.StopFloorBgm( To.Single( entity ) );
		    }
	    }

	    /// <summary>
	    /// Start the elevator BGM.
	    /// </summary>
	    [Input]
	    public void StartElevatorBgm( Entity activator = null )
	    {
		    if ( !IsServer )
			    return;

		    Log.Info( $"StartElevatorBgm in {EntityName}!" );

		    var eligible = this.GetEntsInReach().OfType<OMFPlayer>();

		    foreach (var entity in eligible)
		    {
			    if ( !string.IsNullOrWhiteSpace( FloorBgm ) )
			    {
				    entity.PlayFloorBgm( To.Single( entity ), Position, FloorBgm );
			    }
		    }
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
