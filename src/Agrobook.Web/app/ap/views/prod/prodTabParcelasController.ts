﻿/// <reference path="../../../_all.ts" />

module apArea {
    export class prodTabParcelasController {
        static $inject = ['config', 'apService', 'apQueryService', 'toasterLite', '$routeParams', '$mdPanel', 'loginService',
        'numberFormatter'];

        constructor(
            private config: common.config,
            private apService: apService,
            private apQueryService: apQueryService,
            private toasterLite: common.toasterLite,
            private $routeParams: angular.route.IRouteParamsService,
            private $mdPanel: angular.material.IPanelService,
            private loginService: login.loginService,
            private numberFormatter: common.numberFormatter
        ) {
            this.mostrarForm = false;

            this.idProd = this.$routeParams['idProd'];

            var roles = config.claims.roles;
            this.tienePermiso = this.loginService.autorizar([roles.Gerente, roles.Tecnico]);

            this.obtenerParcelasDelProd();
            this.obtenerDepartamentos();
        }

        // Estados
        tienePermiso: boolean;
        ocultarEliminados = true;
        mostrarForm: boolean;
        submitting: boolean;
        formIsEditing: boolean; // editMode Actually

        // Objetos
        idProd: string;
        parcelaObject: edicionParcelaDto;
        departamentoSeleccionado: string;
        distritoSeleccionado: string;

        // Listas
        parcelas: parcelaDto[] = [];
        departamentos: departamento[];
        distritos: distrito[];

        // Api
        toggleMostrarEliminados() {
            this.ocultarEliminados = !this.ocultarEliminados;
        }

        habilitarCreacionDeNuevaParcela() {
            this.formIsEditing = false;
            this.mostrarFormYHacerFocus();
        }

        habilitarEdicion(parcela: parcelaDto) {
            this.formIsEditing = true;

            this.parcelaObject = new edicionParcelaDto();
            this.parcelaObject.display = parcela.display;
            this.parcelaObject.hectareas = parcela.hectareas;
            this.parcelaObject.idProd = parcela.idProd;
            this.parcelaObject.idParcela = parcela.id;

            this.departamentoSeleccionado = parcela.idDepartamento;
            this.establecerDistritos();
            this.distritoSeleccionado = parcela.idDistrito;

            this.mostrarFormYHacerFocus();
        }

        mostrarOpciones($event: Event, parcela: parcelaDto) {
            let position = this.$mdPanel.newPanelPosition()
                .relativeTo($event.srcElement)
                .addPanelPosition(
                this.$mdPanel.xPosition.ALIGN_START,
                this.$mdPanel.yPosition.BELOW);

            let config: angular.material.IPanelConfig = {
                attachTo: angular.element(document.body),
                controller: panelMenuController,
                controllerAs: 'vm',
                hasBackdrop: true,
                templateUrl: './views/prod/menu-panel-tab-parcelas.html',
                position: position,
                trapFocus: true,
                locals: {
                    'parcela': parcela,
                    'parent': this
                },
                panelClass: 'menu-panel-container',
                openFrom: $event,
                focusOnOpen: true,
                zIndex: 150,
                disableParentScroll: true,
                clickOutsideToClose: true,
                escapeToClose: true,
            };

            this.$mdPanel.open(config);
        }

        checkIfEnter($event) {
            let keyCode = $event.keyCode;
            if (keyCode === this.config.keyCodes.enter)
                this.registrarNuevaParcela();
            else if (keyCode === this.config.keyCodes.esc) {
                this.cancel();
            }
        }

        submit() {
            if (this.parcelaObject.display.length === 0) {
                this.toasterLite.error("Debe especificar el nombre de la parcela");
                return;
            }
            this.submitting = true;

            if (this.formIsEditing)
                this.editarParcela();
            else
                this.registrarNuevaParcela();
        }

        cancel() {
            this.resetForm();
        }

        eliminar(parcela: parcelaDto) {
            this.apService.eliminarParcela(parcela.idProd, parcela.id,
                new common.callbackLite(
                    value => {
                        for (var i = 0; i < this.parcelas.length; i++) {
                            if (this.parcelas[i].id === parcela.id) {
                                this.parcelas[i].eliminado = true;
                                break;
                            }
                        }

                        this.toasterLite.info('Parcela elimnada');
                    },
                    reason => { })
            );
        }

        restaurar(parcela: parcelaDto) {
            this.apService.restaurarParcela(parcela.idProd, parcela.id,
                new common.callbackLite<{}>(
                    value => {
                        for (var i = 0; i < this.parcelas.length; i++) {
                            if (this.parcelas[i].id === parcela.id) {
                                this.parcelas[i].eliminado = false;
                                break;
                            }
                        }

                        this.toasterLite.success('Parcela restaurada');
                    },
                    reason => { })
            );
        }

        // Privados
        registrarNuevaParcela() {
            this.parcelaObject.idProd = this.idProd;
            this.apService.registrarNuevaParcela(
                this.parcelaObject.idProd,
                this.parcelaObject.display, 
                this.numberFormatter.parseCommaAsDecimalSeparatorToUSNumber(this.parcelaObject.hectareas),
                this.departamentoSeleccionado,
                this.distritoSeleccionado,
                new common.callbackLite<string>(
                    value => {
                        var parcela = new parcelaDto(value.data, this.parcelaObject.idProd, this.parcelaObject.display, this.parcelaObject.hectareas,
                            this.departamentoSeleccionado, null, this.distritoSeleccionado, null, false);
                        this.parcelas.push(parcela);
                        this.toasterLite.success('Parcela creada')
                        this.resetForm();
                    },
                    reason => {
                        this.submitting = false;
                        this.toasterLite.error('Hubo un error al registrar la parcela. Verifique que el nombre ya no exista por favor');
                    })
            );
        }

        editarParcela() {
            this.apService.editarParcela(
                this.parcelaObject.idProd,
                this.parcelaObject.idParcela,
                this.parcelaObject.display,
                this.numberFormatter.parseCommaAsDecimalSeparatorToUSNumber(this.parcelaObject.hectareas),
                this.departamentoSeleccionado,
                this.distritoSeleccionado,
                new common.callbackLite<{}>(
                    value => {
                        // eventual consistency handling before reseting form
                        for (var i = 0; i < this.parcelas.length; i++) {
                            if (this.parcelas[i].id === this.parcelaObject.idParcela) {
                                this.parcelas[i].hectareas = this.parcelaObject.hectareas;
                                this.parcelas[i].display = this.parcelaObject.display;
                                break;
                            }
                        }
                        this.toasterLite.success('Parcela editada')

                        this.resetForm();
                    },
                    reason => {
                        this.submitting = false;
                        this.toasterLite.error('Hubo un error al editar la parcela');
                    })
            );
        }

        private mostrarFormYHacerFocus() {
            this.mostrarForm = true;
            setTimeout(() =>
                document.getElementById('parcelaInput').focus(), 0);
        }

        private resetForm() {
            this.mostrarForm = false;
            this.submitting = false;
            this.parcelaObject = undefined;
        }

        private obtenerParcelasDelProd() {
            this.apQueryService.getParcelasDelProd(this.idProd,
                new common.callbackLite<parcelaDto[]>(
                    response => {
                        response.data.forEach(x => {
                            x.hectareas = this.numberFormatter.formatFromUSNumber(parseFloat(x.hectareas));
                        });
                        this.parcelas = response.data;
                    },
                    reason => this.toasterLite.error('Hubo un error al obtener parcelas'))
            );
        }

        private obtenerDepartamentos() {
            this.apQueryService.getDepartamentos(
                new common.callbackLite<departamento[]>(
                    response => {
                        this.departamentos = response.data;
                    },
                    reason => this.toasterLite.error('Hubo un error al obtener los departamentos'))
            );
        }

        establecerDistritos() {
            for (var i = 0; i < this.departamentos.length; i++) {
                let depto = this.departamentos[i];
                if (depto.id === this.departamentoSeleccionado) {
                    this.distritos = depto.distritos;
                    break;
                }
            }
        }
    }

    class panelMenuController {
        static $inject = ['mdPanelRef'];

        constructor(
            private mdPanelRef: angular.material.IPanelRef
        ) {
        }

        parcela: parcelaDto;
        parent: prodTabParcelasController;

        editar() {
            this.mdPanelRef.close().then(
                value => {
                    this.parent.habilitarEdicion(this.parcela);
                })
                .finally(() => this.mdPanelRef.destroy());
        }

        eliminar() {
            this.mdPanelRef.close().then(
                value => {
                    this.parent.eliminar(this.parcela);
                })
                .finally(() => this.mdPanelRef.destroy());
        }

        restaurar() {
            this.mdPanelRef.close().then(
                value => {
                    this.parent.restaurar(this.parcela);
                })
                .finally(() => this.mdPanelRef.destroy());
        }

        cancelar() {
            this.mdPanelRef.close().finally(() => this.mdPanelRef.destroy());
        }
    }
}