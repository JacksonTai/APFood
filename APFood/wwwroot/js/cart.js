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
        success: function (response) {
            document.getElementById(`item-price-${itemId}`).textContent = response.itemPrice;
            document.getElementById("total-items").textContent = `${response.totalItems} items`;
            document.getElementById("total-price").textContent = response.totalPrice;
        },
        error: function (xhr, status, error) {
            console.error("Error updating quantity:", xhr.responseText);
        }
    });
};


const removeItem = (itemId) => {
    $.ajax({
        url: "/Cart/RemoveItem",
        method: "POST",
        data: { itemId: itemId },
        success: function (response) {
            document.getElementById("total-items").textContent = `${response.totalItems} items`;
            window.location.reload();
            document.getElementById("total-price").textContent = response.totalPrice;
        },
        error: function (xhr, status, error) {
            console.error("Error removing item:", xhr.responseText);
        }
    });
}

