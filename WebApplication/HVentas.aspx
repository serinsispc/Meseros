<%@ Page Title="Historial de Ventas" Language="C#" MasterPageFile="~/Menu.Master" AutoEventWireup="true"
    CodeBehind="HVentas.aspx.cs" Inherits="WebApplication.HVentas" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        #topActionBar {
            justify-content: flex-start !important;
            gap: 12px;
        }

        #topActionBar > div:last-child,
        #topActionBar a:not([data-nav="caja"]) {
            display: none !important;
        }

        :root {
            --hv-bg: #f6f8fb;
            --hv-card: #ffffff;
            --hv-border: rgba(10, 26, 54, .10);
            --hv-text: #0b1b33;
            --hv-muted: rgba(11, 27, 51, .68);
            --hv-primary: #2563eb;
            --hv-success: #16a34a;
            --hv-warning: #f59e0b;
            --hv-danger: #ef4444;
            --hv-info: #0ea5e9;
            --hv-radius: 18px;
            --hv-shadow: 0 14px 40px rgba(15, 23, 42, .10);
        }

        .hv-wrap { padding: 18px 0 30px; }
        .hv-hero {
            background: linear-gradient(135deg, rgba(37,99,235,.12), rgba(14,165,233,.10));
            border: 1px solid var(--hv-border);
            border-radius: var(--hv-radius);
            box-shadow: var(--hv-shadow);
            padding: 18px;
        }
        .hv-title {
            font-size: 1.35rem;
            font-weight: 800;
            color: var(--hv-text);
            margin: 0;
        }
        .hv-subtitle { color: var(--hv-muted); margin: 4px 0 0; }

        .hv-pill {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 6px 12px;
            border-radius: 999px;
            border: 1px solid var(--hv-border);
            background: rgba(255,255,255,.65);
            color: var(--hv-muted);
            font-weight: 600;
            font-size: .85rem;
        }

        .hv-hero-action {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            padding: 11px 18px;
            border-radius: 14px;
            border: 1px solid rgba(37,99,235,.18);
            background: #ffffff;
            color: var(--hv-primary);
            font-weight: 800;
            text-decoration: none;
            box-shadow: 0 8px 22px rgba(2, 6, 23, .08);
        }

        .hv-hero-action:hover {
            color: #1d4ed8;
            border-color: rgba(37,99,235,.28);
            text-decoration: none;
        }

        .hv-kpis { margin-top: 14px; }
        .hv-kpi {
            background: var(--hv-card);
            border: 1px solid var(--hv-border);
            border-radius: var(--hv-radius);
            box-shadow: 0 10px 26px rgba(2, 6, 23, .06);
            padding: 14px;
            height: 100%;
        }
        .hv-kpi .kpi-top { display:flex; align-items:center; justify-content:space-between; gap:10px; }
        .hv-kpi .kpi-label { color: var(--hv-muted); font-weight: 700; font-size: .85rem; }
        .hv-kpi .kpi-value { color: var(--hv-text); font-weight: 900; font-size: 1.25rem; margin-top: 6px; }
        .hv-kpi .kpi-icon {
            width: 42px; height: 42px;
            display:flex; align-items:center; justify-content:center;
            border-radius: 14px;
            background: rgba(37,99,235,.10);
            border: 1px solid rgba(37,99,235,.18);
            color: var(--hv-primary);
            font-size: 1.1rem;
        }
        .hv-kpi.kpi-success .kpi-icon { background: rgba(22,163,74,.10); border-color: rgba(22,163,74,.18); color: var(--hv-success); }
        .hv-kpi.kpi-warning .kpi-icon { background: rgba(245,158,11,.12); border-color: rgba(245,158,11,.18); color: var(--hv-warning); }
        .hv-kpi.kpi-danger .kpi-icon { background: rgba(239,68,68,.10); border-color: rgba(239,68,68,.18); color: var(--hv-danger); }

        .hv-card {
            background: var(--hv-card);
            border: 1px solid var(--hv-border);
            border-radius: var(--hv-radius);
            box-shadow: var(--hv-shadow);
        }

        .hv-card-header {
            padding: 14px 16px;
            border-bottom: 1px solid var(--hv-border);
            display:flex;
            align-items:center;
            justify-content:space-between;
            gap: 10px;
        }

        .hv-card-title {
            margin: 0;
            font-weight: 900;
            color: var(--hv-text);
            font-size: 1rem;
            display:flex;
            align-items:center;
            gap: 10px;
        }

        .hv-card-body { padding: 16px; }

        .hv-filter label {
            font-size: .80rem;
            font-weight: 800;
            color: var(--hv-muted);
            margin-bottom: 6px;
        }

        .hv-filter .form-control,
        .hv-filter .form-select {
            border-radius: 14px;
            border: 1px solid rgba(2, 6, 23, .12);
            padding: 10px 12px;
        }

        .hv-table-wrap {
            overflow: auto;
            border-radius: 16px;
            border: 1px solid rgba(2,6,23,.08);
        }

        .hv-table {
            margin: 0;
            min-width: 980px;
        }
        .hv-table thead th {
            position: sticky;
            top: 0;
            z-index: 2;
            background: #f2f5fb;
            color: rgba(11, 27, 51, .80);
            font-size: .82rem;
            font-weight: 900;
            border-bottom: 1px solid rgba(2,6,23,.10);
            white-space: nowrap;
        }
        .hv-table td {
            vertical-align: middle;
            font-size: .90rem;
            color: rgba(11, 27, 51, .88);
            border-color: rgba(2,6,23,.06);
            white-space: nowrap;
        }

        .hv-badge {
            display:inline-flex;
            align-items:center;
            gap:6px;
            padding: 6px 10px;
            border-radius: 999px;
            font-weight: 900;
            font-size: .78rem;
            border: 1px solid transparent;
        }
        .hv-badge.success { background: rgba(22,163,74,.10); color: var(--hv-success); border-color: rgba(22,163,74,.18); }
        .hv-badge.warning { background: rgba(245,158,11,.12); color: #a16207; border-color: rgba(245,158,11,.20); }
        .hv-badge.danger  { background: rgba(239,68,68,.10); color: var(--hv-danger); border-color: rgba(239,68,68,.18); }
        .hv-badge.info    { background: rgba(14,165,233,.10); color: #0369a1; border-color: rgba(14,165,233,.18); }
        .hv-badge.gray    { background: rgba(2,6,23,.06); color: rgba(11,27,51,.72); border-color: rgba(2,6,23,.10); }

        .hv-linkbtn {
            border-radius: 999px !important;
            padding: 8px 12px !important;
            font-weight: 900 !important;
        }

        .hv-money { font-variant-numeric: tabular-nums; font-weight: 900; }
        .hv-muted { color: var(--hv-muted); font-weight: 700; }
        .hv-empty { padding: 48px 16px; }
        .hv-empty i { font-size: 2rem; color: #94a3b8; }
        .hv-detail-table td, .hv-detail-table th { white-space: normal; }
        .cliente-modal .modal-dialog { max-width: 1120px; }
        .cliente-modal .modal-content { border-radius: 20px; overflow: hidden; border: 1px solid rgba(15, 23, 42, .14); box-shadow: 0 24px 48px rgba(15, 23, 42, .16); }
        .cliente-modal .modal-header { background: #1f2b3d; color: #fff; border-bottom: none; padding: 16px 20px; }
        .cliente-modal .modal-title { font-weight: 900; display: flex; align-items: center; gap: 10px; }
        .cliente-modal .btn-close { filter: invert(1); }
        .cliente-modal .modal-body { background: #f8fafc; padding: 16px; }
        .cliente-modal .form-label { font-size: .92rem; font-weight: 800; color: #1f2937; margin-bottom: 6px; }
        .cliente-modal .toolbar,
        .cliente-modal .surface,
        .cliente-modal .grid-shell {
            background: #fff;
            border: 1px solid #dbe3ef;
            border-radius: 16px;
            padding: 12px;
            box-shadow: 0 10px 22px rgba(15, 23, 42, .05);
        }
        .cliente-modal .btn-ghost { background: #111827; color: #fff; border: 1px solid #111827; border-radius: 12px; font-weight: 800; }
        .cliente-modal .btn-outline-ghost { background: #fff; color: #111827; border: 1px solid #6b7280; border-radius: 12px; font-weight: 700; }
        .cliente-modal .tabla-clientes thead th { background: #0f172a; color: #fff; border: 0; font-size: .84rem; }
        .cliente-modal .tabla-clientes tbody td { vertical-align: middle; }
        .cliente-modal .cliente-row { cursor: pointer; }
        .cliente-modal .cliente-row.selected { background: rgba(37, 99, 235, .12); outline: 2px solid rgba(37, 99, 235, .32); }
        .cliente-modal .cliente-row:hover { background: rgba(148, 163, 184, .08); }
    </style>

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
                        Filtra, consulta y revisa las facturas generadas en su turno activo.
                    </div>
                </div>
                <a href="caja.aspx" class="hv-hero-action">
                    <i class="bi bi-shop-window"></i>
                    Caja
                </a>
            </div>

            <div class="row g-3 hv-kpis">
                <div class="col-12 col-md-6 col-xl-3">
                    <div class="hv-kpi">
                        <div class="kpi-top">
                            <div class="kpi-label">Total ventas</div>
                            <div class="kpi-icon"><i class="bi bi-cash-stack"></i></div>
                        </div>
                        <div class="kpi-value"><%= Moneda(TotalVentas) %></div>
                        <div class="hv-muted">Periodo seleccionado</div>
                    </div>
                </div>
                <div class="col-12 col-md-6 col-xl-3">
                    <div class="hv-kpi kpi-success">
                        <div class="kpi-top">
                            <div class="kpi-label">Facturas</div>
                            <div class="kpi-icon"><i class="bi bi-receipt"></i></div>
                        </div>
                        <div class="kpi-value"><%= CantidadFacturas %></div>
                        <div class="hv-muted">Cantidad</div>
                    </div>
                </div>
                <div class="col-12 col-md-6 col-xl-3">
                    <div class="hv-kpi kpi-warning">
                        <div class="kpi-top">
                            <div class="kpi-label">Pendiente</div>
                            <div class="kpi-icon"><i class="bi bi-hourglass-split"></i></div>
                        </div>
                        <div class="kpi-value"><%= Moneda(TotalPendiente) %></div>
                        <div class="hv-muted">Cartera / credito</div>
                    </div>
                </div>
                <div class="col-12 col-md-6 col-xl-3">
                    <div class="hv-kpi kpi-danger">
                        <div class="kpi-top">
                            <div class="kpi-label">Anuladas</div>
                            <div class="kpi-icon"><i class="bi bi-x-octagon"></i></div>
                        </div>
                        <div class="kpi-value"><%= CantidadAnuladas %></div>
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
                    Ajusta filtros y consulta
                </span>
            </div>

            <div class="hv-card-body hv-filter">
                <div class="row g-3">
                    <div class="col-12 col-md-7">
                        <label>Buscar</label>
                        <div class="input-group">
                            <span class="input-group-text bg-white"><i class="bi bi-search"></i></span>
                            <input type="text" class="form-control" id="fBuscar" placeholder="Factura, cliente, NIT, alias, referencia..." />
                        </div>
                    </div>

                    <div class="col-12 col-md-5">
                        <label>Medio de pago</label>
                        <select class="form-select" id="fMedioPago">
                            <option value="">Todos</option>
                            <option>Efectivo</option>
                            <option>Tarjeta</option>
                            <option>Mixto</option>
                            <option>Transferencia</option>
                        </select>
                    </div>
                </div>
            </div>
        </div>

        <div class="hv-card mt-3">
            <div class="hv-card-header">
                <h2 class="hv-card-title">
                    <i class="bi bi-table"></i>
                    Facturas (vista rapida)
                </h2>

                <div class="d-flex flex-wrap gap-2 align-items-center">
                    <span class="hv-pill"><i class="bi bi-list-ul"></i><span id="hvResultadosLabel"><%= Ventas.Count %></span> registros</span>
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
                                <th>Medio</th>
                                <th class="text-end">Total</th>
                                <th class="text-end">Pagado</th>
                                <th class="text-end">Pendiente</th>
                                <th>Estado</th>
                                <th>FE</th>
                                <th>Resolución</th>
                                <th class="text-end">Acciones</th>
                            </tr>
                        </thead>
                        <tbody id="hvVentasBody">
                            <% if (Ventas != null && Ventas.Any()) { %>
                                <% foreach (var venta in Ventas) { %>
                                <tr class="hv-venta-row"
                                    data-search="<%= HttpUtility.HtmlAttributeEncode(VentaBusqueda(venta)) %>"
                                    data-medio="<%= HttpUtility.HtmlAttributeEncode((venta.medioDePago ?? string.Empty).ToLowerInvariant()) %>">
                                    <td>
                                        <span class="hv-badge gray">
                                            <i class="bi bi-hash"></i>
                                            <%= venta.id %>
                                        </span>
                                    </td>
                                    <td>
                                        <div class="fw-bold"><%= FechaLarga(venta.fechaVenta) %></div>
                                        <div class="hv-muted"><%= HoraLarga(venta.fechaVenta) %></div>
                                    </td>
                                    <td>
                                        <div class="fw-bold"><%= FacturaLabel(venta) %></div>
                                        <div class="hv-muted">Alias: <%= string.IsNullOrWhiteSpace(venta.aliasVenta) ? "Sin alias" : venta.aliasVenta %></div>
                                    </td>
                                    <td>
                                        <div class="fw-bold"><%= string.IsNullOrWhiteSpace(venta.nombreCliente) ? "Cliente contado" : venta.nombreCliente %></div>
                                        <div class="hv-muted">NIT: <%= string.IsNullOrWhiteSpace(venta.nit) ? "0" : venta.nit %></div>
                                    </td>
                                    <td>
                                        <span class="hv-badge <%= MedioClase(venta.medioDePago) %>"><i class="bi bi-cash-coin"></i><%= string.IsNullOrWhiteSpace(venta.medioDePago) ? "Sin definir" : venta.medioDePago %></span>
                                    </td>
                                    <td class="text-end hv-money"><%= Moneda(venta.total_A_Pagar) %></td>
                                    <td class="text-end hv-money"><%= Moneda(venta.totalPagadoVenta) %></td>
                                    <td class="text-end hv-money"><%= Moneda(venta.totalPendienteVenta) %></td>
                                    <td>
                                        <span class="hv-badge <%= EstadoClase(venta) %>"><i class="bi bi-check2-circle"></i><%= EstadoTexto(venta) %></span>
                                    </td>
                                    <td>
                                        <span class="hv-badge <%= FeClase(venta) %>"><i class="bi <%= FeIcono(venta) %>"></i><%= FeTexto(venta) %></span>
                                    </td>
                                    <td>
                                        <span class="hv-badge gray"><i class="bi bi-hash"></i><%= venta.idResolucion > 0 ? venta.idResolucion.ToString() : "Sin asignar" %></span>
                                    </td>
                                    <td class="text-end">
                                        <button type="button" class="btn btn-outline-primary btn-sm hv-linkbtn me-2" onclick="hvAbrirVenta(<%= venta.id %>)" data-bs-toggle="modal" data-bs-target="#mdlVenta">
                                            <i class="bi bi-eye me-1"></i>Ver
                                        </button>
                                        <% if (PuedeEditarResolucion(venta)) { %>
                                        <button type="button" class="btn btn-outline-success btn-sm hv-linkbtn me-2" onclick="hvEditarResolucion(<%= venta.id %>, <%= venta.idResolucion %>); return false;">
                                            <i class="bi bi-pencil-square me-1"></i>Editar resolución
                                        </button>
                                        <% } %>
                                        <% if (PuedeEditarCliente(venta)) { %>
                                        <button type="button" class="btn btn-outline-warning btn-sm hv-linkbtn me-2" onclick="hvEditarCliente(<%= venta.id %>, <%= venta.idCliente %>); return false;">
                                            <i class="bi bi-person-gear me-1"></i>Editar cliente
                                        </button>
                                        <% } %>
                                        <% if (PuedeReenviarDian(venta)) { %>
                                        <button type="button" class="btn btn-outline-danger btn-sm hv-linkbtn me-2" onclick="hvReenviarDian(<%= venta.id %>); return false;">
                                            <i class="bi bi-send-check me-1"></i>Reenviar DIAN
                                        </button>
                                        <% } %>
                                        <button type="button" class="btn btn-outline-secondary btn-sm hv-linkbtn" onclick="hvImprimirVenta(<%= venta.id %>); return false;">
                                            <i class="bi bi-printer me-1"></i>Imprimir
                                        </button>
                                    </td>
                                </tr>
                                <% } %>
                            <% } else { %>
                                <tr id="hvSinDatosServidor">
                                    <td colspan="12" class="text-center hv-empty">
                                        <div class="hv-muted">
                                            <i class="bi bi-inboxes d-block mb-2"></i>
                                            No hay ventas registradas en la base activa del turno.
                                        </div>
                                    </td>
                                </tr>
                            <% } %>
                            <tr id="hvSinResultados" style="display:none;">
                                <td colspan="12" class="text-center hv-empty">
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

    <div class="modal fade" id="mdlVenta" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content hv-card">
                <div class="hv-card-header">
                    <h3 class="hv-card-title">
                        <i class="bi bi-receipt-cutoff"></i>
                        Detalle de la venta
                    </h3>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>

                <div class="hv-card-body">
                    <div class="row g-3 mb-3">
                        <div class="col-md-3">
                            <div class="hv-muted mb-1">Factura</div>
                            <div class="fw-bold" id="hvDetalleFactura">-</div>
                        </div>
                        <div class="col-md-3">
                            <div class="hv-muted mb-1">Cliente</div>
                            <div class="fw-bold" id="hvDetalleCliente">-</div>
                        </div>
                        <div class="col-md-3">
                            <div class="hv-muted mb-1">Fecha</div>
                            <div class="fw-bold" id="hvDetalleFecha">-</div>
                        </div>
                        <div class="col-md-3">
                            <div class="hv-muted mb-1">Total</div>
                            <div class="fw-bold" id="hvDetalleTotal">-</div>
                        </div>
                        <div class="col-md-4">
                            <div class="hv-muted mb-1">NIT</div>
                            <div class="fw-bold" id="hvDetalleNit">-</div>
                        </div>
                        <div class="col-md-4">
                            <div class="hv-muted mb-1">Medio de pago</div>
                            <div class="fw-bold" id="hvDetalleMedio">-</div>
                        </div>
                        <div class="col-md-4">
                            <div class="hv-muted mb-1">Estado</div>
                            <div class="fw-bold" id="hvDetalleEstado">-</div>
                        </div>
                        <div class="col-12">
                            <div class="alert alert-light border mb-0">
                                <strong>Observacion:</strong>
                                <span id="hvDetalleObservacion">Sin observacion</span>
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
                                <tr>
                                    <td colspan="6" class="text-center py-4 hv-muted">Seleccione una venta para ver el detalle.</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <div class="d-flex justify-content-end gap-2 mt-3">
                        <button type="button" class="btn btn-outline-secondary rounded-pill px-4" data-bs-dismiss="modal">
                            <i class="bi bi-x-lg me-2"></i>Cerrar
                        </button>
                        <button type="button" class="btn btn-primary rounded-pill px-4" onclick="hvImprimirVentaActual(); return false;">
                            <i class="bi bi-printer me-2"></i>Imprimir
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="mdlResolucionVenta" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content hv-card">
                <div class="hv-card-header">
                    <h3 class="hv-card-title">
                        <i class="bi bi-pencil-square"></i>
                        Editar resolución
                    </h3>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="hv-card-body">
                    <asp:HiddenField ID="hfVentaResolucionId" runat="server" />
                    <div class="mb-3">
                        <label class="form-label hv-muted">Resolución disponible</label>
                        <asp:DropDownList ID="ddlResolucionEditar" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>
                    <div class="alert alert-light border mb-0">
                        Selecciona la resolución y se actualizará el campo <strong>idResolucion</strong> de la venta.
                    </div>
                    <div class="d-flex justify-content-end gap-2 mt-3">
                        <button type="button" class="btn btn-outline-secondary rounded-pill px-4" data-bs-dismiss="modal">
                            <i class="bi bi-x-lg me-2"></i>Cancelar
                        </button>
                        <asp:Button ID="btnGuardarResolucion" runat="server" CssClass="btn btn-success rounded-pill px-4" Text="Guardar resolución" OnClick="btnGuardarResolucion_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade cliente-modal" id="mdlClienteVenta" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-xl">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title"><i class="bi bi-people-fill"></i> Gestionar Cliente</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hfVentaClienteId" runat="server" />
                    <asp:HiddenField ID="hfClienteEditarSeleccionadoId" runat="server" />

                    <div class="toolbar mb-3">
                        <div class="row g-2 align-items-end">
                            <div class="col-12 col-lg-7">
                                <label class="form-label">Filtrar cliente por Nombre</label>
                                <input type="text" class="form-control" id="hvFiltroClienteNombre" placeholder="Buscar por nombre o razón social" />
                            </div>
                            <div class="col-12 col-lg-3">
                                <label class="form-label">Buscar Documento</label>
                                <input type="text" class="form-control" id="hvBuscarDocumentoCliente" placeholder="Número de identificación" />
                            </div>
                            <div class="col-12 col-lg-2 d-grid">
                                <button type="button" class="btn btn-ghost" id="hvBtnBuscarClienteDocumento">
                                    <i class="bi bi-search me-1"></i> Buscar
                                </button>
                            </div>
                        </div>
                    </div>

                    <div class="surface mb-3">
                        <div class="row g-2">
                            <div class="col-12 col-md-3">
                                <label class="form-label">Tipo Documento *</label>
                                <asp:DropDownList ID="ddlTipoDocumentoHv" runat="server" CssClass="form-select" ClientIDMode="Static"></asp:DropDownList>
                            </div>
                            <div class="col-12 col-md-3">
                                <label class="form-label">Número de identificación *</label>
                                <input type="text" class="form-control" id="hvTxtIdentificacionCliente" />
                            </div>
                            <div class="col-12 col-md-3">
                                <label class="form-label">Tipo Organización *</label>
                                <asp:DropDownList ID="ddlTipoOrganizacionHv" runat="server" CssClass="form-select" ClientIDMode="Static"></asp:DropDownList>
                            </div>
                            <div class="col-12 col-md-3">
                                <label class="form-label">Municipio *</label>
                                <asp:DropDownList ID="ddlMunicipioHv" runat="server" CssClass="form-select" ClientIDMode="Static"></asp:DropDownList>
                            </div>
                        </div>
                        <div class="row g-2 mt-1">
                            <div class="col-12 col-md-3">
                                <label class="form-label">Tipo Régimen *</label>
                                <asp:DropDownList ID="ddlTipoRegimenHv" runat="server" CssClass="form-select" ClientIDMode="Static"></asp:DropDownList>
                            </div>
                            <div class="col-12 col-md-3">
                                <label class="form-label">Tipo de responsabilidad *</label>
                                <asp:DropDownList ID="ddlTipoResponsabilidadHv" runat="server" CssClass="form-select" ClientIDMode="Static"></asp:DropDownList>
                            </div>
                            <div class="col-12 col-md-6">
                                <label class="form-label">Detalle de Impuesto *</label>
                                <asp:DropDownList ID="ddlDetalleImpuestoHv" runat="server" CssClass="form-select" ClientIDMode="Static"></asp:DropDownList>
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
                                        <i class="bi bi-floppy2-fill me-1"></i> <span id="hvLblGuardarCliente">Guardar</span>
                                    </button>
                                    <button type="button" class="btn btn-outline-ghost" id="hvBtnLimpiarClienteFormulario">
                                        <i class="bi bi-brush me-1"></i> Limpiar
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
                                        <th style="width:18%">Documento</th>
                                        <th style="width:18%">NIT</th>
                                        <th style="width:39%">Nombre Cliente</th>
                                        <th style="width:25%">Correo</th>
                                    </tr>
                                </thead>
                                <tbody id="hvClientesBody">
                                    <% if (ClientesDisponibles != null && ClientesDisponibles.Any()) { %>
                                        <% foreach (var cliente in ClientesDisponibles) { %>
                                        <tr class="cliente-row hv-cliente-row"
                                            tabindex="0"
                                            data-cliente-id="<%= cliente.id %>"
                                            data-type-doc-id="<%= cliente.typeDocumentIdentification_id %>"
                                            data-org-id="<%= cliente.typeOrganization_id %>"
                                            data-municipio-id="<%= cliente.municipality_id %>"
                                            data-regimen-id="<%= cliente.typeRegime_id %>"
                                            data-responsabilidad-id="<%= cliente.typeLiability_id %>"
                                            data-impuesto-id="<%= cliente.typeTaxDetail_id %>"
                                            data-nit="<%= HttpUtility.HtmlAttributeEncode(cliente.identificationNumber ?? string.Empty) %>"
                                            data-nombre="<%= HttpUtility.HtmlAttributeEncode(cliente.nameCliente ?? string.Empty) %>"
                                            data-correo="<%= HttpUtility.HtmlAttributeEncode(cliente.email ?? string.Empty) %>"
                                            data-comercio="<%= HttpUtility.HtmlAttributeEncode(cliente.tradeName ?? string.Empty) %>"
                                            data-telefono="<%= HttpUtility.HtmlAttributeEncode(cliente.phone ?? string.Empty) %>"
                                            data-direccion="<%= HttpUtility.HtmlAttributeEncode(cliente.adress ?? string.Empty) %>"
                                            data-matricula="<%= HttpUtility.HtmlAttributeEncode(cliente.merchantRegistration ?? string.Empty) %>"
                                            data-es-cliente="<%= cliente.idTipoTercero == 1 || cliente.idTipoTercero == 3 ? "1" : "0" %>"
                                            data-es-proveedor="<%= cliente.idTipoTercero == 2 || cliente.idTipoTercero == 3 ? "1" : "0" %>">
                                            <td><%= NombreTipoDocumentoCliente(cliente.typeDocumentIdentification_id) %></td>
                                            <td><%= string.IsNullOrWhiteSpace(cliente.identificationNumber) ? "-" : cliente.identificationNumber %></td>
                                            <td><%= string.IsNullOrWhiteSpace(cliente.nameCliente) ? "Cliente" : cliente.nameCliente %></td>
                                            <td><%= string.IsNullOrWhiteSpace(cliente.email) ? "-" : cliente.email %></td>
                                        </tr>
                                        <% } %>
                                    <% } else { %>
                                        <tr>
                                            <td colspan="4" class="text-center py-4 text-muted">No hay clientes disponibles.</td>
                                        </tr>
                                    <% } %>
                                </tbody>
                            </table>
                        </div>
                    </div>

                    <div class="text-center mt-3">
                        <button type="button" id="hvBtnSeleccionarCliente" class="btn btn-outline-ghost px-4" disabled>
                            <i class="bi bi-check2-square me-1"></i> Seleccionar cliente
                        </button>
                    </div>

                    <div class="d-flex justify-content-end gap-2 mt-3 flex-wrap">
                        <button type="button" class="btn btn-outline-secondary rounded-pill px-4" data-bs-dismiss="modal">
                            <i class="bi bi-x-lg me-2"></i>Cancelar
                        </button>
                        <asp:DropDownList ID="ddlClienteEditar" runat="server" CssClass="d-none"></asp:DropDownList>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script>
        window.hvVentasDetalle = <%= VentasDetalleJson %>;

        (function () {
            const txtBuscar = document.getElementById('fBuscar');
            const selMedio = document.getElementById('fMedioPago');
            const rows = Array.from(document.querySelectorAll('.hv-venta-row'));
            const sinResultados = document.getElementById('hvSinResultados');
            const labelResultados = document.getElementById('hvResultadosLabel');

            function aplicarFiltros() {
                const texto = (txtBuscar?.value || '').trim().toLowerCase();
                const medio = (selMedio?.value || '').trim().toLowerCase();
                let visibles = 0;

                rows.forEach(function (row) {
                    const cumpleTexto = !texto || (row.dataset.search || '').indexOf(texto) >= 0;
                    const cumpleMedio = !medio || (row.dataset.medio || '') === medio;
                    const visible = cumpleTexto && cumpleMedio;
                    row.style.display = visible ? '' : 'none';
                    if (visible) visibles += 1;
                });

                if (labelResultados) {
                    labelResultados.textContent = visibles.toString();
                }

                if (sinResultados) {
                    sinResultados.style.display = visibles === 0 && rows.length > 0 ? '' : 'none';
                }
            }

            if (txtBuscar) txtBuscar.addEventListener('input', aplicarFiltros);
            if (selMedio) selMedio.addEventListener('change', aplicarFiltros);
            aplicarFiltros();
        })();

        function hvEditarResolucion(idVenta, idResolucion) {
            var hidden = document.getElementById("<%= hfVentaResolucionId.ClientID %>");
            var ddl = document.getElementById("<%= ddlResolucionEditar.ClientID %>");
            if (hidden) hidden.value = idVenta || 0;
            if (ddl) ddl.value = idResolucion && Number(idResolucion) > 0 ? String(idResolucion) : "";
            var modalEl = document.getElementById("mdlResolucionVenta");
            if (!modalEl || !window.bootstrap || !window.bootstrap.Modal) { return; }
            var modal = window.bootstrap.Modal.getOrCreateInstance(modalEl);
        }

        function hvMostrarModalCliente() {
            var modalEl = document.getElementById("mdlClienteVenta");
            if (!modalEl) { return; }
            var intentos = 0;
            function abrir() {
                if (window.bootstrap && window.bootstrap.Modal) {
                    var modal = window.bootstrap.Modal.getOrCreateInstance(modalEl);
                    try { sessionStorage.removeItem("hvClienteVentaStayOpen"); } catch (e) { }
                    return;
                }
                intentos += 1;
                if (intentos < 30) {
                    window.setTimeout(abrir, 120);
                }
            }
            abrir();
        }

        function hvEditarCliente(idVenta, idCliente) {
            var hidden = document.getElementById("<%= hfVentaClienteId.ClientID %>");
            var ddl = document.getElementById("<%= ddlClienteEditar.ClientID %>");
            var selected = document.getElementById("<%= hfClienteEditarSeleccionadoId.ClientID %>");
            if (hidden) hidden.value = idVenta || 0;
            if (ddl) ddl.value = "";
            if (selected) selected.value = "";
            if (window.hvClienteModal && typeof window.hvClienteModal.limpiar === 'function') { window.hvClienteModal.limpiar(); }
            hvMostrarModalCliente();
            return;
        }

        const hvClienteModal = (function () {
            const modalClienteEl = document.getElementById('mdlClienteVenta');
            const clienteTable = modalClienteEl ? modalClienteEl.querySelector('table') : null;
            const filtroNombre = document.getElementById('hvFiltroClienteNombre');
            const filtroDoc = document.getElementById('hvBuscarDocumentoCliente');
            const btnBuscar = document.getElementById('hvBtnBuscarClienteDocumento');
            const btnSeleccionar = document.getElementById('hvBtnSeleccionarCliente');
            const btnGuardar = document.getElementById('hvBtnGuardarClienteCatalogo');
            const btnLimpiar = document.getElementById('hvBtnLimpiarClienteFormulario');
            const hiddenVenta = document.getElementById("<%= hfVentaClienteId.ClientID %>");
            const hiddenCliente = document.getElementById("<%= hfClienteEditarSeleccionadoId.ClientID %>");
            const ddlCliente = document.getElementById("<%= ddlClienteEditar.ClientID %>");
            const modalStateKey = 'hvClienteVentaStayOpen';

            const byId = function (id) { return document.getElementById(id); };
            const filasClientes = function () { return clienteTable ? Array.from(clienteTable.querySelectorAll('.cliente-row')) : []; };
            const setInput = function (id, value) { var el = byId(id); if (el) el.value = value || ''; };
            const setCheck = function (id, value) { var el = byId(id); if (el) el.checked = !!value; };
            const setGuardarLabel = function (label) {
                var lbl = byId('hvLblGuardarCliente');
                if (lbl && label) lbl.textContent = label;
            };
            const setSelect = function (id, value) {
                var el = byId(id);
                if (!el) return;
                var val = (value || '').toString().trim();
                if (!val) {
                    el.value = '';
                    return;
                }
                var option = Array.from(el.options).find(function (o) { return o.value === val; });
                if (option) {
                    el.value = val;
                    return;
                }
                var opt = document.createElement('option');
                opt.value = val;
                opt.textContent = val;
                el.appendChild(opt);
                el.value = val;
            };

            const aplicarFiltroClientes = function () {
                var texto = ((filtroNombre && filtroNombre.value) || '').toUpperCase().trim();
                var doc = ((filtroDoc && filtroDoc.value) || '').toUpperCase().trim();
                filasClientes().forEach(function (row) {
                    var nombre = (row.dataset.nombre || '').toUpperCase();
                    var nit = (row.dataset.nit || '').toUpperCase();
                    row.style.display = nombre.indexOf(texto) >= 0 && nit.indexOf(doc) >= 0 ? '' : 'none';
                });
            };

            const aplicarDatosCliente = function (data) {
                if (!data) return;
                if (hiddenCliente) hiddenCliente.value = (data.clienteId || '').toString();
                if (ddlCliente) ddlCliente.value = (data.clienteId || '').toString();
                setSelect('ddlTipoDocumentoHv', data.typeDocId);
                setInput('hvTxtIdentificacionCliente', data.nit);
                setInput('hvBuscarDocumentoCliente', data.nit);
                setSelect('ddlTipoOrganizacionHv', data.orgId);
                setSelect('ddlMunicipioHv', data.municipioId);
                setSelect('ddlTipoRegimenHv', data.regimenId);
                setSelect('ddlTipoResponsabilidadHv', data.responsabilidadId);
                setSelect('ddlDetalleImpuestoHv', data.impuestoId);
                setInput('hvTxtNombreCliente', data.nombre);
                setInput('hvFiltroClienteNombre', data.nombre);
                aplicarFiltroClientes();
                setInput('hvTxtNombreComercioCliente', data.comercio);
                setInput('hvTxtTelefonoCliente', data.telefono);
                setInput('hvTxtDireccionCliente', data.direccion);
                setInput('hvTxtCorreoCliente', data.correo);
                setInput('hvTxtMatriculaCliente', data.matricula);
                setCheck('hvChkClientesModal', data.esCliente !== false);
                setCheck('hvChkProveedoresModal', !!data.esProveedor);
                if (data.actionLabel) setGuardarLabel(data.actionLabel);
            };

            const pintarSeleccion = function (row) {
                if (!row || !clienteTable) return;
                clienteTable.querySelectorAll('.cliente-row').forEach(function (item) { item.classList.remove('selected'); });
                row.classList.add('selected');
                row.focus();
                var clienteId = row.dataset.clienteId || '';
                if (hiddenCliente) hiddenCliente.value = clienteId;
                if (ddlCliente) ddlCliente.value = clienteId;
                if (btnSeleccionar) btnSeleccionar.disabled = !clienteId;

                aplicarDatosCliente({
                    clienteId: clienteId,
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
                    esCliente: row.dataset.esCliente === '1',
                    esProveedor: row.dataset.esProveedor === '1',
                    actionLabel: 'Editar'
                });
            };

            const limpiarFormulario = function () {
                aplicarDatosCliente({
                    clienteId: 0,
                    typeDocId: '',
                    nit: '',
                    orgId: '',
                    municipioId: '',
                    regimenId: '',
                    responsabilidadId: '',
                    impuestoId: '',
                    nombre: '',
                    comercio: '',
                    telefono: '',
                    direccion: '',
                    correo: '',
                    matricula: '',
                    esCliente: true,
                    esProveedor: false,
                    actionLabel: 'Guardar'
                });
                if (clienteTable) clienteTable.querySelectorAll('.cliente-row').forEach(function (item) { item.classList.remove('selected'); });
                if (btnSeleccionar) btnSeleccionar.disabled = true;
                setInput('hvBuscarDocumentoCliente', '');
                setInput('hvFiltroClienteNombre', '');
                aplicarFiltroClientes();
            };

            const preseleccionar = function () {
                aplicarFiltroClientes();
                var ddlTipoDocumento = byId('ddlTipoDocumentoHv');
                if (ddlTipoDocumento) {
                    var nitOption = Array.from(ddlTipoDocumento.options).find(function (opt) {
                        return (opt.textContent || '').trim().toUpperCase() === 'NIT';
                    });
                    if (nitOption && !ddlTipoDocumento.value) ddlTipoDocumento.value = nitOption.value;
                }
                var id = ((hiddenCliente && hiddenCliente.value) || (ddlCliente && ddlCliente.value) || '').trim();
                if (!id) return;
                var row = filasClientes().find(function (item) { return (item.dataset.clienteId || '') === id; }) || null;
                if (row) pintarSeleccion(row);
            };

            if (filtroNombre) {
                filtroNombre.addEventListener('input', function () {
                    filtroNombre.value = filtroNombre.value.toUpperCase();
                    aplicarFiltroClientes();
                });
            }
            if (filtroDoc) filtroDoc.addEventListener('input', aplicarFiltroClientes);

            if (modalClienteEl) {
                modalClienteEl.addEventListener('shown.bs.modal', function () {
                    preseleccionar();
                });
            }
            if (clienteTable) {
                clienteTable.addEventListener('click', function (event) {
                    var row = event.target.closest('.cliente-row');
                    if (!row) return;
                    pintarSeleccion(row);
                });

                clienteTable.addEventListener('keydown', function (event) {
                    var row = event.target.closest('.cliente-row');
                    if (!row) return;
                    var rows = filasClientes().filter(function (item) { return item.style.display !== 'none'; });
                    var index = rows.indexOf(row);
                    if (index === -1) return;
                    if (event.key === 'ArrowDown') {
                        event.preventDefault();
                        pintarSeleccion(rows[Math.min(index + 1, rows.length - 1)]);
                    }
                    if (event.key === 'ArrowUp') {
                        event.preventDefault();
                        pintarSeleccion(rows[Math.max(index - 1, 0)]);
                    }
                    if (event.key === 'Enter' || event.key === ' ') {
                        event.preventDefault();
                        pintarSeleccion(row);
                    }
                });
            }

            if (btnBuscar) {
                btnBuscar.addEventListener('click', function () {
                    var nit = ((filtroDoc && filtroDoc.value) || '').trim();
                    var tipoDoc = byId('ddlTipoDocumentoHv');
                    var tipoDocVal = tipoDoc ? (tipoDoc.value || '') : '';
                    if (!nit || !tipoDocVal) {
                        Swal.fire({ icon: 'error', title: 'Falta información', text: 'Para buscar el NIT debes seleccionar el tipo de documento.', confirmButtonColor: '#2563eb' });
                        return;
                    }
                    try { sessionStorage.setItem(modalStateKey, '1'); } catch (e) { }
                    if (typeof window.__doPostBack === 'function') window.__doPostBack('BuscarNitClienteHV', nit);
                });
            }

            if (btnSeleccionar) {
                btnSeleccionar.addEventListener('click', function () {
                    var selected = document.querySelector('#mdlClienteVenta .cliente-row.selected');
                    var clienteId = selected && selected.dataset ? selected.dataset.clienteId : '';
                    if (!clienteId) {
                        Swal.fire({ icon: 'warning', title: 'Selecciona un cliente', text: 'Debes seleccionar un cliente antes de continuar.', confirmButtonColor: '#2563eb' });
                        return;
                    }
                    if (typeof window.__doPostBack === 'function') window.__doPostBack('SeleccionarClienteFacturaHV', clienteId);
                });
            }

            if (btnGuardar) {
                btnGuardar.addEventListener('click', function () {
                    var selected = document.querySelector('#mdlClienteVenta .cliente-row.selected');
                    var payload = {
                        idVenta: parseInt((hiddenVenta && hiddenVenta.value) || '0', 10) || 0,
                        clienteId: parseInt((selected && selected.dataset ? selected.dataset.clienteId : '0'), 10) || parseInt((hiddenCliente && hiddenCliente.value) || '0', 10) || 0,
                        typeDocId: parseInt((byId('ddlTipoDocumentoHv') && byId('ddlTipoDocumentoHv').value) || '0', 10) || 0,
                        nit: ((byId('hvTxtIdentificacionCliente') && byId('hvTxtIdentificacionCliente').value) || '').trim(),
                        orgId: parseInt((byId('ddlTipoOrganizacionHv') && byId('ddlTipoOrganizacionHv').value) || '0', 10) || 0,
                        municipioId: parseInt((byId('ddlMunicipioHv') && byId('ddlMunicipioHv').value) || '0', 10) || 0,
                        regimenId: parseInt((byId('ddlTipoRegimenHv') && byId('ddlTipoRegimenHv').value) || '0', 10) || 0,
                        responsabilidadId: parseInt((byId('ddlTipoResponsabilidadHv') && byId('ddlTipoResponsabilidadHv').value) || '0', 10) || 0,
                        impuestoId: parseInt((byId('ddlDetalleImpuestoHv') && byId('ddlDetalleImpuestoHv').value) || '0', 10) || 0,
                        nombre: ((byId('hvTxtNombreCliente') && byId('hvTxtNombreCliente').value) || '').trim(),
                        comercio: ((byId('hvTxtNombreComercioCliente') && byId('hvTxtNombreComercioCliente').value) || '').trim(),
                        telefono: ((byId('hvTxtTelefonoCliente') && byId('hvTxtTelefonoCliente').value) || '').trim(),
                        direccion: ((byId('hvTxtDireccionCliente') && byId('hvTxtDireccionCliente').value) || '').trim(),
                        correo: ((byId('hvTxtCorreoCliente') && byId('hvTxtCorreoCliente').value) || '').trim(),
                        matricula: ((byId('hvTxtMatriculaCliente') && byId('hvTxtMatriculaCliente').value) || '').trim(),
                        esCliente: !!(byId('hvChkClientesModal') && byId('hvChkClientesModal').checked),
                        esProveedor: !!(byId('hvChkProveedoresModal') && byId('hvChkProveedoresModal').checked)
                    };

                    if (!payload.typeDocId || !payload.orgId || !payload.municipioId || !payload.regimenId || !payload.responsabilidadId || !payload.impuestoId || !payload.nit || !payload.nombre) {
                        Swal.fire({ icon: 'warning', title: 'Información incompleta', text: 'Completa los campos obligatorios del cliente antes de guardar.', confirmButtonColor: '#2563eb' });
                        return;
                    }

                    try { sessionStorage.setItem(modalStateKey, '1'); } catch (e) { }
                    var arg = btoa(unescape(encodeURIComponent(JSON.stringify(payload))));
                    if (typeof window.__doPostBack === 'function') window.__doPostBack('GuardarClienteFacturaHV', arg);
                });
            }

            if (btnLimpiar) btnLimpiar.addEventListener('click', limpiarFormulario);

            try {
                if (modalClienteEl && sessionStorage.getItem(modalStateKey) === '1') {
                    hvMostrarModalCliente();
                }
            } catch (e) { }

            return { preseleccionar: preseleccionar, setData: aplicarDatosCliente, limpiar: limpiarFormulario };
        })();

        window.hvClienteModal = hvClienteModal;
        window.setHvClienteData = function (data) { hvClienteModal.setData(data); };
        function hvMoneda(valor) {
            const numero = Number(valor || 0);
            try {
                return numero.toLocaleString('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 });
            } catch (e) {
                return '$ ' + numero.toFixed(0);
            }
        }

        window.hvVentaActualId = 0;

        function hvImprimirVenta(idVenta) {
            if (!idVenta) {
                return;
            }

            if (typeof window.__doPostBack === 'function') {
                window.__doPostBack('ImprimirFacturaHV', idVenta.toString());
            }
        }

        function hvReenviarDian(idVenta) {
            if (!idVenta) {
                return;
            }

            if (typeof window.__doPostBack === 'function') {
                window.__doPostBack('ReenviarFacturaDianHV', idVenta.toString());
            }
        }

        function hvImprimirVentaActual() {
            hvImprimirVenta(window.hvVentaActualId || 0);
        }

        function hvAbrirVenta(idVenta) {
            window.hvVentaActualId = idVenta;
            const venta = window.hvVentasDetalle ? window.hvVentasDetalle[idVenta] : null;
            if (!venta) {
                return;
            }

            document.getElementById('hvDetalleFactura').textContent = venta.factura || '-';
            document.getElementById('hvDetalleCliente').textContent = venta.cliente || '-';
            document.getElementById('hvDetalleFecha').textContent = ((venta.fecha || '-') + ' ' + (venta.hora || '')).trim();
            document.getElementById('hvDetalleTotal').textContent = hvMoneda(venta.total);
            document.getElementById('hvDetalleNit').textContent = venta.nit || '0';
            document.getElementById('hvDetalleMedio').textContent = venta.medioPago || 'Sin definir';
            document.getElementById('hvDetalleEstado').textContent = venta.estado || '-';
            document.getElementById('hvDetalleObservacion').textContent = venta.observacion || 'Sin observacion';

            const body = document.getElementById('hvDetalleBody');
            if (!body) {
                return;
            }

            const detalles = Array.isArray(venta.detalles) ? venta.detalles : [];
            if (!detalles.length) {
                body.innerHTML = '<tr><td colspan="6" class="text-center py-4 hv-muted">Esta venta no tiene detalle cargado.</td></tr>';
                return;
            }

            body.innerHTML = detalles.map(function (item) {
                const producto = [item.producto || 'Producto', item.presentacion || ''].filter(Boolean).join(' / ');
                return '<tr>' +
                    '<td><strong>' + producto + '</strong></td>' +
                    '<td>' + (item.cuenta || 'General') + '</td>' +
                    '<td class="text-end hv-money">' + (item.cantidad || 0) + '</td>' +
                    '<td class="text-end hv-money">' + hvMoneda(item.precio) + '</td>' +
                    '<td class="text-end hv-money">' + hvMoneda(item.total) + '</td>' +
                    '<td>' + (item.nota || 'Sin nota') + '</td>' +
                    '</tr>';
            }).join('');
        }
    </script>

</asp:Content>

























