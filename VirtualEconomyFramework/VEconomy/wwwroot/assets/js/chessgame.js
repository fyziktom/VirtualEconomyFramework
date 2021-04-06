// https://chessboardjs.com/examples.html
// https://github.com/jhlywa/chess.js

var chessInit = false;
var board = null; 
var game = new Chess();
var whiteSquareGrey = '#a9a9a9';
var blackSquareGrey = '#696969';
var $status = $('#chessStatus');

var allGamesIds = [];
var actualGameId = '';
var accountChessHistory = {};
var selectedChessHistory = '';
var selectedChessAccount = '';
var GameHistory = [];
var newMovedragged = false;
var replayRunning = false;


function removeGreySquares () {
    $('#board1 .square-55d63').css('background', '')
  }
  
  function greySquare (square) {
    var $square = $('#board1 .square-' + square)
  
    var background = whiteSquareGrey
    if ($square.hasClass('black-3c85d')) {
      background = blackSquareGrey
    }
  
    $square.css('background', background)
  }

function onDragStart (source, piece, position, orientation) {
    // do not pick up pieces if the game is over
    if (game.game_over()) return false;

    // only pick up pieces for the side to move
    if ((game.turn() === 'w' && piece.search(/^b/) !== -1) ||
        (game.turn() === 'b' && piece.search(/^w/) !== -1)) {
        return false;
    }

    newMovedragged = true;
    document.getElementById('btnChessHistory').innerText = 'New Unconfirmed Move';
    $('#chessAutoRefreshCheckBox').prop('checked', false);
    $('#btnConfirmChessGameMove').removeClass('btn-secondary').addClass('btn-primary');
}

function onDrop (source, target) {

    removeGreySquares();

    // see if the move is legal
    var move = game.move({
      from: source,
      to: target,
      promotion: 'q' // NOTE: always promote to a queen for example simplicity
    });
  
    // illegal move
    if (move === null) return 'snapback';
  
    updateStatus();
}

// update the board position after the piece snap
// for castling, en passant, pawn promotion
function onSnapEnd () {
    board.position(game.fen());
}

function onMouseoverSquare (square, piece) {
    // get list of possible moves for this square
    var moves = game.moves({
      square: square,
      verbose: true
    })
  
    // exit if there are no moves available for this square
    if (moves.length === 0) return
  
    // highlight the square they moused over
    greySquare(square);
  
    // highlight the possible squares for this piece
    for (var i = 0; i < moves.length; i++) {
      greySquare(moves[i].to);
    }
}
  
function onMouseoutSquare (square, piece) {
    removeGreySquares();
}

function updateStatus () {
    var status = '';
  
    var moveColor = 'White';
    if (game.turn() === 'b') {
      moveColor = 'Black';
    }
  
    // checkmate?
    if (game.in_checkmate()) {
      status = 'Game over, ' + moveColor + ' is in checkmate.';
    }
  
    // draw?
    else if (game.in_draw()) {
      status = 'Game over, drawn position';
    }
  
    // game still on
    else {
      status = moveColor + ' to move';
  
      // check?
      if (game.in_check()) {
        status += ', ' + moveColor + ' is in check';
      }
    }
  
    $status.html(status);
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
        onDragStart : onDragStart,
        onDrop: onDrop,
        onMouseoutSquare: onMouseoutSquare,
        onMouseoverSquare: onMouseoverSquare,
        onSnapEnd: onSnapEnd
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
                                onDragStart : onDragStart,
                                onDrop: onDrop,
                                onMouseoutSquare: onMouseoutSquare,
                                onMouseoverSquare: onMouseoverSquare,
                                onSnapEnd: onSnapEnd
                                
                            }
                            board = Chessboard('board1', config);
                        }
                        else {
                            var config = {
                                draggable: true,
                                position: 'start',
                                onChange: onChange,
                                onDragStart : onDragStart,
                                onDrop: onDrop,
                                onMouseoutSquare: onMouseoutSquare,
                                onMouseoverSquare: onMouseoverSquare,
                                onSnapEnd: onSnapEnd
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
                                onDragStart : onDragStart,
                                onDrop: onDrop,
                                onMouseoutSquare: onMouseoutSquare,
                                onMouseoverSquare: onMouseoverSquare,
                                onSnapEnd: onSnapEnd
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
                        onDragStart : onDragStart,
                        onDrop: onDrop,
                        onMouseoutSquare: onMouseoutSquare,
                        onMouseoverSquare: onMouseoverSquare,
                        onSnapEnd: onSnapEnd
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
            var add = a.substring(0,3) + '...' + a.substring(a.length-3); 
            document.getElementById('chessAccountsDropDown').innerHTML += '<button class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setChessAccount(\'' + acc.Address + '\',\'' + acc.Name + '\')\">' + acc.Name + ' - ' + add + '</button>';
        }
    }
    catch{
    }
}

function setAccountChessGameId(id) {
    actualGameId = id;
    var gmId = id.substring(0,4) + '...' + id.substring(id.length-4);
    document.getElementById('btnAccountChessGames').innerText = gmId;

    LoadChessGameRequest(true);
}

function reloadAccountChessGames() {

    try{
        document.getElementById('chessAccountGamesDropDown').innerHTML = '';

        for (var indx in allGamesIds) {
            gameId = allGamesIds[indx];
            var gmId = gameId.substring(0,4) + '...' + gameId.substring(gameId.length-4);
            document.getElementById('chessAccountGamesDropDown').innerHTML += '<button class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setAccountChessGameId(\'' + gameId + '\')\">' + gmId + '</button>';
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

        for(var p in dto.Players) {
            if (p != selectedChessAccount) {
                $('#chessPlayer2Address').val(p);
            }
        }

        var config = {
            draggable: true,
            position: pos,
            onChange: onChange,
            onDragStart : onDragStart,
            onDrop: onDrop,
            onMouseoutSquare: onMouseoutSquare,
            onMouseoverSquare: onMouseoverSquare,
            onSnapEnd: onSnapEnd
        }

        board = Chessboard('board1', config);

        var saved_positions =  board.fen();
        if (saved_positions == 'rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1') {
            game = new Chess();
        }

        var onmove = 'w';

        if (gs.LastMovePlayer != selectedChessAccount) {
            $('#chessOnMove').text('Your Turn!');
            isUserOnMoveNow = true;
            if(dto.Players[selectedChessAccount].FigureType == 0) {
                onmove = 'b';
            }
            else if (dto.Players[selectedChessAccount].FigureType == 1) {
                onmove = 'w';
            }
        }
        else {
            $('#chessOnMove').text('Partner Turn!');
            isUserOnMoveNow = false;

            var player = null;
            for(var p in dto.Players) {
                if (p != selectedChessAccount) {
                    player = p;
                }
            }

            if(dto.Players[p].FigureType == 0) {
                onmove = 'b';
            }
            else if (dto.Players[p].FigureType == 1) {
                onmove = 'w';
            }
        }

        game.load(saved_positions + ' ' + onmove + ' - - 0 1');
        //game.setTurn('w');

        updateStatus();
    }
    else {
        var config = {
            draggable: true,
            position: 'start',
            onChange: onChange,
            onDragStart : onDragStart,
            onDrop: onDrop,
            onMouseoutSquare: onMouseoutSquare,
            onMouseoverSquare: onMouseoverSquare,
            onSnapEnd: onSnapEnd
        }
        board = Chessboard('board1', config);
        game = new Chess();
        updateStatus();
    }

}

function reloadChessHistory(account) {

    try{
        document.getElementById('chessHistoryDropDown').innerHTML = '';

        for (var h in accountChessHistory) {
            var dto = GameHistory[accountChessHistory[h]];
            document.getElementById('chessHistoryDropDown').innerHTML += '<button style="font-size:10px;" class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setChessHistory(\'' + h + '\')\">' + h + ' - ' + dto.TimeStamp.toString() + '</button>';
        }
    }
    catch{
    }
}