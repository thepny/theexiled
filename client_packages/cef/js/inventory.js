var itemsData = {
    "-1": "Mask",
    "-3": "Gloves",
    "-4": "Trousers",
    "-5": "Backpack",
    "-6": "Footwear",
    "-7": "Accessory",
    "-8": "Underwear",
    "-9": "Broochet",
    "-10": "Decorations",
    "-11": "Outerwear",
    "-12": "Headdress",
    "-13": "Glasses",
    "-14": "Accessory",
    0: "Test Item",
    1: "First aid kit",
    2: "Kanister",
    3: "Crisps",
    4: "Beer",
    5: "Pizza",
    6: "Burger",
    7: "Hot dog",
    8: "Sandwich",
    9: "eCola",
    10: "Sprunk",
    11: "Lock for locks",
    12: "Bag with money",
    13: "Materials",
    14: "Drugs",
    15: "Drill bag",
    16: "Military Lock",
    17: "Bag",
    18: "Tie",
    19: "Car keys",
    40: "Present",
    41: "Bunch of keys",

    20: `"On the crust of lemon"`,
    21: `"To cranberry"`,
    22: `"Russian standard"`,
    23: `"Asahi"`,
    24: `"Midori"`,
    25: `"Yamazaki"`,
    26: `"Martini Asti"`,
    27: `"Sambuca"`,
    28: `"Campari"`,
    29: `"Jivan"`,
    30: `"Ararat"`,
    31: `"Noyan Tapan"`,

    100: "Pistol",
    101: "Combat Pistol",
    102: "Pistol .50",
    103: "SNS Pistol",
    104: "Heavy Pistol",
    105: "Vintage Pistol",
    106: "Marksman Pistol",
    107: "Revolver",
    108: "AP Pistol",
    109: "Stun Gun",
    110: "Flare Gun",
    111: "Double Action",
    112: "Pistol Mk2",
    113: "SNSPistol Mk2",
    114: "Revolver Mk2",

    115: "Micro SMG",
    116: "Machine Pistol",
    117: "SMG",
    118: "Assault SMG",
    119: "Combat PDW",
    120: "MG",
    121: "Combat MG",
    122: "Gusenberg",
    123: "Mini SMG",
    124: "SMG Mk2",
    125: "Combat MG Mk2",

    126: "Assault Rifle",
    127: "Carbine Rifle",
    128: "Advanced Rifle",
    129: "Special Carbine",
    130: "Bullpup Rifle",
    131: "Compact Rifle",
    132: "Assault Rifle Mk2",
    133: "Carbine Rifle Mk2",
    134: "Special Carbine Mk2",
    135: "Bullpup Rifle Mk2",

    136: "Sniper Rifle",
    137: "Heavy Sniper",
    138: "Marksman Rifle",
    139: "Heavy Sniper Mk2",
    140: "Marksman Rifle Mk2",

    141: "Pump Shotgun",
    142: "SawnOff Shotgun",
    143: "Bullpup Shotgun",
    144: "Assault Shotgun",
    145: "Musket",
    146: "Heavy Shotgun",
    147: "Double Barrel Shotgun",
    148: "Sweeper Shotgun",
    149: "Pump Shotgun Mk2",

    180: "Knife",
    181: "Club",
    182: "Hammer",
    183: "Bat",
    184: "Scrap",
    185: "golf club",
    186: "Bottle",
    187: "Dagger",
    188: "Axe",
    189: "Brass knuckles",
    190: "Machete",
    191: "Flashlight",
    192: "Swiss knife",
    193: "Cue",
    194: "Key",
    195: "Battle ax",

    200: "Pistol Caliber",
    201: "Small caliber",
    202: "Automatic Caliber",
    203: "Sniper Caliber",
    204: "Fraction",
	
	// Fishing
	205: "Fishing rod",
	206: "Improved fishing rod",
	207: "Fishing rod mk2",
    208: "Racial",
    209: "Smelt",
    210: "Kunja",
    211: "Salmon",
    212: "Perch",
    213: "Sturgeon",
    214: "Skate",
	215: "Tuna",
	216: "Acne",
	217: "Black Amur",
	218: "Pike",
	
	// AlcoShop
	219: "Martini Asti",
	220: "Sambuca",
	221: "Vodka with lemon",
	222: "Vodka on cranberries",
	223: "Russian standard",
	224: "Cognac Jivan",
	225: "Cognac Ararat",
	226: "Beer on tap",
	227: "Bottled beer",
    228: "Hookah",
    234: "Harvest",
	235: "Seeds",
    777: "Repair kit",

    556: "Such.Pakov",
}

var itemsInfo = {
	"-1": "The mask will help hide your identity.",
    "-3": "Gloves.",
    "-4": "Trousers.",
    "-5": "Backpack.",
    "-6": "Shoes.",
    "-7": "Jewelry/Tie.",
    "-8": "Underwear.",
    "-9": "Bulletproof vest - will help you receive less damage from a shot.",
    "-10": ".",
    "-11": "Outerwear.",
    "-12": "Hats.",
    "-13": "Glasses.",
    "-14": "Watches/Keychains.",
    1:	"First aid kit - will help to reanimate or restore your health.",
    2: "Container of gasoline - will help to refuel the vehicle anywhere.",
    3: "A pack of chips - will help restore hunger.",
    4:"Non alcoholic beer.",
    5:"Pizza - will help restore hunger.",
    6: "Burger - will help restore hunger.",
    7:  "Hot Dog - will help restore hunger.",
	8:	"Sandwich - will help restore hunger.",
	9:	"Cola - helps restore thirst.",
	10:	"Sprunk - helps restore thirst.",
	11:	"Master key - will help to open the lock.",
	12:	"Bag with money - most likely stolen money.",
	13:	"Materials - help craft weapons.",
	14:	"A bag of marijuana - will help restore health.",
	15:	"A bag with a drill - will help open the vault.",
	16:	"Military master key - will help to hide military equipment.",
	17:	"Bag - putting it on a person, he will not see.",
	18:	"Ties - help to bind a person.",
	19:	"Keys - will help you open your personal vehicle.",
	20:	"Drinking - gives an alcoholic effect.",
	21:	"Drinking - gives an alcoholic effect.",
	22:	"Drinking - gives an alcoholic effect.",
	23:	"Drinking - gives an alcoholic effect.",
	24:	"Drinking - gives an alcoholic effect.",
	25:	"Drinking - gives an alcoholic effect.",
	26:	"Drinking - gives an alcoholic effect.",
	27:	"Drinking - gives an alcoholic effect.",
	28:	"Drinking - gives an alcoholic effect.",
	1:	"First aid kit - will help to reanimate or restore your health.",
    556: "Dry Ration (MRE) - Fully restores Food and Water.",
}

Vue.component('item', {
	template: '<div :class="test"><div class="item" v-bind:title="name" v-bind:class="{active: isactive}" @click.right.prevent="select"> \
    <img :src="src"><span>{{count}}</span><!--<p class="sub">{{subdata}}</p>--><p class="names">{{name}}<br><a>{{info}}</a><b>{{count}} pieces.</b></p></div></div>',
    props: ['id', 'index', 'count', 'isactive', 'type', 'subdata'],
    data: function () {
        return {
            src: 'items/' + this.id + '.png',
			title: itemsData[this.id],
            name: itemsData[this.id],
            info: itemsInfo[this.id],
			test: 'item' + this.id + 'ma',
        }
    },
    methods: {
        select: function (event) {
            board.sType = (this.type == 'inv') ? 1 : 0;
            board.sID = this.id;
            board.sIndex = this.index;
            context.type = (this.type == 'inv') ? 1 : 0;
        }
    }
})
var board = new Vue({
    el: ".board",
    data: {
        active: false,
        outside: false,
		zohan: ["Level", "Warns", "Date of creation", "Phone number", "Bank number", "Passport number", "Fraction", "Rank", "Work", "Status", "You promocode"],
		outType: 0,
        outHead: "Other inventory", 
		//		0        1  	 2 		     3 		        4	   5            6		  7			  8				9	  10	  11        12	      13    
		stats: ["15", "30/60", "777 777", "Администрация", "2", "A B C D", "01.05.1980", "Строитель", "CityHall", "17", "Vovan", "Putin", "333 666", "4276 7700", "noref"],
        items: [[-6, 5, 1],[-7, 5, 1],[-8, 5, 1],[-9, 5, 1],[-11, 5, 1],[-12, 5, 1],[-13, 5, 1],[-14, 5, 1],[-1, 5, 1],[-3, 5, 1],[-4, 5, 1],[1, 5, 1],[1, 5, 1],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [1, 5, 1],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0], [10, 500, 0], [11, 100, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0], [10, 500, 0], [11, 100, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0]],
        outitems: [[1, 5, 1],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0],[5, 10, 0],[5, 10, 0],[5, 10, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0]],
		money: 0,
		donate: 0,
		bank: 0, 
		page: 1,
		statis: 0,
        sIndex: 0,
        sType: 0,
        sID: 0,
        key: 0,
		arraymax: 0,
        balance: 0,
        menu: 0,
        totrans: null,
        aftertrans: null,
        fname: null,
        pause:0,
        lname: null,
        properties: null,
    },
    methods: {
        context: function (event) {
            if (clickInsideElement(event, 'item')) {
                context.show(event.pageX, event.pageY)
            } else {
                context.hide()
            }
        },
		        close: function(){
            this.active = false
            this.balance = 0;
            this.menu = 0;
            this.totrans = null;
            this.aftertrans = null;
			this.fname = null;
			this.lname = null;
        },
        onInputTrans: function(){
            if(!this.check(this.totrans)){
                this.totrans = null;
                this.aftertrans = null;
            } else {
				if(Number(this.totrans) < 0) this.totrans = 0;
                this.aftertrans = Number(this.totrans) * 1000;
            }
        },
        onInputName: function(){
            if(this.check(this.fname) || this.check(this.lname)){
                this.fname = null;
                this.lname = null;
            }
        },
        check: function(str) {
            return (/[^a-zA-Z]/g.test(str));
        },
        back: function(){
            this.menu = 4;
        },
        open: function(id){
            this.menu = id;
        },
		
        wheel: function(id){
            this.pause = new Date().getTime();
            let data = null;
            mp.trigger("wheel", id, data);
        },
        buy: function(id){
            if (new Date().getTime() - this.pause < 5000) {
                mp.events.call('notify', 4, 9, "Wait 5 seconds", 3000);
                return;
            }
            this.pause = new Date().getTime();
            let data = null;
            switch(id){
                case 1:
                data = this.fname+"_"+this.lname;
                break;
                case 2:
                data = this.totrans;
                break;
                case 9:
                    data = this.totrans;
                    break;
                default:
                break;
            }
            mp.trigger("donbuy", id, data);
        },
		show: function(stars){
			this.balance = stars;
		},
        hide: function (event) {
            context.hide()
        },
        outSet: function (json) {
            this.key++
            this.outType = json[0]
            this.outHead = json[1]
            this.outitems = json[2]
        },
		pages: function(id){
            this.page = id;
        },
		statsid: function(id){
            this.statis = id;
        },
		itemsSet: function(t) {
                this.key++, this.items = t, this.usedFastSlots = [!1, !1, !1, !1, !1];
                for (let t = 1; t < 6; t++) mp.trigger("bindSlotKey", 0, t, !1);
                for (let t = 0; t < this.items.length; t++) {
                    const s = this.items[t];
                    s[6] > 0 && (this.usedFastSlots[s[6]] = !0, mp.trigger("bindSlotKey", t, s[6], !0))
                }
                this.updateWeight()
            },
            itemUpd: function(t, s) {
                this.key++, this.items[t] = s, this.updateWeight()
            }, 
			updateWeight: function() {
                let t = 0;
                this.items.forEach(s => {
                    t += s[4] * s[1]
                }), this.weight = t
            },
            useFastSlot: function(t) {
                this.usedFastSlots[t] || (this.selectFastSlot = !1, this.sFastSlot = t, this.items[board.sIndex][6] = this.sFastSlot, this.usedFastSlots[t] = !0, mp.trigger("useFastSlot", this.sIndex, this.sFastSlot, 0))
            },
            unsetFastSlot: function() {
                let t = this.items[board.sIndex][6];
                this.usedFastSlots[t] = !1, this.items[board.sIndex][6] = 0, this.key++, mp.trigger("useFastSlot", this.sIndex, 0, t)
            },
        send: function (id) {
            let type = (this.sType) ? 0 : this.outType
            mp.trigger('boardCB', id, type, this.sIndex)
        }
    }
})
var context = new Vue({
    el: ".context_menu",
    data: {
		men: ["Use", "Hand over", "Take", "Drop"],
        active: false,
        style: '',
        type: true,
        fastSlot: -1
    },
    methods: {
        show: function (x, y) {
            this.style = `left:${x}px;top:${y}px;`
            this.active = true
        },
        hide: function () {
            this.active = false
        },
        btn: function (id) {
            this.hide()
            board.send(id)
        },
        setFastSlot: function() {
		board.usedFastSlots.includes(!1) && (this.hide(), board.selectFastSlot = !0)
            },
            unsetFastSlot: function() {
                this.hide(), board.unsetFastSlot()
            }
    }
})
function clickInsideElement(e, className) {
    var el = e.srcElement || e.target;
    if (el.classList.contains(className)) {
        return el;
    } else {
        while (el = el.parentNode) {
            if (el.classList && el.classList.contains(className)) {
                return el;
            }
        }
    }
    return false;
}