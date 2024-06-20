const runnerPointsContainer = document.getElementById("runner-points-container");

document.addEventListener('DOMContentLoaded', () => {
    const dineInOption = document.getElementById('dine-in-option');
    const locationFormField = document.getElementById('location-form-field');
    const toggleLocationFormField = () => {
        locationFormField.style.display = dineInOption.value == 'Delivery' ? 'block' : 'none';
    }
    dineInOption.addEventListener('change', toggleLocationFormField);
    toggleLocationFormField();

    const runnerPoints = parseInt(document.getElementById("runner-points").value, 10);
    const runnerPointsCheckbox = document.getElementById("is-using-runner-points");
    let isUsingRunnerPoints = $('#is-using-runner-points').prop('checked');

     if (!isUsingRunnerPoints) {
        runnerPointsContainer.classList.remove('d-flex');
         runnerPointsContainer.classList.add('d-none');
    }
    if (runnerPoints === 0) {
        console.log("No runner points available")
        runnerPointsCheckbox.disabled = true;
        $('#is-using-runner-points').prop('checked', false);
        runnerPointsContainer.classList.remove('d-flex');
        runnerPointsContainer.classList.add('d-none');
    }
});


const updateQuantity = (itemId, delta) => {
    const inputField = document.getElementById(`quantity-${itemId}`);
    let currentQuantity = parseInt(inputField.value);
    const newQuantity = Math.max(1, currentQuantity + delta);
    inputField.value = newQuantity;

    const updateQuantityRequest = { itemId: itemId, newQuantity: newQuantity }
    $.ajax({
        url: "/Cart/UpdateQuantity",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(updateQuantityRequest),
        success:  (response) =>  {
            $(`#item-price-${itemId}`).text(`RM ${response.itemPrice}`);
            $('#subtotal').text(`RM ${response.subtotal}`);
            $('#total').text(`RM ${response.total}`);
        },
        error:  (xhr, _status, _error) => {
            console.error("Error updating quantity:", xhr.responseText);
        }
    });
};

const removeItem = (itemId) => {
    $.ajax({
        url: "/Cart/RemoveItem",
        method: "POST",
        data: { itemId: itemId },
        success: (response) => {
            $(`#subtotal`).text(`RM ${response.subtotal}`);
            $('#total').text(`RM ${response.total}`);
            window.location.reload();
        },
        error: (xhr, _status, _error) => {
            console.error("Error removing item:", xhr.responseText);
        }
    });
}

const updateRunnerPoints = () => {
    let isUsingRunnerPoints = $('#is-using-runner-points').prop('checked');
    if (isUsingRunnerPoints) {
        runnerPointsContainer.classList.add('d-flex');
        runnerPointsContainer.classList.remove('d-none');
    }
    if (!isUsingRunnerPoints) {
        runnerPointsContainer.classList.add('d-none');
        runnerPointsContainer.classList.remove('d-flex');
    }
    $.ajax({
        type: "POST",
        url: "/Cart/UpdateRunnerPoints",
        data: { isUsingRunnerPoints },
        success: (response) => {
            $('#runner-points-redeemed').text(`- RM ${response.runnerPointsRedeemed}`);
            $('#total').text(`RM ${response.total}`);
        },
        error: (_xhr, _status, error) => {
            console.error("Update failed: " + error);
        }
    });
}

const updateDineInOption = () => {
    let dineInOption = $('#dine-in-option').val();
    $.ajax({
        type: "POST",
        url: "/Cart/UpdateDineInOption",
        data: { dineInOption },
        success: (response) => {
            $('#delivery-fee').text(`RM ${response.deliveryFee}`);
            $('#total').text(`RM ${response.total}`);
        },
        error: (_xhr, _status, error) => {
            console.error("Update failed: " + error);
        }
    });
}