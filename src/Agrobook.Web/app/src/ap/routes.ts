﻿/// <reference path="../_all.ts" />

module apArea {

    export interface IRouteConfig {
        path: string;
        route: angular.route.IRoute
    }

    export function getRouteConfigs(): IRouteConfig[] {
        return [
            {
                path: '/',
                route: {
                    templateUrl: './dist/ap/views/main-content.html',
                    reloadOnSearch: false
                }
            },
            {
                path: '/prod/:idProd',
                route: {
                    templateUrl: './dist/ap/views/prod-main-content.html',
                    reloadOnSearch: false
                }
            },
            {
                path: '/org/:idOrg',
                route: {
                    templateUrl: './dist/ap/views/org-main-content.html',
                    reloadOnSearch: false
                }
            },
            {
                path: '/servicios/:idServicio',
                route: {
                    templateUrl: './dist/ap/views/servicio-main-content.html',
                    reloadOnSearch: false
                }
            }
        ];
    }
}