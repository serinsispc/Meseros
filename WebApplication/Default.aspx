<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication._Default" Async="true" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <link href="<%: ResolveUrl("~/Content/css/login.css") %>" rel="stylesheet" />

    <style>
        .users-grid-title { font-size: .9rem; font-weight: 600; margin-bottom: .5rem; color: #0050b8; }
        .login-card { width: 100%; max-width: 420px; }
        @media (min-width: 768px) { .login-card { max-width: 720px; } }
        @media (min-width: 1200px) { .login-card { max-width: 960px; } }

        .user-card {
            border-radius: .75rem;
            padding: .6rem .4rem;
            display: flex; flex-direction: column; align-items: center; justify-content: center;
            min-height: 80px;
            font-size: .85rem;
            text-align: center;
            opacity: 1 !important; visibility: visible !important; filter: none !important;
            background: rgba(255, 255, 255, 0.98) !important;
            border: 1px solid rgba(0, 0, 0, 0.08) !important;
            color: #0050b8 !important;
            cursor: pointer;
            transition: .18s ease;
        }
        .user-card .user-avatar { font-size: 2rem; line-height: 1; }
        .user-card .user-avatar i { color: #0050b8 !important; }
        .user-card .user-name { margin-top: .2rem; word-break: break-word; color: #003366 !important; font-weight: 600; }
        .user-card.selected { border-color: #00c6ff !important; box-shadow: 0 0 0 2px rgba(0, 198, 255, 0.45); }
        .user-card:hover { transform: translateY(-2px); box-shadow: 0 6px 16px rgba(0, 0, 0, 0.15); }
        @media (max-width: 575.98px) { .user-card { font-size: .8rem; padding: .5rem .3rem; min-height: 70px; } }
    </style>

    <main class="login-page">
        <div class="login-card">

            <div class="logo-badge">
                <img src="<%:ResolveUrl($"~/Recursos/Imagenes/Logo/{Session["db"]}.png") %>" alt="Logo" />
            </div>

            <h2 class="login-title">Iniciar Sesión</h2>
            <div class="login-subtitle">Selecciona tu usuario y escribe tu clave</div>

            <div class="mb-3">
                <div class="users-grid-title">Meseros disponibles</div>

                <asp:Repeater ID="rptUsuarios" runat="server">
                    <HeaderTemplate>
                        <div class="row g-2 row-cols-2 row-cols-sm-3 row-cols-md-4 row-cols-lg-5 row-cols-xl-6">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <div class="col">
                            <button type="button"
                                    class="btn user-card w-100"
                                    data-celular='<%# Eval("telefonoVendedor") %>'>
                                <div class="user-avatar mb-1">
                                    <i class="bi bi-person-circle"></i>
                                </div>
                                <div class="user-name">
                                    <%# Eval("nombreVendedor") %>
                                </div>
                            </button>
                        </div>
                    </ItemTemplate>
                    <FooterTemplate>
                        </div>
                    </FooterTemplate>
                </asp:Repeater>
            </div>

            <div class="mb-3 input-icon d-none">
                <label for="txtCelular" class="form-label">Celular</label>
                <asp:TextBox ID="txtCelular" runat="server" CssClass="form-control with-icon"
                    TextMode="Number" MaxLength="10"
                    oninput="if(this.value.length>10) this.value=this.value.slice(0,10);" />
            </div>

            <div class="mb-3 input-icon">
                <label for="txtContrasena" class="form-label">Contraseña</label>
                <asp:TextBox ID="txtContrasena" runat="server" TextMode="Password" CssClass="form-control with-icon" />
            </div>

            <div class="d-grid">
                <asp:Button ID="btnIngresar" runat="server" CssClass="btn btn-login text-white"
                    Text="Ingresar" UseSubmitBehavior="true" OnClick="btnIngresar_Click" />
            </div>

            <div class="login-foot">
                © <%: DateTime.Now.Year %> · Todos los derechos reservados
            </div>

        </div>
    </main>

    <!-- ✅ MODAL APERTURA DE CAJA -->
    <div class="modal fade" id="modalBaseCaja" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-4">
                <div class="modal-header">
                    <h5 class="modal-title">Aperturar caja</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">
                    <div class="mb-2 text-muted">
                        No hay una base activa. Ingresa el valor de la base del día para aperturar.
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Valor base (COP)</label>
                        <asp:TextBox ID="txtValorBaseModal" runat="server" CssClass="form-control"
                            placeholder="Ej: 200000" />
                        <small class="text-muted">Puedes escribir 200000 o 200.000</small>
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancelar</button>

                    <asp:Button ID="btnAperturarBase" runat="server"
                        CssClass="btn btn-primary"
                        Text="Aperturar caja"
                        OnClick="btnAperturarBase_Click" />
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        (function () {
            function marcarSeleccion(btn) {
                var cards = document.querySelectorAll('.user-card');
                cards.forEach(function (c) { c.classList.remove('selected'); });
                btn.classList.add('selected');
            }

            function setCelularYFoco(btn) {
                var txtCelular = document.getElementById('<%= txtCelular.ClientID %>');
            var txtContrasena = document.getElementById('<%= txtContrasena.ClientID %>');

                var celular = btn.getAttribute('data-celular') || '';
                if (txtCelular) txtCelular.value = celular;

                marcarSeleccion(btn);

                if (txtContrasena) {
                    txtContrasena.value = '';
                    txtContrasena.focus();
                }
            }

            // ✅ Un solo listener para todo (no se pierde con postbacks/parciales)
            document.addEventListener('click', function (e) {
                var btn = e.target.closest('.user-card');
                if (!btn) return;

                // ✅ Evita submit/refresh accidental
                e.preventDefault();
                e.stopPropagation();

                setCelularYFoco(btn);
            }, true);
        })();
    </script>



<script type="text/javascript">
    function openBaseModal() {
        var el = document.getElementById('modalBaseCaja');
        if (!el) return;

        // Esperar hasta que bootstrap esté disponible (máx 2s)
        var intentos = 0;
        var timer = setInterval(function () {
            intentos++;

            if (window.bootstrap && bootstrap.Modal) {
                clearInterval(timer);

                var modal = bootstrap.Modal.getOrCreateInstance(el, { backdrop: 'static', keyboard: false });
                modal.show();

                setTimeout(function () {
                    var txt = document.getElementById('<%= txtValorBaseModal.ClientID %>');
                    if (txt) txt.focus();
                }, 250);
            }

            if (intentos >= 40) { // 40 * 50ms = 2000ms
                clearInterval(timer);
                console.error("Bootstrap aún no está listo para abrir el modal.");
            }
        }, 50);
    }
</script>



</asp:Content>
