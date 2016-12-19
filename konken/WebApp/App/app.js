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
            $http.get("http://konkenapi.azurewebsites.net/getleague?fplLeagueId=414219")
                .then(
                    function (response) {
                        vm.league = response.data;
                    },
                    function (response) {
                        console.log(response)
                    });
        };
    }
}());

