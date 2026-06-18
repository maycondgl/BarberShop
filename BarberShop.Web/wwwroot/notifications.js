window.barberShopNotifications = {
    async subscribe(publicKey) {
        if (!("serviceWorker" in navigator) || !("PushManager" in window)) {
            return null;
        }

        const registration = await navigator.serviceWorker.ready;
        const permission = await Notification.requestPermission();

        if (permission !== "granted") {
            return null;
        }

        const subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: this.urlBase64ToUint8Array(publicKey)
        });

        const json = subscription.toJSON();

        return {
            endpoint: json.endpoint,
            p256Dh: json.keys.p256dh,
            auth: json.keys.auth
        };
    },

    async showLocalNotification(title, body, url) {
        if (!("serviceWorker" in navigator) || Notification.permission !== "granted") {
            return;
        }

        const registration = await navigator.serviceWorker.ready;
        await registration.showNotification(title, {
            body,
            icon: "/icon-192.png",
            badge: "/icon-192.png",
            data: { url }
        });
    },

    urlBase64ToUint8Array(base64String) {
        const padding = "=".repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding).replace(/-/g, "+").replace(/_/g, "/");
        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);

        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }

        return outputArray;
    }
};
