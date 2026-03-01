<%@ Page Title="Servicios" Language="C#" MasterPageFile="~/Menu.Master" AutoEventWireup="true" CodeBehind="Menu.aspx.cs" Inherits="WebApplication.Menu"
    MaintainScrollPositionOnPostback="false" Async="true" %>



<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <!-- =========================
     Estilos específicos de la página
     ========================= -->
    <style>
        /* Contenedor flotante */
        .scroll-buttons {
            position: fixed;
            bottom: 20px;
            right: 20px;
            display: flex;
            flex-direction: column;
            gap: 10px;
            z-index: 9999;
        }

        .scroll-btn {
            width: 50px;
            height: 50px;
            border-radius: 50%;
            border: none;
            background-color: #007bff;
            color: white;
            font-size: 24px;
            cursor: pointer;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.3);
            transition: background-color 0.3s, transform 0.3s;
        }

            .scroll-btn:hover {
                background-color: #0056b3;
                transform: scale(1.1);
            }

        /* Pequeñas utilidades usadas en el markup */
        .service-chip.active {
            box-shadow: 0 0 0 3px rgba(0,123,255,0.12);
        }

        .mesa-card {
            padding: 10px;
            border-radius: 6px;
            display: block;
            text-decoration: none;
        }

            .mesa-card.libre {
                background: #f8f9fa;
                color: #212529;
            }

            .mesa-card.ocupada {
                background: #ffdede;
                color: #212529;
            }

        .producto-item {
            border-bottom: 1px solid #eee;
            padding: 8px 0;
        }

        .controls-inline .qty-input, .quantity-input {
            width: 70px;
        }

        .icon-btn {
            background: transparent;
            border: none;
            cursor: pointer;
            padding: 6px;
        }

            .icon-btn.danger {
                color: #dc3545;
            }

        .price-badge {
            padding: 6px 10px;
            background: #f1f3f5;
            border-radius: 6px;
        }

        .cta {
            font-weight: 600;
            border-radius: 8px;
        }

        .cta-orange {
            background: linear-gradient(90deg,#ff7a00,#ff9a33);
            color: white;
            border: none;
        }

        .cta-purple {
            background: linear-gradient(90deg,#7b61ff,#b992ff);
            color: white;
            border: none;
        }

        .cta-green {
            background: linear-gradient(90deg,#28a745,#52d172);
            color: white;
            border: none;
        }
    </style>

    <!-- =========================
         Scripts globales pequeños (scroll)
         ========================= -->
    <script>
        function scrollToTop() {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
        function scrollToBottom() {
            window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' });
        }
    </script>


    <!-- Hidden fields / botones ocultos para postback -->
    <asp:HiddenField ID="hfMesaId" runat="server" />
    <asp:HiddenField ID="hfServicioId" runat="server" />

    <asp:HiddenField ID="hdIdClienteDomicilio" runat="server" />

    <asp:Button ID="btnMesaNuevaCuenta" runat="server"
        OnClick="MesaNuevaCuenta" Style="display: none" UseSubmitBehavior="false" />

    <asp:Button ID="btnMesaAmarrar" runat="server"
        OnClick="MesaAmarar" Style="display: none" UseSubmitBehavior="false" />

    <!-- =========================
         Main layout (columns)
         ========================= -->
    <div class="container-fluid menu-wrap py-3 py-lg-4">
        <!-- fila: barra de servicios + acciones -->
        <div class="row g-3 align-items-center mb-3">
            <div class="col-12 col-xl">
                <div class="d-flex flex-wrap gap-2">
                    <asp:Repeater runat="server" ID="rpCuentas"
                        DataSource="<%# Models.cuentas %>"
                        OnItemCommand="rpServicios_ItemCommand">
                        <ItemTemplate>
                            <div class="position-relative">
                                <!-- Botón flotante del lápiz -->
                                <button type="button"
                                    class="btn btn-link position-absolute top-0 end-0 p-0 m-1 z-3 btn-alias"
                                    data-id='<%# Eval("id") %>'
                                    data-alias='<%# Eval("aliasVenta") %>'
                                    title="Editar alias">
                                    <i class="bi bi-pencil-fill text-warning fs-5"></i>
                                </button>

        


                                <!-- LinkButton principal -->
                                <asp:LinkButton ID="btnServicio" runat="server"
                                    data-id='<%# Eval("id") %>'
                                    data-alias='<%# Eval("aliasVenta") %>'
                                    CommandName="AbrirServicio"
                                    CommandArgument='<%# Eval("id") %>'
                                    CssClass='<%# "service-chip w-100 d-block text-start p-3 border rounded shadow-sm bg-white position-relative" 
           + (Convert.ToInt32(Eval("id")) == Models.IdCuentaActiva ? " active" : "") %>'>
                <span class="fw-bold text-primary d-block fs-5"><%# Eval("aliasVenta") %></span>

<small class="text-muted d-block">
    <%# MostrarNombreCliente(Eval("nombreCD"), Eval("nombremesa")) %>
</small>


         
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>


                </div>
            </div>

            <div class="col-12 col-xl-auto">
                <div class="d-flex gap-2 justify-content-start justify-content-xl-end">

                    <%-- btnDomicilio --%>
                    <button type="button"
                        class="btn btn-success btn-sm"
                        id="btnDomicilio"
                        data-idmesa="<%# Models.IdMesaActiva %>"
                        data-idservicio="<%# Models.IdCuentaActiva %>"
                        onclick="btnDomicilio_click(this);">
                        <i class="bi bi-house-door me-1"></i>Domicilio
                    </button>



                    <%-- btnNuevoServicio --%>
                    <button id="btnNuevoServicio"
                        type="button"
                        class="btn btn-primary btn-sm">
                        <i class="bi bi-plus-circle me-1"></i>Nuevo servicio
                    </button>

                    <%-- btnLiberarMesa --%>
                    <button type="button"
                        class="btn btn-outline-danger btn-sm"
                        id="btnLiberarMesa"
                        data-idservicio='<%# Models.IdCuentaActiva %>'
                        data-idmesa='<%# Models.IdMesaActiva %>'>
                        <i class="bi bi-x-circle me-1"></i>Liberar mesa
                    </button>


                </div>
            </div>

        </div>

        <!-- banner servicio activo -->
        <div class="alert alert-primary-soft d-flex align-items-center justify-content-between px-3 py-2 mb-3">
            <div class="fw-semibold">Mesero Activo: <%# Models.NombreMesero %> </div>
            <div class="small text-muted"></div>
        </div>

        <!-- 3 columnas -->
        <div class="row g-3">
            <!-- Columna 1: Zonas / Mesas -->
            <% if (Models.zonas.Count > 0)
            {%>
            <div class="col-12 col-lg-6 col-xl-4">
                <div class="card h-100">
                    <div class="card-body">
                        <ul class="nav nav-pills mb-3 zone-tabs">
                            <asp:Repeater ID="rpZonas" runat="server"
                                DataSource="<%# Models.zonas %>"
                                OnItemCommand="rpZonas_ItemCommand">
                                <ItemTemplate>
                                    <li class="nav-item m-1">
                                        <asp:LinkButton ID="lnkZona" runat="server"
                                            CommandName="CambiarZona"
                                            CommandArgument='<%# Eval("id") %>'
                                            CssClass='<%# (Session["zonaactiva"]?.ToString() == Eval("id").ToString()) ? "nav-link bg-primary text-white" : "nav-link" %>'>
                                            <%# Eval("nombreZona") %>
                                        </asp:LinkButton>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>

                        <!-- grilla de mesas -->
                        <div class="row g-2 lista-mesas">
                            <asp:Repeater runat="server" ID="rpMesas"
                                DataSource='<%# (Models != null && Models.Mesas != null
                         ? Models.Mesas.Where(x => x.idZona == Models.IdZonaActiva).ToList()
                         : new System.Collections.Generic.List<DAL.Model.Mesas>()) %>'>
                                <ItemTemplate>
                                    <div class="col-2 col-lg-6 col-xl-4" style="min-width: 100px; max-width: 110px">
                                        <button 
                                            id="lnkMesa"
                                            type="button"
                                            data-id='<%# Eval("id") %>'
                                            data-name='<%# Eval("nombreMesa") %>'
                                            class='<%# (Convert.ToInt32(Eval("estadoMesa")) == 1)
                                ? "btnMesa mesa-card ocupada d-block text-start"
                                : "btnMesa mesa-card libre d-block text-start" %>'>
                                            <div class="mesa-titulo"><%# Eval("nombreMesa") %></div>
                                            <div class="mesa-sub">
                                                <%# (Convert.ToInt32(Eval("estadoMesa")) == 1) ? "Ocupada" : "Libre" %>
                                            </div>
                                        </button>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>



                    </div>
                </div>
            </div>
            <% } %>


<!-- Columna 2: Productos -->
<div class='col-12 <%= (Models != null && Models.zonas != null && Models.zonas.Count > 0)
        ? "col-lg-6 col-xl-4"
        : "col-lg-12 col-xl-8" %>'>

                <div class="card h-100">
                    <div class="card-body">
                        <!-- Buscador -->
                        <div class="input-group mb-3">
                            <span class="input-group-text bg-white"><i class="bi bi-search"></i></span>
                            <input
                                type="search"
                                inputmode="search"
                                id="buscador-productos"
                                class="form-control"
                                placeholder="Buscar código / nombre"
                                autocomplete="on"
                                 autofocus  />
                            <button type="button" id="limpiar-buscador" class="btn btn-outline-secondary">
                                <i class="bi bi-x-lg"></i>
                            </button>
                        </div>

                        <!-- categorías -->
                        <div class="d-flex flex-wrap gap-2 mb-3">
                            <div id="categorias-container">
                                <%
                                    // Blindaje contra null
                                    var categorias = Models?.categorias ?? new List<DAL.Model.V_Categoria>();
                                    var idCatActiva = Models?.IdCategoriaActiva ?? 0;
                                    foreach (var cat in categorias)
                                    {
                                %>
                                <a href="#"
                                    class="pill <%= (cat.id == idCatActiva ? "active" : "") %>"
                                    data-id="<%= cat.id %>">
                                    <%= cat.nombreCategoria %>
                                </a>
                                <%
                                    }
                                %>
                            </div>
                        </div>


                        <!-- lista de productos -->
                        <div class="vstack gap-2">
                            <div id="productos-container">
                                <asp:Repeater ID="rpProductos" runat="server" OnItemCommand="rpProductos_ItemCommand">
                                    <ItemTemplate>
                                        <div class="producto-item" data-categoria='<%# Eval("idCategoria") %>'>
                                            <div class="prod-main d-flex justify-content-between align-items-center">
                                                <div class="prod-info">
                                                    <div class="prod-name"><%# Eval("nombreProducto") %></div>
                                                    <div class="prod-meta">
                                                        $<%# string.Format("{0:N0}", Eval("precioVenta")) %>
                                                        <a href="#" class="link-primary small ms-2">Ver detalle</a>
                                                    </div>
                                                </div>

                                                <div class="product-actions d-flex align-items-center">
                                                    <div class="controls-inline d-flex align-items-center gap-2">
                                                        <button type="button" class="btn btn-light btn-sm minus" title="Disminuir">
                                                            <i class="bi bi-dash"></i>
                                                        </button>

                                                        <asp:TextBox ID="txtCantidad" runat="server" CssClass="form-control qty-input text-center" Text="0" />

                                                        <button type="button" class="btn btn-light btn-sm plus" title="Aumentar">
                                                            <i class="bi bi-plus"></i>
                                                        </button>

                                                        <asp:LinkButton ID="btnAgregarCarrito" runat="server"
                                                            CommandName="AgregarAlCarrito"
                                                            CommandArgument='<%# Eval("idPresentacion") %>'
                                                            CssClass="icon-btn cart-btn ms-2"
                                                            title="Agregar al carrito">
                                                            <i class="bi bi-cart2"></i>
                                                        </asp:LinkButton>
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

            <!-- Columna 3: Pedido -->
            <div class="col-12 col-xl-4">
                <div class="card h-100 order-card">
                    <div class="card-body">
                        <div class="d-flex align-items-center gap-2 mb-2">
                            <button runat="server" id="btnNuevaCuentaUI" type="button"
                                class="btn btn-success-subtle btn-sm border-success text-success"
                                onclick="abrirModalCuenta('crear', '', '');">
                                <i class="bi bi-plus-lg me-1"></i>Nueva cuenta
                            </button>

                            <div class="flex-grow-1">
                                <asp:LinkButton ID="btnCuentaGeneral"
                                    runat="server"
                                    OnClick="btnCuentaGeneral_Click"
                                    CssClass="btn btn-primary w-100 text-white d-flex justify-content-between align-items-center">

        <span>Cuenta General</span>

        <span>
            <%
                // Blindaje seguro
                decimal total = Models?.venta?.total_A_Pagar ?? 0M;
                string totalFormato = string.Format(
                    new System.Globalization.CultureInfo("es-CO"),
                    "{0:C0}",
                    total
                );
            %>
            <%= totalFormato %>
        </span>

                                </asp:LinkButton>
                            </div>



                        </div>

                        <!-- Lista Cuentas Clientes -->
                        <div class="mb-3">
                            <div class="d-flex flex-wrap gap-2">
                                <% 
                                    // Asegurarse que la colección no sea null y filtrar por move
                                    if (Models?.v_CuentaClientes != null && Models.v_CuentaClientes.Any(x => x.idVenta == Models.IdCuentaActiva))
                                    {
                                        var cuentasFiltradas = Models.v_CuentaClientes.Where(x => x.idVenta == Models.IdCuentaActiva);


                                        foreach (var cuenta in cuentasFiltradas)
                                        { %>
                                <a class="card cuenta-card border-success-subtle shadow-sm" style="min-width: 160px; flex: 1;"
                                    data-id='<%= cuenta.id %>'>
                                    <div class="card-body p-2 d-flex flex-column justify-content-between">
                                        <div class="d-flex justify-content-between align-items-start mb-1">
                                            <span class="fw-semibold text-uppercase text-truncate" style="max-width: 120px;">
                                                <%= cuenta.nombreCuenta %>
                                            </span>
                                            <button type="button" class="btn btn-link p-0 text-success"
                                                title="Editar cuenta"
                                                onclick="abrirModalCuenta('editar', '<%= cuenta.id %>', '<%= cuenta.nombreCuenta %>');">
                                                <i class="bi bi-pencil-square"></i>
                                            </button>
                                        </div>
                                        <div class="text-end fw-bold text-success fs-6">
                                            <%= string.Format(new System.Globalization.CultureInfo("es-CO"), "{0:C0}", cuenta.total_A_Pagar) %>
                                        </div>
                                    </div>
                                </a>
                                <%      }
                                    }
                                    else
                                    { %>
                                <div class="alert alert-light border text-center w-100 p-2">
                                    No hay cuentas activas
                                </div>
                                <% } %>
                            </div>
                        </div>

                        <!-- lista detalle pedido -->
                        <asp:Repeater runat="server" ID="rpDetalleCaja" DataSource="<%# Models.detalleCaja %>">
                            <ItemTemplate>
                                <div class="pedido-item mb-2 p-2 border rounded" style="width: 100%; box-sizing: border-box;">
                                    <div class="d-flex flex-column">
                                        <asp:Panel runat="server" CssClass="account-badge"
                                            Visible='<%# !String.IsNullOrEmpty(Eval("nombreCuenta") as string) %>'>
                                            <%# Eval("nombreCuenta") %>
                                        </asp:Panel>

                                        <div class="d-flex justify-content-between align-items-start mb-2">
                                            <div class="nombre-producto fw-semibold lh-sm text-uppercase"><%# Eval("nombreProducto") %></div>
                                            <div class="small text-muted precio-pequenho"><%# string.Format(new System.Globalization.CultureInfo("es-CO"), "{0:C0}", Eval("precioVenta")) %></div>
                                        </div>

                                        <div class="d-flex justify-content-start align-items-center gap-2 flex-wrap mb-2">
                                            <div class="quantity-group btn-group btn-group-sm" role="group" aria-label="Cantidad">
                                                <button type="button" class="btn btn-light btn-qty btn-square btn-decrease">
                                                    <i class="bi bi-dash"></i>
                                                </button>

                                                <asp:TextBox ID="txtCantidad"
    runat="server"
    CssClass="form-control quantity-input text-center"
    Text='<%# FormatearNumero3Dec(Eval("unidad")) %>'
    TextMode="Number"
    step="0.001"
    min="1"
    style="width:100px;" />


                                                <button type="button" class="btn btn-light btn-qty btn-square btn-increase">
                                                    <i class="bi bi-plus"></i>
                                                </button>
                                            </div>

                                            <button type="button" class="icon-btn cart-btn ms-auto" title="Guardar" data-id='<%# Eval("id") %>'>
                                                <i class="bi bi-floppy"></i>
                                            </button>
                                        </div>

                                        <div class="d-flex flex-wrap gap-2 mb-2">
                                            <button type="button" class="icon-btn btn-comentario" title="Comentario"
                                                data-id='<%# Eval("id") %>'
                                                data-idcategoria='<%# Eval("idCategoria") %>'
                                                data-adiciones='<%# Eval("adiciones") %>'
                                                data-bs-toggle="modal" data-bs-target="#modalNotasDetalle">
                                                <i class="bi bi-chat"></i>
                                            </button>

                                            <button type="button" class="icon-btn btn-anclar" title="Anclar" data-id='<%# Eval("id") %>'><i class="bi bi-link-45deg"></i></button>
                                            <button type="button" class="icon-btn btn-eliminar danger" title="Eliminar" data-id='<%# Eval("id") %>'><i class="bi bi-trash"></i></button>
                                            <button type="button" class="icon-btn btn-dividir" title="Dividir" data-id='<%# Eval("id") %>' data-cantidadactual='<%# Eval("unidad") %>'><i class="bi bi-scissors"></i></button>

                                            <% if (Session["cajero"] != null && Convert.ToInt32(Session["cajero"]) == 1)
                                                { %>

                                            <button type="button" class="icon-btn btn-EditarPrecio" title="Editar Precio" data-id='<%# Eval("id") %>' data-valor='<%# Eval("precioVenta") %>'><i class="bi bi-cash-coin"></i></button>
                                            <button type="button" class="icon-btn btn-EditarProducto" title="Editar Producto" data-id='<%# Eval("id") %>' data-descripcion='<%# Eval("nombreProducto") %>'><i class="bi bi-pencil-square"></i></button>


                                            <% } %>
                                        </div>



                                        <div class="d-flex justify-content-between align-items-center">
                                            <div class="text-muted">
                                                <i class="bi bi-journal-text me-1"></i>
                                                <%# Eval("adiciones") %>
                                            </div>
                                            <div class="price-badge">
                                                <%# string.Format(new System.Globalization.CultureInfo("es-CO"), "{0:C0}", Eval("totalDetalle")) %>
                                            </div>
                                        </div>

                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>

                        <hr />
                        <%
                            dynamic totales;
                            if (Models != null)
                            {
                                if (Models.IdCuenteClienteActiva == 0)
                                {
                                    if (Models.venta != null)
                                    {
                                        totales = Models.venta;
                                    }
                                    else
                                    {
                                        totales = new { subtotalVenta = 0, ivaVenta = 0, totalVenta = 0, por_propina = 0, propina = 0, total_A_Pagar = 0 };
                                    }
                                }
                                else
                                {
                                    if (Models.ventaCuenta != null)
                                    {
                                        totales = Models.ventaCuenta;
                                    }
                                    else
                                    {
                                        totales = new { subtotalVenta = 0, ivaVenta = 0, totalVenta = 0, por_propina = 0, propina = 0, total_A_Pagar = 0 };
                                    }
                                }
                            }
                            else
                            {
                                totales = new { subtotalVenta = 0, ivaVenta = 0, totalVenta = 0, por_propina = 0, propina = 0, total_A_Pagar = 0 };
                            }

                        %>
                        <!-- totales -->
                        <div class="d-flex justify-content-between small mb-1">
                            <span class="text-muted">SubTotal:</span>
                            <span><%= "$" + string.Format("{0:N0}", totales.subtotalVenta) %></span>
                        </div>
                        <div class="d-flex justify-content-between small mb-2">
                            <span class="text-muted">Impuestos (8%)</span>
                            <span><%= "$" + string.Format("{0:N0}", totales.ivaVenta) %></span>
                        </div>
                        <div class="d-flex justify-content-between fw-semibold mb-2">
                            <span>Total 1:</span>
                            <span><%= "$" + string.Format("{0:N0}", totales.totalVenta) %></span>
                        </div>



                        <div class="d-flex justify-content-between align-items-center mb-3">
                            <span>Servicio (<%= totales.por_propina %>%)</span>
                            <div>
                                <button 
                                    type="button"
                                    id="btnEditarPropina"
                                    class="badge bg-primary-subtle text-primary fw-semibold me-2"
                                    data-porcentaje='<%= totales.por_propina %>'
                                    data-propina='<%= Convert.ToInt32(totales.propina) %>'
                                    data-idventa='<%# Models.IdCuentaActiva %>'
                                    data-idcuenta='<%# Models.IdCuenteClienteActiva %>'
                                    data-subtotal='<%= Convert.ToInt32(totales.subtotalVenta) %>'>
                                    Editar
                                </button>
                                <span><%= "$" + string.Format("{0:N0}", totales.propina) %></span>
                            </div>
                        </div>

                        <div class="d-flex justify-content-between fs-6 fw-bold mb-3">
                            <span>Total 2:</span>
                            <span><%= "$" + string.Format("{0:N0}", totales.total_A_Pagar) %></span>
                        </div>

                        <div class="row g-3">
                            <div class="col-12 col-md-4">
                                <button type="button" runat="server" id="btnComandar" onserverclick="btnComandar_ServerClick" class="cta cta-orange w-100" style="height: 80px;"><i class="bi bi-send me-2"></i>Comandar</button>
                            </div>
                            <div class="col-12 col-md-4">
                                <button type="button" runat="server" id="btnCuenta" onserverclick="btnCuenta_ServerClick" class="cta cta-purple w-100" style="height: 80px;">
                                    <i class="bi bi-chat-left-text me-2"></i>Solicitar<br />
                                    Cuenta</button>
                            </div>

                            <% if (Session["cajero"] != null && Convert.ToInt32(Session["cajero"]) == 1)
                                { %>

<div class="col-12 col-md-4">
    <button type="button"
            class="cta cta-green w-100"
            id="btnCobrar"
            style="height: 80px;"
            onclick="__doPostBack('btnCobrar','')">
        <i class="bi bi-cash-coin me-2"></i>Cobrar
    </button>
</div>


                            <% } %>



                        </div>

                    </div>
                    <!-- card-body -->
                </div>
                <!-- card -->
            </div>
            <!-- col -->
        </div>
        <!-- /row 3 cols -->
    </div>
    <!-- /container-fluid -->


        <!-- Modal: Editar Producto -->
<div class="modal fade" id="mdlEditarProducto" tabindex="-1" aria-hidden="true">

    <input type="hidden"
       id="hfIdEditarProducto"
       runat="server"
       ClientIDMode="Static" />

    <div class="modal-dialog modal-dialog-centered modal-md">
        <div class="modal-content">

            <div class="modal-header">
                <h5 class="modal-title">
                    Editar producto
                </h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
            </div>

            <div class="modal-body">

                <div class="mb-3">
                    <label for="txtEditarValor" class="form-label fw-semibold">
                        Nuevo producto
                    </label>

                    <input type="text"
                           id="txtNuevaDescripcion"
                           class="form-control"
                           runat="server"
                           ClientIDMode="Static"
                           placeholder="Ingrese el nuevo producto" />
                </div>

            </div>

            <div class="modal-footer">
                <button type="button"
                        class="btn btn-light"
                        data-bs-dismiss="modal">
                    Cancelar
                </button>

                <button type="button"
                        id="btnConfirmarEdicionProducto"
                        class="btn btn-primary">
                    Editar
                </button>
            </div>

        </div>
    </div>
</div>


    <!-- Modal: Editar valor -->
<div class="modal fade" id="mdlEditar" tabindex="-1" aria-hidden="true">

    <input type="hidden"
       id="hfIdEditar"
       runat="server"
       ClientIDMode="Static" />

    <div class="modal-dialog modal-dialog-centered modal-md">
        <div class="modal-content">

            <div class="modal-header">
                <h5 class="modal-title">
                    Editar valor
                </h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
            </div>

            <div class="modal-body">

                <div class="mb-3">
                    <label for="txtEditarValor" class="form-label fw-semibold">
                        Nuevo valor
                    </label>

                    <input type="number"
                           id="txtEditarValor"
                           class="form-control"
                           runat="server"
                           ClientIDMode="Static"
                           placeholder="Ingrese el nuevo valor" />
                </div>

            </div>

            <div class="modal-footer">
                <button type="button"
                        class="btn btn-light"
                        data-bs-dismiss="modal">
                    Cancelar
                </button>

                <button type="button"
                        id="btnConfirmarEdicion"
                        class="btn btn-primary">
                    Editar
                </button>
            </div>

        </div>
    </div>
</div>

    <!-- Modal: seleccionar servicio existente -->
    <div class="modal fade" id="mdlServicios" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Seleccionar servicio para <span id="lblMesaSeleccionada" class="fw-semibold"></span></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <p class="text-muted mb-2">Servicios activos:</p>

                    <div id="listaServicios" class="list-group">
                        <asp:Repeater ID="rpServiciosActivos" runat="server" DataSource="<%# Models.cuentas %>">
                            <ItemTemplate>
                                <button type="button"
                                    class="list-group-item list-group-item-action d-flex justify-content-between align-items-center servicio-item"
                                    data-id='<%# Eval("id") %>'>
                                    <span class="fw-semibold"><%# Eval("aliasVenta") %></span>
                                    <small class="text-muted">#<%# Eval("id") %> · <%# Eval("nombremesa") %></small>
                                </button>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <div class="text-center text-secondary mt-3" runat="server" id="divSinServicios"
                        visible='<%# (Models.cuentas?.Count ?? 0) == 0 %>'>
                        No hay servicios activos para amarrar.
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-light" data-bs-dismiss="modal">Cerrar</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal eliminar detalle -->
    <div class="modal fade" id="modalEliminarDetalle" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content p-3">
                <div class="modal-header">
                    <h5 class="modal-title">Eliminar detalle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <label for="notaEliminar" class="form-label">Ingrese el motivo de eliminación:</label>
                    <textarea id="notaEliminar" class="form-control" rows="3"></textarea>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                    <button type="button" class="btn btn-danger" id="btnConfirmarEliminar">Eliminar</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal alerta sesión -->
    <div class="modal fade" id="sessionModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-warning">
                <div class="modal-header bg-warning text-dark">
                    <h5 class="modal-title">Sesión a punto de expirar</h5>
                </div>
                <div class="modal-body">
                    Tu sesión ha expirado o está por expirar. Serás redirigido automáticamente.
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-primary" id="btnGoDefault">Ir Ahora</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Hidden fields para mantener estado -->
    <asp:HiddenField ID="hfCuentaId" runat="server" />
    <asp:HiddenField ID="hfCuentaMode" runat="server" />

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
                        <div id="cuentaError" class="form-text text-danger d-none">Ingrese un nombre válido (mín. 2 caracteres).</div>
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>

                    <!-- botón de guardar-client (no server control) -->
                    <button type="button" id="btnGuardarCuenta" class="btn btn-primary" onclick="guardarCuenta()">Guardar</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal: Anclar detalle a cuenta -->
    <div class="modal fade" id="modalAnclar" tabindex="-1" aria-labelledby="modalAnclarLabel" aria-hidden="true">
        <div class="modal-dialog modal-sm modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="modalAnclarLabel">Anclar detalle a cuenta</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div id="anclar-cuentas-list" class="d-grid gap-2">
                        <!-- botones de cuentas se agregan vía JS -->
                    </div>
                    <div id="anclar-empty" class="text-muted small d-none">No hay cuentas disponibles.</div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Hidden field para detalle seleccionado (opcional) -->
    <asp:HiddenField ID="hfDetalleId" runat="server" />


    <!-- Modal Dividir Detalle -->
    <div class="modal fade" id="modalDividirDetalle" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content p-3">
                <div class="modal-header">
                    <h5 class="modal-title">Dividir detalle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-2">
                        <label class="form-label">Cantidad actual:</label>
                        <input type="number" id="txtCantidadActual" class="form-control" readonly />
                    </div>
                    <div class="mb-2">
                        <label class="form-label">Cantidad a dividir:</label>
                        <input type="number" id="txtCantidadDividir" class="form-control" min="1" />
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                    <button type="button" class="btn btn-primary" id="btnConfirmarDividir">Dividir</button>
                </div>
            </div>
        </div>
    </div>



    <!-- Modal: Notas / Adiciones del detalle -->
    <div class="modal fade" id="modalNotasDetalle" tabindex="-1" aria-labelledby="modalNotasDetalleLabel" aria-hidden="true">
        <div class="modal-dialog modal-md modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="modalNotasDetalleLabel">Comentario / Adiciones</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">
                    <!-- Lista de adiciones -->
                    <div class="mb-2">
                        <div class="small text-muted mb-1">Toque para agregar o quitar:</div>
                        <div id="notas-adiciones-list" class="d-flex flex-wrap gap-2"></div>
                        <div id="notas-adiciones-empty" class="text-muted small d-none">No hay adiciones para esta categoría.</div>
                    </div>

                    <!-- Textarea -->
                    <label for="notas-adiciones-textarea" class="form-label small text-muted">Comentario (puede escribir manualmente):</label>
                    <textarea id="notas-adiciones-textarea" class="form-control" rows="4" placeholder="Ej: con hielo; sin verduras;"></textarea>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary" id="btnNotasLimpiar">
                        <i class="bi bi-eraser"></i>Limpiar
                    </button>
                    <button type="button" class="btn btn-primary" id="btnNotasGuardar">
                        <i class="bi bi-check2"></i>Guardar
                    </button>
                    <button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancelar</button>
                </div>
            </div>
        </div>
    </div>


    <!-- Modal: Editar Alias -->
    <div class="modal fade" id="modalAlias" tabindex="-1" aria-labelledby="modalAliasLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="modalAliasLabel">Editar alias de la cuenta</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">
                    <!-- ID cuenta oculto -->
                    <asp:HiddenField ID="HiddenField1" runat="server" />

                    <div class="mb-3">
                        <label for="txtAlias" class="form-label">Alias</label>
                        <asp:TextBox ID="txtAlias" runat="server" CssClass="form-control" MaxLength="100" />
                        <div class="form-text">Ej.: agrega el nombre personalizado para este servicio.</div>
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancelar</button>
                    <asp:Button ID="btnGuardarAlias" runat="server" CssClass="btn btn-primary"
                        Text="Guardar" OnClick="btnGuardarAlias_Click" />
                </div>
            </div>
        </div>
    </div>




    <!-- 🔒 Hidden para enviar datos al servidor -->
    <input type="hidden" id="hdnEditarPropina" name="hdnEditarPropina" />

    <!-- 🧮 Modal Editar Propina -->
    <!-- 🧮 Modal Editar Propina -->
    <div class="modal fade" id="modalPropina" tabindex="-1" aria-labelledby="modalPropinaLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-sm">
            <div class="modal-content">
                <div class="modal-header py-2">
                    <h6 class="modal-title" id="modalPropinaLabel">Editar propina</h6>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">
                    <div class="mb-2">
                        <label class="form-label small mb-1">Subtotal</label>
                        <input type="text" id="txtSubtotal" class="form-control form-control-sm" readonly>
                    </div>

                    <div class="row g-2">
                        <div class="col-6">
                            <label class="form-label small mb-1">% Propina</label>
                            <!-- ✅ Solo enteros -->
                            <input type="number" id="txtPorcentaje"
                                min="0" max="15" step="1"
                                class="form-control form-control-sm"
                                inputmode="numeric"
                                oninput="this.value = this.value.replace(/[^0-9]/g,'');" />
                        </div>
                        <div class="col-6">
                            <label class="form-label small mb-1">Valor propina</label>
                            <!-- ✅ Solo números sin decimales -->
                            <input type="text" id="txtPropina"
                                class="form-control form-control-sm"
                                inputmode="numeric"
                                pattern="[0-9]*"
                                oninput="this.value = this.value.replace(/[^0-9]/g,'');" />
                        </div>
                    </div>

                    <div class="mt-2 d-flex flex-wrap gap-1">
                        <button type="button" class="btn btn-outline-secondary btn-sm quick-tip" data-tip="0">0%</button>
                        <button type="button" class="btn btn-outline-secondary btn-sm quick-tip" data-tip="5">5%</button>
                        <button type="button" class="btn btn-outline-secondary btn-sm quick-tip" data-tip="10">10%</button>
                        <button type="button" class="btn btn-outline-secondary btn-sm quick-tip" data-tip="15">15%</button>
                        <button type="button" class="btn btn-outline-danger btn-sm" id="btnQuitarPropina">Quitar</button>
                    </div>

                    <small id="ayudaPropina" class="text-muted d-block mt-2"></small>
                </div>

                <div class="modal-footer py-2">
                    <button type="button" class="btn btn-light btn-sm" data-bs-dismiss="modal">Cancelar</button>
                    <button type="button" class="btn btn-primary btn-sm" id="btnGuardarPropina">Guardar</button>
                </div>
            </div>
        </div>
    </div>



    <!-- Modal DOMICILIOS -->
    <div class="modal fade" id="modalDomicilio" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">

                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title">
                        <i class="bi bi-phone"></i>
                        Domicilios
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>

                <div class="modal-body">

                    <!-- Buscar celular -->
                    <div class="mb-3">
                        <label class="form-label">Buscar celular</label>
                        <input type="text" id="txtBuscarCelular" class="form-control"
                            placeholder="Escriba el celular y presione Enter..." />
                    </div>

                    <div class="row g-2 mb-3">
                        <div class="col-md-4">
                            <label class="form-label">Teléfono</label>
                            <input type="text" id="txtTelefono" class="form-control" />
                        </div>
                        <div class="col-md-8">
                            <label class="form-label">Nombre Cliente</label>
                            <input type="text" id="txtNombreCliente" class="form-control" />
                        </div>
                    </div>

                    <div class="row g-2 mb-2 align-items-end">
                        <div class="col-md-9">
                            <label class="form-label">Dirección</label>
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
                        <button id="btnSeleccionarDomicilio" class="btn btn-success w-100">
                            <i class="bi bi-check-circle"></i>Seleccionar
                        </button>
                    </div>

                    <div class="table-responsive" style="max-height: 250px; overflow-y: auto;">
                        <table class="table table-hover mb-0" id="tblDomicilios">
                            <thead class="table-light">
                                <tr>
                                    <th>Teléfono</th>
                                    <th>Cliente</th>
                                    <th>Dirección</th>
                                </tr>
                            </thead>
                            <tbody>
                                <!-- Se llenará por JS -->
                            </tbody>
                        </table>
                    </div>

                </div>

                <div class="modal-footer justify-content-between">
                    <button type="button" class="btn btn-link" data-bs-dismiss="modal">
                        <i class="bi bi-x-circle"></i>Cerrar
                    </button>
                </div>

            </div>
        </div>
    </div>



    <script>
        // ==========================================================
        // ✅ ACTIBAR EBENTO CLIC AL PRECIONAR ENTER EN txtEditarValor
        // ==========================================================
        (function () {
            const byId = (id) => document.getElementById(id);

            const input = byId('txtEditarValor'); // ⚠️ ID real (ideal con ClientIDMode="Static")
            if (!input) return;

            input.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    e.stopPropagation();

                    // ✅ Aquí dispara el botón que tú quieres (ej: confirmar edición)
                    const btnOk = byId('btnConfirmarEdicion');
                    if (btnOk) btnOk.click();
                }
            });
        })();
    </script>


        <script>
            // ==========================================================
            // ✅ ACTIBAR EBENTO CLIC AL PRECIONAR ENTER EN txtNuevaDescripcion
            // ==========================================================
            (function () {
                const byId = (id) => document.getElementById(id);

                const input = byId('txtNuevaDescripcion'); // ⚠️ ID real (ideal con ClientIDMode="Static")
                if (!input) return;

                input.addEventListener('keydown', function (e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        e.stopPropagation();

                        // ✅ Aquí dispara el botón que tú quieres (ej: confirmar edición)
                        const btnOk = byId('btnConfirmarEdicionProducto');
                        if (btnOk) btnOk.click();
                    }
                });
            })();
        </script>



<script>
    // ==========================================================
    // ✅ BOTÓN EDITAR PRECIO (abre modal, pasa ID y deja foco listo)
    // ==========================================================
    (function () {
        const byId = (id) => document.getElementById(id);

        document.addEventListener('click', function (e) {

            const btn = e.target.closest('.btn-EditarPrecio');
            if (!btn) return;

            // 1️⃣ Obtener ID desde data-id
            const id = (btn.getAttribute('data-id') || '').trim();
            if (!id) return;

            // 2️⃣ Obtener valor desde data-valor
            const valor = (btn.getAttribute('data-valor') || '').trim();
            const valorDecimal = Number(valor.replace(',', '.')); // si viene "1500,5"

            // 3️⃣ Guardar ID en hidden del modal
            const hf = byId('hfIdEditar');
            if (hf) hf.value = id;

            // 4️⃣ Abrir modal
            const modalEl = byId('mdlEditar');
            if (!modalEl) return;

            const modal = bootstrap.Modal.getOrCreateInstance(modalEl, { backdrop: 'static' });
            modal.show();

            // 5️⃣ Cuando el modal YA esté visible: poner foco y preparar el input
            //    { once: true } evita que se acumulen listeners con cada click
            modalEl.addEventListener('shown.bs.modal', function () {

                const txt = byId('txtEditarValor');
                if (!txt) return;

                // ✅ Si quieres que aparezca el valor pero quede listo para escribir encima:
                txt.value = Number.isFinite(valorDecimal) ? valorDecimal.toString() : '';
                txt.focus();
                txt.select(); // así escribes y reemplaza de una

                // ✅ Si lo quieres borrar automáticamente en vez de seleccionar, usa esto:
                // txt.value = '';
                // txt.focus();

            }, { once: true });

        });
    })();
</script>


<script>
    // ==========================================================
    // ✅ BOTÓN EDITAR PRODUCTO (abre modal, pasa ID y deja foco listo)
    // ==========================================================

    // Helpers globales
    window.byId = (id) => document.getElementById(id);
    window.getValue = (id) => (window.byId(id)?.value ?? '').trim();

    (function () {

        document.addEventListener('click', function (e) {

            const btn = e.target.closest('.btn-EditarProducto');
            if (!btn) return;

            // 1️⃣ Obtener ID desde data-id
            const id = (btn.getAttribute('data-id') || '').trim();
            if (!id) return;

            // 2️⃣ Obtener descripción desde data-descripcion
            const descripcion = (btn.getAttribute('data-descripcion') || '').trim();

            // 3️⃣ Guardar ID en hidden
            const hf = byId('hfIdEditarProducto');
            if (hf) hf.value = id;

            // 4️⃣ Abrir modal
            const modalEl = byId('mdlEditarProducto');
            if (!modalEl) return;

            const modal = bootstrap.Modal.getOrCreateInstance(modalEl, { backdrop: 'static' });
            modal.show();

            // 5️⃣ Cuando el modal YA esté visible → foco + preparar input
            modalEl.addEventListener('shown.bs.modal', function () {

                const txt = byId('txtNuevaDescripcion'); // ID exacto
                if (!txt) return;

                // OPCIÓN A: mostrar valor y dejar listo para escribir encima (recomendado)
                txt.value = descripcion;
                txt.focus();
                txt.select();

                // OPCIÓN B: borrar y escribir directo (si prefieres)
                // txt.value = '';
                // txt.focus();

            }, { once: true });

        });

    })();
</script>


<script>
    // ==========================================================
    // ✅ CONFIRMAR EDICIÓN VALOR (Modal) - Versión ASPX (sin getVal)
    // ==========================================================
    (function () {
        const byId = (id) => document.getElementById(id);

        // PostBack seguro: usa __doPostBack si existe, si no, fallback con hidden
        const firePostBack = (target, argument) => {
            if (typeof window.__doPostBack === 'function') {
                window.__doPostBack(target, argument);
                return;
            }

            const form = document.querySelector('form');
            if (!form) {
                alert('No se encontró el formulario en el MasterPage.');
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

        const btnConfirmarEdicion = byId('btnConfirmarEdicion');
        if (!btnConfirmarEdicion) return;

        btnConfirmarEdicion.addEventListener('click', () => {

            const id = (byId('hfIdEditar')?.value || '').trim();

            // ✅ Leer valor del input sin getVal()
            const raw = (byId('txtEditarValor')?.value || '').toString();
            const valor = parseInt(raw.replace(/[^\d]/g, ''), 10) || 0;

            if (!id) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Falta el ID',
                    text: 'No se pudo identificar el registro a editar.',
                    confirmButtonColor: '#2563eb'
                });
                return;
            }

            if (valor <= 0) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Valor inválido',
                    text: 'Debe ingresar un valor válido.',
                    confirmButtonColor: '#2563eb'
                });
                return;
            }

            firePostBack('btnEditarValor', `${id}|${valor}`);

            const modalEl = byId('mdlEditar');
            if (modalEl) bootstrap.Modal.getOrCreateInstance(modalEl).hide();
        });

    })();
</script>


<script>
    // ==========================================================
    // ✅ CONFIRMAR EDICIÓN PRODUCTO (Modal) - Versión ASPX (sin getVal)
    // ==========================================================
    (function () {

        const byId = (id) => document.getElementById(id);
        const getValue = (id) => (byId(id)?.value ?? '').trim();

        // PostBack seguro: usa __doPostBack si existe, si no, fallback con hidden
        const firePostBack = (target, argument) => {
            if (typeof window.__doPostBack === 'function') {
                window.__doPostBack(target, argument);
                return;
            }

            const form = document.querySelector('form');
            if (!form) {
                alert('No se encontró el formulario en el MasterPage.');
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

        const btnConfirmarEdicion = byId('btnConfirmarEdicionProducto');
        if (!btnConfirmarEdicion) return;

        btnConfirmarEdicion.addEventListener('click', () => {

            const id = (byId('hfIdEditarProducto')?.value ?? '').trim();

            // ✅ OJO: confirma que el ID sea el mismo que tienes en el input real
            const descripcion = getValue('txtNuevaDescripcion'); // o 'txtNuevaDescriccion' si ese es el real

            if (!id) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Falta el ID',
                    text: 'No se pudo identificar el registro a editar.',
                    confirmButtonColor: '#2563eb'
                });
                return;
            }

            if (!descripcion) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Campo vacío',
                    text: 'Debe ingresar un nombre de producto.',
                    confirmButtonColor: '#2563eb'
                });
                return;
            }

            firePostBack('btnEditarProducto', `${id}|${descripcion}`);

            const modalEl = byId('mdlEditarProducto');
            if (modalEl) bootstrap.Modal.getOrCreateInstance(modalEl).hide();
        });

    })();
</script>


    <script type="text/javascript">
        window.ListaClientesDomicilio = <%= Newtonsoft.Json.JsonConvert.SerializeObject(Models.clienteDomicilios) %>;
    </script>


    <script>
        (function () {
            // --- Guardar inicialización para evitar doble wiring en UpdatePanel / postbacks parciales ---
            if (window.__menuInit) return;
            window.__menuInit = true;

            // -----------------------
            // Helpers DOM / utilidades
            // -----------------------
            function $id(id) { return document.getElementById(id); }
            function toArray(nodeList) { return nodeList ? Array.prototype.slice.call(nodeList) : []; }
            function formatSafe(val, fallback) { return val == null ? (fallback || '') : val; }

            // Obtener colecciones (accede a ClientID server controls con inline expressions)
            var hfMesaClientId = '<%= hfMesaId.ClientID %>';
            var hfServicioClientId = '<%= hfServicioId.ClientID %>';
            var btnMesaAmarrarUniqueId = '<%= btnMesaAmarrar.UniqueID %>';
            var btnMesaAmarrarClientId = '<%= btnMesaAmarrar.ClientID %>'; // en caso de necesitar
            // campos de cuentas (modal)
            var hfCuentaIdClientId = '<%= hfCuentaId.ClientID %>';
            var hfCuentaModeClientId = '<%= hfCuentaMode.ClientID %>';

            // -----------------------
            // Funciones exportadas (globales para uso en HTML)
            // -----------------------
            window.abrirModalServicios = function (nombreMesa, idMesa) {
                var lbl = $id('lblMesaSeleccionada');
                if (lbl) lbl.textContent = nombreMesa || '';

                var hf = $id(hfMesaClientId);
                if (hf) hf.value = idMesa || '';

                var el = $id('mdlServicios');
                if (!el) { console.warn('#mdlServicios no existe'); return; }

                if (window.bootstrap && bootstrap.Modal) {
                    var m = bootstrap.Modal.getInstance(el) || new bootstrap.Modal(el);
                    m.show();
                } else if (window.jQuery && $.fn.modal) {
                    $(el).modal('show');
                } else {
                    console.error('Bootstrap no está cargado.');
                }
            };

            window.abrirModalCuenta = function (mode, id, name) {
                var titulo = $id('modalCuentaTitulo');
                if (titulo) titulo.textContent = (mode === 'editar') ? 'Editar nombre de cuenta' : 'Nueva cuenta';

                var input = $id('txtCuentaNombre');
                if (input) input.value = name || '';

                var err = $id('cuentaError');
                if (err) err.classList.add('d-none');

                var hfId = $id(hfCuentaIdClientId);
                var hfMode = $id(hfCuentaModeClientId);
                if (hfId) hfId.value = id || '';
                if (hfMode) hfMode.value = mode || 'crear';

                var modalEl = $id('modalCuentaCliente');
                if (!modalEl) return console.warn('modalCuentaCliente no encontrado');
                var modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                modal.show();

                modalEl.addEventListener('shown.bs.modal', function () {
                    if (input) input.focus();
                }, { once: true });
            };

            window.guardarCuenta = function () {
                var input = $id('txtCuentaNombre');
                var err = $id('cuentaError');
                var hfId = $id(hfCuentaIdClientId);
                var hfMode = $id(hfCuentaModeClientId);

                var nombre = input ? input.value.trim() : '';
                var mode = hfMode ? hfMode.value : 'crear';
                var id = hfId ? hfId.value : '';

                if (!nombre || nombre.length < 2) {
                    if (err) err.classList.remove('d-none');
                    if (input) input.focus();
                    return;
                } else {
                    if (err) err.classList.add('d-none');
                }

                var safeNombre = nombre.replace(/\|/g, ' ');
                var argumento = mode + '|' + (id || '') + '|' + safeNombre;

                __doPostBack('GuardarCuenta', argumento);
            };

            // -----------------------
            // DOMContentLoaded: inicializadores y delegación
            // -----------------------
            document.addEventListener('DOMContentLoaded', function () {
                // referencias containers
                var serviciosContainer = $id('listaServicios');
                var buscador = $id('buscador-productos');
                var btnLimpiar = $id('limpiar-buscador');
                var categoriaContainer = $id('categorias-container');
                var productosContainer = $id('productos-container');

                // helpers para productos/categorias
                function obtenerProductosLista() {
                    return productosContainer ? toArray(productosContainer.querySelectorAll('.producto-item')) : [];
                }
                function obtenerCategoriasLista() {
                    return categoriaContainer ? toArray(categoriaContainer.querySelectorAll('.pill')) : [];
                }

                // Filtrado por texto
                function filtrarProductos(texto) {
                    texto = (texto || '').toLowerCase().trim();
                    var productos = obtenerProductosLista();
                    var categoriaCount = {};

                    productos.forEach(function (prod) {
                        var nameEl = prod.querySelector('.prod-name');
                        var nombre = nameEl ? nameEl.textContent.toLowerCase() : '';
                        var catId = prod.dataset.categoria || '';
                        var visible = nombre.indexOf(texto) !== -1;
                        prod.style.display = visible ? 'block' : 'none';
                        if (visible) categoriaCount[catId] = (categoriaCount[catId] || 0) + 1;
                    });

                    // activar categoria con más coincidencias
                    var maxCat = null, maxCount = 0;
                    for (var k in categoriaCount) {
                        if (categoriaCount[k] > maxCount) { maxCount = categoriaCount[k]; maxCat = k; }
                    }
                    var cats = obtenerCategoriasLista();
                    cats.forEach(function (c) {
                        if (c.dataset.id === maxCat) c.classList.add('active'); else c.classList.remove('active');
                    });
                }

                // Inicializar buscador
                if (buscador) {
                    buscador.addEventListener('input', function () {
                        filtrarProductos(buscador.value);
                    });
                }

                if (btnLimpiar) {
                    btnLimpiar.addEventListener('click', function () {
                        if (buscador) buscador.value = '';
                        obtenerProductosLista().forEach(function (p) { p.style.display = 'block'; });
                        obtenerCategoriasLista().forEach(function (c) { c.classList.remove('active'); });
                    });
                }

                // Manejo de categorias (delegación sobre container)
                if (categoriaContainer) {
                    categoriaContainer.addEventListener('click', function (ev) {
                        var a = ev.target.closest('.pill');
                        if (!a) return;
                        ev.preventDefault();
                        var idCategoria = a.dataset.id;
                        obtenerCategoriasLista().forEach(function (c) { c.classList.remove('active'); });
                        a.classList.add('active');
                        obtenerProductosLista().forEach(function (prod) {
                            prod.style.display = (prod.dataset.categoria === idCategoria) ? 'block' : 'none';
                        });
                        if (buscador) buscador.value = '';
                    });

                    // activar la categoría que tenga la clase active (si existe)
                    var primeraActiva = categoriaContainer.querySelector('.pill.active');
                    if (primeraActiva) primeraActiva.click();
                }

                // -----------------------
                // Delegación unificada de click sobre document.body
                // - minus / plus (productos)
                // - cart-btn (productos)
                // - btn-decrease / btn-increase / cart-btn en detalle pedido
                // - eliminar (icon-btn.danger)
                // -----------------------
                document.body.addEventListener('click', function (ev) {
                    // servicios modal: selección
                    var servicioBtn = ev.target.closest('.servicio-item');
                    if (servicioBtn && serviciosContainer && serviciosContainer.contains(servicioBtn)) {
                        var idServicio = servicioBtn.getAttribute('data-id');
                        if (idServicio) {
                            var hfServicio = $id(hfServicioClientId);
                            if (hfServicio) hfServicio.value = idServicio;

                            var modalEl = $id('mdlServicios');
                            if (modalEl && window.bootstrap && bootstrap.Modal) {
                                var modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                                modal.hide();
                                setTimeout(function () { __doPostBack(btnMesaAmarrarUniqueId, ''); }, 200);
                            } else {
                                __doPostBack(btnMesaAmarrarUniqueId, '');
                            }
                        }
                        return;
                    }

                    // Productos - decrease / increase (lista de productos)
                    var minus = ev.target.closest('.minus');
                    if (minus) {
                        var container = minus.closest('.producto-item');
                        var input = container ? container.querySelector('.qty-input') : null;
                        var val = parseInt(input?.value || '0') || 0;
                        if (val > 0 && input) input.value = val - 1;
                        return;
                    }
                    var plus = ev.target.closest('.plus');
                    if (plus) {
                        var container = plus.closest('.producto-item');
                        var input = container ? container.querySelector('.qty-input') : null;
                        var val = parseInt(input?.value || '0') || 0;
                        if (input) input.value = val + 1;
                        return;
                    }
                    var add = ev.target.closest('.cart-btn');
                    if (add && add.closest('.producto-item')) {
                        // Si el botón es LinkButton server-side, el postback lo gestiona ASP.NET automáticamente
                        // Si se quisiera manejar con fetch/axios, poner aquí la lógica
                        return;
                    }

                    // Detalle pedido - cantidad / guardar
                    var dec = ev.target.closest('.btn-decrease');
                    if (dec) {
                        var grp = dec.closest('.quantity-group');
                        var input = grp ? grp.querySelector('.quantity-input') : null;
                        if (!input) return;
                        var val = parseInt(input.value || '0') || 0;
                        if (val > 1) input.value = val - 1;
                        return;
                    }
                    var inc = ev.target.closest('.btn-increase');
                    if (inc) {
                        var grp2 = inc.closest('.quantity-group');
                        var input2 = grp2 ? grp2.querySelector('.quantity-input') : null;
                        if (!input2) return;
                        var val2 = parseInt(input2.value || '0') || 0;
                        input2.value = val2 + 1;
                        return;
                    }
                    var guardar = ev.target.closest('.pedido-item .cart-btn');
                    if (guardar) {
                        var id = guardar.getAttribute('data-id');
                        var row = guardar.closest('.pedido-item');
                        var inputCantidad = row ? row.querySelector('.quantity-input') : null;
                        //if (!inputCantidad || parseInt(inputCantidad.value || '0') < 1) {
                        //    alert('Ingrese una cantidad válida.');
                        //    return;
                        //}
                        // Llamada estándar al server; el code-behind debe manejar el target "ActualizarCantidad"
                        __doPostBack('ActualizarCantidad', id + '|' + inputCantidad.value);
                        return;
                    }

                    // Eliminar detalle (icon-btn.danger)
                    var eliminar = ev.target.closest('.icon-btn.danger');
                    if (eliminar) {
                        var idEliminar = eliminar.getAttribute('data-id');
                        var nota = $id('notaEliminar');
                        if (nota) nota.value = '';
                        var modalEl = $id('modalEliminarDetalle');
                        if (modalEl && window.bootstrap && bootstrap.Modal) {
                            var m = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                            m.show();
                            modalEl._idToDelete = idEliminar;
                        } else {
                            var motivo = prompt('Motivo de eliminación:');
                            if (motivo) __doPostBack('EliminarDetalle', idEliminar + '|' + motivo);
                        }
                        return;
                    }
                });

                // -----------------------
                // Confirmar eliminar (modal)
                // -----------------------
                var btnConfirmarEliminar = $id('btnConfirmarEliminar');
                if (btnConfirmarEliminar) {
                    btnConfirmarEliminar.addEventListener('click', function () {
                        var notaEl = $id('notaEliminar');
                        var motivo = notaEl ? notaEl.value.trim() : '';
                        var modalEl = $id('modalEliminarDetalle');
                        var idEliminar = modalEl ? modalEl._idToDelete : null;
                        if (!motivo) { alert('Debe ingresar una nota para eliminar el detalle.'); return; }
                        if (!idEliminar) { alert('No se determinó el detalle a eliminar.'); return; }
                        __doPostBack('EliminarDetalle', idEliminar + '|' + motivo);
                        if (modalEl && window.bootstrap && bootstrap.Modal) {
                            var m = bootstrap.Modal.getInstance(modalEl);
                            if (m) m.hide();
                        }
                    });
                }

                // -----------------------
                // Interceptar ENTER en inputs de cantidad (detalle)
                // -----------------------
                document.body.addEventListener('keydown', function (e) {
                    var input = e.target;
                    if (input && input.classList && input.classList.contains('quantity-input') && (e.key === 'Enter' || e.keyCode === 13)) {
                        e.preventDefault();
                        var row = input.closest('.pedido-item');
                        var btn = row ? row.querySelector('.cart-btn') : null;
                        if (!btn) return;
                        var v = parseInt(input.value || '0') || 0;
                        if (v < 1) { input.value = 1; return; }
                        btn.click();
                    }
                });

                // -----------------------
                // Sesión: mostrar modal al expirar (timeout calculado en servidor)
                // -----------------------
                (function () {
                    try {
                        var sessionTimeoutMinutes = <%= Session.Timeout %>;
                        var sessionTimeoutMs = sessionTimeoutMinutes * 60 * 1000;
                        var nombreDB = '<%= Session["db"] != null ? Session["db"].ToString() : "" %>';
                        setTimeout(function () {
                            var modalEl = $id('sessionModal');
                            if (!modalEl) return;
                            if (window.bootstrap && bootstrap.Modal) {
                                var m = new bootstrap.Modal(modalEl, { backdrop: 'static', keyboard: false });
                                m.show();
                                setTimeout(function () { window.location.href = 'Default.aspx?db=' + encodeURIComponent(nombreDB); }, 5000);
                            } else {
                                window.location.href = 'Default.aspx?db=' + encodeURIComponent(nombreDB);
                            }
                        }, sessionTimeoutMs);

                        var btnGo = $id('btnGoDefault');
                        if (btnGo) btnGo.addEventListener('click', function () {
                            window.location.href = 'Default.aspx?db=' + encodeURIComponent(nombreDB);
                        });
                    } catch (err) { console.warn(err); }


                    // Capturamos Enter en cualquier TextBox de cantidad dentro del repeater
                    document.querySelectorAll('.qty-input').forEach(function (input) {
                        input.addEventListener('keydown', function (e) {
                            if (e.key === 'Enter') {
                                e.preventDefault(); // Evita que el formulario se envíe por defecto

                                // Buscar el botón de carrito dentro del mismo contenedor de producto
                                const productoItem = input.closest('.producto-item');
                                if (productoItem) {
                                    const btn = productoItem.querySelector('.cart-btn');
                                    if (btn) {
                                        btn.click(); // Dispara el postback del LinkButton
                                    }
                                }
                            }
                        });
                    });

                })();

            }); // DOMContentLoaded
        })();
    </script>



    <script type="text/javascript">
        (function () {
            // --- Evita doble inicialización ---
            if (window.__initAnclar) return;
            window.__initAnclar = true;

            // --- Helpers ---
            function $id(id) { return document.getElementById(id); }
            function toArray(nodeList) { return nodeList ? Array.prototype.slice.call(nodeList) : []; }

            // --- Variable global con cuentas (desde code-behind) ---
            window.cuentas = <%= CuentasJson ?? "[]" %>;
            console.log("Cuentas cargadas:", window.cuentas);

            // --- Inicializar modal ---
            var modalEl = $id('modalAnclar');
            if (!modalEl) { console.error('modalAnclar no encontrado'); return; }

            var bsModal;
            function initModal() {
                if (!bsModal && window.bootstrap && bootstrap.Modal) {
                    bsModal = new bootstrap.Modal(modalEl, { backdrop: 'static', keyboard: true });
                }
            }
            initModal();

            // --- Función para abrir modal y crear botones ---
            function openAnclarModal(detalleId) {
                initModal(); // asegúrate que bootstrap.Modal esté inicializado

                var container = $id('anclar-cuentas-list');
                var empty = $id('anclar-empty');
                if (!container || !empty) return console.error('Elementos del modal no encontrados');

                container.innerHTML = '';

                if (!window.cuentas || !Array.isArray(window.cuentas) || window.cuentas.length === 0) {
                    empty.classList.remove('d-none');
                    empty.textContent = 'No hay cuentas disponibles';
                    bsModal.show();
                    return;
                }

                empty.classList.add('d-none');

                window.cuentas.forEach(function (c) {
                    if (!c) return;

                    var btn = document.createElement('button');
                    btn.type = 'button';
                    btn.className = 'btn btn-outline-primary btn-sm d-flex justify-content-between align-items-center';
                    btn.style.gap = '8px';
                    btn.setAttribute('data-cuenta-id', c.id ?? '');
                    btn.setAttribute('data-detalle-id', detalleId ?? '');

                    var left = document.createElement('span');
                    left.textContent = c.nombre ?? '(sin nombre)';

                    var right = document.createElement('span');
                    right.className = 'badge bg-light text-dark';
                    right.textContent = (!isNaN(Number(c.total)))
                        ? Number(c.total).toLocaleString('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 })
                        : c.total ?? '';

                    btn.appendChild(left);
                    btn.appendChild(right);

                    btn.addEventListener('click', function () {
                        var cuentaId = this.getAttribute('data-cuenta-id');
                        var detId = this.getAttribute('data-detalle-id');
                        var arg = (detId ?? '') + '|' + (cuentaId ?? '');
                        if (typeof __doPostBack === 'function') {
                            __doPostBack('AnclarDetalle', arg);
                        } else {
                            alert('No se puede hacer postback, __doPostBack no definido.');
                        }
                    });

                    container.appendChild(btn);
                });

                bsModal.show();
            }

            // --- Delegación global para abrir modal ---
            document.addEventListener('click', function (e) {
                var btn = e.target.closest('.btn-anclar');
                if (!btn) return;

                // Alert para verificar que el click está llegando
                console.log("Click detectado en .btn-anclar");
                //alert("Click detectado en .btn-anclar");

                var detalleId = btn.getAttribute('data-id');
                if (!detalleId) return console.warn('data-id no definido en el botón .btn-anclar');

                openAnclarModal(detalleId);
            });

        })();
    </script>


    <script>
        (function () {
            // --- Evita doble inicialización ---
            if (window.__initDividirDetalle) return;
            window.__initDividirDetalle = true;

            function $id(id) { return document.getElementById(id); }

            // --- Inicializar modal ---
            const modalEl = $id('modalDividirDetalle');
            if (!modalEl) { console.error('modalDividirDetalle no encontrado'); return; }

            let bsModal;
            function initModal() {
                if (!bsModal && window.bootstrap && bootstrap.Modal) {
                    bsModal = new bootstrap.Modal(modalEl, { backdrop: 'static', keyboard: true });
                }
            }
            initModal();

            const inputActual = $id('txtCantidadActual');
            const inputDividir = $id('txtCantidadDividir');
            const btnConfirmar = $id('btnConfirmarDividir');

            let detalleIdGlobal = null;
            let cantidadActualGlobal = 0;

            // --- Función para abrir modal ---
            function openDividirModal(detalleId, cantidadActual) {
                initModal();

                detalleIdGlobal = detalleId;
                cantidadActualGlobal = cantidadActual;

                if (inputActual) inputActual.value = cantidadActualGlobal;
                if (inputDividir) {
                    inputDividir.value = 1;
                    inputDividir.max = cantidadActualGlobal - 1;
                }

                bsModal.show();
            }

            // --- Delegación click en botones .btn-dividir ---
            document.addEventListener('click', function (e) {
                const btn = e.target.closest('.btn-dividir');
                if (!btn) return;

                const cantidadActual = parseInt(btn.getAttribute('data-cantidadActual') || '0');
                const detalleId = btn.getAttribute('data-id');

                if (!detalleId) {
                    console.warn('data-id no definido en el botón .btn-dividir');
                    return;
                }

                if (cantidadActual <= 1) {
                    Swal.fire({
                        icon: 'warning',
                        title: '¡Atención!',
                        text: 'No se puede dividir un detalle con cantidad menor o igual a 1.',
                        confirmButtonColor: '#3085d6',
                        confirmButtonText: 'Aceptar'
                    });
                    return;
                }

                console.log('Click detectado en .btn-dividir, detalleId:', detalleId);
                openDividirModal(detalleId, cantidadActual);
            });

            // --- Confirmar división ---
            if (btnConfirmar) {
                btnConfirmar.addEventListener('click', function () {
                    const cantidadDividir = parseInt(inputDividir.value || '0');
                    if (cantidadDividir < 1 || cantidadDividir >= cantidadActualGlobal) {
                        Swal.fire({
                            icon: 'warning',
                            title: '¡Atención!',
                            text: 'Cantidad inválida para dividir.',
                            confirmButtonColor: '#3085d6',
                            confirmButtonText: 'Aceptar'
                        });
                        return;
                    }

                    if (typeof __doPostBack === 'function') {
                        const arg = detalleIdGlobal + '|' + cantidadActualGlobal + '|' + cantidadDividir;
                        __doPostBack('DividirDetalle', arg);
                        bsModal.hide();
                    } else {
                        Swal.fire({
                            icon: 'success',
                            title: 'División realizada',
                            html: `DetalleId: <b>${detalleIdGlobal}</b><br>
                           Cantidad a dividir: <b>${cantidadDividir}</b>`,
                            confirmButtonColor: '#3085d6',
                            confirmButtonText: 'Aceptar'
                        });
                        bsModal.hide();
                    }
                });
            }

        })();
    </script>



    <script runat="server">
        protected string AdicionesJson
        {
            get
            {
                var lista = (Models != null && Models.adiciones != null)
                            ? Models.adiciones
                            : new List<DAL.Model.V_CatagoriaAdicion>();

                return Newtonsoft.Json.JsonConvert.SerializeObject(lista);
            }
        }
    </script>


    <!-- 👇 ESTO sí es cliente (JS en el navegador) -->
    <script>
        // V_CatagoriaAdicion => { id, idCategoria, idAdicion, nombreCategoria, nombreAdicion, estado }
        window.adiciones = <%= AdicionesJson %>;
        console.log('[NotasDetalle] adiciones cargadas:', Array.isArray(window.adiciones) ? window.adiciones.length : window.adiciones);
    </script>



    <script type="text/javascript">
        (function () {
            if (window.__initNotasDetalle2) return;
            window.__initNotasDetalle2 = true;

            function $id(id) { return document.getElementById(id); }
            function tokenize(str) {
                const seen = new Set(), out = [];
                (str || '').split(';').map(s => s.trim()).filter(Boolean).forEach(s => {
                    const k = s.toLowerCase(); if (!seen.has(k)) { seen.add(k); out.push(s); }
                });
                return out;
            }
            function joinCanon(tokens) { return tokens.length ? (tokens.join('; ') + ';') : ''; }

            var modalEl = $id('modalNotasDetalle');
            var listEl = $id('notas-adiciones-list');
            var emptyEl = $id('notas-adiciones-empty');
            var txtEl = $id('notas-adiciones-textarea');
            var btnGuardar = $id('btnNotasGuardar');
            var btnLimpiar = $id('btnNotasLimpiar');

            // Render de adiciones para la categoría
            function renderAdiciones(idCategoria, selectedTokens) {
                listEl.innerHTML = '';
                const cat = Number(idCategoria);

                const items = (window.adiciones || []).filter(a => Number(a.idCategoria) === cat);
                console.log('[NotasDetalle] filtrando por categoría', { cat, total: (window.adiciones || []).length, filtradas: items.length });

                if (!items.length) {
                    emptyEl.classList.remove('d-none');
                    return;
                }
                emptyEl.classList.add('d-none');

                const setSel = new Set((selectedTokens || []).map(t => t.toLowerCase()));

                items.forEach(a => {
                    const name = String(a.nombreAdicion || '').trim();
                    if (!name) return;

                    const btn = document.createElement('button');
                    btn.type = 'button';
                    btn.className = 'btn btn-sm btn-outline-secondary adicion-btn';
                    btn.dataset.name = name;
                    btn.textContent = name;

                    if (setSel.has(name.toLowerCase())) btn.classList.add('active');

                    btn.onclick = function () {
                        const toks = tokenize(txtEl.value);
                        const i = toks.findIndex(t => t.toLowerCase() === name.toLowerCase());
                        if (i >= 0) { toks.splice(i, 1); btn.classList.remove('active'); }
                        else { toks.push(name); btn.classList.add('active'); }
                        txtEl.value = joinCanon(toks);
                    };

                    listEl.appendChild(btn);
                });
            }


            function syncFromTextarea() {
                const toks = tokenize(txtEl.value);
                const set = new Set(toks.map(t => t.toLowerCase()));
                listEl.querySelectorAll('.adicion-btn').forEach(btn => {
                    const name = (btn.dataset.name || '').toLowerCase();
                    btn.classList.toggle('active', set.has(name));
                });
            }
            txtEl.addEventListener('input', syncFromTextarea);

            // ⚡ Punto clave: cuando el modal va a mostrarse, Bootstrap nos da el botón que lo abrió
            modalEl.addEventListener('show.bs.modal', function (ev) {
                const triggerBtn = ev.relatedTarget; // <-- el botón "Comentario" que se clickeó
                if (!triggerBtn) return;

                const id = triggerBtn.getAttribute('data-id');
                const idCategoria = triggerBtn.getAttribute('data-idcategoria');
                const adicionesIniciales = triggerBtn.getAttribute('data-adiciones') || '';

                // Precarga textarea y lista
                txtEl.value = adicionesIniciales;
                renderAdiciones(idCategoria, tokenize(adicionesIniciales));

                // Configurar Guardar para este id
                btnGuardar.onclick = function () {
                    const texto = joinCanon(tokenize(txtEl.value));
                    const payload = String(id) + '|' + texto;
                    if (typeof __doPostBack === 'function') {
                        __doPostBack('NotasDetalle', payload);
                    } else {
                        console.error('__doPostBack no disponible');
                    }
                };

                // Limpiar con confirmación moderna
                btnLimpiar.onclick = function () {
                    if (!txtEl.value.trim()) return;

                    AlertModerno.Confirm(
                        "¿Deseas limpiar el comentario/adiciones?",
                        "Esta acción eliminará todo el texto actual.",
                        function (ok) {
                            if (ok) {
                                txtEl.value = '';
                                syncFromTextarea();
                                AlertModerno.Success(null, "¡Listo!", "Comentario limpiado correctamente.", false, 800);
                            }
                        }
                    );
                };

            });

        })();
    </script>




    <script>
        (function () {
            if (window.__initModalAlias) return;
            window.__initModalAlias = true;

            function $id(id) { return document.getElementById(id); }

            // Instancia del modal
            var modalEl = $id('modalAlias');
            var modalAlias = null;
            function ensureModal() {
                if (!modalAlias && window.bootstrap && bootstrap.Modal) {
                    modalAlias = new bootstrap.Modal(modalEl);
                }
                return modalAlias;
            }

            // Delegación de eventos: cualquier .btn-alias abre el modal
            document.addEventListener('click', function (ev) {
                var btn = ev.target.closest('.btn-alias');
                if (!btn) return;

                // Evita que el click “pase” al LinkButton debajo
                ev.preventDefault();
                ev.stopPropagation();

                var id = btn.getAttribute('data-id') || '';
                // Si quieres pre-cargar el alias actual, puedes guardarlo en data-alias
                var aliasActual = btn.getAttribute('data-alias') || '';

                $id('<%= hfCuentaId.ClientID %>').value = id;
                $id('<%= txtAlias.ClientID %>').value = aliasActual;

                var m = ensureModal();
                if (m) m.show();
            }, true);
        })();
    </script>


   <script>
        document.getElementById('btnNuevoServicio').addEventListener('click', function (e) {
            e.preventDefault();
            __doPostBack('btnNuevoServicio', '');
        });
    </script>


    <script>
        document.addEventListener('DOMContentLoaded', function () {
            var idCuentaActiva = <%= (Models != null ? Models.IdCuentaActiva : 0) %>;
            var btnEliminar = document.getElementById('btnEliminarServicio');

            if (!btnEliminar) return;

            // Buscar aliasVenta correspondiente
            var botonServicio = document.querySelector('[data-id="' + idCuentaActiva + '"]');
            var aliasVenta = botonServicio ? botonServicio.getAttribute('data-alias') : "este servicio";

            btnEliminar.addEventListener('click', function (e) {
                e.preventDefault();

                Swal.fire({
                    title: '¿Eliminar servicio?',
                    html: `<b>${aliasVenta}</b><br>Esta acción no se puede deshacer.`,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#d33',
                    cancelButtonColor: '#3085d6',
                    confirmButtonText: 'Sí, eliminar',
                    cancelButtonText: 'Cancelar'
                }).then((result) => {
                    if (result.isConfirmed) {
                        __doPostBack('btnEliminarServicio', '');
                    }
                });
            });
        });
    </script>



    <script>
        document.addEventListener('DOMContentLoaded', function () {
            const contenedor = document.querySelector('.lista-mesas');
            if (!contenedor) {
                /*Swal.fire('Error', 'No se encontró el contenedor .lista-mesas', 'error');*/
                return;
            }


            contenedor.addEventListener('click', function (e) {
                const boton = e.target.closest('.btnMesa');
                if (!boton || !contenedor.contains(boton)) return;

                e.preventDefault();

                const idMesa = boton.dataset.id;
                const nombreMesa = boton.dataset.name;

                if (!idMesa) {
                    Swal.fire('Error', 'El botón no tiene el atributo data-id.', 'error');
                    return;
                }

                if (typeof __doPostBack === 'function') {
                    __doPostBack('btnMesa', idMesa)
                } else {
                    Swal.fire('Error', '__doPostBack no está disponible en esta página.', 'error');
                    return;
                }
            });
        });
    </script>

    <script>
        document.addEventListener('DOMContentLoaded', function () {
            // 1️⃣ Referencias principales
            const btnLiberar = document.getElementById('btnLiberarMesa');
            const contenedor = document.querySelector('.lista-mesas'); // clase, no id

            // 2️⃣ Validaciones básicas
            if (!btnLiberar) {
                console.error('No se encontró el botón #btnLiberarMesa');
                return;
            }
            if (!contenedor) {
                console.error('No se encontró el contenedor .lista-mesas');
                return;
            }

            // 3️⃣ Escuchar el clic en el botón principal
            btnLiberar.addEventListener('click', function (e) {
                e.preventDefault();

                // 4️⃣ Tomar el data-idmesa
                const idMesa = btnLiberar.dataset.idmesa;
                if (!idMesa) {
                    Swal.fire('Error', 'El botón no tiene el atributo data-idmesa.', 'error');
                    return;
                }

                // 5️⃣ Buscar dentro del contenedor el botón con el mismo data-id
                const botonCoincidente = contenedor.querySelector('[data-id="' + idMesa + '"]');
                if (!botonCoincidente) {
                    Swal.fire('Error', `No se encontró ninguna mesa con id ${idMesa}.`, 'error');
                    return;
                }

                // 6️⃣ Tomar el nombre de la mesa
                const nombreMesa = botonCoincidente.dataset.name || 'esta mesa';

                // 7️⃣ Mostrar confirmación con SweetAlert2
                Swal.fire({
                    title: '¿Liberar mesa?',
                    html: `<b>${nombreMesa}</b><br>Esta acción no se puede deshacer.`,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Sí, liberar',
                    cancelButtonText: 'Cancelar',
                    confirmButtonColor: '#d33',
                    cancelButtonColor: '#3085d6',
                    allowOutsideClick: false,
                    allowEscapeKey: true
                }).then((result) => {
                    if (!result.isConfirmed) return;

                    const payload = idMesa + '|' + nombreMesa;

                    // 8️⃣ Llamar al postback con el idMesa
                    if (typeof __doPostBack === 'function') {
                        __doPostBack('btnLiberarMesa', payload);
                    } else {
                        console.error('__doPostBack no está disponible en esta página.');
                    }
                });
            });
        });
    </script>



    <script>
        document.addEventListener('DOMContentLoaded', function () {
            // Selecciona todos los enlaces de cuenta
            const cuentas = document.querySelectorAll('.cuenta-card');

            cuentas.forEach(cuenta => {
                cuenta.addEventListener('click', function (e) {
                    e.preventDefault();

                    // Obtiene el id del atributo data-id
                    const idCuenta = this.dataset.id;

                    // Llama al postback con el id
                    __doPostBack('btnCuentaCliente', idCuenta);
                });
            });
        });
    </script>

    <script>
        (function () {
            if (window.__initEditarPropina) return;
            window.__initEditarPropina = true;

            // ---- Elementos base ----
            const btnTrigger = document.getElementById('btnEditarPropina');
            const hdnPayload = document.getElementById('hdnEditarPropina');
            const modalEl = document.getElementById('modalPropina');

            if (!btnTrigger || !hdnPayload || !modalEl) {
                console.error('Faltan elementos: btnEditarPropina, hdnEditarPropina o modalPropina.');
                return;
            }

            let bsModal;
            function ensureModal() {
                if (!bsModal && window.bootstrap?.Modal) {
                    bsModal = new bootstrap.Modal(modalEl);
                }
                return bsModal;
            }

            // ---- Constantes y helpers ----
            const MAX_PERCENT = 15; // % máximo permitido
            const nfCOP = new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 });

            function formatCOP(n) { return nfCOP.format(isFinite(n) ? n : 0); }
            function clamp(n, min, max) { return Math.min(Math.max(n, min), max); }
            function roundTo100(n) { return Math.round(n / 100) * 100; } // múltiplo de 100

            // Parser robusto: quita miles y normaliza decimal
            function parseNumber(str) {
                if (typeof str === 'number') return str;
                if (!str) return 0;
                str = String(str).trim();
                str = str.replace(/[^\d.,-]/g, '');

                if (str.includes('.') && str.includes(',')) {
                    str = str.replace(/\./g, '').replace(',', '.');
                } else if (str.includes(',')) {
                    str = str.replace(/\./g, '').replace(',', '.');
                } else {
                    str = str.replace(/\./g, '');
                }

                const n = parseFloat(str);
                return Number.isFinite(n) ? n : 0;
            }

            // ---- Controles del modal ----
            const txtSubtotal = document.getElementById('txtSubtotal');
            const txtPorcentaje = document.getElementById('txtPorcentaje');
            const txtPropina = document.getElementById('txtPropina');
            const ayuda = document.getElementById('ayudaPropina');
            const btnGuardar = document.getElementById('btnGuardarPropina');
            const btnQuitar = document.getElementById('btnQuitarPropina');

            // Estado
            let subtotal = 0, porcentaje = 0, propina = 0, idventa = 0, idcuenta = 0;
            let lastEdited = null; // 'percent' | 'value'

            function renderHelp() {
                ayuda && (ayuda.textContent = `Esto equivale a ${porcentaje}% sobre ${formatCOP(subtotal)}.`);
            }

            // Sincronización: % -> valor
            function syncFromPercent() {
                porcentaje = Math.round(clamp(parseNumber(txtPorcentaje.value), 0, MAX_PERCENT));
                const calc = (subtotal * porcentaje) / 100;
                propina = Math.min(roundTo100(calc), subtotal);
                txtPorcentaje.value = porcentaje;
                txtPropina.value = formatCOP(propina);
                lastEdited = 'percent';
                renderHelp();
            }

            // Sincronización: valor -> %
            function syncFromValue() {
                let raw = parseNumber(txtPropina.value);
                raw = Math.max(0, raw);
                propina = Math.min(roundTo100(raw), subtotal);
                porcentaje = subtotal > 0 ? Math.round((propina / subtotal) * 100) : 0;
                porcentaje = clamp(porcentaje, 0, MAX_PERCENT);
                txtPorcentaje.value = porcentaje;
                txtPropina.value = formatCOP(propina);
                lastEdited = 'value';
                renderHelp();
            }

            // Eventos
            txtPorcentaje?.addEventListener('input', () => {
                porcentaje = Math.round(clamp(parseNumber(txtPorcentaje.value), 0, MAX_PERCENT));
                const calc = (subtotal * porcentaje) / 100;
                propina = Math.min(roundTo100(calc), subtotal);
                txtPropina.value = formatCOP(propina);
                lastEdited = 'percent';
                renderHelp();
            });

            txtPropina?.addEventListener('input', () => {
                let raw = Math.max(0, parseNumber(txtPropina.value));
                let pTmp = subtotal > 0 ? Math.round((raw / subtotal) * 100) : 0;
                pTmp = clamp(pTmp, 0, MAX_PERCENT);
                txtPorcentaje.value = pTmp;
                lastEdited = 'value';
                renderHelp();
            });

            txtPorcentaje?.addEventListener('blur', syncFromPercent);
            txtPropina?.addEventListener('blur', syncFromValue);

            // Atajos de porcentaje
            document.querySelectorAll('.quick-tip').forEach(btn => {
                btn.addEventListener('click', () => {
                    const p = parseNumber(btn.dataset.tip);
                    txtPorcentaje.value = p;
                    syncFromPercent();
                });
            });

            // Quitar propina
            btnQuitar?.addEventListener('click', () => {
                txtPorcentaje.value = '0';
                txtPropina.value = formatCOP(0);
                syncFromPercent();
            });

            // Abrir modal
            btnTrigger.addEventListener('click', (e) => {
                e.preventDefault();

                subtotal = parseNumber(btnTrigger.dataset.subtotal);
                porcentaje = Math.round(clamp(parseNumber(btnTrigger.dataset.porcentaje), 0, MAX_PERCENT));
                propina = Math.max(0, parseNumber(btnTrigger.dataset.propina));
                idventa = parseInt(btnTrigger.dataset.idventa || '0', 10) || 0;
                idcuenta = parseInt(btnTrigger.dataset.idcuenta || '0', 10) || 0;

                if (propina > 0) {
                    porcentaje = subtotal > 0 ? Math.round(clamp((propina / subtotal) * 100, 0, MAX_PERCENT)) : 0;
                } else {
                    const calc = (subtotal * porcentaje) / 100;
                    propina = Math.min(roundTo100(calc), subtotal);
                }

                txtSubtotal.value = formatCOP(subtotal);
                txtPorcentaje.value = porcentaje;
                txtPropina.value = formatCOP(propina);
                lastEdited = null;
                renderHelp();

                ensureModal()?.show();
            });

            // Guardar
            btnGuardar.addEventListener('click', () => {
                if (lastEdited === 'value') syncFromValue();
                else syncFromPercent();

                const payload = {
                    porcentaje: porcentaje,  // entero sin decimales
                    propina: propina,        // entero COP múltiplo de 100
                    idventa: idventa,
                    idcuenta: idcuenta
                };

                hdnPayload.value = JSON.stringify(payload);
                if (typeof __doPostBack === 'function') {
                    __doPostBack('btnEditarPropina', '');
                }

                btnGuardar.disabled = true;
                setTimeout(() => { btnGuardar.disabled = false; }, 2000);
            });

        })();
    </script>




    <script>
        document.addEventListener("DOMContentLoaded", function () {
            const buscador = document.getElementById("buscador-productos");

            // Evento más confiable para móviles
            buscador.addEventListener("keydown", function (e) {
                // Detecta tecla Enter (tanto física como virtual)
                if (e.key === "Enter" || e.keyCode === 13) {
                    e.preventDefault(); // Evita tabular
                    const valor = buscador.value.trim();
                    if (valor !== "") {
                        // Llamada al postback
                        __doPostBack("btnCuscarProducto", valor);
                    }
                }
            });

            // Botón de limpiar (opcional)
            document.getElementById("limpiar-buscador").addEventListener("click", function () {
                buscador.value = "";
                buscador.focus();
            });
        });
    </script>



    <script type="text/javascript">
        function btnDomicilio_click(btn) {

            // Leer los atributos del botón
            var idMesa = btn.getAttribute('data-idmesa') || "0";
            var idServicio = btn.getAttribute('data-idservicio') || "0";

            // Armar argumento: idMesa|idServicio
            var arg = idMesa + "|" + idServicio;

            // Llamar al postback
            __doPostBack('btnDomicilio', arg);
        }
    </script>


    <% if (Models.AbrirModalDomicilio == true)
        { %>
    <script type="text/javascript">
        document.addEventListener("DOMContentLoaded", function () {
            var modalEl = document.getElementById('modalDomicilio');
            if (modalEl && window.bootstrap && bootstrap.Modal) {
                var modal = new bootstrap.Modal(modalEl, { backdrop: 'static' });
                modal.show();
                cargarTablaDomicilios();

                // foco automático
                setTimeout(function () {
                    var txt = document.getElementById('txtBuscarCelular');
                    if (txt) txt.focus();
                }, 300);
            }
        });
    </script>
    <% } %>




    <script type="text/javascript">
        // ClientID real del hidden (runat="server")
        var hdIdClienteDomicilioId = '<%= hdIdClienteDomicilio.ClientID %>';

        // Cargar tabla completa o filtrada
        function cargarTablaDomicilios(filtro) {
            const lista = window.ListaClientesDomicilio || [];
            const tbody = document.querySelector('#tblDomicilios tbody');
            if (!tbody) return 0;

            tbody.innerHTML = "";

            const valor = (filtro || "").trim();
            const v = valor.toUpperCase();

            const filtrados = valor
                ? lista.filter(item => {
                    const tel = (item.celularCliente || "").toString();
                    const nom = (item.nombreCliente || "").toString().toUpperCase();
                    return tel.startsWith(valor) || nom.includes(v);
                })
                : lista;

            filtrados.forEach(item => {
                const tr = document.createElement("tr");

                tr.innerHTML = `
                <td>${item.celularCliente || ""}</td>
                <td>${item.nombreCliente || ""}</td>
                <td>${item.direccionCliente || ""}</td>
            `;

                tr.addEventListener("click", function () {
                    document.getElementById("txtTelefono").value = item.celularCliente || "";
                    document.getElementById("txtNombreCliente").value = item.nombreCliente || "";
                    document.getElementById("txtDireccion").value = item.direccionCliente || "";

                    // Guardar ID en el hidden
                    var hd = document.getElementById(hdIdClienteDomicilioId);
                    if (hd) {
                        hd.value = item.id || "";
                        console.log("ID cliente seleccionado:", hd.value);
                    }
                });

                tbody.appendChild(tr);
            });

            return filtrados.length;
        }
    </script>





    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var txtBuscar = document.getElementById('txtBuscarCelular');
            var tbody = document.querySelector('#tblDomicilios tbody');

            if (!txtBuscar || !tbody) return;

            // Filtro en tiempo real
            txtBuscar.addEventListener('input', function () {
                var filtro = (this.value || '').trim().toUpperCase();
                var filas = tbody.querySelectorAll('tr');

                filas.forEach(function (tr) {
                    // Tomamos teléfono y nombre desde las celdas 0 y 1
                    var tel = (tr.cells[0]?.textContent || '').toUpperCase();
                    var nom = (tr.cells[1]?.textContent || '').toUpperCase();

                    // Lógica de filtro:
                    // - Teléfono que comience por lo escrito
                    // - Ó nombre que contenga lo escrito
                    var coincide =
                        !filtro ||
                        tel.indexOf(filtro) === 0 ||
                        nom.indexOf(filtro) !== -1;

                    tr.style.display = coincide ? '' : 'none';
                });
            });
        });
    </script>


    <script>
        // 🔹 Handler global: si SweetAlert está visible, cualquier ENTER solo cierra el alert
        document.addEventListener('keydown', function (e) {
            if (typeof Swal !== 'undefined' && Swal.isVisible()) {
                if (e.key === 'Enter' || e.keyCode === 13) {
                    e.preventDefault();
                    e.stopPropagation();

                    // Simular click en el botón de confirmar
                    const btn = Swal.getConfirmButton && Swal.getConfirmButton();
                    if (btn) {
                        btn.click();
                    }
                }
            }
        }, true); // 👈 en captura para interceptar lo antes posible

        // EVENTO: Presionar ENTER en el buscador
        document.addEventListener("DOMContentLoaded", function () {
            const txtBuscar = document.getElementById("txtBuscarCelular");
            const txtTelefono = document.getElementById("txtTelefono");
            const hdIdCliente = document.getElementById("hdIdClienteDomicilio");

            if (!txtBuscar) return;

            txtBuscar.addEventListener("keydown", function (e) {

                // ⛔ Si hay un SweetAlert abierto, no procesar nada aquí
                if (typeof Swal !== 'undefined' && Swal.isVisible()) {
                    return;
                }

                if (e.key === "Enter" || e.keyCode === 13) {
                    e.preventDefault();

                    const filtro = txtBuscar.value.trim();

                    // 1️⃣ Validar longitud = 10
                    if (filtro.length !== 10) {
                        Swal.fire({
                            icon: 'warning',
                            title: 'Número inválido',
                            text: 'El número debe tener exactamente 10 dígitos.',
                            confirmButtonText: 'Aceptar',
                            confirmButtonColor: '#0d6efd',
                            allowOutsideClick: false,
                            allowEscapeKey: false,
                            focusConfirm: true,
                            didOpen: () => {
                                // Aseguramos que el botón Aceptar tenga el foco
                                const btn = Swal.getConfirmButton();
                                if (btn) btn.focus();
                            }
                        });
                        return;
                    }

                    // 2️⃣ Filtrar tabla
                    let cantidad = cargarTablaDomicilios(filtro);
                    if (typeof cantidad !== "number") cantidad = 0;

                    // 3️⃣ Si NO hay registros → pasar al campo Teléfono y preparar creación
                    if (cantidad === 0) {
                        if (txtTelefono) txtTelefono.value = filtro;
                        if (hdIdCliente) hdIdCliente.value = "";

                        const txtNombre = document.getElementById("txtNombreCliente");
                        if (txtNombre) txtNombre.focus();
                    }
                }
            });
        });
    </script>






    <script>
        document.addEventListener("DOMContentLoaded", function () {
            const txtBuscar = document.getElementById("txtBuscarCelular");

            if (txtBuscar) {
                txtBuscar.addEventListener("input", function () {

                    // Quitar todo lo que no sea número
                    this.value = this.value.replace(/\D/g, '');

                    // Limitar a 10 dígitos
                    if (this.value.length > 10) {
                        this.value = this.value.substring(0, 10);
                    }
                });
            }

            const txtTelefono = document.getElementById("txtTelefono");
            if (txtTelefono) {
                txtTelefono.addEventListener("input", function () {

                    this.value = this.value.replace(/\D/g, '');

                    if (this.value.length > 10) {
                        this.value = this.value.substring(0, 10);
                    }
                });
            }
        });
    </script>


    <%-- boton Crear / Actualizar del modal domicilio --%>
    <script type="text/javascript">
        document.addEventListener("DOMContentLoaded", function () {

            var hdIdCliente = document.getElementById(hdIdClienteDomicilioId);
            var btnCrear = document.getElementById('btnCrearDomicilio');

            if (btnCrear) {
                btnCrear.addEventListener('click', function (e) {
                    e.preventDefault();

                    var id = hdIdCliente ? (hdIdCliente.value || "").trim() : "";

                    var tel = (document.getElementById('txtTelefono').value || '').trim();
                    var nom = (document.getElementById('txtNombreCliente').value || '').trim();
                    var dir = (document.getElementById('txtDireccion').value || '').trim();

                    // Validaciones básicas
                    if (!tel || !nom || !dir) {
                        if (window.Swal) {
                            Swal.fire({
                                icon: 'warning',
                                title: 'Campos incompletos',
                                text: 'Debe llenar Teléfono, Nombre y Dirección.',
                                confirmButtonText: 'Aceptar',
                                allowOutsideClick: false,
                                allowEscapeKey: false
                            });
                        } else {
                            alert('Debe llenar Teléfono, Nombre y Dirección.');
                        }
                        return;
                    }

                    // Armar argumento: id|tel|nom|dir
                    var arg = id + '|' + tel + '|' + nom + '|' + dir;

                    console.log("Enviando a btnCrearActualizarClienteDomicilio:", arg);

                    if (typeof __doPostBack === 'function') {
                        __doPostBack('btnCrearActualizarClienteDomicilio', arg);
                    } else {
                        console.error('__doPostBack no está definido');
                    }
                });
            }

        });
    </script>


    <%-- boton Seleccionar Cliente del modal domicilio --%>
    <script type="text/javascript">
        document.addEventListener("DOMContentLoaded", function () {

            const btnSel = document.getElementById("btnSeleccionarDomicilio");
            const hdIdCliente = document.getElementById(hdIdClienteDomicilioId);

            if (!btnSel) return;

            btnSel.addEventListener("click", function (e) {
                e.preventDefault();

                const id = hdIdCliente ? (hdIdCliente.value || "").trim() : "";
                const tel = (document.getElementById("txtTelefono").value || "").trim();
                const nom = (document.getElementById("txtNombreCliente").value || "").trim();
                const dir = (document.getElementById("txtDireccion").value || "").trim();

                // 1️⃣ Validar que el usuario haya seleccionado un cliente
                if (!id) {
                    Swal.fire({
                        icon: "warning",
                        title: "Seleccione un cliente",
                        text: "Debe seleccionar un cliente de la lista o crearlo antes.",
                        confirmButtonText: "Aceptar",
                        confirmButtonColor: "#198754",
                        allowOutsideClick: false,
                        allowEscapeKey: false,
                        focusConfirm: true
                    });
                    return;
                }

                // 2️⃣ Argumento para enviar al servidor
                const arg = `${id}|${tel}|${nom}|${dir}`;
                console.log("PostBack Seleccionar Cliente:", arg);

                // 3️⃣ Llamar al case en el servidor
                if (typeof __doPostBack === "function") {
                    __doPostBack("btnSeleccionarClienteDomicilio", arg);
                } else {
                    console.error("__doPostBack no está definido");
                }

                // ❗ No cerrar el modal desde aquí
            });

        });
    </script>



<script type="text/javascript">
    document.addEventListener('DOMContentLoaded', function () {
        var btn = document.getElementById('btnCerrarCaja');
        if (!btn) return;

        btn.addEventListener('click', function (e) {
            e.preventDefault();

            Swal.fire({
                title: 'Cerrar caja',
                text: '¿Está seguro de cerrar la caja?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sí, cerrar',
                cancelButtonText: 'Cancelar',
                reverseButtons: true,
                focusCancel: true,
                confirmButtonColor: '#dc3545', // rojo Bootstrap
                cancelButtonColor: '#6c757d'
            }).then((result) => {
                if (result.isConfirmed) {
                    // 🔥 Dispara tu switch case "btnCerrarCaja"
                    __doPostBack('btnCerrarCaja', '');
                }
            });
        });
    });
</script>


    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {

            const input = document.getElementById('buscador-productos');
            if (!input) return;

            input.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    e.stopPropagation();

                    const valor = (input.value || '').trim();
                    if (!valor) return;

                    // 🔥 Llama tu switch: case "BuscarCodigoProducto"
                    __doPostBack('BuscarCodigoProducto', valor);
                }
            });

        });
    </script>


<script>
    function EnfocarBuscador() {
        const input = document.getElementById('buscador-productos');
        if (!input) return;

        setTimeout(() => {
            input.focus();
            input.select();
        }, 50);
    }

    document.addEventListener('DOMContentLoaded', function () {
        EnfocarBuscador();
    });
</script>


</asp:Content>
