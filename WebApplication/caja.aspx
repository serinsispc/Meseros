<%@ Page Title="" Language="C#" MasterPageFile="~/CajaMaster.Master" AutoEventWireup="true" CodeBehind="caja.aspx.cs" Inherits="WebApplication.caja" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
        <style>
        :root {
            --serinsis-blue: #0b3a7e;
            --serinsis-blue2: #1e88ff;
        }

        .btn-eliminar-servicio-disabled {
            background: #e5e7eb !important;
            color: #6b7280 !important;
            border: 1px solid #cbd5e1 !important;
            box-shadow: none !important;
            cursor: not-allowed !important;
        }

        .btn-eliminar-servicio-disabled i {
            color: #6b7280 !important;
        }

        .cuenta-item.cuenta-item-activa,
        .btn-cuenta-general.cuenta-item-activa {
            border-color: rgba(30, 136, 255, .42) !important;
            background: linear-gradient(135deg, rgba(11,58,126,.08), rgba(30,136,255,.18)) !important;
            color: #0b3a7e !important;
            box-shadow: 0 14px 28px rgba(30, 136, 255, .18) !important;
            transform: translateY(-2px);
        }

        .cuenta-item small {
            display: block;
            margin-top: 4px;
            font-weight: 700;
            color: rgba(15, 23, 42, .56);
        }

        .detalle-empty {
            padding: 28px 18px;
            border: 1px dashed rgba(148, 163, 184, .8);
            border-radius: 18px;
            background: rgba(248, 250, 252, .9);
            text-align: center;
            color: rgba(15, 23, 42, .68);
            font-weight: 700;
        }

        .detalle-empty i {
            font-size: 2rem;
            color: #94a3b8;
            display: block;
            margin-bottom: 8px;
        }

        .prod-actions-detalle .act-btn[disabled] {
            opacity: .45;
            cursor: not-allowed;
        }

        .cuenta-detalle-chip {
            display: inline-flex;
            align-items: center;
            gap: 6px;
            margin-top: 10px;
            padding: 6px 10px;
            border-radius: 999px;
            background: rgba(37, 99, 235, .08);
            border: 1px solid rgba(37, 99, 235, .14);
            color: #1d4ed8;
            font-size: .78rem;
            font-weight: 800;
        }

        .app-loading {
            position: fixed;
            inset: 0;
            z-index: 999999;
            align-items: center;
            justify-content: center;
            background: rgba(10, 22, 45, .38);
            backdrop-filter: blur(6px);
            -webkit-backdrop-filter: blur(6px);
        }

        .app-loading-card {
            width: min(360px, calc(100% - 32px));
            background: rgba(255,255,255,.92);
            border: 1px solid rgba(255,255,255,.55);
            border-radius: 18px;
            box-shadow: 0 18px 45px rgba(0,0,0,.20);
            padding: 18px 18px 16px;
            text-align: center;
        }

        .app-spinner {
            width: 56px;
            height: 56px;
            border-radius: 50%;
            border: 5px solid rgba(30, 136, 255, .22);
            border-top-color: var(--serinsis-blue2);
            border-right-color: var(--serinsis-blue);
            animation: appSpin .9s linear infinite;
            margin: 8px auto 10px;
        }

        .app-loading-title {
            font-weight: 800;
            color: #0f172a;
            font-size: 1.05rem;
        }

        .app-loading-sub {
            color: rgba(15,23,42,.70);
            font-size: .92rem;
            margin-top: 4px;
        }

        @keyframes appSpin {
            to {
                transform: rotate(360deg);
            }
        }
        .ventas-vista {
            background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(248,250,252,.98));
            border: 1px solid rgba(148,163,184,.18);
            border-radius: 24px;
            padding: 18px;
            box-shadow: 0 20px 40px rgba(15,23,42,.08);
        }

        .ventas-vista__hero {
            background: linear-gradient(135deg, rgba(37,99,235,.10), rgba(14,165,233,.08));
            border: 1px solid rgba(37,99,235,.14);
            border-radius: 20px;
            padding: 18px;
        }

        .ventas-vista__title {
            font-size: 1.45rem;
            font-weight: 800;
            color: #0f172a;
            margin: 0;
        }

        .ventas-vista__sub {
            color: rgba(15,23,42,.68);
            margin-top: 4px;
            font-weight: 600;
        }

        .ventas-kpi {
            background: #fff;
            border: 1px solid rgba(148,163,184,.18);
            border-radius: 18px;
            padding: 14px;
            box-shadow: 0 12px 24px rgba(15,23,42,.06);
            height: 100%;
        }

        .ventas-kpi__label {
            color: rgba(15,23,42,.60);
            font-size: .82rem;
            font-weight: 800;
        }

        .ventas-kpi__value {
            color: #0f172a;
            font-size: 1.35rem;
            font-weight: 900;
            margin-top: 6px;
        }

        .ventas-kpi__muted {
            color: rgba(15,23,42,.56);
            font-weight: 700;
            margin-top: 4px;
            font-size: .84rem;
        }

        .ventas-tabla-wrap {
            margin-top: 14px;
            background: #fff;
            border: 1px solid rgba(148,163,184,.18);
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 14px 28px rgba(15,23,42,.06);
        }

        .ventas-tabla-head {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 12px;
            padding: 14px 16px;
            border-bottom: 1px solid rgba(148,163,184,.18);
        }

        .ventas-tabla-head h3 {
            margin: 0;
            font-size: 1rem;
            font-weight: 800;
            color: #0f172a;
        }

        .ventas-tabla-responsive {
            overflow: auto;
            max-height: 62vh;
        }

        .ventas-tabla {
            width: 100%;
            min-width: 980px;
            border-collapse: separate;
            border-spacing: 0;
        }

        .ventas-tabla thead th {
            position: sticky;
            top: 0;
            background: #f8fafc;
            color: rgba(15,23,42,.72);
            font-size: .8rem;
            font-weight: 900;
            padding: 12px 14px;
            border-bottom: 1px solid rgba(148,163,184,.18);
            white-space: nowrap;
        }

        .ventas-tabla tbody td {
            padding: 12px 14px;
            border-bottom: 1px solid rgba(226,232,240,.85);
            vertical-align: middle;
            color: #0f172a;
            font-weight: 600;
            white-space: nowrap;
        }

        .ventas-badge {
            display: inline-flex;
            align-items: center;
            gap: 6px;
            padding: 6px 10px;
            border-radius: 999px;
            font-size: .78rem;
            font-weight: 900;
        }

        .ventas-badge.ok { background: rgba(22,163,74,.10); color: #15803d; }
        .ventas-badge.warn { background: rgba(245,158,11,.12); color: #b45309; }
        .ventas-badge.bad { background: rgba(239,68,68,.10); color: #dc2626; }
        .ventas-badge.info { background: rgba(14,165,233,.10); color: #0369a1; }
        .ventas-badge.gray { background: rgba(148,163,184,.16); color: #475569; }

        .ventas-empty {
            padding: 32px 18px;
            text-align: center;
            color: rgba(15,23,42,.60);
            font-weight: 700;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- ? LOADING OVERLAY (visible en primer load, oculto en postbacks) -->
    <div id="appLoading" class="app-loading"
        style="<%= IsPostBack ? "display:none;": "display:flex;" %>">
        <div class="app-loading-card">
            <div class="app-spinner" aria-hidden="true"></div>
            <div class="app-loading-title">Cargando...</div>
            <div class="app-loading-sub">Estamos preparando tu informaci&oacute;n</div>
        </div>
    </div>


    <asp:ScriptManager runat="server" EnablePageMethods="true" />

    <asp:HiddenField ID="hidAccion" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hidArgumento" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hdIdClienteDomicilio" runat="server" ClientIDMode="Static" />

    <asp:Button ID="btnBridge" runat="server"
        ClientIDMode="Static"
        Style="display: none;"
        OnClick="Evento_Click" />


    <!-- ? OJO: tu CSS usa .app-shell -->
    <main class="container-fluid app-shell">

        <div class="row g-1 h-100">

            <!-- ================= IZQUIERDA ================= -->
            <section class="col-12 <%= EnVistaVentas() ? string.Empty : "col-xl-8" %> d-flex flex-column">

                <!-- Top: nombre mesero + botones -->
                <div class="panel panel-alto-auto b-orange mb-1 py-2 border-0">

                    <div class="d-flex flex-column flex-lg-row align-items-start align-items-lg-center justify-content-between gap-2">

                        <!-- IZQUIERDA -->
                        <div class="fw-bold text-dark ps-2">
                            ?? <span class="fw-semibold"><%: models.vendedor.nombreVendedor %></span>
                        </div>

                        <!-- DERECHA: Botones -->
                        <div class="acciones-top d-flex flex-wrap gap-2">

                            <button id="btnActualizar" onclick="EjecutarAccion('Actualizar','',this)" type="button" class="btn-top btn-actualizar">
                                <i class="bi bi-arrow-clockwise"></i>
                                Actualizar
                            </button>

                            <button type="button" class="btn-top btn-nuevo" onclick="EjecutarAccion('NuevoServicio','',this)">
                                <i class="bi bi-plus-circle-fill"></i>
                                Nuevo Servicio
                            </button>

                            <button type="button" class="btn-top btn-domicilio" data-idmesa="<%= models.IdMesaActiva %>" data-idservicio="<%= models.IdCuentaActiva %>" onclick="return btnDomicilioCaja(this);">
                                <i class="bi bi-house-door-fill"></i>
                                Domicilio
                            </button>

                            <%if (models.vendedor.cajaMovil == 1)
                                { %>

                            <button type="button"
                                class="btn-top btn-logout<%= PuedeEliminarServicioActivo() ? string.Empty : " btn-eliminar-servicio-disabled" %>"
                                onclick="<%= PuedeEliminarServicioActivo() ? "return ConfirmarEliminarServicio(this)" : "return false;" %>"
                                <%= PuedeEliminarServicioActivo() ? string.Empty : "disabled=\"disabled\" title=\"No se puede eliminar porque el servicio tiene productos cargados.\"" %>>
                                <i class="bi bi-trash3-fill"></i>
                                Eliminar servicio
                            </button>

                            <button type="button" class="btn-top btn-ventas" onclick="EjecutarAccion('<%= AccionBotonVentas() %>','',this)">
                                <i class="bi <%= EnVistaVentas() ? "bi-cash-coin" : "bi-graph-up-arrow" %>"></i>
                                <%= TextoBotonVentas() %>
                            </button>

                            <a href="PGastos.aspx" class="btn-top btn-domicilio text-decoration-none">
                                <i class="bi bi-wallet2"></i>
                                Gastos
                            </a>
                            <button type="button" class="btn-top btn-cerrar-caja" onclick="return ConfirmarCerrarCaja(this)">
                                <i class="bi bi-lock-fill"></i>
                                Cerrar caja
                            </button>

                            <%} %>

                            <button type="button" class="btn-top btn-logout" onclick="return ConfirmarCerrarSesion(this)">
                                <i class="bi bi-box-arrow-right"></i>
                                Cerrar sesi&oacute;n
                            </button>

                        </div>

                    </div>

                </div>

                <% if (!EnVistaVentas()) { %>
                <!-- Top: cuentas + nuevo servicio -->
                <div class="panel panel-alto-auto b-orange mb-1 border-0">
                    <div class="row g-1">

                        <!-- CUENTAS -->
                        <div class="col-12 col-lg-12">
                            <div class="box h-cuentas p-1 cuentas-box bg-white">
                                <div class="cuentas-grid">

                                    <asp:Repeater runat="server" ID="rpCuentas">
                                        <ItemTemplate>

                                            <div class="cuenta-card <%# (Convert.ToInt32(Eval("id")) == models.IdCuentaActiva ? "cuenta-activa" : "") %>">

                                                <!-- ?? EDITAR: abre modal (NO debe disparar seleccionar cuenta) -->
                                                <button type="button"
                                                    class="cuenta-edit"
                                                    title="Editar"
                                                    onclick="return abrirModalCuenta(this, event);"
                                                    data-id="<%# Eval("id") %>"
                                                    data-nombre="<%# Eval("aliasVenta") %>">
                                                    <i class="bi bi-pencil-fill"></i>
                                                </button>

                                                <!-- Click de la tarjeta: selecciona cuenta (si lo necesitas) -->
                                                <div class="cuenta-body"
                                                    onclick="EjecutarAccion('SeleccionarCuenta','ID=<%# Eval("id") %>',this)">
                                                    <div class="cuenta-num"><%# Eval("aliasVenta") %></div>
                                                    <div class="cuenta-name"><%# Eval("nombremesa") %></div>
                                                </div>

                                            </div>

                                        </ItemTemplate>
                                    </asp:Repeater>

                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Bottom: Zonas/Mesas + Buscar/Categorias/Productos -->
                <div class="row g-1 flex-grow-1">

                    <!-- Zonas + Mesas -->
                    <div id="divZonas" runat="server" class="col-12 col-lg-5 d-flex">
                        <div class="panel bg-light w-100 border-1">
                            <div class="row">
                                <div class="col-12 panel-zona">
                                    <div class="box p-0 h-zonas zonas-box">

                                        <div class="zonas-grid">

                                            <asp:Repeater runat="server" ID="rpZonas">
                                                <ItemTemplate>

                                                    <button type="button"
                                                        class="zona-btn <%# (Convert.ToInt32(Eval("id")) == models.IdZonaActiva ? "zona-activa" : "") %>"
                                                        onclick="EjecutarAccion('SeleccionarZona','ID=<%# Eval("id") %>',this)">

                                                        <%# Eval("nombreZona") %>
                                                    </button>

                                                </ItemTemplate>
                                            </asp:Repeater>



                                        </div>

                                    </div>
                                </div>

                                <div class="col-12">
                                    <div class="box h-mesas p-1 mesas-box panel-alto-auto ">

                                        <div class="mesas-grid panel-alto-auto">

                                            <asp:Repeater runat="server" ID="rpMesas">
                                                <ItemTemplate>
                                                    <button type="button"
                                                        class='mesa-btn <%# (int)Eval("estadoMesa") == 1 ? "mesa-activa" : "" %>'
                                                        onclick="EjecutarAccion('SeleccionarMesa','ID=<%# Eval("id") %>', this)">

                                                        <%# Eval("nombreMesa") %>
                                                    </button>
                                                </ItemTemplate>
                                            </asp:Repeater>

                                        </div>

                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Buscar + Categorias + Productos -->
                    <div id="divProductos" runat="server" class="col-12 col-lg-7 d-flex">
                        <div class="panel b-purple w-100">
                            <div class="row g-0 ">
                                <div class="col-12" style="height: 50px">

                                    <div class="buscar-wrapper w-100">

                                        <i class="bi bi-search buscar-icon"></i>

                                        <input id="btnbuscar" type="text" runat="server" ClientIDMode="Static" class="buscar-input" placeholder="Buscar producto..." autocomplete="off" />

                                        <button type="button"
                                            class="buscar-clear"
                                            onclick="window.CajaBuscador && CajaBuscador.clear();">
                                            <i class="bi bi-x-lg"></i>
                                        </button>

                                    </div>
                                </div>

                                <div class="col-12 panel-alto-auto">
                                    <div class="box h-categorias p-1 categorias-box" style="height: 40vh;">

                                        <div class="categorias-grid">

                                            <asp:Repeater runat="server" ID="rpCategorias">
                                                <ItemTemplate>

                                                                                                        <button type="button" class='categoria-btn <%# (int)Eval("id") == models.IdCategoriaActiva ? "categoria-activa" : "" %>'
                                                        data-categoria-id="<%# Eval("id") %>"
                                                        data-categoria-nombre="<%# Eval("nombreCategoria") %>"
                                                        onclick="EjecutarAccion('SeleccionarCategoria','id=<%# Eval("id") %>',this)">
                                                        <%# Eval("nombreCategoria") %>
                                                    </button>

                                                </ItemTemplate>
                                            </asp:Repeater>



                                        </div>

                                    </div>
                                </div>

                                <div class="col-12">
                                    <div class="box h-productos p-0 productos-box">

                                        <div class="productos-list">

                                            <asp:Repeater runat="server" ID="rpProductos">
                                                <ItemTemplate>

                                                    <!-- ITEM -->
                                                    <div class="producto-item" data-producto-id="<%# Eval("idPresentacion") %>" data-producto-codigo="<%# Eval("codigoProducto") %>" data-producto-nombre="<%# Eval("nombreProducto") %>" data-producto-descripcion="<%# Eval("descripcionProducto") %>" data-categoria-id="<%# Eval("idCategoria") %>" data-categoria-nombre="<%# Eval("nombreCategoria") %>">

                                                        <div class="row g-0 align-items-center">

                                                            <!-- 1) NOMBRE 100% -->
                                                            <div class="col-12">
                                                                <div class="producto-nombre">
                                                                    <%# Eval("nombreProducto") %>
                                                                </div>
                                                            </div>

                                                            <!-- 2) IZQUIERDA: SUB (precio + detalle) -->
                                                            <div class="col-12 col-md-6">
                                                                <div class="producto-sub">
                                                                    <span class="producto-precio"><%# Eval("precioVenta","{0:C0}") %></span>
                                                                    <a href="#" class="producto-detalle">Ver detalle</a>
                                                                </div>
                                                            </div>

                                                            <!-- 3) DERECHA: ACCIONES -->
                                                            <div class="col-12 col-md-6">
                                                                <div class="producto-acciones">

                                                                    <button type="button" class="prod-btn prod-minus js-restar" aria-label="Disminuir">
                                                                        <i class="bi bi-dash-lg"></i>
                                                                    </button>

                                                                    <input type="text" class="prod-cant js-cantidad" value="1" inputmode="numeric" />

                                                                    <button type="button" class="prod-btn prod-plus js-sumar" aria-label="Aumentar">
                                                                        <i class="bi bi-plus-lg"></i>
                                                                    </button>

                                                                    <button type="button" class="prod-btn prod-cart" aria-label="Agregar" data-id="<%# Eval("idPresentacion") %>" onclick="return agregarProductoDesdeCard(this);">
                                                                        <i class="bi bi-cart"></i>
                                                                    </button>
                                                                </div>
                                                            </div>

                                                        </div>

                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>










                                        </div>

                                    </div>
                                </div>

                            </div>
                        </div>
                    </div>

                </div>
                <% } else { %>
                <div class="ventas-vista flex-grow-1">
                    <div class="ventas-vista__hero">
                        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
                            <div>
                                <h2 class="ventas-vista__title">Historial de ventas del turno</h2>
                                <div class="ventas-vista__sub">Consulta las ventas de la base activa sin salir de caja.</div>
                            </div>
                            <span class="ventas-badge info"><i class="bi bi-collection"></i><%= VentasCajaCantidad %> facturas</span>
                        </div>
                        <div class="row g-3 mt-1">
                            <div class="col-12 col-md-6 col-xl-3">
                                <div class="ventas-kpi">
                                    <div class="ventas-kpi__label">Total ventas</div>
                                    <div class="ventas-kpi__value"><%= MonedaVistaVentas(VentasCajaTotal) %></div>
                                    <div class="ventas-kpi__muted">Periodo del turno activo</div>
                                </div>
                            </div>
                            <div class="col-12 col-md-6 col-xl-3">
                                <div class="ventas-kpi">
                                    <div class="ventas-kpi__label">Facturas</div>
                                    <div class="ventas-kpi__value"><%= VentasCajaCantidad %></div>
                                    <div class="ventas-kpi__muted">Cantidad emitida</div>
                                </div>
                            </div>
                            <div class="col-12 col-md-6 col-xl-3">
                                <div class="ventas-kpi">
                                    <div class="ventas-kpi__label">Pendiente</div>
                                    <div class="ventas-kpi__value"><%= MonedaVistaVentas(VentasCajaPendiente) %></div>
                                    <div class="ventas-kpi__muted">Cartera / credito</div>
                                </div>
                            </div>
                            <div class="col-12 col-md-6 col-xl-3">
                                <div class="ventas-kpi">
                                    <div class="ventas-kpi__label">Anuladas</div>
                                    <div class="ventas-kpi__value"><%= VentasCajaAnuladas %></div>
                                    <div class="ventas-kpi__muted">Ventas canceladas</div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="ventas-tabla-wrap">
                        <div class="ventas-tabla-head">
                            <h3><i class="bi bi-receipt-cutoff me-2"></i>Ventas de la base activa</h3>
                            <span class="ventas-badge gray"><i class="bi bi-clock-history"></i>Actualizado en tiempo de recarga</span>
                        </div>
                        <div class="ventas-tabla-responsive">
                            <table class="ventas-tabla">
                                <thead>
                                    <tr>
                                        <th>Cuenta</th>
                                        <th>Fecha</th>
                                        <th>Factura</th>
                                        <th>Cliente</th>
                                        <th>Medio</th>
                                        <th class="text-end">Total</th>
                                        <th class="text-end">Pagado</th>
                                        <th class="text-end">Pendiente</th>
                                        <th>Estado</th>
                                        <th>FE</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <% if (VentasCaja != null && VentasCaja.Any()) { %>
                                        <% foreach (var venta in VentasCaja) { %>
                                        <tr>
                                            <td><span class="ventas-badge gray"><i class="bi bi-hash"></i><%= venta.id %></span></td>
                                            <td>
                                                <div><%= venta.fechaVenta.ToString("yyyy-MM-dd") %></div>
                                                <div class="text-muted small"><%= venta.fechaVenta.ToString("hh:mm tt") %></div>
                                            </td>
                                            <td>
                                                <div><strong><%= FacturaLabelVista(venta) %></strong></div>
                                                <div class="text-muted small">Alias: <%= string.IsNullOrWhiteSpace(venta.aliasVenta) ? "Sin alias" : venta.aliasVenta %></div>
                                            </td>
                                            <td>
                                                <div><strong><%= string.IsNullOrWhiteSpace(venta.nombreCliente) ? "Cliente contado" : venta.nombreCliente %></strong></div>
                                                <div class="text-muted small">NIT: <%= string.IsNullOrWhiteSpace(venta.nit) ? "0" : venta.nit %></div>
                                            </td>
                                            <td><span class="ventas-badge info"><i class="bi bi-credit-card-2-front"></i><%= string.IsNullOrWhiteSpace(venta.medioDePago) ? "Sin definir" : venta.medioDePago %></span></td>
                                            <td class="text-end"><strong><%= MonedaVistaVentas(venta.total_A_Pagar) %></strong></td>
                                            <td class="text-end"><strong><%= MonedaVistaVentas(venta.totalPagadoVenta) %></strong></td>
                                            <td class="text-end"><strong><%= MonedaVistaVentas(venta.totalPendienteVenta) %></strong></td>
                                            <td>
                                                <span class='ventas-badge <%= EsVentaAnulada(venta) ? "bad" : (venta.totalPendienteVenta > 0 ? "warn" : "ok") %>'>
                                                    <i class='bi <%= EsVentaAnulada(venta) ? "bi-x-octagon" : (venta.totalPendienteVenta > 0 ? "bi-hourglass-split" : "bi-check2-circle") %>'></i>
                                                    <%= EstadoVentaVista(venta) %>
                                                </span>
                                            </td>
                                            <td>
                                                <span class='ventas-badge <%= !string.IsNullOrWhiteSpace(venta.cufe) || !string.IsNullOrWhiteSpace(venta.estadoFE) ? "ok" : "gray" %>'>
                                                    <i class='bi <%= !string.IsNullOrWhiteSpace(venta.cufe) || !string.IsNullOrWhiteSpace(venta.estadoFE) ? "bi-qr-code-scan" : "bi-dash-circle" %>'></i>
                                                    <%= !string.IsNullOrWhiteSpace(venta.cufe) || !string.IsNullOrWhiteSpace(venta.estadoFE) ? (string.IsNullOrWhiteSpace(venta.estadoFE) ? "Emitida" : venta.estadoFE) : "No aplica" %>
                                                </span>
                                            </td>
                                        </tr>
                                        <% } %>
                                    <% } else { %>
                                        <tr>
                                            <td colspan="10">
                                                <div class="ventas-empty">
                                                    <i class="bi bi-inboxes d-block mb-2" style="font-size:2rem;color:#94a3b8;"></i>
                                                    No hay ventas registradas en la base activa del turno.
                                                </div>
                                            </td>
                                        </tr>
                                    <% } %>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
                <% } %>
            </section>

            <% if (!EnVistaVentas()) { %>
            <!-- ================= DERECHA ================= -->
                        <aside class="col-12 col-xl-4 d-flex">
                <div class="panel w-100">
                    <div class="row g-0 h-100 aside-stack">

                        <div class="col-12">
                            <div class="box h-precios p-0 precios-box">
                                <div class="precios-resumen">
                                    <div class="precio-row">
                                        <div class="precio-label">SUBTOTAL:</div>
                                        <div class="precio-value"><%: FormatearMoneda(ResumenSubtotal()) %></div>
                                    </div>

                                    <div class="precio-row">
                                        <div class="precio-label">IMPUESTOS</div>
                                        <div class="precio-value"><%: FormatearMoneda(ResumenImpuestos()) %></div>
                                    </div>

                                    <div class="precio-row precio-total">
                                        <div class="precio-label">TOTAL 1:</div>
                                        <div class="precio-value"><%: FormatearMoneda(ResumenTotal1()) %></div>
                                    </div>

                                    <div class="precio-row precio-servicio">
                                        <div class="precio-label">SERVICIO (<%: ResumenPorcentajePropina().ToString("0.##") %>%)</div>
                                        <div class="precio-servicio-acciones">
                                            <button type="button"
                                                id="btnEditarPropina"
                                                class="btn-servicio-editar"
                                                title="Editar propina"
                                                data-porcentaje="<%: ResumenPorcentajePropina().ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) %>"
                                                data-propina="<%: Convert.ToInt32(ResumenPropina()) %>"
                                                data-idventa="<%: models.IdCuentaActiva %>"
                                                data-idcuenta="<%: models.IdCuenteClienteActiva %>"
                                                data-subtotal="<%: Convert.ToInt32(ResumenSubtotal()) %>"
                                                onclick="return abrirModalPropina(this);">Editar</button>
                                            <div class="precio-value"><%: FormatearMoneda(ResumenPropina()) %></div>
                                        </div>
                                    </div>

                                    <div class="precio-row precio-total2">
                                        <div class="precio-label">TOTAL 2:</div>
                                        <div class="precio-value"><%: FormatearMoneda(ResumenTotal2()) %></div>
                                    </div>
                                </div>

                                <div class="precios-acciones">
                                    <button type="button" class="accion-btn accion-comandar" onclick="EjecutarAccion('Comandar','',this)">
                                        <i class="bi bi-send-fill me-1"></i>Comandar
                                    </button>

                                    <button type="button" class="accion-btn accion-cuenta" onclick="EjecutarAccion('SolicitarCuenta','',this)">
                                        <i class="bi bi-chat-square-text-fill me-1"></i>Solicitar<br />
                                        Cuenta
                                    </button>
                                    <% if (models.vendedor.cajaMovil == 1)
                                        {%>
                                    <button type="button" class="accion-btn accion-cobrar" onclick="EjecutarAccion('Cobrar','',this)">
                                        <i class="bi bi-cash-coin me-1"></i>Cobrar
                                    </button>
                                    <%} %>
                                </div>
                            </div>
                        </div>

                        <div class="col-12">
                            <div class="box h-nueva-cuenta">
                                <div class="cuentas-header col-12">
                                    <button type="button" class="btn-nueva-cuenta" onclick="return abrirModalCuentaClienteNueva();">
                                        <i class="bi bi-plus-lg me-2"></i>
                                        Nueva cuenta
                                    </button>

                                    <button type="button" class="btn-cuenta-general <%= models.IdCuenteClienteActiva == 0 ? "cuenta-item-activa" : string.Empty %>" onclick="EjecutarAccion('SeleccionarCuentaCliente','ID=0',this)">
                                        <span><%: NombreCuentaClienteActiva() == "Cuenta General" ? "Cuenta General" : "Cuenta General" %></span>
                                        <span class="cuenta-total"><%: FormatearMoneda(models.venta?.total_A_Pagar ?? 0) %></span>
                                    </button>
                                </div>

                                <div class="cuentas-lista">
                                    <asp:Repeater runat="server" ID="rpCuentasCliente">
                                        <ItemTemplate>
                                            <button type="button" class="cuenta-item <%# Convert.ToInt32(Eval("id")) == models.IdCuenteClienteActiva ? "cuenta-item-activa" : string.Empty %>" onclick="EjecutarAccion('SeleccionarCuentaCliente','ID=<%# Eval("id") %>',this)">
                                                <%# Eval("nombreCuenta") %>
                                                <small><%# FormatearMoneda(Eval("total_A_Pagar")) %></small>
                                            </button>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>
                        </div>

                        <div class="col-12">
                            <div class="box h-lista">
                                <div class="row lista-productos">
                                    <asp:Repeater runat="server" ID="rpDetalleCaja">
                                        <ItemTemplate>
                                            <div class="col-6 col-xl-12 item-col">
                                                <div class="producto-item-detalle" data-detalle-id="<%# Eval("id") %>" data-detalle-nombre="<%# Eval("nombreProducto") %>" data-detalle-nota="<%# Eval("adiciones") %>" data-detalle-categoria-id="<%# Eval("idCategoria") %>" data-detalle-cantidad="<%# Convert.ToDecimal(Eval("unidad")).ToString("0") %>" data-detalle-precio="<%# Convert.ToDecimal(Eval("precioVenta")).ToString(System.Globalization.CultureInfo.InvariantCulture) %>">
                                                    <div class="prod-top-detalle">
                                                        <div class="prod-nombre"><%# Eval("nombreProducto") %></div>
                                                        <div class="prod-precio"><%# FormatearMoneda(Eval("precioVenta")) %></div>
                                                    </div>

                                                    <div class="prod-mid-detalle">
                                                        <div class="prod-cantidad">
                                                            <button type="button" class="qty-btn js-detalle-restar" aria-label="Disminuir">-</button>
                                                            <input type="text" class="qty-input js-detalle-cantidad" value="<%# Convert.ToDecimal(Eval("unidad")).ToString("0") %>" inputmode="numeric" />
                                                            <button type="button" class="qty-btn js-detalle-sumar" aria-label="Aumentar">+</button>
                                                        </div>

                                                        <button type="button" class="save-btn" aria-label="Guardar" onclick="return guardarCantidadDetalle(this);">
                                                            <i class="bi bi-floppy2-fill"></i>
                                                        </button>
                                                    </div>

                                                    <div class="prod-actions-detalle">
                                                        <button type="button" class="act-btn" aria-label="Notas" title="Notas del producto" onclick="return editarNotaDetalle(this);">
                                                            <i class="bi bi-chat"></i>
                                                        </button>
                                                        <button type="button" class="act-btn" aria-label="Anclar a cuenta" title="<%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("nombreCuenta"))) ? "Anclar a cuenta" : Eval("nombreCuenta") %>" onclick="return anclarDetalleCuenta(this);">
                                                            <i class="bi bi-link-45deg"></i>
                                                        </button>
                                                        <button type="button" class="act-btn act-danger" aria-label="Eliminar" title="Eliminar producto" onclick="return confirmarEliminarDetalle(this);">
                                                            <i class="bi bi-trash"></i>
                                                        </button>
                                                        <button type="button" class="act-btn" aria-label="Dividir" title="Dividir producto" onclick="return dividirDetalle(this);">
                                                            <i class="bi bi-scissors"></i>
                                                        </button>
                                                        <button type="button" class="act-btn" aria-label="Descuento" title="Editar valor o descuento" onclick="return editarValorDetalle(this);">
                                                            <i class="bi bi-cash-coin"></i>
                                                        </button>
                                                        <button type="button" class="act-btn" aria-label="Editar" title="Editar producto" onclick="return editarNombreDetalle(this);">
                                                            <i class="bi bi-pencil-square"></i>
                                                        </button>
                                                    </div>

                                                    <div class="cuenta-detalle-chip <%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("nombreCuenta"))) ? "d-none" : string.Empty %>">
                                                        <i class="bi bi-person-badge"></i>
                                                        <span><%# Eval("nombreCuenta") %></span>
                                                    </div>

                                                    <div class="prod-bottom">
                                                        <button type="button" class="nota-btn" aria-label="Notas" title="<%# Eval("adiciones") %>">
                                                            <i class="bi bi-journal-text me-2"></i><span><%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("adiciones"))) || Convert.ToString(Eval("adiciones")) == "--" ? "Sin nota" : Eval("adiciones") %></span>
                                                        </button>

                                                        <div class="prod-total"><%# FormatearMoneda(Eval("totalDetalle")) %></div>
                                                    </div>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>

                                    <% if (!TieneDetalleServicioActivo()) { %>
                                    <div class="col-12">
                                        <div class="detalle-empty">
                                            <i class="bi bi-bag-x"></i>
                                            Este servicio aÃºn no tiene productos cargados.
                                        </div>
                                    </div>
                                    <% } %>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </aside>
            <% } %>

        </div>
    </main>


    <div class="modal fade" id="modalDomicilio" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title"><i class="bi bi-phone"></i> Domicilios</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">Buscar celular</label>
                        <input type="text" id="txtBuscarCelular" class="form-control" placeholder="Escriba el celular y presione Enter..." />
                    </div>
                    <div class="row g-2 mb-3">
                        <div class="col-md-4">
                            <label class="form-label">Telefono</label>
                            <input type="text" id="txtTelefono" class="form-control" />
                        </div>
                        <div class="col-md-8">
                            <label class="form-label">Nombre Cliente</label>
                            <input type="text" id="txtNombreCliente" class="form-control" />
                        </div>
                    </div>
                    <div class="row g-2 mb-2 align-items-end">
                        <div class="col-md-9">
                            <label class="form-label">Direccion</label>
                            <input type="text" id="txtDireccion" class="form-control" />
                        </div>
                        <div class="col-md-3 text-end">
                            <a href="#" id="btnCrearDomicilio" class="btn btn-link">
                                <i class="bi bi-save2"></i>
                                Crear / Actualizar
                            </a>
                        </div>
                    </div>
                    <div class="mb-2">
                        <button id="btnSeleccionarDomicilio" type="button" class="btn btn-success w-100">
                            <i class="bi bi-check-circle"></i> Seleccionar
                        </button>
                    </div>
                    <div class="table-responsive">
                        <table class="table table-hover" id="tblDomicilios">
                            <thead>
                                <tr>
                                    <th>Telefono</th>
                                    <th>Nombre</th>
                                    <th>Direccion</th>
                                </tr>
                            </thead>
                            <tbody></tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- Modal: Crear / Editar Nombre de Cuenta -->
    <div class="modal fade" id="modalCuentaCliente" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="modalCuentaTitulo">Nueva Cuenta</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">
                    <div class="mb-2">
                        <label for="txtCuentaNombre" class="form-label">Nombre de la cuenta</label>
                        <input type="text" id="txtCuentaNombre" class="form-control" maxlength="100" placeholder="Ingrese el nombre de la cuenta" />
                        <div id="cuentaError" class="form-text text-danger d-none">Ingrese un nombre v&aacute;lido (m&iacute;n. 2 caracteres).</div>
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                    <input type="hidden" id="idCuentaModalEditar" />
                    <input type="hidden" id="accionCuentaModal" value="EditarAliasCuenta" />
                    <!-- boton de guardar-client (no server control) -->
                    <button type="button" id="btnGuardarCuenta" class="btn btn-primary" onclick="guardarCuentaDirecto(this)">Guardar</button>
                </div>
            </div>
        </div>
    </div>



    
    <div class="modal fade" id="modalPropina" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Editar propina</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <div class="row g-3">
                        <div class="col-12">
                            <label for="txtSubtotalPropina" class="form-label small mb-1">Subtotal</label>
                            <input type="text" id="txtSubtotalPropina" class="form-control" readonly />
                        </div>
                        <div class="col-12 col-md-6">
                            <label for="txtPorcentajePropina" class="form-label small mb-1">% Propina</label>
                            <input type="number" id="txtPorcentajePropina" class="form-control" min="0" max="15" step="1" />
                        </div>
                        <div class="col-12 col-md-6">
                            <label for="txtValorPropina" class="form-label small mb-1">Valor propina</label>
                            <input type="text" id="txtValorPropina" class="form-control" inputmode="numeric" />
                        </div>
                        <div class="col-12">
                            <div class="d-flex gap-2 flex-wrap">
                                <button type="button" class="btn btn-outline-primary btn-sm quick-tip" data-tip="0">0%</button>
                                <button type="button" class="btn btn-outline-primary btn-sm quick-tip" data-tip="8">8%</button>
                                <button type="button" class="btn btn-outline-primary btn-sm quick-tip" data-tip="10">10%</button>
                                <button type="button" class="btn btn-outline-primary btn-sm quick-tip" data-tip="15">15%</button>
                                <button type="button" class="btn btn-outline-danger btn-sm" id="btnQuitarPropina">Quitar</button>
                            </div>
                            <small id="ayudaPropina" class="text-muted d-block mt-2"></small>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                    <button type="button" id="btnGuardarPropina" class="btn btn-primary">Guardar</button>
                </div>
            </div>
        </div>
    </div>

<!-- ==========================================================
     Modal: Acciones de Mesa (Bootstrap 5)
     - Mostrar nombre de mesa seleccionada
     - 3 botones: Crear nuevo servicio, Amarrar mesa, Cancelar
========================================================== -->
    <div runat="server" class="modal fade" id="modalAccionesMesa" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow-lg" style="border-radius: 18px; overflow: hidden;">

                <!-- Header -->
                <div class="modal-header border-0" style="background: linear-gradient(135deg, rgba(11,58,126,.12), rgba(30,136,255,.10));">
                    <div class="w-100">
                        <div class="fw-bold" style="font-size: 1.05rem; color: #0f172a;">
                            Mesa seleccionada:
            <span runat="server" id="lblMesaSeleccionada" class="fw-bolder" style="color: #0b3a7e;">--</span>
                        </div>
                        <div class="mt-1" style="color: rgba(15,23,42,.70); font-size: .92rem;">
                            &iquest;Cu&aacute;l de las siguientes acciones desea?
                        </div>
                    </div>

                    <button type="button" class="btn-close ms-2" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <!-- Body -->
                <div class="modal-body pt-2 pb-3">
                    <div class="p-3" style="background: rgba(2,6,23,.03); border: 1px solid rgba(2,6,23,.08); border-radius: 14px;">
                        <div class="d-flex align-items-start gap-2">
                            <div class="rounded-circle d-inline-flex align-items-center justify-content-center"
                                style="width: 38px; height: 38px; background: rgba(30,136,255,.12); color: #0b3a7e;">
                                <i class="bi bi-info-circle-fill"></i>
                            </div>
                            <div>
                                <div class="fw-semibold" style="color: #0f172a;">Acciones disponibles</div>
                                <div style="color: rgba(15,23,42,.72); font-size: .92rem;">
                                    Elige una opci&oacute;n para continuar con la mesa seleccionada.
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Botones (estilo moderno) -->
                    <div class="d-grid gap-2 mt-3">

                        <button type="button" id="btnCrearServicioMesa" class="btn btn-primary btn-lg"
                            style="border-radius: 14px; font-weight: 800;"
                            onclick="EjecutarAccion('AccionMesa_CrearServicio','',this)">
                            <i class="bi bi-plus-circle-fill me-2"></i>
                            Crear nuevo servicio
                        </button>

                        <button type="button" id="btnAmarrarMesa" class="btn btn-outline-primary btn-lg"
                            style="border-radius: 14px; font-weight: 800;"
                            onclick="EjecutarAccion('AccionMesa_AmarrarMesa','',this)">
                            <i class="bi bi-link-45deg me-2"></i>
                            Amarrar mesa
                        </button>

                        <button type="button" id="btnLiberarMesa" class="btn btn-outline-danger btn-lg"
                            style="border-radius: 14px; font-weight: 800;"
                            onclick="ConfirmarLiberarMesa(this)">
                            <i class="bi bi-unlock-fill me-2"></i>
                            Liberar mesa
                        </button>
                        <button type="button" class="btn btn-light btn-lg"
                            style="border-radius: 14px; font-weight: 800;"
                            data-bs-dismiss="modal">
                            <i class="bi bi-x-circle me-2"></i>
                            Cancelar
                        </button>

                    </div>
                </div>

            </div>
        </div>
    </div>

    <!-- =========================
     MODAL: Seleccionar Cuenta
========================== -->
    <div class="modal fade" id="mdlCuentas" tabindex="-1" aria-hidden="true" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable modal-lg">
            <div class="modal-content modal-cuentas">

                <!-- Header -->
                <div class="modal-header modal-cuentas__header">
                    <div class="d-flex align-items-center gap-2">
                        <div class="modal-cuentas__icon">
                            <i class="bi bi-wallet2"></i>
                        </div>
                        <div>
                            <h5 class="modal-title m-0">Seleccionar cuenta</h5>
                            <small class="text-white-50">Elige una cuenta para continuar</small>
                        </div>
                    </div>

                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <!-- Body -->
                <div class="modal-body">

                    <!-- Lista -->
                    <div class="cuentas-list" id="lstCuentas">

                        <asp:Repeater runat="server" ID="rpCuentasModal">
                            <ItemTemplate>

                                <button type="button"
                                    class="cuenta-item" onclick="EjecutarAccion('AmarrarMesaCuenta','id=<%# Eval("id") %>','this')">
                                    <div class="cuenta-item__left">
                                        <div class="cuenta-item__title"><%# Eval("aliasVenta") %></div>
                                        <div class="cuenta-item__sub">
                                            <i class="bi bi-grid-3x3-gap me-1"></i>
                                            Mesas: <%# Eval("nombremesa") %>
                                        </div>
                                    </div>

                                    <div class="cuenta-item__right">
                                        <span class="cuenta-item__badge">Seleccionar</span>
                                        <i class="bi bi-chevron-right"></i>
                                    </div>
                                </button>

                            </ItemTemplate>
                        </asp:Repeater>



                    </div>

                    <!-- Vacio -->
                    <div class="cuentas-empty d-none" id="cuentasEmpty">
                        <div class="cuentas-empty__box">
                            <i class="bi bi-inboxes"></i>
                            <div class="mt-2 fw-semibold">No hay resultados</div>
                            <small class="text-muted">Intenta con otro nombre o mesa.</small>
                        </div>
                    </div>

                </div>

                <!-- Footer -->
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">
                        Cerrar
                    </button>
                </div>

            </div>
        </div>
    </div>

    <script>
        window.SerinsisLoading = {
            show: function () {
                const el = document.getElementById('appLoading');
                if (el) el.style.display = 'flex';
            },
            hide: function () {
                const el = document.getElementById('appLoading');
                if (el) el.style.display = 'none';
            }
        };

        // En la carga inicial: ocultar cuando ya se pinto la pagina
        window.addEventListener('load', function () {
            SerinsisLoading.hide();
        });
    </script>

    <script>
        window.ListaClientesDomicilio = <%= ClienteDomiciliosJson() %>;
        window.CajaAdicionesCatalogo = <%= AdicionesCatalogoJson() %>;
    </script>

    <% if (models.AbrirModalDomicilio) { %>
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            var modalEl = document.getElementById('modalDomicilio');
            if (modalEl && window.bootstrap) {
                bootstrap.Modal.getOrCreateInstance(modalEl, { backdrop: 'static' }).show();
                if (typeof cargarTablaDomicilios === 'function') {
                    cargarTablaDomicilios();
                }
            }
        });
    </script>
    <% } %>
    <script src="Scripts/js/caja.js"></script>
    <script src="Scripts/js/app-modal.js"></script>
</asp:Content>





























