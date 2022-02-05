let allTemperatures = [];

//Runs functions sequentially after the page is loaded
$(function () {
    SetLoadDataProgressbar(false);

    allTemperatures = GetAllTemperatures("NWpT6Wiri9ZAsjVSH8m7eX85Nthqa2J8aY");
    SortByDateTime();
    DisplayStatistics();

    SetTemperatureTable();

    PrepareDataToCharts();
    SetTemperatureChart();
    SetSmallCharts();

    SortByCategories();
    SetStatsChart();

    SetNotification();

});


let progressbarStatus = 100;

//Sets the main progressbar during the first page load
function SetLoadDataProgressbar(complete) {
    let progressbar = $('#progress');

    if(complete)
    {
        progressbarStatus = 0;
        progressbar.width(progressbarStatus + "%");
    }
    else if (progressbarStatus >= 10) {
        progressbarStatus -= 5;

        progressbar.width(progressbarStatus + "%");
        setTimeout(function () {
            SetLoadDataProgressbar()
        }, 50);

    }
}

//Gets all data for the preset address
function GetAllTemperatures(address) {
    let ajaxData;

    $.ajax({
        url: 'https://venftappserver.azurewebsites.net/iotapi/GetTemperatures',
        data: JSON.stringify({
            address: address,
            start: 0,
            loadmax: 999999
        }),
        contentType: 'application/json;charset=utf-8',
        method: 'POST',
        dataType: 'json',   // type of response data
        timeout: 1000000,     // timeout milliseconds
        async: false,

        success: function (data, status, xhr) {   // success callback function
            ajaxData = data.data;
            SetLoadDataProgressbar(true);
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback
            console.log('Error: "' + errorMessage + '"');
        }
    });

    return ajaxData;
}

//Sorts the preset data and sets its timestamp
function SortByDateTime() {
    for (let i = 0; i < allTemperatures.length; i++)
        allTemperatures[i].timestamp = new Date(allTemperatures[i].datetime).getTime();

    allTemperatures = allTemperatures.sort(function (a, b) {
        return a.timestamp - b.timestamp
    });
}


let mainChartData = [];
let smallChartsData = [];

//Sets and sorts the data for the main chart and small charts on the page
function PrepareDataToCharts() {
    let lastDate = null;

    let reverseTemperaturesArray = allTemperatures.reverse();

    let daysInArrayCounter = 0;

    for (let i = 0; i < reverseTemperaturesArray.length; i++) {
        let dataRow = reverseTemperaturesArray[i];

        mainChartData.push([dataRow.timestamp, dataRow.temperature]);

        let currentDate = moment(dataRow.timestamp).format("YYYY-MM-DD");
        if (currentDate == lastDate) {
            smallChartsData[currentDate].push([dataRow.timestamp, dataRow.temperature]);
        } else {
            daysInArrayCounter++;

            //max 7 mini charts
            if (daysInArrayCounter > 7) {
                break;
            }
            lastDate = currentDate;
            smallChartsData[currentDate] = [];
            smallChartsData[currentDate].push([dataRow.timestamp, dataRow.temperature]);
        }
    }

    mainChartData.reverse();

    for (let date in smallChartsData) {
        smallChartsData[date].reverse();
    }
}

//Sets statistics at the top of the page
function DisplayStatistics() {
    $('#incoming-data-count').text(allTemperatures.length);
    $('#total-data-count').text(allTemperatures.length);

    let percentageAvalData = (allTemperatures.length / allTemperatures.length * 100).toFixed(0);
    $('#incoming-data-percent').text(percentageAvalData + " %");
    $('#incoming-data-progressbar').width(percentageAvalData + "%");


    ValidDataCount();

    NextTemperaturePrediction();
}

let temperatureTable;

//Sets up a temperature table and populates it with preset data
function SetTemperatureTable() {

    temperatureTable = $('#temperature-table');

    for (let i = 0; i < allTemperatures.length; i++) {
        AddToTable(allTemperatures[i]);
    }

    temperatureTable.DataTable({
        "searching": false,
        paging: false,
        rowReorder: true,
        "order": [[1, 'desc']],
        "info": false,
        "scrollY": "250px",
        "scrollCollapse": true,

        columnDefs: [
            {
                targets: 0,
                className: 'dt-body-left'
            }]
    });
}

//Adds a new row to the summary data table
function AddToTable(data) {
    let dateTime = moment(data.datetime).format("YYYY-MM-DD HH:mm:ss");

    temperatureTable.append("<tr>" +
        "<td>" + data.description + "</td>" +
        "<td>" + dateTime + "</td>" +
        "<td>" + data.temperature + "</td>" +
        "<td><a href='https://explorer.nebl.io/tx/" + data.txid + "' target='_blank'>Explorer</a></td>" +
        "</tr>");
}

//Sets notifications (bell) => number of new data since last visit
function SetNotification() {
    let previousNftCount = isNaN(GetCookie('nftcount')) ? 0 : GetCookie('nftcount');
    let notificationCount = allTemperatures.length - previousNftCount;

    $('#notifikace').text(notificationCount);

    SetCookie('nftcount', allTemperatures.length, 100);
}

let catValues = [];

//Sorts data into a pie chart
function SortByCategories() {
    catValues[0] = 0;
    catValues[1] = 0;
    catValues[2] = 0;
    catValues[3] = 0;

    for (let i = 0; i < allTemperatures.length; i++) {
        let temperature = allTemperatures[i].temperature;

        if (temperature <= 21.5)
            catValues[0]++;
        else if (temperature > 21.5 && temperature <= 22)
            catValues[1]++;
        else if (temperature > 22 && temperature <= 22.5)
            catValues[2]++;
        else
            catValues[3]++;

    }
}

//Counts the number of valid data out of the total number of (>= 22°C)
function ValidDataCount() {
    let count = 0;

    for (let i = 0; i < allTemperatures.length; i++) {
        if (allTemperatures[i].temperature < 22)
            count++;

        let percent = Math.round(100 - (count / allTemperatures.length * 100));

        $('#valid-data-percentage').text(percent);

        $('#valid-data-progressbar').width(percent + "%");
    }
}

//Predicts the next value from the last 10 loaded values
function NextTemperaturePrediction() {
    let lastTen = [];

    if (allTemperatures.length < 11)
        return;

    for (let i = 1; i <= 11; i++) {
        lastTen.push(allTemperatures[allTemperatures.length - i].temperature);
    }

    let lastValue = lastTen[0];

    lastTen.reverse();

    let sumOfDiff = 0;

    for (let i = 1; i < lastTen.length; i++) {
        sumOfDiff += (lastTen[i] - lastTen[i - 1]);
    }

    let avgDiff = sumOfDiff / lastTen.length;

    let predictValue = (lastValue + avgDiff).toFixed(2);

    $('#prediction').text(predictValue + " °C");
    $('#current-temperature').text(lastValue + " °C");

    PredictionTimeout();
}

//Every second provides a countdown to the next prediction
function PredictionTimeout() {

    let currTime = new Date().getTime();
    let lastTime = allTemperatures[allTemperatures.length - 1].timestamp;

    let seconds = (currTime - lastTime) / 1000;

    let div = $('#prediction-timeout');
    let progressbar = $('#prediction-progressbar');

    //prisla pozdeji nez pred 15 min
    if (seconds > 15 * 60) {
        div.removeClass('green-text');
        div.addClass('red-text');
        div.text("-" + toHHMMSS(seconds - (15 * 60)));
        progressbar.width("0%");
    } else {
        let secondsLeft = (15 * 60) - seconds;

        div.addClass('green-text');
        div.removeClass('red-text');

        div.text(toHHMMSS(secondsLeft));

        let percentageLeft = secondsLeft / (15 * 60) * 100;
        progressbar.width(percentageLeft + "%");

    }
    setTimeout(function () {
        PredictionTimeout()
    }, 1000);
}

//Converts seconds to readable time (Hours:minutes:seconds)
let toHHMMSS = (secs) => {
    let sec_num = parseInt(secs, 10)
    let hours = Math.floor(sec_num / 3600)
    let minutes = Math.floor(sec_num / 60) % 60
    let seconds = sec_num % 60

    return [hours, minutes, seconds]
        .map(v => v < 10 ? "0" + v : v)
        .filter((v, i) => v !== "00" || i > 0)
        .join(":")
}

//Sets a cookie with the current number of notifications
function SetCookie(name, value, days) {
    let expires = "";
    if (days) {
        let date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}


//Gets a cookie with the number of notifications stored
function GetCookie(name) {
    let nameEQ = name + "=";
    let ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}