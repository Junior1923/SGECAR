// Modo oscuro: se carga en el <head> para aplicar el tema antes de pintar la página
// (así no se ve un parpadeo blanco al cambiar de pantalla). La elección se guarda en el navegador.
(function () {
    try {
        if (localStorage.getItem("sgecar-tema") === "oscuro")
            document.documentElement.setAttribute("data-tema", "oscuro");
    } catch (e) { /* si el navegador no permite guardar, se queda en claro */ }
})();
