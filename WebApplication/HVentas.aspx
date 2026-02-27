<%@ Page Title="Historial de Ventas" Language="C#" MasterPageFile="~/Menu.Master" AutoEventWireup="true"
    CodeBehind="HVentas.aspx.cs" Inherits="WebApplication.HVentas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- ✅ Asegúrate que en tu Master ya estén Bootstrap 5 + Bootstrap Icons.
         Si NO están, descomenta estas líneas:
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
    -->

    <style>
        /* =========================
           HISTORIAL VENTAS - UI PRO
           ========================= */
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
            padding: 18px 18px;
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

        .hv-kpis { margin-top: 14px; }
        .hv-kpi {
            background: var(--hv-card);
            border: 1px solid var(--hv-border);
            border-radius: var(--hv-radius);
            box-shadow: 0 10px 26px rgba(2, 6, 23, .06);
            padding: 14px 14px;
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
        .hv-kpi.kpi-danger  .kpi-icon { background: rgba(239,68,68,.10); border-color: rgba(239,68,68,.18); color: var(--hv-danger); }

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

        .hv-actions .btn {
            border-radius: 999px;
            padding: 10px 14px;
            font-weight: 800;
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
    </style>

    <div class="container-fluid hv-wrap">

        <!-- ✅ Encabezado -->
        <div class="hv-hero">
            <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
                <div>
                    <div class="hv-pill mb-2">
                        <i class="bi bi-receipt-cutoff"></i>
                        Historial / Ventas
                    </div>
                    <h1 class="hv-title mb-1">Historial de Ventas</h1>
                    <div class="hv-subtitle">
                        Filtra, consulta y revisa las facturas generadas en el su turno.
                    </div>
                </div>

            </div>

            <!-- ✅ KPIs -->
            <div class="row g-3 hv-kpis">
                <div class="col-12 col-md-6 col-xl-3">
                    <div class="hv-kpi">
                        <div class="kpi-top">
                            <div class="kpi-label">Total Ventas</div>
                            <div class="kpi-icon"><i class="bi bi-cash-stack"></i></div>
                        </div>
                        <div class="kpi-value">$ 0</div>
                        <div class="hv-muted">Periodo seleccionado</div>
                    </div>
                </div>
                <div class="col-12 col-md-6 col-xl-3">
                    <div class="hv-kpi kpi-success">
                        <div class="kpi-top">
                            <div class="kpi-label">Facturas</div>
                            <div class="kpi-icon"><i class="bi bi-receipt"></i></div>
                        </div>
                        <div class="kpi-value">0</div>
                        <div class="hv-muted">Cantidad</div>
                    </div>
                </div>
                <div class="col-12 col-md-6 col-xl-3">
                    <div class="hv-kpi kpi-warning">
                        <div class="kpi-top">
                            <div class="kpi-label">Pendiente</div>
                            <div class="kpi-icon"><i class="bi bi-hourglass-split"></i></div>
                        </div>
                        <div class="kpi-value">$ 0</div>
                        <div class="hv-muted">Cartera / crédito</div>
                    </div>
                </div>
                <div class="col-12 col-md-6 col-xl-3">
                    <div class="hv-kpi kpi-danger">
                        <div class="kpi-top">
                            <div class="kpi-label">Anuladas</div>
                            <div class="kpi-icon"><i class="bi bi-x-octagon"></i></div>
                        </div>
                        <div class="kpi-value">0</div>
                        <div class="hv-muted">Registros anulados</div>
                    </div>
                </div>
            </div>
        </div>

        <!-- ✅ Filtros -->
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


                                        <div class="col-12 col-md-6">
                        <label>Buscar</label>
                        <div class="input-group">
                            <span class="input-group-text bg-white"><i class="bi bi-search"></i></span>
                            <input type="text" class="form-control" id="fBuscar"
                                   placeholder="Factura, cliente, NIT, alias, referencia…" />
                        </div>
                    </div>

                    <div class="col-12 col-md-6">
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

        <!-- ✅ Tabla -->
        <div class="hv-card mt-3">
            <div class="hv-card-header">
                <h2 class="hv-card-title">
                    <i class="bi bi-table"></i>
                    Facturas (vista rápida)
                </h2>

                <div class="d-flex flex-wrap gap-2">
                    <span class="hv-pill"><i class="bi bi-lightning-charge"></i>Relevante</span>
                    <span class="hv-pill"><i class="bi bi-check2-circle"></i>Estado</span>
                    <span class="hv-pill"><i class="bi bi-qr-code-scan"></i>FE</span>
                </div>
            </div>

            <div class="hv-card-body">
                <div class="hv-table-wrap">
                    <!-- ✅ Tabla visual (puedes cambiar por GridView con el mismo look) -->
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
                                <th class="text-end">Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            <!-- FILA DEMO -->
                            <tr>
                                <!-- CUENTA / ID -->
                                <td>
                                    <span class="hv-badge gray">
                                        <i class="bi bi-hash"></i>
                                        123
                                    </span>
                                </td>
                                <td>
                                    <div class="fw-bold">2026-02-27</div>
                                    <div class="hv-muted">08:30 AM</div>
                                </td>
                                <td>
                                    <div class="fw-bold">POS-000123</div>
                                    <div class="hv-muted">Alias: Mostrador</div>
                                </td>
                                <td>
                                    <div class="fw-bold">Cliente Contado</div>
                                    <div class="hv-muted">NIT: 0</div>
                                </td>
                                <td>
                                    <span class="hv-badge info"><i class="bi bi-cash-coin"></i>Efectivo</span>
                                </td>
                                <td class="text-end hv-money">$ 0</td>
                                <td class="text-end hv-money">$ 0</td>
                                <td class="text-end hv-money">$ 0</td>
                                <td>
                                    <span class="hv-badge success"><i class="bi bi-check2-circle"></i>Pagada</span>
                                </td>
                                <td>
                                    <span class="hv-badge gray"><i class="bi bi-dash-circle"></i>No aplica</span>
                                </td>
                                <td class="text-end">
                                    <button type="button" class="btn btn-outline-primary btn-sm hv-linkbtn"
                                            data-bs-toggle="modal" data-bs-target="#mdlVenta">
                                        <i class="bi bi-eye me-1"></i>Ver
                                    </button>
                                    <button type="button" class="btn btn-outline-secondary btn-sm hv-linkbtn">
                                        <i class="bi bi-printer me-1"></i>Imprimir
                                    </button>
                                </td>
                            </tr>

                            <!-- DUPLICA FILAS PARA MAQUETAR -->
                        </tbody>
                    </table>
                </div>
            </div>
        </div>

    </div>

    <!-- ✅ Modal Detalle (solo UI) -->
    <div class="modal fade" id="mdlVenta" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content hv-card">
                <div class="hv-card-header">
                    <h3 class="hv-card-title">
                        <i class="bi bi-receipt-cutoff"></i>
                        Detalle de la venta
                    </h3>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>

                <div class="hv-card-body">
                    <div class="row g-3">
                        <div class="col-md-4">
                            <div class="hv-muted mb-1">Factura</div>
                            <div class="fw-bold">POS-000123</div>
                        </div>
                        <div class="col-md-4">
                            <div class="hv-muted mb-1">Cliente</div>
                            <div class="fw-bold">Cliente Contado</div>
                        </div>
                        <div class="col-md-4">
                            <div class="hv-muted mb-1">Total</div>
                            <div class="fw-bold">$ 0</div>
                        </div>

                        <div class="col-12">
                            <div class="alert alert-info mb-0">
                                <i class="bi bi-info-circle me-2"></i>
                                Aquí tú cargas el detalle real: productos, impuestos, medios, QR, CUFE, etc.
                            </div>
                        </div>
                    </div>

                    <div class="d-flex justify-content-end gap-2 mt-3">
                        <button type="button" class="btn btn-outline-secondary rounded-pill px-4" data-bs-dismiss="modal">
                            <i class="bi bi-x-lg me-2"></i>Cerrar
                        </button>
                        <button type="button" class="btn btn-primary rounded-pill px-4">
                            <i class="bi bi-printer me-2"></i>Imprimir
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

</asp:Content>