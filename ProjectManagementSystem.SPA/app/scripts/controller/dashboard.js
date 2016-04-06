(function () {
    'use strict';

    angular
        .module('app')
        .controller('dashboard', dashboard);

    dashboard.$inject = ['$location']; 

    function dashboard($location) {
        /* jshint validthis:true */
        var vm = this;
        vm.title = 'dashboard';

        activate();

        function activate() { }
    }
})();
