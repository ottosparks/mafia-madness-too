//cmd.cs
//ServerCmds for general use and administration here.
//Role-related commands will be in their respective role scripts.

$MM::LoadedCmd = true;

$MM::AdminToolsMidGame = true;
$MM::NotifyHostMidGame = true;

// $MMGameModeName[0] = "Standard";
// $MMGameModeName[1] = "Classic";
// $MMGameModeName[2] = "I Hate You All";
// $MMGameModeName[3] = "hahahahahahahahahahahaha";
// $MMGameModeName[4] = "Just Try to Survive";
// $MMGameModeName[5] = "Abduct Titanium Tonight";
// $MMGameModeName[6] = "brackets, think of the name -ottosparks (6)"; //wtf is this one
// $MMGameModes = 6;

if(!$MMRulesPrefsLoaded) {
	if(isFile("config/server/mmrulesprefs.cs"))
		exec("config/server/mmrulesprefs.cs");
	$MMRulesPrefsLoaded = 1;
}

function serverCmdMafList(%this)
{
	if(!%this.MM_isMaf())
		return;

	%this.MM_DisplayMafiaList();
}

function serverCmdPlaceAfterLife(%this, %mode, %p0, %p1, %p2, %p3, %p4, %p5)
{
	if(!%this.isSuperAdmin)
		return;

	switch$(%mode)
	{
		case "Location" or "Loc" or "0":
			if(!isObject(%obj = %this.getControlObject()))
			{
				messageClient(%this, '', "\c4Spawn and move your player or camera to where you would like to set the afterlife location, then try again.");
				return;
			}
			else
				%pos = %obj.getPosition();

			$Pref::Server::MMAfterLifeLoc = %pos;
			if(isObject($DefaultMinigame))
				$DefaultMinigame.afterLifeLoc = %pos;

			messageClient(%this, '', "\c4Set the afterlife spawn position to \c3" @ %pos @ "\c6.");

		case "Box" or "1":
			if(!isObject(%obj = %this.getControlObject()))
			{
				messageClient(%this, '', "\c4Spawn and move your player or camera to where you would like to set the corner position, then try again.");
				return;
			}

			if(%this.boxParam $= "")
			{
				%this.boxParam = %obj.getPosition();
				messageClient(%this, '', "\c4Set the first spawn box corner to\c3" SPC %pos @ "\c6. Use the command again to set the second corner.");
				return;
			}

			%box = %this.boxParam SPC (%pos = %obj.getPosition());
			%this.boxParam = "";

			$Pref::Server::MMAfterLifeBox = %box;
			if(isObject($DefaultMinigame))
				$DefaultMinigame.afterLifeBox = %box;

			messageClient(%this, '', "\c4Set the second spawn box corner to\c3" SPC %pos @ "\c6.");

		case "Brick" or "2":
			%bName = "_" @ %p0;

			if(!isObject(%bName))
			{
				messageClient(%this, '', "\c4Could not find any bricks by the name of \c3\'" @ %p0 @ "\'");
				return;
			}

			$Pref::Server::MMAfterLifeBrick = %bName;
			if(isObject($DefaultMinigame))
				$DefaultMinigame.afterLifeBrick = %bName;

			messageClient(%this, '', "\c4Set afterlife spawn brick name to \c3\'" @ %p0 @ "\'");

		default:
			messageClient(%this, '', "\c4Afterlife spawning works a bit differently here! \c6You can make players spawn in three different ways:");
			messageClient(%this, '', "\c3Location \c6Mode: The original method. Players will always spawn at a fixed position in the afterlife.");
			messageClient(%this, '', "\c3Box \c6Mode: Players will randomly spawn between two defined corners.");
			messageClient(%this, '', "\c3Brick \c6Mode: Players will spawn on the defined named brick.");
			messageClient(%this, '', "\c4Or, you can just place after life spawn points and disable the rest! Isn\'t that easier? Why didn\'t I think of that before?");
	}

	messageClient(%this, '', "\c4Use \c3/setAfterLifeMode [mode]\c4to change how people will spawn in the afterlife.");
}

function serverCmdSetAfterLifeMode(%this, %mode)
{
	if(!%this.isSuperAdmin)
		return;

	switch$(%mode)
	{
		case "0" or "Location" or "Loc":
			%mode = "Location";
			%sMode = 0;

		case "1" or "Box":
			%mode = "Box";
			%sMode = 1;

		case "2" or "Brick":
			%mode = "Brick";
			%sMode = 2;

		case "-1" or "None" or "Off" or "Disable":
			messageClient(%this, '', "\c4Afterlife will now use spawn bricks.");

			$Pref::Server::MMAfterLifeMode = -1;
			if(isObject($DefaultMinigame))
				$DefaultMinigame.afterLifeMode = -1;

			return;

		default:
			messageClient(%this, '', "\c4Afterlife spawning works a bit differently here! \c6You can make players spawn in three different ways:");
			messageClient(%this, '', "\c3Location \c6Mode: The original method. Players will always spawn at a fixed position in the afterlife.");
			messageClient(%this, '', "\c3Box \c6Mode: Players will randomly spawn between two defined corners.");
			messageClient(%this, '', "\c3Brick \c6Mode: Players will spawn on the defined named brick.");
			messageClient(%this, '', "\c4Or, you can just place after life spawn points and disable the rest! Isn\'t that easier? Why didn\'t I think of that before?");
			messageClient(%this, '', "\c4Use \c3/placeAfterLife [mode] [parameters]\c4 to apply location or brick name to the spawning mode.");
	}

	messageClient(%this, '', "\c4Afterlife spawn mode set to \c3" @ %mode @ "\c6.");

	$Pref::Server::MMAfterLifeMode = %sMode;
	if(isObject($DefaultMinigame))
		$DefaultMinigame.afterLifeMode = %sMode;
}

function redruls(%n)
{
	if(!isObject(%n))
		%n = findClientByName(%n);
	talk("R:" SPC %n.rules);
	for(%i = 1; %i <= 7; %i++)
		talk(%i SPC %n.rules[%i]);
}

function serverCmdRedRuls(%client, %n, %old)
{
	%oldn = %n;
	%n = findClientByName(%n);
	if(!isObject(%n) && isObject(findClientByBL_ID(%oldn)))
		%n = findClientByBL_ID(%oldn);
	%b = %n.bl_id;
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "<font:impact:24><shadow:-2:-2><shadowcolor:000000>GAME OVER GAME OVER DINO LOSER DINO LOSER");
		return;
	}
	if(!isObject(%n) && !$MMReadRules[%oldn])
	{
		messageClient(%client, '', "\c6This player has no client/record!");
		return;
	}
	if($MMReadRules[%oldn])
		%b = %oldn;
	if(%old)
	{
		messageClient(%client, '', "\c6R:" SPC %n.rules SPC "\c7" @ $MMReadRules[%b]);
		for(%i = 1; %i <= 7; %i++)
			messageClient(%client, '', "\c6" @ %i SPC %n.rules[%i] SPC "\c7" @ $MMReadRules[%b, %i]);
		return;
	}
	messageClient(%client, '', (%n.rules ?
				    "\c3" @ %n.getPlayerName() SPC "\c6has done \c3/rules \c5and read these categories\c6..." :
				    (!$MMReadRules[%b] ? "\c3" @ %n.getPlayerName() SPC "\c6has not done \c3/rules\c6!" :
				    "\c6BL_ID\c3" SPC %b SPC "\c6has done \c3/rules \c5at some point and read these categories\c6...")));
	if(!%n.rules && !$MMReadRules[%b])
		return;
	for(%i = 1; %i <= 7; %i++)
		%str = %str @ (%n.rules[%i] ? " " @ %i : "");
	%str = %str @ "\c7";
	for(%i = 1; %i <= 7; %i++)
		%str = %str @ ($MMReadRules[%b, %i] ? " " @ %i : "");
	messageClient(%client, '', "\c6" @ trim(%str));
}

function serverCmdMMManualRole(%client, %player, %role)
{
	if(!%client.isSuperAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%client, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	if(!isObject(%role = $MM::RoleKey[%role]))
	{
		messageClient(%client, '', "\c4That role doesn\'t exist!");
		return;
	}

	%player = findClientByName(%player);

	%player.manualRole = %role;
}

function serverCmdMMRole(%client, %player, %role)
{
	if(!%client.isSuperAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%client, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	if(!isObject(%role = $MM::RoleKey[%role]))
	{
		messageClient(%client, '', "\c4That role doesn\'t exist!");
		return;
	}

	%player = findClientByName(%player);

	%player.MM_SetRole(%role);
}

function serverCmdMMSetIgnore(%client, %player, %i)
{
	if(!%client.isSuperAdmin && !%client.isAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%client, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	%player = findClientByName(%player);
	%n = %player.getPlayerName();
	if(%i $= "")
		%player.MMIgnore = !%player.MMIgnore;
	else
		%player.MMIgnore = %i;
	
	if(%player.MMIgnore)
		messageAll('', "\c3" @ %n SPC "\c1is no longer being included in Mafia Madness games.");
	else
		messageAll('', "\c3" @ %n SPC "\c1is now being included in Mafia Madness games.");
}

function serverCmdMMSetGameMode(%client, %mode)
{
	if(!%client.isSuperAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%client, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	if(%mode >= $MM::GameModes)
	{
		messageClient(%client, '', "\c1OUT OF RANGE");
		return;
	}
	messageAll('', "\c1Mafia Madness game mode set to \c3" @ $MM::GameMode[$DefaultMinigame.gameMode = %mode] @ "\c1.");
}

// function serverCmdMMManualGame(%client, %i)
// {
// 	if(!%client.isSuperAdmin)
// 		return;

// 	$MMManualGame = (%i $= "" ? !$MMManualGame : %i);
// }

function serverCmdMMReqInfo(%client, %targ, %type, %flag)
{
	if(!%client.isAdmin && !%client.isSuperAdmin)
		return;
	%targ = findClientByName(%targ);
	switch(%type)
	{
		case 0: commandToClient(%client, 'MMRecInfo', %type, %targ.MMIgnore, %flag);
		case 1: commandToClient(%client, 'MMRecInfo', %type, %targ.manualRole, %flag);
		case 2: commandToClient(%client, 'MMRecInfo', %type, %targ.role, %flag);
		case 3: commandToClient(%client, 'MMRecInfo', %type, $MMManualGame, %flag);
	}
}

function serverCmdStartMM(%this)
{
	if(!%this.isSuperAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%this, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	if(!$DefaultMinigame.running)
	{
		$DefaultMinigame.MM_InitRound();

		echo("MM round started by" SPC %this.getPlayerName() SPC "(" @ %this.getBLID() @ ")");
	}
}

function serverCmdStopMM(%this)
{
	if(!%this.isSuperAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%this, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	if($DefaultMinigame.running)
	{
		$DefaultMinigame.MM_Stop();

		echo("MM round topped by" SPC %this.getPlayerName() SPC "(" @ %this.getBLID() @ ")");
	}
}

function serverCmdDediMM(%this)
{
	if(!%this.isSuperAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%this, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	$DefaultMinigame.MMDedi ^= 1;

	if($DefaultMinigame.MMDedi)
	{
		talk("Mafia Madness is now running in Dedicated mode.");

		if(!$DefaultMinigame.MMGame)
			$DefaultMinigame.MM_InitRound();

		echo("MM put into dedicated mode by" SPC %this.getPlayerName() SPC "(" @ %this.getBLID() @ ")");
	}
	else
	{
		talk("Mafia Madness is no longer in Dedicated mode.");

		cancel($DefaultMinigame.MMNextGame);

		echo("MM taken out of dedicated mode by" SPC %this.getPlayerName() SPC "(" @ %this.getBLID() @ ")");
	}
}

function serverCmdMMKillList(%this)
{
	if(!%this.isAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%this, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	if(%this.lives > 0 && !%this.isGhost && !$MM::AdminToolsMidGame)
	{
		messageClient(%this, '', "\c4Admin tools are currently disabled for living players.");
		return;
	}

	$DefaultMinigame.MM_ChatEventLog(%this);

	if(isObject(%host = findClientByBL_ID(getNumKeyID())) && !(%host.lives > 0 && !%host.isGhost && !$MM::NotifyHostMidGame))
		messageClient(%host, '', "\c3" @ %this.getPlayerName() SPC "\c5accessed the kills list!");
}

function serverCmdEventLog(%this)
{
	serverCmdMMKillList(%this);
}

function serverCmdKillList(%this)
{
	serverCmdMMKillList(%this);
}

function serverCmdMMRoleList(%this)
{
	if(!%this.isAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%this, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	if(%this.lives > 0 && !%this.isGhost && !$MM::AdminToolsMidGame)
	{
		messageClient(%this, '', "\c4Admin tools are currently disabled for living players.");
		return;
	}

	for(%i = 0; %i < $DefaultMinigame.memberCacheLen; %i++)
	{
		%role = $DefaultMinigame.role[$DefaultMinigame.memberCache[%i]];
		%name = $DefaultMinigame.memberCacheName[%i];
		%c = %role.getColour();

		messageClient(%this, '', %c @ %name SPC "\c6(" @ %c @ %role.getRoleName() @ "\c6)");
	}

	if(isObject(%host = findClientByBL_ID(getNumKeyID())) && !(%host.lives > 0 && !%host.isGhost && !$MM::NotifyHostMidGame))
		messageClient(%host, '', "\c3" @ %this.getPlayerName() SPC "\c5accessed the roles list!");
}

function serverCmdMMIgnoreMe(%this)
{
	if(!%this.isAdmin)
		return;

	if(!isObject(%mini = getMinigameFromObject(%this)))
		return;

	%this.MMIgnore ^= 1;

	messageAll('', "\c3" @ %this.getPlayerName() SPC "\c1is" SPC (!%this.MMIgnore ? "now" : "no longer") SPC "being included in Mafia Madness games.");
}

//TODO: move this to vent role script instead, move functionality to a method, and add /impU as well
function serverCmdImp(%this, %v0, %v1, %v2, %v3, %v4)
{
	if(!$DefaultMinigame.running || !isObject(%mini = getMinigameFromObject(%this)) || !%mini.isMM)
		return;

	if(!%mini.MM_canImp(%this))
		return;

	%v = trim(%v0 SPC %v1 SPC %v2 SPC %v3 SPC %v4);
	%c = findClientByName(%v);

	if(%v $= "" || %c == %this)
	{
		%this.MMImpersonate = "";
		%this.MMUnnoticeable = false;
		messageClient(%this, '', "\c4You are no longer impersonating anyone.");

		return;
	}

	if(!isObject(%this))
	{
		%c = findClientByBL_ID($Pref::Server::MMNicknames[%v]);
		if(!isObject(%c))
		{
			messageClient(%this, '', "\c4Could not locate client\c3" SPC %v @ "\c4.");
			return;
		}
	}

	if((%c.MMIgnore && %c.lives < 1) || %c.minigame != %mini)
	{
		messageClient(%this, '', "\c4That client is not part of the game!");
		return;
	}

	%this.MMImpersonate = %c;
	%this.MMUnnoticeable = false;

	messageClient(%this, '', "\c4You are now impersonating\c3" SPC %c.getPlayerName() @ "\c4!");
}

function serverCmdGetNickname(%this, %v0, %v1, %v2, %v3, %v4)
{
	%v = trim(%v0 SPC %v1 SPC %v2 SPC %v3 SPC %v4);

	%blid = $Pref::Server::MMNicknames[%v];
	if(%blid $= "")
	{
		messageClient(%this, '', "\c3The nickname of\c6" SPC %v SPC "\c3could not be found.");
		return;
	}

	%cl = findClientByBL_ID(%blid);

	if(isObject(%cl))
		messageClient(%client,'',"\c3The nickname of\c6" SPC %v SPC "\c3belongs to\c6" SPC %cl.getPlayerName() SPC "\c3.");
	else
		messageClient(%client,'',"\c3The nickname of\c6" SPC %v SPC "\c3belongs to BLID\c6" SPC %blid SPC "\c3.");
}

function serverCmdClaim(%this, %claim)
{
	if(!$DefaultMinigame.running)
	{
		messageClient(%this, '', "\c4MM is currently not running.");
		return;
	}

	if(!isObject(%this.player))
		return;

	if(!isObject(%role = $MM::RoleKey[%claim]))
	{
		messageClient(%this, '', "\c4That role doesn\'t exist!");
		return;
	}

	if($Sim::Time - %this.MMClaimTimeout < 10)
	{
		messageClient(%this, '', "\c4You have already shifted your claim too recently! Wait ten seconds.");
		return;
	}

	%this.MMClaimTimeout = $Sim::Time;
	%this.player.setShapeNameColor(%role.getNameColour());
}

//uuuuuuuuuuuuuuuuuuuuuu this old thing jeeeesus christ maybe consider reworking this somehow
function serverCmdRules(%client,%cat,%subcat) {
	%client.rules = 1;
	$MMReadRules[%client.bl_id] = 1;
	switch$(strLwr(%cat)) {
		case "1":
			// messageClient(%client,'',"\c4-MMRules- \c6\"/rules 1\" doesn't work!  Type \"/rules game basics\" instead.");
			serverCmdRules(%client,"game",%subcat);
		case "game":
			messageClient(%client,'',"\c4========Game Basics========");
			messageClient(%client,'',"\c4-MMRules- \c6In mafia madness, there are two teams: Mafia and Innocent!");
			messageClient(%client,'',"\c4-MMRules- \c6You win by killing everyone on the other team.");
			messageClient(%client,'',"\c4-MMRules- \c6The game is divided into \"rounds\" which are completely independent of each other.");
			messageClient(%client,'',"\c4-MMRules- \c6If someone's mafia in one round, that doesn't mean they're mafia in the next round.");
			messageClient(%client,'',"\c4-MMRules- \c6There is a short delay inbetween rounds, during which noone has a team and everyone respawns.");
			messageClient(%client,'',"\c4-MMRules- \c6This short delay is used by admins to find people who broke the rules in the last round.");
			messageClient(%client,'',"\c4-MMRules- \c6Inbetween the rounds, there's no rule against killing people.");
			messageClient(%client,'',"\c4-MMRules- \c6Once the round starts, however, the rules will be in full effect, so exercise caution!");
			// messageClient(%client,'',"\c4-MMRules- \c3You will be given a description of your role in the chat when you spawn.");
			messageClient(%client,'',"\c4-MMRules- \c3When you spawn, you will be given a chat message explaining briefly how to use your role.");
			// messageClient(%client,'',"\c4-MMRules- \c6When the game starts, the Mafia will be given a chat list of who all the other maf are.");
			messageClient(%client,'',"\c4-MMRules- \c6In addition, if you're Mafia you will also be given a list of who the other mafia are.");
			// messageClient(%client,'',"\c4-MMRules- \c6If you're Mafia, be sure to check the list carefully, as killing other maf may get you banned!");
			messageClient(%client,'',"\c4-MMRules- \c6You may have to page up to see it, so be sure to check it carefully.  See \c34\c6. Offenses for killing other Mafia.");
			// messageClient(%client,'',"\c4-MMRules- \c3You can type /maflist to see the list of mafia again, but only if you're mafia.");
			messageClient(%client,'',"\c4-MMRules- \c3If you miss it, you can type /maflist to check the list again.  (Mafia-only.)");
			messageClient(%client,'',"\c4-MMRules- \c6The Innocents don't know the role of anyone else when the game starts.");
			messageClient(%client,'',"\c4-MMRules- \c6To balance out the Mafia's foreknowledge, there's usually a lot more Innocents than Mafia in a game.");
			// messageClient(%client,'',"\c4-MMRules- \c6The mafia win by being sneaky and exploiting their knowledge to the greatest advantage!");
			messageClient(%client,'',"\c4-MMRules- \c6The Mafia win by infiltrating the Innos and getting them killed in ways that don't cause suspicion.");
			messageClient(%client,'',"\c4-MMRules- \c6The Innos win by using logic and reasoning to figure out who the mafia are.");
			messageClient(%client,'',"\c4========Game Basics========");
			messageClient(%client,'',"\c4-MMRules- \c6Use \c3PGUp\c6 and \c3PGDown\c6 to scroll the rules up and down.");
			%client.rules[1] = 1;
			$MMReadRules[%client.bl_id, 1] = 1;
		case "2":
			// messageClient(%client,'',"\c4-MMRules- \c6\"/rules 2\" doesn't work!  Type \"/rules advanced game rules\" instead.");
			serverCmdRules(%client,"advanced",%subcat);
		case "advanced":
			messageClient(%client,'',"\c4========Advanced Game Rules========");
			messageClient(%client,'',"\c4-MMRules- \c6When the game starts, it's dawn, signified by the reddish tinge of lighting.");
			messageClient(%client,'',"\c4-MMRules- \c6The sun will then slowly ascend through the sky, and eventually it will reach sunset.");
			messageClient(%client,'',"\c4-MMRules- \c6Once the sun sets, it will turn Night.");
			messageClient(%client,'',"\c4-MMRules- \c6During the night, everyone's avatars are blank!");
			messageClient(%client,'',"\c4-MMRules- \c6If you move close enough to a player, you can see their name.  (For now)");
			messageClient(%client,'',"\c4-MMRules- \c6Some Special Role abilities can only be used once per night.  (See the Special Roles section.)");
			// messageClient(%client,'',"\c4-MMRules- \c6Clicking someone will push them away, which you can use to kill someone if you push them from a high place.");
			// messageClient(%client,'',"\c4-MMRules- \c6Pushing someone should only be done if you are Mafia, as doing it as Innocent will probably get you shot.");
			messageClient(%client,'',"\c4-MMRules- \c6When a player dies, they'll leave a corpse.");
			messageClient(%client,'',"\c4-MMRules- \c6Clicking the corpse will tell you the name of the player, their role, and their Cause of Death.");
			messageClient(%client,'',"\c4-MMRules- \c6Right-click the corpse to pick it up, and right-click again to put it down.");
			messageClient(%client,'',"\c4-MMRules- \c6\"upstanding citizen\" on a corpse means they were inno, \"mafia scumbag\" means they were maf.");
			messageClient(%client,'',"\c4-MMRules- \c6See the Special Roles section for more corpse occupations.");
			messageClient(%client,'',"\c4-MMRules- \c6When you chat normally, only nearby players will hear the message!");
			messageClient(%client,'',"\c4-MMRules- \c6To shout so far-away people can hear, start your message with a !");
			messageClient(%client,'',"\c4-MMRules- \c6To speak in a low voice so people can't hear through walls, use Team Chat.  (Y by default)");
			messageClient(%client,'',"\c4-MMRules- \c6To speak in a whisper that only very close players can hear, use Team Chat and start with a !");
			messageClient(%client,'',"\c4-MMRules- \c6Dead players and spectators cannot be heard by any living player, but they can hear all chat!");
			messageClient(%client,'',"\c4-MMRules- \c6E.G. to shout \"everyone go to roof\" you would type \"!everyone go to roof\"!");
			messageClient(%client,'',"\c4-MMRules- \c6All Shouted messages come out in all-caps, regardless of how they were typed in.");
			messageClient(%client,'',"\c4========Advanced Game Rules========");
			messageClient(%client,'',"\c4-MMRules- \c6Use \c3PGUp\c6 and \c3PGDown\c6 to scroll the rules up and down.");
			%client.rules[2] = 1;
			$MMReadRules[%client.bl_id, 2] = 1;
		case "3":
			// messageClient(%client,'',"\c4-MMRules- \c6\"/rules 3\" doesn't work!  Type \"/rules special roles\" instead.");
			serverCmdRules(%client,"special",%subcat);
		case "special":
			switch$(strLwr(%subcat)) {
				case "abductor":
					messageClient(%client,'',"\c4-MMRules- ==" @ $MMRoleColor["A"] @ "Abductor\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6The Abductor is a member of the Mafia who abducts Innocents.");
					messageClient(%client,'',"\c4-MMRules- \c6The Abductor's ability works by right-clicking an Innocent at close range.");
					messageClient(%client,'',"\c4-MMRules- \c6The Abductor's ability only works at night, and only works once per night.");
					messageClient(%client,'',"\c4-MMRules- \c6The body of the person abducted by the Abductor goes to the basement.");
					messageClient(%client,'',"\c4-MMRules- \c6The specific section it goes to is always opposite the spawn point in the basement.");
					messageClient(%client,'',"\c4-MMRules- \c6The Abductor shows up as" SPC $MMRoleColor["A"] @ "dark purple\c6 on the /maflist.");
					messageClient(%client,'',"\c4-MMRules- \c6The Abductor's ability does not work on mafia members.");
					messageClient(%client,'',"\c4-MMRules- \c6The Abductor's ability is hard to spot, but some players can spot it, so use it in crowds.");
					messageClient(%client,'',"\c4-MMRules- \c6The Abductor is \c3essential\c6 to the mafia for eliminating the Cop and Fingerprint Expert.");
					messageClient(%client,'',"\c4-MMRules- \c6Abducting someone leaves a fingerprint on the corpse.");
					messageClient(%client,'',"\c4-MMRules- \c6Therefor, it is essential that the Mafia prevent the Fingerprint Expert from seeing the bodies.");
					messageClient(%client,'',"\c4-MMRules- ==" @ $MMRoleColor["A"] @ "Abductor\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6Use \c3PGUp\c6 and \c3PGDown\c6 to scroll the rules up and down.");
					%client.rules[3,"A"] = 1;
					$MMReadRules[%client.bl_id, 3, "A"] = 1;
				case "ventriloquist":
					messageClient(%client,'',"\c4-MMRules- ==\c7Ventriloquist\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6The Ventriloquist is a member of the mafia who impersonates other players' voices.");
					messageClient(%client,'',"\c4-MMRules- \c6By typing /imp [name] (without the [ ]s), the Ventriloquist can disguise his voice.");
					messageClient(%client,'',"\c4-MMRules- \c6When the Ventriloquist's voice is disguised, any chat messages he send will appear to come from whoever he impersonated.");
					messageClient(%client,'',"\c4-MMRules- \c6Only the Ventriloquist or dead people can tell the difference between an impersonation and the real thing.");
					messageClient(%client,'',"\c4-MMRules- \c6The person the Ventriloquist impersonated will see the chat message too, but they may not notice it immediately.");
					messageClient(%client,'',"\c4-MMRules- \c6It is customary to shout \"VENTED\" if you are impersonated, see Terminology.");
					messageClient(%client,'',"\c4-MMRules- \c6In order to make the impersonation not visible to the target of the impersonation, the ventriloquist may use /impu instead.");
					// messageClient(%client,'',"\c4-MMRules- \c6/impu was recently added April 9th, though, so it may not work properly.  I encourage you to try it anyway.");
					messageClient(%client,'',"\c4-MMRules- \c6When using /impu, everyone but the person you impersonated will see the chat message.");
					messageClient(%client,'',"\c4-MMRules- \c6Typing either /impu or /imp with nothing after it will allow the Ventriloquist to return to normal chat.");
					messageClient(%client,'',"\c4-MMRules- \c6The Ventriloquist can use all forms of speaking while impersonating, including shouting, low voice, and whispering.");
					messageClient(%client,'',"\c4-MMRules- \c6However, the Ventriloquist's impersonation will only disguise their voice.  It will not change the location of the voice.");
					messageClient(%client,'',"\c4-MMRules- \c6E.G, if you whisper while impersonating someone, only people nearby YOU will hear it.");
					messageClient(%client,'',"\c4-MMRules- \c6Impersonating someone will not allow you to force them to use commands.  The commands will still come from you.");
					messageClient(%client,'',"\c4-MMRules- \c6The Ventriloquist is especially useful when he targets the Cop role, as then he can confuse the innocents and destroy the Cop's credibility.");
					messageClient(%client,'',"\c4-MMRules- \c6The Ventriloquist can also be used to manipulate voting, if done carefully.");
					messageClient(%client,'',"\c4-MMRules- ==\c7Ventriloquist\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6Use \c3PGUp\c6 and \c3PGDown\c6 to scroll the rules up and down.");
					%client.rules[3,"V"] = 1;
					$MMReadRules[%client.bl_id, 3, "V"] = 1;
				case "godfather":
					messageClient(%client,'',"\c4-MMRules- ==\c6Godfather\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6The Godfather is a member of the mafia who appears Innocent to the Cop!");
					messageClient(%client,'',"\c4-MMRules- \c6Whenever the Godfather is investigated by the Cop, the Cop will get the same result as if he had investigated an Innocent.");
					messageClient(%client,'',"\c4-MMRules- \c6However, the Paranoid Cop will still think the Godfather is suspicious, because the Paranoid Cop thinks everyone is suspicious.");
					messageClient(%client,'',"\c4-MMRules- \c6In addition, the Godfather has the ability to talk directly to the other mafia.");
					messageClient(%client,'',"\c4-MMRules- \c6By starting a message with ^, the Godfather can send messages that go directly to every mafia member without being intercepted.");
					messageClient(%client,'',"\c4-MMRules- \c6However, the other Mafia cannot respond to the Godfather in the same way, so the messages are one-way.");
					messageClient(%client,'',"\c4-MMRules- \c6The Godfather can use this ability to send plans and strategies to the other mafia.");
					messageClient(%client,'',"\c4-MMRules- \c6The Godfather can also use this to orchestrate a method of signaling that the mafia can use to send each other information.");
					messageClient(%client,'',"\c4-MMRules- ==\c6Godfather\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6Use \c3PGUp\c6 and \c3PGDown\c6 to scroll the rules up and down.");
					%client.rules[3,"G"] = 1;
					$MMReadRules[%client.bl_id, 3, "G"] = 1;
				case "crazy":
					messageClient(%client,'',"\c4-MMRules- ==\c5Crazy\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6The Crazy is a member of the mafia who can disfigure bodies!");
					messageClient(%client,'',"\c4-MMRules- \c6The Crazy spawns with a Knife, which, when used on a corpse, disfigures it.");
					messageClient(%client,'',"\c4-MMRules- \c6A disfigured body shows up with the name of \"disfigured corpse\" and the occupation of \"permanently retired\".");
					messageClient(%client,'',"\c4-MMRules- \c6You cannot learn the Cause of Death or take any Fingerprints from a disfigured corpse.");
					messageClient(%client,'',"\c4-MMRules- \c6The knife normally makes a small \"woosh\" sound, but you can also make it perfectly silent.");
					messageClient(%client,'',"\c4-MMRules- \c6To make the knife perfectly silent, hold down the mouse button for a short time before releasing.");
					messageClient(%client,'',"\c4-MMRules- \c6The knife will play a small animation to indicate that it's \"charging up\", and upon the conclusion, is perfectly silent.");
					messageClient(%client,'',"\c4-MMRules- \c6Whether the knife is silent or not, it will still disfigure corpses.");
					messageClient(%client,'',"\c4-MMRules- \c6While both silenced and non-silenced, the knife does enough damage to instantly kill a player.");
					messageClient(%client,'',"\c4-MMRules- \c6However, it can be somewhat inconvenient for that purpose compared to a gun.");
					messageClient(%client,'',"\c4-MMRules- \c6The Crazy's role in a game is often to lurk beneath the roof, killing any innocents he sees and disfiguring the bodies.");
					messageClient(%client,'',"\c4-MMRules- \c6The Crazy can also help the mafia by going to the Basement and disfiguring any bodies there.  (See Abductor role.)");
					messageClient(%client,'',"\c4-MMRules- \c6The Crazy will only appear in a game that already has 4 Mafia under the standard game-mode.");
					messageClient(%client,'',"\c4-MMRules- ==\c5Crazy\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6Use \c3PGUp\c6 and \c3PGDown\c6 to scroll the rules up and down.");
					%client.rules[3,"C"] = 1;
					$MMReadRules[%client.bl_id, 3, "C"] = 1;
				case "cop":
					messageClient(%client,'',"\c4-MMRules- ==<color:1122CC>Cop\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6sorry these aren't done yet, cop investigates with /inv");
					messageClient(%client,'',"\c4-MMRules- \c6also the initial for Cop is O, not C.  remember that.");
				default:
					messageClient(%client,'',"\c4========Special Roles========");
					messageClient(%client,'',"\c4-MMRules- \c6Some players will receive a role other than the basic \"Mafia\" and \"Innocent\"");
					messageClient(%client,'',"\c4-MMRules- \c6Having one of these roles is almost the same as being a basic Mafia or Innocent.");
					messageClient(%client,'',"\c4-MMRules- \c6The only difference is that you get an extra ability to use for your team.");
					messageClient(%client,'',"\c4-MMRules- \c6There will only be one of each of these roles per game, so if two people claim to have it, one's an imposter!");
					messageClient(%client,'',"\c4-MMRules- \c6One exception to the above rule is during a strange \"game mode\" such as \"I HATE YOU ALL\" or \"HAHAHAHAHAHAHA\".");
					messageClient(%client,'',"\c4-MMRules- \c6Another exception is the Cop role, where there will usually be a Paranoid Cop who thinks he's a Cop.");
					messageClient(%client,'',"\c4-MMRules- \c3A brief summary of each role will be given below.  To get a more extensive summary, type \c6/rules special [role name]");
					messageClient(%client,'',"\c4-MMRules- \c6In addition, even for roles lacking a summary, if you spawn as a role, you will receive a chat message detailing it.");
					messageClient(%client,'',"\c4-MMRules- \c6Here is the list of special roles:");
					messageClient(%client,'',"\c4-MMRules- ==" @ $MMRoleColor["A"] @ "Abductor\c4== \c3/rules special Abductor");
					messageClient(%client,'',"\c4-MMRules- \c6The Abductor is a mafia member who can abduct one person per night in perfect silence!");
					messageClient(%client,'',"\c4-MMRules- \c6When the Abductor abducts someone, they die instantly, and their body goes to the basement.");
					messageClient(%client,'',"\c4-MMRules- \c6Abducting someone leaves a fingerprint detectable by the Fingerprint Expert.");
					messageClient(%client,'',"\c4-MMRules- ==\c7Ventriloquist\c4== \c3/rules special Ventriloquist");
					messageClient(%client,'',"\c4-MMRules- \c6The Ventriloquist is a mafia member who can impersonate people's voices!");
					messageClient(%client,'',"\c4-MMRules- \c6The Ventriloquist can choose to make it so that the person they impersonate doesn't realize they were impersonated!");
					messageClient(%client,'',"\c4-MMRules- ==\c6Godfather\c4== \c3/rules special Godfather");
					messageClient(%client,'',"\c4-MMRules- \c6The Godfather is the mafia leader, who appears Innocent to the Cop!");
					messageClient(%client,'',"\c4-MMRules- \c6The Godfather can also talk directly to other mafia using ^");
					messageClient(%client,'',"\c4-MMRules- ==\c5Crazy\c4== \c3/rules special Crazy");
					messageClient(%client,'',"\c4-MMRules- \c6The Crazy is a mafia member who spawns with a knife which he can use to disfigure bodies!");
					messageClient(%client,'',"\c4-MMRules- \c6You can't get any information from a body which the Crazy has used his knife on!");
					messageClient(%client,'',"\c4-MMRules- ==" @ $MMRoleColor["F"] @ "Fingerprint Expert\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6The Fingerprint Expert is an innocent who can check a body for fingerprints!");
					messageClient(%client,'',"\c4-MMRules- \c6The Fingerprint Expert will get a list of people who picked up a body, in order.");
					messageClient(%client,'',"\c4-MMRules- \c6The Fingerprint Expert can also see the relative time of death of the body.");
					messageClient(%client,'',"\c4-MMRules- ==<color:1122CC>Cop\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6The Cop is an innocent who can investigate players and learn whether they're innocent or mafia using /inv [name]!");
					messageClient(%client,'',"\c4-MMRules- \c6The Cop can only investigate one person each night.  He is a very important role to the innocents!");
					messageClient(%client,'',"\c4-MMRules- ==" @ $MMRoleColor["P"] @ "Paranoid Cop\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6The Paranoid Cop thinks he's a normal Cop, but will get \"suspicious\" every time he investigates anyone!");
					messageClient(%client,'',"\c4-MMRules- \c6Make sure you're not the Paranoid Cop before you kill someone for being suspicious, if you're a cop!");
					messageClient(%client,'',"\c4-MMRules- ==" @ $MMRoleColor["N"] @ "Naive Cop\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6The Naive Cop is basically the same as the Paranoid Cop, but gets \"innocent\" instead when he investigates.");
					messageClient(%client,'',"\c4-MMRules- \c6The Naive Cop doesn't appear in standard games currently.  You can identify them in the roles list as \"N\".");
					messageClient(%client,'',"\c4-MMRules- ==" @ $MMRoleColor["L"] @ "Miller\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6[]'s Notes: i added the miller recently as an experimental role, so he \c3might \c2appear in a game");
					messageClient(%client,'',"\c4-MMRules- \c6his initial is L and he's an inno who thinks he's a normal inno but cop and para think he's Guilty when they /inv him");
					messageClient(%client,'',"\c4-MMRules- ==" @ $MMRoleColor["D"] @ "Devil\c4==");
					messageClient(%client,'',"\c4-MMRules- \c6The Devil is a mafia role who can investigate Innocents like the cop.");
					messageClient(%client,'',"\c4-MMRules- \c6Instead of just learning whether a person is innocent or mafia, the Devil will learn their full role, such as Cop or Fingerprint Expert.");
					messageClient(%client,'',"\c4========Special Roles========");
					messageClient(%client,'',"\c4-MMRules- \c6Use \c3PGUp\c6 and \c3PGDown\c6 to scroll the rules up and down.");
					%client.rules[3] = 1;
					$MMReadRules[%client.bl_id, 3] = 1;
			}
		case "4":
			// messageClient(%client,'',"\c4-MMRules- \c6\"/rules 4\" doesn't work!  Type \"/rules offenses\" instead.");
			serverCmdRules(%client,"offenses",%subcat);
		case "offenses":
			if(strLwr(%subcat) $= "examples") {
				messageClient(%client,'',"\c4========Offense Examples========");
				messageClient(%client,'',"\c4-MMRules- \c6Here are some examples of breaking these rules (These aren't the only examples, though!)");
				messageClient(%client,'',"\c4-MMRules- 1. =Inno=\c6Seeing someone on the roof and shooting them instantly.");
				messageCLient(%client,'',"\c4-MMRules- \c6REASON: Just being on the roof alone is not a reason to believe someone is mafia!");
				messageClient(%client,'',"\c4-MMRules- 2. =Mafia=\c6Deciding to shoot everyone on the roof and shooting your fellow mafia members too.");
				messageClient(%client,'',"\c4-MMRules- \c6REASON: Your fellow mafia members are essential!  Shooting them does not help your team at all.");
				messageClient(%client,'',"\c4-MMRules- 3. =All=\c6As mafia, killing yourself when you spawn, or telling the Innocents who the mafia are.");
				messageClient(%client,'',"\c4-MMRules- \c6REASON: If you're mafia, you're mafia!  Selling your team out or refusing to play is childish and ruins the game.");
				messageClient(%client,'',"\c4-MMRules- 4. =All=\c6Using IRC to tell an alive player what happened either after or before you died.");
				messageClient(%client,'',"\c4-MMRules- \c6REASON: IRC is not part of the game, and using it or another out-of-game communication like this will get you banned permanently!");
				messageClient(%client,'',"\c4-MMRules- 5. =All=\c6Letting a friend use the same AUTH key as you and connect to the server.");
				messageClient(%client,'',"\c4-MMRules- \c6REASON: Two people with the same name is incredibly confusing in-game, as names are the only reliable method of identification.");
				messageClient(%client,'',"\c4-MMRules- \c6(extra: it'll also mess up the /inv and /imp commands)");
				messageClient(%client,'',"\c4-MMRules- 6. =Dead=\c6Spam-clicking the red ramp that spawns a new pong.");
				messageClient(%client,'',"\c4-MMRules- \c6REASON: It's a pain to reset the pong game once it's messed up, and spamming it may cause people lag.");
				messageClient(%client,'',"\c4========Offense Examples========");
				messageClient(%client,'',"\c4-MMRules- \c6Use \c3PGUp\c6 and \c3PGDown\c6 to scroll the rules up and down.");
				%client.rules[4,"E"] = 1;
				$MMReadRules[%client.bl_id, 4, "E"] = 1;
				return;
			}
			messageClient(%client,'',"\c4========Offenses========");
			messageClient(%client,'',"\c4-MMRules- \c6Here we will make a list of things which are \c3against \c6the rules, and could get you banned!");
			messageClient(%client,'',"\c4-MMRules- 1. =Inno=\c6DO NOT kill someone without even the slightest provocation or reason to believe they're mafia.");
			messageClient(%client,'',"\c4-MMRules- 2. =Mafia=\c6DO NOT kill any of your fellow Mafia members.");
			messageClient(%client,'',"\c4-MMRules- 2a.=Mafia=\c6EXCEPTION TO ABOVE: Doing so can be part of a clever ploy to prove yourself innocent.");
			messageClient(%client,'',"\c4-MMRules- 3. =All=\c6DO NOT make a deliberate attempt to cause your team to lose.");
			messageClient(%client,'',"\c4-MMRules- 4. =All=\c6DO NOT use any out-of-game method of communication to another player about a game in progress.");
			messageClient(%client,'',"\c4-MMRules- 5. =All=\c6DO NOT connect to the server with the same exact name as another player on the server.");
			messageClient(%client,'',"\c4-MMRules- 6. =Dead=\c6DO NOT deliberately attempt to mess up the Pong game in the Afterlife.");
			messageClient(%client,'',"\c4-MMRules- \c6Type \"/rules offenses examples\" to get some examples of breaking the rules, and reasons why.");
			messageClient(%client,'',"\c4========Offenses========");
			messageClient(%client,'',"\c4-MMRules- \c6Use \c3PGUp\c6 and \c3PGDown\c6 to scroll the rules up and down.");
			%client.rules[4] = 1;
			$MMReadRules[%client.bl_id, 4] = 1;
		case "5":
			// messageClient(%client,'',"\c4-MMRules- \c6\"/rules 5\" doesn't work!  Type \"/rules terminology\" instead.");
			serverCmdRules(%client,"terminology",%subcat);
		case "terminology":
			// messageClient(%client,'',"\c4-MMRules- \c6Sorry, this section of the rules is not finished yet.  Type \c0/oldrules\c6 for the old rules!");
			messageClient(%client,'',"\c4========Terminology========");
			messageClient(%client,'',"\c4-MMRules- \c6Here we'll make a small glossary of some terminology sometimes used in Mafia Madness.");
			messageClient(%client,'',"\c4-MMRules- \c6Maf: Short for a member of the Mafia.");
			messageClient(%client,'',"\c4-MMRules- \c6Scum: Same as Maf.");
			messageClient(%client,'',"\c4-MMRules- \c6Inno: Short for Innocent.");
			messageClient(%client,'',"\c4-MMRules- \c6Doc: Sometimes short for Fingerprint Expert.");
			messageClient(%client,'',"\c4-MMRules- \c6ROOF: means \"GO TO THE ROOF\".");
			messageClient(%client,'',"\c4-MMRules- \c6Basement: The series of tunnels at the base of the pyramid.");
			messageClient(%client,'',"\c4-MMRules- \c6Dumpster: A specific spot in the Basement where Abducted bodies are dumped.");
			messageClient(%client,'',"\c4-MMRules- \c6VENT: The Ventriloquist mafia role.  (See Special Roles.)");
			messageClient(%client,'',"\c4-MMRules- \c6VENTED: When someone is impersonated by the Ventriloquist.");
			messageClient(%client,'',"\c4-MMRules- \c6IMP/IMPED: short for Impersonate(d).  (See the Ventriloquist role.)");
			messageClient(%client,'',"\c4-MMRules- \c6Inv: Short for Investigate.  (See the Cop role.)");
			messageClient(%client,'',"\c4-MMRules- \c6PARA/PARANOID: means Paranoid Cop.");
			// messageClient(%client,'',"\c4-MMRules- \c6");
			messageClient(%client,'',"\c4========Terminology========");
			messageClient(%client,'',"\c4-MMRules- \c6Use \c3PGUp\c6 and \c3PGDown\c6 to scroll the rules up and down.");
			%client.rules[5] = 1;
			$MMReadRules[%client.bl_id, 5] = 1;
		case "6":
			// messageClient(%client,'',"\c4-MMRules- \c6\"/rules 6\" doesn't work!  Type \"/rules customs\" instead.");
			serverCmdRules(%client,"customs",%subcat);
		case "customs":
			// messageClient(%client,'',"\c4-MMRules- \c6Sorry, this section of the rules is not finished yet.  Type \c0/oldrules\c6 for the old rules!");
			messageClient(%client,'',"\c4========Customs========");
			messageClient(%client,'',"\c4-MMRules- \c6Customs are not hard rules, but usually they're what people will expect you to do.");
			messageClient(%client,'',"\c4========Roof and Voting========");
			messageClient(%client,'',"\c4-MMRules- \c6The first thing everyone does in a round is make their way to the Roof as fast as possible.");
			messageClient(%client,'',"\c4-MMRules- \c6If you can't find your way to the roof, just look around until you find an upwards stairway, and never go down.");
			messageClient(%client,'',"\c4-MMRules- \c6Once everyone is on the Roof, the typical strategy to find the mafia is to vote on who to kill.");
			messageClient(%client,'',"\c4-MMRules- \c6If you don't vote on who to kill, the Mafia and the Abductor will slowly eliminate you regardless.");
			messageClient(%client,'',"\c4-MMRules- \c6You should never, however, vote to kill the Cop, Paranoid Cop, or Fingerprint Expert.");
			messageClient(%client,'',"\c4-MMRules- \c6Typically, you should avoid killing someone directly until a majority vote has been reached.");
			messageClient(%client,'',"\c4-MMRules- \c6Leaving the roof is a bad idea, as then people will suspect you of being mafia, and sometimes shoot you.");
			messageClient(%client,'',"\c4-MMRules- \c6Unexpectedly shooting someone is a good way to get yourself killed.  If you announce it first, you're more likely to survive.");
			messageClient(%client,'',"\c4-MMRules- \c6The Mafia should attempt to infiltrate the roof subtly, and manipulate the voting.");
			messageClient(%client,'',"\c4========Mafia Duties========");
			messageClient(%client,'',"\c4-MMRules- \c6Each special member of the Mafia has their own duty to perform in the game.");
			messageClient(%client,'',"\c4-MMRules- \c6The Abductor's duty is to eliminate very dangerous roles like the Cop or Fingerprint Expert.");
			messageClient(%client,'',"\c4-MMRules- \c6The Ventriloquist's duty is to confuse the innocents about the Cop, manipulate the vote, and cause split-second kills.");
			messageClient(%client,'',"\c4-MMRules- \c6The Godfather's duty is to unite the mafia with a shared strategy.");
			messageClient(%client,'',"\c4-MMRules- \c6The Crazy's duty is to be a loose cannon, operating below-roof and silently disfiguring as many bodies as possible.");
			messageClient(%client,'',"\c4-MMRules- \c6The Crazy is also important for making sure that the Fingerprint Expert can't find out who the Abductor is.");
			messageClient(%client,'',"\c4========Dos and Don'ts========");
			messageClient(%client,'',"\c4-MMRules- \c6While on the roof, there are some things that if you do them, you might be shot instantly without voting.");
			messageClient(%client,'',"\c4-MMRules- \c6Shooting someone without declaring it first or any votes having passed will obviously likely get you shot.");
			messageClient(%client,'',"\c4-MMRules- \c6Jumping off the roof or leaving the roof suddenly will also probably get you shot.");
			messageClient(%client,'',"\c4-MMRules- \c6Throwing bodies off the roof, or taking bodies from the pile and carrying them will also get you shot.");
			messageClient(%client,'',"\c4-MMRules- \c6When you find a body, you should report its' name and role, and bring it to the roof.");
			messageClient(%client,'',"\c4-MMRules- \c6At night, while the Abductor is alive, don't run too close to someone, or people might get suspicious.");
			messageClient(%client,'',"\c4-MMRules- \c6Pulling out a gun for no reason or shooting at something for no reason will most likely get you shot.");
			// messageClient(%client,'',"\c4-MMRules- \c3Pushing /anyone/ has a good chance of getting you shot.  It's dangerous, and extremely suspicious.");
			messageClient(%client,'',"\c4-MMRules- \c6Finally, disguising yourself as a body (avatar entirely red, default blocko) is very suspicious and might get you shot.");
			messageClient(%client,'',"\c4-MMRules- \c6Also, people tend to vote people who wear pedobear avatars when there's no other reasons to vote.");
			messageClient(%client,'',"\c4========Customs========");
			messageClient(%client,'',"\c4-MMRules- \c6Use \c3PGUp\c6 and \c3PGDown\c6 to scroll the rules up and down.");
			%client.rules[6] = 1;
			$MMReadRules[%client.bl_id, 6] = 1;
		case "7":
			serverCmdRules(%client,"changelog",%subcat);
		case "changelog":
			messageClient(%client,'',"\c4-MMRules- \c3April 9th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Added the Changelog.  Check here for updates!");
			messageClient(%client,'',"\c4-MMRules- \c6Added /impu to the Ventriloquist.");
			messageClient(%client,'',"\c4-MMRules- \c6Added Ventriloquist, Godfather, and Abductor subcategories to the Special Roles section.");
			messageClient(%client,'',"\c4-MMRules- \c6Made it so that zombies can now only whisper.");
			messageClient(%client,'',"\c4-MMRules- \c6Made it so that the dead rise at the dawn of the 4th day.");
			messageClient(%client,'',"\c4-MMRules- \c6Edited it so that the dead will re-rise every day/night after the dawn of the 4th.");
			messageClient(%client,'',"\c4-MMRules- \c3Added Clickpush.  Experimental!");
			messageClient(%client,'',"\c4-MMRules- \c3April 11th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Fixed corpses seeing double of every chat they were in range for.");
			messageClient(%client,'',"\c4-MMRules- \c6Added mention of Clickpush to the Advanced Rules section, made a slight edit.");
			messageClient(%client,'',"\c4-MMRules- \c6Added Dos and Don'ts subsection to Customs.");
			messageClient(%client,'',"\c4-MMRules- \c6Added a detailed Crazy subcategory to the Special Roles section.  Type \c3/rules special crazy\c6 to see it!");
			messageClient(%client,'',"\c4-MMRules- \c3Removed clickpush.  Holy fuck.");
			messageClient(%client,'',"\c4-MMRules- \c3April 25th (or about then)\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Messed with the gun a lot!  Gun will now fire 6 shots, and then need manual reloading.");
			messageClient(%client,'',"\c4-MMRules- \c6Use the Light function, like with every other gun, to reload.");
			messageClient(%client,'',"\c4-MMRules- \c6You can reload an infinite number of times, and there is no way to tell how much ammo is in a gun.");
			messageClient(%client,'',"\c4-MMRules- \c6Added a rule against multiclienting in the Offenses section.  It's now official!  Mafia Madness hates multiclienters.");
			messageClient(%client,'',"\c4-MMRules- \c3April 26th (maybe???)\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Made gun slow you down while reloading, still buggy though and a bit crashy.");
			messageClient(%client,'',"\c4-MMRules- \c3Added a very rudimentary system wherein if you are shot you will stay alive for exactly one second.  Experimental!");
			messageClient(%client,'',"\c4-MMRules- \c3April 27th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Removed all mention of Clickpush from the rules.  Hopefully.");
			messageClient(%client,'',"\c4-MMRules- \c6Fixed a bug that let you put away the gun while reloading to slow down more and more.");
			messageClient(%client,'',"\c4-MMRules- \c6Added a red flash to the Death and Dying system.");
			messageClient(%client,'',"\c4-MMRules- \c6Added message of \"IMP\" to the Terminology section.");
			messageClient(%client,'',"\c4-MMRules- \c6Fixed a bug that allowed dead bodies and dying people to shout and talk.");
			messageClient(%client,'',"\c4-MMRules- \c6Made you stand still for 3 seconds before dying when abducted.");
			messageClient(%client,'',"\c4-MMRules- \c3May 2nd(?)\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Abducting people now properly leaves fingerprints on the bodies.  (Bugfix)");
			messageClient(%client,'',"\c4-MMRules- \c3May 4th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Mafia should now be able to tell when a chat message is from the Vent.");
			messageClient(%client,'',"\c4-MMRules- \c3May 8th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Added the experimental role Miller and a short passage about him in the Specal Roles section.");
			messageClient(%client,'',"\c4-MMRules- \c6Added an experimental 'afterlife' to hang out in when you're dead.  Currently empty.");
			messageClient(%client,'',"\c4-MMRules- \c3May 18th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Added a Pong machine in the Afterlife, and made it so you could toggle Afterlife/Spectate using the Light key.");
			messageClient(%client,'',"\c4-MMRules- \c6Updated the Offenses section to have a rule against fucking with the pong game.");
			messageClient(%client,'',"\c4-MMRules- \c6Made some minor edits to the Game Basics section to increase readability and clearness.");
			messageClient(%client,'',"\c4-MMRules- \c6Fixed the Crazy's Knife causing the same delayed death as a gunshot wound.");
			messageClient(%client,'',"\c4-MMRules- \c6Enlarged the range of picking up corpses and abducting people by 3/5ths.");
			messageClient(%client,'',"\c4-MMRules- \c6Added an experimental swordfighting arena in the Afterlife.");
			messageClient(%client,'',"\c4-MMRules- \c3June 1st\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Updated The Summoning to use Low chat instead of Say chat.");
			messageClient(%client,'',"\c4-MMRules- \c6The Summoning will now kill you if you say it 3 times, and only function if the stars are aligned to the milisecond.");
			messageClient(%client,'',"\c4-MMRules- \c3June 3rd\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Made it so that people who are Abducted can still move, and might not even realize they're abducted.");
			messageClient(%client,'',"\c4-MMRules- \c6Pressing the Light Key in the afterlife now works properly.  (No more pressing Space twice!)");
			messageClient(%client,'',"\c4-MMRules- \c3June 7th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Added the Naive Cop.  (See Special Roles.)");
			messageClient(%client,'',"\c4-MMRules- \c6Added a gamemode in which all mafia can abduct!  This will not be the default.");
			messageClient(%client,'',"\c4-MMRules- \c6Added a Limbo to the afterlife!  You can fight with guns and stuff.");
			messageClient(%client,'',"\c4-MMRules- \c3June 12th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Removed an old line and added a new line to the Ventriloquist subcategory of Special Roles.  (/rules 3 Ventriloquist)");
			messageClient(%client,'',"\c4-MMRules- \c3June 22nd\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Added a new Gamemode, tentatively called \"Abduct Titanium Tonight\"!  In this gamemode, all mafia use Godfather chat!");
			messageClient(%client,'',"\c4-MMRules- \c6The WIP role \"Insane Cop\" now appears in one gamemode.  He always gets a random result!");
			messageClient(%client,'',"\c4-MMRules- \c3July 2nd\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Players who can't use Godfather chat can no longer make messages starting with ^.  Nice try.");
			messageClient(%client,'',"\c4-MMRules- \c6Edited the way the Kill List works.  This may screw up a bit.");
			messageClient(%client,'',"\c4-MMRules- \c3July 4th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6FIREWOOOOOOOORKS");
			messageClient(%client,'',"\c4-MMRules- \c6Fixed a HILARIOUS bug where Innos could talk to the mafia in Abduct Titanium Tonight with ^, but couldn't hear the messages.");
			messageClient(%client,'',"\c4-MMRules- \c3July 26th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Revamped the /maflist command to list the individual roles of the mafia members and use proper colors.");
			messageClient(%client,'',"\c4-MMRules- \c6Abductions should now show up on the kill list.");
			messageClient(%client,'',"\c4-MMRules- \c3July 27th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Added the" SPC $MMRoleColor["D"] @ "Devil \c6role, to appear in specific gamemodes.");
			messageClient(%client,'',"\c4-MMRules- \c6He's kind of like a mafia cop, who learns what type of inno as well as just whether they're inno or maf.");
			messageClient(%client,'',"\c4-MMRules- \c3August 26th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Updated Mafia Madness for the Shadows and Shaders update!  It is now in Alpha until further notice.");
			messageClient(%client,'',"\c4-MMRules- \c6Renovated the day/night system to work with the Shaders update's new Day Cycles.");
			messageClient(%client,'',"\c4-MMRules- \c6Instituted temporary daycycles for Day and Night.  The daycycle is a gradient between red and cyan, and the night one is the reverse.");
			messageClient(%client,'',"\c4-MMRules- \c6Made the game into a gamemode compatible with the Gamemode system, and the Default Minigame.");
			messageClient(%client,'',"\c4-MMRules- \c6Highlighted an important line in Special Roles and added an extra one for those who failed to read Game Basics.");
			messageClient(%client,'',"\c4-MMRules- \c6Added the command to get more information about a role to the beginning of every role section in Special Roles.");
			messageClient(%client,'',"\c4-MMRules- \c3November 5th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Added a selection of 4 different guns!  They are currently just reskins of the default gun, but I'll change their stats later.");
			messageClient(%client,'',"\c4-MMRules- \c6To use them, type /setgun # where # is a number between 0 and 3, and it will set your gun to that next time you spawn in-game.");
			messageClient(%client,'',"\c4-MMRules- \c3November 30th\c6:");
			messageClient(%client,'',"\c4-MMRules- \c6Fixed some bugs in Just Try To Survive, including one which allowed all innocents to abduct along with the mafia.");
			messageClient(%client,'',"\c4-MMRules- \c6Added Day, Night, and Dead Rising notifications to the Kill List.");
			%client.rules[7] = 1;
			$MMReadRules[%client.bl_id, 7] = 1;
		default:
			messageClient(%client,'',"\c4-MMRules- \c6Welcome to the updated Mafia Madness rules!");
			messageClient(%client,'',"\c4-MMRules- \c6The game is a WIP and these rules may not be complete.");
			// messageClient(%client,'',"\c4-MMRules- \c6To see the old list of rules, type \c2/OldRules\c6 in chat.");
			messageClient(%client,'',"\c4-MMRules- \c6The rules will be split into categories for different subjects.");
			messageClient(%client,'',"\c4-MMRules- \c6To access a category, type /rules [category name]");
			messageClient(%client,'',"\c4-MMRules- \c6E.G. to access Category 1. Game Basics, you would type \"/rules game basics\"");
			messageClient(%client,'',"\c4-MMRules- \c3It's very important that you read all categories, as each is required to play.");
			messageClient(%client,'',"\c4-MMRules- \c6Here are the categories:");
			messageClient(%client,'',"\c4-MMRules- \c31\c6. Game Basics");
			messageClient(%client,'',"\c4-MMRules- \c32\c6. Advanced Game Rules");
			messageClient(%client,'',"\c4-MMRules- \c33\c6. Special Roles");
			messageClient(%client,'',"\c4-MMRules- \c34\c6. Offenses");
			messageClient(%client,'',"\c4-MMRules- \c35\c6. Terminology");
			messageClient(%client,'',"\c4-MMRules- \c36\c6. Customs");
			messageClient(%client,'',"\c4-MMRules- \c37\c6. Changelog (Not necessarily required.)");
			messageClient(%client,'',"\c4-MMRules- \c6Use \c3PGUp\c6 and \c3PGDown\c6 to scroll the rules up and down.");
	}
	export("$MMReadRules*", "config/server/mmrulesprefs.cs");
}