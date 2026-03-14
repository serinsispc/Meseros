<%@ Page Title="" Language="C#" MasterPageFile="~/CajaMaster.Master" AutoEventWireup="true" CodeBehind="caja.aspx.cs" Inherits="WebApplication.caja" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        :root {
            /* Ajusta a tu azul corporativo Serinsis PC */
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

        .app-loading {
            position: fixed;
            inset: 0;
            z-index: 999999;
            /*display: none;  se controla por JS */
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

        @keyframes appSpin {
            to {
                transform: rotate(360deg);
            }
        }

            <div class="app-loading-title">Cargando...</div>
            font-weight: 800;
            color: #0f172a;
            font-size: 1.05rem;
        }

            <div class="app-loading-sub">Estamos preparando tu informaci&oacute;n</div>
            color: rgba(15,23,42,.70);
            font-size: .92rem;
            margin-top: 4px;
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

    <asp:Button ID="btnBridge" runat="server"
        ClientIDMode="Static"
        Style="display: none;"
        OnClick="Evento_Click" />


    <!-- ? OJO: tu CSS usa .app-shell -->
    <main class="container-fluid app-shell">

        <div class="row g-1 h-100">

            <!-- ================= IZQUIERDA ================= -->
            <section class="col-12 col-xl-8 d-flex flex-column">

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
                            <button type="button"
                                class="btn-top btn-logout<%= PuedeEliminarServicioActivo() ? string.Empty : " btn-eliminar-servicio-disabled" %>"
                                onclick="<%= PuedeEliminarServicioActivo() ? "return ConfirmarEliminarServicio(this)" : "return false;" %>"
                                <%= PuedeEliminarServicioActivo() ? string.Empty : "disabled=\"disabled\" title=\"No se puede eliminar porque el servicio tiene productos cargados.\"" %>>
                                <i class="bi bi-trash3-fill"></i>
                                Eliminar servicio
                            </button>

                            <button type="button" class="btn-top btn-domicilio">
                                <i class="bi bi-house-door-fill"></i>
                                Domicilio
                            </button>

                            <button type="button" class="btn-top btn-ventas">
                                <i class="bi bi-graph-up-arrow"></i>
                                Ventas
                            </button>

                            <button type="button" class="btn-top btn-cerrar-caja">
                                <i class="bi bi-lock-fill"></i>
                                Cerrar caja
                            </button>

                            <button type="button" class="btn-top btn-logout">
                                <i class="bi bi-box-arrow-right"></i>
                                Cerrar sesi&oacute;n
                            </button>

                        </div>

                    </div>

                </div>

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

                                                                    <button type="button" class="prod-btn prod-cart" aria-label="Agregar">
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
            </section>

            <!-- ================= DERECHA ================= -->
            <aside class="col-12 col-xl-4 d-flex">
                <div class="panel w-100">
                    <div class="row g-0 h-100 aside-stack">

                        <div class="col-12">
                            <div class="box h-precios p-0 precios-box">

                                <div class="precios-resumen">

                                    <div class="precio-row">
                                        <div class="precio-label">SUBTOTAL:</div>
                                        <div class="precio-value">$ 1.501.150</div>
                                    </div>

                                    <div class="precio-row">
                                        <div class="precio-label">IMPUESTOS (8%)</div>
                                        <div class="precio-value">$ 0</div>
                                    </div>

                                    <div class="precio-row precio-total">
                                        <div class="precio-label">TOTAL 1:</div>
                                        <div class="precio-value">$ 1.501.150</div>
                                    </div>

                                    <div class="precio-row precio-servicio">
                                        <div class="precio-label">SERVICIO (0.10%)</div>
                                        <div class="precio-servicio-acciones">
                                            <button type="button" class="btn-servicio-editar">Editar</button>
                                            <div class="precio-value">$ 150.115</div>
                                        </div>
                                    </div>

                                    <div class="precio-row precio-total2">
                                        <div class="precio-label">TOTAL 2:</div>
                                        <div class="precio-value">$ 1.651.265</div>
                                    </div>

                                </div>

                                <div class="precios-acciones">
                                    <button type="button" class="accion-btn accion-comandar">
                                        <i class="bi bi-send-fill me-1"></i>Comandar
                                    </button>

                                    <button type="button" class="accion-btn accion-cuenta">
                                        <i class="bi bi-chat-square-text-fill me-1"></i>Solicitar<br />
                                        Cuenta
                                    </button>

                                    <button type="button" class="accion-btn accion-cobrar">
                                        <i class="bi bi-cash-coin me-1"></i>Cobrar
                                    </button>
                                </div>

                            </div>
                        </div>



                        <div class="col-12">
                            <div class="box h-nueva-cuenta">

                                <!-- FILA SUPERIOR -->
                                <div class="cuentas-header col-12">

                                    <button type="button" class="btn-nueva-cuenta">
                                        <i class="bi bi-plus-lg me-2"></i>
                                        Nueva cuenta
                                    </button>

                                    <button type="button" class="btn-cuenta-general">
                                        <span>Cuenta General </span>
                                        <span class="cuenta-total">$ 1.651.265</span>
                                    </button>

                                </div>

                                <!-- LISTADO DE CUENTAS -->
                                <div class="cuentas-lista">
                                    <button type="button" class="cuenta-item">Mesa 01</button>
                                    <button type="button" class="cuenta-item">Mesa 02</button>
                                    <button type="button" class="cuenta-item">Cliente Juan</button>
                                    <button type="button" class="cuenta-item">Servicio</button>
                                    <button type="button" class="cuenta-item">Domicilio 03</button>
                                    <button type="button" class="cuenta-item">Mesa 10</button>
                                </div>

                            </div>
                        </div>


                        <div class="col-12">
                            <div class="box h-lista">

                                <div class="row lista-productos">

                                    <div class="col-6 col-xl-12 item-col">
                                        <div class="producto-item-detalle">

                                            <!-- TOP -->
                                            <div class="prod-top-detalle">
                                                <div class="prod-nombre">CAMARA IP V380</div>
                                                <div class="prod-precio">$ 101.150</div>
                                            </div>

                                            <!-- MID -->
                                            <div class="prod-mid-detalle">
                                                <div class="prod-cantidad">
                                                    <button type="button" class="qty-btn" aria-label="Disminuir">-</button>
                                                    <input type="text" class="qty-input" value="1" inputmode="numeric" />
                                                    <button type="button" class="qty-btn" aria-label="Aumentar">+</button>
                                                </div>

                                                <button type="button" class="save-btn" aria-label="Guardar">
                                                    <i class="bi bi-floppy2-fill"></i>
                                                </button>
                                            </div>

                                            <!-- ACTIONS -->
                                            <div class="prod-actions-detalle">
                                                <button type="button" class="act-btn" aria-label="Chat"><i class="bi bi-chat"></i></button>
                                                <button type="button" class="act-btn" aria-label="Link"><i class="bi bi-link-45deg"></i></button>
                                                <button type="button" class="act-btn act-danger" aria-label="Eliminar"><i class="bi bi-trash"></i></button>
                                                <button type="button" class="act-btn" aria-label="Cortar"><i class="bi bi-scissors"></i></button>
                                                <button type="button" class="act-btn" aria-label="Descuento"><i class="bi bi-cash-coin"></i></button>
                                                <button type="button" class="act-btn" aria-label="Editar"><i class="bi bi-pencil-square"></i></button>
                                            </div>

                                            <!-- BOTTOM -->
                                            <div class="prod-bottom">
                                                <button type="button" class="nota-btn" aria-label="Notas">
                                                    <i class="bi bi-journal-text me-2"></i><span>--</span>
                                                </button>

                                                <div class="prod-total">$ 101.150</div>
                                            </div>

                                        </div>
                                    </div>

                                    <div class="col-6 col-xl-12 item-col">
                                        <div class="producto-item-detalle">

                                            <!-- TOP -->
                                            <div class="prod-top-detalle">
                                                <div class="prod-nombre">CAMARA IP V380</div>
                                                <div class="prod-precio">$ 101.150</div>
                                            </div>

                                            <!-- MID -->
                                            <div class="prod-mid-detalle">
                                                <div class="prod-cantidad">
                                                    <button type="button" class="qty-btn" aria-label="Disminuir">-</button>
                                                    <input type="text" class="qty-input" value="1" inputmode="numeric" />
                                                    <button type="button" class="qty-btn" aria-label="Aumentar">+</button>
                                                </div>

                                                <button type="button" class="save-btn" aria-label="Guardar">
                                                    <i class="bi bi-floppy2-fill"></i>
                                                </button>
                                            </div>

                                            <!-- ACTIONS -->
                                            <div class="prod-actions-detalle">
                                                <button type="button" class="act-btn" aria-label="Chat"><i class="bi bi-chat"></i></button>
                                                <button type="button" class="act-btn" aria-label="Link"><i class="bi bi-link-45deg"></i></button>
                                                <button type="button" class="act-btn act-danger" aria-label="Eliminar"><i class="bi bi-trash"></i></button>
                                                <button type="button" class="act-btn" aria-label="Cortar"><i class="bi bi-scissors"></i></button>
                                                <button type="button" class="act-btn" aria-label="Descuento"><i class="bi bi-cash-coin"></i></button>
                                                <button type="button" class="act-btn" aria-label="Editar"><i class="bi bi-pencil-square"></i></button>
                                            </div>

                                            <!-- BOTTOM -->
                                            <div class="prod-bottom">
                                                <button type="button" class="nota-btn" aria-label="Notas">
                                                    <i class="bi bi-journal-text me-2"></i><span>--</span>
                                                </button>

                                                <div class="prod-total">$ 101.150</div>
                                            </div>

                                        </div>
                                    </div>


                                    <div class="col-6 col-xl-12 item-col">
                                        <div class="producto-item-detalle">

                                            <!-- TOP -->
                                            <div class="prod-top-detalle">
                                                <div class="prod-nombre">CAMARA IP V380</div>
                                                <div class="prod-precio">$ 101.150</div>
                                            </div>

                                            <!-- MID -->
                                            <div class="prod-mid-detalle">
                                                <div class="prod-cantidad">
                                                    <button type="button" class="qty-btn" aria-label="Disminuir">-</button>
                                                    <input type="text" class="qty-input" value="1" inputmode="numeric" />
                                                    <button type="button" class="qty-btn" aria-label="Aumentar">+</button>
                                                </div>

                                                <button type="button" class="save-btn" aria-label="Guardar">
                                                    <i class="bi bi-floppy2-fill"></i>
                                                </button>
                                            </div>

                                            <!-- ACTIONS -->
                                            <div class="prod-actions-detalle">
                                                <button type="button" class="act-btn" aria-label="Chat"><i class="bi bi-chat"></i></button>
                                                <button type="button" class="act-btn" aria-label="Link"><i class="bi bi-link-45deg"></i></button>
                                                <button type="button" class="act-btn act-danger" aria-label="Eliminar"><i class="bi bi-trash"></i></button>
                                                <button type="button" class="act-btn" aria-label="Cortar"><i class="bi bi-scissors"></i></button>
                                                <button type="button" class="act-btn" aria-label="Descuento"><i class="bi bi-cash-coin"></i></button>
                                                <button type="button" class="act-btn" aria-label="Editar"><i class="bi bi-pencil-square"></i></button>
                                            </div>

                                            <!-- BOTTOM -->
                                            <div class="prod-bottom">
                                                <button type="button" class="nota-btn" aria-label="Notas">
                                                    <i class="bi bi-journal-text me-2"></i><span>--</span>
                                                </button>

                                                <div class="prod-total">$ 101.150</div>
                                            </div>

                                        </div>
                                    </div>

                                    <div class="col-6 col-xl-12 item-col">
                                        <div class="producto-item-detalle">

                                            <!-- TOP -->
                                            <div class="prod-top-detalle">
                                                <div class="prod-nombre">CAMARA IP V380</div>
                                                <div class="prod-precio">$ 101.150</div>
                                            </div>

                                            <!-- MID -->
                                            <div class="prod-mid-detalle">
                                                <div class="prod-cantidad">
                                                    <button type="button" class="qty-btn" aria-label="Disminuir">-</button>
                                                    <input type="text" class="qty-input" value="1" inputmode="numeric" />
                                                    <button type="button" class="qty-btn" aria-label="Aumentar">+</button>
                                                </div>

                                                <button type="button" class="save-btn" aria-label="Guardar">
                                                    <i class="bi bi-floppy2-fill"></i>
                                                </button>
                                            </div>

                                            <!-- ACTIONS -->
                                            <div class="prod-actions-detalle">
                                                <button type="button" class="act-btn" aria-label="Chat"><i class="bi bi-chat"></i></button>
                                                <button type="button" class="act-btn" aria-label="Link"><i class="bi bi-link-45deg"></i></button>
                                                <button type="button" class="act-btn act-danger" aria-label="Eliminar"><i class="bi bi-trash"></i></button>
                                                <button type="button" class="act-btn" aria-label="Cortar"><i class="bi bi-scissors"></i></button>
                                                <button type="button" class="act-btn" aria-label="Descuento"><i class="bi bi-cash-coin"></i></button>
                                                <button type="button" class="act-btn" aria-label="Editar"><i class="bi bi-pencil-square"></i></button>
                                            </div>

                                            <!-- BOTTOM -->
                                            <div class="prod-bottom">
                                                <button type="button" class="nota-btn" aria-label="Notas">
                                                    <i class="bi bi-journal-text me-2"></i><span>--</span>
                                                </button>

                                                <div class="prod-total">$ 101.150</div>
                                            </div>

                                        </div>
                                    </div>

                                    <div class="col-6 col-xl-12 item-col">
                                        <div class="producto-item-detalle">

                                            <!-- TOP -->
                                            <div class="prod-top-detalle">
                                                <div class="prod-nombre">CAMARA IP V380</div>
                                                <div class="prod-precio">$ 101.150</div>
                                            </div>

                                            <!-- MID -->
                                            <div class="prod-mid-detalle">
                                                <div class="prod-cantidad">
                                                    <button type="button" class="qty-btn" aria-label="Disminuir">-</button>
                                                    <input type="text" class="qty-input" value="1" inputmode="numeric" />
                                                    <button type="button" class="qty-btn" aria-label="Aumentar">+</button>
                                                </div>

                                                <button type="button" class="save-btn" aria-label="Guardar">
                                                    <i class="bi bi-floppy2-fill"></i>
                                                </button>
                                            </div>

                                            <!-- ACTIONS -->
                                            <div class="prod-actions-detalle">
                                                <button type="button" class="act-btn" aria-label="Chat"><i class="bi bi-chat"></i></button>
                                                <button type="button" class="act-btn" aria-label="Link"><i class="bi bi-link-45deg"></i></button>
                                                <button type="button" class="act-btn act-danger" aria-label="Eliminar"><i class="bi bi-trash"></i></button>
                                                <button type="button" class="act-btn" aria-label="Cortar"><i class="bi bi-scissors"></i></button>
                                                <button type="button" class="act-btn" aria-label="Descuento"><i class="bi bi-cash-coin"></i></button>
                                                <button type="button" class="act-btn" aria-label="Editar"><i class="bi bi-pencil-square"></i></button>
                                            </div>

                                            <!-- BOTTOM -->
                                            <div class="prod-bottom">
                                                <button type="button" class="nota-btn" aria-label="Notas">
                                                    <i class="bi bi-journal-text me-2"></i><span>--</span>
                                                </button>

                                                <div class="prod-total">$ 101.150</div>
                                            </div>

                                        </div>
                                    </div>

                                    <div class="col-6 col-xl-12 item-col">
                                        <div class="producto-item-detalle">

                                            <!-- TOP -->
                                            <div class="prod-top-detalle">
                                                <div class="prod-nombre">CAMARA IP V380</div>
                                                <div class="prod-precio">$ 101.150</div>
                                            </div>

                                            <!-- MID -->
                                            <div class="prod-mid-detalle">
                                                <div class="prod-cantidad">
                                                    <button type="button" class="qty-btn" aria-label="Disminuir">-</button>
                                                    <input type="text" class="qty-input" value="1" inputmode="numeric" />
                                                    <button type="button" class="qty-btn" aria-label="Aumentar">+</button>
                                                </div>

                                                <button type="button" class="save-btn" aria-label="Guardar">
                                                    <i class="bi bi-floppy2-fill"></i>
                                                </button>
                                            </div>

                                            <!-- ACTIONS -->
                                            <div class="prod-actions-detalle">
                                                <button type="button" class="act-btn" aria-label="Chat"><i class="bi bi-chat"></i></button>
                                                <button type="button" class="act-btn" aria-label="Link"><i class="bi bi-link-45deg"></i></button>
                                                <button type="button" class="act-btn act-danger" aria-label="Eliminar"><i class="bi bi-trash"></i></button>
                                                <button type="button" class="act-btn" aria-label="Cortar"><i class="bi bi-scissors"></i></button>
                                                <button type="button" class="act-btn" aria-label="Descuento"><i class="bi bi-cash-coin"></i></button>
                                                <button type="button" class="act-btn" aria-label="Editar"><i class="bi bi-pencil-square"></i></button>
                                            </div>

                                            <!-- BOTTOM -->
                                            <div class="prod-bottom">
                                                <button type="button" class="nota-btn" aria-label="Notas">
                                                    <i class="bi bi-journal-text me-2"></i><span>--</span>
                                                </button>

                                                <div class="prod-total">$ 101.150</div>
                                            </div>

                                        </div>
                                    </div>

                                </div>




                            </div>
                        </div>





                    </div>
                </div>
            </aside>

        </div>
    </main>


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
                    <!-- boton de guardar-client (no server control) -->
                    <button type="button" id="btnGuardarCuenta" class="btn btn-primary" onclick="guardarCuentaDirecto(this)">Guardar</button>
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

    <script src="Scripts/js/caja.js"></script>
    <script src="Scripts/js/app-modal.js"></script>
</asp:Content>



