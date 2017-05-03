/// <reference path="../_all.ts" />
var usuariosArea;
(function (usuariosArea) {
    function getRouteConfigs() {
        return [
            {
                path: '/',
                route: {
                    templateUrl: './dist/usuarios/views/main-content.html',
                    reloadOnSearch: false
                }
            },
            {
                path: '/usuario/:idUsuario',
                route: {
                    templateUrl: './dist/usuarios/views/main-content.html',
                    reloadOnSearch: false
                }
            }
        ];
    }
    usuariosArea.getRouteConfigs = getRouteConfigs;
})(usuariosArea || (usuariosArea = {}));
//# sourceMappingURL=routes.js.map