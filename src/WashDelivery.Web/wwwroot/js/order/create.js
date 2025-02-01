async function nextStep() {
    const errors = [];

    if (!selectedPickupAddressId) {
        errors.push('Wybierz adres odbioru');
    }

    if (!selectedDeliveryAddressId) {
        errors.push('Wybierz adres dostawy');
    }

    const pickupTimeOption = document.querySelector('input[name="pickup-time-option"]:checked')?.value;
    if (!pickupTimeOption) {
        errors.push('Wybierz opcję czasu odbioru');
    }

    if (pickupTimeOption === 'scheduled' && !document.getElementById('pickup-time').value) {
        errors.push('Wybierz datę i godzinę odbioru');
    }

    if (errors.length > 0) {
        const errorsList = document.getElementById('validation-errors-list');
        errorsList.innerHTML = errors.map(error => `<p>${error}</p>`).join('');
        document.getElementById('validation-errors').classList.remove('hidden');
        return;
    }

    document.getElementById('validation-errors').classList.add('hidden');

    let pickupDate = null;
    let pickupTime = null;

    if (pickupTimeOption === 'scheduled') {
        const pickupDateTime = document.getElementById('pickup-time').value;
        if (pickupDateTime) {
            try {
                const dt = new Date(pickupDateTime);
                // Format date as yyyy-MM-dd
                pickupDate = dt.getFullYear() + '-' + 
                    String(dt.getMonth() + 1).padStart(2, '0') + '-' + 
                    String(dt.getDate()).padStart(2, '0');
                // Format time as HH:mm:ss
                pickupTime = String(dt.getHours()).padStart(2, '0') + ':' + 
                    String(dt.getMinutes()).padStart(2, '0') + ':00';
                console.log('Parsed date/time:', { pickupDateTime, pickupDate, pickupTime });
            } catch (error) {
                console.error('Error parsing date/time:', error);
                const errorsList = document.getElementById('validation-errors-list');
                errorsList.innerHTML = '<p>Nieprawidłowy format daty i czasu</p>';
                document.getElementById('validation-errors').classList.remove('hidden');
                return;
            }
        }
    } else {
        // For ASAP pickup, set the date and time to current time + 1 hour
        const dt = new Date(Date.now() + 60 * 60 * 1000); // Current time + 1 hour
        pickupDate = dt.getFullYear() + '-' + 
            String(dt.getMonth() + 1).padStart(2, '0') + '-' + 
            String(dt.getDate()).padStart(2, '0');
        pickupTime = String(dt.getHours()).padStart(2, '0') + ':' + 
            String(dt.getMinutes()).padStart(2, '0') + ':00';
    }

    const leaveAtDoor = document.getElementById('leave-at-door')?.getAttribute('aria-checked') === 'true';
    const courierInstructions = document.getElementById('courier-instructions')?.value?.trim() || null;

    const requestData = {
        pickupAddressId: selectedPickupAddressId,
        deliveryAddressId: selectedDeliveryAddressId,
        pickupDate,
        pickupTime,
        leaveAtDoor,
        courierInstructions
    };

    console.log('Sending request data:', requestData);

    try {
        const response = await fetch('/Order/SaveAddressDetails', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(requestData)
        });

        if (!response.ok) {
            const errorData = await response.text();
            console.error('Server returned error:', errorData);
            try {
                const jsonError = JSON.parse(errorData);
                throw new Error(jsonError.message || jsonError.error || 'Failed to save address details');
            } catch (e) {
                throw new Error(errorData || 'Failed to save address details');
            }
        }

        // Save to localStorage
        const orderData = JSON.parse(localStorage.getItem('orderData') || '{}');
        orderData.addressStep = {
            pickupAddressId: selectedPickupAddressId,
            deliveryAddressId: selectedDeliveryAddressId,
            pickupTime: pickupTimeOption === 'scheduled' ? document.getElementById('pickup-time').value : null,
            leaveAtDoor,
            courierInstructions
        };
        localStorage.setItem('orderData', JSON.stringify(orderData));

        // Redirect to next step
        window.location.href = '/Order/LaundryDetails';
    } catch (error) {
        console.error('Error saving address details:', error);
        const errorsList = document.getElementById('validation-errors-list');
        errorsList.innerHTML = `<p>${error.message || 'Wystąpił błąd podczas zapisywania danych. Spróbuj ponownie.'}</p>`;
        document.getElementById('validation-errors').classList.remove('hidden');
    }
} 