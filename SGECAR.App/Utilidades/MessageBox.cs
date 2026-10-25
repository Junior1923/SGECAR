using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SGECAR.App.Utilidades;

public enum TipoMensaje { Exito, Error, Advertencia, Informacion }


public static class MessageBox
{
    public static void MostrarMessageBox(this PageModel pagina, TipoMensaje tipo, string titulo, string texto)
    {
        pagina.TempData["MsgTipo"] = tipo.ToString();
        pagina.TempData["MsgTitulo"] = titulo;
        pagina.TempData["MsgTexto"] = texto;
    }
}
