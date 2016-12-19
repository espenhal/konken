(function () {
    "use strict";

    angular.module("konkenModule", []);
}());

(function () {
    "use strict";

    angular
        .module("konkenModule")
        .controller("konkenController", ["$scope", "$http", konkenController]);

    function konkenController($scope, $http) {
        load();

        function load() {
            $http.get("http://konkenapi.azurewebsites.net/getleague?fplLeagueId=414219")
                .then(
                    function (response) {
                        console.log(response)
                    },
                    function (response) {
                        console.log(response)
                    });
        };

        //$scope.load = function () {
        //    $http.get("http://konkenapi.azurewebsites.net/getleague?fplLeagueId=414219")
        //        .then(
        //            function (response) {
        //                console.log(response)
        //            },
        //            function (response) {
        //                console.log(response)
        //            });
        //};
    }
}());

