document.addEventListener('DOMContentLoaded', function () {
    const creditCardOption = document.getElementById('credit-card-option');
    const paypalOption = document.getElementById('paypal-option');
    const creditCardForm = document.getElementById('credit-card-form');

    function toggleForm() {
        if (creditCardOption.checked) {
            creditCardForm.style.display = 'block';
        } else if (paypalOption.checked) {
            creditCardForm.style.display = 'none';
        }
    }

    creditCardOption.addEventListener('change', toggleForm);
    paypalOption.addEventListener('change', toggleForm);

    // Initial check
    toggleForm();

});
