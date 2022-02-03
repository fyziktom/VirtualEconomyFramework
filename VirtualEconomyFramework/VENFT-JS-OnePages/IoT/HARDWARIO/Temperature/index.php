<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, shrink-to-fit=no">
    <title>HARDWARIO IoT</title>

    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@300;400&display=swap" rel="stylesheet">

    <script src="https://code.jquery.com/jquery-3.6.0.min.js" integrity="sha256-/xUj+3OJU5yExlq6GSYGSHk7tPXikynS7ogEvDej/m4=" crossorigin="anonymous"></script>
    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.min.js" integrity="sha256-VazP97ZCwtekAsvgPBSUwPFKdrwD3unUfSGVYrahUqU=" crossorigin="anonymous"></script>

    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" integrity="sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3" crossorigin="anonymous">
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.min.js"></script>

    <link rel="stylesheet" href="https://cdn.datatables.net/1.11.3/css/dataTables.bootstrap5.min.css">
    <link rel="stylesheet" href="css/style.css">

    <script src="https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.29.1/moment.min.js" integrity="sha512-qTXRIMyZIFb8iQcfjXWCO8+M5Tbc38Qi5WzdPOYZHIlZpzBHG3L3by84BBBOiRGiEb7KKtAOAs5qYdUiZiQNNQ==" crossorigin="anonymous" referrerpolicy="no-referrer"></script>

    <script src="https://cdn.datatables.net/1.11.3/js/jquery.dataTables.min.js"></script>
    <script src="https://cdn.datatables.net/1.11.3/js/dataTables.bootstrap5.min.js"></script>

    <script src="https://code.highcharts.com/stock/highstock.js"></script>
    <script src="https://code.highcharts.com/stock/modules/data.js"></script>
    <script src="https://code.highcharts.com/stock/modules/exporting.js"></script>
    <script src="https://code.highcharts.com/stock/modules/export-data.js"></script>

    <script src="https://code.highcharts.com/modules/variable-pie.js"></script>
    <script src="https://code.highcharts.com/modules/accessibility.js"></script>


    <script src="js/charts.js"></script>
    <script src="js/main.js"></script>


</head>

<body>
<nav class="navbar navbar-expand-lg navbar-dark bg-primary px-4">
    <div class="container-fluid">
        <a class="navbar-brand" href="#">
            <img src="img/hwio.png" class="d-inline-block align-middle float-start" style="margin-right: 12px;">
            <div class="float-start" style="margin-top: 13px;font-size: 23px;font-weight: bold; letter-spacing: 2px;">IoT NFT information - current temperature:
                <span id="current-temperature"></span></div>
        </a>

        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarToggler" aria-controls="navbarToggler" aria-expanded="false" aria-label="Toggle navigation">
            <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarToggler">
            <ul class="navbar-nav ms-auto mb-2 mb-lg-0">
                <li class="nav-item my-4 my-lg-0 me-5">
                    <div style="height: 40px;" class="me-2 p-2 float-start">
                        <a href="#" class="position-relative">
                            <img src="img/bell.svg" alt="Notification" style="width: 20px;">
                            <span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger" id="notifikace">0</span>
                        </a>
                    </div>
                </li>

                <li class="nav-item position-relative text-white fw-bolder">
                    <div style="width: 40px; height: 40px;" class="rounded-circle bg-white me-2 p-2 float-start">
                        <img src="img/profile.svg" alt="Notification">
                    </div>
                    <div class="float-start" style="line-height: 40px;">Test user</div>


                </li>

            </ul>
        </div>

    </div>
    <div class="progress justify-content-end position-absolute" style="bottom: 0; left: 0; right: 0; background: transparent;">
        <div class="progress-bar progress-bar-striped bg-info progress-bar-animated" role="progressbar" id="progress" style="width: 100%" aria-valuenow="50" aria-valuemin="0" aria-valuemax="100"></div>
    </div>
</nav>


<div class="container-fluid p-3 pb-0">


    <div class="row mx-auto mb-3 proj-progress-card shadow rounded p-3">
        <div class="col-xl-3 col-md-6">
            <h6 style="margin-bottom: 5px;">Sensors count</h6>
            <h5>1 / 1 <span class="ms-2 green-text">100%</span>
                <span class="float-end" style="font-size: 1rem; margin-top: 3px;">(active / total)</span></h5>

            <div class="progress">
                <div class="progress-bar bg-c-red" style="width:100%"></div>
            </div>
        </div>

        <div class="col-xl-3 col-md-6 position-relative">
            <h6 class="float-start" style="margin-bottom: 5px;">Incoming data count</h6>
            <button class="btn mini-btn float-end position-absolute" style="right: 0; display: none;">Load more</button>

            <div style="clear: both;"></div>

            <h5><span id="incoming-data-count">0</span> / <span id="total-data-count">0</span>
                <span id="incoming-data-percent" class="ms-2 green-text">0%</span>
                <span class="float-end" style="font-size: 1rem; margin-top: 3px;">(loaded / total)</span></h5>
            <div class="progress">
                <div class="progress-bar bg-c-blue" id="incoming-data-progressbar" style="width:0%"></div>
            </div>
        </div>

        <div class="col-xl-3 col-md-6">
            <h6 style="margin-bottom: 5px;">Temperatures < 22 °C</h6>
            <h5><span id="valid-data-percentage">0</span>% <span class="ms-2 green-text"> + 0%</span></h5>

            <div class="progress">
                <div class="progress-bar bg-c-green" id="valid-data-progressbar" style="width:100%"></div>
            </div>
        </div>

        <div class="col-xl-3 col-md-6">
            <h6 style="margin-bottom: 5px;">Next temperature prediction</h6>
            <h5><span id="prediction">0</span>
                <span class="ms-2 red-text float-end" style="font-size: 1.5rem;" id="prediction-timeout"></span></h5>

            <div class="progress">
                <div class="progress-bar bg-c-yellow" id="prediction-progressbar" style="width:0%"></div>
            </div>
        </div>
    </div>

    <div class="row mb-3 gx-3">
        <div class="col-md-8">
            <div class="shadow rounded p-3">
                <h6 class="mini-line">Temperature</h6>

                <div id="temperature-chart" style="height: 370px;"></div>
            </div>
        </div>

        <div class="col-md-4 d-flex flex-column">

            <div id="temperature-summary" class="h-100">
                <div class="h-100 ">
                    <div class="row mb-3 temperature-row">
                        <div class="col">
                            <div class="shadow rounded p-3 h-100 d-flex align-items-center justify-content-between">
                                <div class="temperature-row-in">
                                    <h6>Maximum temperature</h6>
                                    <h5 id="max-temperature" class="red-text fw-bold"></h5>
                                    <p class="small" style="height: 20px;" id="summary-time-max"></p>
                                </div>
                                <div class="btn-temperature bg-c-red">
                                    <img src="img/temp-max.svg" alt="">
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="row mb-3 temperature-row">
                        <div class="col">
                            <div class="shadow rounded p-3 h-100 d-flex align-items-center justify-content-between">
                                <div class="temperature-row-in">
                                    <h6>Average temperature</h6>
                                    <h5 id="avg-temperature" class="yellow-text fw-bold"></h5>
                                    <p class="small" style="height: 20px;"></p>

                                </div>
                                <div class="btn-temperature bg-c-yellow">
                                    <img src="img/temp-avg.svg" alt="">
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="row temperature-row">
                        <div class="col">
                            <div class="shadow rounded p-3 h-100 d-flex align-items-center justify-content-between">
                                <div class="temperature-row-in">
                                    <h6>Minimum temperature</h6>
                                    <h5 id="min-temperature" class="blue-text fw-bold"></h5>
                                    <p class="small" style="height: 20px;" id="summary-time-min"></p>
                                </div>
                                <div class="btn-temperature bg-c-blue">
                                    <img src="img/temp-min.svg" alt="">
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>


            <div id="temperature-detail" class="h-100" style="display: none;">
                <div class="h-100">
                    <div class="row temperature-row mb-3">
                        <div class="col">
                            <div class="shadow rounded p-3 h-100 d-flex align-items-center justify-content-between">
                                <div class="temperature-row-in">
                                    <h6>Selected temperature</h6>
                                    <h5 id="selected-temperature" class="green-text fw-bold"></h5>
                                    <p class="small" id="selected-date"></p>
                                </div>
                                <div class="btn-temperature bg-c-green">
                                    <img src="img/temp-avg.svg" alt="">
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="row" style="height: 280px;">
                        <div class="col">
                            <div class="shadow rounded p-3 h-100">

                                <div class="row h-100">
                                    <div class="col-md-7" style="line-height: 21px;">
                                        <p>Device name: <span style="font-style: italic;">CHESTER Track Demo</span>
                                        </p>
                                        <p>Device ID:
                                            <span style="font-style: italic;">5f7722790305b500185b2847</span>

                                        <p>Vendor:
                                            <span style="font-style: italic;"><a class="no-link" target="_blank" href="https://www.hardwario.com">hardwario.com</a></span>
                                        </p>
                                        </p>

                                        <div>
                                            <p style="float: left;">Position address:<span style="font-style: italic;"><br>Dr. M. Horákové 413/9,
                                                <br>460 01 Liberec</span>
                                            </p>

                                            <div style="width: 100px; height: auto; margin-left: 25px; border: 1px solid #dee2e6; margin-left: calc(50% - 115px);" class="rounded-circle bg-white me-2 p-2 float-start">
                                                <a target="_blank" href="https://www.hardwario.com"><img style="width: 62px;margin-left: 12px;" src="img/hw-mini-logo.png"></a>
                                            </div>
                                        </div>


                                        <div style="clear: both;"></div>
                                        <p class="mb-0">Lat: <span style="font-style: italic;">50.7633629</span><br>Lon:
                                            <span style="font-style: italic;">15.0550699</span></p>

                                    </div>
                                    <div class="col-md-5">
                                        <a href="https://goo.gl/maps/kNdFAP6QJTEyaNMe8" target="_blank">
                                            <img src="img/mapa.jpg" class="img-thumbnail" style="height: 230px; float: right;" alt="">
                                        </a>
                                    </div>
                                </div>
                            </div>

                        </div>
                    </div>
                </div>
            </div>

        </div>
    </div>


    <div class="row mb-3 flex-row gx-3">
        <div class="col-xl-4 col-md-6">
            <div class="shadow rounded p-3" style="height: 350px;">
                <h6 class="mini-line">Statistics</h6>

                <div id="statistics" style="height: 275px;"></div>
            </div>
        </div>

        <div class="col-xl-8 col-md-6  flex-grow-1">
            <div class="shadow rounded p-3 h-100" style="height: 350px !important; overflow-y: auto;">
                <table class="table table-striped nowrap dataTable" role="grid" id="temperature-table">
                    <thead>
                    <tr>
                        <th class="dt-head-left">Name</th>
                        <th>Date time</th>
                        <th>Temperature °C</th>
                        <th>NFT Link</th>
                    </tr>
                    </thead>
                    <tbody>

                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <div class="row mx-auto gx-3">
        <div class="col" style="padding: 0; margin: 0;">
            <div id="smallchart1" class="shadow rounded mini-chart"></div>
            <div id="smallchart2" class="shadow rounded mini-chart"></div>
            <div id="smallchart3" class="shadow rounded mini-chart"></div>
            <div id="smallchart4" class="shadow rounded mini-chart"></div>
            <div id="smallchart5" class="shadow rounded mini-chart"></div>
            <div id="smallchart6" class="shadow rounded mini-chart"></div>
            <div id="smallchart7" class="shadow rounded mini-chart"></div>
        </div>
    </div>

</div>


<div class="alert mb-0 footer" style="text-align: center; border-radius: 0;">
    <a class="no-link" href="https://ve-nft.com/" target="_blank">VirtualEconomyFramework &copy; 2022</a>
</div>

</body>

</html>