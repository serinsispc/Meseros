<%@ Page Title="Cobro Caja" Language="C#" MasterPageFile="~/CobrarMaster.Master" AutoEventWireup="true"
    CodeBehind="Cobrar.aspx.cs" Inherits="WebApplication.Cobrar"
    MaintainScrollPositionOnPostback="false" Async="true"%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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

            <div class="d-flex align-items-center gap-2 topbar-actions">
                <button type="button" class="btn btnx px-3 w-100 w-sm-auto" id="btnVolver">
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
                                       runat="server" ClientIDMode="Static" />
                                <div class="hint mt-1">Valor descuento</div>

                                <!-- ✅ Botones DESCUENTO -->
                                <div class="d-flex gap-2 mt-2 flex-wrap flex-sm-nowrap">
                                    <button type="button" class="btn btnx btn-blue flex-fill" id="btnGuardarDescuento">
                                        <i class="bi bi-check2-circle"></i> Guardar descuento
                                    </button>
                                    <button type="button" class="btn btnx flex-fill" id="btnEliminarDescuento">
                                        <i class="bi bi-trash3"></i> Eliminar
                                    </button>
                                </div>
                            </div>

                            <div class="col-12">
                                <label class="form-label fw-bold mb-1 mt-2">Razon Descuento</label>
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
                                <div class="d-flex gap-2 mt-2 flex-wrap flex-sm-nowrap">
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
                                        <%
                                            if (Session["cliente_seleccionado_nit"] != null)
                                            {
                %>
<input class="form-check-input" type="checkbox" id="swFE" checked="checked" />
                <%
                    }
                    else
                    {
                %>
                        <input class="form-check-input" type="checkbox" id="swFE"/>
                        <%} %>
                        
                        
                    </div>
                </div>

               <%-- Información del cliente seleccionado --%>
                <%
                    if (Session["cliente_seleccionado_nit"] != null)
                    {
                %>
                <div class="d-flex align-items-center justify-content-between border rounded-3 p-3 mb-3" style="border-color: var(--border) !important;">
                    <div>
                        <div class="fw-bold">Cliente seleccionado</div>
                        <div class="hint">NIT: <asp:Label ID="cliente_seleccionado_nit" runat="server"></asp:Label></div>
                        <div class="hint">Nombre: <asp:Label ID="cliente_seleccionado_nombre" runat="server"></asp:Label></div>
                        <div class="hint">Email: <asp:Label ID="cliente_seleccionado_correo" runat="server"></asp:Label></div>
                    </div>
                </div>
                <%
                    }
                %>


                <!-- Efectivo / Cambio -->
                <div class="row g-2 mb-3">
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-bold mb-1">Efectivo</label>
                        <input type="text" class="form-control" id="txtEfectivo" runat="server" ClientIDMode="Static" value="0" />
                        <div class="hint mt-1">Se auto completa igual al Total</div>
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
    <div class="modal fade cliente-modal" id="mdlCliente" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-xl">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title"><i class="bi bi-people-fill"></i> Gestionar Cliente</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <div class="toolbar mb-3">
                        <div class="row g-2 align-items-end">
                            <div class="col-12 col-lg-7">
                                <label class="form-label">Filtrar cliente por Nombre</label>
                                <input type="text" class="form-control" id="txtFiltroClienteNombre" placeholder="Buscar por nombre o razón social" />
                            </div>
                            <div class="col-12 col-lg-3">
                                <label class="form-label">Buscar Documento</label>
                                <input type="text" class="form-control" id="txtBuscarNIT" runat="server" ClientIDMode="Static"
                                       placeholder="Número de identificación" />
                            </div>
                            <div class="col-12 col-lg-2 d-grid">
                                <button type="button" class="btn btn-ghost" id="btnBuscarNIT">
                                    <i class="bi bi-search me-1"></i> Buscar
                                </button>
                            </div>
                        </div>
                    </div>

                    <div class="surface mb-3">
                        <div class="row g-2">
                            <div class="col-12 col-md-3">
                                <label class="form-label">Tipo Documento *</label>
                                <asp:DropDownList ID="ddlTipoDocumento" runat="server" CssClass="form-select" AppendDataBoundItems="true" ClientIDMode="Static">
                                    <asp:ListItem Text="Seleccionar tipo" Value=""></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-12 col-md-3">
                                <label class="form-label">Número de identificación *</label>
                                <input type="text" class="form-control" id="txtIdentificacionCliente" />
                            </div>
                            <div class="col-12 col-md-3">
                                <label class="form-label">Tipo Organización *</label>
                                <asp:DropDownList ID="ddlTipoOrganizacion" runat="server" CssClass="form-select" AppendDataBoundItems="true" ClientIDMode="Static">
                                    <asp:ListItem Text="Seleccionar tipo" Value=""></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-12 col-md-3">
                                <label class="form-label">Municipio *</label>
                                <asp:DropDownList ID="ddlMunicipio" runat="server" CssClass="form-select" AppendDataBoundItems="true" ClientIDMode="Static">
                                    <asp:ListItem Text="Seleccionar municipio" Value=""></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="row g-2 mt-1">
                            <div class="col-12 col-md-3">
                                <label class="form-label">Tipo Régimen *</label>
                                <asp:DropDownList ID="ddlTipoRegimen" runat="server" CssClass="form-select" AppendDataBoundItems="true" ClientIDMode="Static">
                                    <asp:ListItem Text="Seleccionar régimen" Value=""></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-12 col-md-3">
                                <label class="form-label">Tipo de responsabilidad *</label>
                                <asp:DropDownList ID="ddlTipoResponsabilidad" runat="server" CssClass="form-select" AppendDataBoundItems="true" ClientIDMode="Static">
                                    <asp:ListItem Text="Seleccionar responsabilidad" Value=""></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-12 col-md-6">
                                <label class="form-label">Detalle de Impuesto *</label>
                                <asp:DropDownList ID="ddlDetalleImpuesto" runat="server" CssClass="form-select" AppendDataBoundItems="true" ClientIDMode="Static">
                                    <asp:ListItem Text="Seleccionar impuesto" Value=""></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="row g-2 mt-1">
                            <div class="col-12 col-md-6">
                                <label class="form-label">Nombre o razón social del empresa *</label>
                                <input type="text" class="form-control" id="txtNombreRazonCliente" />
                            </div>
                            <div class="col-12 col-md-6">
                                <label class="form-label">Nombre comercio</label>
                                <input type="text" class="form-control" id="txtNombreComercioCliente" />
                            </div>
                        </div>

                        <div class="row g-2 mt-1">
                            <div class="col-12 col-md-2">
                                <label class="form-label">Teléfono *</label>
                                <input type="text" class="form-control" id="txtTelefonoCliente" />
                            </div>
                            <div class="col-12 col-md-4">
                                <label class="form-label">Dirección *</label>
                                <input type="text" class="form-control" id="txtDireccionCliente" />
                            </div>
                            <div class="col-12 col-md-6">
                                <label class="form-label">Correo *</label>
                                <input type="email" class="form-control" id="txtCorreoCliente" />
                            </div>
                        </div>

                        <div class="row g-2 mt-1 align-items-end">
                            <div class="col-12 col-md-2">
                                <label class="form-label">Matrícula mercantil</label>
                                <input type="text" class="form-control" id="txtMatriculaCliente" />
                            </div>
                            <div class="col-12 col-md-4">
                                <div class="d-flex gap-3 align-items-center pt-2">
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="chkClientesModal" checked />
                                        <label class="form-check-label" for="chkClientesModal">Clientes</label>
                                    </div>
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="chkProveedoresModal" />
                                        <label class="form-check-label" for="chkProveedoresModal">Proveedores</label>
                                    </div>
                                </div>
                            </div>
                            <div class="col-12 col-md-6">
                                <div class="d-flex justify-content-end gap-2 flex-wrap">
                                <button type="button" class="btn btn-ghost" id="btnGuardarCliente">
                                    <i class="bi bi-floppy2-fill me-1"></i> <span id="lblGuardarCliente">Guardar</span>
                                </button>
                                    <button type="button" class="btn btn-outline-ghost">
                                        <i class="bi bi-brush me-1"></i> Limpiar
                                    </button>
                                    <button type="button" class="btn btn-outline-ghost">
                                        <i class="bi bi-envelope-at me-1"></i> Correos
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="grid-shell">
                        <div class="table-responsive">
                            <table class="table tabla-clientes mb-0">
                                <thead>
                                    <tr>
                                        <th style="width:16%">Tipo Documento</th>
                                        <th style="width:12%">NIT</th>
                                        <th style="width:42%">Nombre Cliente</th>
                                        <th style="width:30%">Correo</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptClientesModal" runat="server">
                                        <ItemTemplate>
                                            <tr class="cliente-row" tabindex="0"
                                                data-cliente-id='<%# Eval("ClienteId") %>'
                                                data-type-doc-id='<%# Eval("TipoDocumentoId") %>'
                                                data-nit='<%# HttpUtility.HtmlAttributeEncode(Eval("Nit")?.ToString() ?? "") %>'
                                                data-nombre='<%# HttpUtility.HtmlAttributeEncode(Eval("NombreCliente")?.ToString() ?? "") %>'
                                                data-correo='<%# HttpUtility.HtmlAttributeEncode(Eval("Correo")?.ToString() ?? "") %>'
                                                data-org-id='<%# Eval("TipoOrganizacionId") %>'
                                                data-municipio-id='<%# Eval("MunicipioId") %>'
                                                data-regimen-id='<%# Eval("TipoRegimenId") %>'
                                                data-responsabilidad-id='<%# Eval("TipoResponsabilidadId") %>'
                                                data-impuesto-id='<%# Eval("DetalleImpuestoId") %>'
                                                data-comercio='<%# HttpUtility.HtmlAttributeEncode(Eval("NombreComercio")?.ToString() ?? "") %>'
                                                data-telefono='<%# HttpUtility.HtmlAttributeEncode(Eval("Telefono")?.ToString() ?? "") %>'
                                                data-direccion='<%# HttpUtility.HtmlAttributeEncode(Eval("Direccion")?.ToString() ?? "") %>'
                                                data-matricula='<%# HttpUtility.HtmlAttributeEncode(Eval("MatriculaMercantil")?.ToString() ?? "") %>'>
                                                <td><%# Eval("TipoDocumento") %></td>
                                                <td><%# Eval("Nit") %></td>
                                                <td><%# Eval("NombreCliente") %></td>
                                                <td><%# Eval("Correo") %></td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                        </div>
                    </div>
                    <div class="grid-footer mt-2"></div>
                    <div class="text-center mt-3">
                        <button type="button" id="btnSeleccionarCliente" class="btn btnx px-4" disabled>
                            <i class="bi bi-check2-square me-1"></i> Seleccionar cliente
                        </button>
                    </div>
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


    <!-- ==================== MODAL PAGO MIXTO ==================== -->
<div class="modal fade" id="mdlPagoMixto" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered modal-lg">
    <div class="modal-content">

      <div class="modal-header">
        <h5 class="modal-title">Pagos Internos</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
      </div>

      <div class="modal-body">
        <div class="d-flex justify-content-between align-items-center mb-2">
          <div class="fw-semibold" id="lblResumenMixto">Total: $0 - Pagado: $0 - Efectivo: $0</div>
        </div>

        <div class="table-responsive">
          <table class="table table-bordered align-middle mb-0">
            <thead class="table-light">
              <tr>
                <th style="width:60%">Medio interno</th>
                <th style="width:40%">Valor</th>
              </tr>
            </thead>
            <tbody id="tblMixtoBody">
              <!-- filas dinámicas -->
            </tbody>
          </table>
        </div>

        <div class="small text-muted mt-2">
          Escribe el valor en cada medio. Se suma automáticamente.
        </div>
      </div>

      <div class="modal-footer">
        <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cerrar</button>
        <button type="button" class="btn btn-primary" id="btnConfirmarPagoMixto">
          <i class="bi bi-calculator"></i> Agregar Pagos
        </button>
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
                // ✅ IMPORTANTE: en WebForms NO se puede escribir "<form>" literal dentro de scripts
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

        // ==========================================================
        // ✅ NUEVO: saldo en efectivo para "Mixto"
        // ==========================================================
        let sumMixtoInternos = 0; // suma digitada en el modal mixto
        const esMixtoPorNombre = (nombre) => ((nombre || '').trim().toLowerCase() === 'mixto');

        const aplicarReglaEfectivoPorMedio = () => {
            const ddl_ = byId('ddlMedioPago');
            const nombre_ = ddl_ ? (ddl_.options[ddl_.selectedIndex]?.text || '') : '';

            if (!esMixtoPorNombre(nombre_)) {
                // ✅ NO mixto => efectivo 0 y cambio 0
                setVal('txtEfectivo', 0);
                setVal('txtCambio', 0);
                return;
            }

            // ✅ mixto => efectivo = saldo
            const total = getTotalActual();
            let saldo = total - (sumMixtoInternos || 0);
            if (saldo < 0) saldo = 0;

            setVal('txtEfectivo', saldo);
            setVal('txtCambio', 0);
        };

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

            // ✅ efectivo/cambio dependen del medio de pago
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

        // ==========================================================
        // ✅ MODAL PAGO MIXTO (ROBUSTO)
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
                if (lbl) lbl.textContent = `Pago Mixto - Total ingresado: ${formatCOP(sumMixtoInternos)}`;
                aplicarReglaEfectivoPorMedio();
            };

            // ✅ Listener delegado
            modalEl.oninput = (e) => {
                const t = e.target;
                if (t && t.tagName === 'INPUT') recalcularResumenMixto();
            };
            modalEl.onchange = (e) => {
                const t = e.target;
                if (t && t.tagName === 'INPUT') recalcularResumenMixto();
            };

            // ✅ Al cerrar modal, recalcula por si hubo cambios
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

                const payload = { idMetodoPago: idMetodoPago, pagos: pagos };
                const arg = encodeB64(JSON.stringify(payload));
                firePostBack('btnGuardarPagoMixto', arg);

                bootstrap.Modal.getOrCreateInstance(modalEl).hide();
            };

            // ✅ Al abrir mixto: suma 0 => efectivo = total
            sumMixtoInternos = 0;
            aplicarReglaEfectivoPorMedio();

            recalcularResumenMixto();
            bootstrap.Modal.getOrCreateInstance(modalEl, { backdrop: 'static' }).show();
        };

        // ==========================================================
        // ✅ Cambio de medio de pago (Mixto vs normal)
        // ==========================================================
        const ddl = byId('ddlMedioPago');
        if (ddl) {
            ddl.addEventListener('change', function () {
                const idMedioPago = this.value;
                const nombre = this.options[this.selectedIndex]?.text || '';

                sumMixtoInternos = 0;

                if (esMixtoPorNombre(nombre)) {
                    aplicarReglaEfectivoPorMedio();
                    abrirModalMixto(idMedioPago, nombre);
                    return;
                }

                aplicarReglaEfectivoPorMedio();
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
        const clienteTable = document.querySelector('#mdlCliente table');
        if (clienteTable) {
            const modalClienteEl = byId('mdlCliente');
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

            if (modalClienteEl) {
                modalClienteEl.addEventListener('shown.bs.modal', () => {
                    const ddlTipoDocumento = byId('ddlTipoDocumento');
                    if (!ddlTipoDocumento) return;

                    const nitOption = Array.from(ddlTipoDocumento.options)
                        .find(opt => (opt.textContent || '').trim().toUpperCase() === 'NIT');

                    if (nitOption) {
                        ddlTipoDocumento.value = nitOption.value;
                    }
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

                if (data.actionLabel) {
                    setGuardarLabel(data.actionLabel);
                }
            };

            window.setClienteData = (data) => {
                aplicarDatosCliente(data);
            };

            const seleccionarFila = (row) => {
                if (!row) return;

                clienteTable.querySelectorAll('.cliente-row').forEach(r => r.classList.remove('selected'));
                row.classList.add('selected');
                row.focus();
                if (btnSeleccionarCliente) {
                    btnSeleccionarCliente.disabled = !row.dataset.clienteId;
                }

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
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });

                firePostBack('btnBuscarNIT', nit);
            });
        }

        if (getVal('txtPropinaPorcentaje') > 0) propinaDesdePct();
        else if (getVal('txtPropina') > 0) pctDesdePropina();

        if (getVal('txtDescuentoPorcentaje') > 0) descuentoDesdePct();
        else if (getVal('txtDescuento') > 0) pctDesdeDescuento();

        recalcularTotal();

    })();
</script>






</asp:Content>
