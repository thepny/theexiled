global.afkSecondsCount = 0;

setInterval(function () {
    if (!global.menuOpened) {

        afkSecondsCount++;
        if (afkSecondsCount >= 900) {
			if(localplayer.getVariable('IS_ADMIN') == true) afkSecondsCount = 0;
			else {
				mp.gui.chat.push('You were excluded from the game for AFK more than 15 minutes.');
				mp.events.callRemote('kickclient');
			}
        }
    }
}, 1000);