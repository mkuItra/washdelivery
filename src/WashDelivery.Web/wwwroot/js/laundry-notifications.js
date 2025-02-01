// Initialize UI variables and functions first
let currentOrderId;
let timerInterval;
let notificationElement;
let orderDetailsElement;
let timerDisplay;
let acceptButton;
let declineButton;
let closeButton;
let notificationsEnabled = false;
let activeNotifications = new Set(); // Track active notifications by orderId

async function requestNotificationPermission() {
    debugLog("Requesting notification permission...");
    try {
        if (!("Notification" in window)) {
            debugLog("Browser does not support desktop notifications");
            return;
        }

        const permission = await Notification.requestPermission();
        debugLog("Notification permission:", permission);
        notificationsEnabled = permission === "granted";
    } catch (err) {
        debugLog("Error requesting notification permission:", err);
    }
}

function showDesktopNotification(order) {
    debugLog("Showing desktop notification...");
    try {
        if (!notificationsEnabled || !("Notification" in window)) {
            debugLog("Desktop notifications not enabled or not supported");
            return;
        }

        // Check if notification for this order is already active
        if (activeNotifications.has(order.id)) {
            debugLog("Notification for order", order.id, "is already active");
            return;
        }

        // Add creation time to the notification
        const createdAt = new Date(order.createdAt);
        const now = new Date();
        const minutesOld = Math.floor((now - createdAt) / 1000 / 60);

        // Don't show notifications for orders older than 3 minutes
        if (minutesOld >= 3) {
            debugLog("Order", order.id, "is too old (", minutesOld, "minutes). Skipping notification.");
            return;
        }

        const notification = new Notification("Nowe zamówienie!", {
            body: `Nowe zamówienie do pralni\nAdres odbioru: ${order.pickupAddress.street}, ${order.pickupAddress.city}\nCzas odbioru: ${new Date(order.pickupTime).toLocaleTimeString()}`,
            icon: "/img/logo.png",
            requireInteraction: true,
            tag: `order-${order.id}` // Use tag to prevent duplicate notifications
        });

        // Add to active notifications
        activeNotifications.add(order.id);

        notification.onclick = function() {
            debugLog("Notification clicked");
            window.focus();
            showNotification();
        };

        notification.onclose = function() {
            debugLog("Notification closed");
            activeNotifications.delete(order.id);
        };

        // Auto-close notification after 3 minutes
        setTimeout(() => {
            if (notification && activeNotifications.has(order.id)) {
                notification.close();
                activeNotifications.delete(order.id);
            }
        }, 180000); // 3 minutes
    } catch (err) {
        debugLog("Error showing desktop notification:", err);
    }
}

function initializeUI() {
    debugLog("Initializing UI elements");
    try {
        // Request notification permission when initializing UI
        requestNotificationPermission();

        notificationElement = document.getElementById('orderNotification');
        debugLog("notificationElement:", notificationElement);

        orderDetailsElement = document.getElementById('orderDetails');
        debugLog("orderDetailsElement:", orderDetailsElement);

        timerDisplay = document.getElementById('orderTimer');
        debugLog("timerDisplay:", timerDisplay);

        acceptButton = document.getElementById('acceptOrder');
        debugLog("acceptButton:", acceptButton);

        declineButton = document.getElementById('declineOrder');
        debugLog("declineButton:", declineButton);

        closeButton = document.getElementById('closeNotification');
        debugLog("closeButton:", closeButton);

        if (!notificationElement || !orderDetailsElement || !timerDisplay || !acceptButton || !declineButton || !closeButton) {
            throw new Error("Required UI elements not found");
        }

        // Add click handlers for notification buttons
        acceptButton.addEventListener('click', async () => {
            debugLog("Accept button clicked");
            if (!currentOrderId || !window.hubConnection) {
                debugLog("Cannot accept order: no current order ID or connection");
                return;
            }

            try {
                debugLog("Invoking AcceptOrder with orderId:", currentOrderId);
                await window.hubConnection.invoke("AcceptOrder", currentOrderId);
                debugLog("AcceptOrder invoked successfully");
            } catch (err) {
                debugLog("Error accepting order:", err);
                debugLog("Error stack:", err.stack);
                alert("Wystąpił błąd podczas akceptacji zamówienia");
            }
        });

        declineButton.addEventListener('click', async () => {
            debugLog("Decline button clicked");
            if (!currentOrderId || !window.hubConnection) {
                debugLog("Cannot decline order: no current order ID or connection");
                return;
            }

            try {
                debugLog("Invoking DeclineOrder with orderId:", currentOrderId);
                await window.hubConnection.invoke("DeclineOrder", currentOrderId);
                debugLog("DeclineOrder invoked successfully");
            } catch (err) {
                debugLog("Error declining order:", err);
                debugLog("Error stack:", err.stack);
                alert("Wystąpił błąd podczas odrzucania zamówienia");
            }
        });

        closeButton.addEventListener('click', hideNotification);
        debugLog("UI elements initialized successfully");
    } catch (err) {
        debugLog("Error initializing UI:", err);
        debugLog("Error stack:", err.stack);
    }
}

function startTimer(duration) {
    let timer = duration;
    clearInterval(timerInterval);
    timerInterval = setInterval(() => {
        const minutes = Math.floor(timer / 60);
        const seconds = timer % 60;
        if (timerDisplay) {
            timerDisplay.textContent = `${minutes}:${seconds.toString().padStart(2, '0')}`;
        }
        if (--timer < 0) {
            clearInterval(timerInterval);
            hideNotification();
        }
    }, 1000);
}

function showNotification() {
    debugLog("Showing notification...");
    try {
        if (!notificationElement) {
            debugLog("Error: notificationElement is null");
            return;
        }
        notificationElement.classList.remove('hidden');
        debugLog("Notification shown successfully");
    } catch (err) {
        debugLog("Error showing notification:", err);
        debugLog("Error stack:", err.stack);
    }
}

function hideNotification() {
    debugLog("Hiding notification...");
    try {
        if (!notificationElement) {
            debugLog("Error: notificationElement is null");
            return;
        }
        notificationElement.classList.add('hidden');
        clearInterval(timerInterval);
        currentOrderId = null;
        debugLog("Notification hidden successfully");
    } catch (err) {
        debugLog("Error hiding notification:", err);
        debugLog("Error stack:", err.stack);
    }
}

function formatOrderDetails(order) {
    debugLog("formatOrderDetails called with order:", order);
    debugLog("Raw order data:", JSON.stringify(order, null, 2));
    try {
        const formattedHtml = `
            <div class="text-sm text-gray-600">
                <div class="font-medium text-gray-900 mb-2">Zamówione usługi:</div>
                <ul class="list-disc pl-5 space-y-1">
                    ${order.items.map(item => `
                        <li>${item.name} - ${item.quantity} szt.${item.weight ? ` (${item.weight} kg)` : ''} - ${item.price.toFixed(2)} zł</li>
                    `).join('')}
                </ul>
                ${order.pickupTime ? `
                    <div class="mt-3">
                        <span class="font-medium text-gray-900">Odbiór:</span>
                        ${new Date(order.pickupTime).toLocaleString('pl-PL')}
                    </div>
                ` : ''}
                <div class="mt-3">
                    <span class="font-medium text-gray-900">Adres odbioru:</span>
                    <div>${order.pickupAddress.street}${order.pickupAddress.apartmentNumber ? `, m. ${order.pickupAddress.apartmentNumber}` : ''}</div>
                    <div>${order.pickupAddress.postalCode} ${order.pickupAddress.city}</div>
                </div>
            </div>
        `;
        debugLog("Formatted HTML:", formattedHtml);
        return formattedHtml;
    } catch (err) {
        debugLog("Error in formatOrderDetails:", err);
        debugLog("Error message:", err.message);
        debugLog("Raw order data:", order);
        debugLog("Stack trace:", err.stack);
        throw err;
    }
}

// Initialize when the document is ready
document.addEventListener('DOMContentLoaded', async () => {
    try {
        await requestNotificationPermission();
        initializeUI();

        // Initialize SignalR with handlers for this view
        window.initializeSignalR({
            onNewOrder: (order) => {
                currentOrderId = order.id;
                showDesktopNotification(order);
                if (orderDetailsElement) {
                    orderDetailsElement.innerHTML = formatOrderDetails(order);
                    showNotification();
                    startTimer(180); // 3 minutes
                }
            },
            onOrderAccepted: (orderId) => {
                hideNotification();
                const pendingOrdersElement = document.querySelector('[data-stat="pending"] dd');
                const inProgressOrdersElement = document.querySelector('[data-stat="inProgress"] dd');
                if (pendingOrdersElement && inProgressOrdersElement) {
                    const pendingCount = parseInt(pendingOrdersElement.textContent);
                    const inProgressCount = parseInt(inProgressOrdersElement.textContent);
                    pendingOrdersElement.textContent = Math.max(0, pendingCount - 1);
                    inProgressOrdersElement.textContent = inProgressCount + 1;
                }
            },
            onOrderDeclined: (orderId) => {
                hideNotification();
                const pendingOrdersElement = document.querySelector('[data-stat="pending"] dd');
                if (pendingOrdersElement) {
                    const currentCount = parseInt(pendingOrdersElement.textContent);
                    pendingOrdersElement.textContent = Math.max(0, currentCount - 1);
                }
            }
        });
    } catch (error) {
        console.error("Error during initialization:", error);
    }
}); 