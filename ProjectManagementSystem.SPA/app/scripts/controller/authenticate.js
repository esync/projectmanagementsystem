(function () {
    'use strict';

    angular
        .module('app')
        .controller('authenticate', authenticate);

    authenticate.$inject = ['$state, $scope'];

    function authenticate($state, $scope) {
        /* jshint validthis:true */
        var vm = this;

        $scope.login = function () {
            alert('hit login controller');
            $state.go('dashboard');
        }

        

        vm.title = 'authenticate';

        activate();

        function activate() { }
    }
})();
