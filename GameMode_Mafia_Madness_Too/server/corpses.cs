//corpses.cs
//Handles spawning, handling, and investigation of corpses.
//See the script in the roles folder for fingerprint expert functionality.

$MM::LoadedCorpses = true;

$MM::CauseOfDeath[0] = "Murder";
$MM::CauseOfDeath[1] = "Suicide";
$MM::CauseOfDeath[2] = "Falling";

$MM::CorpseThrowSpeed = 5;
$MM::CorpseGrabRange = 8;
$MM::CorpseInvestigationRange = 5;

function AIPlayer::MM_getCorpseName(%this)
{
	if(!%this.isCorpse)
		return "";

	return %this.name;
}

function AIPlayer::MM_getRole(%this)
{
	if(!%this.isCorpse)
		return -1;

	return %this.role;
}

function AIPlayer::MM_getRoleName(%this)
{
	if(!%this.isCorpse)
		return -1;

	if(!isObject(%r = %this.MM_getRole()))
		return -1;

	return %r.getCorpseName();
}

function AIPlayer::MM_onCorpseSpawn(%this, %mini, %client, %killerClient)
{
	//mostly here for other modules and whatnot to hook in
}

function AIPlayer::MM_onCorpsePickUp(%this, %obj)
{

}

function AIPlayer::MM_onCorpseThrow(%this, %obj)
{
	
}

function AIPlayer::MM_Investigate(%this, %client)
{
	if(!%this.isCorpse)
		return false;

	messageClient(%client, '', "\c2Name\c3:" SPC %this.MM_getCorpseName());
	messageClient(%client, '', "\c2Job\c3:" SPC %this.MM_getRoleName());
	messageClient(%client, '', "\c2Cause of death\c3:" SPC $MM::CauseOfDeath[%this.suicide]);
}

function Player::MM_PickUpCorpse(%this, %obj)
{
	if(!%obj.isCorpse)
		return false;

	if(!isObject(%this) || !isObject(%cl = %this.getControllingClient()) || %this != %cl.player || %cl.isGhost || %cl.lives < 1)
		return false;

	if(isObject(%this.heldCorpse))
		%this.MM_ThrowCorpse();

	%this.mountObject(%obj, 0);
	%this.heldCorpse = %obj;

	%obj.MM_onCorpsePickUp(%this);
}

function Player::MM_ThrowCorpse(%this)
{
	if(!isObject(%obj = %this.heldCorpse))
		return;

	%this.mountObject(%obj, 8);
	%obj.dismount();
	%obj.addVelocity(VectorScale(%this.getForwardVector(), $MM::CorpseThrowSpeed));

	%this.heldCorpse = 0;

	%obj.MM_onCorpseThrow(%this);
}

package MM_Corpses
{
	function GameConnection::onDeath(%this, %srcObj, %srcClient, %damageType, %loc)
	{
		if(!isObject(%mini = getMiniGameFromObject(%this)))
			return parent::onDeath(%this, %srcObj, %srcClient, %damageType, %loc);

		if(!%mini.running || !%mini.isMM)
			return parent::onDeath(%this, %srcObj, %srcClient, %damageType, %loc);

		if(%this.player.isGhost)
			return parent::onDeath(%this, %srcObj, %srcClient, %damageType, %loc);

		if(isObject(%this.player.heldCorpse))
		{
			%this.player.heldCorpse.dismount();
			%this.player.heldCorpse = "";
		}

		%suicide = 0;
		if(%srcClient == %this)
			%suicide = 1;
		else if(!isObject(%srcClient))
			%suicide = 2;

		if(%this.player.getName() $= "botCorpse")
		{
			%this.player.setName("oldBotCorpse");
			%corpse = new AIPlayer(botCorpse : oldBotCorpse);
			%corpse.client = 0;
			%notARealDeath = true;
		}
		else
		{
			%corpse = new AIPlayer(botCorpse)
						{
							datablock = PlayerNoJet;

							originalClient = %this;
							name = %this.name;
							role = %this.role;

							timeOfDeath = $Sim::Time;
							causeOfDeath = %suicide;

							isCorpse = true;
						};

			%corpse.MM_onCorpseSpawn(%mini, %this, %srcClient);
		}

		if(%this.player.doombot)
		{
			%corpse.unHideNode("ALL");
			%corpse.doombot = true;
		}

		if(isObject(%img = %this.player.getMountedImage(0)))
			%corpse.mountImage(%img, 0);

		%corpse.setTransform(%this.player.getTransform());

		%this.player.removeBody();

		%corpse.setNodeColor("ALL", "1 0 0 1");
		%corpse.mountImage(BlankImage, 3);
		%corpse.setImageTrigger(3, 1);
		%corpse.playThread(3, "death1");

		if(!%notARealDeath)
		{
			%this.corpse = %corpse;
			%this.lives--;
		}
		else
			%corpse.originalClient.corpse = %corpse;

		%this.camera.setMode("Corpse", %corpse);
		%this.setControlObject(%this.camera);
	}

	function Armor::onTrigger(%this, %obj, %slot, %val)
	{
		if(!isObject(%cl = %obj.getControllingClient()) || !isObject(%mini = getMiniGameFromObject(%cl)) || !$DefaultMinigame.running)
			return parent::onTrigger(%this, %obj, %slot, %val);

		if(%cl.isGhost || %cl.lives < 1)
			return parent::onTrigger(%this, %obj, %slot, %val);

		switch(%slot)
		{
			case 0:
				%start = %obj.getEyePoint();
				%vec = %obj.getEyeVector();
				%end = VectorAdd(%start, VectorScale(%vec, $MM::CorpseInvestigationRange));

				%ray = containerRayCast(%start, %end, $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %obj);
				%hObj = firstWord(%ray);
				if(!isObject(%hObj) || !%hObj.isCorpse || %hObj.getClassName() !$= "AIPlayer")
					return;

				%hObj.MM_Investigate(%cl);

			case 4:
				if(!isObject(%obj.heldCorpse))
				{
					%start = %obj.getEyePoint();
					%vec = %obj.getEyeVector();
					%end = VectorAdd(%start, VectorScale(%vec, $MM::CorpseGrabRange));

					%ray = containerRayCast(%start, %end, $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %obj);
					%hObj = firstWord(%ray);
					if(!isObject(%hObj) || !%hObj.isCorpse || %hObj.getClassName() !$= "AIPlayer")
						return;
				}

				%obj.MM_ThrowCorpse();

				return;
		}

		return parent::onTrigger(%this, %obj, %slot, %val);
	}
};
activatePackage(MM_Corpses);