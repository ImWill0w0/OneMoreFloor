using System;
using Sandbox;

namespace OneMoreFloor.Player
{
	public partial class OMFPlayer
	{
		private ModelEntity pants;
		private ModelEntity jacket;
		private ModelEntity shoes;
		private ModelEntity hat;

		private bool dressed;

		public enum BodyGroup
		{
			Legs,
			Chest,
			Feet,
			Head
		}

		public void SetBodyModel( BodyGroup group, string model )
		{
			Host.AssertServer();

			ModelPropData? propInfo = null;

			switch ( group )
			{
				case BodyGroup.Legs:
					this.pants.Delete();

					this.pants = new ModelEntity();
					this.pants.SetModel( model );
					this.pants.SetParent( this, true );
					this.pants.EnableShadowInFirstPerson = true;
					this.pants.EnableHideInFirstPerson = true;

					break;
				case BodyGroup.Chest:
					this.jacket.Delete();

					this.jacket = new ModelEntity();
					this.jacket.SetModel( model );
					this.jacket.SetParent( this, true );
					this.jacket.EnableShadowInFirstPerson = true;
					this.jacket.EnableHideInFirstPerson = true;

					propInfo = this.jacket.GetModel().GetPropData();

					break;
				case BodyGroup.Feet:
					this.shoes.Delete();

					this.shoes = new ModelEntity();
					this.shoes.SetModel( model );
					this.shoes.SetParent( this, true );
					this.shoes.EnableShadowInFirstPerson = true;
					this.shoes.EnableHideInFirstPerson = true;
					break;
			}

			this.SetBodyGroup( "Legs", 1 );

			if ( propInfo.HasValue && propInfo.Value.ParentBodyGroupName != null )
			{
				this.SetBodyGroup( propInfo.Value.ParentBodyGroupName, propInfo.Value.ParentBodyGroupValue );
			}
			else
			{
				this.SetBodyGroup( "Chest", 0 );
			}

			this.SetBodyGroup( "Feet", 1 );

			if ( group == BodyGroup.Head )
			{
				this.hat.Delete();

				this.hat = new ModelEntity();
				this.hat.SetModel( model );
				this.hat.SetParent( this, true );
				this.hat.EnableShadowInFirstPerson = true;
				this.hat.EnableHideInFirstPerson = true;
			}
		}

		public void Dress()
		{
			if ( this.dressed )
			{
				return;
			}

			this.dressed = true;

			if ( true )
			{
				var model = Rand.FromArray( new[]
				{
					"models/citizen_clothes/trousers/trousers.jeans.vmdl",
					"models/citizen_clothes/trousers/trousers.lab.vmdl",
					"models/citizen_clothes/trousers/trousers.police.vmdl",
					"models/citizen_clothes/trousers/trousers.smart.vmdl",
					"models/citizen_clothes/trousers/trousers.smarttan.vmdl",
					"models/citizen_clothes/trousers/trousers_tracksuitblue.vmdl",
					"models/citizen_clothes/trousers/trousers_tracksuit.vmdl",
					"models/citizen_clothes/shoes/shorts.cargo.vmdl"
				} );

				this.pants = new ModelEntity();
				this.pants.SetModel( model );
				this.pants.SetParent( this, true );
				this.pants.EnableShadowInFirstPerson = true;
				this.pants.EnableHideInFirstPerson = true;

				this.SetBodyGroup( "Legs", 1 );
			}

			if ( true )
			{
				var model = Rand.FromArray( new[]
				{
					"models/citizen_clothes/jacket/labcoat.vmdl", "models/citizen_clothes/jacket/jacket.red.vmdl",
					"models/citizen_clothes/jacket/jacket.tuxedo.vmdl",
					"models/citizen_clothes/jacket/jacket_heavy.vmdl"
				} );

				this.jacket = new ModelEntity();
				this.jacket.SetModel( model );
				this.jacket.SetParent( this, true );
				this.jacket.EnableShadowInFirstPerson = true;
				this.jacket.EnableHideInFirstPerson = true;

				var propInfo = this.jacket.GetModel().GetPropData();
				if ( propInfo.ParentBodyGroupName != null )
				{
					this.SetBodyGroup( propInfo.ParentBodyGroupName, propInfo.ParentBodyGroupValue );
				}
				else
				{
					this.SetBodyGroup( "Chest", 0 );
				}
			}

			if ( true )
			{
				var model = Rand.FromArray( new[]
				{
					"models/citizen_clothes/shoes/trainers.vmdl",
					"models/citizen_clothes/shoes/shoes.workboots.vmdl"
				} );

				this.shoes = new ModelEntity();
				this.shoes.SetModel( model );
				this.shoes.SetParent( this, true );
				this.shoes.EnableShadowInFirstPerson = true;
				this.shoes.EnableHideInFirstPerson = true;

				this.SetBodyGroup( "Feet", 1 );
			}

			if ( true )
			{
				var model = Rand.FromArray( new[]
				{
					"models/citizen_clothes/hat/hat_hardhat.vmdl", "models/citizen_clothes/hat/hat_woolly.vmdl",
					"models/citizen_clothes/hat/hat_securityhelmet.vmdl",
					"models/citizen_clothes/hair/hair_malestyle02.vmdl",
					"models/citizen_clothes/hair/hair_femalebun.black.vmdl",
					"models/citizen_clothes/hat/hat_beret.red.vmdl", "models/citizen_clothes/hat/hat.tophat.vmdl",
					"models/citizen_clothes/hat/hat_beret.black.vmdl", "models/citizen_clothes/hat/hat_cap.vmdl",
					"models/citizen_clothes/hat/hat_leathercap.vmdl",
					"models/citizen_clothes/hat/hat_leathercapnobadge.vmdl",
					"models/citizen_clothes/hat/hat_securityhelmetnostrap.vmdl",
					"models/citizen_clothes/hat/hat_service.vmdl",
					"models/citizen_clothes/hat/hat_uniform.police.vmdl",
					"models/citizen_clothes/hat/hat_woollybobble.vmdl"
				} );

				this.hat = new ModelEntity();
				this.hat.SetModel( model );
				this.hat.SetParent( this, true );
				this.hat.EnableShadowInFirstPerson = true;
				this.hat.EnableHideInFirstPerson = true;
			}
		}

		public void SetClothingAlpha(float alpha)
		{
			if (this.pants != null) this.pants.RenderAlpha = alpha;
			if (this.jacket != null) this.jacket.RenderAlpha = alpha;
			if (this.shoes != null) this.shoes.RenderAlpha = alpha;
			if (this.hat != null) this.hat.RenderAlpha = alpha;
		}
	}
}
