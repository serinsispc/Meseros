<%@ Page Title="Gastos" Language="C#" MasterPageFile="~/Menu.Master" AutoEventWireup="true"
    CodeBehind="PGastos.aspx.cs" Inherits="WebApplication.PGastos" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- =========================
         MÓDULO: GASTOS (VISUAL)
         Requiere Bootstrap + Bootstrap Icons (ya los usas)
         ========================= -->

    <div class="container-fluid py-3">

        <!-- Encabezado -->
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
            <div>
                <h3 class="mb-0 fw-bold">Gastos</h3>
                <div class="text-muted small">Administra, filtra y controla los egresos registrados.</div>
            </div>

            <div class="d-flex gap-2">
                <button type="button" class="btn btn-primary rounded-pill shadow-sm px-4"
                    id="btnNuevoGasto" onclick="AbrirModalGasto('NUEVO')">
                    <i class="bi bi-plus-circle me-2"></i>Nuevo gasto
                </button>
            </div>
        </div>

        <!-- Filtros -->
        <div class="card shadow-sm border-0 mb-3">
            <div class="card-body">
                <div class="row g-2 align-items-end">

                    <div class="col-12 col-md-5">
                        <label class="form-label small text-muted mb-1">Buscar (concepto / bolsillo / tipo)</label>
                        <div class="input-group">
                            <span class="input-group-text bg-white"><i class="bi bi-search"></i></span>
                            <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control"
                                placeholder="Ej: arriendo, gasolina, caja menor..." />
                        </div>
                    </div>

                    <div class="col-6 col-md-2">
                        <label class="form-label small text-muted mb-1">Desde</label>
                        <asp:TextBox ID="txtDesde" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>

                    <div class="col-6 col-md-2">
                        <label class="form-label small text-muted mb-1">Hasta</label>
                        <asp:TextBox ID="txtHasta" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>

                    <div class="col-12 col-md-3 d-flex gap-2 justify-content-md-end">
                        <asp:Button ID="btnFiltrar" runat="server" CssClass="btn btn-outline-primary rounded-pill px-4"
                            Text="Filtrar" />
                        <asp:Button ID="btnLimpiar" runat="server" CssClass="btn btn-outline-secondary rounded-pill px-4"
                            Text="Limpiar" />
                    </div>

                </div>
            </div>
        </div>

        <!-- Tabla/Listado -->
        <div class="card shadow-sm border-0">
            <div class="card-body">

                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-2">
                    <div class="text-muted small">
                        Listado de gastos registrados
                        <span class="ms-2 badge rounded-pill text-bg-light border" id="lblTotalGastos">0</span>
                    </div>

                    <div class="text-muted small">
                        Tip: Usa los filtros para encontrar rápido un gasto.
                    </div>
                </div>

                <div class="table-responsive">
                    <table class="table table-hover align-middle mb-0" id="tblGastos">
                        <thead class="table-light">
                            <tr>
                                <th style="width: 140px;">Fecha</th>
                                <th>Concepto</th>
                                <th style="width: 170px;" class="text-end">Valor</th>
                                <th style="width: 220px;">Bolsillo</th>
                                <th style="width: 220px;">Tipo gasto</th>
                                <th style="width: 150px;" class="text-end">Acciones</th>
                            </tr>
                        </thead>
                        <tbody id="tbodyGastos">
                            <!--
                                Aquí renderizas filas desde code-behind (Repeater/GridView) o por JS/AJAX.
                                Esta fila es ejemplo visual.
                            -->
                            <tr>
                                <td><span class="badge text-bg-light border">2026-02-27</span></td>
                                <td>
                                    <div class="fw-semibold">Compra insumos</div>
                                    <div class="text-muted small">Registro de ejemplo</div>
                                </td>
                                <td class="text-end fw-bold">$ 35.000</td>
                                <td>CAJA MENOR</td>
                                <td>Operativo</td>
                                <td class="text-end">
                                    <div class="btn-group">
                                        <button type="button" class="btn btn-sm btn-outline-primary"
                                            onclick="AbrirModalGasto('EDITAR', 1)">
                                            <i class="bi bi-pencil-square"></i>
                                        </button>
                                        <button type="button" class="btn btn-sm btn-outline-danger"
                                            onclick="ConfirmarEliminarGasto(1)">
                                            <i class="bi bi-trash"></i>
                                        </button>
                                    </div>
                                </td>
                            </tr>

                            <!-- Si no hay datos, puedes mostrar esto desde backend -->
                            <!--
                            <tr>
                                <td colspan="6" class="text-center text-muted py-5">
                                    <i class="bi bi-inbox fs-2 d-block mb-2"></i>
                                    No hay gastos registrados.
                                </td>
                            </tr>
                            -->
                        </tbody>
                    </table>
                </div>

            </div>
        </div>

    </div>


    <!-- =========================
         MODAL: CREAR / EDITAR
         ========================= -->
    <div class="modal fade" id="mdlGasto" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold" id="mdlGastoTitle">Nuevo gasto</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>

                <div class="modal-body">
                    <!-- Hidden para Id -->
                    <asp:HiddenField ID="hfIdGasto" runat="server" />

                    <div class="row g-3">
                        <div class="col-12 col-md-4">
                            <label class="form-label small text-muted mb-1">Fecha</label>
                            <asp:TextBox ID="txtFechaGasto" runat="server" CssClass="form-control" TextMode="Date" />
                        </div>

                        <div class="col-12 col-md-8">
                            <label class="form-label small text-muted mb-1">Concepto</label>
                            <asp:TextBox ID="txtConcepto" runat="server" CssClass="form-control"
                                placeholder="Ej: arriendo, papelería, gasolina, mantenimiento..." />
                        </div>

                        <div class="col-12 col-md-4">
                            <label class="form-label small text-muted mb-1">Valor</label>
                            <asp:TextBox ID="txtValor" runat="server" CssClass="form-control"
                                placeholder="0" />
                            <div class="form-text">Puedes formatearlo en el backend o con JS.</div>
                        </div>

                        <div class="col-12 col-md-4">
                            <label class="form-label small text-muted mb-1">Bolsillo</label>
                            <!-- Aquí puedes llenar con tus bolsillos -->
                            <asp:DropDownList ID="ddlBolsillo" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Selecciona..." Value="" />
                                <asp:ListItem Text="CAJA MENOR" Value="1" />
                                <asp:ListItem Text="BANCO" Value="2" />
                            </asp:DropDownList>
                        </div>

                        <div class="col-12 col-md-4">
                            <label class="form-label small text-muted mb-1">Tipo de gasto</label>
                            <!-- Aquí llenas con tu catálogo de tipos -->
                            <asp:DropDownList ID="ddlTipoGasto" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Selecciona..." Value="" />
                                <asp:ListItem Text="Operativo" Value="1" />
                                <asp:ListItem Text="Administrativo" Value="2" />
                            </asp:DropDownList>
                        </div>
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary rounded-pill px-4" data-bs-dismiss="modal">
                        Cancelar
                    </button>

                    <!-- Botón visual (en backend le pones OnClick / CommandArgument) -->
                    <asp:Button ID="btnGuardarGasto" runat="server" CssClass="btn btn-primary rounded-pill px-4"
                        Text="Guardar" />
                </div>
            </div>
        </div>
    </div>


    <!-- =========================
         MODAL: CONFIRMAR ELIMINACIÓN
         ========================= -->
    <div class="modal fade" id="mdlEliminarGasto" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold text-danger">
                        <i class="bi bi-exclamation-triangle me-2"></i>Eliminar gasto
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>

                <div class="modal-body">
                    <asp:HiddenField ID="hfIdEliminar" runat="server" />
                    <div class="mb-1">¿Seguro que deseas eliminar este gasto?</div>
                    <div class="text-muted small">Esta acción no se puede deshacer.</div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary rounded-pill px-4" data-bs-dismiss="modal">
                        Cancelar
                    </button>

                    <!-- Botón visual (en backend lo conectas al DELETE) -->
                    <asp:Button ID="btnEliminarGasto" runat="server" CssClass="btn btn-danger rounded-pill px-4"
                        Text="Eliminar" />
                </div>
            </div>
        </div>
    </div>


    <!-- =========================
         JS mínimo SOLO para UI
         (abre modales y setea títulos/hidden ids)
         ========================= -->
    <script>
        function AbrirModalGasto(modo, id) {
            const modalEl = document.getElementById('mdlGasto');
            const titleEl = document.getElementById('mdlGastoTitle');

            // set title
            if (modo === 'EDITAR') titleEl.textContent = 'Editar gasto';
            else titleEl.textContent = 'Nuevo gasto';

            // set hidden id (si existe)
            const hf = document.getElementById('<%= hfIdGasto.ClientID %>');
            if (hf) hf.value = (modo === 'EDITAR' && id) ? id : '';

            // abrir modal
            const modal = new bootstrap.Modal(modalEl);
            modal.show();
        }

        function ConfirmarEliminarGasto(id) {
            const modalEl = document.getElementById('mdlEliminarGasto');
            const hf = document.getElementById('<%= hfIdEliminar.ClientID %>');
            if (hf) hf.value = id || '';

            const modal = new bootstrap.Modal(modalEl);
            modal.show();
        }
    </script>

</asp:Content>