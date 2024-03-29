﻿(function () {

    var app = angular.module("app");

    app.directive('onFinishRender', function ($timeout) {
        return {
            restrict: 'A',
            link: function (scope, element, attr) {
                if (scope.$last === true) {
                    $timeout(function () {
                        scope.$emit(attr.onFinishRender);
                    });
                }
            }
        }
    });

    app.controller('octopusController', function ($scope, $http) {
        var projects = []; // global(ish) because its used in makeTimeLine and project list

        $scope.$on('finished', function (ngRepeatFinishedEvent) {
            $('.project').on('click', function (e) {
                var projName = $(this)[0].getElementsByTagName("h4")[0].innerHTML;
                makeTimeLine(projName);
                $('#projectModal').modal('show');
            });
        });
        
        $http.get("api/Octo/projectGroups").then(function (response) { 
            $scope.projectGroups = response.data;
        });
        $http.get("api/Octo/environments").then(function (response) {
            $scope.environments = response.data;
        });
        $http.get("api/Octo/projects").then(function (response) {
            $scope.projects = response.data;
        });
        $http.get("api/Octo/lifecycles").then(function (response) {
            $scope.lifecycles = response.data;
        });
        $http.get("api/Octo/deploysByStatus?status=Executing").then(function (response) {
            $scope.liveDeploys = response.data;
        });

        function getProjectGroupName(projectGroupId)
        {
            $scope.projectGroups.forEach(function (group) {
                console.log(group);
            });
        }

        function getReleases(projectName) {
            var releases = [];
            $http.get("api/Octo/releasesByProject?project=" + projectName).then(function (response) {
                response.data.forEach(function (rel) {
                    releases.push(rel);
                });
            });
            return releases;
        };

        function makeTimeLine(pName) {
            var proj;                              // used to define the project JSON we're dealing with
            projects.forEach(function (p) {        // loop all projects to find the one
                if (p.name == pName) { proj = p; } // and set it to our reference
            });
            if (proj === undefined) { return; }    // dont do anything if we cant find the project
            var releases = getReleases(proj.id);
            var dates = [];
            $("#tl").html("<div id='timeline-embed'></div>");
            setTimeout(function () { // this 1 second wait allows the dateObj's to populate, without it the timeline doesn't generate
                                     // TODO: remove delay and sync wait for the dateObj's to load before making timeline to increase speed
                for (var x = 0; x < releases.length; x++) {
                    var r = releases[x];
                    var releaseURL = "\"" + r.webUrl.toString() + "\"";

                    var releaseDeployHtml = "<a class=\"open-in-octopus " + r.id + "-link c\" href=\"#\"><div class=\"btn btn-primary open-in-octopus\">Open in Octopus</div></a><div class=\"list-group\">";

                    //console.log(r.releaseDeploys);
                    for (var deplo in r.releaseDeploys) {
                        var depl = r.releaseDeploys[deplo];
                        console.log(depl);
                        var date = {
                            "startDate": depl.created,
                            "endDate": depl.completedTime,
                            "headline": depl.id,
                            "text": "<a class=\"open-in-octopus-deploy " + depl.id + "-link c\" href=\"#\"><div class=\"btn btn-primary open-in-octopus-deploy\">Open in Octopus</div></a>" + depl.errorMessage
                        };
                        dates.push(date);
                        releaseDeployHtml += "<a href=\"javascript:void(0)\" onclick=\"" +
                            "\" class=\"deploy-button list-group-item \" data-toggle=\"tooltip\" data-original-title=\"" + moment(depl.created).fromNow() +
                            "\">" +
                            "<h4 class=\"list-group-item-heading\">" + depl.id + "<small class=\"pull-right\">" + moment(depl.created).fromNow() + "</small></h4>" +
                            "<p class=\"list-group-item-text\">Duration: " + depl.duration + "</p></a>";
                    }
                    releaseDeployHtml += "</div>";
                    var infoAlert = "<div class=\"alert alert-info\"><i class=\"fa fa-info-circle\"></i> This release was created " +
                        moment(r.assembled).fromNow() + "</div>";
                    var date = {
                        "startDate": r.assembled,
                        "endDate": r.assembled,
                        "headline": r.version,
                        "text": (r.releasenotes === undefined || r.releasenotes === "" ? infoAlert + "No description" + releaseDeployHtml : infoAlert +
                            r.releasenotes + releaseDeployHtml)
                    };
                    dates.push(date);
                }
                var dataObj = {
                    "timeline":
                    {
                        "headline": "Progression timeline for " + pName,
                        "type": "default",
                        "text": "<p>A brief history of the projects releases</p>",
                        "date": dates
                    }
                };

                if (dataObj.timeline.date.length < 1) { $("#tl").html("No releases for this project yet!"); } else {
                    console.log(dataObj);
                    createStoryJS({
                        width: '100%',
                        height: '500',
                        source: dataObj,
                        embed_id: 'timeline-embed'
                    });
                    setTimeout(function () { // this 3 second wait is to allow the timeline to generate all the slides
                                             // without it, the links to octopus for deploys and releases wouldn't work.
                                             // Either figure out how to directly set the links href or wait just until
                                             // the link element exists to set its href
                        for (var z = 0; z < releases.length; z++) {
                            var r = releases[z];
                            for (var deplo in r.releaseDeploys)
                            {
                                var depl = r.releaseDeploys[deplo];
                                $("." + depl.id + "-link").attr("href", depl.webUrl);
                                $("." + depl.id + "-link").attr("target", "_blank");
                            }
                            $("." + r.id + "-link").attr("href", r.webUrl);
                            $("." + r.id + "-link").attr("target", "_blank");
                        }
                        
                    }, 3000);
                }
            }, 1000);
        };

        $http.get("api/Octo/projects").then(function (response) {
            $scope.projectList = response.data;
            
            response.data.forEach(function (p) {
                projects.push(p);
            });
        });

        $(document).ready(function () {
            $('.charts').hide();
            $('.octo-line-canvas').show();
            $('.octo-pie-canvas').hide();
            $('.octo-bar-canvas').hide();

            $('.btn-group .btn').mouseup(function (e) {
                setTimeout(function () {
                        document.getElementsByClassName("canvas").onclick =
                            function (e) { }; // clear out the graph onclick event
                        var btnId = $(".btn-group").find(".active").attr("id");
                        if (btnId == "opt1") {
                            $('.octo-line-canvas').show();
                            $('.octo-pie-canvas').hide();
                            $('.octo-bar-canvas').hide();
                        }
                        if (btnId == "opt2") {
                            $('.octo-line-canvas').hide();
                            $('.octo-pie-canvas').show();
                            $('.octo-bar-canvas').hide();
                        }
                        if (btnId == "opt3") {
                            $('.octo-line-canvas').hide();
                            $('.octo-pie-canvas').hide();
                            $('.octo-bar-canvas').show();
                        }
                    },
                    300);

            });
        });

        $http.get("api/Octo/deployEvents").then(function(response) {
            $scope.deployEvents = response.data;
            console.log($scope.deployEvents);

            var hr = new Date(); // the date and time right now
            var startTime = hr.getHours(); // the "start" time (really the time the graph ends at)
            $scope.times = []; // array of hours for the graphs labels


            var lastHour = 1; // used to determine if 12 o'clock is noon or midnight

            for (var hs = startTime - 23; hs <= startTime; hs++) {
                var s = (hs < 0 ? 24 + hs : (hs == 0 ? 12 : hs));
                if (lastHour == 11 || lastHour == 23) {
                    if (lastHour == 11) {
                        $scope.times.push("Noon");
                    }
                    if (lastHour == 23) {
                        $scope.times.push("Midnight");
                    }
                    lastHour = s;
                    continue;
                }
                $scope.times.push((s <= 11 ? s + "AM" : (s == 12 ? 12 : s % 12) + "PM"));
                lastHour = s;
            }

            function parseIsoLocal(s) {
                var b = s.split(/\D/);
                return new Date(b[0], b[1] - 1, b[2], b[3], b[4], b[5]);
            }

            // These 5 arrays hold information about deployments over the past 24 hours
            var failed = [];
            failed.length = 24;
            failed.fill(0);
            var succeeded = [];
            succeeded.length = 24;
            succeeded.fill(0);
            var queued = [];
            queued.length = 24;
            queued.fill(0);
            var started = [];
            started.length = 24;
            started.fill(0);
            if ($scope.allDeploys == undefined) {
                $scope.allDeploys = [];
            }
            $scope.allDeploys.length = 24;
            $scope.allDeploys.fill([]);

            // total 24 hour counts for each deploy type
            var failedCount = 0;
            var succeededCount = 0;
            var queuedCount = 0;
            var startedCount = 0;

            for (var index in $scope.deployEvents) {
                d = $scope.deployEvents[index];
                var timeString = moment(d.timeAndDate);
                var rightNow = moment();
                var hour = timeString.hour();
                var timeDiff = timeString.diff(rightNow, 'hours');
                hour = 23 - (timeDiff < 0 ? timeDiff * -1 : timeDiff);
                $scope.allDeploys[hour] = ($scope.allDeploys[hour] === undefined ? [] : $scope.allDeploys[hour]).concat(
                    {
                        "message": d.message,
                        "category": d.category,
                        "dateTime": timeString,
                        "environs": d.environs,
                        "webUrl": d.webUrl
                    }
                );
                if (d.category === "DeploymentFailed") {
                    failed[hour] = (failed[hour] !== undefined ? failed[hour] + 1 : 1)
                    failedCount++;
                };
                if (d.category === "DeploymentSucceeded") {
                    succeeded[hour] = (succeeded[hour] !== undefined ? succeeded[hour] + 1 : 1)
                    succeededCount++;
                };
                if (d.category === "DeploymentQueued") {
                    queued[hour] = (queued[hour] !== undefined ? queued[hour] + 1 : 1)
                    queuedCount++;
                };
                if (d.category === "DeploymentStarted") {
                    started[hour] = (started[hour] !== undefined ? started[hour] + 1 : 1)
                    startedCount++;
                };
            }

            function coloredListElement(c) {
                if (c == "DeploymentStarted") { return "list-group-item-info"; }
                if (c == "DeploymentQueued") { return "list-group-item-warning"; }
                if (c == "DeploymentFailed") { return "list-group-item-danger"; }
                if (c == "DeploymentSucceeded") { return "list-group-item-success"; }
            }

            $scope.octoChartData = [started, queued, succeeded, failed];

            $scope.octoPieData = [startedCount, queuedCount, succeededCount, failedCount];

            $scope.octoColors = [
                '#97bbed',
                '#fdb45c',
                '#46dfbd',
                '#f7464a'
            ];

            $scope.octoSeries = [
                "Deployments Started",
                "Deployments Queued",
                "Deployments Succeeded",
                "Deployments Failed"
            ];

            $scope.octoPieOptions = {
                legend: {
                    display: true
                }
            }
           
            $scope.openDeployModal = function(points, evt) {
                if (points[0] === undefined) { // user didn't click on a point
                    $(".octoModal").hide();
                    return;
                }
                $(".deployData").hide();
                var scope = angular.element($('#octoModal')).scope();

                while (scope.selectedDeploys.length > 0) {
                    scope.selectedDeploys.pop();
                }
                for (var index in $scope.allDeploys[points[0]._index]) {
                    var deployEvent = $scope.allDeploys[points[0]._index][index];
                    console.log(deployEvent.category);
                    scope.selectedDeploys.push({
                        environmentName: deployEvent.environs[0].name,
                        time: moment(deployEvent.dateTime).fromNow(),
                        colorClass: coloredListElement(deployEvent.category),
                        message: deployEvent.message,
                        date: moment(deployEvent.dateTime),
                        webUrl: deployEvent.webUrl,
                        click: function () {
                            scope.selectEnvironment(environmentName, date, webUrl);
                        }
                    });
                }
                scope.$apply();
                $("#octoModal").modal("show");
            };

            $scope.octoPieClick = function (points, evt) {
                if (points[0] === undefined) { // user didn't click on a point
                    $(".octoModal").hide();
                    return;
                }
                $(".deployData").hide();
                var scope = angular.element($('#octoModal')).scope();

                while (scope.selectedDeploys.length > 0) {
                    scope.selectedDeploys.pop();
                }
                var statusNum = points[0]._index;
                var status = (statusNum == 0 ? "DeploymentStarted" : (statusNum == 1 ? "DeploymentQueued" : (statusNum == 2 ? "DeploymentSucceeded" : (statusNum == 3 ? "DeploymentFailed" : "Unrecognized"))));
                for (var index in $scope.allDeploys) {
                    for (var jindex in $scope.allDeploys[index]) {
                        var deployEvent = $scope.allDeploys[index][jindex];
                        var datetime = moment(deployEvent.dateTime);
                        var fromnow = datetime.fromNow();
                        if (deployEvent.category === status) {
                            scope.selectedDeploys.push({
                                environmentName: deployEvent.environs[0].name,
                                time: fromnow,
                                colorClass: coloredListElement(status),
                                message: deployEvent.message,
                                date: datetime,
                                webUrl: deployEvent.webUrl,
                                click: function() {
                                    scope.selectEnvironment(environmentName, date, webUrl);
                                }
                            });
                        }
                    }
                }
                scope.$apply();
                $("#octoModal").modal("show");
            };

            $scope.octoChartOptions = {
                responsive: true,
                maintainAspectRatio: false,
                    legend: {
                        display: true
                    },
                    scales: {
                    yAxes: [{
                            ticks: {
                                beginAtZero: true,
                                stepSize: 1
                            }
                        }],
                        xAxes: [{
                        ticks: {
                            stepSize: 1,
                            autoSkip: false
                        }
                    }]
                    }
            }

            $(document).ready(function () {
                $('.graph-loading').hide();
                $('.charts').show();
            });
        });
    });

    app.controller('octopusModalController', function ($scope, $http) {
        $scope.selectedDeploys = [];
        $scope.webUrl = "/#/";
        $scope.date = "now";
        $scope.eventClick = function (environmentName, date, webUrl) {
            $scope.date = date;
            $scope.webUrl = webUrl;
            var octoScope = angular.element($('.octopus-column')).scope();
            for (var index in octoScope.environments) {
                var environment = octoScope.environments[index];
                if (environment.name === environmentName) {
                    if (environment.description == undefined) {
                        environment.description = "No description";
                    }
                    $scope.selectedEnvironment = environment;
                    $('.deployData').show();
                    return;
                }
            }
        };
    });

    app.controller('octopusProjectModalController', function ($scope, $http) {

    });
}());