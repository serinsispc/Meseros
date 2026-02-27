<%@ Page Title="Cerrar Caja" Language="C#" MasterPageFile="~/Menu.Master" AutoEventWireup="true"
    CodeBehind="CerrarCaja.aspx.cs" Inherits="WebApplication.CerrarCaja" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hidAccion" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hidArgumento" runat="server" ClientIDMode="Static" />

    <asp:Button ID="btnBridge" runat="server"
    ClientIDMode="Static"
    Style="display:none;"
    OnClick="btn_Click" />


    <style>
        /* =========================
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
                    <button type="button" class="btn btn-outline-primary shadow-sm" id="btnImprimir">
                        <i class="bi bi-printer me-2"></i>Imprimir
                    </button>
                    <button type="button" class="btn btn-danger shadow-sm" data-bs-toggle="modal" data-bs-target="#mdlConfirmarCierre" id="btnCerrarCaja">
                        <i class="bi bi-lock-fill me-2"></i>Cerrar Caja
                    </button>
                </div>
            </div>

            <!-- ✅ KPIs principales -->
            <div class="row g-3 mt-2">
                <div class="col-12 col-md-6 col-xl-3">
                    <div class="cc-kpi">
                        <div class="top">
                            <div class="label">Base Inicial</div>
                            <div class="ico"><i class="bi bi-safe2"></i></div>
                        </div>
                        <div class="value">
                            <span id="lblValorBase" runat="server">$ 0</span>
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
                            <span id="lblTotalIngresos" runat="server">$ 0</span>
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
                            <span id="lblTotalEgresos" runat="server">$ 0</span>
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
                            <span id="lblProducido" runat="server">$ 0</span>
                        </div>
                        <div class="text-muted small fw-semibold">producido</div>
                    </div>
                </div>
            </div>
        </div>

        <!-- ✅ Datos del turno + Arqueo -->
        <div class="row g-3 mt-3">

            <!-- Turno -->
            <div class="col-12 col-xl-5">
                <div class="cc-card h-100">
                    <div class="cc-card-h">
                        <h2 class="cc-card-t"><i class="bi bi-person-badge"></i>Datos del Turno</h2>
                        <span class="cc-pill">
                            <i class="bi bi-hash"></i>
                            Turno: <span id="lblIdTurno" runat="server">0</span>
                        </span>
                    </div>
                    <div class="cc-card-b">

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-person"></i>Cajero</div>
                            <div class="v"><span id="lblNombreUsuario" runat="server">—</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-clock"></i>Apertura</div>
                            <div class="v"><span id="lblFechaApertura" runat="server">—</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-clock-history"></i>Cierre</div>
                            <div class="v"><span id="lblFechaCierre" runat="server">—</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-shield-check"></i>Estado Base</div>
                            <div class="v">
                                <span class="cc-badge ok" id="lblEstadoBaseBadge" runat="server">
                                    <i class="bi bi-check2-circle"></i><span id="lblEstadoBase" runat="server">—</span>
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
                                    <div class="v"><span id="lblTotalEfectivo" runat="server">$ 0</span></div>
                                </div>
                                <div class="text-muted small fw-semibold mt-1">totalEfectivo</div>
                            </div>

                            <div class="col-12 col-md-4">
                                <div class="cc-kv mb-0">
                                    <div class="k"><i class="bi bi-plus-circle"></i>Efectivo + Base</div>
                                    <div class="v"><span id="lblEfectivoMasBase" runat="server">$ 0</span></div>
                                </div>
                                <div class="text-muted small fw-semibold mt-1">efectivoMasBase</div>
                            </div>

                            <div class="col-12 col-md-4">
                                <div class="cc-kv mb-0">
                                    <div class="k"><i class="bi bi-credit-card"></i>Ventas Tarjeta</div>
                                    <div class="v"><span id="lblVentasTarjeta" runat="server">$ 0</span></div>
                                </div>
                                <div class="text-muted small fw-semibold mt-1">ventasTargeta</div>
                            </div>
                        </div>

                        <div class="cc-sep"></div>

                        <!-- Efectivo físico (tú lo usas para comparar si quieres) -->
                        <div class="row g-3 align-items-end">
                            <div class="col-12 col-md-6">
                                <label class="small fw-bold text-muted mb-1 d-block">Efectivo físico contado (opcional)</label>
                                <input type="text" class="form-control cc-input" id="txtEfectivoFisico" placeholder="$ 0" />
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
                            <div class="v"><span id="lblVentasEfectivo" runat="server">$ 0</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-credit-card"></i>Ventas Tarjeta</div>
                            <div class="v"><span id="lblVentasTargeta2" runat="server">$ 0</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-calendar2-check"></i>Ventas Crédito</div>
                            <div class="v"><span id="lblVentasCredito" runat="server">$ 0</span></div>
                        </div>

                        <div class="cc-kv mb-0">
                            <div class="k"><i class="bi bi-cash-stack"></i>Total Ingresos</div>
                            <div class="v"><span id="lblTotalIngresos2" runat="server">$ 0</span></div>
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
                            <div class="v"><span id="lblGastosEfectivo" runat="server">$ 0</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-arrow-repeat"></i>Pago CxC (Efectivo)</div>
                            <div class="v"><span id="lblPagoCC_Efectivo" runat="server">$ 0</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-credit-card-2-front"></i>Pago CxC (Tarjeta)</div>
                            <div class="v"><span id="lblPagoCC_Targeta" runat="server">$ 0</span></div>
                        </div>

                        <div class="cc-kv">
                            <div class="k"><i class="bi bi-box-arrow-up"></i>Pago CxP (Efectivo)</div>
                            <div class="v"><span id="lblPagoCP_Efectivo" runat="server">$ 0</span></div>
                        </div>

                        <div class="cc-kv mb-0">
                            <div class="k"><i class="bi bi-cash"></i>Total Egresos</div>
                            <div class="v"><span id="lblTotalEgresos2" runat="server">$ 0</span></div>
                        </div>

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



    <script src="Scripts/js/cerrar-caja.js"></script>


</asp:Content>
