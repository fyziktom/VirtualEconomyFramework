
$(document).ready(function () {

    $("#btnLogin").off();
    $("#btnLogin").click(function() {
        $('#loginPass').val(null); 
        $('#loginError').text(null); 
        $('#loginModal').modal("show");

        $('#loginModal').on('shown.bs.modal', function () {
            $(function() {
                $('#loginName').focus();
            });
        });
        
    });

    $("#btnConfirmLogin").off();
    $("#btnConfirmLogin").click(function() {
        LoginRequest();
    });

    $("#btnCancelLogin").off();
    $("#btnCancelLogin").click(function() {
        CancelLogin();
    });

    $("#btnCloseLogin").off();
    $("#btnCloseLogin").click(function() {
        CancelLogin();
    });

    $("#btnLogOut").off();
    $("#btnLogOut").click(function() {
        LogOut();
    });

    $('#loginName').bind("enterKey",function(e){
        $('#loginPass').focus();
     });
     $('#loginName').keyup(function(e){
         if(e.keyCode == 13)
         {
             $(this).trigger("enterKey");
         }
     });

    $('#loginPass').bind("enterKey",function(e){
        LoginRequest();
     });
     $('#loginPass').keyup(function(e){
         if(e.keyCode == 13)
         {
             $(this).trigger("enterKey");
         }
     });

});

function LoginRequest() {
    var name = $('#loginName').val();
    var pass = $('#loginPass').val();
    

    console.log('Name is: ' + name + 'Pass is: ' + pass);

    var url = ApiUrl+ "/security/login"; 
 
    var obj = { 
        Login: name, 
        Pass: pass, 
    }; 
 
    $.ajax(url, 
        { 
            contentType: 'application/json;charset=utf-8', 
            data: JSON.stringify(obj), 
            method: 'POST', 
            dataType: 'json',   // type of response data 
            timeout: 10000,     // timeout milliseconds 
            success: function (data, status, xhr) {   // success callback function 
                console.log(data.Login + " " + data.Name + "" + data.Rights); 
                $('#loginPass').val(null); 
                $('#loginModal').modal("hide");  
                $('#userName').text(data.Name + ' (' + data.Login + ')'); 
                UserRights = data.Rights; 
                HideVisibleItemsByRights(); 
            }, 
            error: function (jqXhr, textStatus, errorMessage) { // error callback  
                $('#loginError').text(jqXhr.responseText); 
                $('#loginPass').val(null); 
                UserRights = 0; 
                $('#userName').text("anonymous"); 
                HideVisibleItemsByRights(); 
            } 
        }); 
 
}


function LogOut() {

    console.log('Logging Out');

    var url = ApiUrl+ "/security/logout"; 
 
    $.ajax(url, 
        { 
            contentType: 'application/json;charset=utf-8', 
            data: '', 
            method: 'POST', 
            dataType: 'json',   // type of response data 
            timeout: 10000,     // timeout milliseconds 
            success: function (data, status, xhr) {   // success callback function 
                $('#loginError').text(jqXhr.responseText); 
                $('#loginPass').val(null); 
                UserRights = 0; 
                $('#userDropDownMenu').removeClass('show');
                $('#userName').text("anonymous"); 
                HideVisibleItemsByRights(); 
            }, 
            error: function (jqXhr, textStatus, errorMessage) { // error callback  
                $('#loginError').text(jqXhr.responseText); 
                $('#loginPass').val(null); 
                $('#userDropDownMenu').removeClass('show');
                UserRights = 0; 
                $('#userName').text("anonymous"); 
                HideVisibleItemsByRights(); 
            } 
    }); 

    /*
    $.cookie('userName', null, { expires: 1 });
    $.cookie('userLogin', null, { expires: 1 });
    $.cookie('UserRights', null, { expires: 1 });
    $.cookie('remember', false, { expires: 1 });
    */
}

function CancelLogin() {
    window.location.replace(document.location.origin);
    HideVisibleItemsByRights();
}

function HideVisibleItemsByRights() {
    if (!bootstrapstudio) {
        $(".rights-admin").show();
    }
    else {
        if (UserRights == 1) {
            $(".rights-admin").show();
        }
        else {
            $(".rights-admin").hide();
        }
    }
}