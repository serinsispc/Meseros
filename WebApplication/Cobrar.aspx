<%@ Page Title="Cobro Caja" Language="C#" MasterPageFile="~/CobrarMaster.Master" AutoEventWireup="true"
    CodeBehind="Cobrar.aspx.cs" Inherits="WebApplication.Cobrar"
    MaintainScrollPositionOnPostback="false" Async="true"%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- Bootstrap 5 -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- Bootstrap Icons -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />

    <style>
        :root{
            --bg: #f4f6f9;
            --card: #ffffff;
            --border: #e6e8ee;
            --text: #0f172a;
            --muted: #64748b;
            --green: #16a34a;
            --red: #dc2626;
            --blue: #2563eb;
            --shadow: 0 10px 30px rgba(2, 6, 23, .08);
            --radius: 16px;
        }

        body{ background: var(--bg); color: var(--text); }
        .wrap{ max-width: 860px; margin: 0 auto; padding: 16px; }

        .topbar{
            display:flex; align-items:center; justify-content:space-between; gap:12px;
            padding: 10px 12px; background: var(--card);
            border:1px solid var(--border); border-radius: var(--radius);
            box-shadow: var(--shadow);
            margin-bottom: 12px;
        }

        .brand{ display:flex; align-items:center; gap:10px; }
        .brand .logo{
            width:40px;
            height:40px;
            border-radius: 12px;
            border:1px solid var(--border);
            background:#f8fafc;
            object-fit: contain;
            padding:4px;
        }

        .brand .title{ line-height: 1.15; }
        .brand .title b{ display:block; font-size: 15px; }
        .brand .title small{ color: var(--muted); }

        .cardx{
            background: var(--card);
            border: 1px solid var(--border);
            border-radius: var(--radius);
            box-shadow: var(--shadow);
        }
        .cardx .head{
            padding: 12px 14px;
            border-bottom: 1px solid var(--border);
            display:flex; align-items:center; justify-content:space-between;
            gap: 10px;
        }
        .cardx .body{ padding: 12px 14px; }

        .hint{ color: var(--muted); font-size: 12px; }

        .totalBox{
            background: #eafff0;
            border: 1px solid #c9f3d6;
            border-radius: 14px;
            padding: 12px;
        }
        .totalBox .lbl{ font-weight:800; font-size: 16px; }
        .totalBox .val{
            margin-top: 6px;
            background: var(--green);
            color: #fff;
            border-radius: 12px;
            text-align:center;
            padding: 14px 10px;
            font-size: 34px;
            font-weight: 900;
            letter-spacing: .5px;
        }

        .form-control, .form-select{
            border-radius: 12px;
            border-color: var(--border);
        }
        .form-control:focus, .form-select:focus{
            box-shadow: 0 0 0 .25rem rgba(37,99,235,.12);
            border-color: rgba(37,99,235,.45);
        }

        .btnx{
            border-radius: 12px;
            border: 1px solid var(--border);
            background: #fff;
            font-weight: 700;
        }
        .btnx:hover{ filter: brightness(.98); }

        .btn-green{
            background: var(--green);
            border-color: var(--green);
            color: #fff;
        }
        .btn-green:hover{ filter: brightness(.95); color:#fff; }

        .btn-blue{
            background: var(--blue);
            border-color: var(--blue);
            color: #fff;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- ✅ HiddenField con Static para JS -->
    <asp:HiddenField ID="hfRelMediosInternos" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hfIdVentaActual" runat="server" ClientIDMode="Static" />

    <div class="wrap">

        <!-- TOPBAR -->
        <div class="topbar">
            <div class="brand">
                <img src="<%: ResolveUrl($"~/Recursos/Imagenes/Logo/{(Session["db"] ?? "").ToString()}.png") %>"
                     alt="Logo" class="logo" />
                <div class="title">
                    <b>Cobro Caja</b>
                </div>
            </div>

            <div class="d-flex align-items-center gap-2">
                <button type="button" class="btn btnx px-3" id="btnVolver">
                    <i class="bi bi-arrow-left"></i> Volver
                </button>
            </div>
        </div>

        <div class="cardx">
            <div class="head">
                <div class="d-flex align-items-center gap-2">
                    <i class="bi bi-cash-stack"></i>
                    <b>Datos del cobro</b>
                </div>
            </div>

            <div class="body">

                <!-- Contado / Crédito -->
                <div class="d-flex flex-wrap gap-3 mb-3">
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" id="chkContado" checked />
                        <label class="form-check-label fw-bold" for="chkContado">Venta de Contado</label>
                    </div>
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" id="chkCredito" />
                        <label class="form-check-label fw-bold" for="chkCredito">Venta Crédito</label>
                    </div>
                </div>

                <!-- SubTotal / IVA -->
                <div class="row g-2 mb-2">
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-bold mb-1">SubTotal</label>
                        <input type="text" class="form-control" id="txtSubTotal" runat="server" ClientIDMode="Static" />
                    </div>
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-bold mb-1">IVA</label>
                        <input type="text" class="form-control" id="txtIVA" runat="server" ClientIDMode="Static" />
                    </div>
                </div>

                <!-- Descuento / Propina -->
                <div class="row g-2 mb-3">

                    <!-- DESCUENTO -->
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-bold mb-1">Descuento</label>

                        <div class="row g-2">
                            <div class="col-5">
                                <div class="input-group">
                                    <input type="text" class="form-control" id="txtDescuentoPorcentaje" value="0" />
                                    <span class="input-group-text">%</span>
                                </div>
                                <div class="hint mt-1">% sobre (SubTotal + IVA)</div>
                            </div>

                            <div class="col-7">
                                <input type="text" class="form-control" id="txtDescuento"
                                       runat="server" ClientIDMode="Static" value="0" />
                                <div class="hint mt-1">Valor descuento</div>

                                <!-- ✅ Botones DESCUENTO -->
                                <div class="d-flex gap-2 mt-2">
                                    <button type="button" class="btn btnx btn-blue flex-fill" id="btnGuardarDescuento">
                                        <i class="bi bi-check2-circle"></i> Guardar descuento
                                    </button>
                                    <button type="button" class="btn btnx flex-fill" id="btnEliminarDescuento">
                                        <i class="bi bi-trash3"></i> Eliminar
                                    </button>
                                </div>
                            </div>

                            <div class="col-12">
                                <label class="form-label fw-bold mb-1 mt-2">Razón del descuento</label>
                                <input type="text" class="form-control" id="txtRazonDescuento"
                                       runat="server" ClientIDMode="Static"
                                       placeholder="Ej: Cortesía / Promoción / Ajuste..." />
                            </div>
                        </div>
                    </div>

                    <!-- PROPINA -->
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-bold mb-1">Propina</label>

                        <div class="row g-2">
                            <div class="col-5">
                                <div class="input-group">
                                    <input type="text" class="form-control" id="txtPropinaPorcentaje" value="0" />
                                    <span class="input-group-text">%</span>
                                </div>
                                <div class="hint mt-1">% sobre SubTotal</div>
                            </div>

                            <div class="col-7">
                                <input type="text" class="form-control" id="txtPropina"
                                       runat="server" ClientIDMode="Static" value="0" />
                                <div class="hint mt-1">Valor propina</div>

                                <!-- ✅ Botones PROPINA -->
                                <div class="d-flex gap-2 mt-2">
                                    <button type="button" class="btn btnx btn-blue flex-fill" id="btnGuardarPropina">
                                        <i class="bi bi-check2-circle"></i> Guardar propina
                                    </button>
                                    <button type="button" class="btn btnx flex-fill" id="btnEliminarPropina">
                                        <i class="bi bi-trash3"></i> Eliminar
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>

                </div>

                <!-- Total -->
                <div class="totalBox mb-3">
                    <div class="lbl">Total:</div>
                    <div class="val" id="lblTotalGrande" runat="server" ClientIDMode="Static">0</div>
                </div>

                <!-- Medio de Pago -->
                <div class="mb-3">
                    <label class="form-label fw-bold mb-1">Medio de Pago</label>
                    <asp:DropDownList ID="ddlMedioPago" runat="server" CssClass="form-select" ClientIDMode="Static"></asp:DropDownList>
                </div>

                <!-- Factura electrónica -->
                <div class="d-flex align-items-center justify-content-between border rounded-3 p-3 mb-3" style="border-color: var(--border) !important;">
                    <div>
                        <div class="fw-bold">Factura electrónica</div>
                        <div class="hint">Si activas, se abrirá el modal para agregar cliente</div>
                    </div>
                    <div class="form-check form-switch m-0">
                        <input class="form-check-input" type="checkbox" id="swFE" />
                    </div>
                </div>

                <!-- Efectivo / Cambio -->
                <div class="row g-2 mb-3">
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-bold mb-1">Efectivo</label>
                        <input type="text" class="form-control" id="txtEfectivo" runat="server" ClientIDMode="Static" value="0" />
                        <div class="hint mt-1">Se autocompleta igual al Total</div>
                    </div>
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-bold mb-1">Cambio</label>
                        <input type="text" class="form-control" id="txtCambio" runat="server" ClientIDMode="Static" value="0" />
                        <div class="hint mt-1">Queda en 0</div>
                    </div>
                </div>

                <!-- Guardar -->
                <div class="d-grid">
                    <button type="button" class="btn btnx btn-green bigSave" id="btnGuardar">
                        <i class="bi bi-floppy2-fill"></i> Guardar
                    </button>
                </div>

            </div>
        </div>

    </div>

    <!-- MODAL VACÍO: Agregar Cliente -->
    <div class="modal fade" id="mdlCliente" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content" style="border-radius: 16px;">
                <div class="modal-header">
                    <h5 class="modal-title"><i class="bi bi-person-plus"></i> Agregar Cliente</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <div class="text-muted">Aquí irá el formulario del cliente (por ahora vacío).</div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btnx" data-bs-dismiss="modal">Cerrar</button>
                    <button type="button" class="btn btnx btn-blue" disabled>Guardar Cliente</button>
                </div>
            </div>
        </div>
    </div>

    <!-- MODAL: Medios de pago internos -->
    <div class="modal fade" id="mdlMediosInternos" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content" style="border-radius:16px;">
                <div class="modal-header">
                    <h5 class="modal-title"><i class="bi bi-list-check"></i> Medios internos</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">
                    <div class="text-muted mb-2" id="lblMedioSeleccionado"></div>
                    <div class="list-group" id="listMediosInternos"></div>
                    <div class="text-muted mt-2" style="font-size:12px;">Selecciona uno para continuar.</div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btnx" data-bs-dismiss="modal">Cerrar</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Bootstrap JS -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>

    <!-- ✅ SweetAlert2 (Premium Alerts) -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

<script>
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
                alert('No se encontró el formulario. El MasterPage debe tener &lt;form runat="server"&gt;.');
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

        const getSubTotal = () => getVal('txtSubTotal');
        const getIVA = () => getVal('txtIVA');
        const baseDescuento = () => getSubTotal() + getIVA(); // (SubTotal + IVA)

        const recalcularTotal = () => {
            const subtotal = getSubTotal();
            const iva = getIVA();
            const propina = getVal('txtPropina');
            const descuento = getVal('txtDescuento');

            let total = subtotal + iva + propina - descuento;
            if (total < 0) total = 0;

            setTotalText(total);

            setVal('txtEfectivo', total);
            setVal('txtCambio', 0);
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
                // si hay porcentajes, recalcula valores
                if (getVal('txtPropinaPorcentaje') > 0) propinaDesdePct();
                if (getVal('txtDescuentoPorcentaje') > 0) descuentoDesdePct();
                // si hay valores, recalcula porcentajes (solo si están editando valores)
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

        // ====== Volver ======
        const btnVolver = byId('btnVolver');
        if (btnVolver) {
            btnVolver.addEventListener('click', () => {
                window.location.href = '<%: ResolveUrl("~/Menu.aspx") %>';
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

        const ddl = byId('ddlMedioPago');
        if (ddl) {
            ddl.addEventListener('change', function () {
                const idMedioPago = this.value;
                const nombre = this.options[this.selectedIndex]?.text || '';
                abrirModalInternos(idMedioPago, nombre);
            });
        }

        // ==========================================================
        // ✅ BOTONES DESCUENTO / PROPINA (POSTBACK) - CORREGIDOS
        // ==========================================================

        // Guardar Descuento
        const btnGuardarDescuento = byId('btnGuardarDescuento');
        if (btnGuardarDescuento) {
            btnGuardarDescuento.addEventListener('click', () => {
                const valor = getVal('txtDescuento');
                const pct = getVal('txtDescuentoPorcentaje');

                const razonEl = byId('txtRazonDescuento');
                const razon = (razonEl ? (razonEl.value || '') : '').trim();

                // ✅ obliga razón (SweetAlert2)
                if (!razon) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Falta la razón del descuento',
                        text: 'Debes escribir una razón para poder guardar el descuento.',
                        confirmButtonText: 'Entendido',
                        confirmButtonColor: '#2563eb'
                    }).then(() => {
                        if (razonEl) razonEl.focus();
                    });
                    return;
                }

                // argumento: "valor|pct|razon"
                const arg = `${valor}|${pct}|${encodeURIComponent(razon)}`;
                firePostBack('btnGuardarDescuento', arg);
            });
        }

        // Eliminar Descuento
        const btnEliminarDescuento = byId('btnEliminarDescuento');
        if (btnEliminarDescuento) {
            btnEliminarDescuento.addEventListener('click', () => {
                const razonEl = byId('txtRazonDescuento');
                const pctEl = byId('txtDescuentoPorcentaje');

                setVal('txtDescuento', 0);
                if (pctEl) pctEl.value = '0';
                if (razonEl) razonEl.value = '';

                recalcularTotal();
                firePostBack('btnEliminarDescuento', '0');
            });
        }

        // Guardar Propina
        const btnGuardarPropina = byId('btnGuardarPropina');
        if (btnGuardarPropina) {
            btnGuardarPropina.addEventListener('click', () => {
                const valor = getVal('txtPropina');
                const pct = getVal('txtPropinaPorcentaje');

                const arg = `${valor}|${pct}`;
                firePostBack('btnGuardarPropina', arg);
            });
        }

        // Eliminar Propina
        const btnEliminarPropina = byId('btnEliminarPropina');
        if (btnEliminarPropina) {
            btnEliminarPropina.addEventListener('click', () => {
                const pctEl = byId('txtPropinaPorcentaje');

                setVal('txtPropina', 0);
                if (pctEl) pctEl.value = '0';

                recalcularTotal();
                firePostBack('btnEliminarPropina', '0');
            });
        }

        // ====== Inicializa ======
        // 🔧 Asegura que los valores/porcentajes queden coherentes al cargar
        if (getVal('txtPropinaPorcentaje') > 0) propinaDesdePct();
        else if (getVal('txtPropina') > 0) pctDesdePropina();

        if (getVal('txtDescuentoPorcentaje') > 0) descuentoDesdePct();
        else if (getVal('txtDescuento') > 0) pctDesdeDescuento();

        recalcularTotal();

    })();
</script>

</asp:Content>
