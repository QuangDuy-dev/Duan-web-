document.addEventListener('DOMContentLoaded', function() {
    // Set current date
    const now = new Date();
    document.getElementById('previewDate').textContent = now.toLocaleDateString();
    
    // Initialize items array
    let items = [];
    
    // Load saved data if available
    loadSavedData();
    
    // Add item event
    document.getElementById('addItem').addEventListener('click', addItem);
    
    // Generate receipt event
    document.getElementById('generateReceipt').addEventListener('click', generateReceipt);
    
    // Print receipt event
    document.getElementById('printReceipt').addEventListener('click', printReceipt);
    
    // Clear all event
    document.getElementById('clearAll').addEventListener('click', clearAll);
    
    // Enter key to add item
    document.getElementById('itemName').addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            addItem();
        }
    });
    
    // Auto-save when inputs change
    document.querySelectorAll('input, select, textarea').forEach(input => {
        input.addEventListener('input', saveData);
    });
    
    function addItem() {
        const name = document.getElementById('itemName').value.trim();
        const price = parseFloat(document.getElementById('itemPrice').value);
        const quantity = parseInt(document.getElementById('itemQuantity').value) || 1;
        
        if (!name || isNaN(price) || price <= 0) {
            alert('Please enter a valid item name and price');
            return;
        }
        
        const item = {
            id: Date.now(),
            name,
            price,
            quantity,
            total: price * quantity
        };
        
        items.push(item);
        updateItemsList();
        updateReceiptPreview();
        clearItemInputs();
        saveData();
    }
    
    function removeItem(id) {
        items = items.filter(item => item.id !== id);
        updateItemsList();
        updateReceiptPreview();
        saveData();
    }
    
    function updateItemsList() {
        const itemsList = document.getElementById('itemsList');
        
        if (items.length === 0) {
            itemsList.innerHTML = `
                <tr>
                    <td colspan="5" class="empty-state">
                        <div>No items added yet</div>
                    </td>
                </tr>
            `;
            return;
        }
        
        itemsList.innerHTML = items.map(item => `
            <tr>
                <td>${item.name}</td>
                <td>$${item.price.toFixed(2)}</td>
                <td>${item.quantity}</td>
                <td>$${item.total.toFixed(2)}</td>
                <td class="action-buttons">
                    <button class="btn btn-danger action-btn" onclick="removeItem(${item.id})">Remove</button>
                </td>
            </tr>
        `).join('');
    }
    
    function clearItemInputs() {
        document.getElementById('itemName').value = '';
        document.getElementById('itemPrice').value = '';
        document.getElementById('itemQuantity').value = '1';
        document.getElementById('itemName').focus();
    }
    
    function generateReceipt() {
        updateReceiptPreview();
    }
    
    function updateReceiptPreview() {
        // Update business info
        document.getElementById('previewBusinessAddress').textContent = 
            document.getElementById('businessAddress').value || '123 Business St, City, State 12345';
        document.getElementById('previewBusinessPhone').textContent = 
            document.getElementById('businessPhone').value || '(123) 456-7890';
        
        // Update receipt details
        document.getElementById('previewReceiptNumber').textContent = 
            document.getElementById('receiptNumber').value || 'RCPT-001';
        document.getElementById('previewCustomerName').textContent = 
            document.getElementById('customerName').value || 'Customer Name';
        document.getElementById('previewPaymentMethod').textContent = 
            document.getElementById('paymentMethod').value;
        
        // Update items
        const previewItems = document.getElementById('previewItems');
        
        if (items.length === 0) {
            previewItems.innerHTML = `
                <tr>
                    <td colspan="4" style="text-align: center; padding: 20px;">
                        No items added
                    </td>
                </tr>
            `;
            
            // Reset totals
            document.getElementById('previewSubtotal').textContent = '0.00';
            document.getElementById('previewTax').textContent = '0.00';
            document.getElementById('previewDiscount').textContent = '0.00';
            document.getElementById('previewTotal').textContent = '0.00';
            return;
        }
        
        previewItems.innerHTML = items.map(item => `
            <tr>
                <td>${item.name}</td>
                <td class="text-right">$${item.price.toFixed(2)}</td>
                <td class="text-right">${item.quantity}</td>
                <td class="text-right">$${item.total.toFixed(2)}</td>
            </tr>
        `).join('');
        
        // Calculate totals
        const subtotal = items.reduce((sum, item) => sum + item.total, 0);
        const taxRate = parseFloat(document.getElementById('taxRate').value) || 0;
        const discount = parseFloat(document.getElementById('discount').value) || 0;
        const taxAmount = (subtotal * taxRate) / 100;
        const total = subtotal + taxAmount - discount;
        
        // Update totals
        document.getElementById('previewSubtotal').textContent = subtotal.toFixed(2);
        document.getElementById('previewTaxRate').textContent = taxRate;
        document.getElementById('previewTax').textContent = taxAmount.toFixed(2);
        document.getElementById('previewDiscount').textContent = discount.toFixed(2);
        document.getElementById('previewTotal').textContent = total.toFixed(2);
    }
    
    function printReceipt() {
        window.print();
    }
    
    function clearAll() {
        if (confirm('Are you sure you want to clear all data?')) {
            items = [];
            document.getElementById('businessAddress').value = '';
            document.getElementById('businessPhone').value = '';
            document.getElementById('receiptNumber').value = '';
            document.getElementById('customerName').value = '';
            document.getElementById('paymentMethod').value = 'Cash';
            document.getElementById('taxRate').value = '0';
            document.getElementById('discount').value = '0';
            
            updateItemsList();
            updateReceiptPreview();
            localStorage.removeItem('receiptData');
        }
    }
    
    function applyTemplate(template) {
        // In a real app, this would apply different CSS styles
        const receipt = document.getElementById('receiptPreview');
        
        // Reset all template classes
        receipt.className = 'receipt';
        
        // Add template-specific class
        if (template !== 'default') {
            receipt.classList.add(`template-${template}`);
        }
    }
    
    function saveData() {
        const data = {
            businessAddress: document.getElementById('businessAddress').value,
            businessPhone: document.getElementById('businessPhone').value,
            receiptNumber: document.getElementById('receiptNumber').value,
            customerName: document.getElementById('customerName').value,
            paymentMethod: document.getElementById('paymentMethod').value,
            taxRate: document.getElementById('taxRate').value,
            discount: document.getElementById('discount').value,
            items: items
        };
        
        localStorage.setItem('receiptData', JSON.stringify(data));
    }
    
    function loadSavedData() {
        const savedData = localStorage.getItem('receiptData');
        
        if (savedData) {
            const data = JSON.parse(savedData);
        
            document.getElementById('businessAddress').value = data.businessAddress || '';
            document.getElementById('businessPhone').value = data.businessPhone || '';
            document.getElementById('receiptNumber').value = data.receiptNumber || '';
            document.getElementById('customerName').value = data.customerName || '';
            document.getElementById('paymentMethod').value = data.paymentMethod || 'Cash';
            document.getElementById('taxRate').value = data.taxRate || '0';
            document.getElementById('discount').value = data.discount || '0';
            
            if (data.items && data.items.length > 0) {
                items = data.items;
                updateItemsList();
            }
            
            updateReceiptPreview();
        }
    }
    
    // Make removeItem function available globally
    window.removeItem = removeItem;
});