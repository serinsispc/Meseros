<%@ Page Title="Gastos" Language="C#" MasterPageFile="~/Menu.Master" AutoEventWireup="true"
    CodeBehind="PGastos.aspx.cs" Inherits="WebApplication.PGastos" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container-fluid py-3">
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
            <div>
                <h3 class="mb-0 fw-bold">Gastos</h3>
                <div class="text-muted small">Administra, filtra y controla los egresos registrados.</div>
            </div>

            <div class="d-flex gap-2">
                <a href="caja.aspx" class="btn btn-outline-secondary rounded-pill shadow-sm px-4">
                    <i class="bi bi-arrow-left me-2"></i>Volver a caja
                </a>
                <button type="button" class="btn btn-primary rounded-pill shadow-sm px-4"
                    id="btnNuevoGasto" onclick="AbrirModalGasto('NUEVO')">
                    <i class="bi bi-plus-circle me-2"></i>Nuevo gasto
                </button>
            </div>
        </div>

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
                            Text="Filtrar" OnClick="btnFiltrar_Click" />
                        <asp:Button ID="btnLimpiar" runat="server" CssClass="btn btn-outline-secondary rounded-pill px-4"
                            Text="Limpiar" OnClick="btnLimpiar_Click" />
                    </div>
                </div>
            </div>
        </div>

        <div class="card shadow-sm border-0">
            <div class="card-body">
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-2">
                    <div class="text-muted small">
                        Listado de gastos registrados
                        <span class="ms-2 badge rounded-pill text-bg-light border" id="lblTotalGastos"><%: ListaGastos.Count %></span>
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
                            <% if (ListaGastos != null && ListaGastos.Any()) { %>
                                <% foreach (var gasto in ListaGastos) { %>
                                <tr>
                                    <td><span class="badge text-bg-light border"><%= gasto.fecha.ToString("yyyy-MM-dd") %></span></td>
                                    <td>
                                        <div class="fw-semibold"><%= gasto.concepto %></div>
                                        <div class="text-muted small">ID gasto: <%= gasto.idGasto %></div>
                                    </td>
                                    <td class="text-end fw-bold"><%= FormatearMoneda(gasto.valor) %></td>
                                    <td><%= gasto.nombreBolsillo %></td>
                                    <td><%= gasto.nombreTipoGasto %></td>
                                    <td class="text-end">
                                        <div class="btn-group">
                                            <button type="button" class="btn btn-sm btn-outline-primary"
                                                data-id="<%= gasto.idGasto %>"
                                                data-fecha="<%= gasto.fecha.ToString("yyyy-MM-dd") %>"
                                                data-concepto="<%= gasto.concepto %>"
                                                data-valor="<%= gasto.valor.ToString(System.Globalization.CultureInfo.InvariantCulture) %>"
                                                data-bolsillo="<%= gasto.idBolsillo %>"
                                                data-tipogasto="<%= gasto.idTipoGasto %>"
                                                onclick="AbrirModalGastoEditar(this); return false;">
                                                <i class="bi bi-pencil-square"></i>
                                            </button>
                                            <button type="button" class="btn btn-sm btn-outline-danger"
                                                onclick="ConfirmarEliminarGasto(<%= gasto.idGasto %>)">
                                                <i class="bi bi-trash"></i>
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                                <% } %>
                            <% } else { %>
                                <tr>
                                    <td colspan="6" class="text-center text-muted py-5">
                                        <i class="bi bi-inbox fs-2 d-block mb-2"></i>
                                        No hay gastos registrados.
                                    </td>
                                </tr>
                            <% } %>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="mdlGasto" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold" id="mdlGastoTitle">Nuevo gasto</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>

                <div class="modal-body">
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
                            <asp:TextBox ID="txtValor" runat="server" CssClass="form-control" placeholder="0" />
                            <div class="form-text">Puedes formatearlo en el backend o con JS.</div>
                        </div>

                        <div class="col-12 col-md-4">
                            <label class="form-label small text-muted mb-1">Bolsillo</label>
                            <asp:DropDownList ID="ddlBolsillo" runat="server" CssClass="form-select">                            </asp:DropDownList>
                        </div>

                        <div class="col-12 col-md-4">
                            <label class="form-label small text-muted mb-1">Tipo de gasto</label>
                            <asp:DropDownList ID="ddlTipoGasto" runat="server" CssClass="form-select">                            </asp:DropDownList>
                        </div>
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary rounded-pill px-4" data-bs-dismiss="modal">
                        Cancelar
                    </button>
                    <asp:Button ID="btnGuardarGasto" runat="server" CssClass="btn btn-primary rounded-pill px-4"
                        Text="Guardar" OnClick="btnGuardarGasto_Click" />
                </div>
            </div>
        </div>
    </div>

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
                    <asp:Button ID="btnEliminarGasto" runat="server" CssClass="btn btn-danger rounded-pill px-4"
                        Text="Eliminar" OnClick="btnEliminarGasto_Click" />
                </div>
            </div>
        </div>
    </div>

    <script>
        function AbrirModalGasto(modo, id) {
            const modalEl = document.getElementById('mdlGasto');
            const titleEl = document.getElementById('mdlGastoTitle');
            const hf = document.getElementById('<%= hfIdGasto.ClientID %>');
            const txtFecha = document.getElementById('<%= txtFechaGasto.ClientID %>');
            const txtConcepto = document.getElementById('<%= txtConcepto.ClientID %>');
            const txtValor = document.getElementById('<%= txtValor.ClientID %>');
            const ddlBolsillo = document.getElementById('<%= ddlBolsillo.ClientID %>');
            const ddlTipoGasto = document.getElementById('<%= ddlTipoGasto.ClientID %>');

            if (modo === 'EDITAR') {
                titleEl.textContent = 'Editar gasto';
            } else {
                titleEl.textContent = 'Nuevo gasto';
                if (hf) hf.value = '';
                if (txtFecha && !txtFecha.value) txtFecha.value = new Date().toISOString().slice(0, 10);
                if (txtConcepto) txtConcepto.value = '';
                if (txtValor) txtValor.value = '';
                if (ddlBolsillo) ddlBolsillo.value = '';
                if (ddlTipoGasto) ddlTipoGasto.value = '';
            }

            if (hf && modo === 'EDITAR') hf.value = id || '';

            const modal = new bootstrap.Modal(modalEl);
            modal.show();
        }

        function AbrirModalGastoEditar(btn) {
            AbrirModalGasto('EDITAR', btn ? btn.dataset.id : '');

            const txtFecha = document.getElementById('<%= txtFechaGasto.ClientID %>');
            const txtConcepto = document.getElementById('<%= txtConcepto.ClientID %>');
            const txtValor = document.getElementById('<%= txtValor.ClientID %>');
            const ddlBolsillo = document.getElementById('<%= ddlBolsillo.ClientID %>');
            const ddlTipoGasto = document.getElementById('<%= ddlTipoGasto.ClientID %>');

            if (btn && txtFecha) txtFecha.value = btn.dataset.fecha || '';
            if (btn && txtConcepto) txtConcepto.value = btn.dataset.concepto || '';
            if (btn && txtValor) txtValor.value = btn.dataset.valor || '';
            if (btn && ddlBolsillo) ddlBolsillo.value = btn.dataset.bolsillo || '';
            if (btn && ddlTipoGasto) ddlTipoGasto.value = btn.dataset.tipogasto || '';
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