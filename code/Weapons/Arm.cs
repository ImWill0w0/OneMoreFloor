using Sandbox;

[Library( "weapon_arm", Title = "arm", Spawnable = true )]
partial class arm : Weapon
{
	//public override string ViewModelPath => "models/weapons/v_crowbar.vmdl";
	public override float PrimaryRate => 3.5f;


	public override void Spawn()
	{
		base.Spawn();

		//SetModel( "models/weapons/w_crowbar.vmdl" );
	}

	public override void CreateViewModel()
	{
		base.CreateViewModel();
	}
	public override bool CanReload()
	{
		return false;
	}

	public override void AttackPrimary()
	{

		(Owner as AnimEntity)?.SetAnimBool("b_attack", true);
		if ( MeleeAttack() )
		{
			OnMeleeHit();
		}
		else
		{
			OnMeleeMiss();
		}

		PlaySound( "crowbar" ); 
	}

	private bool MeleeAttack()
	{
		var forward = Owner.EyeRot.Forward;
		forward = forward.Normal;

		bool hit = false;

		foreach ( var tr in TraceBullet( Owner.EyePos, Owner.EyePos + forward * 80, 20.0f ) )
		{
			if ( !tr.Entity.IsValid() ) continue;

			tr.Surface.DoBulletImpact( tr );

			hit = true;

			if ( !IsServer ) continue;

			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100, 25 )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}

		return hit;
	}

	[ClientRpc]
	private void OnMeleeMiss()
	{
		Host.AssertClient();

		if ( IsLocalPawn )
		{
			_ = new Sandbox.ScreenShake.Perlin();
		}

		ViewModelEntity?.SetAnimBool( "miss", true );
	}

	[ClientRpc]
	private void OnMeleeHit()
	{
		Host.AssertClient();

		if ( IsLocalPawn )
		{
			_ = new Sandbox.ScreenShake.Perlin( 1.0f, 1.0f, 3.0f );
		}

		ViewModelEntity?.SetAnimBool("fire", true );
	}

	public override void SimulateAnimator(PawnAnimator anim)
	{
		anim.SetParam("holdtype", 4); // TODO this is shit
		anim.SetParam("aimat_weight", 1.0f);
		anim.SetParam("holdtype_handedness", 1);
		anim.SetParam("holdtype_pose_hand", 0.07f);
	}
}
