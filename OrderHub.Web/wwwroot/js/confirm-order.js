
(() => {
    const form = document.getElementById('order-form');
    if (!form) return;

    const subtotalEl = document.getElementById('subtotal');
    const toGBP = pence => new Intl.NumberFormat('en-GB',
        { style: 'currency', currency: 'GBP' }).format(pence / 100);

    function recalc() {
        let totalPence = 0;

        for (const row of form.querySelectorAll('[data-line]')) {
            const unitPence = Number(row.dataset.unitPence);
            const qty = parseInt(row.querySelector('input[type="number"]').value, 10);
            const valid = Number.isInteger(qty) && qty > 0;
            const linePence = valid ? unitPence * qty : 0;

            row.querySelector('[data-line-total]').textContent = valid ? toGBP(linePence) : '—';
            totalPence += linePence;
        }

        subtotalEl.textContent = toGBP(totalPence);
    }

    form.addEventListener('input', e => {
        if (e.target.matches('input[type="number"]')) recalc();
    });

    recalc();
})();