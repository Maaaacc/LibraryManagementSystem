function showToast(type, message) {
    const container = document.querySelector('.toast-container');

    // Define colors/icons depending on type
    let bgClass = "bg-secondary";
    let icon = "bi-info-circle";

    if (type === "success") {
        bgClass = "bg-success text-white";
        icon = "bi-check-circle";
    } else if (type === "error") {
        bgClass = "bg-danger text-white";
        icon = "bi-exclamation-triangle";
    } else if (type === "warning") {
        bgClass = "bg-warning text-dark";
        icon = "bi-exclamation-circle";
    } else if (type === "info") {
        bgClass = "bg-info text-dark";
        icon = "bi-info-circle";
    }

    // Create toast element
    const toastEl = document.createElement("div");
    toastEl.className = `toast align-items-center ${bgClass} border-0 shadow`;
    toastEl.role = "alert";
    toastEl.ariaLive = "assertive";
    toastEl.ariaAtomic = "true";

    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                <i class="bi ${icon} me-2"></i> ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    container.appendChild(toastEl);

    // Initialize and show toast
    const toast = new bootstrap.Toast(toastEl, { delay: 3000 });
    toast.show();

    // Remove element after hidden
    toastEl.addEventListener('hidden.bs.toast', () => {
        toastEl.remove();
    });
}
