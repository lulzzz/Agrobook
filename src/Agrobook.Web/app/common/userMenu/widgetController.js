/// <reference path="../../_all.ts" />
var common;
(function (common) {
    var userMenuWidgetController = (function () {
        function userMenuWidgetController(config, $mdPanel, loginQueryService, $rootScope, $scope) {
            var _this = this;
            this.config = config;
            this.$mdPanel = $mdPanel;
            this.loginQueryService = loginQueryService;
            this.$rootScope = $rootScope;
            this.$scope = $scope;
            this.estaLogueado = false;
            if (window.location.search === "?unauth=1")
                return;
            this.$rootScope.$on(this.config.eventIndex.login.loggedIn, function (e, args) {
                _this.verificarLogueo();
            });
            this.$rootScope.$on(this.config.eventIndex.login.loggedOut, function (e, args) {
                _this.verificarLogueo();
            });
            this.verificarLogueo();
            this.$scope.$on(this.config.eventIndex.usuarios.perfilActualizado, function (e, args) {
                if (_this.estaLogueado && _this.usuario === args.usuario) {
                    _this.nombreParaMostrar = args.nombreParaMostrar;
                    _this.avatarUrl = args.avatarUrl;
                }
            });
        }
        userMenuWidgetController.prototype.mostrarMenu = function ($event) {
            var position = this.$mdPanel
                .newPanelPosition()
                .relativeTo($event.target)
                .addPanelPosition(this.$mdPanel.xPosition.ALIGN_START, this.$mdPanel.yPosition.BELOW)
                .withOffsetX('-1px');
            var panelConfig = {
                position: position,
                attachTo: angular.element(document.body),
                controller: panelMenuController,
                controllerAs: 'vm',
                templateUrl: '../common/userMenu/menu-items-template.html',
                panelClass: 'menu-panel-container',
                openFrom: $event,
                clickOutsideToClose: true,
                disableParentScroll: true,
                hasBackdrop: true,
                escapeToClose: true,
                focusOnOpen: true,
                zIndex: 100
            };
            this.$mdPanel.open(panelConfig);
        };
        userMenuWidgetController.prototype.verificarLogueo = function () {
            var result = this.loginQueryService.tryGetLocalLoginInfo();
            if (result !== undefined && result.loginExitoso) {
                // verificar si no hay una peticion para que se desloguee
                this.estaLogueado = true;
                this.usuario = result.usuario;
                this.nombreParaMostrar = result.nombreParaMostrar;
                this.avatarUrl = result.avatarUrl;
            }
            else {
                this.estaLogueado = false;
            }
        };
        return userMenuWidgetController;
    }());
    userMenuWidgetController.$inject = ['config', '$mdPanel', 'loginQueryService', '$rootScope', '$scope'];
    common.userMenuWidgetController = userMenuWidgetController;
    var panelMenuController = (function () {
        function panelMenuController(mdPanelRef, loginService, config) {
            this.mdPanelRef = mdPanelRef;
            this.loginService = loginService;
            this.config = config;
            this.estaEnHome = window.location.pathname == '/app/src/home/index.html';
            var claims = this.config.claims;
            var esTecnicoOSuperior = this.loginService.autorizar([claims.roles.Tecnico, claims.roles.Gerente]);
            this.menuItemList = [
                new menuItem('Inicio', '../home/index.html', 'home'),
                new menuItem('Ag. de Precisión', '../ap/index.html', 'my_location'),
                new menuItem(esTecnicoOSuperior
                    ? 'Usuarios' : 'Mi Perfil', '../usuarios/index.html#!/', 'people')
            ];
        }
        panelMenuController.prototype.logOut = function () {
            this.loginService.logOut();
            this.closeMenu();
            if (!this.estaEnHome)
                window.location.href = '../home/index.html';
        };
        panelMenuController.prototype.closeMenu = function () {
            this.mdPanelRef.close();
        };
        panelMenuController.prototype.seleccionarItem = function (item) {
            window.location.href = item.link;
        };
        return panelMenuController;
    }());
    panelMenuController.$inject = ['mdPanelRef', 'loginService', 'config'];
    var menuItem = (function () {
        function menuItem(name, link, icon) {
            this.name = name;
            this.link = link;
            this.icon = icon;
        }
        return menuItem;
    }());
})(common || (common = {}));
//# sourceMappingURL=widgetController.js.map