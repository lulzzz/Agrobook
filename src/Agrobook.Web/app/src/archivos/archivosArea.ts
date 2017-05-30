﻿/// <;reference path="../_all.ts" />

module archivosArea {
    angular.module('archivosArea', ['ngRoute', 'ngMaterial', 'ngMdIcons'])
        .value('config', new common.config())
        .service('localStorageLite', common.localStorageLite)
        .service('toasterLite', common.toasterLite)
        .service('httpLite', common.httpLite)
        .service('loginService', login.loginService)
        .service('loginQueryService', login.loginQueryService)
        .service('archivosQueryService', archivosQueryService)
        .controller('userMenuWidgetController', common.userMenuWidgetController)
        .controller('sidenavController', sidenavController)
        .controller('prodSidenavController', prodSidenavController)
        .controller('toolbarHeaderController', toolbarHeaderController)
        .controller('mainContentController', mainContentController)
        .config(['$mdIconProvider', '$mdThemingProvider', '$httpProvider', '$routeProvider', (
            $mdIconProvider: angular.material.IIconProvider,
            $mdThemingProvider: angular.material.IThemingProvider,
            $httpProvider: angular.IHttpProvider,
            $routeProvider: angular.route.IRouteProvider
        ) => {
            // most from flat icon dot com
            $mdIconProvider
                .defaultIconSet('./assets/svg/avatars.svg', 128)
                .icon('menu', './assets/svg/menu.svg', 24)
                .icon('close', './assets/svg/close.svg');

            $mdThemingProvider.theme('default')
                .primaryPalette('green')
                .accentPalette('blue');

            common.registerHttpInterceptors($httpProvider);

            getRouteConfigs().forEach(config => {
                $routeProvider.when(config.path, config.route);
            });
            $routeProvider.otherwise({ redirectTo: '/' });
        }]);
}