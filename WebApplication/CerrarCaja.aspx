<%@ Page Title="Cerrar Caja" Language="C#" MasterPageFile="~/Menu.Master" AutoEventWireup="true"
    CodeBehind="CerrarCaja.aspx.cs" Inherits="WebApplication.CerrarCaja" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hidAccion" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hidArgumento" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hidGastosTurnoJson" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hidProductosVendidosJson" runat="server" ClientIDMode="Static" />

        <div id="appLoading" class="app-loading">
        <div class="app-loading-card">
            <div class="app-spinner" aria-hidden="true"></div>
            <div class="app-loading-title">Procesando cierre...</div>
            <div class="app-loading-sub">Estamos guardando y preparando el informe final</div>
        </div>
    </div>
<asp:Button ID="btnBridge" runat="server"
    ClientIDMode="Static"
    Style="display:none;"
    OnClick="btn_Click" />


    <style>

        #topActionBar {
            justify-content: flex-start !important;
            gap: 12px;
        }

        #topActionBar > div:last-child,
        #topActionBar a:not([data-nav="caja"]) {
            display: none !important;
        }/* =========================
           CIERRE DE CAJA - UI PRO
           (1 registro - turno activo)
           ========================= */
        :root{
            --cc-bg:#f6f8fb;
            --cc-card:#fff;
            --cc-border: rgba(10,26,54,.10);
            --cc-text:#0b1b33;
            --cc-muted: rgba(11,27,51,.68);
            --cc-primary:#2563eb;
            --cc-success:#16a34a;
            --cc-warning:#f59e0b;
            --cc-danger:#ef4444;
            --cc-radius:18px;
            --cc-shadow: 0 14px 40px rgba(15,23,42,.10);
        }

        .cc-wrap{ padding:18px 0 30px; }
        .cc-hero{
            background: linear-gradient(135deg, rgba(22,163,74,.12), rgba(37,99,235,.10));
            border: 1px solid var(--cc-border);
            border-radius: var(--cc-radius);
            box-shadow: var(--cc-shadow);
            padding: 18px;
        }
        .cc-pill{
            display:inline-flex; align-items:center; gap:8px;
            padding:6px 12px; border-radius:999px;
            border:1px solid var(--cc-border);
            background: rgba(255,255,255,.70);
            color: var(--cc-muted);
            font-weight: 800; font-size:.85rem;
        }
        .cc-title{ font-size:1.35rem; font-weight:900; margin:0; color:var(--cc-text); }
        .cc-sub{ margin-top:4px; color:var(--cc-muted); }

        .cc-card{
            background: var(--cc-card);
            border: 1px solid var(--cc-border);
            border-radius: var(--cc-radius);
            box-shadow: var(--cc-shadow);
        }
        .cc-card-h{
            padding:14px 16px;
            border-bottom: 1px solid var(--cc-border);
            display:flex; align-items:center; justify-content:space-between; gap:10px;
        }
        .cc-card-t{
            margin:0; font-weight:900; color:var(--cc-text); font-size:1rem;
            display:flex; align-items:center; gap:10px;
        }
        .cc-card-b{ padding:16px; }

        .cc-kpi{
            background:#fff;
            border:1px solid rgba(2,6,23,.08);
            border-radius: 16px;
            box-shadow: 0 10px 26px rgba(2,6,23,.06);
            padding:14px;
            height:100%;
        }
        .cc-kpi .top{ display:flex; align-items:center; justify-content:space-between; gap:10px; }
        .cc-kpi .label{ color:var(--cc-muted); font-weight:900; font-size:.82rem; }
        .cc-kpi .value{ margin-top:6px; font-weight:900; color:var(--cc-text); font-size:1.25rem; font-variant-numeric: tabular-nums; }
        .cc-kpi .ico{
            width:42px; height:42px; display:flex; align-items:center; justify-content:center;
            border-radius: 14px;
            border:1px solid rgba(37,99,235,.18);
            background: rgba(37,99,235,.10);
            color: var(--cc-primary);
            font-size:1.1rem;
        }
        .cc-kpi.success .ico{ border-color: rgba(22,163,74,.18); background: rgba(22,163,74,.10); color: var(--cc-success);}
        .cc-kpi.warning .ico{ border-color: rgba(245,158,11,.22); background: rgba(245,158,11,.12); color: var(--cc-warning);}
        .cc-kpi.danger  .ico{ border-color: rgba(239,68,68,.18); background: rgba(239,68,68,.10); color: var(--cc-danger);}

        .cc-kv{
            display:flex; align-items:center; justify-content:space-between;
            padding: 10px 12px;
            border:1px solid rgba(2,6,23,.08);
            border-radius: 14px;
            background: rgba(246,248,251,.7);
            margin-bottom:10px;
        }
        .cc-kv .k{ color: var(--cc-muted); font-weight:900; font-size:.82rem; display:flex; gap:8px; align-items:center;}
        .cc-kv .v{ color: var(--cc-text); font-weight:900; font-variant-numeric: tabular-nums; }
        .cc-badge{
            display:inline-flex; align-items:center; gap:6px;
            padding:6px 10px; border-radius:999px; font-weight:900; font-size:.78rem;
            border:1px solid transparent;
        }
        .cc-badge.ok{ background: rgba(22,163,74,.10); color: var(--cc-success); border-color: rgba(22,163,74,.18); }
        .cc-badge.warn{ background: rgba(245,158,11,.12); color: #a16207; border-color: rgba(245,158,11,.22); }
        .cc-badge.bad{ background: rgba(239,68,68,.10); color: var(--cc-danger); border-color: rgba(239,68,68,.18); }
        .cc-actions .btn{ border-radius:999px; padding:10px 14px; font-weight:900; }
        .cc-note{
            border-radius: 16px;
            border: 1px dashed rgba(2,6,23,.18);
            background: rgba(255,255,255,.7);
            padding: 12px;
            color: var(--cc-muted);
            font-weight: 700;
        }
        .cc-input{
            border-radius: 14px;
            border: 1px solid rgba(2,6,23,.12);
            padding: 10px 12px;
        }
        .cc-sep{ height:1px; background: rgba(2,6,23,.08); margin: 10px 0; }
        .cc-report-wrap{ width:100%; max-width:none; }
        .cc-report-table{ width:100%; border-collapse:collapse; font-size:.95rem; }
        .cc-report-table th{ background:#cfe0f2; color:#0b1b33; font-weight:900; padding:8px 10px; border:1px solid #aebfd1; }
        .cc-report-table td{ background:#fff; padding:7px 10px; border:1px solid #cfd7e3; }
        .cc-report-table td.money{ font-weight:900; white-space:nowrap; }
        .cc-report-total{ display:flex; justify-content:flex-end; align-items:center; gap:10px; margin-top:10px; font-weight:900; color:var(--cc-text); }
        .cc-report-wrap{ width:100%; max-width:none; }
        .cc-report-table{ width:100%; border-collapse:separate; border-spacing:0; font-size:.95rem; }
        .cc-report-table th{ background:#d7e9fb; color:#0b1b33; font-weight:900; padding:10px 12px; border:1px solid #b7cfe7; }
        .cc-report-table td{ background:#fff; padding:8px 12px; border-left:1px solid #d5dbe5; border-right:1px solid #d5dbe5; border-bottom:1px solid #d5dbe5; }
        .cc-report-table tbody tr:first-child td{ border-top:1px solid #d5dbe5; }
        .cc-report-table td.money{ font-weight:900; white-space:nowrap; }
                .cc-report-total{ display:flex; justify-content:flex-end; align-items:center; gap:12px; margin-top:10px; font-weight:900; color:var(--cc-text); }
        .app-loading {
            position: fixed;
            inset: 0;
            z-index: 999999;
            display: none;
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
            border: 5px solid rgba(37,99,235,.18);
            border-top-color: #2563eb;
            border-right-color: #0b3a7e;
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
            to { transform: rotate(360deg); }
        }
    </style>

    <div class="container-fluid cc-wrap">

        <!-- ✅ Encabezado -->
        <div class="cc-hero">
            <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
                <div>
                    <span class="cc-pill">
                        <i class="bi bi-journal-check"></i>
                        Cierre de Caja / Turno
                    </span>
                    <h1 class="cc-title mt-2">Resumen de Caja (Turno Activo)</h1>
                    <div class="cc-sub">Revisa el arqueo, confirma totales y realiza el cierre contable.</div>
                </div>

                <div class="d-flex flex-wrap gap-2 cc-actions">
                    <a href="caja.aspx" class="btn btn-success shadow-sm">
                        <i class="bi bi-cash-coin me-2"></i>Caja
                    </a>

                            <%if (MostrarCierreCajaHabilitado)
                                {%>
                                        <button type="button" class="btn btn-outline-warning shadow-sm" id="btnAperturarCajon">
                        <i class="bi bi-safe me-2"></i>Aperturar Cajón
                    </button>
                    <button type="button" class="btn btn-outline-secondary shadow-sm" data-bs-toggle="modal" data-bs-target="#mdlEditarBase" id="btnEditarBase">
                        <i class="bi bi-pencil-square me-2"></i>Modificar Base
                    </button>
                    <button type="button" class="btn btn-outline-primary shadow-sm" id="btnImprimir">
                        <i class="bi bi-printer me-2"></i>Imprimir
                    </button>
                    
                    <%  }%>





                    <button type="button" class="btn btn-danger shadow-sm" data-bs-toggle="modal" data-bs-target="#mdlConfirmarCierre" id="btnCerrarCaja">
                        <i class="bi bi-lock-fill me-2"></i>Cerrar Caja
                    </button>
                </div>
            </div>


            <% if(MostrarCierreCajaHabilitado)
            {%>

                        <!-- ✅ KPIs principales -->
            <div class="row g-3 mt-2">
                <div class="col-12 col-md-6 col-xl-3">
                    <div class="cc-kpi">
                        <div class="top">
                            <div class="label">Base Inicial</div>
                            <div class="ico"><i class="bi bi-safe2"></i></div>
                        </div>
                        <div class="value">
                            <span id="lblValorBase" runat="server" ClientIDMode="Static">$ 0</span>
                        </div>
                        <div class="text-muted small fw-semibold">valorBase</div>
                    </div>
                </div>

                <div class="col-12 col-md-6 col-xl-3">
                    <div class="cc-kpi success">
                        <div class="top">
                            <div class="label">Total Ingresos</div>
                            <div class="ico"><i class="bi bi-arrow-down-circle"></i></div>
                        </div>
                        <div class="value">
                            <span id="lblTotalIngresos" runat="server" ClientIDMode="Static">$ 0</span>
                        </div>
                        <div class="text-muted small fw-semibold">totalIngresos</div>
                    </div>
                </div>

                <div class="col-12 col-md-6 col-xl-3">
                    <div class="cc-kpi danger">
                        <div class="top">
                            <div class="label">Total Egresos</div>
                            <div class="ico"><i class="bi bi-arrow-up-circle"></i></div>
                        </div>
                        <div class="value">
                            <span id="lblTotalEgresos" runat="server" ClientIDMode="Static">$ 0</span>
                        </div>
                        <div class="text-muted small fw-semibold">totalEgresos</div>
                    </div>
                </div>

                <div class="col-12 col-md-6 col-xl-3">
                    <div class="cc-kpi warning">
                        <div class="top">
                            <div class="label">Producido</div>
                            <div class="ico"><i class="bi bi-graph-up-arrow"></i></div>
                        </div>
                        <div class="value">
                            <span id="lblProducido" runat="server" ClientIDMode="Static">$ 0</span>
                        </div>
                        <div class="text-muted small fw-semibold">producido</div>
                    </div>
                </div>
            </div>

<%            } %>




        </div>

        <%if (MostrarCierreCajaHabilitado)
            {%>


                <!-- ✅ Datos del turno + Arqueo -->
        <div class="row g-3 mt-3">

            <!-- Turno -->
            <div class="col-12 col-xl-5">
                <div class="cc-card h-100">
                    <div class="cc-card-h">
                        <h2 class="cc-card-t"><i class="bi bi-person-badge"></i>Datos del Turno</h2>
                        <span class="cc-pill">
                            <i class="bi bi-hash"></i>
                            Turno: <span id="lblIdTurno" runat="server" ClientIDMode="Static">0</span>
                        </span>
                    </div>
                    <div class="cc-card-b">

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-person"></i>Cajero</div>
                            <div class="v"><span id="lblNombreUsuario" runat="server" ClientIDMode="Static">—</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-clock"></i>Apertura</div>
                            <div class="v"><span id="lblFechaApertura" runat="server" ClientIDMode="Static">—</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-clock-history"></i>Cierre</div>
                            <div class="v"><span id="lblFechaCierre" runat="server" ClientIDMode="Static">—</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-shield-check"></i>Estado Base</div>
                            <div class="v">
                                <span class="cc-badge ok" id="lblEstadoBaseBadge" runat="server" ClientIDMode="Static">
                                    <i class="bi bi-check2-circle"></i><span id="lblEstadoBase" runat="server" ClientIDMode="Static">—</span>
                                </span>
                            </div>
                        </div>

                        <div class="cc-note mt-2">
                            <i class="bi bi-info-circle me-2"></i>
                            Este informe corresponde al turno activo. Verifica el arqueo antes de confirmar el cierre.
                        </div>

                    </div>
                </div>
            </div>

            <!-- Arqueo / Efectivo -->
            <div class="col-12 col-xl-7">
                <div class="cc-card h-100">
                    <div class="cc-card-h">
                        <h2 class="cc-card-t"><i class="bi bi-cash-stack"></i>Arqueo de Efectivo</h2>
                        <span class="cc-pill"><i class="bi bi-calculator"></i>Validación</span>
                    </div>

                    <div class="cc-card-b">
                        <div class="row g-3">
                            <div class="col-12 col-md-4">
                                <div class="cc-kv mb-0">
                                    <div class="k"><i class="bi bi-cash-coin"></i>Total Efectivo</div>
                                    <div class="v"><span id="lblTotalEfectivo" runat="server" ClientIDMode="Static">$ 0</span></div>
                                </div>
                                <div class="text-muted small fw-semibold mt-1">totalEfectivo</div>
                            </div>

                            <div class="col-12 col-md-4">
                                <div class="cc-kv mb-0">
                                    <div class="k"><i class="bi bi-plus-circle"></i>Efectivo + Base</div>
                                    <div class="v"><span id="lblEfectivoMasBase" runat="server" ClientIDMode="Static">$ 0</span></div>
                                </div>
                                <div class="text-muted small fw-semibold mt-1">efectivoMasBase</div>
                            </div>

                            <div class="col-12 col-md-4">
                                <div class="cc-kv mb-0">
                                    <div class="k"><i class="bi bi-credit-card"></i>Ventas Tarjeta</div>
                                    <div class="v"><span id="lblVentasTarjeta" runat="server" ClientIDMode="Static">$ 0</span></div>
                                </div>
                                <div class="text-muted small fw-semibold mt-1">ventasTargeta</div>
                            </div>
                        </div>

                        <div class="cc-sep"></div>

                        <!-- Efectivo físico (tú lo usas para comparar si quieres) -->
                        <div class="row g-3 align-items-end">
                            <div class="col-12 col-md-6">
                                <label class="small fw-bold text-muted mb-1 d-block">Efectivo físico contado (opcional)</label>
                                <div class="input-group">
                                    <input type="text" class="form-control cc-input" id="txtEfectivoFisico" placeholder="$ 0" oninput="ccActualizarDiferencia()" onkeyup="ccActualizarDiferencia()" onchange="ccActualizarDiferencia()" />
                                </div>
                                <div class="text-muted small fw-semibold mt-1">
                                    (Esto es para validar contra “Efectivo + Base”)
                                </div>
                            </div>

                            <div class="col-12 col-md-6">
                                <div class="cc-kv mb-0">
                                    <div class="k"><i class="bi bi-exclamation-triangle"></i>Diferencia</div>
                                    <div class="v">
                                        <span class="cc-badge warn" id="lblDiferenciaBadge">
                                            <i class="bi bi-dash-circle"></i><span id="lblDiferencia">$ 0</span>
                                        </span>
                                    </div>
                                </div>
                                <div class="text-muted small fw-semibold mt-1">
                                    (Solo visual; tú decides el cálculo)
                                </div>
                            </div>
                        </div>

                        <div class="cc-note mt-3">
                            <i class="bi bi-shield-check me-2"></i>
                            Recomendación contable: si hay diferencia, registra una observación antes del cierre.
                        </div>

                    </div>
                </div>
            </div>

        </div>

        <!-- ✅ Detalles contables (Ingresos/Egresos por concepto) -->
        <div class="row g-3 mt-3">

            <!-- Ingresos -->
            <div class="col-12 col-lg-6">
                <div class="cc-card h-100">
                    <div class="cc-card-h">
                        <h2 class="cc-card-t"><i class="bi bi-arrow-down-circle"></i>Ingresos del Turno</h2>
                        <span class="cc-pill"><i class="bi bi-receipt"></i>Ventas</span>
                    </div>
                    <div class="cc-card-b">

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-cash-coin"></i>Ventas Efectivo</div>
                            <div class="v"><span id="lblVentasEfectivo" runat="server" ClientIDMode="Static">$ 0</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-credit-card"></i>Ventas Tarjeta</div>
                            <div class="v"><span id="lblVentasTargeta2" runat="server" ClientIDMode="Static">$ 0</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-calendar2-check"></i>Ventas Crédito</div>
                            <div class="v"><span id="lblVentasCredito" runat="server" ClientIDMode="Static">$ 0</span></div>
                        </div>

                        <div class="cc-kv mb-0">
                            <div class="k"><i class="bi bi-cash-stack"></i>Total Ingresos</div>
                            <div class="v"><span id="lblTotalIngresos2" runat="server" ClientIDMode="Static">$ 0</span></div>
                        </div>

                    </div>
                </div>
            </div>

            <!-- Egresos -->
            <div class="col-12 col-lg-6">
                <div class="cc-card h-100">
                    <div class="cc-card-h">
                        <h2 class="cc-card-t"><i class="bi bi-arrow-up-circle"></i>Egresos del Turno</h2>
                        <span class="cc-pill"><i class="bi bi-wallet2"></i>Salidas</span>
                    </div>
                    <div class="cc-card-b">

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-bag-dash"></i>Gastos (Efectivo)</div>
                            <div class="v"><span id="lblGastosEfectivo" runat="server" ClientIDMode="Static">$ 0</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-arrow-repeat"></i>Pago CxC (Efectivo)</div>
                            <div class="v"><span id="lblPagoCC_Efectivo" runat="server" ClientIDMode="Static">$ 0</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-credit-card-2-front"></i>Pago CxC (Tarjeta)</div>
                            <div class="v"><span id="lblPagoCC_Targeta" runat="server" ClientIDMode="Static">$ 0</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-box-arrow-up"></i>Pago CxP (Efectivo)</div>
                            <div class="v"><span id="lblPagoCP_Efectivo" runat="server" ClientIDMode="Static">$ 0</span></div>
                        </div>

                        <div class="cc-kv mb-0">
                            <div class="k"><i class="bi bi-cash"></i>Total Egresos</div>
                            <div class="v"><span id="lblTotalEgresos2" runat="server" ClientIDMode="Static">$ 0</span></div>
                        </div>

                    </div>
                </div>
            </div>

        </div>



                <div class="row g-3 mt-3">
            <div class="col-12">
                <div class="cc-card">
                    <div class="cc-card-h">
                        <h2 class="cc-card-t"><i class="bi bi-table"></i>Reporte pagos internos por turno</h2>
                        <span class="cc-pill"><i class="bi bi-diagram-3"></i>SP InformePagoInterno_Turno</span>
                    </div>
                    <div class="cc-card-b">
                        <div id="pnlPagosInternos" runat="server" ClientIDMode="Static" class="cc-report-wrap">
                            <table class="cc-report-table">
                                <thead>
                                    <tr>
                                        <th>Medio de pago</th>
                                        <th>Total Venta</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptPagosInternos" runat="server">
                                        <ItemTemplate>
                                            <tr>
                                                <td><%# Eval("nombreMPI") %></td>
                                                <td class="money"><%# string.Format("$ {0:N0}", Convert.ToDecimal(Eval("total"))) %></td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                            <div class="cc-report-total">
                                <span>Total:</span>
                                <span id="lblTotalPagosInternos" runat="server" ClientIDMode="Static">$ 0</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>


            <%} %>




    </div>
    <div class="modal fade" id="mdlEditarBase" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content cc-card">
                <div class="cc-card-h">
                    <h3 class="cc-card-t"><i class="bi bi-safe2"></i>Modificar base activa</h3>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="cc-card-b">
                    <div class="alert alert-info mb-3">
                        <i class="bi bi-info-circle me-2"></i>
                        Actualiza el valor de la base activa y se guardará en la base de datos del turno actual.
                    </div>

                    <label class="small fw-bold text-muted mb-1 d-block">Nuevo valor base</label>
                    <input type="text" class="form-control cc-input" id="txtValorBaseEditar" inputmode="numeric" placeholder="$ 0" />

                    <div class="text-muted small fw-semibold mt-2">
                        Ingresa solo números. El sistema recalculará "Efectivo + Base" con el nuevo valor.
                    </div>

                    <div class="d-flex justify-content-end gap-2 mt-3 cc-actions">
                        <button type="button" class="btn btn-outline-secondary shadow-sm" data-bs-dismiss="modal">
                            <i class="bi bi-x-lg me-2"></i>Cancelar
                        </button>
                        <button type="button" class="btn btn-primary shadow-sm" id="btnGuardarBase">
                            <i class="bi bi-save me-2"></i>Guardar Base
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>


    <!-- ✅ Modal confirmar cierre -->
    <div class="modal fade" id="mdlConfirmarCierre" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content cc-card">
                <div class="cc-card-h">
                    <h3 class="cc-card-t"><i class="bi bi-lock-fill"></i>Confirmar cierre</h3>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="cc-card-b">
                    <div class="alert alert-warning mb-3">
                        <i class="bi bi-exclamation-triangle me-2"></i>
                        Vas a cerrar el turno activo. Esta acción debe realizarse solo después de verificar el arqueo.
                    </div>

                    <label class="small fw-bold text-muted mb-1 d-block">Observación de cierre (opcional)</label>
                    <textarea class="form-control cc-input" id="txtObsCierre" rows="3"
                              placeholder="Ej: Arqueo OK, sin diferencias."></textarea>

                    <div class="d-flex justify-content-end gap-2 mt-3 cc-actions">
                        <button type="button" class="btn btn-outline-secondary shadow-sm" data-bs-dismiss="modal">
                            <i class="bi bi-x-lg me-2"></i>Cancelar
                        </button>


                        <!-- Este botón tú lo conectas a tu lógica -->
                        <button type="button" class="btn btn-danger shadow-sm" id="btnConfirmarCierre">
                            <i class="bi bi-check2-circle me-2"></i>Confirmar Cierre
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script>
        window.ccMostrarCierreCaja = <%= MostrarCierreCajaHabilitado ? "true" : "false" %>;
    </script>
    <script src="Scripts/js/cerrar-caja.js?v=20260408-1"></script>


<script>
(function () {
    function ccGetTextInline(id, fallback) {
        var el = document.getElementById(id);
        if (!el) return fallback || '';
        var text = (el.textContent || el.innerText || '').trim();
        return text || (fallback || '');
    }

    function ccEscapeHtmlInline(value) {
        return (value || '')
            .toString()
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function ccTicketLineInline(label, value) {
        return '<div class="row"><span>' + ccEscapeHtmlInline(label) + '</span><strong>' + ccEscapeHtmlInline(value) + '</strong></div>';
    }

    function ccPagosInternosInline() {
        var table = document.querySelector('.cc-report-table tbody');
        if (!table) return '<div class="empty">Sin pagos internos registrados.</div>';
        var rows = Array.prototype.slice.call(table.querySelectorAll('tr'));
        if (!rows.length) return '<div class="empty">Sin pagos internos registrados.</div>';
        return rows.map(function (row) {
            var cells = row.querySelectorAll('td');
            if (cells.length < 2) return '';
            return '<div class="row"><span>' + ccEscapeHtmlInline(cells[0].textContent.trim()) + '</span><strong>' + ccEscapeHtmlInline(cells[1].textContent.trim()) + '</strong></div>';
        }).join('');
    }

    function ccGetProductosVendidosInline() {
        var field = document.getElementById('hidProductosVendidosJson');
        if (!field || !field.value) return [];
        try {
            var data = JSON.parse(field.value);
            return Array.isArray(data) ? data : [];
        } catch (e) {
            return [];
        }
    }

    function ccGetGastosTurnoInline() {
        var field = document.getElementById('hidGastosTurnoJson');
        if (!field || !field.value) return [];
        try {
            var data = JSON.parse(field.value);
            return Array.isArray(data) ? data : [];
        } catch (e) {
            return [];
        }
    }

    function ccFormatCantidadInline(value) {
        var number = Number(value || 0);
        if (!isFinite(number)) return '0';
        if (Math.floor(number) === number) return number.toString();
        return number.toLocaleString('es-CO', { minimumFractionDigits: 0, maximumFractionDigits: 2 });
    }

    function ccFormatMoneyInline(value) {
        var number = Number(value || 0);
        if (!isFinite(number)) number = 0;
        return number.toLocaleString('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0, maximumFractionDigits: 0 });
    }

    function ccProductosVendidosInline() {
        var productos = ccGetProductosVendidosInline();
        if (!productos.length) return '';

        var rows = productos.map(function (item) {
            return '' +
                '<div class="row"><span>' + ccEscapeHtmlInline(item.nombreProducto || '-') + '</span><strong>' + ccEscapeHtmlInline(ccFormatCantidadInline(item.cantidad)) + '</strong></div>' +
                '<div class="row"><span>Valor</span><strong>' + ccEscapeHtmlInline(ccFormatMoneyInline(item.valor)) + '</strong></div>';
        }).join('<div style="border-top:1px dotted #000;margin:4px 0;"></div>');

        return '<div class="sep"></div><div class="section"><h4>Productos vendidos</h4>' + rows + '</div>';
    }

    function ccGastosTurnoInline() {
        var gastos = ccGetGastosTurnoInline();
        if (!gastos.length) return '';

        var rows = gastos.map(function (item) {
            var concepto = item.concepto || '-';
            var tipo = item.nombreTipoGasto || '';
            var etiqueta = tipo ? (concepto + ' (' + tipo + ')') : concepto;
            return '<div class="row"><span>' + ccEscapeHtmlInline(etiqueta) + '</span><strong>' + ccEscapeHtmlInline(ccFormatMoneyInline(item.valor)) + '</strong></div>';
        }).join('');

        return '<div class="sep"></div><div class="section"><h4>Gastos del turno</h4>' + rows + '<div class="row tot"><span>Total gastos</span><strong>' + ccEscapeHtmlInline(ccGetTextInline('lblGastosEfectivo', '$ 0')) + '</strong></div></div>';
    }

    window.ccImprimirTicketInline = function () {
        var efectivoFisicoEl = document.getElementById('txtEfectivoFisico');
        var efectivoFisico = efectivoFisicoEl && efectivoFisicoEl.value ? efectivoFisicoEl.value : '$ 0';
        var observacionEl = document.getElementById('txtObsCierre');
        var observacion = observacionEl && observacionEl.value ? observacionEl.value.trim() : '';
        var html = '' +
            '<!doctype html><html><head><meta charset="utf-8"><title>Ticket cierre caja</title><style>' +
            '@page{size:auto;margin:0;}html,body{margin:0;padding:0;background:#fff;font-family:Consolas,"Courier New",monospace;color:#000;}body{width:auto;max-width:none;padding:3mm;box-sizing:border-box;font-size:10px;line-height:1.3;}' +
            '.center{text-align:center;}.title{font-size:14px;font-weight:700;margin-bottom:2px;}.sub{font-size:10px;margin-bottom:7px;}.sep{border-top:1px dashed #000;margin:6px 0;}.section{margin-top:5px;}.section h4{margin:0 0 4px;font-size:10px;text-transform:uppercase;}.row{display:flex;justify-content:space-between;gap:6px;align-items:flex-start;margin:2px 0;}.row span:first-child{max-width:65%;word-break:break-word;}.row strong{font-weight:700;text-align:right;white-space:nowrap;}.tot{font-size:11px;font-weight:700;}.note{white-space:pre-wrap;word-break:break-word;}.empty{font-style:italic;}' +
            '</style></head><body>' +
            '<div class="center"><div class="title">CIERRE DE CAJA</div><div class="sub">' + ccEscapeHtmlInline(document.title || 'Mi empresa') + '</div></div>' +
            '<div class="sep"></div><div class="section"><h4>Datos del turno</h4>' +
            ccTicketLineInline('Turno', ccGetTextInline('lblIdTurno', '0')) +
            ccTicketLineInline('Cajero', ccGetTextInline('lblNombreUsuario', '-')) +
            ccTicketLineInline('Apertura', ccGetTextInline('lblFechaApertura', '-')) +
            ccTicketLineInline('Cierre', ccGetTextInline('lblFechaCierre', '-')) +
            ccTicketLineInline('Estado', ccGetTextInline('lblEstadoBase', '-')) +
            '</div><div class="sep"></div><div class="section"><h4>Resumen</h4>' +
            ccTicketLineInline('Base inicial', ccGetTextInline('lblValorBase', '$ 0')) +
            ccTicketLineInline('Total ingresos', ccGetTextInline('lblTotalIngresos', '$ 0')) +
            ccTicketLineInline('Total egresos', ccGetTextInline('lblTotalEgresos', '$ 0')) +
            ccTicketLineInline('Producido', ccGetTextInline('lblProducido', '$ 0')) +
            '</div><div class="sep"></div><div class="section"><h4>Arqueo</h4>' +
            ccTicketLineInline('Total efectivo', ccGetTextInline('lblTotalEfectivo', '$ 0')) +
            ccTicketLineInline('Efectivo + base', ccGetTextInline('lblEfectivoMasBase', '$ 0')) +
            ccTicketLineInline('Ventas tarjeta', ccGetTextInline('lblVentasTarjeta', '$ 0')) +
            ccTicketLineInline('Efectivo fisico', efectivoFisico) +
            ccTicketLineInline('Diferencia', ccGetTextInline('lblDiferencia', '$ 0')) +
            '</div><div class="sep"></div><div class="section"><h4>Ingresos</h4>' +
            ccTicketLineInline('Ventas efectivo', ccGetTextInline('lblVentasEfectivo', '$ 0')) +
            ccTicketLineInline('Ventas tarjeta', ccGetTextInline('lblVentasTargeta2', '$ 0')) +
            ccTicketLineInline('Ventas credito', ccGetTextInline('lblVentasCredito', '$ 0')) +
            '<div class="row tot"><span>Total ingresos</span><strong>' + ccEscapeHtmlInline(ccGetTextInline('lblTotalIngresos2', '$ 0')) + '</strong></div>' +
            '</div><div class="sep"></div><div class="section"><h4>Egresos</h4>' +
            ccTicketLineInline('Gastos efectivo', ccGetTextInline('lblGastosEfectivo', '$ 0')) +
            ccTicketLineInline('Pago CxC efectivo', ccGetTextInline('lblPagoCC_Efectivo', '$ 0')) +
            ccTicketLineInline('Pago CxC tarjeta', ccGetTextInline('lblPagoCC_Targeta', '$ 0')) +
            ccTicketLineInline('Pago CxP efectivo', ccGetTextInline('lblPagoCP_Efectivo', '$ 0')) +
            '<div class="row tot"><span>Total egresos</span><strong>' + ccEscapeHtmlInline(ccGetTextInline('lblTotalEgresos2', '$ 0')) + '</strong></div>' +
            '</div><div class="sep"></div><div class="section"><h4>Pagos internos</h4>' + ccPagosInternosInline() + '<div class="row tot"><span>Total pagos internos</span><strong>' + ccEscapeHtmlInline(ccGetTextInline('lblTotalPagosInternos', '$ 0')) + '</strong></div>' +
            '</div>' + ccGastosTurnoInline() + ccProductosVendidosInline() +
            '<div class="sep"></div><div class="section"><h4>Observacion</h4><div class="note">' + ccEscapeHtmlInline(observacion || 'Sin observacion.') + '</div></div>' +
            '<div class="sep"></div><div class="center">Impreso: ' + ccEscapeHtmlInline(new Date().toLocaleString('es-CO')) + '</div></body></html>';
        var win = window.open('', 'cierreCajaTicket', 'width=380,height=760');
        if (!win) return;
        win.document.open();
        win.document.write(html);
        win.document.close();
        win.focus();
        setTimeout(function () { win.print(); }, 250);
    };

    var bindPrint = function () {
        var btnImprimir = document.getElementById('btnImprimir');
        if (btnImprimir && !btnImprimir.dataset.ticketBound) {
            btnImprimir.addEventListener('click', window.ccImprimirTicketInline);
            btnImprimir.dataset.ticketBound = '1';
        }
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', bindPrint);
    } else {
        bindPrint();
    }
})();
</script>

</asp:Content>





















