

var graphLabels = [];
var graphValues = [];
function getSortedTemperatures(tableId) {
    graphLabels = [];
    graphValues = [];
    var table = document.getElementById(tableId);
    var rows = table.rows;
    for (i = 0; i < (rows.length - 1); i++) {

        var time = rows[i].getElementsByTagName("TD")[1];
        var temperature = rows[i + 1].getElementsByTagName("TD")[2];
        // Check if the two rows should switch place:
        if (time != undefined && temperature != undefined) {
            var tms = time.innerHTML.toLowerCase();
            var tmps = temperature.innerHTML.toLowerCase();
            graphLabels.push(tms);
            graphValues.push(tmps);
        }
    }
}

function loadDataToTheChart() {
    getSortedTemperatures('temperatures');

    var xArray = graphLabels.reverse();
    var yArray = graphValues.reverse();

    // Define Data
    var data = [{
        x: xArray,
        y: yArray,
        mode:"lines"
    }];

    // Define Layout
    var layout = {
    xaxis: {title: "Time"},
    yaxis: {title: "Temperature in °C"},  
    title: "Sensor Temperature"
    };

    // Display using Plotly
    Plotly.newPlot("temperaturePlot", data, layout);
}
