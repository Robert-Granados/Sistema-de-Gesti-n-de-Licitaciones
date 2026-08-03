// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(() => {
    "use strict";

    const button = document.getElementById("themeToggle");
    if (!button) return;

    const root = document.documentElement;
    const icon = button.querySelector("[data-theme-icon]");
    const label = button.querySelector("[data-theme-label]");

    const applyTheme = theme => {
        const dark = theme === "dark";
        root.dataset.theme = dark ? "dark" : "light";
        root.dataset.bsTheme = dark ? "dark" : "light";
        button.setAttribute("aria-pressed", dark.toString());
        button.setAttribute("aria-label", dark
            ? "Cambiar a modo claro"
            : "Cambiar a modo oscuro");
        icon.textContent = dark ? "☀" : "☾";
        label.textContent = dark ? "Modo claro" : "Modo oscuro";
    };

    button.addEventListener("click", () => {
        const theme = root.dataset.theme === "dark" ? "light" : "dark";
        applyTheme(theme);
        try {
            localStorage.setItem("licitaciones-theme", theme);
        } catch {
            // El control sigue funcionando si el almacenamiento está bloqueado.
        }
    });

    applyTheme(root.dataset.theme);
})();

(() => {
    "use strict";

    const toggle = document.querySelector("[data-currency-toggle]");
    if (!toggle) return;

    const rate = Number.parseFloat(toggle.dataset.exchangeRate);
    if (!Number.isFinite(rate) || rate <= 0) return;

    const crcFormatter = new Intl.NumberFormat("es-CR", {
        style: "currency", currency: "CRC", minimumFractionDigits: 2
    });
    const usdFormatter = new Intl.NumberFormat("en-US", {
        style: "currency", currency: "USD", minimumFractionDigits: 2
    });

    const render = currency => {
        document.querySelectorAll("[data-currency-amount]").forEach(element => {
            const crc = Number.parseFloat(element.dataset.currencyAmount);
            if (!Number.isFinite(crc)) return;
            element.textContent = currency === "USD"
                ? usdFormatter.format(crc / rate)
                : crcFormatter.format(crc);
        });
        document.querySelectorAll("[data-currency-label]")
            .forEach(element => element.textContent = currency);
        toggle.querySelectorAll("[data-currency]").forEach(button => {
            const selected = button.dataset.currency === currency;
            button.classList.toggle("active", selected);
            button.setAttribute("aria-pressed", selected.toString());
        });
    };

    toggle.addEventListener("click", event => {
        const button = event.target.closest("[data-currency]");
        if (button) render(button.dataset.currency);
    });

    render("CRC");
})();

(() => {
    "use strict";

    const contenedor = document.getElementById("notificaciones");
    if (contenedor) {
        const alertas = Array.from(contenedor.querySelectorAll(".notificacion"));
        alertas.forEach(alerta => {
            window.setTimeout(() => {
                if (window.bootstrap && window.bootstrap.Alert) {
                    window.bootstrap.Alert.getOrCreateInstance(alerta).close();
                }
            }, 6000);
        });
    }

    window.mostrarNotificacion = (tipo, mensaje) => {
        const destino = document.getElementById("notificaciones");
        if (!destino) return;

        const clases = {
            exito: "alert-success",
            advertencia: "alert-warning",
            error: "alert-danger"
        };
        const titulos = {
            exito: "Éxito",
            advertencia: "Advertencia",
            error: "Error"
        };
        const clase = clases[tipo] || "alert-info";

        const alerta = document.createElement("div");
        alerta.className = `alert ${clase} alert-dismissible fade show`;
        alerta.setAttribute("role", tipo === "exito" ? "status" : "alert");

        const titulo = document.createElement("strong");
        titulo.textContent = `${titulos[tipo] || "Información"}. `;

        const cuerpo = document.createElement("span");
        cuerpo.textContent = String(mensaje ?? "");

        const boton = document.createElement("button");
        boton.type = "button";
        boton.className = "btn-close";
        boton.dataset.bsDismiss = "alert";
        boton.setAttribute("aria-label", "Cerrar");

        alerta.append(titulo, cuerpo, boton);
        destino.appendChild(alerta);

        window.setTimeout(() => {
            if (window.bootstrap && window.bootstrap.Alert) {
                window.bootstrap.Alert.getOrCreateInstance(alerta).close();
            }
        }, 6000);
    };
})();
