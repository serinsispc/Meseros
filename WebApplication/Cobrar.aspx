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
                            <input class="form-check-input" type="checkbox" id="swFE" />
                        <% } %>
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
                    <div class="col-12 col-md-4">
                        <label class="form-label fw-bold mb-1">Abono Efectivo</label>
                        <input type="text" class="form-control" id="txtAbonoEfectivo" runat="server" clientidmode="Static" value="0" readonly />
                        <div class="hint mt-1">Saldo pago efectivo</div>
                    </div>
                    <div class="col-12 col-md-4">
                        <label class="form-label fw-bold mb-1">Efectivo</label>
                        <input type="text" class="form-control" id="txtEfectivo" runat="server" ClientIDMode="Static" value="0" />
                        <div class="hint mt-1">Se auto completa igual al Total</div>
                    </div>
                    <div class="col-12 col-md-4">
                        <label class="form-label fw-bold mb-1">Cambio</label>
                        <input type="text" class="form-control" id="txtCambio" runat="server" ClientIDMode="Static" value="0" readonly />
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

    <!-- ✅ Config desde servidor (NO mover a .js externo) -->
    <script>
        window.CobrarConfig = {
            urlMenu: '<%: ResolveUrl("~/Menu.aspx") %>'
        };
    </script>

    <!-- ✅ TU JS EXTERNO -->
    <script src="<%: ResolveUrl("~/Scripts/js/cobrar.js") %>"></script>

</asp:Content>
