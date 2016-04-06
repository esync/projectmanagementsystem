(function () {
    'use strict';

    angular
        .module('app')
        .controller('myHome', myHome);

    myHome.$inject = ['$scope']; 

    function myHome($scope) {
        $scope.title = 'myHome';

        activate();

        function activate() { }
    }
})();
