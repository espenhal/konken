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
                .otherwise({
                    templateUrl: "AppViews/standing.html",
                    controller: "standingController",
                    controllerAs: "vm"
                });
        }]);
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

(function () {
    "use strict";

    angular
        .module("konkenModule")
        .controller("standingController", ["$scope", "$http", "environmentService", standingController]);

    function standingController($scope, $http, envSvc) {
        var vm = this;
        vm.league = undefined;

        load();

        function load() {
            $scope.loading = true;

            $http.get(envSvc.get("url") + "getstanding?fplLeagueId=414219&round=38")
                .then(
                    function (response) {
                        vm.league = response.data;

                        angular.forEach(vm.league.PlayerStandings, function (o) {
                            o.Chips = Object.assign({}, o.Chips);
                        });

                        $scope.loading = false;
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
    }
}());

