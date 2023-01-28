let pc = {
    active : false,
    self : null,
    head : null,
    el : null,
    openCar : function(model, owner){
        this.reset()
        this.head.html('Base of numbers')
        this.el.append('<input type="text" maxlength="5" placeholder="Car Number">')
        this.el.append('<div class="button">Punch</div>')
        this.el.append('<p>МАРКА: <span></span></p><p>OWNER: <span></span></p>')
        this.el.children('p:first').children().html(model)
        this.el.children('p:last').children().html(owner)
        this.set()
    },
    openWanted : function(data){
        this.reset()
        this.head.html('Now wanted')
        this.el.append('<ol></ol>')
        var json = JSON.parse(data);
        json.forEach(function(item, i, arr) {
            pc.el.children('ol').append('<li>'+item+'</li>');
        });
    },
    openPerson : function(fname,lname,pass,gender,lvl,lic){
        this.reset()
        this.head.html("Database")
        this.el.append('<input type="text" maxlength="30" placeholder="Passport / Name_Familia">')
        this.el.append('<div class="button">Punch</div>')
        this.el.append('<p>Name: <span>'+fname+'</span></p>')
        this.el.append('<p>Surname: <span>' + lname + '</span></p>')
        this.el.append('<p>Passport: <span>' + pass + '</span></p>')
        this.el.append('<p>Floor: <span>'+gender+'</span></p>')
        this.el.append('<p>Rosewear level: <span>'+lvl+'</span></p>')
        this.el.append('<p>List of licenses: <span>'+lic+'</span></p>')
        this.set()
    },
    clearWanted : function(){
        this.reset()
        this.head.html("Rosant")
        this.el.append('<input type="text" maxlength="30" placeholder="Passport / Name_Familia">')
        this.el.append('<div class="button">Clear wanted</div>')
        this.set()
    },
    set : function(){
        $('.button').on('click', function(){
            var t = $(this);
            //console.log(t);
            var data = $('input')[0].value;
            //console.log('pcMenuInput:'+data);
            mp.trigger('pcMenuInput', data);
        });
    },
    reset : function(){
        $('.button').off('click');
        $(".elements").empty();
    },
    show : function(){
        this.active = true
        this.self.css('display','block')
    },
    hide : function(){
        this.active = false
        this.self.css('display','none')
    }
}
$('.pc menu li').on('click', function(){
    var t = $(this)
    //console.log("pcMenu:"+t[0].id);
    mp.trigger('pcMenu', Number(t[0].id));
})
$(document).ready(function(){
    pc.head = $('.pc .right h1')
    pc.el = $('.pc .right .elements')
    pc.self = $('.pc');
})