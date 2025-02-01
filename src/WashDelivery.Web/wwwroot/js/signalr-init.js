// Debug flag
const DEBUG = true;

// Debug logging function
function debugLog(...args) {
    if (DEBUG) {
        console.log("[SignalR Debug]", ...args);
    }
}

// Global SignalR connection
window.initializeSignalR = async function(handlers = {}) {
    debugLog("Initializing SignalR...");
    
    try {
        if (!window.hubConnection) {
            debugLog("Creating new hub connection");
            window.hubConnection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/laundryOrder", {
                    withCredentials: true
                })
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Debug)
                .build();

            debugLog("Hub connection created");

            window.hubConnection.onreconnecting(error => {
                debugLog("Reconnecting...", error);
            });

            window.hubConnection.onreconnected(connectionId => {
                debugLog("Reconnected. ConnectionId:", connectionId);
                joinLaundryGroup().catch(err => {
                    debugLog("Failed to join group after reconnect:", err);
                });
            });

            window.hubConnection.onclose(error => {
                debugLog("Connection closed", error);
                setTimeout(() => initializeSignalR(handlers), 5000);
            });

            // Set up handlers
            if (handlers.onNewOrder) {
                window.hubConnection.on("ReceiveNewOrder", handlers.onNewOrder);
            }

            if (handlers.onOrderAccepted) {
                window.hubConnection.on("OrderAccepted", handlers.onOrderAccepted);
            }

            if (handlers.onOrderDeclined) {
                window.hubConnection.on("OrderDeclined", handlers.onOrderDeclined);
            }

            window.hubConnection.on("OrderError", (message) => {
                debugLog("Received error message:", message);
                if (!message.includes("Connection test")) {
                    alert(message);
                }
            });
        }

        debugLog("Starting connection...");
        await window.hubConnection.start();
        debugLog("Connection started successfully");
        
        // Join laundry group after connection
        await joinLaundryGroup();
        debugLog("Joined laundry group");

        // Send test message
        await window.hubConnection.invoke("TestConnection");
        debugLog("Test message sent");
    } catch (err) {
        debugLog("Error during initialization:", err);
        debugLog("Error message:", err.message);
        debugLog("Stack trace:", err.stack);
        setTimeout(() => initializeSignalR(handlers), 5000);
    }
};

async function joinLaundryGroup() {
    debugLog("Joining laundry group...");
    try {
        await window.hubConnection.invoke("JoinLaundryGroup");
        debugLog("Successfully joined laundry group");
    } catch (err) {
        debugLog("Error joining laundry group:", err);
        throw err;
    }
}

// Handle page visibility changes
document.addEventListener('visibilitychange', () => {
    debugLog("Page visibility changed:", document.visibilityState);
    if (document.visibilityState === 'visible' && 
        window.hubConnection?.state === signalR.HubConnectionState.Disconnected) {
        debugLog("Page became visible and connection is disconnected, attempting to reconnect...");
        initializeSignalR();
    }
}); 