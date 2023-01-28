let browser = null;
let inShape = false;

mp.events.add("playerEnterColshape", (shape) => {
    if (shape.name == "Garbage") {
        mp.game.graphics.notify('Press the key ~g~E!');
        inShape = true;
    }
});

mp.events.add("playerExitColshape", (shape) => {
    if (shape.name == "Garbage") {
        inShape = false;
    }
});

mp.events.add("garbageMenuCancel", () => {
    if (browser != null) {
        browser.destroy();
        browser = null;
        mp.gui.cursor.show(false, false);
    }
});

mp.events.add("garbageMenuOpen", () => {
    if (inShape && browser == null) {
        browser = mp.browsers.new("package://garbageWork/work/web/index.html");
        mp.gui.cursor.show(true, true);
    } else if (browser.active == false){
        browser.active = true;
        mp.gui.cursor.show(true, true);
    }
});

mp.events.add("WORK:START", () => {
    mp.game.graphics.notify('~g~You started work!');

    if (browser != null) {
        browser.active = false;
        mp.gui.cursor.show(false, false);
    }

    mp.events.callRemote('SERVER:WORK:START');
});

mp.events.add("WORK:END",() => {
    mp.game.graphics.notify('~r~You have finished work!');

    if (browser != null) {
        browser.destroy();
        browser = null;
        mp.gui.cursor.show(false, false);
    }

    mp.events.callRemote('SERVER:WORK:END');
});