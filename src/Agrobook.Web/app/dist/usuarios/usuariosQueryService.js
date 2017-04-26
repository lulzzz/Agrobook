/// <reference path="../_all.ts" />
var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var usuariosArea;
(function (usuariosArea) {
    var usuariosQueryService = (function (_super) {
        __extends(usuariosQueryService, _super);
        function usuariosQueryService($http) {
            var _this = _super.call(this, $http, 'usuarios/query') || this;
            _this.$http = $http;
            return _this;
        }
        usuariosQueryService.prototype.obtenerListaDeTodosLosUsuarios = function (onSuccess, onError) {
            _super.prototype.get.call(this, 'todos', onSuccess, onError);
        };
        usuariosQueryService.prototype.obtenerInfoBasicaDeUsuario = function (usuario, onSuccess, onError) {
            _super.prototype.get.call(this, 'info-basica/' + usuario, onSuccess, onError);
        };
        return usuariosQueryService;
    }(common.httpLite));
    usuariosQueryService.$inject = ['$http'];
    usuariosArea.usuariosQueryService = usuariosQueryService;
})(usuariosArea || (usuariosArea = {}));
//# sourceMappingURL=usuariosQueryService.js.map