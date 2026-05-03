(function () {
    const state = {
        products: [],
        services: []
    };

    const currency = new Intl.NumberFormat(undefined, {
        style: "currency",
        currency: "USD"
    });

    function parseIntOrZero(value) {
        const parsed = Number.parseInt(value, 10);
        return Number.isFinite(parsed) ? parsed : 0;
    }

    function renderProducts() {
        const body = document.getElementById("productsTableBody");
        const hidden = document.getElementById("ProductosJson");
        const totalNode = document.getElementById("productsTotal");

        if (!body || !hidden || !totalNode) {
            return;
        }

        body.innerHTML = "";
        let total = 0;

        state.products.forEach((item, index) => {
            const subtotal = item.cantidad * item.precioUnitario;
            total += subtotal;

            body.insertAdjacentHTML("beforeend", `
                <tr>
                    <td>${item.nombreProducto}</td>
                    <td class="text-end">${item.cantidad}</td>
                    <td class="text-end">${currency.format(item.precioUnitario)}</td>
                    <td class="text-end">${currency.format(subtotal)}</td>
                    <td class="text-end"><button type="button" class="btn btn-sm btn-outline-danger" data-remove-product="${index}">Eliminar</button></td>
                </tr>
            `);
        });

        hidden.value = JSON.stringify(state.products);
        totalNode.textContent = currency.format(total);
        updateGrandTotal();
    }

    function renderServices() {
        const body = document.getElementById("servicesTableBody");
        const hidden = document.getElementById("ServiciosJson");
        const totalNode = document.getElementById("servicesTotal");

        if (!body || !hidden || !totalNode) {
            return;
        }

        body.innerHTML = "";
        let total = 0;

        state.services.forEach((item, index) => {
            total += item.precio;

            body.insertAdjacentHTML("beforeend", `
                <tr>
                    <td>${item.nombre}</td>
                    <td class="text-end">${currency.format(item.precio)}</td>
                    <td class="text-end"><button type="button" class="btn btn-sm btn-outline-danger" data-remove-service="${index}">Eliminar</button></td>
                </tr>
            `);
        });

        hidden.value = JSON.stringify(state.services);
        totalNode.textContent = currency.format(total);
        updateGrandTotal();
    }

    function updateGrandTotal() {
        const totalNode = document.getElementById("orderTotal");
        if (!totalNode) {
            return;
        }

        const productsTotal = state.products.reduce((sum, item) => sum + item.cantidad * item.precioUnitario, 0);
        const servicesTotal = state.services.reduce((sum, item) => sum + item.precio, 0);
        totalNode.textContent = currency.format(productsTotal + servicesTotal);
    }

    function attachSelectFilter(input) {
        const selectId = input.dataset.filterSelect;
        const select = document.getElementById(selectId);
        if (!select) {
            return;
        }

        input.addEventListener("input", () => {
            const query = input.value.trim().toLowerCase();
            Array.from(select.options).forEach((option, index) => {
                if (index === 0) {
                    option.hidden = false;
                    return;
                }

                option.hidden = !option.textContent.toLowerCase().includes(query);
            });
        });
    }

    function openDetailModal(button) {
        const body = document.getElementById("detailModalBody");
        if (!body) {
            return;
        }

        const products = JSON.parse(button.dataset.products || "[]");
        const services = JSON.parse(button.dataset.services || "[]");

        body.innerHTML = `
            <div class="report-grid mb-3">
                <div><span>Orden</span><strong>#${button.dataset.orderId}</strong></div>
                <div><span>Cliente</span><strong>${button.dataset.clientId}</strong></div>
                <div><span>Vehículo</span><strong>${button.dataset.vehicleId}</strong></div>
                <div><span>Total</span><strong>${currency.format(Number(button.dataset.total || 0))}</strong></div>
            </div>
            <div class="mb-3"><span class="text-muted">Descripción</span><p class="mt-1">${button.dataset.description || "Sin descripción"}</p></div>
            <div class="row g-3">
                <div class="col-12 col-lg-6">
                    <h6>Productos</h6>
                    <ul class="list-group list-group-flush">
                        ${products.map(item => `<li class="list-group-item bg-transparent text-light d-flex justify-content-between"><span>${item.nombreProducto}</span><span>${item.cantidad} x ${currency.format(item.precioUnitario)}</span></li>`).join("")}
                    </ul>
                </div>
                <div class="col-12 col-lg-6">
                    <h6>Servicios</h6>
                    <ul class="list-group list-group-flush">
                        ${services.map(item => `<li class="list-group-item bg-transparent text-light d-flex justify-content-between"><span>${item.nombre}</span><span>${currency.format(item.precio)}</span></li>`).join("")}
                    </ul>
                </div>
            </div>
        `;
    }

    function loadOrderToForm(button) {
        const cliente = document.getElementById("clienteSelect");
        const vehiculo = document.getElementById("vehiculoSelect");
        const descripcion = document.getElementById("Descripcion");
        const editingId = document.getElementById("EditingOrderId");

        if (cliente) cliente.value = button.dataset.clientId || "";
        if (vehiculo) vehiculo.value = button.dataset.vehicleId || "";
        if (descripcion) descripcion.value = button.dataset.description || "";
        if (editingId) editingId.value = button.dataset.orderId || "";

        state.products = JSON.parse(button.dataset.products || "[]");
        state.services = JSON.parse(button.dataset.services || "[]");
        renderProducts();
        renderServices();
        document.getElementById("orderForm")?.scrollIntoView({ behavior: "smooth", block: "start" });
    }

    function initIndexPage() {
        state.products = [];
        state.services = [];

        document.querySelectorAll(".order-filter[data-filter-select]").forEach(attachSelectFilter);

        document.getElementById("addProductBtn")?.addEventListener("click", () => {
            const select = document.getElementById("productoSelect");
            const quantity = document.getElementById("productoCantidad");
            if (!select || !quantity || !select.value) {
                return;
            }

            const product = window.tallerMecanico.catalog.products.find(item => String(item.id) === select.value);
            if (!product) {
                return;
            }

            state.products.push({
                productoId: product.id,
                nombreProducto: product.nombre,
                cantidad: Math.max(1, parseIntOrZero(quantity.value)),
                precioUnitario: product.precio
            });

            renderProducts();
            quantity.value = 1;
        });

        document.getElementById("addServiceBtn")?.addEventListener("click", () => {
            const select = document.getElementById("servicioSelect");
            if (!select || !select.value) {
                return;
            }

            const service = window.tallerMecanico.catalog.services.find(item => String(item.id) === select.value);
            if (!service) {
                return;
            }

            state.services.push({
                id: service.id,
                nombre: service.nombre,
                precio: service.precio
            });

            renderServices();
        });

        document.getElementById("productsTableBody")?.addEventListener("click", event => {
            const target = event.target;
            if (!(target instanceof HTMLElement) || !target.dataset.removeProduct) {
                return;
            }

            state.products.splice(Number(target.dataset.removeProduct), 1);
            renderProducts();
        });

        document.getElementById("servicesTableBody")?.addEventListener("click", event => {
            const target = event.target;
            if (!(target instanceof HTMLElement) || !target.dataset.removeService) {
                return;
            }

            state.services.splice(Number(target.dataset.removeService), 1);
            renderServices();
        });

        document.getElementById("resetOrderBtn")?.addEventListener("click", () => {
            state.products = [];
            state.services = [];
            document.getElementById("orderForm")?.reset();
            const editingId = document.getElementById("EditingOrderId");
            if (editingId) editingId.value = "";
            renderProducts();
            renderServices();
        });

        document.getElementById("detailModal")?.addEventListener("show.bs.modal", event => {
            const button = event.relatedTarget;
            if (button instanceof HTMLElement) {
                openDetailModal(button);
            }
        });

        const success = window.tallerMecanico.messages?.success;
        if (success) {
            console.info(success);
        }

        renderProducts();
        renderServices();
    }

    window.tallerMecanico = Object.assign(window.tallerMecanico || {}, {
        initIndexPage,
        loadOrderToForm,
        openDetailModal
    });

    document.addEventListener("DOMContentLoaded", () => {
        if (document.getElementById("orderForm")) {
            initIndexPage();
        }
    });
})();