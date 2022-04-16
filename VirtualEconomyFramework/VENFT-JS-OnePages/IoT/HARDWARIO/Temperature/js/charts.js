const timezone = new Date().getTimezoneOffset();

//Sets the main top chart
function SetTemperatureChart() {
    Highcharts.stockChart('temperature-chart', {
        time: {
            timezoneOffset: timezone
        },

        navigator: {
            top: 0,
            maskFill: 'rgba(16, 16, 16, 0.5)',
            outlineWidth: 0,
            maskFill: 'rgba(0, 0, 0, 0.2)',
            handles: {
                backgroundColor: '#eee',
                borderColor: '#777'
            },
            series: {
                type: 'areaspline',
                color: '#bbb',
                fillOpacity: 0.3,
                shadow: false
            }
        },
        rangeSelector: {
            selected: 4,
            inputEnabled: false,
            buttonTheme: {
                visibility: 'hidden'
            },
            labelStyle: {
                visibility: 'hidden'
            }
        },
        chart: {
            type: 'line',
            zoomType: 'x',
            marginTop: 0,
            style: {
                fontFamily: 'Open Sans'
            },
            events: {
                load: function () {
                    let chartInstance = this;
                    let xAxis = chartInstance.xAxis[0];
                    let xAxExtremes = xAxis.getExtremes();
                    TemperaturesSummary(xAxExtremes.dataMin, xAxExtremes.dataMax);
                }
            }
        },

        title: {
            text: ''
        },


        xAxis: {
            type: 'datetime',

            events: {
                setExtremes: function (event) {
                    TemperaturesSummary(event.min, event.max);
                }
            },
            crosshair: false

        },

        yAxis: {
            min: 20,
            max: 25,
            tickInterval: 1,
        },

        plotOptions: {
            series: {
                fillOpacity: 0.3,
                lineWidth: 3,
                shadow: {
                    color: 'black',
                    offsetX: 2,
                    offsetY: 2,
                    opacity: 0.1,
                    width: 4
                }
            }
        },
        tooltip: {
            xDateFormat: '%Y-%m-%d %H:%M:%S',
            split: false,
            shared: true
        },
        series: [{
            name: 'Temperature',
            data: mainChartData,
            type: 'area',
            tooltip: {
                valueDecimals: 2
            },
            cursor: 'pointer',
            color: '#4099ff',

            point: {
                events: {
                    click: function () {
                        ChooseTemperature(moment(this.category).format("YYYY-MM-DD HH:mm:ss"), this.y);
                    }
                }
            }
        }],
    }, function (chart) {

        if (mainChartData.length >= 60)
            chart.xAxis[0].setExtremes(mainChartData[mainChartData.length - 60][0], mainChartData[mainChartData.length - 1][0]);
    });

}

//Function is called after clicking on a point on the main chart
function ChooseTemperature(date, temperature) {
    $('#temperature-summary').hide();
    $('#temperature-detail').show();

    $('#selected-temperature').text(temperature + " °C");
    $('#selected-date').text(date);
}

//Function is called after selecting a range of times on the main graph and calculates the maximum, average and minimum temperature.
function TemperaturesSummary(minTimestamp, maxTimestamp) {
    let summaryTemperatures = [];
    let summaryTimes = [];

    for (let i = 0; i < mainChartData.length; i++) {
        if (mainChartData[i][0] >= minTimestamp && mainChartData[i][0] <= maxTimestamp) {
            summaryTemperatures.push(mainChartData[i][1]);
            summaryTimes.push(mainChartData[i][0]);
        }
    }

    let minTemperature = Math.min(...summaryTemperatures);
    let maxTemperature = Math.max(...summaryTemperatures);

    let total = 0;
    for (let i = 0; i < summaryTemperatures.length; i++) {
        total += summaryTemperatures[i];
    }
    let avgTemperature = (total / summaryTemperatures.length).toFixed(2);

    $('#max-temperature').text(maxTemperature + " °C");
    $('#min-temperature').text(minTemperature + " °C");
    $('#avg-temperature').text(avgTemperature + " °C");


    let timeMin;
    let timeMax;

    for (let i = 0; i < summaryTemperatures.length; i++) {
        if (summaryTemperatures[i] == maxTemperature)
            timeMax = moment(summaryTimes[i]).format("YYYY-MM-DD HH:mm:ss");
        if (summaryTemperatures[i] == minTemperature)
            timeMin = moment(summaryTimes[i]).format("YYYY-MM-DD HH:mm:ss");
    }


    $('#summary-time-min').text(timeMin);
    $('#summary-time-max').text(timeMax);

    $('#temperature-summary').show();
    $('#temperature-detail').hide();
}

//Sets a statistics pie chart
function SetStatsChart() {
    Highcharts.chart('statistics', {
        chart: {
            type: 'pie',
            style: {
                fontFamily: 'Open Sans'
            }
        },

        exporting: {enabled: false},

        time: {
            timezoneOffset: timezone
        },
        title: {
            text: ''
        },
        yAxis: {
            title: {
                text: ''
            },
        },
        plotOptions: {
            pie: {
                shadow: false,
                colors: [
                    '#FF5370',
                    '#32aefb',
                    '#2ed8b6',
                    '#FFB64D',
                ],
                borderWidth: 3,
            }
        },
        tooltip: {
            formatter: function () {
                return '<b>' + this.point.name + '</b>: ' + this.y + 'x';
            }
        },
        series: [{
            name: '',
            data: [["< 21.5 °C", catValues[0]], ["21.5 - 22 °C", catValues[1]], ["22 - 22.5 °C", catValues[2]], ["> 22.5 °C", catValues[3]]],
            size: '100%',
            innerSize: '70%',
            showInLegend: true,
            dataLabels: {
                enabled: false
            }
        }]
    }, function (chart) {
        var textX = chart.plotLeft + (chart.series[0].center[0]);
        var textY = chart.plotTop + (chart.series[0].center[1]);


        chart.renderer.image(
            'img/temp-pie.png',
            textX - 25,
            textY - 25,
            50,
            50
        ).add();
    });
}

let minichartsColor = ['#FF5370', '#4099ff', '#2ed8b6', '#FFB64D', '#FF5370', '#4099ff', '#2ed8b6', '#FFB64D'];
let miniChartCounter = 1;

//Sets the bottom mini charts in the cycle
function SetSmallCharts() {

    let smallChartsDateArray = GetSmallChartsDatesArray();

    for (let i = 0; i < smallChartsDateArray.length; i++) {
        let date = smallChartsDateArray[i];

        let dayChartData = smallChartsData[date];

        Highcharts.chart('smallchart' + miniChartCounter, {

            chart: {
                type: 'areaspline',
                margin: 0, //removes all margin
                style: {
                    fontFamily: 'Open Sans'
                },
            },
            title: {
                text: '' //removes title of chart
            },
            exporting: {enabled: false},
            xAxis: {
                type: 'datetime',
                min: dayChartData[0][0],
                max: dayChartData[dayChartData.length - 1][0],
                crosshair: false, //hover effect of column
                lineWidth: 0, //removes axis line
                gridLineWidth: 0, //removes charts background line
                lineColor: 'transparent',
                minorTickLength: 0, //removes minor axis ticks
                tickLength: 0, //removes  axis ticks
                title: {
                    enabled: false
                },
                labels: {
                    enabled: false
                },
                minPadding: 0,
                maxPadding: 0
            },
            yAxis: {
                min: 19,
                max: 25,
                tickInterval: 1,
                title: {
                    text: ''
                },
                lineWidth: 0,
                gridLineWidth: 0,
                lineColor: 'transparent',
                minorTickLength: 0,
                tickLength: 0,
                labels: {
                    enabled: false
                },
                crosshair: false

            },
            tooltip: {
                enabled: false
            },
            credits: {
                enabled: false
            },
            legend: {
                enabled: false
            },
            plotOptions: {
                column: {
                    pointPadding: 0.2,
                    borderWidth: 0
                },
                series: {
                    lineWidth: 3,
                    fillOpacity: 0.3,
                    enableMouseTracking: false,
                    pointPlacement: 'on',
                    shadow: {
                        color: 'black',
                        offsetX: 2,
                        offsetY: 2,
                        opacity: 0.1,
                        width: 4
                    }
                },
                line: {
                    marker: {
                        enabled: false
                    }
                },
            },
            series: [{
                name: 'Temperature',
                data: dayChartData,
                color: minichartsColor[miniChartCounter - 1],
                marker: {
                    radius: 0
                }

            }]
        }, function (chart) { // on complete remder text

            let dayMax = 0;
            let dayMin = 1000;
            let dayAvg = 0;
            let daySum = 0;

            for (let i = 0; i < dayChartData.length; i++) {
                let temperature = dayChartData[i][1];

                if (temperature > dayMax)
                    dayMax = temperature;
                if (temperature < dayMin)
                    dayMin = temperature;

                daySum += temperature;
            }

            dayAvg = (daySum / dayChartData.length).toFixed(2);

            chart.renderer.text(' <span style="font-size:16px" class="mini-line">' + date + '</span>', 15, 35)
                .css({
                    color: '#333',
                }).add();

            chart.renderer.text(' <span style="font-size:14px">Max<br>' + dayMax + ' °C</span>', 20, 170)
                .css({
                    color: '#333',
                }).add();

            chart.renderer.text(' <span style="font-size:14px">Avg<br>' + dayAvg + ' °C</span>', 105, 170)
                .css({
                    color: '#333',
                }).add();

            chart.renderer.text(' <span style="font-size:14px">Min<br>' + dayMin + ' °C</span>', 195, 170)
                .css({
                    color: '#333',
                }).add();

        });

        miniChartCounter++;
    }
}

//Returns the date field for the bottom charts
function GetSmallChartsDatesArray()
{
    let smallChartsDateArray = [];
    for (let date in smallChartsData)
        smallChartsDateArray.push(date);

    smallChartsDateArray.reverse();

    return smallChartsDateArray;
}