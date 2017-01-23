(function () {
    "use strict";

    angular
        .module("konkenModule", ["ngRoute"])

        .config(["$routeProvider", function ($routeProvider) {
            $routeProvider
                .when("/standing", {
                    templateUrl: "AppViews/standing.html",
                    controller: "standingController",
                    controllerAs: "vm"
                })
                .when("/gameweek/:number?", {
                    templateUrl: "AppViews/gameweek.html",
                    controller: "gameweekController",
                    controllerAs: "vm"
                })
                .otherwise({
                    templateUrl: "AppViews/index.html",
                    controller: "indexController",
                    controllerAs: "vm"
                });
        }]);
}());

(function () {
    "use strict";

    angular
        .module("konkenModule")
        .controller("indexController", ["$scope", "$http", "environmentService", indexController]);

    function indexController($scope, $http, envSvc) {
        var vm = this;
        vm.league = undefined;
        $scope.Rounds = undefined;
        $scope.Round = undefined;

        load();

        function load() {
            $scope.loading = true;

            var url = envSvc.get("url") + "getstanding?fplLeagueId=414219";

            $http.get(url)
                .then(
                    function (responseRound) {
                        vm.league = responseRound.data;
                        $scope.loading = false;
                    },
                    function (responseRoundError) {
                        console.log(responseRoundError);
                        $scope.error = true;
                    });
        };
    }
}());

(function () {
    "use strict";

    angular
        .module("konkenModule")
        .controller("gameweekController", ["$scope", "$http", "environmentService", gameweekController]);

    function gameweekController($scope, $http, envSvc) {
        var vm = this;
        vm.gameweek = undefined;
        $scope.Rounds = undefined;
        $scope.Round = undefined;

        //if ($routeParams.number) {
        //    $scope.Round = $routeParams.number;
        //    load();
        //}

        loadRounds();

        function loadRounds() {
            $scope.loading = true;

            $http.get(envSvc.get("url") + "getrounds?fplLeagueId=414219")
                .then(
                    function (responseRounds) {
                        $scope.Rounds = responseRounds.data;

                        if (!$scope.Round) {
                            $scope.Round = Array.max($scope.Rounds);
                            load();
                        }

                        $scope.loading = false;
                    },
                    function (responseRoundsError) {
                        console.log(responseRoundsError);
                        $scope.error = true;
                    });
        };

        function load() {
            $scope.loading = true;

            if ($scope.Round) {
                var url = envSvc.get("url") + "getgameweek?fplLeagueId=414219&round=" + $scope.Round;

                $http.get(url)
                    .then(
                        function (responseRound) {
                            vm.gameweek = responseRound.data;
                            $scope.loading = false;
                        },
                        function (responseRoundError) {
                            console.log(responseRoundError);
                            $scope.error = true;
                        });
            } else {
                $scope.loading = false;
            }
        };

        $scope.ChangeRound = function (round) {
            $scope.Round = round;
            load();
        };

        Array.max = function (array) {
            return Math.max.apply(Math, array);
        };
    }
}());

(function () {
    "use strict";

    angular
        .module("konkenModule")
        .controller("standingController", ["$scope", "$http", "environmentService", standingController]);

    function standingController($scope, $http, envSvc) {
        var vm = this;
        vm.league = undefined;
        $scope.Rounds = undefined;
        $scope.Round = undefined;

        load();

        function load() {
            $scope.loading = true;

            var url = envSvc.get("url") + "getstanding?fplLeagueId=414219";

            if ($scope.Round)
                url += "&round=" + $scope.Round;

            $http.get(envSvc.get("url") + "getrounds?fplLeagueId=414219")
                .then(
                    function (response) {
                        $scope.Rounds = response.data;

                        $http.get(url)
                            .then(
                                function (response) {
                                    vm.league = response.data;

                                    angular.forEach(vm.league.PlayerStandings, function (o) {
                                        o.Chips = Object.assign({}, o.Chips);
                                    });

                                    if ($scope.Round)
                                        $scope.Round = $scope.Round;
                                    else
                                        $scope.Round = Array.max($scope.Rounds);

                                    $scope.loading = false;
                                },
                                function (response) {
                                    console.log(response);
                                    $scope.error = true;
                                });
                    },
                    function (response) {
                        console.log(response);
                        $scope.error = true;
                    });
        };

        $scope.orderByProp = "Points";
        $scope.orderByReverse = true;

        $scope.OrderBy = function (prop, reverse) {
            //orderByProp = 'Transfers'; orderByReverse = !orderByReverse
            if ($scope.orderByProp === prop) {
                $scope.orderByReverse = !$scope.orderByReverse;
            } else {
                $scope.orderByProp = prop;
                $scope.orderByReverse = (reverse) ? false : true;
            }
        }

        $scope.ChangeRound = function (round) {
            console.log(round)
            $scope.Round = round;
            load();
        };

        Array.max = function (array) {
            return Math.max.apply(Math, array);
        };
    }
}());

(function () {
    "use strict";

    angular
        .module("konkenModule")
        .service("environmentService", ["$location", environmentService]);

    function environmentService($location) {
        var environments = {
            live: {
                host: "konken.azurewebsites.net",
                config: {
                    url: "http://konkenapi.azurewebsites.net/"
                }
            },
            local: {
                host: "konken.app",
                config: {
                    url: "http://konken.api/"
                }
            }
        },
        environment;

        return {
            getEnvironment: function () {
                var host = $location.host();

                if (environment) {
                    return environment;
                }

                for (var e in environments) {
                    if (typeof environments[e].host && environments[e].host === host) {
                        environment = e;
                        return environment;
                    }
                }

                return null;
            },
            get: function (property) {
                return environments[this.getEnvironment()].config[property];
            }
        }
    }
}());

//(function () {
//    "use strict";

//    angular
//        .module("konkenModule")
//        .filter("range", function () {
//            return function (input, total) {
//                total = parseInt(total);

//                for (var i = 0; i < total; i++) {
//                    input.push(i);
//                }

//                return input;
//            };
//        });
//}());

