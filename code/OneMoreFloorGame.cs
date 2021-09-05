using System;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using OneMoreFloor.Entities;
using OneMoreFloor.Hud;
using OneMoreFloor.Player;

namespace OneMoreFloor
{
	public partial class OneMoreFloorGame : Sandbox.Game
	{
		public static OneMoreFloorGame Instance { get; private set; }

		[ServerVar( "omf_floors_until_top", Help = "Number of floors until players are forced to the top floor.")]
		public static int NumFloorsUntilTop { get; set; } = 6;

		private List<string> seenFloors = new();
		private int numSeen = 0;

		public OneMoreFloorGame()
		{
			Instance = this;

			if ( IsServer )
			{
				new OMFHudEntity();
			}
		}

		public override void PostLevelLoaded()
		{
			if ( !IsServer )
				return;

			NumFloorsUntilTop = All.OfType<FloorMarkerEntity>().Count() - 3;
			Log.Info( $"[S] Going to end after {NumFloorsUntilTop + 1}" );
		}

		private FloorMarkerEntity GetTopFloor( bool lastResort = false )
		{
			var top = All.OfType<FloorMarkerEntity>().FirstOrDefault( x => x.IsTop );

			if ( top == null )
			{
				throw new InvalidOperationException( "Top floor not found!" );
			}

			if ( top.IsOccupied && !lastResort )
			{
				return null;
			}

			return top;
		}

		private FloorMarkerEntity GetRandomFloor() => All.OfType<FloorMarkerEntity>().Where( x => !x.IsLobby && !x.IsTop && !x.IsOccupied ).Random();
		
		// To consider: Do we want to count the seen floors per player? Reconciliation in multiplayer will be more complicated, but better when people join after the fact.
		public FloorMarkerEntity GetNextFloor()
		{
			var floors = All.OfType<FloorMarkerEntity>().ToList();
			var floorsUnseen = floors.Where( x => !this.seenFloors.Contains( x.EntityName ) && !x.IsLobby && !x.IsTop && !x.IsOccupied ).ToList();

			// If we've seen more floors than set as a goal, force people to the top floor
			if ( this.numSeen > NumFloorsUntilTop )
			{
				Log.Info( "[S] Seen enough floors, sending to top..." );
				var top = this.GetTopFloor();

				if ( top != null )
				{
					return top;
				}
				else
				{
					Log.Info( "[S] Top floor was occupied, going to random floor..." );
					return this.GetRandomFloor();
				}
			}

			if ( !floorsUnseen.Any() )
			{
				Log.Info( "[S] No more unseen floors! Choosing random floor..." );

				var randomFloor = this.GetRandomFloor();
				if ( randomFloor != null )
				{
					return randomFloor;
				}

				// If there's no unoccupied floors for whatever reason, force people to the top floor
				var topFloor = this.GetTopFloor( true );
				return topFloor;
			}

			// Choose a random floor and add it to the list of seen floors
			var nextFloor = floorsUnseen.Random();
			this.seenFloors.Add( nextFloor.EntityName );
			this.numSeen++;
			return nextFloor;
		}

		[ServerCmd("omf_debug_trigger")]
		public static void DebugTrigger(string entName)
		{
			var target = All.OfType<FloorMarkerEntity>().FirstOrDefault( x => x.EntityName == entName );

			if ( target == null )
			{
				Log.Error( $"Could not find entity with name '{entName}'" );
				return;
			}

			target.Teleport();
		}

		[ServerCmd("omf_debug_teleport")]
		public static void DebugTeleport(string entName)
		{
			var target = All.OfType<FloorMarkerEntity>().FirstOrDefault( x => x.EntityName == entName );

			if ( target == null )
			{
				Log.Error( $"Could not find entity with name '{entName}'" );
				return;
			}

			foreach (var client in Client.All)
			{
				client.Pawn.Position = target.Position;
			}

			var _ = target.OnArrival.Fire( null, 10.0f );
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
