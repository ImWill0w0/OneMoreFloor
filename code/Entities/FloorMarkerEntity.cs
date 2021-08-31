using System.Linq;
using Sandbox;

namespace OneMoreFloor.Entities
{
    [Library("ent_omf_floormarker", Title = "OMF Floor Marker", Description = "Marks an elevator floor for One More Floor.", Editable = true, Spawnable = true, Icon = "attribution")]
    public partial class FloorMarkerEntity : Entity
    {
	    private const int TeleportDistanceUnits = 300;

	    /// <summary>
	    /// Fires when a teleport to this floor is activated.
	    /// </summary>
	    protected Output OnArrival { get; set; }

        /// <summary>
        /// Activates teleport to an in this session unseen floor.
        /// </summary>
        [Input]
        public void Teleport( Entity activator = null )
        {
	        if ( !IsServer )
		        return;

	        // Get ICanRideElevator entities in range to teleport
	        var eligibleToTeleport = All.Where( x => x.Position.Distance( this.Position ) <= TeleportDistanceUnits ).OfType<ICanRideElevator>();
	        var nextFloor = OneMoreFloorGame.Instance.GetNextFloor();

	        Log.Info( $"[S] Teleporting {eligibleToTeleport.Count()} entities to {nextFloor.EntityName}" );

	        var _ = nextFloor.OnArrival.Fire( this );
        }
    }
}
