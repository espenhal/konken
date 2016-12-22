(function () {
    "use strict";

    angular
        .module("konkenModule", ["ngRoute"])

        .config(['$routeProvider', function ($routeProvider) {
            $routeProvider
                .when('/standing', {
                    templateUrl: 'AppViews/standing.html',
                    controller: 'standingController',
                    controllerAs: 'vm'
                })
                .otherwise({
                    templateUrl: 'AppViews/standing.html',
                    controller: 'standingController',
                    controllerAs: 'vm'
                });
        }]);
}());

(function () {
    "use strict";

    angular
        .module("konkenModule")
        .controller("standingController", ["$scope", "$http", standingController]);

    function standingController($scope, $http) {
        var vm = this;
        vm.league = undefined;

        load();

        function load() {
            $scope.loading = true;

            //$http.get("//konkenapi.azurewebsites.net/getstanding?fplLeagueId=414219")
            $http.get("//konken.api/getstanding?fplLeagueId=414219")
                .then(
                    function (response) {
                        vm.league = response.data;
                        $scope.loading = false;
                    },
                    function (response) {
                        console.log(response)
                        $scope.error = true;
                    });
        };

        $scope.orderByProp = "Points";
        $scope.orderByReverse = true;

        $scope.OrderBy = function (prop, reverse) {
            console.log($scope.orderByProp + "===" + prop)

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

