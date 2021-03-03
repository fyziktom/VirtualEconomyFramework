var client;

function initClient(host, port, user, pass){
  client = new Paho.Client(host, Number(port), "mqttjs_" + Math.random().toString(16).substr(2, 8));

  // set callback handlers
  client.onConnectionLost = onConnectionLost;
  client.onMessageArrived = onMessageArrived;

  // connect the client
  client.connect({onSuccess:onConnect, userName: user, password: pass});

}

// called when the client connects
function onConnect() {
  // Once a connection has been made, make a subscription and send a message.
  console.log("onConnect");
    
  client.subscribe(`VEF/Wallets`);
  client.subscribe(`VEF/Nodes`);
  client.subscribe(`VEF/Accounts`);
  client.subscribe(`VEF/Cryptocurrencies`);
  client.subscribe(`VEF/Owners`); 
  client.subscribe(`VEF/NewTransaction`);
  client.subscribe(`VEF/TokensReceived`);
  //message = new Paho.MQTT.Message("Hello");
  //message.destinationName = "World";
  //client.send(message);
}

// called when the client loses its connection
function onConnectionLost(responseObject) {
  if (responseObject.errorCode !== 0) {
    console.log("onConnectionLost:"+responseObject.errorMessage);
    client.connect({onSuccess:onConnect, userName: "user", password: "password"});
  }
}

// called when a message arrives
function onMessageArrived(message) {

  checkLocation();

  try{
      
    //console.log("onMessageArrived on topic :" + message.topic + " with payload:" + message.payloadString);
    if (message.topic == `VEF/Wallets` ){
      try {
          WalletsRefresh(JSON.parse(message.payloadString));
      }
      catch{}
    }
    else if (message.topic == `VEF/Cryptocurrencies` ){
      try {
         CryptocurrenciesRefresh(JSON.parse(message.payloadString));
      }
      catch{}
    }
    else if (message.topic == `VEF/Owners`) {
      try {
        OwnersRefresh(JSON.parse(message.payloadString).RFIDTag);
      }
      catch{}
    }
    else if (message.topic == `VEF/NewTransaction`) {
      try {
        //NewTransactionArrived(JSON.parse(message.payloadString));
        showNewTxDetails(JSON.parse(message.payloadString));
      }
      catch{}
    }
    else if (message.topic == `VEF/TokensReceived`) {
      try {
        showNewTokensDetails(JSON.parse(message.payloadString));
      }
      catch{}
    }
    else if (message.topic == `VEF/Nodes`) {
      try {
        refreshNodesStatus(JSON.parse(message.payloadString));
      }
      catch{}
    }
    else if (message.topic == `VEF/Accounts`) {
      try {
        refreshAccountsStatus(JSON.parse(message.payloadString));
      }
      catch{}
    }
    

    //HideVisibleItemsByRights();
  }
  catch{}

  
}