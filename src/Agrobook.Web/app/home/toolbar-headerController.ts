﻿/// <reference path="../_all.ts" />

module homeArea {
    export class ToolbarHeaderController {

        static $inject = ['loginService', 'loginQueryService', 'config', '$rootScope', 'toasterLite'];

        constructor(
            private loginService: login.loginService,
            private loginQueryService: login.loginQueryService,
            private config: common.config,
            private $rootScope: angular.IRootScopeService,
            private toasterLite: common.toasterLite
        ) {
            // Auth
            this.mostrarUsuariosSiSePuede();


            this.$rootScope.$on(this.config.eventIndex.login.loggedOut, (e, a) => {
                this.verificarSiEstaLogueado();
            });

            this.establecerFormularioDeLogin();
            this.verificarSiEstaLogueado();

            if (window.location.search === "?unauth=1") {
                let message = 'Usted no tiene permiso para continuar o sus permisos fueron modificados. Por favor vuelva a introducir sus credenciales';
                this.toasterLite.error(message, this.toasterLite.delayForever);
                //window.alert(message);
                if (this.estaLogueado)
                    this.loginService.logOut();
            }
        }

        estaLogueado: boolean = false;
        enEspera: boolean = false;
        loginBtnLabel: string;

        nombreParaMostrar: string;

        usuario: string;

        password: string;

        // mostrar menus
        showUsuarios: boolean = false;

        recargarHome() {
            window.location.reload();
        }

        goTo(uri: string) {
            window.location.href = uri;
        }

        onInputKeyPress($event): void {
            if ($event.keyCode == this.config.keyCodes.enter)
                this.login();
        }

        login(): void {
            if (this.usuario == undefined || this.usuario == '') {
                window.alert('Por favor ingrese su usuario');
                return;
            }

            if (this.password == undefined || this.password == '') {
                window.alert('Por favor ingrese su contraseña');
                return;
            }

            this.establecerFormularioDeLogin(true);
            this.loginService.tryLogin(
                new login.credencialesDto(this.usuario, this.password),
                value => {
                    if (value.data.loginExitoso) {
                        if (window.location.search === "?unauth=1") {
                            window.location.search = '';
                            return;
                        }

                        this.establecerUsuarioLogueado(value.data.nombreParaMostrar);
                        this.mostrarUsuariosSiSePuede();
                        // clear the inputs
                        this.usuario = '';
                        this.password = '';
                        this.establecerFormularioDeLogin();
                    }
                    else {
                        window.alert("Credenciales inválidas");
                        this.password = '';
                        this.establecerFormularioDeLogin();
                        setTimeout(() => document.getElementById('passwordInput').focus(), 0);
                    }
                },
                reason => {
                    this.establecerFormularioDeLogin();
                    window.alert('Hubo un error inesperado al intentar inciar sesión');
                    console.log(reason);
                });
        }

        private verificarSiEstaLogueado(): void {
            var result = this.loginQueryService.tryGetLocalLoginInfo();
            if (result === undefined || !result.loginExitoso) {
                this.estaLogueado = false;
            }
            else {
                this.establecerUsuarioLogueado(result.nombreParaMostrar);
            }
        }

        private establecerUsuarioLogueado(nombreParaMostrar: string) {
            this.nombreParaMostrar = nombreParaMostrar;
            this.estaLogueado = true;
        }

        private establecerFormularioDeLogin(enEspera: boolean = false): void {
            this.enEspera = enEspera;
            if (enEspera) {
                this.loginBtnLabel = 'Inciando sesión...';
            }
            else {
                this.loginBtnLabel = 'Login';
            }
        }

        private mostrarUsuariosSiSePuede() {
            var roles = this.config.claims.roles;
            this.showUsuarios = this.loginService.autorizar([roles.Gerente, roles.Tecnico]);
        }
    }
}
