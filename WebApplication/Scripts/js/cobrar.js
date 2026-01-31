(function () {
    const byId = (id) => document.getElementById(id);

    // ====== Utils ======
    const getNumber = (txt) => {
        const raw = (txt || "").toString().replace(/[^\d]/g, '');
        return raw ? parseInt(raw, 10) : 0;
    };

    const getVal = (id) => {
        const el = byId(id);
        return el ? getNumber(el.value) : 0;
    };

    const setVal = (id, val) => {
        const el = byId(id);
        if (el) el.value = (val || 0).toString();
    };

    const formatCOP = (n) => {
        try {
            return new Intl.NumberFormat('es-CO', {
                style: 'currency',
                currency: 'COP',
                maximumFractionDigits: 0
            }).format(n);
        } catch {
            return '$ ' + (n || 0).toString();
        }
    };

    const setTotalText = (total) => {
        const el = byId('lblTotalGrande');
        if (!el) return;
        el.textContent = formatCOP(total || 0);
        el.setAttribute('data-value', (total || 0));
    };

    const getTotalActual = () => {
        const el = byId('lblTotalGrande');
        if (!el) return 0;

        const dv = el.getAttribute('data-value');
        if (dv && dv.toString().trim() !== '') return parseInt(dv, 10) || 0;

        return getNumber(el.textContent || '0');
    };

    // ====== PostBack seguro (con fallback si NO existe __doPostBack) ======
    const firePostBack = (target, argument) => {
        if (typeof window.__doPostBack === 'function') {
            window.__doPostBack(target, argument);
            return;
        }

        const form = document.querySelector('form');
        if (!form) {
            alert('No se encontró el formulario en el MasterPage.');
            return;
        }

        const ensureHidden = (name) => {
            let input = form.querySelector(`input[name="${name}"]`);
            if (!input) {
                input = document.createElement('input');
                input.type = 'hidden';
                input.name = name;
                form.appendChild(input);
            }
            return input;
        };

        ensureHidden('__EVENTTARGET').value = target;
        ensureHidden('__EVENTARGUMENT').value = argument;

        form.submit();
    };

    // ====== Cálculos ======
    let lock = false;

    // ==========================================================
    // ✅ Mixto: persistencia de la suma para sobrevivir postback
    // ==========================================================
    let sumMixtoInternos = 0;
    const KEY_SUM_MIXTO = 'cobrar_sum_mixto';

    const guardarSumMixto = () => {
        try { sessionStorage.setItem(KEY_SUM_MIXTO, String(sumMixtoInternos || 0)); } catch { }
    };

    const cargarSumMixto = () => {
        try {
            const v = parseInt(sessionStorage.getItem(KEY_SUM_MIXTO) || '0', 10);
            return isNaN(v) ? 0 : v;
        } catch { return 0; }
    };

    const limpiarSumMixto = () => {
        try { sessionStorage.removeItem(KEY_SUM_MIXTO); } catch { }
    };

    const norm = (s) => (s || '').toString().trim().toLowerCase();

    // ✅ Más robusto (no exacto)
    const esMixtoPorNombre = (nombre) => norm(nombre).includes('mixto');
    const esEfectivoPorNombre = (nombre) => norm(nombre).includes('efectivo');

    // ==========================================================
    // ✅ control para no pisar el efectivo cuando el usuario lo edita
    // ==========================================================
    let lastMedioPagoNombre = '';
    let efectivoTouched = false;

    // ✅ Cambio se calcula contra ABONO EFECTIVO
    const calcularCambioDesdeEfectivo = () => {
        const abonoEfectivo = getVal('txtAbonoEfectivo');
        const efectivo = getVal('txtEfectivo');

        let cambio = efectivo - abonoEfectivo;
        if (cambio < 0) cambio = 0;

        setVal('txtCambio', cambio);
    };

    const getNombreMedioSeleccionado = () => {
        const ddl_ = byId('ddlMedioPago');
        return ddl_ ? (ddl_.options[ddl_.selectedIndex]?.text || '') : '';
    };

    // ✅ Regla principal
    const aplicarReglaEfectivoPorMedio = () => {
        const nombre_ = getNombreMedioSeleccionado();

        // Detectar cambio de medio
        if (norm(nombre_) !== norm(lastMedioPagoNombre)) {
            efectivoTouched = false;

            // si cambia a mixto, NO borres sumMixtoInternos aquí; lo restaura/carga según caso
            if (!esMixtoPorNombre(nombre_)) {
                sumMixtoInternos = 0;
            }
        }

        // ✅ Mixto
        if (esMixtoPorNombre(nombre_)) {
            const total = getTotalActual();
            let saldo = total - (sumMixtoInternos || 0);
            if (saldo < 0) saldo = 0;

            // ✅ Abono efectivo = saldo
            setVal('txtAbonoEfectivo', saldo);

            // ✅ Por defecto: efectivo = abono, cambio = 0
            if (!efectivoTouched) {
                setVal('txtEfectivo', saldo);
                setVal('txtCambio', 0);
            } else {
                calcularCambioDesdeEfectivo();
            }

            lastMedioPagoNombre = nombre_;
            return;
        }

        // ✅ Efectivo
        if (esEfectivoPorNombre(nombre_)) {
            const total = getTotalActual();

            // ✅ Abono efectivo = total
            setVal('txtAbonoEfectivo', total);

            const efectivoActual = getVal('txtEfectivo');
            if (!efectivoTouched) {
                setVal('txtEfectivo', total);
            } else {
                if (efectivoActual === 0 && total > 0) {
                    setVal('txtEfectivo', total);
                    efectivoTouched = false;
                }
            }

            calcularCambioDesdeEfectivo();
            lastMedioPagoNombre = nombre_;
            return;
        }

        // ✅ Otros medios
        setVal('txtAbonoEfectivo', 0);
        setVal('txtEfectivo', 0);
        setVal('txtCambio', 0);

        lastMedioPagoNombre = nombre_;
    };

    const getSubTotal = () => getVal('txtSubTotal');
    const getIVA = () => getVal('txtIVA');
    const baseDescuento = () => getSubTotal() + getIVA();

    const recalcularTotal = () => {
        const subtotal = getSubTotal();
        const iva = getIVA();
        const propina = getVal('txtPropina');
        const descuento = getVal('txtDescuento');

        let total = subtotal + iva + propina - descuento;
        if (total < 0) total = 0;

        setTotalText(total);
        aplicarReglaEfectivoPorMedio();
    };

    // ====== Propina (% <-> valor) sobre SubTotal ======
    const propinaDesdePct = () => {
        if (lock) return;
        lock = true;

        const subtotal = getSubTotal();
        const pct = getVal('txtPropinaPorcentaje');
        const valor = Math.round((subtotal * pct) / 100);

        setVal('txtPropina', valor);
        recalcularTotal();

        lock = false;
    };

    const pctDesdePropina = () => {
        if (lock) return;
        lock = true;

        const subtotal = getSubTotal();
        const valor = getVal('txtPropina');
        const pct = subtotal > 0 ? Math.round((valor * 100) / subtotal) : 0;

        const elPct = byId('txtPropinaPorcentaje');
        if (elPct) elPct.value = pct.toString();

        recalcularTotal();
        lock = false;
    };

    // ====== Descuento (% <-> valor) sobre (Subtotal + IVA) ======
    const descuentoDesdePct = () => {
        if (lock) return;
        lock = true;

        const base = baseDescuento();
        const pct = getVal('txtDescuentoPorcentaje');
        const valor = Math.round((base * pct) / 100);

        setVal('txtDescuento', valor);
        recalcularTotal();

        lock = false;
    };

    const pctDesdeDescuento = () => {
        if (lock) return;
        lock = true;

        const base = baseDescuento();
        const valor = getVal('txtDescuento');
        const pct = base > 0 ? Math.round((valor * 100) / base) : 0;

        const elPct = byId('txtDescuentoPorcentaje');
        if (elPct) elPct.value = pct.toString();

        recalcularTotal();
        lock = false;
    };

    // ====== Eventos (mientras escribe) ======
    const elPropPct = byId('txtPropinaPorcentaje');
    const elPropVal = byId('txtPropina');
    const elDescPct = byId('txtDescuentoPorcentaje');
    const elDescVal = byId('txtDescuento');

    if (elPropPct) { elPropPct.addEventListener('input', propinaDesdePct); elPropPct.addEventListener('change', propinaDesdePct); }
    if (elPropVal) { elPropVal.addEventListener('input', pctDesdePropina); elPropVal.addEventListener('change', pctDesdePropina); }

    if (elDescPct) { elDescPct.addEventListener('input', descuentoDesdePct); elDescPct.addEventListener('change', descuentoDesdePct); }
    if (elDescVal) { elDescVal.addEventListener('input', pctDesdeDescuento); elDescVal.addEventListener('change', pctDesdeDescuento); }

    const elSub = byId('txtSubTotal');
    const elIva = byId('txtIVA');

    if (elSub) {
        elSub.addEventListener('input', () => {
            if (getVal('txtPropinaPorcentaje') > 0) propinaDesdePct();
            if (getVal('txtDescuentoPorcentaje') > 0) descuentoDesdePct();
            if (getVal('txtPropina') > 0) pctDesdePropina();
            if (getVal('txtDescuento') > 0) pctDesdeDescuento();
            recalcularTotal();
        });
    }
    if (elIva) {
        elIva.addEventListener('input', () => {
            if (getVal('txtDescuentoPorcentaje') > 0) descuentoDesdePct();
            if (getVal('txtDescuento') > 0) pctDesdeDescuento();
            recalcularTotal();
        });
    }

    // ==========================================================
    // ✅ Cambio automático cuando se modifica el efectivo
    // ==========================================================
    const elEfectivo = byId('txtEfectivo');
    if (elEfectivo) {
        const onEfectivoChange = () => {
            const nombre_ = getNombreMedioSeleccionado();
            if (!esEfectivoPorNombre(nombre_) && !esMixtoPorNombre(nombre_)) return;

            efectivoTouched = true;
            calcularCambioDesdeEfectivo();
        };

        elEfectivo.addEventListener('input', onEfectivoChange);
        elEfectivo.addEventListener('change', onEfectivoChange);
        elEfectivo.addEventListener('keyup', onEfectivoChange);
    }

    // ====== Volver ======
    const btnVolver = byId('btnVolver');
    if (btnVolver) {
        btnVolver.addEventListener('click', () => {
            const url = window.CobrarConfig?.urlMenu || '/Menu.aspx';
            window.location.href = url;
        });
    }

    // ====== Factura electrónica ======
    const swFE = byId('swFE');
    const modalClienteEl = byId('mdlCliente');
    if (swFE && modalClienteEl) {
        const modalCliente = bootstrap.Modal.getOrCreateInstance(modalClienteEl, { backdrop: 'static' });
        swFE.addEventListener('change', function () { if (this.checked) modalCliente.show(); });
        modalClienteEl.addEventListener('hidden.bs.modal', function () { swFE.checked = false; });
    }

    // ====== Medios internos ======
    const getRel = () => {
        const hf = byId('hfRelMediosInternos');
        if (!hf || !hf.value) return [];
        try { return JSON.parse(hf.value); } catch { return []; }
    };

    const abrirModalInternos = (idMedioPago, nombreMedioPago) => {
        const modalEl = byId('mdlMediosInternos');
        const cont = byId('listMediosInternos');
        const lbl = byId('lblMedioSeleccionado');
        if (!modalEl || !cont) return;

        const rel = getRel();
        const id = parseInt(idMedioPago, 10);
        const filtrados = rel.filter(x => parseInt(x.idMedioDePago, 10) === id);

        if (!filtrados.length) return;

        if (lbl) lbl.textContent = `Medio seleccionado: ${nombreMedioPago} (ID ${id})`;

        cont.innerHTML = '';

        filtrados.forEach(item => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'list-group-item list-group-item-action d-flex justify-content-between align-items-center';
            btn.innerHTML = `
                <div>
                    <div class="fw-bold">${item.nombreRMPI || 'Sin nombre'}</div>
                    <div class="text-muted" style="font-size:12px;">${item.reporteRDIAN || ''}</div>
                </div>
                <span class="badge text-bg-secondary">ID ${item.idMediosDePagoInternos}</span>
            `;

            btn.addEventListener('click', () => {
                const ddl = byId('ddlMedioPago');
                const idMetodoPago = ddl ? parseInt(ddl.value || '0', 10) : 0;
                const idMedioInterno = parseInt(item.idMediosDePagoInternos || '0', 10);

                const arg = `${idMedioInterno}|${idMetodoPago}`;
                firePostBack('btnSeleccionarPagoInterno', arg);
            });

            cont.appendChild(btn);
        });

        bootstrap.Modal.getOrCreateInstance(modalEl, { backdrop: 'static' }).show();
    };

    // ==========================================================
    // ✅ MODAL PAGO MIXTO
    // ==========================================================
    const cleanInt = (v) => {
        const s = (v ?? '').toString().replace(/[^\d-]/g, '');
        const n = parseInt(s || '0', 10);
        return isNaN(n) ? 0 : n;
    };

    const encodeB64 = (str) => {
        try { return btoa(unescape(encodeURIComponent(str))); }
        catch { return btoa(str); }
    };

    const sumarValoresMixto = () => {
        const body = byId('tblMixtoBody');
        if (!body) return 0;

        const inputs = body.querySelectorAll('input');
        let sum = 0;
        inputs.forEach(i => { sum += cleanInt(i.value); });

        return sum;
    };

    const abrirModalMixto = (idMedioPago, nombreMedioPago) => {
        const modalEl = byId('mdlPagoMixto');
        const body = byId('tblMixtoBody');
        const lbl = byId('lblResumenMixto');
        const btnOk = byId('btnConfirmarPagoMixto');
        if (!modalEl || !body || !btnOk) return;

        const rel = getRel();
        const id = parseInt(idMedioPago, 10);
        const filtrados = rel.filter(x => parseInt(x.idMedioDePago, 10) === id);

        if (!filtrados.length) return;

        if (lbl) lbl.textContent = `Pago Mixto (${nombreMedioPago})`;

        body.innerHTML = '';

        filtrados.forEach(item => {
            const tr = document.createElement('tr');

            const nombre = (item.nombreRMPI || 'Sin nombre');
            const reporte = (item.reporteRDIAN || '');

            tr.innerHTML = `
                <td>
                    <div class="fw-bold">${nombre}</div>
                    <div class="text-muted" style="font-size:12px;">${reporte}</div>
                    <div class="text-muted" style="font-size:12px;">ID ${item.idMediosDePagoInternos}</div>
                </td>
                <td>
                    <input type="text"
                           class="form-control form-control-sm"
                           data-id="${item.idMediosDePagoInternos}"
                           placeholder="0" inputmode="numeric" />
                </td>
            `;
            body.appendChild(tr);
        });

        const recalcularResumenMixto = () => {
            sumMixtoInternos = sumarValoresMixto();
            guardarSumMixto(); // ✅ clave para postback

            if (lbl) lbl.textContent = `Pago Mixto - Total ingresado: ${formatCOP(sumMixtoInternos)}`;
            aplicarReglaEfectivoPorMedio();
        };

        modalEl.oninput = (e) => {
            const t = e.target;
            if (t && t.tagName === 'INPUT') recalcularResumenMixto();
        };
        modalEl.onchange = (e) => {
            const t = e.target;
            if (t && t.tagName === 'INPUT') recalcularResumenMixto();
        };

        modalEl.addEventListener('hidden.bs.modal', function () {
            recalcularResumenMixto();
        }, { once: true });

        btnOk.onclick = () => {
            const ddl = byId('ddlMedioPago');
            const idMetodoPago = ddl ? parseInt(ddl.value || '0', 10) : 0;

            const inputs = body.querySelectorAll('input[data-id]');
            const pagos = [];
            inputs.forEach(i => {
                const idInterno = parseInt(i.getAttribute('data-id') || '0', 10);
                const val = cleanInt(i.value);
                if (idInterno > 0 && val > 0) {
                    pagos.push({ idMedioInterno: idInterno, valor: val });
                }
            });

            if (!pagos.length) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Pago mixto vacío',
                    text: 'Debes ingresar al menos un valor para agregar pagos.',
                    confirmButtonText: 'Entendido',
                    confirmButtonColor: '#2563eb'
                });
                return;
            }

            // ✅ Guardar suma antes de postback
            sumMixtoInternos = sumarValoresMixto();
            guardarSumMixto();

            const payload = { idMetodoPago: idMetodoPago, pagos: pagos };
            const arg = encodeB64(JSON.stringify(payload));
            firePostBack('btnGuardarPagoMixto', arg);

            bootstrap.Modal.getOrCreateInstance(modalEl).hide();
        };

        // ✅ Al abrir: restaura suma previa (si la hay), y recalcula
        sumMixtoInternos = cargarSumMixto();
        efectivoTouched = false;
        aplicarReglaEfectivoPorMedio();
        recalcularResumenMixto();

        bootstrap.Modal.getOrCreateInstance(modalEl, { backdrop: 'static' }).show();
    };

    // ==========================================================
    // ✅ Cambio de medio de pago
    // ==========================================================
    const ddl = byId('ddlMedioPago');
    if (ddl) {
        ddl.addEventListener('change', function () {
            const idMedioPago = this.value;
            const nombre = this.options[this.selectedIndex]?.text || '';

            efectivoTouched = false;

            // si NO es mixto, borra suma guardada
            if (!esMixtoPorNombre(nombre)) {
                sumMixtoInternos = 0;
                limpiarSumMixto();
            } else {
                // si es mixto, trae lo guardado
                sumMixtoInternos = cargarSumMixto();
            }

            if (esMixtoPorNombre(nombre)) {
                aplicarReglaEfectivoPorMedio();
                abrirModalMixto(idMedioPago, nombre);
                return;
            }

            if (esEfectivoPorNombre(nombre)) {
                aplicarReglaEfectivoPorMedio();
                return;
            }

            aplicarReglaEfectivoPorMedio();
            abrirModalInternos(idMedioPago, nombre);
        });
    }

    // ==========================================================
    // ✅ Clientes (tu código original)
    // ==========================================================
    const clienteTable = document.querySelector('#mdlCliente table');
    if (clienteTable) {
        const modalClienteEl2 = byId('mdlCliente');
        const filtroNombre = byId('txtFiltroClienteNombre');
        const filasClientes = () => Array.from(clienteTable.querySelectorAll('.cliente-row'));

        const aplicarFiltroClientes = () => {
            if (!filtroNombre) return;
            const texto = (filtroNombre.value || '').toUpperCase().trim();
            const rows = filasClientes();
            rows.forEach(row => {
                const nombre = (row.dataset.nombre || '').toUpperCase();
                row.style.display = nombre.includes(texto) ? '' : 'none';
            });
        };

        if (filtroNombre) {
            filtroNombre.addEventListener('input', () => {
                filtroNombre.value = filtroNombre.value.toUpperCase();
                aplicarFiltroClientes();
            });
        }

        if (modalClienteEl2) {
            modalClienteEl2.addEventListener('shown.bs.modal', () => {
                const ddlTipoDocumento = byId('ddlTipoDocumento');
                if (!ddlTipoDocumento) return;

                const nitOption = Array.from(ddlTipoDocumento.options)
                    .find(opt => (opt.textContent || '').trim().toUpperCase() === 'NIT');

                if (nitOption) ddlTipoDocumento.value = nitOption.value;
            });
        }

        const setInput = (id, value) => {
            const el = byId(id);
            if (el) el.value = value ?? '';
        };

        const setSelect = (id, value) => {
            const el = byId(id);
            if (!el) return;
            const val = (value ?? '').toString().trim();
            if (!val) return;

            const option = Array.from(el.options).find(o => o.value === val);
            if (option) {
                el.value = val;
                return;
            }

            const opt = document.createElement('option');
            opt.value = val;
            opt.textContent = val;
            el.appendChild(opt);
            el.value = val;
        };

        const setGuardarLabel = (label) => {
            const lbl = byId('lblGuardarCliente');
            if (lbl && label) lbl.textContent = label;
        };

        const aplicarDatosCliente = (data) => {
            if (!data) return;
            setSelect('ddlTipoDocumento', data.typeDocId);
            setInput('txtIdentificacionCliente', data.nit);
            setSelect('ddlTipoOrganizacion', data.orgId);
            setSelect('ddlMunicipio', data.municipioId);
            setSelect('ddlTipoRegimen', data.regimenId);
            setSelect('ddlTipoResponsabilidad', data.responsabilidadId);
            setSelect('ddlDetalleImpuesto', data.impuestoId);
            setInput('txtNombreRazonCliente', data.nombre);
            setInput('txtFiltroClienteNombre', data.nombre);
            aplicarFiltroClientes();
            setInput('txtNombreComercioCliente', data.comercio);
            setInput('txtTelefonoCliente', data.telefono);
            setInput('txtDireccionCliente', data.direccion);
            setInput('txtCorreoCliente', data.correo);
            setInput('txtMatriculaCliente', data.matricula);

            if (data.actionLabel) setGuardarLabel(data.actionLabel);
        };

        window.setClienteData = (data) => { aplicarDatosCliente(data); };

        const btnSeleccionarCliente = byId('btnSeleccionarCliente');

        const seleccionarFila = (row) => {
            if (!row) return;

            clienteTable.querySelectorAll('.cliente-row').forEach(r => r.classList.remove('selected'));
            row.classList.add('selected');
            row.focus();
            if (btnSeleccionarCliente) btnSeleccionarCliente.disabled = !row.dataset.clienteId;

            aplicarDatosCliente({
                clienteId: row.dataset.clienteId,
                typeDocId: row.dataset.typeDocId,
                nit: row.dataset.nit,
                orgId: row.dataset.orgId,
                municipioId: row.dataset.municipioId,
                regimenId: row.dataset.regimenId,
                responsabilidadId: row.dataset.responsabilidadId,
                impuestoId: row.dataset.impuestoId,
                nombre: row.dataset.nombre,
                comercio: row.dataset.comercio,
                telefono: row.dataset.telefono,
                direccion: row.dataset.direccion,
                correo: row.dataset.correo,
                matricula: row.dataset.matricula,
                actionLabel: 'Editar'
            });
        };

        clienteTable.addEventListener('click', (event) => {
            const row = event.target.closest('.cliente-row');
            if (!row) return;
            seleccionarFila(row);
        });

        clienteTable.addEventListener('keydown', (event) => {
            const row = event.target.closest('.cliente-row');
            if (!row) return;

            const rows = Array.from(clienteTable.querySelectorAll('.cliente-row'));
            const index = rows.indexOf(row);
            if (index === -1) return;

            if (event.key === 'ArrowDown') {
                event.preventDefault();
                seleccionarFila(rows[Math.min(index + 1, rows.length - 1)]);
            }

            if (event.key === 'ArrowUp') {
                event.preventDefault();
                seleccionarFila(rows[Math.max(index - 1, 0)]);
            }
        });
    }

    const btnSeleccionarCliente = byId('btnSeleccionarCliente');
    if (btnSeleccionarCliente) {
        btnSeleccionarCliente.addEventListener('click', () => {
            const selected = document.querySelector('#mdlCliente .cliente-row.selected');
            const clienteId = selected?.dataset?.clienteId;

            if (!clienteId) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Selecciona un cliente',
                    text: 'Debes seleccionar un cliente antes de continuar.',
                    confirmButtonColor: '#2563eb'
                });
                return;
            }

            firePostBack('btnSeleccionarCliente', clienteId);
        });
    }

    const btnBuscarNIT = byId('btnBuscarNIT');
    if (btnBuscarNIT) {
        btnBuscarNIT.addEventListener('click', () => {
            const nitInput = byId('txtBuscarNIT');
            const tipoDoc = byId('ddlTipoDocumento');

            const nit = (nitInput ? nitInput.value : '').trim();
            const tipoDocVal = tipoDoc ? (tipoDoc.value || '') : '';

            if (!nit || !tipoDocVal) {
                Swal.fire({
                    icon: 'error',
                    title: 'Falta información',
                    text: 'Para buscar el NIT debes seleccionar el tipo de documento.',
                    confirmButtonColor: '#2563eb'
                });
                return;
            }

            Swal.fire({
                title: 'Consultando...',
                text: 'Buscando cliente, por favor espera.',
                allowOutsideClick: false,
                didOpen: () => { Swal.showLoading(); }
            });

            firePostBack('btnBuscarNIT', nit);
        });
    }

    // ==========================================================
    // ✅ Inicialización final
    // ==========================================================

    // Si arrancamos en mixto (después de postback), restaurar suma
    const nombreInit = getNombreMedioSeleccionado();
    if (esMixtoPorNombre(nombreInit)) {
        sumMixtoInternos = cargarSumMixto();
    }

    if (getVal('txtPropinaPorcentaje') > 0) propinaDesdePct();
    else if (getVal('txtPropina') > 0) pctDesdePropina();

    if (getVal('txtDescuentoPorcentaje') > 0) descuentoDesdePct();
    else if (getVal('txtDescuento') > 0) pctDesdeDescuento();

    recalcularTotal();

})();
