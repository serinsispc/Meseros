using System.Data;
using System.Data.Common;
using ComandasVentas.Blazor.Models.Caja;
using ComandasVentas.Blazor.Models.Auth;
using Microsoft.Data.SqlClient;

namespace ComandasVentas.Blazor.Services.Caja;

public sealed class CajaDataService(IConfiguration configuration)
{
    private const string DefaultSqlServer = "www.serinsispc.com";
    private const string DefaultSqlUser = "emilianop";
    private const string DefaultSqlPassword = "Ser1ns1s@2020*";

    public async Task<CajaPageData> LoadCajaAsync(
        string db,
        VendedorInfo vendedor,
        DbConexionInfo? dbConexion,
        int? activeCuentaId = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildSqlConnectionString(db));
        await connection.OpenAsync(cancellationToken);

        var cuentas = await LoadCuentasAsync(connection, vendedor, dbConexion, cancellationToken);
        var cuentasMesasVista = await LoadCuentasMesasVistaAsync(connection, cancellationToken);
        var relacionesVentaMesa = await LoadRelacionesVentaMesaAsync(connection, cancellationToken);
        var zonas = await LoadZonasAsync(connection, cancellationToken);
        var mesas = await LoadMesasAsync(connection, cancellationToken);
        var categorias = await LoadCategoriasAsync(connection, cancellationToken);
        var productos = await LoadProductosAsync(connection, cancellationToken);

        var idCuentaActiva = activeCuentaId is > 0 && cuentas.Any(x => x.Id == activeCuentaId.Value)
            ? activeCuentaId.Value
            : cuentas.FirstOrDefault()?.Id ?? 0;
        VentaCajaData? ventaActiva = null;
        List<CuentaClienteCajaData> cuentasCliente = [];
        List<DetalleCajaData> detalles = [];

        if (idCuentaActiva > 0)
        {
            ventaActiva = await LoadVentaActivaAsync(connection, idCuentaActiva, cancellationToken);
            cuentasCliente = await LoadCuentasClienteAsync(connection, idCuentaActiva, cancellationToken);
            detalles = await LoadDetallesAsync(connection, idCuentaActiva, cancellationToken);
        }

        return new CajaPageData
        {
            Cuentas = cuentas,
            CuentasMesasVista = cuentasMesasVista,
            RelacionesVentaMesa = relacionesVentaMesa,
            Zonas = zonas,
            Mesas = mesas,
            Categorias = categorias,
            Productos = productos,
            VentaActiva = ventaActiva,
            CuentasCliente = cuentasCliente,
            Detalles = detalles
        };
    }

    private async Task<List<CuentaCajaData>> LoadCuentasAsync(
        SqlConnection connection,
        VendedorInfo vendedor,
        DbConexionInfo? dbConexion,
        CancellationToken cancellationToken)
    {
        var sql = vendedor.CajaMovil == 1 || dbConexion?.MeserosCompartidos == true
            ? """
              SELECT id, aliasVenta, eliminada, idvendedor, nombrevendedor, nombremesa, nombreCD
              FROM V_CuentasVenta
              WHERE numeroVenta = 0 AND eliminada = 0
              ORDER BY id
              """
            : """
              SELECT id, aliasVenta, eliminada, idvendedor, nombrevendedor, nombremesa, nombreCD
              FROM V_CuentasVenta
              WHERE idvendedor = @idVendedor AND numeroVenta = 0 AND eliminada = 0
              ORDER BY id
              """;

        await using var command = new SqlCommand(sql, connection);
        if (!(vendedor.CajaMovil == 1 || dbConexion?.MeserosCompartidos == true))
        {
            command.Parameters.AddWithValue("@idVendedor", vendedor.Id);
        }

        return await ReadListAsync(command, reader => new CuentaCajaData
        {
            Id = GetInt(reader, "id"),
            AliasVenta = GetString(reader, "aliasVenta"),
            Eliminada = GetBool(reader, "eliminada"),
            IdVendedor = GetInt(reader, "idvendedor"),
            NombreVendedor = GetString(reader, "nombrevendedor"),
            NombreMesa = GetString(reader, "nombremesa"),
            NombreClienteDomicilio = GetString(reader, "nombreCD")
        }, cancellationToken);
    }

    private async Task<List<CuentaCajaData>> LoadCuentasMesasVistaAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, aliasVenta, eliminada, idvendedor, nombrevendedor, nombremesa, nombreCD
            FROM V_CuentasVenta
            WHERE numeroVenta = 0 AND eliminada = 0
            ORDER BY id
            """;

        await using var command = new SqlCommand(sql, connection);
        return await ReadListAsync(command, reader => new CuentaCajaData
        {
            Id = GetInt(reader, "id"),
            AliasVenta = GetString(reader, "aliasVenta"),
            Eliminada = GetBool(reader, "eliminada"),
            IdVendedor = GetInt(reader, "idvendedor"),
            NombreVendedor = GetString(reader, "nombrevendedor"),
            NombreMesa = GetString(reader, "nombremesa"),
            NombreClienteDomicilio = GetString(reader, "nombreCD")
        }, cancellationToken);
    }

    private async Task<List<VentaMesaRelacionData>> LoadRelacionesVentaMesaAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, idVenta, idMesa
            FROM R_VentaMesa
            ORDER BY id
            """;

        await using var command = new SqlCommand(sql, connection);
        return await ReadListAsync(command, reader => new VentaMesaRelacionData
        {
            Id = GetInt(reader, "id"),
            IdVenta = GetInt(reader, "idVenta"),
            IdMesa = GetInt(reader, "idMesa")
        }, cancellationToken);
    }

    private async Task<List<ZonaCajaData>> LoadZonasAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, nombreZona
            FROM Zonas
            ORDER BY id
            """;

        await using var command = new SqlCommand(sql, connection);
        return await ReadListAsync(command, reader => new ZonaCajaData
        {
            Id = GetInt(reader, "id"),
            NombreZona = GetString(reader, "nombreZona")
        }, cancellationToken);
    }

    private async Task<List<MesaCajaData>> LoadMesasAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, nombreMesa, estadoMesa, idZona
            FROM Mesas
            ORDER BY idZona, id
            """;

        await using var command = new SqlCommand(sql, connection);
        return await ReadListAsync(command, reader => new MesaCajaData
        {
            Id = GetInt(reader, "id"),
            NombreMesa = GetString(reader, "nombreMesa"),
            EstadoMesa = GetInt(reader, "estadoMesa"),
            IdZona = GetInt(reader, "idZona")
        }, cancellationToken);
    }

    private async Task<List<CategoriaCajaData>> LoadCategoriasAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, nombreCategoria, visible
            FROM V_Categoria
            ORDER BY id
            """;

        await using var command = new SqlCommand(sql, connection);
        return await ReadListAsync(command, reader => new CategoriaCajaData
        {
            Id = GetInt(reader, "id"),
            NombreCategoria = GetString(reader, "nombreCategoria"),
            Visible = GetInt(reader, "visible")
        }, cancellationToken);
    }

    private async Task<List<ProductoCajaData>> LoadProductosAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, idCategoria, impuesto_id, idPresentacion, codigoProducto, nombreProducto, costo_mas_impuesto, porcentaje_impuesto, precioVenta, visible, estadoProducto, estadoPresentacion
            FROM v_productoVenta
            ORDER BY idCategoria, nombreProducto
            """;

        await using var command = new SqlCommand(sql, connection);
        return await ReadListAsync(command, reader => new ProductoCajaData
        {
            Id = GetInt(reader, "id"),
            IdCategoria = GetInt(reader, "idCategoria"),
            ImpuestoId = GetInt(reader, "impuesto_id"),
            IdPresentacion = GetInt(reader, "idPresentacion"),
            CodigoProducto = GetString(reader, "codigoProducto"),
            NombreProducto = GetString(reader, "nombreProducto"),
            CostoMasImpuesto = GetDecimal(reader, "costo_mas_impuesto"),
            PorcentajeImpuesto = GetDecimal(reader, "porcentaje_impuesto"),
            PrecioVenta = GetDecimal(reader, "precioVenta"),
            Visible = GetInt(reader, "visible"),
            EstadoProducto = GetInt(reader, "estadoProducto"),
            EstadoPresentacion = GetInt(reader, "estadoPresentacion")
        }, cancellationToken);
    }

    private async Task<VentaCajaData?> LoadVentaActivaAsync(SqlConnection connection, int idVenta, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) id, aliasVenta, subtotalVenta, ivaVenta, totalVenta, propina, por_propina, total_A_Pagar
            FROM V_TablaVentas
            WHERE id = @idVenta
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idVenta", idVenta);

        return await ReadSingleAsync(command, reader => new VentaCajaData
        {
            Id = GetInt(reader, "id"),
            AliasVenta = GetString(reader, "aliasVenta"),
            SubtotalVenta = GetDecimal(reader, "subtotalVenta"),
            IvaVenta = GetDecimal(reader, "ivaVenta"),
            TotalVenta = GetDecimal(reader, "totalVenta"),
            Propina = GetDecimal(reader, "propina"),
            PorcentajePropina = GetDecimal(reader, "por_propina"),
            TotalAPagar = GetDecimal(reader, "total_A_Pagar")
        }, cancellationToken);
    }

    private async Task<List<CuentaClienteCajaData>> LoadCuentasClienteAsync(SqlConnection connection, int idVenta, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, idVenta, nombreCuenta, eliminada, subtotalVenta, ivaVenta, totalVenta, por_propina, propina, total_A_Pagar
            FROM V_CuentaCliente
            WHERE eliminada = 0 AND idVenta = @idVenta
            ORDER BY fecha, id
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idVenta", idVenta);

        return await ReadListAsync(command, reader => new CuentaClienteCajaData
        {
            Id = GetInt(reader, "id"),
            IdVenta = GetInt(reader, "idVenta"),
            NombreCuenta = GetString(reader, "nombreCuenta"),
            Eliminada = GetBool(reader, "eliminada"),
            SubtotalVenta = GetDecimal(reader, "subtotalVenta"),
            IvaVenta = GetDecimal(reader, "ivaVenta"),
            TotalVenta = GetDecimal(reader, "totalVenta"),
            PorcentajePropina = GetDecimal(reader, "por_propina"),
            Propina = GetDecimal(reader, "propina"),
            TotalAPagar = GetDecimal(reader, "total_A_Pagar")
        }, cancellationToken);
    }

    private async Task<List<DetalleCajaData>> LoadDetallesAsync(SqlConnection connection, int idVenta, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, idVenta, idCuentaCliente, idCategoria, estadoDetalle, unidad, nombreProducto, nombreCuenta, adiciones, itemComandado, precioVenta, totalDetalle
            FROM V_DetalleCaja
            WHERE idVenta = @idVenta AND estadoDetalle = 1
            ORDER BY id
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idVenta", idVenta);

        return await ReadListAsync(command, reader => new DetalleCajaData
        {
            Id = GetInt(reader, "id"),
            IdVenta = GetInt(reader, "idVenta"),
            IdCuentaCliente = GetInt(reader, "idCuentaCliente"),
            IdCategoria = GetInt(reader, "idCategoria"),
            EstadoDetalle = GetInt(reader, "estadoDetalle"),
            Unidad = GetDecimal(reader, "unidad"),
            NombreProducto = GetString(reader, "nombreProducto"),
            NombreCuenta = GetString(reader, "nombreCuenta"),
            Adiciones = GetString(reader, "adiciones"),
            ItemComandado = GetInt(reader, "itemComandado"),
            PrecioVenta = GetDecimal(reader, "precioVenta"),
            TotalDetalle = GetDecimal(reader, "totalDetalle")
        }, cancellationToken);
    }

    public async Task<int> CrearNuevoServicioAsync(
        string db,
        VendedorInfo vendedor,
        SedeInfo? sede,
        BaseCajaInfo? baseCaja,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildSqlConnectionString(db));
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var idVenta = await InsertVentaAsync(connection, transaction, sede, baseCaja, cancellationToken);
            await InsertRelacionVentaVendedorAsync(connection, transaction, idVenta, vendedor.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return idVenta;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<int> CrearServicioEnMesaAsync(
        string db,
        int idMesa,
        VendedorInfo vendedor,
        SedeInfo? sede,
        BaseCajaInfo? baseCaja,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildSqlConnectionString(db));
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var mesaOcupada = await ExisteRelacionMesaAsync(connection, transaction, idMesa, cancellationToken);
            if (mesaOcupada)
            {
                throw new InvalidOperationException("La mesa seleccionada ya tiene un servicio activo.");
            }

            var idVenta = await InsertVentaAsync(connection, transaction, sede, baseCaja, cancellationToken);
            await InsertRelacionVentaVendedorAsync(connection, transaction, idVenta, vendedor.Id, cancellationToken);
            await InsertRelacionVentaMesaAsync(connection, transaction, idVenta, idMesa, cancellationToken);
            await ActualizarEstadoMesaAsync(connection, transaction, idMesa, 1, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return idVenta;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<CajaCommandResult> LiberarMesaAsync(string db, int idMesa, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildSqlConnectionString(db));
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await ActualizarEstadoMesaAsync(connection, transaction, idMesa, 0, cancellationToken);

            const string deleteSql = """
                DELETE FROM R_VentaMesa
                WHERE idMesa = @idMesa
                """;
            await using var deleteCommand = new SqlCommand(deleteSql, connection, (SqlTransaction)transaction);
            deleteCommand.Parameters.AddWithValue("@idMesa", idMesa);
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return CajaCommandResult.SuccessResult("Mesa liberada correctamente.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return CajaCommandResult.ErrorResult(ex.Message);
        }
    }

    public async Task<CajaCommandResult> AgregarProductoAsync(
        string db,
        int idVenta,
        int idPresentacion,
        decimal cantidad,
        int idCuentaCliente,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildSqlConnectionString(db));
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var producto = await LoadProductoPorPresentacionAsync(connection, transaction, idPresentacion, cancellationToken);
            if (producto is null)
            {
                return CajaCommandResult.ErrorResult("No se encontro el producto seleccionado.");
            }

            const string insertDetalleSql = """
                INSERT INTO DetalleVenta
                (
                    idVenta,
                    idPresentacion,
                    nombreProducto,
                    costoUnidad,
                    precioVenta,
                    estadoDetalle,
                    ivaDetalle,
                    cantidadDetalle,
                    codigoProducto,
                    observacion,
                    guidDetalle,
                    opciones,
                    adiciones,
                    impuesto_id
                )
                OUTPUT INSERTED.id
                VALUES
                (
                    @idVenta,
                    @idPresentacion,
                    @nombreProducto,
                    @costoUnidad,
                    @precioVenta,
                    1,
                    @ivaDetalle,
                    @cantidadDetalle,
                    @codigoProducto,
                    '--',
                    @guidDetalle,
                    '--',
                    '--',
                    @impuestoId
                )
                """;

            await using var insertDetalle = new SqlCommand(insertDetalleSql, connection, (SqlTransaction)transaction);
            insertDetalle.Parameters.AddWithValue("@idVenta", idVenta);
            insertDetalle.Parameters.AddWithValue("@idPresentacion", idPresentacion);
            insertDetalle.Parameters.AddWithValue("@nombreProducto", producto.NombreProducto);
            insertDetalle.Parameters.AddWithValue("@costoUnidad", producto.CostoMasImpuesto);
            insertDetalle.Parameters.AddWithValue("@precioVenta", producto.PrecioVenta);
            insertDetalle.Parameters.AddWithValue("@ivaDetalle", producto.PorcentajeImpuesto);
            insertDetalle.Parameters.AddWithValue("@cantidadDetalle", cantidad);
            insertDetalle.Parameters.AddWithValue("@codigoProducto", producto.CodigoProducto);
            insertDetalle.Parameters.AddWithValue("@guidDetalle", Guid.NewGuid());
            insertDetalle.Parameters.AddWithValue("@impuestoId", producto.ImpuestoId);

            var idDetalle = Convert.ToInt32(await insertDetalle.ExecuteScalarAsync(cancellationToken));

            if (idCuentaCliente > 0)
            {
                const string insertRelacionCuentaSql = """
                    INSERT INTO R_CuentaCliente_DetalleVenta (fecha, idCuentaCliente, idDetalleVenta, eliminada)
                    VALUES (@fecha, @idCuentaCliente, @idDetalleVenta, 0)
                    """;
                await using var insertRelacion = new SqlCommand(insertRelacionCuentaSql, connection, (SqlTransaction)transaction);
                insertRelacion.Parameters.AddWithValue("@fecha", DateTime.Now);
                insertRelacion.Parameters.AddWithValue("@idCuentaCliente", idCuentaCliente);
                insertRelacion.Parameters.AddWithValue("@idDetalleVenta", idDetalle);
                await insertRelacion.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return CajaCommandResult.SuccessResult($"Producto ({producto.NombreProducto}) agregado.", idDetalle);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return CajaCommandResult.ErrorResult(ex.Message);
        }
    }

    public async Task<int> CrearCuentaClienteAsync(
        string db,
        int idVenta,
        string nombreCuenta,
        SedeInfo? sede,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildSqlConnectionString(db));
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO CuentaCliente (fecha, idVenta, nombreCuenta, preCuenta, eliminada, por_propina, propina)
            OUTPUT INSERTED.id
            VALUES (@fecha, @idVenta, @nombreCuenta, 0, 0, @porPropina, 0)
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@fecha", DateTime.Now);
        command.Parameters.AddWithValue("@idVenta", idVenta);
        command.Parameters.AddWithValue("@nombreCuenta", nombreCuenta);
        command.Parameters.AddWithValue("@porPropina", (sede?.PorcentajePropina ?? 0) / 100m);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<CajaCommandResult> ActualizarCantidadDetalleAsync(
        string db,
        int idDetalle,
        decimal cantidad,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildSqlConnectionString(db));
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE DetalleVenta
            SET cantidadDetalle = @cantidad
            WHERE id = @idDetalle
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@cantidad", cantidad);
        command.Parameters.AddWithValue("@idDetalle", idDetalle);
        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0
            ? CajaCommandResult.SuccessResult("Cantidad actualizada.")
            : CajaCommandResult.ErrorResult("No se encontro el detalle para actualizar.");
    }

    public async Task<CajaCommandResult> ActualizarNotaDetalleAsync(
        string db,
        int idDetalle,
        string nota,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildSqlConnectionString(db));
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE DetalleVenta
            SET adiciones = @nota
            WHERE id = @idDetalle
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@nota", string.IsNullOrWhiteSpace(nota) ? "--" : nota.Trim());
        command.Parameters.AddWithValue("@idDetalle", idDetalle);
        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0
            ? CajaCommandResult.SuccessResult("Nota actualizada.")
            : CajaCommandResult.ErrorResult("No se encontro el detalle para actualizar.");
    }

    public async Task<CajaCommandResult> EliminarDetalleAsync(
        string db,
        int idDetalle,
        string nota,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildSqlConnectionString(db));
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE DetalleVenta
            SET nombreProducto = @nota,
                estadoDetalle = 0
            WHERE id = @idDetalle
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@nota", string.IsNullOrWhiteSpace(nota) ? "Eliminado" : nota.Trim());
        command.Parameters.AddWithValue("@idDetalle", idDetalle);
        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0
            ? CajaCommandResult.SuccessResult("Producto eliminado correctamente.")
            : CajaCommandResult.ErrorResult("No se encontro el detalle para eliminar.");
    }

    public async Task<CajaCommandResult> EditarPropinaAsync(
        string db,
        int idVenta,
        int idCuentaCliente,
        decimal porcentaje,
        decimal propina,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildSqlConnectionString(db));
        await connection.OpenAsync(cancellationToken);

        var porPropina = porcentaje / 100m;
        var sql = idCuentaCliente > 0
            ? """
              UPDATE CuentaCliente
              SET por_propina = @porPropina,
                  propina = @propina
              WHERE id = @id
              """
            : """
              UPDATE TablaVentas
              SET porpropina = @porPropina,
                  propina = @propina
              WHERE id = @id
              """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@porPropina", porPropina);
        command.Parameters.AddWithValue("@propina", propina);
        command.Parameters.AddWithValue("@id", idCuentaCliente > 0 ? idCuentaCliente : idVenta);
        var rows = await command.ExecuteNonQueryAsync(cancellationToken);

        return rows > 0
            ? CajaCommandResult.SuccessResult("Propina actualizada correctamente.")
            : CajaCommandResult.ErrorResult("No fue posible actualizar la propina.");
    }

    public async Task<CajaCommandResult> EnviarComandaAsync(
        string db,
        int idVenta,
        int idMesa,
        int idMesero,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildSqlConnectionString(db));
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO ImprecionComandaAdd (idVenta, idMesa, idMesero, estado)
            VALUES (@idVenta, @idMesa, @idMesero, 1)
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idVenta", idVenta);
        command.Parameters.AddWithValue("@idMesa", idMesa.ToString());
        command.Parameters.AddWithValue("@idMesero", idMesero.ToString());
        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0
            ? CajaCommandResult.SuccessResult("Comanda enviada correctamente.")
            : CajaCommandResult.ErrorResult("Comanda no enviada correctamente.");
    }

    public async Task<CajaCommandResult> SolicitarCuentaAsync(string db, int idVenta, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(BuildSqlConnectionString(db));
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO ImprimirCuenta (idVenta)
            VALUES (@idVenta)
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idVenta", idVenta);
        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0
            ? CajaCommandResult.SuccessResult("Cuenta enviada correctamente.")
            : CajaCommandResult.ErrorResult("Cuenta no enviada correctamente.");
    }

    private static async Task<int> InsertVentaAsync(
        SqlConnection connection,
        DbTransaction transaction,
        SedeInfo? sede,
        BaseCajaInfo? baseCaja,
        CancellationToken cancellationToken)
    {
        const string insertVentaSql = """
            INSERT INTO TablaVentas
            (
                fechaVenta,
                numeroVenta,
                descuentoVenta,
                efectivoVenta,
                cambioVenta,
                estadoVenta,
                numeroReferenciaPago,
                diasCredito,
                observacionVenta,
                IdSede,
                guidVenta,
                abonoTarjeta,
                propina,
                abonoEfectivo,
                idMedioDePago,
                idResolucion,
                idFormaDePago,
                razonDescuento,
                idBaseCaja,
                aliasVenta,
                porpropina,
                eliminada
            )
            OUTPUT INSERTED.id
            VALUES
            (
                @fechaVenta,
                0,
                0,
                0,
                0,
                'PENDIENTE',
                '-',
                0,
                '-',
                @idSede,
                @guidVenta,
                0,
                0,
                0,
                10,
                0,
                1,
                '-',
                @idBaseCaja,
                '--',
                @porPropina,
                0
            )
            """;

        await using var command = new SqlCommand(insertVentaSql, connection, (SqlTransaction)transaction);
        command.Parameters.AddWithValue("@fechaVenta", DateTime.Now);
        command.Parameters.AddWithValue("@idSede", sede?.Id ?? 0);
        command.Parameters.AddWithValue("@guidVenta", Guid.NewGuid());
        command.Parameters.AddWithValue("@idBaseCaja", baseCaja?.Id ?? 0);
        command.Parameters.AddWithValue("@porPropina", (sede?.PorcentajePropina ?? 0) / 100m);

        var idVenta = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));

        const string updateAliasSql = """
            UPDATE TablaVentas
            SET aliasVenta = @aliasVenta
            WHERE id = @idVenta
            """;
        await using var updateAlias = new SqlCommand(updateAliasSql, connection, (SqlTransaction)transaction);
        updateAlias.Parameters.AddWithValue("@aliasVenta", idVenta.ToString());
        updateAlias.Parameters.AddWithValue("@idVenta", idVenta);
        await updateAlias.ExecuteNonQueryAsync(cancellationToken);

        return idVenta;
    }

    private static async Task InsertRelacionVentaVendedorAsync(
        SqlConnection connection,
        DbTransaction transaction,
        int idVenta,
        int idVendedor,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO R_VentaVendedor (idVenta, idVendedor)
            VALUES (@idVenta, @idVendedor)
            """;

        await using var command = new SqlCommand(sql, connection, (SqlTransaction)transaction);
        command.Parameters.AddWithValue("@idVenta", idVenta);
        command.Parameters.AddWithValue("@idVendedor", idVendedor);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertRelacionVentaMesaAsync(
        SqlConnection connection,
        DbTransaction transaction,
        int idVenta,
        int idMesa,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO R_VentaMesa (idVenta, idMesa)
            VALUES (@idVenta, @idMesa)
            """;

        await using var command = new SqlCommand(sql, connection, (SqlTransaction)transaction);
        command.Parameters.AddWithValue("@idVenta", idVenta);
        command.Parameters.AddWithValue("@idMesa", idMesa);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> ExisteRelacionMesaAsync(
        SqlConnection connection,
        DbTransaction transaction,
        int idMesa,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) 1
            FROM R_VentaMesa
            WHERE idMesa = @idMesa
            """;
        await using var command = new SqlCommand(sql, connection, (SqlTransaction)transaction);
        command.Parameters.AddWithValue("@idMesa", idMesa);
        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    private static async Task ActualizarEstadoMesaAsync(
        SqlConnection connection,
        DbTransaction transaction,
        int idMesa,
        int estadoMesa,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE Mesas
            SET estadoMesa = @estadoMesa
            WHERE id = @idMesa
            """;

        await using var command = new SqlCommand(sql, connection, (SqlTransaction)transaction);
        command.Parameters.AddWithValue("@estadoMesa", estadoMesa);
        command.Parameters.AddWithValue("@idMesa", idMesa);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<ProductoCajaData?> LoadProductoPorPresentacionAsync(
        SqlConnection connection,
        DbTransaction transaction,
        int idPresentacion,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) id, idCategoria, impuesto_id, idPresentacion, codigoProducto, nombreProducto, costo_mas_impuesto, porcentaje_impuesto, precioVenta, visible, estadoProducto, estadoPresentacion
            FROM v_productoVenta
            WHERE idPresentacion = @idPresentacion
            """;

        await using var command = new SqlCommand(sql, connection, (SqlTransaction)transaction);
        command.Parameters.AddWithValue("@idPresentacion", idPresentacion);
        return await ReadSingleAsync(command, reader => new ProductoCajaData
        {
            Id = GetInt(reader, "id"),
            IdCategoria = GetInt(reader, "idCategoria"),
            ImpuestoId = GetInt(reader, "impuesto_id"),
            IdPresentacion = GetInt(reader, "idPresentacion"),
            CodigoProducto = GetString(reader, "codigoProducto"),
            NombreProducto = GetString(reader, "nombreProducto"),
            CostoMasImpuesto = GetDecimal(reader, "costo_mas_impuesto"),
            PorcentajeImpuesto = GetDecimal(reader, "porcentaje_impuesto"),
            PrecioVenta = GetDecimal(reader, "precioVenta"),
            Visible = GetInt(reader, "visible"),
            EstadoProducto = GetInt(reader, "estadoProducto"),
            EstadoPresentacion = GetInt(reader, "estadoPresentacion")
        }, cancellationToken);
    }

    private static async Task<List<T>> ReadListAsync<T>(SqlCommand command, Func<SqlDataReader, T> map, CancellationToken cancellationToken)
    {
        var results = new List<T>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(map(reader));
        }

        return results;
    }

    private static async Task<T?> ReadSingleAsync<T>(SqlCommand command, Func<SqlDataReader, T> map, CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? map(reader) : default;
    }

    private string BuildSqlConnectionString(string db)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = GetSetting("COMANDASVENTAS_DB_SERVER", "ComandasVentas:DbServer", DefaultSqlServer),
            InitialCatalog = db,
            UserID = GetSetting("COMANDASVENTAS_DB_USER", "ComandasVentas:DbUser", DefaultSqlUser),
            Password = GetSetting("COMANDASVENTAS_DB_PASSWORD", "ComandasVentas:DbPassword", DefaultSqlPassword),
            TrustServerCertificate = true,
            MultipleActiveResultSets = true,
            PersistSecurityInfo = true
        };

        return builder.ConnectionString;
    }

    private string GetSetting(string envName, string configPath, string fallback)
    {
        var envValue = Environment.GetEnvironmentVariable(envName);
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            return envValue.Trim();
        }

        var configValue = configuration[configPath];
        return !string.IsNullOrWhiteSpace(configValue) ? configValue.Trim() : fallback;
    }

    private static string GetString(SqlDataReader reader, string columnName) =>
        reader[columnName] is DBNull ? string.Empty : Convert.ToString(reader[columnName]) ?? string.Empty;

    private static int GetInt(SqlDataReader reader, string columnName) =>
        reader[columnName] is DBNull ? 0 : Convert.ToInt32(reader[columnName]);

    private static decimal GetDecimal(SqlDataReader reader, string columnName) =>
        reader[columnName] is DBNull ? 0m : Convert.ToDecimal(reader[columnName]);

    private static bool GetBool(SqlDataReader reader, string columnName) =>
        reader[columnName] is not DBNull && Convert.ToBoolean(reader[columnName]);
}

public sealed class CajaCommandResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int? AffectedId { get; init; }

    public static CajaCommandResult SuccessResult(string message, int? affectedId = null) =>
        new() { Success = true, Message = message, AffectedId = affectedId };

    public static CajaCommandResult ErrorResult(string message) =>
        new() { Success = false, Message = message };
}
