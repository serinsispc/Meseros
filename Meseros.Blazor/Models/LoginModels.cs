namespace Meseros.Blazor.Models;

public sealed class LoginBootstrapData
{
    public string Db { get; set; } = string.Empty;
    public string SedeNombre { get; set; } = string.Empty;
    public int SedeId { get; set; }
    public Guid? SedeGuid { get; set; }
    public string? LogoDataUrl { get; set; }
    public List<VendedorInfo> Vendedores { get; set; } = new();
}

public sealed class VendedorInfo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public bool CajaMovil { get; set; }
}

public sealed class LoginResult
{
    public bool Success { get; set; }
    public bool RequiresBaseAperture { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string Db { get; set; } = string.Empty;
    public int SedeId { get; set; }
    public int VendedorId { get; set; }
    public string VendedorNombre { get; set; } = string.Empty;
    public int CajaUsuarioId { get; set; }
    public int BaseCajaId { get; set; }
    public string TokenEmpresa { get; set; } = string.Empty;
}

public sealed class BaseCajaRequest
{
    public string Db { get; set; } = string.Empty;
    public int CajaUsuarioId { get; set; }
    public int SedeId { get; set; }
    public int VendedorId { get; set; }
    public string VendedorNombre { get; set; } = string.Empty;
    public decimal ValorBase { get; set; }
    public string TokenEmpresa { get; set; } = string.Empty;
}

internal sealed class SedeRecord
{
    public int id { get; set; }
    public string? nombreSede { get; set; }
    public Guid guidSede { get; set; }
}

internal sealed class ImagenRecord
{
    public Guid id { get; set; }
    public byte[]? imagenBytes { get; set; }
}

internal sealed class VendedorRecord
{
    public int id { get; set; }
    public string? nombreVendedor { get; set; }
    public string? telefonoVendedor { get; set; }
    public string? calveVendedor { get; set; }
    public int cajaMovil { get; set; }
}

internal sealed class R_VendedorUsuarioRecord
{
    public int id { get; set; }
    public int idVendedor { get; set; }
    public int idUSuario { get; set; }
}

internal sealed class TokenEmpresaRecord
{
    public string? token { get; set; }
}

internal sealed class BaseCajaRecord
{
    public int id { get; set; }
    public DateTime fechaApertura { get; set; }
    public int idUsuarioApertura { get; set; }
    public decimal valorBase { get; set; }
    public DateTime? fechaCierre { get; set; }
    public int? idUsuarioCierre { get; set; }
    public string? estadoBase { get; set; }
    public int idSedeBAse { get; set; }
}

internal sealed class CrudResponse
{
    public bool estado { get; set; }
    public string? nuevoId { get; set; }
    public string? idAfectado { get; set; }
    public string? mensaje { get; set; }

    public string? FinalId => !string.IsNullOrWhiteSpace(idAfectado) ? idAfectado : nuevoId;
}
