$(document).ready(function () {

    $("#btnAddNewNode").off();
    $("#btnAddNewNode").click(function() {
        AddNewNode();
    });
    
    $("#btnRemoveNode").off();
    $("#btnRemoveNode").click(function() {
        DeleteNode();
    });

    $("#btnSaveChangedNodeData").off();
    $("#btnSaveChangedNodeData").click(function() {
        UpdateNode();
    });  
    
    $("#btnLoadNodeDataFromFile").off();
    $("#btnLoadNodeDataFromFile").click(function() {
        loadNodeDataFromFile();
    });   

    $("#btnSaveNodeData").off();
    $("#btnSaveNodeData").click(function() {
        downloadNodeDataAsFile();
    });   

    $("#btnOpenNodeParametersDetails").off();
    $("#btnOpenNodeParametersDetails").click(function() {
        if (ActualNode != null) {
            showNodeParametersDetails();
        }
    });  

    $("#btnSaveChangedNodeParameters").off();
    $("#btnSaveChangedNodeParameters").click(function() {
        saveChangedNodeParameters();
    });

    $("#btnTestScript").off();
    $("#btnTestScript").click(function() {
        testScript();
    });

    registerNodesFilter();

    try {
        getNodeTriggerTypes();
        getNodeTypes();
    }
    catch {
        console.log('Cannot load node types from API.');
    }
});

function getNodeComponent(node) {
    
    var state = 'Inactive';

    if (node.IsActivated) {
        state = 'Active';
    }

    var type = getNodeTypeByIndx(node.Type);
    var trigger = getNodeTriggerTypeByIndx(node.ActualTriggerType);

    var accountName = "Not connected to account";
    var account = getAccountByID(node.AccountId);
    if (account != null){
        accountName = account.Name;
    }

    var nodeComponent = '<tr>'+
    '    <td>' + node.Name + '</td>'+
    '    <td>' + accountName + '</td>'+
    '    <td>' + state + '</td>'+
    '    <td>' + type + '</td>'+
    '    <td>' + trigger + '</td>'+
    '    <td>'+
    '<button class="btn btn-primary" type="button" style="margin: 5px;" onclick="showNodeDetails(\'' + node.Id + '\')">Details</button>'+
    '</td>'+
    '</tr>';
    
    return nodeComponent;
}

function refreshNodesStatus(nodes) {
    Nodes = nodes;

    ////////////////////////////
    // todo reload ActualNode

    var list = $('#nodesTable tbody');
    list.children().remove();

    for (var n in nodes) {
         nd = nodes[n];
         if (nd != null && nd != undefined) {
            list.append(getNodeComponent(nd));
         }
    }	
    
    filterNodesTable($('#nodesTableFilterInput').val());
}

function getNodeTriggerTypes() {
    var url = ApiUrl + "/GetNodeTriggersTypes";

    if (bootstrapstudio) {
        url = url.replace(':8000',':8080'); //just for debug with BootstrapStudio
    }

    $.ajax(url,
        {
            method: 'GET',
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function

                NodeTriggersTypes = data;
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log("Cannot load configuration");
            }
        });
}

function getNodeTypes() {
    var url = ApiUrl + "/GetNodesTypes";

    if (bootstrapstudio) {
        url = url.replace(':8000',':8080'); //just for debug with BootstrapStudio
    }

    $.ajax(url,
        {
            method: 'GET',
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function

                NodeTypes = data;
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log("Cannot load configuration");
            }
        });
}

function getNodeTypeByIndx(typeIndx) {
    if (NodeTypes != null) {
        if (NodeTypes.length > typeIndx) {
            return NodeTypes[typeIndx];
        }
    }
}

function getNodeTypeByName(name) {
    if (NodeTypes != null) {
        for(var i = 0; i < NodeTypes.length; i++) {
            if (NodeTypes[i] == name) {
                return i;
            }
        }
    }
}

function getNodeTriggerTypeByIndx(typeIndx) {
    if (NodeTriggersTypes != null) {
        if (NodeTriggersTypes.length > typeIndx) {
            return NodeTriggersTypes[typeIndx];
        }
    }
}

function getNodeTriggerTypeByName(name) {
    if (NodeTriggersTypes != null) {
        for(var i = 0; i < NodeTriggersTypes.length; i++) {
            if (NodeTriggersTypes[i] == name) {
                return i;
            }
        }
    }
}

function getNodeById(nodeId) {
    if (Nodes != null) {
        for(var n in Nodes) {
            if (Nodes[n] != undefined && Nodes[n] != null) {
                if (Nodes[n].Id == nodeId) {
                    return Nodes[n];
                }
            }
        }
    }
}

//////////////////////////////
// API calls

function UpdateNode() {

    if (ActualNode != null) {
        var params = ActualNode.ParsedParams;
        params.TriggerType = selectedNodeTriggerType;
        selectedNodeActivationState = ActualNode.IsActivated;

        selectedNodeAccountAddress = getAccountByID(ActualNode.AccountId).Address;

        var node = {
            "nodeId": ActualNode.Id,
            "accountAddress": selectedNodeAccountAddress,
            "nodeName": $('#nodeDetailsName').val(),
            "isActivated": selectedNodeActivationState,
            "nodeType": selectedNodeType,
            "parameters": params
        };
    
        $("#confirmButtonOk").off();
        $("#confirmButtonOk").click(function() {
            ComandAPIRequest('UpdateNode', node);
        });
        
        ShowConfirmModal('', 'Do you realy want to update this node: '+ node.nodeName +'?');  
    }
}

function DeleteNode() {

    var node = {
        "nodeId": $('#nodeDetailsId').text(),
     };
 
     $("#confirmButtonOk").off();
     $("#confirmButtonOk").click(function() {
        ComandAPIRequest('DeleteNode', node);
    });
    
    ShowConfirmModal('', 'Do you realy want to remove this node: '+ $('#nodeDetailsName').text() +'?');  
}

////////////////////////////////////////////////////
// Node details

function fillNodeDetails(node) {
    
    if (node != null) {
        var accnt = getAccountByID(node["AccountId"]);
        var state = 'InActive';

        if (node["IsActivated"]){ 
            state = 'Active';
            selectedNodeActivationState = true;
        }

        $('#nodeDetailsId').text(node["Id"]);
        $('#nodeDetailsName').val(node["Name"]);
        $('#btnNodeActivation').text(state);
        if (accnt != undefined || accnt != null) {
            $('#btnNodeAccountAddress').text(accnt.Name + ' - ' + accnt.Address);
            selectedNodeAccountAddress = accnt.Address;
        }
        else {
            $('#btnNodeAccountAddress').text('Not connected to account');
        }
        $('#btnNodeTriggerType').text(getNodeTriggerTypeByIndx(node["ActualTriggerType"]));
        $('#btnNodeType').text(getNodeTypeByIndx(node["Type"]));
        selectedNodeType = node.Type;
        selectedNodeTriggerType = node.ActualTriggerType;

        $('#nodeFullData').text(JSON.stringify(node, null, 2));

        ActualNode = node;
    }
}

function showNodeDetails(nodeId) {
    var node = getNodeById(nodeId);
    if (node != null) {
        ActualNode = node;
        fillNodeDetails(node);
        reloadNodeTypesDropDown();
        reloadNodeTriggerTypesDropDown();
        reloadNodeActivationStatusDropDown();
        reloadNodeAccountsAddressesDropDown();
        $('#nodeDetailsModal').modal("show"); 
    }
}

function showNodeDetailsByNodeObj(node) {
    if (node != null) {
        fillNodeDetails(node);
        reloadNodeTypesDropDown();
        reloadNodeTriggerTypesDropDown();
        reloadNodeActivationStatusDropDown();
        reloadNodeAccountsAddressesDropDown();
        $('#nodeDetailsModal').modal("show"); 
    }
}

////////////////////////////////
// node parameters

function fillNodeParametersDetails(node) {
    if (node != null) {
        $('#nodeParametersDetailsCommand').val(node.ParsedParams.Command);
        jar.updateCode(node.ParsedParams.Script);

        $('#nodeParametersDetailsIsScriptActive').prop('checked', node.ParsedParams.IsScriptActive);
        $('#nodeParametersDetailsTimeDelay').val(node.ParsedParams.TimeDelay.toString());
        $('#nodeParametersDetailsLogin').val(node.ParsedParams.Login);
        $('#nodeParametersDetailsPassword').val(node.ParsedParams.Password);
        $('#btnNodeParamTriggerType').text(getNodeTriggerTypeByIndx(node.ParsedParams.TriggerType));
    }

    $("#btnOpenNodeSpecificParametersModal").off();
    $("#btnOpenNodeSpecificParametersModal").click(function() {
        showNodeSpecificParametersDetails(node);
    });  
}

function showNodeParametersDetails() {
    getNodeSpecificParametersShape();
    fillNodeParametersDetails(ActualNode);
    reloadNodeParamTriggerTypesDropDown();
    $('#nodeParametersDetailsModal').modal("show"); 
}

function saveChangedNodeParameters() {

    $("#confirmButtonOk").off();
    $("#confirmButtonOk").click(function() {
        var code = jar.toString();
        ActualNode.ParsedParams.Command = $('#nodeParametersDetailsCommand').val();
        ActualNode.ParsedParams.Script = code;
        ActualNode.ParsedParams.TimeDelay = parseInt($('#nodeParametersDetailsTimeDelay').val());
        ActualNode.ParsedParams.Login = $('#nodeParametersDetailsLogin').val();
        ActualNode.ParsedParams.Password = $('#nodeParametersDetailsPassword').val();
        ActualNode.ParsedParams.TriggerType = getNodeTriggerTypeByName($('#btnNodeParamTriggerType').text());
        ActualNode.ActualTriggerType = getNodeTriggerTypeByName($('#btnNodeParamTriggerType').text());
        
        if(!$("#nodeParametersDetailsIsScriptActive").is(':checked')) {
            ActualNode.ParsedParams.IsScriptActive = false;
        }
        else {
            ActualNode.ParsedParams.IsScriptActive = true;
        }

        showNodeDetailsByNodeObj(ActualNode);
        setTimeout(() => {
            $('#nodeParametersDetailsModal').modal("toggle"); 
        }, 500);
   });
   
   ShowConfirmModal('', 'Do you realy want to save these parameters? Remember still need to save whole node!');  
}

///////////////////////////////////////////
// node specific parameters
var specificParamsShape = {};

function getNodeSpecificParametersShape() {
    var url = ApiUrl + '/GetNodeSpecificParametersCarrier?nodeType=' + ActualNode.Type.toString();

    if (bootstrapstudio) {
        url = url.replace(':8000',':8080'); //just for debug with BootstrapStudio
    }

    $.ajax(url,
        {
            method: 'GET',
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function
                specificParamsShape = data;
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log(errorMessage);
            }
        });
}

function getParamLine(name, value) {

    var line = '<tr>'+
        '    <td class="d-xl-flex justify-content-xl-end parameterName">'+
        '       <input type="text" class="parameterName" style="width: 200px;" value="' + name + '" readonly />'+
        '    </td>'+
        '    <td class="parameterContent">'+
        '       <input type="text" class="parameterContent" style="width: 200px;margin-left: 20px;" value="' + value + '" />'+
        '    </td>'+
        '</tr>';
        
    $('#nodeSpecificParametersTable tbody').append(line);
}

function fillNodeSpecificParametersDetails(node) {
    if (node != null) {
        $('#nodeSpecificParametersTable tbody').children().remove();
        var parm = null;
        if (node.ParsedParams.Parameters != undefined) {
            if (node.ParsedParams.Parameters != null) {
                if (node.ParsedParams.Parameters != '' || node.ParsedParams.Parameters != '' ) {
                    try {
                        parm = JSON.parse(node.ParsedParams.Parameters);
                    }
                    catch {
                        console.log('Cannot parse JSON with node specific parameters!');
                    }
                }
            }
        }

        for (var o in specificParamsShape) {
            // todo if node has some params like this, fill it
            var p = null;
            if (parm != null) {
                if (parm[o] != undefined) {
                    if (parm[o] != null) {
                        p = parm[o];
                    }
                }
            }
            if (p == null) {
                p = specificParamsShape[o];
            }

            getParamLine(o, p); 
        }
   }
}

function showNodeSpecificParametersDetails(node) {

    fillNodeSpecificParametersDetails(node);

    $("#btnSaveNodeSpecificParameters").off();
    $("#btnSaveNodeSpecificParameters").click(function() {
        saveNodeSpecificParameters(node);
    }); 
    
    reloadNodeParamTriggerTypesDropDown();
    $('#nodeSpecificParametersDetailsModal').modal("show"); 
}

function getSpecificParametersToSave() {
    var params = {};
    var lines = $('#nodeSpecificParametersTable > tbody > tr').each(function(index, tr) {
        var name = $(tr).find('td.parameterName').find('input.parameterName').val();
        var content = $(tr).find('td.parameterContent').find('input.parameterContent').val();

        if (content != '' && content != ' ') {
            if (content == 'true') {
                params[name] = true;
            }
            else if (content == 'false') {
                params[name] = false;
            }
            else if (isNumeric(content)) {
                if (content.includes('.') || content.includes(',')) {
                    params[name] = parseFloat(content);
                    if (params[name] == null) {
                        params[name] = 0;
                    }
                }
                else {
                    params[name] = parseInt(content);
                    if (params[name] == null) {
                        params[name] = 0;
                    }
                }
            }
            else {
                params[name] = content;
            }
        }
        else if (content == '' || content == ' ') {
            params[name] = content;
        }
    });

    return params;
}

function saveNodeSpecificParameters() {

    var node = {
        "nodeId": $('#nodeDetailsId').val(),
     };
 
     $("#confirmButtonOk").off();
     $("#confirmButtonOk").click(function() {
        var p = getSpecificParametersToSave();
        if (p != null) {
            ActualNode.ParsedParams.Parameters = JSON.stringify(p);
            ActualNode.Parameters = JSON.stringify(ActualNode.ParsedParams);
        }
        $('#nodeSpecificParametersDetailsModal').modal("toggle"); 
    });
    
    ShowConfirmModal('', 'Do you realy want to save these parameters? Remember still need to save whole node!');  
}


///////////////////////////////////////////////
// Node dropdowns

var selectedNodeType = 0;

function setNodeType(type) {
    selectedNodeType = type;
    var t = getNodeTypeByIndx(type);
    document.getElementById('btnNodeType').innerText = t;
}

function reloadNodeTypesDropDown() {
    if (NodeTypes != null) {
        document.getElementById('nodeTypesDropDown').innerHTML = '';
        for (var nt in NodeTypes) {
            var t = getNodeTypeByIndx(nt);
            document.getElementById('nodeTypesDropDown').innerHTML += '<button class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setNodeType(\'' + nt + '\')\">' + t + '</button>';
        }
    }
}

var selectedNodeTriggerType = 0;

function setNodeTriggerType(type) {
    selectedNodeTriggerType = type;
    var t = getNodeTriggerTypeByIndx(type);
    document.getElementById('btnNodeTriggerType').innerText = t;
}

function reloadNodeTriggerTypesDropDown() {
    if (NodeTriggersTypes != null) {
        document.getElementById('nodeTriggerTypesDropDown').innerHTML = '';
        for (var nt in NodeTriggersTypes) {
            var t = getNodeTriggerTypeByIndx(nt);
            document.getElementById('nodeTriggerTypesDropDown').innerHTML += '<button class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setNodeTriggerType(\'' + nt + '\')\">' + t + '</button>';
        }
    }
}

function setNodeParamTriggerType(type) {
    setNodeTriggerType(type);
    selectedNodeTriggerType = type;
    var t = getNodeTriggerTypeByIndx(type);
    document.getElementById('btnNodeParamTriggerType').innerText = t;
}

function reloadNodeParamTriggerTypesDropDown() {
    if (NodeTriggersTypes != null) {
        document.getElementById('nodeParamTriggerTypesDropDown').innerHTML = '';
        for (var nt in NodeTriggersTypes) {
            var t = getNodeTriggerTypeByIndx(nt);
            document.getElementById('nodeParamTriggerTypesDropDown').innerHTML += '<button class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setNodeParamTriggerType(\'' + nt + '\')\">' + t + '</button>';
        }
    }
}

var selectedNodeActivationState = false;

function setNodeActivationStatus(status) {
    selectedNodeActivationState = status;
    if (status) {
        document.getElementById('btnNodeActivation').innerText = 'Active';
    }
    else {
        document.getElementById('btnNodeActivation').innerText = 'InActive';
    }
}

function reloadNodeActivationStatusDropDown() {
    document.getElementById('nodeActivationDropDown').innerHTML = '';
    document.getElementById('nodeActivationDropDown').innerHTML += '<button class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setNodeActivationStatus(false)\">InActive</button>';
    document.getElementById('nodeActivationDropDown').innerHTML += '<button class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setNodeActivationStatus(true)\">Active</button>';
}

var selectedNodeAccountAddress = '';

function setNodeAccountAddress(accountAddress) {
    selectedNodeAccountAddress = accountAddress;
    var a = Accounts[accountAddress];
    if (a != null) {
        document.getElementById('btnNodeAccountAddress').innerText = a.Name + ' - ' + accountAddress;
    }
}

function reloadNodeAccountsAddressesDropDown() {
    if (Accounts != null) {
        document.getElementById('nodeAccountAddressesDropDown').innerHTML = '';
        for (var acc in Accounts) {
            var a = Accounts[acc];
            document.getElementById('nodeAccountAddressesDropDown').innerHTML += '<button style=\"width: 400px;font-size:12px\" class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setNodeAccountAddress(\'' + acc + '\')\">' + a.Name + ' - ' + acc + '</button>';
        }
    }
}

//////////////////////////////////////
// files

function downloadNodeDataAsFile() {
    var fn = 'nodeData-' + Date.now().toString() + '.json';
    downloadDataAsTextFile($('#nodeFullData').text(), fn);
}

function loadNodeDataFromFile() {
    $('#nodeFullData').text('');

    let file = document.querySelector("#file-input").files[0];
    let reader = new FileReader();
    reader.addEventListener('load', function(e) {
            let text = e.target.result;
            var nd = JSON.parse(text);
            $('#nodeFullData').text(JSON.stringify(nd, null, 2));
    });
    
    reader.readAsText(file);
}

////////////////////////////////////
// Add new node

var addNodeSelectedNodeType = 0;

function addNodeSetNodeType(type) {
    addNodeSelectedNodeType = type;
    var t = getNodeTypeByIndx(type);
    document.getElementById('btnAddNodeModalNodeType').innerText = t;
}

function reloadAddNodeNodeTypesDropDown() {
    if (NodeTypes != null) {
        document.getElementById('addNodeModalNodeTypeDropDown').innerHTML = '';
        for (var nt in NodeTypes) {
            var t = getNodeTypeByIndx(nt);
            document.getElementById('addNodeModalNodeTypeDropDown').innerHTML += '<button class=\"dropdown-item btn btn-light\" ' +  'onclick=\"addNodeSetNodeType(\'' + nt + '\')\">' + t + '</button>';
        }
    }
}

function AddNewNode() {

    reloadAddNodeNodeTypesDropDown();

    $("#btnAddNodeModalConfirm").off();
    $("#btnAddNodeModalConfirm").click(function() {
        addNewNodeRequest();
    });  
    
    $('#addNodeModal').modal('show');
}

function addNewNodeRequest() {
    var name = $('#addNodeModalNodeName').val();
    if (name == '') {
        alert('Name cannot be empty!');
        return;
    }

    var node = {
        "nodeId": '',
        "accountAddress": '',
        "nodeName": name,
        "isActivated": false,
        "nodeType": addNodeSelectedNodeType,
        "parameters": {}
    };

    $("#confirmButtonOk").off();
    $("#confirmButtonOk").click(function() {
        ComandAPIRequest('UpdateNode', node);
    });
    
    ShowConfirmModal('', 'Do you realy want to add this node: '+ node.nodeName +'?'); 
}

///////////////////////////
// node table filter

function registerNodesFilter() {
    $("#nodesTableFilterInput").on("keyup", function() {
        var value = $(this).val().toLowerCase();
        filterNodesTable(value);
    });
}

function filterNodesTable(value) {
    $("#nodesTable > tbody > tr").filter(function() {      
        $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
    });
}

/////////////////////////////////////
// Testing script and node

function testScript(){
    
    if(!$("#chbxTestWithNodeFunction").is(':checked')) {
        testScriptWithNode();
    }
    else {
        testScriptWithNode();
    }
}

function testScriptWithoutNode() {
 // just in the browser - could not be same as JS core in core
}

function testScriptWithNode() {
    if (ActualNode != null) {

        var script = jar.toString();
        if (script == '') {
            alert('Script cannot be empty!');
            return;
        }

        var node = {
            "nodeId": ActualNode.Id,
            "triggerType": 5,
            "NumberOfCalls": 1,
            "UseLastTokenTxData": true,
            "altScript": jar.toString(),
            "useAltScript": true
        };
    
        $("#confirmButtonOk").off();
        $("#confirmButtonOk").click(function() {
            InvokeNodeCommandAPI(node);
        });
        
        ShowConfirmModal('', 'Do you realy want to test this node: '+ ActualNode.Name +'?');  
    }
}

function InvokeNodeCommandAPI(node){

    var url = ApiUrl + "/InvokeNodeAction";

    if (bootstrapstudio) {
        url = url.replace('8000','8080');
    }

    $.ajax(url,
        {
            contentType: 'application/json;charset=utf-8',
            data: JSON.stringify(node),
            method: 'PUT',
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function
                alert(`Status: ${status}, Data:${JSON.stringify(data)}`);
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                alert('Error: "' + errorMessage + '"');
            }
        });
}