<%@ Page Title="Historial de Ventas" Language="C#" MasterPageFile="~/Menu.Master" AutoEventWireup="true"
    CodeBehind="HVentas.aspx.cs" Inherits="WebApplication.HVentas" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <link href="Content/css/hventas.css" rel="stylesheet" />

    <div class="container-fluid hv-wrap">
        <div class="hv-hero">
            <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
                <div>
                    <div class="hv-pill mb-2">
                        <i class="bi bi-receipt-cutoff"></i>
                        Historial / Ventas
                    </div>
                    <h1 class="hv-title mb-1">Historial de Ventas</h1>
                    <div class="hv-subtitle">
                        Base visual limpia para reconstruir la lógica de ventas desde cero.
                    </div>
                </div>
                <a href="caja.aspx" class="hv-hero-action">
                    <i class="bi bi-shop-window"></i>
                    Caja
                </a>
            </div>

            <div class="row g-3 hv-kpis">
                <div class="col-12 col-md-6 col-xl-2">
                    <div class="hv-kpi">
                        <div class="kpi-top">
                            <div class="kpi-label">Total ventas</div>
                            <div class="kpi-icon"><i class="bi bi-cash-stack"></i></div>
                        </div>
                        <div class="kpi-value"><%: valores.total.ToString("C0") %></div>
                        <div class="hv-muted">Sin lógica conectada</div>
                    </div>
                </div>
                <div class="col-12 col-md-6 col-xl-2">
                    <div class="hv-kpi kpi-success">
                        <div class="kpi-top">
                            <div class="kpi-label">Facturas</div>
                            <div class="kpi-icon"><i class="bi bi-receipt"></i></div>
                        </div>
                        <div class="kpi-value"><%: valores.facturas %></div>
                        <div class="hv-muted">Cantidad</div>
                    </div>
                </div>
                <div class="col-12 col-md-6 col-xl-2">
                    <div class="hv-kpi kpi-warning">
                        <div class="kpi-top">
                            <div class="kpi-label">Propina</div>
                            <div class="kpi-icon"><i class="bi bi-hourglass-split"></i></div>
                        </div>
                        <div class="kpi-value"><%: valores.propina.ToString("C0") %></div>
                        <div class="hv-muted">Valor propina</div>
                    </div>
                </div>
                <div class="col-12 col-md-6 col-xl-2">
                    <div class="hv-kpi kpi-warning">
                        <div class="kpi-top">
                            <div class="kpi-label">Ventas crédito</div>
                            <div class="kpi-icon"><i class="bi bi-credit-card-2-front"></i></div>
                        </div>
                        <div class="kpi-value"><%: valores.ventasCreditoCantidad %></div>
                        <div class="hv-muted">Cantidad del turno</div>
                    </div>
                </div>
                <div class="col-12 col-md-6 col-xl-2">
                    <div class="hv-kpi kpi-warning">
                        <div class="kpi-top">
                            <div class="kpi-label">Valor crédito</div>
                            <div class="kpi-icon"><i class="bi bi-wallet2"></i></div>
                        </div>
                        <div class="kpi-value"><%: valores.ventasCreditoValor.ToString("C0") %></div>
                        <div class="hv-muted">Ventas crédito del turno</div>
                    </div>
                </div>
                <div class="col-12 col-md-6 col-xl-2">
                    <div class="hv-kpi kpi-danger">
                        <div class="kpi-top">
                            <div class="kpi-label">Anuladas</div>
                            <div class="kpi-icon"><i class="bi bi-x-octagon"></i></div>
                        </div>
                        <div class="kpi-value"><%: valores.anuladas %></div>
                        <div class="hv-muted">Registros anulados</div>
                    </div>
                </div>
            </div>
        </div>

        <div class="hv-card mt-3">
            <div class="hv-card-header">
                <h2 class="hv-card-title">
                    <i class="bi bi-funnel"></i>
                    Filtros
                </h2>
                <span class="hv-pill">
                    <i class="bi bi-info-circle"></i>
                    Visual inicial
                </span>
            </div>

            <div class="hv-card-body hv-filter">
                <div class="row g-3">
                    <div class="col-12 col-md-7">
                        <label for="fBuscar">Buscar</label>
                        <div class="input-group">
                            <span class="input-group-text bg-white"><i class="bi bi-search"></i></span>
                            <input type="text" class="form-control" id="fBuscar" placeholder="Factura, cliente, NIT, alias..." />
                        </div>
                    </div>

                    <div class="col-12 col-md-5">
                        <label for="fMedioPago">Medio de pago</label>
                        <select class="form-select" id="fMedioPago">
                            <option value="">Todos</option>
                            <option value="efectivo">Efectivo</option>
                            <option value="tarjeta">Tarjeta</option>
                            <option value="mixto">Mixto</option>
                            <option value="transferencia">Transferencia</option>
                        </select>
                    </div>
                </div>
            </div>
        </div>

        <div class="hv-card mt-3">
            <div class="hv-card-header">
                <h2 class="hv-card-title">
                    <i class="bi bi-table"></i>
                    Facturas
                </h2>

                <div class="d-flex flex-wrap gap-2 align-items-center">
                    <span class="hv-pill"><i class="bi bi-list-ul"></i><span id="hvResultadosLabel">0</span> registros</span>
                    <span class="hv-pill"><i class="bi bi-check2-circle"></i>Estado</span>
                    <span class="hv-pill"><i class="bi bi-qr-code-scan"></i>FE</span>
                </div>
            </div>

            <div class="hv-card-body">
                <div class="hv-table-wrap">
                    <table class="table hv-table table-hover align-middle">
                        <thead>
                            <tr>
                                <th>Cuenta</th>
                                <th>Fecha</th>
                                <th>Factura</th>
                                <th>Cliente</th>
                                <th>Tipo venta</th>
                                <th>Medio</th>
                                <th class="text-end">Total</th>
                                <th>Estado</th>
                                <th>FE</th>
                                <th>Resolución</th>
                                <th class="text-end">Acciones</th>
                            </tr>
                        </thead>
                        <tbody id="hvVentasBody">

                            <asp:Repeater runat="server" ID="rpVentas" ItemType="DAL.Model.V_TablaVentas">
                                <ItemTemplate>
                                    <tr class='hv-venta-row <%# EsVentaAnulada(Item) ? "hv-venta-row-anulada" : "" %>'
                                        data-search="<%# (Item.nombreCliente ?? "") + " " + (Item.nit ?? "") + " " + (Item.prefijo ?? "") + "-" + Item.numeroVenta + " " + (Item.medioDePago ?? "") %>"
                                        data-medio="<%# (Item.medioDePago ?? "").ToLower() %>">

                                        <td><span class="hv-badge gray"><i class="bi bi-hash"></i><%# Item.aliasVenta %></span></td>
                                        <td>
                                            <div class="fw-bold"><%# Item.fechaVenta.ToShortDateString() %></div>
                                            <div class="hv-muted"><%# Item.fechaVenta.ToShortTimeString() %></div>
                                        </td>
                                        <td>
                                            <div class="fw-bold"><%# Item.prefijo %>-<%# Item.numeroVenta %></div>
                                        </td>
                                        <td>
                                            <div class="fw-bold"><%# Item.nombreCliente %></div>
                                            <div class="hv-muted">NIT: <%# Item.nit %></div>
                                        </td>
                                        <td>
                                            <span class='hv-badge <%# Item.idFormaDePago == 2 ? "warning" : "info" %>'>
                                                <i class='bi <%# Item.idFormaDePago == 2 ? "bi-journal-text" : "bi-cash-coin" %>'></i>
                                                <%# string.IsNullOrWhiteSpace(Item.formaDePago) ? (Item.idFormaDePago == 2 ? "Crédito" : "Contado") : Item.formaDePago %>
                                            </span>
                                        </td>
                                        <td><span class="hv-badge info"><i class="bi bi-cash-coin"></i><%# Item.medioDePago %></span></td>
                                        <td class="text-end hv-money"><%# Item.total_A_Pagar.ToString("C0") %></td>
                                        <td>
                                            <span class='hv-badge <%# EsVentaAnulada(Item) ? "danger" : "success" %>'>
                                                <i class='bi <%# EsVentaAnulada(Item) ? "bi-x-circle" : "bi-check2-circle" %>'></i>
                                                <%# Item.estadoVenta %>
                                            </span>
                                        </td>
                                        <td>
                                            <asp:Literal runat="server" Text='<%# ObtenerEstadoBadge(Item.cufe, Item.tipoFactura) %>' />
                                        </td>
                                        <td><span class="hv-badge gray"><i class="bi bi-hash"></i><%# Item.idResolucion %></span></td>
                                        <td class="text-end">
                                            <button type="button" class="btn btn-outline-primary btn-sm hv-linkbtn me-2 btn-ver-venta" data-id="<%# Item.id %>">
                                                <i class="bi bi-eye me-1"></i>Ver
                                            </button>

                                            <button type="button" class="btn btn-outline-success btn-sm hv-linkbtn me-2 btn-editar-resolucion" data-id="<%# Item.id %>">
                                                <i class="bi bi-pencil-square me-1"></i>Editar resolución
                                            </button>

                                            <button type="button" class="btn btn-outline-warning btn-sm hv-linkbtn me-2 btn-editar-cliente" data-id="<%# Item.id %>">
                                                <i class="bi bi-person-gear me-1"></i>Editar cliente
                                            </button>

                                            <button type="button" class="btn btn-outline-success btn-sm hv-linkbtn btn-enviar-dian" data-id="<%# Item.id %>">
                                                <i class="bi bi-cloud-upload me-1"></i>Transmitir a DIAN
                                            </button>

                                            <button type="button"
                                                class='btn btn-outline-info btn-sm hv-linkbtn me-2 btn-reenviar-correo-fe <%# Item.cufe == "--" ? "disabled" : "" %>'
                                                data-id="<%# Item.id %>"
                                                <%# Item.cufe == "--" ? "disabled=\"disabled\"" : "" %>>
                                                <i class="bi bi-envelope-arrow-up me-1"></i>Reenviar correo FE
                                            </button>

                                            <button type="button" class="btn btn-outline-secondary btn-sm hv-linkbtn btn-imprimir-venta" data-id="<%# Item.id %>">
                                                <i class="bi bi-printer me-1"></i>Imprimir
                                            </button>

                                            <a href="#"
                                                class='btn btn-outline-secondary btn-sm hv-linkbtn btn-descargar-pdf <%# Item.cufe == "--" ? "disabled" : "" %>'
                                                data-id="<%# Item.id %>"
                                                data-cufe="<%# Item.cufe %>"
                                                <%# Item.cufe == "--" ? "onclick=\"return false;\"" : "" %>>
                                                <i class="bi bi-file-earmark-pdf me-1"></i>Descargar PDF
                                            </a>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>

                            <tr id="hvSinResultados" style="display: none;">
                                <td colspan="11" class="text-center hv-empty">
                                    <div class="hv-muted">
                                        <i class="bi bi-search d-block mb-2"></i>
                                        No hay coincidencias con los filtros aplicados.
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <%-- Modal Detalle de la venta --%>
    <div class="modal fade" id="mdlVenta" tabindex="-1" aria-hidden="true"
        data-bs-backdrop="static" data-bs-keyboard="false">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content hv-card">
                <div class="hv-card-header">
                    <h3 class="hv-card-title">
                        <i class="bi bi-receipt-cutoff"></i>
                        Detalle de la venta
                    </h3>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="hv-card-body">
                    <div class="row g-3 mb-3">
                        <div class="col-md-3">
                            <div class="hv-muted mb-1">Factura</div>
                            <div class="fw-bold" id="hvDetalleFactura">
                                <%: venta != null ? (venta.prefijo + "-" + venta.numeroVenta) : "" %>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="hv-muted mb-1">Cliente</div>
                            <div class="fw-bold" id="hvDetalleCliente">
                                <%: venta != null ? venta.nombreCliente : "" %>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="hv-muted mb-1">Fecha</div>
                            <div class="fw-bold" id="hvDetalleFecha">
                                <%: venta != null ? venta.fechaVenta.ToShortDateString() + " - " + venta.fechaVenta.ToShortTimeString() : "" %>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="hv-muted mb-1">Total</div>
                            <div class="fw-bold" id="hvDetalleTotal">
                                <%: venta != null ? venta.total_A_Pagar.ToString("C0") : "$ 0" %>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="hv-muted mb-1">NIT</div>
                            <div class="fw-bold" id="hvDetalleNit">
                                <%: venta != null ? venta.nit : "" %>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="hv-muted mb-1">Medio de pago</div>
                            <div class="fw-bold" id="hvDetalleMedio">
                                <%: venta != null ? venta.medioDePago : "" %>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="hv-muted mb-1">Estado</div>
                            <div class="fw-bold" id="hvDetalleEstado">
                                <%: venta != null ? venta.estadoVenta : "" %>
                            </div>
                        </div>
                        <div class="col-12">
                            <div class="alert alert-light border mb-0">
                                <strong>Observación:</strong>
                                <span id="hvDetalleObservacion">
                                    <%: venta != null ? venta.observacionVenta : "" %>
                                </span>
                            </div>
                        </div>
                    </div>

                    <div class="hv-table-wrap">
                        <table class="table hv-table hv-detail-table align-middle mb-0">
                            <thead>
                                <tr>
                                    <th>Producto</th>
                                    <th>Cuenta</th>
                                    <th class="text-end">Cantidad</th>
                                    <th class="text-end">Precio</th>
                                    <th class="text-end">Total</th>
                                    <th>Nota</th>
                                </tr>
                            </thead>
                            <tbody id="hvDetalleBody">
                                <% if (detalleCaja == null || detalleCaja.Count == 0) { %>
                                <tr>
                                    <td colspan="6" class="text-center py-4 hv-muted">No hay datos disponibles</td>
                                </tr>
                                <% } else { %>
                                <asp:Repeater ID="rpDetalleCaja" runat="server" ItemType="DAL.Model.V_DetalleCaja">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Item.nombreProducto %></td>
                                            <td><%# Item.idVenta %></td>
                                            <td class="text-end"><%# Item.unidad.ToString("N0") %></td>
                                            <td class="text-end"><%# Item.precioVenta.ToString("C0") %></td>
                                            <td class="text-end"><%# Item.totalDetalle.ToString("C0") %></td>
                                            <td><%# Item.adiciones %></td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <% } %>
                            </tbody>
                        </table>
                    </div>

                    <div class="d-flex justify-content-end gap-2 mt-3">
                        <button type="button" class="btn btn-outline-secondary rounded-pill px-4" data-bs-dismiss="modal">
                            <i class="bi bi-x-lg me-2"></i>Cerrar
                        </button>
                        <button type="button" class="btn btn-primary rounded-pill px-4" id="hvBtnImprimirVentaActual" data-id="<%: venta.id %>">
                            <i class="bi bi-printer me-2"></i>Imprimir
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- Modal Editar resolución --%>
    <div class="modal fade" id="mdlResolucionVenta" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content hv-card">
                <div class="hv-card-header">
                    <h3 class="hv-card-title">
                        <i class="bi bi-pencil-square"></i>
                        Editar resolución
                    </h3>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="hv-card-body">
                    <div class="mb-3">
                        <label class="form-label hv-muted">Resoluciones disponibles</label>

                        <div class="table-responsive">
                            <table class="table table-hover align-middle">
                                <thead>
                                    <tr>
                                        <th>Id</th>
                                        <th>Nombre resolución</th>
                                        <th>Número resolución</th>
                                        <th class="text-end">Acción</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <% if (listaResoluciones == null || listaResoluciones.Count == 0) { %>
                                    <tr>
                                        <td colspan="4" class="text-center py-4 text-muted">No hay resoluciones disponibles</td>
                                    </tr>
                                    <% } else { %>
                                    <asp:Repeater ID="rpResoluciones" runat="server" ItemType="DAL.Model.V_Resoluciones">
                                        <ItemTemplate>
                                            <tr>
                                                <td><%# Item.idResolucion %></td>
                                                <td><%# Item.nombreRosolucion %></td>
                                                <td><%# Item.numeroResolucion %></td>
                                                <td class="text-end">
                                                    <button
                                                        data-idventa="<%: venta.id %>"
                                                        type="button"
                                                        class="btn btn-outline-primary btn-sm btn-seleccionar-resolucion"
                                                        data-id="<%# Item.idResolucion %>"
                                                        data-nombre="<%# Item.nombreRosolucion %>"
                                                        data-numero="<%# Item.numeroResolucion %>">
                                                        Seleccionar
                                                    </button>
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                    <% } %>
                                </tbody>
                            </table>
                        </div>
                    </div>

                    <input type="hidden" id="hvResolucionSeleccionadaId" />
                    <input type="hidden" id="hvResolucionSeleccionadaNombre" />
                    <input type="hidden" id="hvResolucionSeleccionadaNumero" />

                    <div class="d-flex justify-content-end gap-2 mt-3">
                        <button type="button" class="btn btn-outline-secondary rounded-pill px-4" data-bs-dismiss="modal">
                            <i class="bi bi-x-lg me-2"></i>Cancelar
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- Modal Gestionar Cliente --%>
    <div class="modal fade cliente-modal" id="mdlClienteVenta" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-xl">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title"><i class="bi bi-people-fill"></i>Gestionar Cliente</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <div class="toolbar mb-3">
                        <div class="row g-2 align-items-end">
                            <div class="col-12 col-lg-7">
                                <label class="form-label">Filtrar cliente por nombre</label>
                                <input type="text" class="form-control" id="hvFiltroClienteNombre" placeholder="Buscar por nombre o razón social" />
                            </div>
                            <div class="col-12 col-lg-3">
                                <label class="form-label">Buscar documento</label>
                                <input type="text" class="form-control" id="hvBuscarDocumentoCliente" placeholder="Número de identificación" />
                            </div>
                            <div class="col-12 col-lg-2 d-grid">
                                <button type="button" class="btn btn-ghost" id="hvBtnBuscarClienteDocumento">
                                    <i class="bi bi-search me-1"></i>Buscar
                                </button>
                            </div>
                        </div>
                    </div>

                    <div class="surface mb-3">
                        <div class="row g-2">
                            <div class="col-12 col-md-3">
                                <label class="form-label">Tipo documento *</label>
                                <asp:DropDownList
                                    ID="ddlTipoDocumentoHv"
                                    runat="server"
                                    ClientIDMode="Static"
                                    CssClass="form-select">
                                </asp:DropDownList>
                            </div>

                            <div class="col-12 col-md-3">
                                <label class="form-label">Número de identificación *</label>
                                <input type="text" class="form-control" id="hvTxtIdentificacionCliente" />
                            </div>

                            <div class="col-12 col-md-3">
                                <label class="form-label">Tipo organización *</label>
                                <asp:DropDownList
                                    ID="ddlTipoOrganizacionHv"
                                    runat="server"
                                    ClientIDMode="Static"
                                    CssClass="form-select">
                                </asp:DropDownList>
                            </div>

                            <div class="col-12 col-md-3">
                                <label class="form-label">Municipio *</label>
                                <asp:DropDownList
                                    ID="ddlMunicipioHv"
                                    runat="server"
                                    ClientIDMode="Static"
                                    CssClass="form-select">
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="row g-2 mt-1">
                            <div class="col-12 col-md-3">
                                <label class="form-label">Tipo régimen *</label>
                                <asp:DropDownList
                                    ID="ddlTipoRegimenHv"
                                    runat="server"
                                    ClientIDMode="Static"
                                    CssClass="form-select">
                                </asp:DropDownList>
                            </div>

                            <div class="col-12 col-md-3">
                                <label class="form-label">Tipo responsabilidad *</label>
                                <asp:DropDownList
                                    ID="ddlTipoResponsabilidadHv"
                                    runat="server"
                                    ClientIDMode="Static"
                                    CssClass="form-select">
                                </asp:DropDownList>
                            </div>

                            <div class="col-12 col-md-6">
                                <label class="form-label">Detalle de impuesto *</label>
                                <asp:DropDownList
                                    ID="ddlDetalleImpuestoHv"
                                    runat="server"
                                    ClientIDMode="Static"
                                    CssClass="form-select">
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="row g-2 mt-1">
                            <div class="col-12 col-md-6">
                                <label class="form-label">Nombre o razón social *</label>
                                <input type="text" class="form-control" id="hvTxtNombreCliente" />
                            </div>
                            <div class="col-12 col-md-6">
                                <label class="form-label">Nombre comercio</label>
                                <input type="text" class="form-control" id="hvTxtNombreComercioCliente" />
                            </div>
                        </div>

                        <div class="row g-2 mt-1">
                            <div class="col-12 col-md-2">
                                <label class="form-label">Teléfono *</label>
                                <input type="text" class="form-control" id="hvTxtTelefonoCliente" />
                            </div>
                            <div class="col-12 col-md-4">
                                <label class="form-label">Dirección *</label>
                                <input type="text" class="form-control" id="hvTxtDireccionCliente" />
                            </div>
                            <div class="col-12 col-md-6">
                                <label class="form-label">Correo *</label>
                                <input type="email" class="form-control" id="hvTxtCorreoCliente" />
                            </div>
                        </div>

                        <div class="row g-2 mt-1 align-items-end">
                            <div class="col-12 col-md-2">
                                <label class="form-label">Matrícula mercantil</label>
                                <input type="text" class="form-control" id="hvTxtMatriculaCliente" />
                            </div>

                            <div class="col-12 col-md-4">
                                <div class="d-flex gap-3 align-items-center pt-2">
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="hvChkClientesModal" checked />
                                        <label class="form-check-label" for="hvChkClientesModal">Clientes</label>
                                    </div>
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="hvChkProveedoresModal" />
                                        <label class="form-check-label" for="hvChkProveedoresModal">Proveedores</label>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12 col-md-6">
                                <div class="d-flex justify-content-end gap-2 flex-wrap">
                                    <button type="button" class="btn btn-ghost" id="hvBtnGuardarClienteCatalogo">
                                        <i class="bi bi-floppy2-fill me-1"></i><span id="hvLblGuardarCliente">Guardar</span>
                                    </button>
                                    <button type="button" class="btn btn-outline-ghost" id="hvBtnLimpiarClienteFormulario">
                                        <i class="bi bi-brush me-1"></i>Limpiar
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="d-flex justify-content-center gap-2 mt-3">
                        <button type="button" id="hvBtnSeleccionarCliente" class="btn btn-outline-ghost px-4" disabled>
                            <i class="bi bi-check2-square me-1"></i>Seleccionar cliente
                        </button>

                        <button type="button" class="btn btn-outline-secondary rounded-pill px-4" data-bs-dismiss="modal">
                            <i class="bi bi-x-lg me-2"></i>Cancelar
                        </button>
                    </div>

                    <input type="hidden" id="hvTxtIdCliente" class="hv_TxtIdCliente" />

                    <div class="grid-shell">
                        <div class="table-responsive">
                            <table class="table tabla-clientes mb-0">
                                <thead>
                                    <tr>
                                        <th style="width: 18%">Documento</th>
                                        <th style="width: 18%">NIT</th>
                                        <th style="width: 39%">Nombre cliente</th>
                                        <th style="width: 25%">Correo</th>
                                    </tr>
                                </thead>
                                <tbody id="hvClientesBody">

                                    <% if (listaClientes.Count == 0) { %>
                                    <tr>
                                        <td colspan="4" class="text-center py-4 text-muted">No hay clientes disponibles</td>
                                    </tr>
                                    <% } else { %>

                                    <asp:Repeater runat="server" ID="rpClientes" ItemType="DAL.Model.Clientes">
                                        <ItemTemplate>
                                            <tr class="cliente-row" tabindex="0"
                                                data-nit='<%# HttpUtility.HtmlAttributeEncode((Item.identificationNumber ?? "").ToString()) %>'
                                                data-nombre='<%# HttpUtility.HtmlAttributeEncode(Item.nameCliente ?? "") %>'
                                                data-cliente='<%# HttpUtility.HtmlAttributeEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Item)) %>'>
                                                <td><%# Item.typeDocumentIdentification_id %></td>
                                                <td><%# Item.identificationNumber %></td>
                                                <td><%# Item.nameCliente %></td>
                                                <td><%# Item.email %></td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>

                                    <% } %>
                                </tbody>
                            </table>
                        </div>
                    </div>

                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="mdlCorreoFacturaFe" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content hv-card">
                <div class="hv-card-header">
                    <h3 class="hv-card-title">
                        <i class="bi bi-envelope-paper"></i>
                        Confirmar envío de factura electrónica
                    </h3>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="hv-card-body">
                    <input type="hidden" id="hvCorreoFacturaVentaId" value="<%: correoFacturaPreview.VentaId %>" />

                    <div class="alert alert-light border">
                        <div><strong>Factura:</strong> <%: correoFacturaPreview.NumeroFactura %></div>
                        <div><strong>Cliente:</strong> <%: correoFacturaPreview.ClienteNombre %></div>
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Correo principal del cliente</label>
                        <input type="email" class="form-control" id="hvCorreoPrincipalFactura" value="<%: correoFacturaPreview.CorreoPrincipal %>" placeholder="cliente@correo.com" />
                    </div>

                    <div class="mb-2 d-flex justify-content-between align-items-center">
                        <label class="form-label mb-0">Correos adicionales</label>
                        <button type="button" class="btn btn-outline-primary btn-sm" id="hvBtnAgregarCorreoFactura">
                            <i class="bi bi-plus-lg me-1"></i>Agregar correo
                        </button>
                    </div>

                    <div id="hvCorreosFacturaLista" class="d-grid gap-2">
                        <% if (correoFacturaPreview.CorreosAdicionales == null || correoFacturaPreview.CorreosAdicionales.Count == 0) { %>
                        <div class="input-group hv-correo-fila">
                            <input type="email" class="form-control hv-correo-adicional" placeholder="correo.adicional@dominio.com" />
                            <button type="button" class="btn btn-outline-danger hv-btn-eliminar-correo">
                                <i class="bi bi-trash"></i>
                            </button>
                        </div>
                        <% } else { %>
                        <% foreach (var correoItem in correoFacturaPreview.CorreosAdicionales) { %>
                        <div class="input-group hv-correo-fila">
                            <input type="email" class="form-control hv-correo-adicional" value="<%: correoItem %>" placeholder="correo.adicional@dominio.com" />
                            <button type="button" class="btn btn-outline-danger hv-btn-eliminar-correo">
                                <i class="bi bi-trash"></i>
                            </button>
                        </div>
                        <% } %>
                        <% } %>
                    </div>

                    <div class="d-flex justify-content-end gap-2 mt-3">
                        <button type="button" class="btn btn-outline-secondary rounded-pill px-4" data-bs-dismiss="modal">
                            <i class="bi bi-x-lg me-2"></i>Cancelar
                        </button>
                        <button type="button" class="btn btn-primary rounded-pill px-4" id="hvBtnConfirmarEnvioCorreoFactura">
                            <i class="bi bi-send me-2"></i>Enviar factura
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/jspdf@2.5.1/dist/jspdf.umd.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/jspdf-autotable@3.8.2/dist/jspdf.plugin.autotable.min.js"></script>
    <script src="Scripts/js/hventas.actions.js"></script>
    <script src="Scripts/js/hventas.pdf.js"></script>
    <script src="Scripts/js/hventas.js"></script>

    <% if (_mdlVenta) { %>
    <script>
        window.addEventListener('load', function () {
            var modalEl = document.getElementById('mdlVenta');
            if (!modalEl || !window.bootstrap) return;

            var modal = bootstrap.Modal.getOrCreateInstance(modalEl, {
                backdrop: 'static',
                keyboard: false
            });

            modal.show();
        });
    </script>
    <% } %>

    <% if (_mdlResolucionVenta) { %>
    <script>
        window.addEventListener('load', function () {
            var modalEl = document.getElementById('mdlResolucionVenta');
            if (!modalEl || !window.bootstrap) return;

            var modal = bootstrap.Modal.getOrCreateInstance(modalEl, {
                backdrop: 'static',
                keyboard: false
            });

            modal.show();
        });
    </script>
    <% } %>

    <% if (_mdlClienteVenta) { %>
    <script>
        window.addEventListener('load', function () {
            var modalEl = document.getElementById('mdlClienteVenta');
            if (!modalEl || !window.bootstrap) return;

            var modal = bootstrap.Modal.getOrCreateInstance(modalEl, {
                backdrop: 'static',
                keyboard: false
            });

            modal.show();
        });
    </script>
    <% } %>

    <% if (_mdlCorreoFactura) { %>
    <script>
        window.addEventListener('load', function () {
            var modalEl = document.getElementById('mdlCorreoFacturaFe');
            if (!modalEl || !window.bootstrap) return;

            var modal = bootstrap.Modal.getOrCreateInstance(modalEl, {
                backdrop: 'static',
                keyboard: false
            });

            modal.show();
        });
    </script>
    <% } %>


    <div id="loader-imprimir" class="loader-overlay" style="display: none;">
        <div class="loader-box">
            <div class="loader-circular"></div>
            <p>Procesando impresión...</p>
        </div>
    </div>


</asp:Content>
