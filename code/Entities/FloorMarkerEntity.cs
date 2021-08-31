using System.Linq;
using Sandbox;

namespace OneMoreFloor.Entities
{
    [Library("ent_omf_floormarker", Title = "OMF Floor Marker", Description = "Marks an elevator floor for One More Floor.", Editable = true, Spawnable = true, Icon = "attribution")]
    [Hammer.EditorSprite("editor/omf_floormarker.vmat")]
    public partial class FloorMarkerEntity : Entity
    {
	    private const int TeleportDistanceUnits = 120;

	    private static Vector3 TeleportExtents => new Vector3( 100, 100, 100 );

	    /// <summary>
	    /// Fires when a teleport to this floor is activated.
	    /// </summary>
	    private Output OnArrival { get; set; }

	    [Property( "islobby", Title = "Is Lobby floor" )]
	    public bool IsLobby { get; set; }

	    [Event.Tick]
	    private void Tick()
	    {
		    DebugOverlay.Box( Position, -TeleportExtents, TeleportExtents, Color.White, depthTest: false);
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
	        var nextFloor = OneMoreFloorGame.Instance.GetNextFloor();

	        Log.Info( $"[S] Teleporting {eligibleToTeleport.Count} entities to {nextFloor.EntityName}" );

	        foreach ( var teleEnt in eligibleToTeleport )
	        {
		        var localTransform = Transform.ToLocal( teleEnt.Transform );
		        var newTransform = nextFloor.Transform.ToWorld( localTransform );

		        teleEnt.Position = newTransform.Position;
	        }

	        var _ = nextFloor.OnArrival.Fire( this );
        }
    }
}
