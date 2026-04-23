(function () {
    'use strict';

    var hvState = {
        ventaActualId: 0,
        ventas: []
    };

    function byId(id) {
        return document.getElementById(id);
    }

    function showInfo(title, text, icon) {
        if (!window.Swal) {
            return;
        }

        Swal.fire({
            icon: icon || 'info',
            title: title || 'Información',
            text: text || '',
            confirmButtonColor: '#2563eb'
        });
    }

    function confirmAction(title, text, confirmText, onConfirm) {
        if (!window.Swal) {
            if (window.confirm((title || 'Confirmar') + '\n\n' + (text || ''))) {
                onConfirm();
            }
            return;
        }

        Swal.fire({
            icon: 'warning',
            title: title || 'Confirmar',
            text: text || '',
            showCancelButton: true,
            confirmButtonText: confirmText || 'Continuar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#dc2626',
            cancelButtonColor: '#64748b'
        }).then(function (result) {
            if (result.isConfirmed) {
                onConfirm();
            }
        });
    }

    function setValue(id, value) {
        var el = byId(id);
        if (el) {
            el.value = value == null ? '' : String(value);
        }
    }

    function setChecked(id, value) {
        var el = byId(id);
        if (el) {
            el.checked = !!value;
        }
    }

    function crearFilaCorreoFactura(valor) {
        var wrapper = document.createElement('div');
        wrapper.className = 'input-group hv-correo-fila';
        wrapper.innerHTML =
            '<input type="email" class="form-control hv-correo-adicional" placeholder="correo.adicional@dominio.com" />' +
            '<button type="button" class="btn btn-outline-danger hv-btn-eliminar-correo">' +
            '<i class="bi bi-trash"></i>' +
            '</button>';

        var input = wrapper.querySelector('.hv-correo-adicional');
        if (input && valor) {
            input.value = valor;
        }

        return wrapper;
    }

    function aplicarFiltrosVentas() {
        var txtBuscar = byId('fBuscar');
        var selMedio = byId('fMedioPago');
        var sinResultados = byId('hvSinResultados');
        var labelResultados = byId('hvResultadosLabel');
        var rows = Array.prototype.slice.call(document.querySelectorAll('.hv-venta-row'));

        var texto = ((txtBuscar && txtBuscar.value) || '').trim().toLowerCase();
        var medio = ((selMedio && selMedio.value) || '').trim().toLowerCase();
        var visibles = 0;

        rows.forEach(function (row) {
            var cumpleTexto = !texto || (row.getAttribute('data-search') || '').toLowerCase().indexOf(texto) >= 0;
            var cumpleMedio = !medio || (row.getAttribute('data-medio') || '').toLowerCase() === medio;
            var visible = cumpleTexto && cumpleMedio;

            row.style.display = visible ? '' : 'none';
            if (visible) {
                visibles += 1;
            }
        });

        if (labelResultados) {
            labelResultados.textContent = String(visibles);
        }

        if (sinResultados) {
            sinResultados.style.display = visibles === 0 ? '' : 'none';
        }
    }

    function filtrarClientes() {
        var filtroNombre = byId('hvFiltroClienteNombre');
        var filtroDocumento = byId('hvBuscarDocumentoCliente');
        var rows = Array.prototype.slice.call(document.querySelectorAll('#hvClientesBody .cliente-row'));

        var nombre = ((filtroNombre && filtroNombre.value) || '').trim().toUpperCase();
        var documento = ((filtroDocumento && filtroDocumento.value) || '').trim().toUpperCase();

        rows.forEach(function (row) {
            var rowNombre = (row.getAttribute('data-nombre') || '').toUpperCase();
            var rowNit = (row.getAttribute('data-nit') || '').toUpperCase();

            var cumpleNombre = !nombre || rowNombre.indexOf(nombre) >= 0;
            var cumpleDocumento = !documento || rowNit.indexOf(documento) >= 0;

            row.style.display = (cumpleNombre && cumpleDocumento) ? '' : 'none';
        });
    }

    function limpiarFormularioCliente() {
        [
            'hvTxtIdentificacionCliente',
            'hvTxtNombreCliente',
            'hvTxtNombreComercioCliente',
            'hvTxtTelefonoCliente',
            'hvTxtDireccionCliente',
            'hvTxtCorreoCliente',
            'hvTxtMatriculaCliente',
            'hvBuscarDocumentoCliente',
            'hvFiltroClienteNombre',
            'hvTxtIdCliente'
        ].forEach(function (id) {
            var el = byId(id);
            if (el) {
                el.value = '';
            }
        });

        [
            'ddlTipoDocumentoHv',
            'ddlTipoOrganizacionHv',
            'ddlMunicipioHv',
            'ddlTipoRegimenHv',
            'ddlTipoResponsabilidadHv',
            'ddlDetalleImpuestoHv'
        ].forEach(function (id) {
            var el = byId(id);
            if (el) {
                el.value = '';
            }
        });

        var chkClientes = byId('hvChkClientesModal');
        var chkProveedores = byId('hvChkProveedoresModal');
        var btnSeleccionar = byId('hvBtnSeleccionarCliente');
        var btnGuardar = byId('hvBtnGuardarClienteCatalogo');
        var lblGuardar = byId('hvLblGuardarCliente');
        var rows = Array.prototype.slice.call(document.querySelectorAll('#hvClientesBody .cliente-row'));

        if (chkClientes) chkClientes.checked = true;
        if (chkProveedores) chkProveedores.checked = false;
        if (btnSeleccionar) btnSeleccionar.disabled = true;

        if (btnGuardar) {
            btnGuardar.value = 'Guardar';
        }

        if (lblGuardar) {
            lblGuardar.textContent = 'Guardar';
        }

        rows.forEach(function (row) {
            row.classList.remove('selected');
        });

        filtrarClientes();
    }

    function parseClienteRow(row) {
        var raw = row.getAttribute('data-cliente') || '';
        if (!raw) {
            return null;
        }

        try {
            return JSON.parse(raw);
        } catch (error) {
            console.error('No se pudo parsear data-cliente', error);
            return null;
        }
    }

    function llenarFormularioCliente(cliente) {
        if (!cliente) {
            return;
        }

        setValue('ddlTipoDocumentoHv', cliente.typeDocumentIdentification_id || cliente.typeDocId);
        setValue('hvTxtIdentificacionCliente', cliente.identificationNumber || cliente.nit);
        setValue('ddlTipoOrganizacionHv', cliente.typeOrganization_id || cliente.orgId);
        setValue('ddlMunicipioHv', cliente.municipality_id || cliente.municipio_id || cliente.municipioId);
        setValue('ddlTipoRegimenHv', cliente.typeRegime_id || cliente.regimenId);
        setValue('ddlTipoResponsabilidadHv', cliente.typeLiability_id || cliente.responsabilidadId);
        setValue('ddlDetalleImpuestoHv', cliente.typeTaxDetail_id || cliente.impuestoId);

        setValue('hvTxtNombreCliente', cliente.nameCliente || cliente.nombre);
        setValue('hvTxtNombreComercioCliente', cliente.commercialName || cliente.tradeName || cliente.comercio);
        setValue('hvTxtTelefonoCliente', cliente.phone || cliente.telefono);
        setValue('hvTxtDireccionCliente', cliente.adress || cliente.direccion);
        setValue('hvTxtCorreoCliente', cliente.email || cliente.correo);
        setValue('hvTxtMatriculaCliente', cliente.merchantRegistration || cliente.matricula);

        setChecked('hvChkClientesModal', cliente.esCliente != null ? cliente.esCliente : true);
        setChecked('hvChkProveedoresModal', cliente.esProveedor);

        setValue('hvFiltroClienteNombre', cliente.nameCliente || cliente.nombre);
        setValue('hvTxtIdCliente', cliente.id);
    }

    function seleccionarClienteRow(row) {
        var rows = Array.prototype.slice.call(document.querySelectorAll('#hvClientesBody .cliente-row'));
        var btnSeleccionar = byId('hvBtnSeleccionarCliente');

        rows.forEach(function (item) {
            item.classList.remove('selected');
        });

        row.classList.add('selected');

        if (btnSeleccionar) {
            btnSeleccionar.disabled = false;
        }

        var nit = row.getAttribute('data-nit') || '';
        var nombre = row.getAttribute('data-nombre') || '';

        setValue('hvTxtIdentificacionCliente', nit);
        setValue('hvBuscarDocumentoCliente', nit);
        setValue('hvTxtNombreCliente', nombre);

        var cliente = parseClienteRow(row);
        if (cliente) {
            llenarFormularioCliente(cliente);
        }
    }

    function configurarEventosClienteModal() {
        var filtroNombre = byId('hvFiltroClienteNombre');
        var filtroDocumento = byId('hvBuscarDocumentoCliente');
        var btnLimpiar = byId('hvBtnLimpiarClienteFormulario');
        var btnBuscar = byId('hvBtnBuscarClienteDocumento');
        var btnGuardar = byId('hvBtnGuardarClienteCatalogo');
        var btnSeleccionar = byId('hvBtnSeleccionarCliente');
        var rows = Array.prototype.slice.call(document.querySelectorAll('#hvClientesBody .cliente-row'));

        rows.forEach(function (row) {
            row.addEventListener('click', function () {
                seleccionarClienteRow(row);
            });
        });

        if (filtroNombre) {
            filtroNombre.addEventListener('input', filtrarClientes);
        }

        if (filtroDocumento) {
            filtroDocumento.addEventListener('input', filtrarClientes);
        }

        if (btnLimpiar) {
            btnLimpiar.addEventListener('click', limpiarFormularioCliente);
        }

        if (btnBuscar) {
            btnBuscar.addEventListener('click', function () {
                var tipoDocumento = byId('ddlTipoDocumentoHv');
                var documento = ((filtroDocumento && filtroDocumento.value) || '').trim();

                if (!tipoDocumento || !tipoDocumento.value || !documento) {
                    showInfo('Atención', 'Completa tipo de documento y número para continuar.', 'warning');
                    return;
                }

                if (window.hvAction && typeof window.hvAction.buscarNIT === 'function') {
                    window.hvAction.buscarNIT(documento);
                    return;
                }

                showInfo('Error', 'No se encontró la acción buscarNIT.', 'error');
            });
        }

        if (btnGuardar) {
            btnGuardar.addEventListener('click', function () {
                var payload = {
                    tipoDocumento: (byId('ddlTipoDocumentoHv') && byId('ddlTipoDocumentoHv').value) || '',
                    identificacion: (byId('hvTxtIdentificacionCliente') && byId('hvTxtIdentificacionCliente').value) || '',
                    tipoOrganizacion: (byId('ddlTipoOrganizacionHv') && byId('ddlTipoOrganizacionHv').value) || '',
                    municipio: (byId('ddlMunicipioHv') && byId('ddlMunicipioHv').value) || '',
                    regimen: (byId('ddlTipoRegimenHv') && byId('ddlTipoRegimenHv').value) || '',
                    responsabilidad: (byId('ddlTipoResponsabilidadHv') && byId('ddlTipoResponsabilidadHv').value) || '',
                    impuesto: (byId('ddlDetalleImpuestoHv') && byId('ddlDetalleImpuestoHv').value) || '',
                    nombre: (byId('hvTxtNombreCliente') && byId('hvTxtNombreCliente').value) || '',
                    comercio: (byId('hvTxtNombreComercioCliente') && byId('hvTxtNombreComercioCliente').value) || '',
                    telefono: (byId('hvTxtTelefonoCliente') && byId('hvTxtTelefonoCliente').value) || '',
                    direccion: (byId('hvTxtDireccionCliente') && byId('hvTxtDireccionCliente').value) || '',
                    correo: (byId('hvTxtCorreoCliente') && byId('hvTxtCorreoCliente').value) || '',
                    matricula: (byId('hvTxtMatriculaCliente') && byId('hvTxtMatriculaCliente').value) || '',
                    esCliente: !!(byId('hvChkClientesModal') && byId('hvChkClientesModal').checked),
                    esProveedor: !!(byId('hvChkProveedoresModal') && byId('hvChkProveedoresModal').checked)
                };

                if (window.hvAction && typeof window.hvAction.guardarCliente === 'function') {
                    window.hvAction.guardarCliente(payload);
                    return;
                }

                showInfo('Error', 'No se encontró la acción guardarCliente.', 'error');
            });
        }

        if (btnSeleccionar) {
            btnSeleccionar.addEventListener('click', function () {
                var clienteId = byId('hvTxtIdCliente') ? byId('hvTxtIdCliente').value : '';

                if (!clienteId) {
                    showInfo('Atención', 'Debes seleccionar un cliente.', 'warning');
                    return;
                }

                var payload = {
                    clienteId: clienteId
                };

                if (window.hvAction && typeof window.hvAction.seleccionarCliente === 'function') {
                    window.hvAction.seleccionarCliente(payload);
                    return;
                }

                showInfo('Error', 'No se encontró la acción seleccionarCliente.', 'error');
            });
        }
    }

    function configurarEventosFiltros() {
        var txtBuscar = byId('fBuscar');
        var selMedio = byId('fMedioPago');

        if (txtBuscar) {
            txtBuscar.addEventListener('input', aplicarFiltrosVentas);
        }

        if (selMedio) {
            selMedio.addEventListener('change', aplicarFiltrosVentas);
        }
    }

    function configurarEventosTabla() {
        document.addEventListener('click', function (e) {
            var btnVer = e.target.closest('.btn-ver-venta');
            if (btnVer) {
                var idVer = btnVer.dataset.id;
                hvAbrirVenta(idVer);
                return;
            }

            var btnResolucion = e.target.closest('.btn-editar-resolucion');
            if (btnResolucion) {
                var idResolucion = btnResolucion.dataset.id;
                hvEditarResolucion(idResolucion);
                return;
            }

            var btnCliente = e.target.closest('.btn-editar-cliente');
            if (btnCliente) {
                var idCliente = btnCliente.dataset.id;
                hvEditarCliente(idCliente);
                return;
            }

            var btnImprimir = e.target.closest('.btn-imprimir-venta');
            if (btnImprimir) {
                if (btnImprimir.dataset.loading === '1') return;

                btnImprimir.dataset.loading = '1';
                btnImprimir.disabled = true;

                var idImprimir = btnImprimir.dataset.id;

                if (window.LoaderGlobal) {
                    LoaderGlobal.mostrar('Preparando impresión...');
                }

                hvImprimirVenta(idImprimir);
                return;
            }

            var btnAnular = e.target.closest('.btn-anular-venta');
            if (btnAnular) {
                if (btnAnular.classList.contains('disabled') || btnAnular.disabled) return;

                var idAnular = btnAnular.dataset.id;
                var facturaAnular = btnAnular.dataset.factura || ('Venta #' + idAnular);

                confirmAction(
                    'Anular venta',
                    'Se anulará la venta ' + facturaAnular + '. Esta acción aplica para ventas POS y electrónicas.',
                    'Sí, anular',
                    function () {
                        hvAnularVenta(idAnular);
                    });
                return;
            }

            var btnDevolucion = e.target.closest('.btn-devolucion-venta');
            if (btnDevolucion) {
                if (btnDevolucion.classList.contains('disabled') || btnDevolucion.disabled) return;

                var idDevolucion = btnDevolucion.dataset.id;
                var facturaDevolucion = btnDevolucion.dataset.factura || ('Venta #' + idDevolucion);

                confirmAction(
                    'Registrar devolución',
                    'Se pondrá en 0 el número de venta de ' + facturaDevolucion + '. Esta acción solo aplica para facturas no electrónicas.',
                    'Sí, devolver',
                    function () {
                        hvDevolucionVenta(idDevolucion);
                    });
                return;
            }

            var btnEnviarDIAN = e.target.closest('.btn-enviar-dian');
            if (btnEnviarDIAN) {
                if (btnEnviarDIAN.dataset.loading === '1') return;

                btnEnviarDIAN.dataset.loading = '1';
                btnEnviarDIAN.disabled = true;

                var idVentaDIAN = btnEnviarDIAN.dataset.id;

                if (window.LoaderGlobal) {
                    LoaderGlobal.mostrar('Enviando factura a DIAN...');
                }

                hvEnviarDIAN(idVentaDIAN);
                return;
            }

            var btnReenviarCorreoFe = e.target.closest('.btn-reenviar-correo-fe');
            if (btnReenviarCorreoFe) {
                if (btnReenviarCorreoFe.classList.contains('disabled') || btnReenviarCorreoFe.disabled) return;
                if (btnReenviarCorreoFe.dataset.loading === '1') return;

                btnReenviarCorreoFe.dataset.loading = '1';
                btnReenviarCorreoFe.disabled = true;

                var idVentaCorreoFe = btnReenviarCorreoFe.dataset.id;

                if (window.LoaderGlobal) {
                    LoaderGlobal.mostrar('Consultando correos para el envío...');
                }

                hvReenviarCorreoFE(idVentaCorreoFe);
                return;
            }

            var btnDescargarPDF = e.target.closest('.btn-descargar-pdf');
            if (btnDescargarPDF) {
                if (btnDescargarPDF.dataset.loading === '1') return;

                btnDescargarPDF.dataset.loading = '1';
                btnDescargarPDF.disabled = true;

                var idVentaPdf = btnDescargarPDF.dataset.id;

                if (window.LoaderGlobal) {
                    LoaderGlobal.mostrar('Descargando PDF...');
                }

                hvDescargarPDF(idVentaPdf);
                return;
            }

            var btnDescargarNotaCredito = e.target.closest('.btn-descargar-nota-credito');
            if (btnDescargarNotaCredito) {
                if (btnDescargarNotaCredito.classList.contains('disabled')) return;
                if (btnDescargarNotaCredito.dataset.loading === '1') return;

                btnDescargarNotaCredito.dataset.loading = '1';
                btnDescargarNotaCredito.disabled = true;

                var idVentaNotaCredito = btnDescargarNotaCredito.dataset.id;

                if (window.LoaderGlobal) {
                    LoaderGlobal.mostrar('Descargando nota crédito PDF...');
                }

                hvDescargarNotaCreditoPDF(idVentaNotaCredito);
                return;
            }
        });
    }

    function configurarEventosModales() {
        document.addEventListener('click', function (e) {
            var btnSeleccionarResolucion = e.target.closest('.btn-seleccionar-resolucion');

            if (btnSeleccionarResolucion) {
                var payload = {
                    resolucionId: btnSeleccionarResolucion.dataset.id || '',
                    ventaId: btnSeleccionarResolucion.dataset.idventa || ''
                };

                if (window.hvAction && typeof window.hvAction.guardarResolucion === 'function') {
                    window.hvAction.guardarResolucion(payload);
                    return;
                }

                showInfo('Error', 'No se encontró la acción guardarResolucion.', 'error');
                return;
            }

            var btnImprimirActual = e.target.closest('#hvBtnImprimirVentaActual');
            if (btnImprimirActual) {
                var idImprimir = btnImprimirActual.dataset.id || '';
                hvImprimirVenta(idImprimir);
                return;
            }

            var btnAgregarCorreo = e.target.closest('#hvBtnAgregarCorreoFactura');
            if (btnAgregarCorreo) {
                var listaCorreos = byId('hvCorreosFacturaLista');
                if (listaCorreos) {
                    listaCorreos.appendChild(crearFilaCorreoFactura(''));
                }
                return;
            }

            var btnEliminarCorreo = e.target.closest('.hv-btn-eliminar-correo');
            if (btnEliminarCorreo) {
                var fila = btnEliminarCorreo.closest('.hv-correo-fila');
                var lista = byId('hvCorreosFacturaLista');
                if (!fila || !lista) return;

                var filas = lista.querySelectorAll('.hv-correo-fila');
                if (filas.length <= 1) {
                    var input = fila.querySelector('.hv-correo-adicional');
                    if (input) input.value = '';
                } else {
                    fila.remove();
                }
                return;
            }

            var btnConfirmarCorreo = e.target.closest('#hvBtnConfirmarEnvioCorreoFactura');
            if (btnConfirmarCorreo) {
                var ventaId = byId('hvCorreoFacturaVentaId') ? byId('hvCorreoFacturaVentaId').value : '';
                var correoPrincipal = byId('hvCorreoPrincipalFactura') ? byId('hvCorreoPrincipalFactura').value : '';
                var correos = Array.prototype.slice.call(document.querySelectorAll('#hvCorreosFacturaLista .hv-correo-adicional'))
                    .map(function (input) { return (input.value || '').trim(); })
                    .filter(function (value) { return !!value; });

                if (!correoPrincipal.trim()) {
                    showInfo('Atención', 'Debes indicar el correo principal del cliente.', 'warning');
                    return;
                }

                if (window.LoaderGlobal) {
                    LoaderGlobal.mostrar('Enviando factura electrónica por correo...');
                }

                hvConfirmarEnvioCorreoFE({
                    ventaId: parseInt(ventaId || '0', 10) || 0,
                    correoPrincipal: correoPrincipal.trim(),
                    correos: correos
                });
            }
        });
    }

    window.setClienteData = function (data) {
        if (!data) {
            return;
        }

        llenarFormularioCliente(data);

        var btnGuardar = byId('hvBtnGuardarClienteCatalogo');
        var lblGuardar = byId('hvLblGuardarCliente');

        if (btnGuardar && data.actionLabel) {
            btnGuardar.value = data.actionLabel;
        }

        if (lblGuardar && data.actionLabel) {
            lblGuardar.textContent = data.actionLabel;
        }
    };

    window.hvAbrirVenta = function (idVenta) {
        hvState.ventaActualId = idVenta;

        if (window.hvAction && typeof window.hvAction.abrirVenta === 'function') {
            window.hvAction.abrirVenta(idVenta);
            return;
        }

        showInfo('Error', 'No se encontró la acción abrirVenta.', 'error');
    };

    window.hvEditarResolucion = function (idVenta) {
        hvState.ventaActualId = idVenta;

        if (window.hvAction && typeof window.hvAction.editarResolucion === 'function') {
            window.hvAction.editarResolucion(idVenta);
            return;
        }

        showInfo('Error', 'No se encontró la acción editarResolucion.', 'error');
    };

    window.hvEditarCliente = function (idVenta) {
        hvState.ventaActualId = idVenta;

        if (window.hvAction && typeof window.hvAction.editarCliente === 'function') {
            window.hvAction.editarCliente(idVenta);
            return;
        }

        showInfo('Error', 'No se encontró la acción editarCliente.', 'error');
    };

    window.hvImprimirVenta = function (idVenta) {
        if (window.hvAction && typeof window.hvAction.imprimirVenta === 'function') {
            window.hvAction.imprimirVenta(idVenta);
            return;
        }

        showInfo('Error', 'No se encontró la acción imprimirVenta.', 'error');
    };

    window.hvAnularVenta = function (idVenta) {
        if (window.hvAction && typeof window.hvAction.anularVenta === 'function') {
            window.hvAction.anularVenta(idVenta);
            return;
        }

        showInfo('Error', 'No se encontró la acción anularVenta.', 'error');
    };

    window.hvDevolucionVenta = function (idVenta) {
        if (window.hvAction && typeof window.hvAction.devolucionVenta === 'function') {
            window.hvAction.devolucionVenta(idVenta);
            return;
        }

        showInfo('Error', 'No se encontró la acción devolucionVenta.', 'error');
    };

    window.hvEnviarDIAN = function (idVenta) {
        if (window.hvAction && typeof window.hvAction.enviarDIAN === 'function') {
            window.hvAction.enviarDIAN(idVenta);
            return;
        }

        showInfo('Error', 'No se encontró la acción enviar factura.', 'error');
    };

    window.hvReenviarCorreoFE = function (idVenta) {
        if (window.hvAction && typeof window.hvAction.reenviarCorreoFE === 'function') {
            window.hvAction.reenviarCorreoFE(idVenta);
            return;
        }

        showInfo('Error', 'No se encontró la acción reenviar correo FE.', 'error');
    };

    window.hvConfirmarEnvioCorreoFE = function (payload) {
        if (window.hvAction && typeof window.hvAction.confirmarEnvioCorreoFE === 'function') {
            window.hvAction.confirmarEnvioCorreoFE(payload);
            return;
        }

        showInfo('Error', 'No se encontró la acción confirmar envío de correo FE.', 'error');
    };

    window.hvDescargarPDF = function (idVenta) {
        if (window.hvAction && typeof window.hvAction.descargarPDF === 'function') {
            window.hvAction.descargarPDF(idVenta);
            return;
        }

        showInfo('Error', 'No se encontró la acción descargar PDF.', 'error');
    };

    window.hvDescargarNotaCreditoPDF = function (idVenta) {
        if (window.hvAction && typeof window.hvAction.descargarNotaCreditoPDF === 'function') {
            window.hvAction.descargarNotaCreditoPDF(idVenta);
            return;
        }

        showInfo('Error', 'No se encontró la acción descargar nota crédito PDF.', 'error');
    };

    window.hvImprimirVentaActual = function () {
        if (!hvState.ventaActualId) {
            return;
        }

        window.hvImprimirVenta(hvState.ventaActualId);
    };

    window.hvSetData = function (data) {
        data = data || {};

        if (Array.isArray(data.ventas)) {
            hvState.ventas = data.ventas;
        }

        aplicarFiltrosVentas();
    };

    document.addEventListener('DOMContentLoaded', function () {
        configurarEventosFiltros();
        configurarEventosClienteModal();
        configurarEventosTabla();
        configurarEventosModales();
        aplicarFiltrosVentas();
    });
})();
