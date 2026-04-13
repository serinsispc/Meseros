<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication._Default" Async="true" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <link href="<%: ResolveUrl("~/Content/css/login.css") %>" rel="stylesheet" />

    <style>
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
            border: 5px solid rgba(30, 136, 255, .22);
            border-top-color: #1e88ff;
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
            to {
                transform: rotate(360deg);
            }
        }
        .login-loading { position: fixed; inset: 0; display: none; align-items: center; justify-content: center; background: rgba(15, 23, 42, .45); backdrop-filter: blur(6px); z-index: 9999; }
        .login-loading-card { width: min(340px, calc(100% - 32px)); background: rgba(255,255,255,.96); border-radius: 22px; padding: 22px 18px; box-shadow: 0 24px 60px rgba(0,0,0,.18); text-align: center; }
        .login-loading-spinner { width: 56px; height: 56px; margin: 0 auto 12px; border-radius: 50%; border: 5px solid rgba(0, 80, 184, .15); border-top-color: #0050b8; animation: loginSpin .9s linear infinite; }
        @keyframes loginSpin { to { transform: rotate(360deg); } }
        .btn-login.is-busy, #btnAperturarBase.is-busy { opacity: .75; pointer-events: none; }
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
        .download-server-link {
            margin-top: 1rem;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: .55rem;
            width: 100%;
            padding: .9rem 1rem;
            border-radius: .95rem;
            text-decoration: none;
            font-weight: 700;
            color: #0b4dbb;
            background: linear-gradient(180deg, #f8fbff 0%, #edf5ff 100%);
            border: 1px solid rgba(11, 77, 187, 0.16);
            box-shadow: 0 12px 28px rgba(11, 77, 187, 0.08);
            transition: transform .18s ease, box-shadow .18s ease, border-color .18s ease;
        }
        .download-server-link:hover {
            color: #083b8f;
            transform: translateY(-1px);
            box-shadow: 0 16px 34px rgba(11, 77, 187, 0.14);
            border-color: rgba(11, 77, 187, 0.28);
        }
        .download-server-link i {
            font-size: 1.15rem;
            line-height: 1;
        }
        .download-server-link span {
            display: inline-block;
        }
        @media (max-width: 575.98px) { .user-card { font-size: .8rem; padding: .5rem .3rem; min-height: 70px; } }
    </style>

    <div id="appLoading" class="app-loading" aria-hidden="true">
        <div class="app-loading-card">
            <div class="app-spinner" aria-hidden="true"></div>
            <div class="app-loading-title">Procesando...</div>
            <div class="app-loading-sub">Estamos aperturando la caja. Por favor espera.</div>
        </div>
    </div>

    <div id="loginLoading" class="login-loading">
        <div class="login-loading-card">
            <div class="login-loading-spinner"></div>
            <div class="fw-bold">Procesando...</div>
            <div class="text-muted small mt-1">Estamos validando tus datos, por favor espera.</div>
        </div>
    </div>

    <main class="login-page">
        <div class="login-card">

            <div class="logo-badge">
                <asp:Image ID="imgLogoLogin" runat="server" AlternateText="Logo" Visible="false" />
            </div>

            <h2 class="login-title">Iniciar Sesión</h2>
            <div class="login-subtitle">Selecciona tu usuario y escribe tu clave</div>

            <div class="mb-3">
                <div class="users-grid-title">Usuarios disponibles</div>

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

            <a class="download-server-link"
               href="https://pos.serinsispc.com/ServerPrinter/Setup_ServerPrinter.exe"
               download="Setup_ServerPrinter.exe">
                <i class="bi bi-download"></i>
                <span>Descargar Servidor de Impresoras</span>
            </a>

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
                        OnClientClick="return prepararAperturaCaja(this);"
                        OnClick="btnAperturarBase_Click" />
                </div>
            </div>
        </div>
    </div>

        <script type="text/javascript">
        (function () {
            window.SerinsisLoading = {
                show: function () {
                    var el = document.getElementById('appLoading');
                    if (el) el.style.display = 'flex';
                },
                hide: function () {
                    var el = document.getElementById('appLoading');
                    if (el) el.style.display = 'none';
                }
            };

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

            document.addEventListener('click', function (e) {
                var btn = e.target.closest('.user-card');
                if (!btn) return;
                e.preventDefault();
                e.stopPropagation();
                setCelularYFoco(btn);
            }, true);

            var overlay = document.getElementById('loginLoading');
            if (overlay) {
                window.addEventListener('pageshow', function () { overlay.style.display = 'none'; });
                window.addEventListener('load', function () { overlay.style.display = 'none'; });
            }

            window.addEventListener('pageshow', function () { window.SerinsisLoading.hide(); });
            window.addEventListener('load', function () { window.SerinsisLoading.hide(); });
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

    <script type="text/javascript">
        function prepararAperturaCaja(btn) {
            if (!btn || btn.dataset.busy === '1') {
                return false;
            }

            btn.dataset.busy = '1';

            if (window.SerinsisLoading && typeof window.SerinsisLoading.show === 'function') {
                window.SerinsisLoading.show();
            }

            window.setTimeout(function () {
                try {
                    btn.disabled = true;
                    btn.classList.add('is-busy');
                } catch (e) {
                }
            }, 0);

            return true;
        }
    </script>



</asp:Content>




