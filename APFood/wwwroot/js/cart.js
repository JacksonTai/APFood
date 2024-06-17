document.addEventListener('DOMContentLoaded', function () {
    const dineInOption = document.getElementById('dine-in-option');
    const locationFormField = document.getElementById('location-form-field');
    const toggleLocationFormField = () => {
        locationFormField.style.display = dineInOption.value == 'Delivery' ? 'block' : 'none';
    }
    dineInOption.addEventListener('change', toggleLocationFormField);
    toggleLocationFormField();
});


const changeQuantity = (itemId, delta) => {
    const inputField = document.getElementById(`quantity-${itemId}`);
    let currentQuantity = parseInt(inputField.value);
    const newQuantity = Math.max(1, currentQuantity + delta);
    inputField.value = newQuantity;

    $.ajax({
        url: "/Cart/UpdateQuantity",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ itemId: itemId, newQuantity: newQuantity }),
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

}

const updateDineInOption = () => {
    let dineInOption = $('#dine-in-option').val();
    console.log(dineInOption)
    $.ajax({
        type: "POST",
        url: "/Cart/UpdateDineInOption",
        data: { dineInOption: dineInOption },
        success: (response) => {
            $('#delivery-fee').text(`RM ${response.deliveryFee}`);
            $('#total').text(`RM ${response.total}`);
        },
        error: (_xhr, _status, error) => {
            console.error("Update failed: " + error);
        }
    });
}