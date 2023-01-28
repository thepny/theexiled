var jobselector = new Vue({
    el: ".joblist",
    data: {
        active: false,
        jobid: -1,
        level: 1,
        list: 
        [
            {class: "electro", name: "Elitect", level: 0, jobid: 1},
            {class: "gazon", name: "Lawnmock", level: 0, jobid: 5},
            {class: "pochta", name: "Postman", level: 1, jobid: 2},
            {class: "taxi", name: "Taxi driver", level: 2, jobid: 3},
            {class: "bus", name: "Bus driver", level: 2, jobid: 4},
            {class: "mechanic", name: "Mechanic", level: 4, jobid: 8},
            {class: "truck", name: "Trucker", level: 5, jobid: 6},
            {class: "inkos", name: "Collection", level: 8, jobid: 7},
        ],
    },
    methods: {
        closeJobMenu: function() {
            mp.trigger("closeJobMenu");
        },
        show: function (level, currentjob) {
            this.level = level;
            this.jobid = currentjob;
            this.active = true;
        },
        hide: function () {
            this.active = false;
        },
        selectJob: function(jobid) {
            mp.trigger("selectJob", jobid);
        }
    }
})