(function () {
    'use strict';

    angular.module('app', [
        // Angular modules 
        'ui.router',
        'ngAnimate',
        'ngCookies',
        'ngMessages',
        'ngResource',
        'ngSanitize'

        // Custom modules 

        // 3rd Party Modules
        
    ])
    .config(function ($stateProvider, $urlRouterProvider) {

        $urlRouterProvider.otherwise("/login");

        $stateProvider

          .state('login', {
              url: '/login',
              templateUrl: '../views/login/authenticate.html',
              controller: 'authenticate'
          })

          .state('dashboard', {
              url: '/dashboard',
              templateUrl: 'views/dashboard.html',
              controller: 'dashboard'
          })

          

    });
})();