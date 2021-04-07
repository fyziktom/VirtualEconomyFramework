// https://www.w3schools.com/howto/howto_js_sort_table.asp
function sortTable(tableID) {
    var table, rows, switching, i, x, y, shouldSwitch;
    table = document.getElementById(tableID);
    switching = true;
    /* Make a loop that will continue until
    no switching has been done: */
    while (switching) {
        // Start by saying: no switching is done:
        switching = false;
        rows = table.rows;
        /* Loop through all table rows (except the
        first, which contains table headers): */
        for (i = 0; i < (rows.length - 1); i++) {
            // Start by saying there should be no switching:
            shouldSwitch = false;
            /* Get the two elements you want to compare,
            one from current row and one from the next: */
            x = rows[i].getElementsByTagName("TD")[0];
            y = rows[i + 1].getElementsByTagName("TD")[0];
            // Check if the two rows should switch place:
            if (x != undefined) {
                if (x.innerHTML.toLowerCase() > y.innerHTML.toLowerCase()) {
                    // If so, mark as a switch and break the loop:
                    shouldSwitch = true;
                    break;
                }
            }
        }
        if (shouldSwitch) {
            /* If a switch has been marked, make the switch
            and mark that a switch has been done: */
            rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
            switching = true;
        }
    }
}

function saveIconCheck(data){
    
        $('#saveIcon').removeClass("fa fa-spinner");
    if (data = "OK") {
        $('#saveIcon').addClass("fa fa-check");
        setTimeout(function() {
            $('#saveIcon').removeClass("fa fa-check");
            $('#saveIcon').addClass("fa fa-save");
        }, 1000);
    }
    else {
        $('#saveIcon').addClass("fa fa-remove");
        setTimeout(function() {
            $('#saveIcon').removeClass("fa fa-remove");
            $('#saveIcon').addClass("fa fa-save");
        }, 1000);
    }
}

function ShowConfirmModal(action, text) {

    $('#confirmModalContent').text(text);
    
    $('#confirmModal').modal("show"); 
}

function loadRights() {

    var url = ApiUrl + "/security/login";

    if (bootstrapstudio) {
        url = url.replace(':8000',':8080'); //just for debug with BootstrapStudio
    }

    $.ajax(url,
        {
            method: 'GET',
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function
                if (data == null) {
                    UserRights = 0;
                    $('#userName').text("anonymous");
                } else {
                    UserRights = parseInt(data.Rights);
                    $('#userName').text(data.Name + ' (' + data.Login + ')');
                }
                
                HideVisibleItemsByRights();
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                UserRights = 0;
                $('#userName').text("anonymous");
                
                HideVisibleItemsByRights();
            }
        });
}

function loadConfig(loaded) {
    var url = ApiUrl + "/GetServerParams";

    if (bootstrapstudio) {
        url = url.replace(':8000',':8080'); //just for debug with BootstrapStudio
    }

    $.ajax(url,
        {
            method: 'GET',
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function

                mqttHost = data.MQTT.Host;
                mqttPort = data.MQTT.WSPort;
                mqttUser = data.MQTT.User;
                mqttPass = data.MQTT.Pass;

                loaded();

                checkComponents();

                tryToConnect = setInterval(function () {
                    ConnectMQTT();
                }, 1000);
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log("Cannot load configuration");
            }
        });
}

function ConnectMQTT() {
    initClient(mqttHost, mqttPort, mqttUser, mqttPass);
    console.log("Initialized MQTT on " + mqttHost + ":" + mqttPort);
    clearInterval(tryToConnect);
}

var tryToConnect = null;
var connectTryAfterStart = 0;

$(document).ready(function () {

    ApiUrl = document.location.origin + '/api';

    if (bootstrapstudio) {
        ApiUrl = ApiUrl.replace(':8000', ':8080');//just for debug with BootstrapStudio
    }

    loadRights();
    loadConfig(loaded);

});

function loaded() {    

    checkLocation();

    if (ActualPage == Pages.users) {
        userAfterLoad();
    }
    else if (ActualPage == Pages.wallets || 
             ActualPage == Pages.dashboard || 
             ActualPage == Pages.tokens || 
             ActualPage == Pages.games || 
             ActualPage == Pages.nodes || 
             ActualPage == Pages.messages || 
             ActualPage == Pages.keys ||
             ActualPage == Pages.shop) {

        walletAfterLoad();
        accountsAfterLoad();
        tokensAfterLoad();
        if (ActualPage == Pages.shop) {
            shopOnLoad(true);
        }
        if (ActualPage == Pages.keys) {
            keysAfterLoad();
        }
        if (ActualPage == Pages.games) {
            loadChessPageStartUp();
        }
        if (ActualPage == Pages.messages) {
            messagesAfterLoad();
        }
    }
}

function checkLocation() {
    var currentLocation = window.location.toString();

    if (currentLocation.includes('index')){
        ActualPage = Pages.wallets;
    }
    else if (currentLocation.includes('wallets')) {
        ActualPage = Pages.wallets;
    }
    else if (currentLocation.includes('accounts')) {
        ActualPage = Pages.accounts;
    }
    else if (currentLocation.includes('nodes')) {
        ActualPage = Pages.nodes;
    }
    else if (currentLocation.includes('users')) {
        ActualPage = Pages.users;
    }
    else if (currentLocation.includes('tokens')) {
        ActualPage = Pages.tokens;
    }
    else if (currentLocation.includes('chess')) {
        ActualPage = Pages.games;
    }
    else if (currentLocation.includes('messages')) {
        ActualPage = Pages.messages;
    }
    else if (currentLocation.includes('keys')) {
        ActualPage = Pages.keys;
    }
    else if (currentLocation.includes('shop')) {
        ActualPage = Pages.shop;
    }
    else {
        ActualPage = Pages.wallets;
    }
}

function ComandAPIRequest(apicommand, data) {

    var url = document.location.origin + "/api/" + apicommand;

    if (bootstrapstudio) {
        url = url.replace('8000','8080');
    }
    /*
    var data = {
        testString: "string test",
        testNumber: 12345,
        testDate: new Date()
    };*/

    //var strdat = JSON.stringify(data);

    $.ajax(url,
        {
            contentType: 'application/json;charset=utf-8',
            data: JSON.stringify(data),
            method: 'PUT',
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function
                console.log(`Status: ${status}, Data:${data}`);
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log('Error: "' + errorMessage + '"');
            }
        });
}

function downloadDataAsTextFile(data,filename){
    var text = data;
    //text = text.replace(/\n/g, "\r\n"); // To retain the Line breaks.
    var blob = new Blob([text], { type: "text/plain"});
    var anchor = document.createElement("a");
    anchor.download = filename;
    anchor.href = window.URL.createObjectURL(blob);
    anchor.target ="_blank";
    anchor.style.display = "none"; // just to be safe!
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
 }

 function isNumeric(num){
    return !isNaN(num)
}

function checkComponents() {
    var url = document.location.origin + '/api/' + 'IsRPCAvailable';

    if (bootstrapstudio) {
        url = url.replace('8000','8080');
    }

    $.ajax(url,
    {
        contentType: 'application/json;charset=utf-8',
        method: 'GET',
        dataType: 'json',   // type of response data
        timeout: 10000,     // timeout milliseconds
        success: function (data, status, xhr) {   // success callback function
            isRPCAvailable = data;
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback 
            console.log('Error: "' + errorMessage + '"');
        }
    });

    url = document.location.origin + '/api/' + 'IsDbAvailable';

    if (bootstrapstudio) {
        url = url.replace('8000','8080');
    }

    $.ajax(url,
    {
        contentType: 'application/json;charset=utf-8',
        method: 'GET',
        dataType: 'json',   // type of response data
        timeout: 10000,     // timeout milliseconds
        success: function (data, status, xhr) {   // success callback function
            isDbAvailable = data;
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback 
            console.log('Error: "' + errorMessage + '"');
        }
    });
}