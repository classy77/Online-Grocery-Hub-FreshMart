// FreshMart Grocery Store - JavaScript

$(document).ready(function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Auto-hide alerts after 5 seconds
    setTimeout(function() {
        $('.alert-dismissible').alert('close');
    }, 5000);

    // Update cart count on page load
    updateCartCount();

    // Search autocomplete
    var searchInput = $('input[name="search"]');
    if (searchInput.length > 0) {
        searchInput.on('input', function() {
            var query = $(this).val();
            if (query.length >= 2) {
                $.get('/Product/Search', { query: query }, function(data) {
                    // Could implement dropdown with suggestions here
                });
            }
        });
    }

    // Add to cart button click handler
    $(document).on('click', '.add-to-cart', function(e) {
        e.preventDefault();
        var productId = $(this).data('product-id');
        var quantity = $('#quantity').val() || 1;
        addToCart(productId, parseInt(quantity));
    });

    // Cart quantity buttons
    $(document).on('click', '.quantity-btn', function(e) {
        e.preventDefault();
        var action = $(this).data('action');
        var cartItemId = $(this).data('cart-item-id');
        var currentRow = $(this).closest('tr');
        var quantityInput = currentRow.find('.quantity-input');
        var currentQuantity = parseInt(quantityInput.val());
        var maxStock = parseInt(quantityInput.attr('max'));
        
        var newQuantity = action === 'increase' ? currentQuantity + 1 : currentQuantity - 1;
        
        if (newQuantity < 1) {
            newQuantity = 1;
        }
        if (newQuantity > maxStock) {
            newQuantity = maxStock;
        }
        
        updateCartItemQuantity(cartItemId, newQuantity);
    });

    // Remove cart item button
    $(document).on('click', '.remove-item', function(e) {
        e.preventDefault();
        if (confirm('Are you sure you want to remove this item from your cart?')) {
            var cartItemId = $(this).data('cart-item-id');
            removeCartItem(cartItemId);
        }
    });

    // Clear cart button
    $(document).on('click', '#clear-cart', function(e) {
        e.preventDefault();
        if (confirm('Are you sure you want to clear your entire cart?')) {
            clearCart();
        }
    });
});

// Update cart count
function updateCartCount() {
    $.get('/Cart/GetCartCount', function(data) {
        if (data.count > 0) {
            $('#cart-count').text(data.count).show();
        } else {
            $('#cart-count').hide();
        }
    });
}

// Add to cart function
function addToCart(productId, quantity = 1) {
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: { productId: productId, quantity: quantity },
        headers: { 
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() 
        },
        success: function(response) {
            if (response.success) {
                showToast('Success', response.message, 'success');
                updateCartCount();
            } else {
                showToast('Error', response.message, 'danger');
            }
        },
        error: function() {
            showToast('Error', 'Failed to add item to cart', 'danger');
        }
    });
}

// Update cart item quantity
function updateCartItemQuantity(cartItemId, quantity) {
    $.ajax({
        url: '/Cart/UpdateQuantity',
        type: 'POST',
        data: { cartItemId: cartItemId, quantity: quantity },
        headers: { 
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() 
        },
        success: function(response) {
            if (response.success) {
                location.reload();
            } else {
                showToast('Error', response.message, 'danger');
            }
        },
        error: function() {
            showToast('Error', 'Failed to update cart', 'danger');
        }
    });
}

// Update quantity in product details page
function updateQuantity(change) {
    var quantityInput = $('#quantity');
    var currentValue = parseInt(quantityInput.val());
    var maxStock = parseInt(quantityInput.attr('max'));
    var newValue = currentValue + change;
    
    if (newValue >= 1 && newValue <= maxStock) {
        quantityInput.val(newValue);
    }
}

// Remove cart item
function removeCartItem(cartItemId) {
    $.ajax({
        url: '/Cart/RemoveItem',
        type: 'POST',
        data: { cartItemId: cartItemId },
        headers: { 
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() 
        },
        success: function(response) {
            if (response.success) {
                location.reload();
            } else {
                showToast('Error', response.message, 'danger');
            }
        },
        error: function() {
            showToast('Error', 'Failed to remove item from cart', 'danger');
        }
    });
}

// Clear cart
function clearCart() {
    $.ajax({
        url: '/Cart/ClearCart',
        type: 'POST',
        headers: { 
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() 
        },
        success: function(response) {
            if (response.success) {
                location.reload();
            } else {
                showToast('Error', response.message, 'danger');
            }
        },
        error: function() {
            showToast('Error', 'Failed to clear cart', 'danger');
        }
    });
}

// Show toast notification
function showToast(title, message, type = 'info') {
    var toastHtml = `
        <div class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${title}</strong><br>${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    
    var toastContainer = $('#toast-container');
    if (toastContainer.length === 0) {
        $('body').append('<div id="toast-container" class="toast-container position-fixed top-0 end-0 p-3"></div>');
        toastContainer = $('#toast-container');
    }
    
    var toastElement = $(toastHtml);
    toastContainer.append(toastElement);
    
    var toast = new bootstrap.Toast(toastElement[0], { delay: 3000 });
    toast.show();
    
    toastElement.on('hidden.bs.toast', function() {
        $(this).remove();
    });
}

// Confirm delete
function confirmDelete(message) {
    return confirm(message || 'Are you sure you want to delete this item?');
}

// Format currency
function formatCurrency(amount) {
    return '₹' + parseFloat(amount).toFixed(2);
}

// Validate quantity input
function validateQuantity(input, min = 1, max = 999) {
    var value = parseInt(input.value);
    if (isNaN(value) || value < min) {
        input.value = min;
    } else if (value > max) {
        input.value = max;
    }
}

// Image preview for file uploads
function previewImage(input, previewId) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function(e) {
            $('#' + previewId).attr('src', e.target.result).show();
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// Smooth scroll to element
function scrollToElement(elementId) {
    var element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
    }
}

// Debounce function for search inputs
function debounce(func, wait) {
    var timeout;
    return function executedFunction(...args) {
        var later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Print functionality
function printPage() {
    window.print();
}

// Export table to CSV
function exportTableToCSV(tableId, filename) {
    var csv = [];
    var rows = document.querySelectorAll('#' + tableId + ' tr');
    
    for (var i = 0; i < rows.length; i++) {
        var row = [], cols = rows[i].querySelectorAll('td, th');
        
        for (var j = 0; j < cols.length; j++) {
            row.push(cols[j].innerText);
        }
        
        csv.push(row.join(','));
    }
    
    downloadCSV(csv.join('\n'), filename);
}

function downloadCSV(csv, filename) {
    var csvFile = new Blob([csv], { type: 'text/csv' });
    var downloadLink = document.createElement('a');
    downloadLink.download = filename;
    downloadLink.href = window.URL.createObjectURL(csvFile);
    downloadLink.style.display = 'none';
    document.body.appendChild(downloadLink);
    downloadLink.click();
    document.body.removeChild(downloadLink);
}
