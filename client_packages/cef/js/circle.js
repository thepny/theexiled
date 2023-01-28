var circleDesc = {
    "handshake": "Shake hands",
    "licenses": "Show licenses",
    "carinv":"Inventory",
    "doors":"Open/Close doors",
    "fraction":"Fraction",
    "offer":"Offer exchange",
    "givemoney":"Give money",
    "heal":"Cure",
    "hood":"Open/Close the hood",
    "leadaway":"To lead",
    "offerheal":"Suggest treatment",
    "passport":"Show passport",
    "search":"Search",
    "sellkit":"Sell a first-aid kit",
    "takegun":"Separate weapons",
    "takeillegal":"Has illegal",
    "trunk":"Open/Close trunk",
    "pocket": "Put on / take off the bag",
    "takemask": "Tear a mask",
    "rob": "Rob",
    "house": "House",
    "ticket": "Write a fine",

    "sellcar": "Sell a car",
    "sellhouse": "Sell a house",
    "roommate": "Take up to the house",
    "invitehouse": "Invite to the house",

    "acancel": "Stop animation",

    "acat1": "Sit/Lie down",
    "acat2": "Social",
    "acat3": "Phys. exercises",
    "acat4": "Indecent",
    "acat5": "Rack",
    "acat6": "Dancing",
	"acat7": "Facial emotions and gait styles",

    "seat1": "Sit reclining",
    "seat2": "Squat down",
    "seat3": "Sit on the ground",
    "seat4": "Lie on the ground",
    "seat5": "Wallow on the ground",
    "seat6": "Get on your knee",
    "seat7": "Sit relaxed",
    "seat8": "Sit on the stairs",

    "social1": "Raise your hands",
    "social2": "View and record",
    "social3": "Like",
    "social4": "Military salute",
	"social5": "Twist at the temple with both hands",
    "social6": "Royal greeting",
    "social7": "To show off",
	"socialnext1": "Next page",
	
	"social8": "Double like",
    "social9": "Scare",
    "social10": "Surrender",
    "social11": "Clap slowly",
	"social12": "Peace",
    "social13": "Refusal",
    "social14": "Joy",
	"socialnext2": "Next page",
	
	"social15": "Show fish",
    "social16": "Facepalm",
    "social17": "Show hen",
    "social18": "OK",
	"social19": "Call for yourself",
    "social20": "Rock!",
    "social21": "Peace to all",

    "phis1": "Physical jerks 1",
    "phis2": "Physical jerks 2",
    "phis3": "Pump muscles",
	"phis4": "Push up",
	"phis5": "Meditate",

    "indecent1": "Show middle finger",
    "indecent2": "Show something else",
	"indecent3": "Pick your nose",
	"indecent4": "Show middle finger everyone",
	"indecent5": "Show middle finger furiously",

    "stay1": "Stand, hands on the belt",
    "stay2": "Stretch your hands",
	"stay3": "Cross your arms over your chest",
	"stay4": "Stop, drive the person away",
	"stay5": "Stop, refuse to pass",
	"stay6": "Show biceps 1",
	"stay7": "Show biceps 2",
	"staynext1": "Next page",
	
	"stay8": "Show biceps 3",
    "stay9": "Show biceps 4",
	"stay10": "Show biceps 5",
	"stay11": "Show biceps 6",

    "dance1": "Dance 1",
    "dance2": "Dance 2",
    "dance3": "Dance 3",
    "dance4": "Dance 4",
    "dance5": "Dance 5",
    "dance6": "Dance 6",
    "dance7": "Dance 7",
	"dancenext1": "Next page",
	
    "dance8": "Dance 8",
	"dance9": "Dance 9",
    "dance10": "Dance 10",
    "dance11": "Dance 11",
    "dance12": "Dance 12",
    "dance13": "Dance 13",
    "dance14": "Dance 14",
	"dancenext2": "Next page",
	
	"dance15": "Dance 15",
	"dance16": "Dance 16",
    "dance17": "Dance 17",
    "dance18": "Dance 18",
    "dance19": "Dance 19",
    "dance20": "Dance 20",
    "dance21": "Dance 21",
	"dancenext3": "Next page",
	
    "dance22": "Dance 22",
	"dance23": "Dance 23",
	
	"mood0": "Clear facial emotion",
	"mood1": "Facial emotion: Contempt",
    "mood2": "Facial emotion: frowning",
    "mood3": "Facial emotion: Podshofe",
    "mood4": "Facial emotion: Fun",
    "mood5": "Facial emotion: Astonishment",
    "mood6": "Facial emotion: Anger",
	"moodnext1": "Next page",
	
	"ws0": "Clear gait style",
	"ws1": "Walking style: Swift",
    "ws2": "Walking style: Confident",
    "ws3": "Walking style: Podshofe",
    "ws4": "Walking style: Waddle",
    "ws5": "Walking style: Sad",
    "ws6": "Walking style: Feminine",
    "ws7": "Walking style: Scared",
    
    "use": "Use",
}
var circleData = {
    "Player":
    [
        ["givemoney", "offer", "fraction", "passport", "licenses", "heal", "house", "handshake"],
    ],
    "Vehicle":
    [
        ["hood", "trunk", "doors", "carinv"],
    ],
    "House":
    [
        ["sellcar", "sellhouse", "roommate", "invitehouse"],
    ],
    "Fraction":
    [
        [], // 0
        ["rob", "robguns", "pocket"], // 1
        ["rob", "robguns", "pocket"], // 2
        ["rob", "robguns", "pocket"], // 3
        ["rob", "robguns", "pocket"], // 4
        ["rob", "robguns", "pocket"], // 5
        ["leadaway"], // 6
        ["leadaway", "search", "takegun", "takeillegal", "takemask", "ticket"], // 7
        ["sellkit", "offerheal"], // 8
        ["leadaway", "search", "takegun", "takeillegal", "takemask"], // 9
        ["leadaway", "pocket", "rob", "robguns"], // 10
        ["leadaway", "pocket", "rob", "robguns"], // 11
        ["leadaway", "pocket", "rob", "robguns"], // 12
        ["leadaway", "pocket", "rob", "robguns"], // 13
        ["leadaway"], // 14
        ["leadaway"], // 15
        ["leadaway"], // 16
        ["leadaway", "search", "takegun", "takeillegal", "takemask"], // 17
        ["leadaway", "search", "takegun", "takeillegal", "takemask", "ticket"], // 18

    ],
    "Categories":
    [
        ["acat1", "acat2", "acat3", "acat4", "acat5", "acat6", "acat7", "acancel"],
    ],
    "Animations":
    [
        ["seat1", "seat2", "seat3", "seat4", "seat5", "seat6", "seat7", "seat8"],
        ["social1", "social2", "social3", "social4", "social5", "social6", "social7", "socialnext1"],
        ["phis1", "phis2", "phis3", "phis4", "phis5"],
        ["indecent1", "indecent2", "indecent3", "indecent4", "indecent5"],
        ["stay1", "stay2", "stay3", "stay4", "stay5", "stay6", "stay7", "staynext1"],
        ["dance1", "dance2", "dance3", "dance4", "dance5", "dance6", "dance7", "dancenext1"],
		["mood0", "mood1", "mood2", "mood3", "mood4", "mood5", "mood6", "moodnext1"],
		["dance8", "dance9", "dance10", "dance11", "dance12", "dance13", "dance14", "dancenext2"],
		["dance15", "dance16", "dance17", "dance18", "dance19", "dance20", "dance21", "dancenext3"],
		["dance22", "dance23"],
		["social8", "social9", "social10", "social11", "social12", "social13", "social14", "socialnext2"],
		["social15", "social16", "social17", "social18", "social19", "social20", "social21"],
		["ws0", "ws1", "ws2", "ws3", "ws4", "ws5", "ws6", "ws7"],
		["stay8", "stay9", "stay10", "stay11"],
    ],
}

var circle = new Vue({
    el: '.circle',
    data: {
        active: false,
        icons: [null,null,null,null,null,null,null,null],
        description: null,
        title: "title",
    },
    methods:{
        set: function(t,id){
            this.icons = circleData[t][id]
            this.description = t
            this.title = t
        },
        over: function(e){
            let id = e.target.id
            if(id == 8){
                this.description = "Close"
                return;
            }
            let iname = this.icons[id]
            //console.log(id, iname)
            if(iname == null){
                this.description = this.title
                return;
            }
            this.description = circleDesc[iname]
        },
        out: function(e){
            this.description = this.title
            //console.log('out')
        },
        btn: function(e){
            let id = e.target.id
            if(id == 8){
                mp.trigger("circleCallback", -1);
                this.hide();
                return;
            }
            mp.trigger("circleCallback", Number(e.target.id));
            this.hide();
        },
        show: function(t,id){
            this.active=true
            this.set(t,id)
            setTimeout(()=>{move('.circle').set('width', '480px').set('height', '480px').set('opacity', 1).end()}, 50);
        },
        hide: function(){
            //move('.circle').set('width', '80px').set('height', '80px').set('opacity', 0).end(()=>{circle.active=false})
            circle.active = false;
        }
    }
})