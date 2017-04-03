/// <reference path="../../_all.ts" />
var common;
(function (common) {
    var userMenuWidgetController = (function () {
        function userMenuWidgetController(config, $mdPanel, loginQueryService, $rootScope) {
            var _this = this;
            this.config = config;
            this.$mdPanel = $mdPanel;
            this.loginQueryService = loginQueryService;
            this.$rootScope = $rootScope;
            this.estaLogueado = false;
            this.$rootScope.$on(this.config.eventIndex.login.loggedIn, function (e, args) {
                _this.verificarLogueo();
            });
            this.$rootScope.$on(this.config.eventIndex.login.loggedOut, function (e, args) {
                _this.verificarLogueo();
            });
            this.verificarLogueo();
        }
        userMenuWidgetController.prototype.mostrarMenu = function ($event) {
            var panelConfig;
            var position = this.$mdPanel
                .newPanelPosition()
                .relativeTo($event.target)
                .addPanelPosition(this.$mdPanel.xPosition.ALIGN_START, this.$mdPanel.yPosition.BELOW)
                .withOffsetX('-75px');
            panelConfig = {
                position: position,
                attachTo: angular.element(document.body),
                controller: panelMenuController,
                controllerAs: 'vm',
                templateUrl: 'dist/common/userMenu/menu-items-template.html',
                panelClass: 'menu-panel-container',
                openFrom: $event,
                clickOutsideToClose: true,
                escapeToClose: true,
                focusOnOpen: true,
                zIndex: 100
            };
            this.$mdPanel.open(panelConfig);
        };
        userMenuWidgetController.prototype.verificarLogueo = function () {
            var result = this.loginQueryService.tryGetLocalLoginInfo();
            if (result !== undefined && result.loginExitoso) {
                this.estaLogueado = true;
                this.nombreParaMostrar = result.nombreParaMostrar;
                this.avatarUrl = result.avatarUrl;
            }
            else {
                this.estaLogueado = false;
            }
        };
        return userMenuWidgetController;
    }());
    userMenuWidgetController.$inject = ['config', '$mdPanel', 'loginQueryService', '$rootScope'];
    common.userMenuWidgetController = userMenuWidgetController;
    var panelMenuController = (function () {
        function panelMenuController(mdPanelRef, loginService) {
            this.mdPanelRef = mdPanelRef;
            this.loginService = loginService;
            this.menuItemList = [
                new menuItem('Inicio', 'home.html'),
                new menuItem('Usuarios', 'usuarios.html'),
                new menuItem('Siscole', 'http://ti.fecoprod.com.py/siscole')
            ];
            this.estaEnHome = window.location.pathname == '/app/home.html';
        }
        panelMenuController.prototype.logOut = function () {
            this.loginService.logOut();
            this.closeMenu();
            if (!this.estaEnHome)
                window.location.href = 'home.html';
        };
        panelMenuController.prototype.closeMenu = function () {
            this.mdPanelRef.close();
        };
        panelMenuController.prototype.seleccionarItem = function (item) {
            window.location.href = item.link;
        };
        return panelMenuController;
    }());
    panelMenuController.$inject = ['mdPanelRef', 'loginService'];
    var menuItem = (function () {
        function menuItem(name, link) {
            this.name = name;
            this.link = link;
        }
        return menuItem;
    }());
})(common || (common = {}));
//# sourceMappingURL=widgetController.js.map