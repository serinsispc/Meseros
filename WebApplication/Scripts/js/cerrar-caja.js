//con esta función llamamos en boton oculto que hace puente con el código C# 
function EjecutarAccion(accion, argumento) {
    document.getElementById("hidAccion").value = accion || "";
    document.getElementById("hidArgumento").value = argumento || "";
    document.getElementById("btnBridge").click();
}

//con esta función interpretamos los argumentos o parámetros
function BuildArgs(obj) {
    return Object.keys(obj)
        .map(k => k + "=" + encodeURIComponent(obj[k] ?? ""))
        .join("|");
}

//es ta es la funcion que llamamos desde el boton de html
//function ConfirmarCierre() {

//    //en esta parte traemos los elementos de donde vamos a extraer los datos
//    const idTurnoEl = document.getElementById("hidIdTurno");
//    const efectivoEl = document.getElementById("txtEfectivoFisicoxxx");
//    const obsEl = document.getElementById("txtObsCierrexxx");

//    // 🔵 Leer valores
//    const idTurno = (idTurnoEl.value || "").trim();
//    const efectivo = (efectivoEl.value || "").trim();
//    const obs = (obsEl.value || "").trim();

//    // 🟢 Construir argumentos
//    const args = BuildArgs({
//        ID: idTurno,
//        EFECTIVO: efectivo || "0",
//        OBS: obs
//    });

//    // 🚀 Llamar al puente hacia C#
//    EjecutarAccion("EventoPrueva", args);
//}

/////////////////////////////////////////////////////////////////////////
///        desde esta parte en adelante van las funciones reales      ///
/////////////////////////////////////////////////////////////////////////

