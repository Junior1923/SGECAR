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
