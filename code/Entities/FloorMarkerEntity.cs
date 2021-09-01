using System.Linq;
using Sandbox;

namespace OneMoreFloor.Entities
{
    [Library("ent_omf_floormarker", Title = "OMF Floor Marker", Description = "Marks an elevator floor for One More Floor.", Editable = true, Spawnable = true, Icon = "attribution")]
    [Hammer.EditorSprite("editor/omf_floormarker.vmat")]
    public partial class FloorMarkerEntity : Entity
    {
	    public bool IsOccupied { get; set; }

	    private static Vector3 TeleportExtents => new( 120, 120, 120 );

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

	    /// <summary>
	    /// The duration this floor's BGM should be played for.
	    /// </summary>
	    [Property( "bgm_duration", Group = "Sounds", Title = "BGM Duration(ms)" )]
	    public int BgmDuration { get; set; } = 3000;

	    /// <summary>
	    /// The origin of the BGM.
	    /// </summary>
	    [Property( "bgm_origin_target", Group = "Sounds", FGDType = "target_destination", Title = "BGM Origin Entity")]
	    public string BgmOriginName { get; set; }

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
	        var eligibleToTeleport = All.Where( x =>
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
	        var nextFloor = OneMoreFloorGame.Instance.GetNextFloor( this );

	        var bgmOrigin = FindByName( nextFloor.BgmOriginName, this );

	        Log.Info( $"[S] Teleporting {eligibleToTeleport.Count} entities to {nextFloor.EntityName}" );

	        foreach ( var teleEnt in eligibleToTeleport )
	        {
		        var localTransform = Transform.ToLocal( teleEnt.Transform );
		        var newTransform = nextFloor.Transform.ToWorld( localTransform );

		        if ( !string.IsNullOrWhiteSpace( nextFloor.FloorBgm ) && teleEnt is OMFPlayer player )
		        {
			        player.PlayFloorBgm( To.Single( player ), bgmOrigin.Position, nextFloor.FloorBgm, nextFloor.BgmDuration );
		        }

		        teleEnt.Position = newTransform.Position;
	        }

	        if (!nextFloor.IsLobby)
				nextFloor.IsOccupied = true;

	        var _ = nextFloor.OnArrival.Fire( this );

	        IsOccupied = false;
        }
    }
}
