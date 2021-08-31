using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OneMoreFloor.Entities;

namespace OneMoreFloor
{
	public partial class OneMoreFloorGame : Sandbox.Game
	{
		public static OneMoreFloorGame Instance { get; private set; }

		private List<string> seenFloors = new();

		public OneMoreFloorGame()
		{
			Instance = this;

			if ( IsServer )
			{
				Log.Info( "My Gamemode Has Created Serverside!" );

				new OMFHudEntity();
			}

			if ( IsClient )
			{
				Log.Info( "My Gamemode Has Created Clientside!" );
			}
		}

		public FloorMarkerEntity GetNextFloor()
		{
			var floors = All.OfType<FloorMarkerEntity>();
			var floorsUnseen = floors.Where( x => !this.seenFloors.Contains( x.EntityName ) );

			if ( !floorsUnseen.Any() )
			{
				Log.Info( "[S] No more unseen floors! Choosing random floor..." );
				return floors.Random();
			}

			var nextFloor = floorsUnseen.Random();
			this.seenFloors.Add( nextFloor.EntityName );
			return nextFloor;
		}

		/// <summary>
		/// A client has joined the server. Make them a pawn to play with
		/// </summary>
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new OMFPlayer();
			client.Pawn = player;

			player.Respawn();
		}
	}

}
