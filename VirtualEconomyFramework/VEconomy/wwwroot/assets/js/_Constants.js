var confirmDone = false;
var confirmFinished = false;

//configuration will ve loaded after start, no defaults
var apiaddr = null;;
var mqttHost = null;
var mqttPort = null;
var mqttUser = null;
var mqttPass = null;

var WalletTypes = [];
var Wallets = {};
var ActualWallet = {};
var Accounts = {};
var ActualAccount = {};
var AccountTypes = {};
var Nodes = {};
var ActualNode = {};
var NodeTypes = [];
var NodeTriggersTypes = [];

const Pages = Object.freeze({
    "none":0,
    "dashboard": 1,
    "wallets": 2, 
    "accounts": 3,
    "nodes": 4,
    "users": 5
});

var ActualPage = Pages.dashboard;

var bootstrapstudio = false;

var ApiUrl = 'http://localhost:8080/api'; //placeholder
var UserRights = 0;  //placeholder to be filled by login
var AvailableRights = {};