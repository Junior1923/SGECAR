document.addEventListener("DOMContentLoaded", function () {

    // Abre el MessageBox si la página trae un mensaje
    var messageBox = document.getElementById("messageBox");
    if (messageBox) {
        new bootstrap.Modal(messageBox).show();
    }

    // Mostrar / ocultar contraseña
    document.querySelectorAll(".ver-clave").forEach(function (boton) {
        boton.addEventListener("click", function () {
            var campo = document.querySelector(boton.dataset.objetivo);
            if (!campo) return;
            var mostrar = campo.type === "password";
            campo.type = mostrar ? "text" : "password";
            boton.querySelector(".ver").classList.toggle("d-none", mostrar);
            boton.querySelector(".ocultar").classList.toggle("d-none", !mostrar);
        });
    });

    // Revalidar al cambiar un campo para quitar el mensaje de error
    if (window.jQuery && jQuery.fn.valid) {
        jQuery("form select, form input").on("change", function () { jQuery(this).valid(); });
    }

    // Botón "Limpiar campos": vacía el formulario y quita los mensajes de error
    document.querySelectorAll("[data-limpiar]").forEach(function (boton) {
        boton.addEventListener("click", function () {
            var form = boton.closest("form");
            form.querySelectorAll("input, select, textarea").forEach(function (campo) {
                if (campo.type === "hidden" || campo.type === "submit" || campo.type === "button") return;
                if (campo.type === "checkbox" || campo.type === "radio") campo.checked = false;
                else if (campo.tagName === "SELECT") campo.selectedIndex = 0;
                else campo.value = "";
            });
            form.querySelectorAll(".error-campo").forEach(function (e) { e.textContent = ""; });
            form.querySelectorAll(".input-validation-error").forEach(function (e) { e.classList.remove("input-validation-error"); });
            var resumen = form.querySelector(".alerta-error");
            if (resumen) resumen.innerHTML = "";
            var primero = form.querySelector("input:not([type=hidden]), select, textarea");
            if (primero) primero.focus();
        });
    });

    // Confirmación antes de enviar formularios con data-confirmar
    var confirmBox = document.getElementById("confirmBox");
    var pendiente = null;
    if (confirmBox) {
        var modal = new bootstrap.Modal(confirmBox);
        document.querySelectorAll("form[data-confirmar]").forEach(function (form) {
            form.addEventListener("submit", function (e) {
                if (form.dataset.confirmado === "si") return;
                e.preventDefault();
                pendiente = form;
                document.getElementById("confirmBoxTexto").textContent = form.dataset.confirmar;
                modal.show();
            });
        });
        document.getElementById("confirmBoxSi").addEventListener("click", function () {
            if (pendiente) {
                pendiente.dataset.confirmado = "si";
                pendiente.submit();
            }
        });
    }
});
