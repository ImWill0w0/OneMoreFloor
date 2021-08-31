using Sandbox;

namespace OneMoreFloor.Entities
{
    [Library("ent_omf_floormarker", Title = "OMF Floor Marker", Description = "Marks an elevator floor for One More Floor.", Editable = true, Spawnable = true, Icon = "attribution")]
    public class FloorMarkerEntity : Entity
    {
        /// <summary>
        /// Activates teleport to an in this session unseen floor.
        /// </summary>
        [Input]
        public void Teleport( Entity activator = null )
        {

        }
    }
}
