/// <reference path="../../../_all.ts" />
var apArea;
(function (apArea) {
    var orgTabContratosController = (function () {
        function orgTabContratosController($routeParams, $scope, $mdPanel, apQueryService, apService, toasterLite, config, $rootScope, $timeout) {
            var _this = this;
            this.$routeParams = $routeParams;
            this.$scope = $scope;
            this.$mdPanel = $mdPanel;
            this.apQueryService = apQueryService;
            this.apService = apService;
            this.toasterLite = toasterLite;
            this.config = config;
            this.$rootScope = $rootScope;
            this.$timeout = $timeout;
            this.submitting = false;
            this.tieneContrato = false;
            this.ocultarEliminados = true;
            this.idOrg = this.$routeParams['idOrg'];
            this.recuperarContratos();
            // Suscrito a cambios en el archivo, para actualizar los contratos
            this.$scope.$on(this.config.eventIndex.filesWidget.fileUploaded, function (s, e) {
                _this.$timeout(function () { return _this.recuperarContratos(); }, 4000);
            });
            this.$scope.$on(this.config.eventIndex.filesWidget.fileDeleted, function (s, e) {
                _this.$timeout(function () { return _this.recuperarContratos(); }, 4000);
            });
            this.$scope.$on(this.config.eventIndex.filesWidget.fileRestored, function (s, e) {
                _this.$timeout(function () { return _this.recuperarContratos(); }, 4000);
            });
        }
        //--------------------------
        // Api
        //--------------------------
        orgTabContratosController.prototype.mostrarForm = function (editMode) {
            this.editMode = editMode;
            this.refrescarEstadoDelForm();
            this.formVisible = true;
            setTimeout(function () { return document.getElementById('nombreContratoInput').focus(); }, 0);
        };
        orgTabContratosController.prototype.toggleMostrarEliminados = function () {
            this.ocultarEliminados = !this.ocultarEliminados;
        };
        orgTabContratosController.prototype.cancelar = function () {
            this.formVisible = false;
            this.resetForm();
        };
        orgTabContratosController.prototype.mostrarOpciones = function ($event, contrato) {
            var position = this.$mdPanel.newPanelPosition()
                .relativeTo($event.srcElement)
                .addPanelPosition(this.$mdPanel.xPosition.ALIGN_START, this.$mdPanel.yPosition.BELOW);
            var config = {
                attachTo: angular.element(document.body),
                controller: panelMenuController,
                controllerAs: 'vm',
                hasBackdrop: true,
                templateUrl: './views/org/menu-panel-tab-contratos.html',
                position: position,
                trapFocus: true,
                locals: {
                    'contrato': contrato,
                    'parent': this
                },
                panelClass: 'menu-panel-container',
                openFrom: $event,
                focusOnOpen: true,
                zIndex: 150,
                disableParentScroll: true,
                clickOutsideToClose: true,
                escapeToClose: true
            };
            this.$mdPanel.open(config);
        };
        orgTabContratosController.prototype.habilitarEdicion = function (contrato) {
            this.dirty = new apArea.contratoDto(contrato.id, contrato.idOrg, contrato.display, contrato.esAdenda, contrato.eliminado, contrato.idContratoDeLaAdenda, contrato.fecha);
            this.idColeccion = this.config.categoriaDeArchivos.orgContratos + "-" + contrato.id;
            this.$rootScope.$broadcast(this.config.eventIndex.filesWidget.reloadFiles, { idColeccion: this.idColeccion });
            this.mostrarForm(true);
        };
        orgTabContratosController.prototype.eliminar = function (contrato) {
            var _this = this;
            var callback = new common.callbackLite(function (value) {
                for (var i = 0; i < _this.contratos.length; i++) {
                    if (_this.contratos[i].id === contrato.id) {
                        _this.contratos[i].eliminado = true;
                        break;
                    }
                }
                for (var i = 0; i < _this.soloContratos.length; i++) {
                    if (_this.contratos[i].id === contrato.id) {
                        _this.contratos[i].eliminado = true;
                        break;
                    }
                }
                if (_this.contratoAdendado && _this.contratoAdendado.id === contrato.id)
                    _this.contratoAdendado.eliminado = true;
                _this.toasterLite.info(contrato.esAdenda ? 'Adenda eliminada' : 'Contrato eliminado');
            }, function (reason) { return _this.toasterLite.error('No se pudo eliminar ' + (contrato.esAdenda ? 'la adenda' : 'el contrato')); });
            if (contrato.esAdenda)
                this.apService.eliminarAdenda(contrato.idContratoDeLaAdenda, contrato.id, callback);
            else
                this.apService.eliminarContrato(contrato.id, callback);
        };
        orgTabContratosController.prototype.restaurar = function (contrato) {
            var _this = this;
            var callback = new common.callbackLite(function (value) {
                for (var i = 0; i < _this.contratos.length; i++) {
                    if (_this.contratos[i].id === contrato.id) {
                        _this.contratos[i].eliminado = false;
                        break;
                    }
                }
                for (var i = 0; i < _this.soloContratos.length; i++) {
                    if (_this.contratos[i].id === contrato.id) {
                        _this.contratos[i].eliminado = false;
                        break;
                    }
                }
                if (_this.contratoAdendado && _this.contratoAdendado.id === contrato.id)
                    _this.contratoAdendado.eliminado = false;
                _this.toasterLite.success(contrato.esAdenda ? 'Adenda restaurada' : 'Contrato restaurado');
            }, function (reason) { return _this.toasterLite.error('Hubo un error al intentar restaurar'); });
            if (contrato.esAdenda)
                this.apService.restaurarAdenda(contrato.idContratoDeLaAdenda, contrato.id, callback);
            else
                this.apService.restaurarContrato(contrato.id, callback);
        };
        orgTabContratosController.prototype.submit = function () {
            var _this = this;
            if (this.dirty.display.length === 0) {
                this.toasterLite.error(this.tipoContrato === 'contrato' ? 'Debe especificar el nombre del contrato' : 'Debe especificar el nombre de la adenda');
                return;
            }
            this.submitting = true;
            // Rellenar datos faltantes
            this.dirty.esAdenda = this.tipoContrato === 'adenda';
            if (this.dirty.esAdenda) {
                if (this.contratoAdendado === undefined) {
                    this.toasterLite.error('Debe seleccionar un contrato para continuar');
                    this.submitting = false;
                    return;
                }
                this.dirty.idContratoDeLaAdenda = this.contratoAdendado.id;
            }
            if (this.editMode) {
                // Edit
                this.apService.editarContrato(this.dirty, new common.callbackLite(function (value) {
                    for (var i = 0; i < _this.contratos.length; i++) {
                        if (_this.contratos[i].id === _this.dirty.id) {
                            _this.contratos.splice(i, 1);
                            _this.contratos.push(_this.dirty);
                            break;
                        }
                    }
                    for (var i = 0; i < _this.soloContratos.length; i++) {
                        if (_this.soloContratos[i].id === _this.dirty.id) {
                            _this.soloContratos.splice(i, 1);
                            _this.soloContratos.push(_this.dirty);
                            break;
                        }
                    }
                    _this.toasterLite.success("Contrato editado");
                    _this.resetForm();
                }, function (reason) {
                    _this.submitting = false;
                    _this.toasterLite.error('Hubo un error al intentar editar. Verifique por favor.');
                }));
            }
            else {
                // New
                this.dirty.idOrg = this.idOrg;
                this.apService.registrarNuevoContrato(this.dirty, new common.callbackLite(function (value) {
                    var contrato = new apArea.contratoDto(value.data, _this.dirty.idOrg, _this.dirty.display, _this.dirty.esAdenda, _this.dirty.eliminado, _this.dirty.idContratoDeLaAdenda, _this.dirty.fecha);
                    _this.contratos.push(contrato);
                    if (_this.tipoContrato === 'contrato') {
                        _this.soloContratos.push(contrato);
                    }
                    _this.toasterLite.success(_this.tipoContrato === 'contrato' ? 'Contrato creado' : 'Adenda agregada');
                    _this.resetForm();
                }, function (reason) {
                    _this.submitting = false;
                    _this.toasterLite.error('Hubo un error al intentar registrar el contrato. Verifique por favor.');
                }));
            }
        };
        //--------------------------
        // Private
        //--------------------------
        orgTabContratosController.prototype.recuperarContratos = function () {
            var _this = this;
            this.apQueryService.getContratos(this.idOrg, new common.callbackLite(function (value) {
                _this.contratos = value.data;
                _this.refrescarEstadoDelForm();
            }, function (reason) { }));
        };
        orgTabContratosController.prototype.refrescarEstadoDelForm = function () {
            if (this.editMode) {
                this.tipoContrato = this.dirty.esAdenda ? 'adenda' : 'contrato';
                for (var i = 0; i < this.soloContratos.length; i++) {
                    var contrato = this.soloContratos[i];
                    if (contrato.id === this.dirty.idContratoDeLaAdenda) {
                        this.contratoAdendado = contrato;
                        break;
                    }
                }
            }
            else {
                // Preparando
                this.soloContratos = this.contratos.filter(function (x) { return !x.esAdenda; });
                // Si tiene contrato
                if (this.soloContratos.length > 0) {
                    this.tieneContrato = true;
                    this.tipoContrato = 'adenda'; // valores por defecto
                }
                else {
                    this.tipoContrato = 'contrato'; // valores por defecto si es new
                }
            }
        };
        orgTabContratosController.prototype.resetForm = function () {
            this.formVisible = false;
            this.dirty = undefined;
            this.submitting = false;
            this.editMode = undefined;
        };
        orgTabContratosController.prototype.formatearFecha = function (fecha) {
            return moment(fecha).format('DD/MM/YYYY');
        };
        return orgTabContratosController;
    }());
    orgTabContratosController.$inject = ['$routeParams', '$scope', '$mdPanel', 'apQueryService', 'apService', 'toasterLite', 'config', '$rootScope',
        '$timeout'];
    apArea.orgTabContratosController = orgTabContratosController;
    var panelMenuController = (function () {
        function panelMenuController(mdPanelRef) {
            this.mdPanelRef = mdPanelRef;
        }
        panelMenuController.prototype.editar = function () {
            var _this = this;
            this.mdPanelRef.close().then(function (value) {
                _this.parent.habilitarEdicion(_this.contrato);
            })
                .finally(function () { return _this.mdPanelRef.destroy(); });
        };
        panelMenuController.prototype.eliminar = function () {
            var _this = this;
            this.mdPanelRef.close().then(function (value) {
                _this.parent.eliminar(_this.contrato);
            })
                .finally(function () { return _this.mdPanelRef.destroy(); });
        };
        panelMenuController.prototype.restaurar = function () {
            var _this = this;
            this.mdPanelRef.close().then(function (value) {
                _this.parent.restaurar(_this.contrato);
            })
                .finally(function () { return _this.mdPanelRef.destroy(); });
        };
        panelMenuController.prototype.cancelar = function () {
            var _this = this;
            this.mdPanelRef.close().finally(function () { return _this.mdPanelRef.destroy(); });
        };
        return panelMenuController;
    }());
    panelMenuController.$inject = ['mdPanelRef'];
})(apArea || (apArea = {}));
//# sourceMappingURL=orgTabContratosController.js.map