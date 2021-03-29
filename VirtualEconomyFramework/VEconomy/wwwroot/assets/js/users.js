
function userAfterLoad() {

    $("#btnAddUserShowModal").off();
    $("#btnAddUserShowModal").click(function() {
        AddUserShowModal();
    });

    $("#btnAddUser").off();
    $("#btnAddUser").click(function() {
        AddUser();
    });

    $("#btnUpdateUser").off();
    $("#btnUpdateUser").click(function() {
        UpdateUser();
    });

    $("#btnDeleteUser").off();
    $("#btnDeleteUser").click(function() {
        DeleteUser();
    });

    LoadRights();

    setInterval(function() {
        if (ActualPage == Pages.users) {
            if (UserRights == 1) {
                GetUsers();
            }
        }
    },2500);
}

function DeleteUser() {

    var url = ApiUrl + "/security/user";
    var user = $('#userDetailsLogin').val();

    $.ajax(url,
        {
            method: 'DELETE',
            contentType: "application/json",
            data: JSON.stringify(user),
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function
                console.log('User deleted');
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                if (jqXhr.status == 200) {
                    console.log('User deleted');
                }
            }
        });
}

function UpdateUser() {

    var login = $('#userDetailsLogin').val();
    var name = $('#userDetailsName').val();
    var pass = $('#userDetailsPass').val();
    var email = $('#userDetailsEmail').val();
    
    var url = ApiUrl+ "/security/user"; 
 
    var obj = { 
        login: login, 
        name: name,
        email: email,
        rights: selectedRightsIndx,
        validFrom: '2020-12-22T11:30:58.257Z',
        validTo: '2021-12-22T11:30:58.257Z',
        active: true
    }; 
 
    $.ajax(url, 
        { 
            contentType: 'application/json;charset=utf-8', 
            data: JSON.stringify(obj), 
            method: 'PUT', 
            dataType: 'json',   // type of response data 
            timeout: 10000,     // timeout milliseconds 
            success: function (data, status, xhr) {   // success callback function 
                console.log('User updated.');
                SetUserPassword();
            }, 
            error: function (jqXhr, textStatus, errorMessage) { // error callback  
                console.log(jqXhr);
                if (jqXhr.status == 200) {
                    console.log('User updated.');
                    SetUserPassword();
                }
            } 
        });     
}

function LoadRights() {

    var url = ApiUrl + "/security/getRights";

    $.ajax(url,
        {
            method: 'GET',
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function
                AvailableRights = data;
                console.log(AvailableRights);

                reloadRightsDropDown();

                try{
                    reloadRolesDropDown();
                }
                catch{}
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log(jqXhr);
            }
        });
}

var selectedRightsIndx = 1;
var selectedRightsName = '';

function selectRights(rights) {
    selectedRightsIndx = rights;
    selectedRightsName = AvailableRights[rights];
    $('#btnSelectedRights').text(selectedRightsName);
}

function reloadRightsDropDown() {

    $('#rightsList').children().remove();
    for(var r in AvailableRights) {
        var row =
        '<a class="dropdown-item" href="#" onclick="selectRights(' + r +')">' + AvailableRights[r] + '</a>';
        $('#rightsList').append(row);
    }

    try{
        $('#rightsList1').children().remove();
        for(var r in AvailableRights) {
            var row =
            '<a class="dropdown-item" href="#" onclick="selectRights(' + r +')">' + AvailableRights[r] + '</a>';
            $('#rightsList1').append(row);
        }
    }
    catch{}

}

function reloadRolesDropDown() {

    try {
        $('#rolesList').children().remove();
        for(var r in AvailableRights) {
            if (AvailableRights[r].includes('Role')) {
                var row =
                '<a class="dropdown-item" href="#" onclick="selectRights(' + r +')">' + AvailableRights[r] + '</a>';
                $('#rolesList').append(row);
            }
        }
    }
    catch{}
}

function AddUserShowModal() {
    $('#addUserModal').modal('show');
}

function AddUser() {
    var login = $('#userLoginInput').val();
    var name = $('#userNameInput').val();
    var pass = $('#userPassInput').val();
    var email = 'none';// $('#userPass').val();
    
    var url = ApiUrl+ "/security/user"; 
 
    var obj = { 
        login: login, 
        name: name,
        email: email,
        rights: selectedRightsIndx,
        validFrom: '2020-12-22T11:30:58.257Z',
        validTo: '2021-12-22T11:30:58.257Z',
        active: true
    }; 
 
    $.ajax(url, 
        { 
            contentType: 'application/json;charset=utf-8', 
            data: JSON.stringify(obj), 
            method: 'POST', 
            dataType: 'json',   // type of response data 
            timeout: 10000,     // timeout milliseconds 
            success: function (data, status, xhr) {   // success callback function 
                console.log('User added.');
                SetUserPassword();
            }, 
            error: function (jqXhr, textStatus, errorMessage) { // error callback  
                console.log(jqXhr);
                if (jqXhr.status == 200) {
                    console.log('User added.');
                    SetUserPassword();
                }
            } 
        }); 
 
}

function SetUserPassword() {
    var login = $('#userLoginInput').val();
    var pass = $('#userPassInput').val();
    
    var url = ApiUrl+ "/security/userPassword"; 
 
    var obj = { 
        login: login, 
        pass: pass,
    }; 
 
    $.ajax(url, 
        { 
            contentType: 'application/json;charset=utf-8', 
            data: JSON.stringify(obj), 
            method: 'PUT', 
            dataType: 'json',   // type of response data 
            timeout: 10000,     // timeout milliseconds 
            success: function (data, status, xhr) {   // success callback function 
                console.log(data);
            }, 
            error: function (jqXhr, textStatus, errorMessage) { // error callback  
                console.log(jqXhr);
            } 
        }); 
 
}

var AvailableUsers = {};

function GetUsers() {

    var url = ApiUrl + "/security/users";

    $.ajax(url,
        {
            method: 'GET',
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function
                AvailableUsers = data;
                //console.log(AvailableUsers);
                reloadUsers();
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                if (jqXhr.status == 403) {
                    console.log('Please login to continue');
                }
            }
        });
}

function reloadUsers() {
    if (AvailableUsers != null) {
        var list = $('#usersTable tbody');
        list.children().remove();

        AvailableUsers.forEach((usr) => {
            var row =
                '<tr>' +
                '<td>' + usr.Login + '</td>' +
                '<td>' + usr.Name + '</td>' +
                '<td>' + AvailableRights[usr.Rights] + '</td>' +
                '<td>' + usr.Email + '</td>' +
                '<td>' +
                    '<button class="btn btn-secondary" type="button" style="width: 40px;" onclick="openUserDetails(\''+ usr.Login +'\')">' +
                    '<i class="fa fa-info-circle"></i></button>' +
                '</td>' +
                '</tr>'; 

            list.append(row);
        });
    }
}

function openUserDetails(usrLogin) {
    
    AvailableUsers.forEach((usr) => {
        if (usr.Login == usrLogin) {
            
            $('#userDetailsLogin').val(usr.Login);
            $('#userDetailsName').val(usr.Name);
            $('#btnUserDetailsSelectedRights').text(AvailableRights[usr.Rights]);
            $('#userDetailsEmail').val(usr.Email);
            $('#userDetailsEmail').val(usr.Email);
            $('#userDetailsModal').modal('show');

            reloadRightsDropDown();
        }
    });

}