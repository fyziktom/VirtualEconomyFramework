
var chessInit = false;
var board = null; 
var allGamesIds = [];
var actualGameId = '';
var accountChessHistory = {};
var selectedChessHistory = '';
var selectedChessAccount = '';
var GameHistory = [];
var newMovedragged = false;
var replayRunning = false;

function onDragStart (source, piece, position, orientation) {
    newMovedragged = true;
    document.getElementById('btnChessHistory').innerText = 'New Unconfirmed Move';
    $('#chessAutoRefreshCheckBox').prop('checked', false);
    $('#btnConfirmChessGameMove').removeClass('btn-secondary').addClass('btn-primary');
}

function onChange (oldPos, newPos) {
    newMovedragged = true;
    document.getElementById('btnChessHistory').innerText = 'New Unconfirmed Move';
    $('#chessAutoRefreshCheckBox').prop('checked', false);
    $('#btnConfirmChessGameMove').removeClass('btn-secondary').addClass('btn-primary');
}

function loadChessPageStartUp() {

    if (chessInit){
        return;
    }

    $("#btnLoadAllChessGames").off();
    $("#btnLoadAllChessGames").click(function() {
        LoadAllChessGames();
    });

    $("#btnLoadChessGame").off();
    $("#btnLoadChessGame").click(function() {
        LoadChessGameRequest(true);
    });

    $("#btnLoadLastChessGameState").off();
    $("#btnLoadLastChessGameState").click(function() {
        LoadChessGameState();
    });   

    $("#btnRequestNewChessGame").off();
    $("#btnRequestNewChessGame").click(function() {
        checkAccountLockStatus(selectedChessAccount);
        setTimeout(() => {
            prepareRequestNewChessGame();
        }, 500);
    });

    $("#btnConfirmChessGameMove").off();
    $("#btnConfirmChessGameMove").click(function() {
        checkAccountLockStatus(selectedChessAccount);
        setTimeout(() => {
            prepareConfirmChessMove();
        }, 500);
        
    });

    $("#btnCapitulationChessGame").off();
    $("#btnCapitulationChessGame").click(function() {
        ChessCapitulation();
    });    

    $("#btnReplayChessGame").off();
    $("#btnReplayChessGame").click(function() {
        ReplayChessGame();
    });  

    var config = {
        draggable: true,
        position: 'start',
        onChange: onChange,
        onDragStart : onDragStart
    }

    board = Chessboard('board1', config);

    chessInit = true;

    setInterval(() => {
        if ($('#chessAutoRefreshCheckBox').is(':checked') && !newMovedragged && !replayRunning) {
            var pl2 = $('#chessPlayer2Address').val();

            if (selectedChessAccount != '' && actualGameId != '' && pl2 != ''){
                LoadChessGame(true);
            }
        }
    }, 5000);
}

var replayStep = 0;

function ReplayChessGame() {
    replayRunning = true;
    $('#chessBoardHeading').text('Board 1 - Replay');
    $('#btnReplayChessGame').removeClass('btn-primary').addClass('btn-secondary');
    replayStep = 0;
    setChessHistory(replayStep);
    replayStep++;
    var replayInt = setInterval(() => {
        setChessHistory(replayStep);

        if (replayStep == GameHistory.length-1) {
            replayRunning = false;
            $('#chessBoardHeading').text('Board 1');
            replayStep = 0;
            clearInterval(replayInt);
            $('#btnReplayChessGame').removeClass('btn-secondary').addClass('btn-primary');
        }

        replayStep++;

    }, 1500);


}

function LoadAllChessGames() {

    if (selectedChessAccount == '') {
        alert('Please select Account!');
        return;
    }

    $("#confirmButtonOk").off();
    $("#confirmButtonOk").click(function() {
        var url = document.location.origin + '/api/GetChessGamesIds/' + selectedChessAccount;

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
                    console.log(data);
                    allGamesIds = data;
                    reloadAccountChessGames();
                },
                error: function (jqXhr, textStatus, errorMessage) { // error callback 
                    console.log('Error: "' + errorMessage + '"');
                }
            });
    }); 

    ShowConfirmModal('', 'Do you want load available games?');

}

function LoadChessGameRequest(setToLast) {

    $("#confirmButtonOk").off();
    $("#confirmButtonOk").click(function() {
        LoadChessGame(setToLast);
    }); 

    ShowConfirmModal('', 'Do you realy want load game history and set last game state?');
}

function LoadChessGame(setToLast) {


    var pl2 = $('#chessPlayer2Address').val();
    if (pl2 == ''){
        alert('Player 2 Address cannot be empty!');
        return;
    }

    if (actualGameId == ''){
        alert('Actual Game cannot be empty!');
        return;
    }

    if (selectedChessAccount == ''){
        alert('Selected Account cannot be empty!');
        return;
    }

    var data = {
        "gameId": actualGameId,
        "player1Address": selectedChessAccount,
        "player2Address": pl2
    };

    var url = document.location.origin + '/api/LoadChessHistory';

    if (bootstrapstudio) {
        url = url.replace('8000','8080');
    }

    $.ajax(url,
        {
            contentType: 'application/json;charset=utf-8',
            method: 'PUT',
            data: JSON.stringify(data),
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function
                console.log(data);

                var newMove = false;
                if (GameHistory.length < data.length) {
                    newMove = true;
                }

                GameHistory = data.reverse();//JSON.parse(data.BoardStringData);

                if (GameHistory.length > 0) {
                    var dto = GameHistory[GameHistory.length - 1];

                    if (!waitingForPartnerAfterMove || (waitingForPartnerAfterMove && newMove)) {
                        if (dto.GameState != '') {
                            var gs = JSON.parse(dto.GameState);
                            var pos = JSON.parse(gs.BoardStringData);

                            var config = {
                                draggable: true,
                                position: pos,
                                onChange: onChange,
                                onDragStart : onDragStart
                            }
                            board = Chessboard('board1', config);
                        }
                        else {
                            var config = {
                                draggable: true,
                                position: 'start',
                                onChange: onChange,
                                onDragStart : onDragStart
                            }
                            board = Chessboard('board1', config);
                        }
                    }
                    var j = GameHistory.length-1;
                    for(var i = 0; i < GameHistory.length; i++) {
                        accountChessHistory[i] = j;
                        j--;
                    }
                    
                    
                    reloadChessHistory(selectedChessAccount);

                    if(setToLast) {
                        setChessHistory(GameHistory.length-1);
                    }
                    else {
                        setChessHistory(0);
                    }

                    if (newMove) {
                        alert('New Move Loaded!');
                        waitingForPartnerAfterMove = false;
                    }
                
                }
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log('Error: "' + errorMessage + '"');
            }
        });
}

function LoadChessGameState() {

    if (actualGameId == '') {
        alert('Please select Game!');
        return;
    }

    $("#confirmButtonOk").off();
    $("#confirmButtonOk").click(function() {
        var url = document.location.origin + '/api/GetActualChessState/' + actualGameId;

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
                    console.log(data);
                    if (data.BoardStringData != null){
                        if (data.BoardStringData != '') {
                            var pos = JSON.parse(data.BoardStringData);
                            var config = {
                                draggable: true,
                                position: pos,
                                onChange: onChange,
                                onDragStart : onDragStart
                            }
                            board = Chessboard('board1', config);
                            
                            setChessHistory(GameHistory.length-1);
                        }
                    }
                },
                error: function (jqXhr, textStatus, errorMessage) { // error callback 
                    console.log('Error: "' + errorMessage + '"');
                }
            });
    }); 

    ShowConfirmModal('', 'Do you realy want load last game state?');
}

function prepareRequestNewChessGame() {
    if (accountLockState) {
        $('#unlockAccountForOneTxConfirm').off();
        $("#unlockAccountForOneTxConfirm").click(function() {
            var password = $('#unlockAccountForOneTxPassword').val();
            RequestNewChessGame(password);
        });

        $('#addPassForTxMessageModal').modal('show');
    }
    else {
        RequestNewChessGame(null);
    }
}

function RequestNewChessGame(password) {

    if (selectedChessAccount == '') {
        alert('Please select Account!');
        return;
    }

    var pl2 = $('#chessPlayer2Address').val();
    if (pl2 == ''){
        alert('Player 2 Address cannot be empty!');
        return;
    }

    var pass = '';
    if (password != null) {
        pass = password;
    }

    var data = {
        "player1Address": selectedChessAccount,
        "player2Address": pl2,
        "password": pass
    };

    $("#confirmButtonOk").off();
    $("#confirmButtonOk").click(function() {
        var url = document.location.origin + '/api/StartNewChessGame';

        if (bootstrapstudio) {
            url = url.replace('8000','8080');
        }

        $.ajax(url,
            {
                contentType: 'application/json;charset=utf-8',
                data: JSON.stringify(data),
                method: 'PUT',
                dataType: 'text',   // type of response data
                timeout: 10000,     // timeout milliseconds
                success: function (data, status, xhr) {   // success callback function
                    actualGameId = data;

                    var config = {
                        draggable: true,
                        position: 'start',
                        onChange: onChange,
                        onDragStart : onDragStart
                    }
                
                    board = Chessboard('board1', config);

                    alert('New Game Requested!');
                },
                error: function (jqXhr, textStatus, errorMessage) { // error callback 
                    console.log('Error: "' + errorMessage + '"');
                }
            });
    });

    ShowConfirmModal('', 'Do you realy want to start new game with player: ' + data.player2Address +'?');
}

var waitingForPartnerAfterMove = false;

function prepareConfirmChessMove() {
    if (accountLockState) {
        $('#unlockAccountForOneTxConfirm').off();
        $("#unlockAccountForOneTxConfirm").click(function() {
            var password = $('#unlockAccountForOneTxPassword').val();
            ConfirmChessMove(password);
        });

        $('#addPassForTxMessageModal').modal('show');
    }
    else {
        ConfirmChessMove(null);
    }
}

function ConfirmChessMove(password) {

    if (!isUserOnMoveNow) {
        alert('This is not your turn, you cannot confirm move now. Wait for partner move!');
        return;
    }

    if (selectedChessAccount == '') {
        alert('Please select Account!');
        return;
    }

    var pl2 = $('#chessPlayer2Address').val();
    if (pl2 == ''){
        alert('Player 2 Address cannot be empty!');
        return;
    }

    if (actualGameId == '') {
        alert('Please select Game!');
        return;
    }

    if (!newMovedragged) {
        alert('You did not move!');
        return;
    }

    var pass = '';
    if (password != null) {
        pass = password;
    }

    var state = JSON.stringify(board.position());
    if (state != null) {
        if(state != '') {
            var data = {
                "player1Address": selectedChessAccount,
                "player2Address": pl2,
                "gameId": actualGameId,
                "chessboardState" : state,
                "password" : pass
            };
        
            $("#confirmButtonOk").off();
            $("#confirmButtonOk").click(function() {

                var url = document.location.origin + '/api/ConfirmChessMove';

                if (bootstrapstudio) {
                    url = url.replace('8000','8080');
                }
        
                $.ajax(url,
                    {
                        contentType: 'application/json;charset=utf-8',
                        data: JSON.stringify(data),
                        method: 'PUT',
                        dataType: 'text',   // type of response data
                        timeout: 10000,     // timeout milliseconds
                        success: function (data, status, xhr) {   // success callback function
                            //actualGameId = data;
                            alert('New Move Sent. Wait for info about partner move!');
                            waitingForPartnerAfterMove = true;
                            $('#chessAutoRefreshCheckBox').prop('checked', true);
                            //LoadChessGame(true);
                            newMovedragged = false;
                        },
                        error: function (jqXhr, textStatus, errorMessage) { // error callback 
                            console.log('Error: "' + errorMessage + '"');
                        }
                    });
            });

            ShowConfirmModal('', 'Do you realy want to confirm and send the move?');
        }
    }
}

function ChessCapitulation() {
    console.log(JSON.stringify(board.position()));
}

/////////////////////////////////////////////
// drop downs

function setChessAccount(account, name) {
    console.log(account);
    selectedChessAccount = account;
    document.getElementById('btnChessAccount').innerText = name;

    LoadAllChessGames();
    //reloadChessHistory(account);
}

function reloadChessAccounts() {
    try{
        document.getElementById('chessAccountsDropDown').innerHTML = '';

        for (var a in Accounts) {
            var acc = Accounts[a];
            document.getElementById('chessAccountsDropDown').innerHTML += '<button class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setChessAccount(\'' + acc.Address + '\',\'' + acc.Name + '\')\">' + acc.Name + ' - ' + acc.Address + '</button>';
        }
    }
    catch{
    }
}

function setAccountChessGameId(id) {
    actualGameId = id;
    document.getElementById('btnAccountChessGames').innerText = id;

    LoadChessGameRequest(true);
}

function reloadAccountChessGames() {

    try{
        document.getElementById('chessAccountGamesDropDown').innerHTML = '';

        for (var indx in allGamesIds) {
            gameId = allGamesIds[indx];
            document.getElementById('chessAccountGamesDropDown').innerHTML += '<button class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setAccountChessGameId(\'' + gameId + '\')\">' + gameId + '</button>';
        }
    }
    catch{
    }
}

var isUserOnMoveNow = false;
function setChessHistory(historytx) {
    newMovedragged = false;
    $('#btnConfirmChessGameMove').removeClass('btn-primary').addClass('btn-secondary');

    console.log(historytx);
    selectedChessHistory = historytx;
    //document.getElementById('btnChessHistory').innerText = historytx;

    var ach = accountChessHistory[historytx];
    var dto = GameHistory[ach];

    document.getElementById('btnChessHistory').innerText = historytx + ' - ' + dto.TimeStamp.toString();

    if (dto.GameState != '') {
        var gs = JSON.parse(dto.GameState);
        var pos = JSON.parse(gs.BoardStringData);

        if (gs.LastMovePlayer != selectedChessAccount) {
            $('#chessOnMove').text('Your Turn!');
            isUserOnMoveNow = true;
        }
        else {
            $('#chessOnMove').text('Partner Turn!');
            isUserOnMoveNow = false;
        }

        for(var p in dto.Players) {
            if (p != selectedChessAccount) {
                $('#chessPlayer2Address').val(p);
            }
        }

        var config = {
            draggable: true,
            position: pos,
            onChange: onChange,
            onDragStart : onDragStart
        }
        board = Chessboard('board1', config);
    }
    else {
        var config = {
            draggable: true,
            position: 'start',
            onChange: onChange,
            onDragStart : onDragStart
        }
        board = Chessboard('board1', config);
    }

}

function reloadChessHistory(account) {

    try{
        document.getElementById('chessHistoryDropDown').innerHTML = '';

        for (var h in accountChessHistory) {
            var dto = GameHistory[accountChessHistory[h]];
            document.getElementById('chessHistoryDropDown').innerHTML += '<button class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setChessHistory(\'' + h + '\')\">' + h + ' - ' + dto.TimeStamp.toString() + '</button>';
        }
    }
    catch{
    }
}