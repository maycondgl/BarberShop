window.barberShopNotifications = {
    async subscribe(publicKey) {
        const support = this.getPushSupport();

        if (!support.supported) {
            return {
                success: false,
                message: support.message,
                subscription: null
            };
        }

        const registration = await navigator.serviceWorker.ready;
        const permission = await Notification.requestPermission();

        if (permission !== "granted") {
            return {
                success: false,
                message: permission === "denied"
                    ? "As notificações estão bloqueadas para este site nas configurações do navegador."
                    : "Permissão de notificação não foi concedida.",
                subscription: null
            };
        }

        const applicationServerKey = this.urlBase64ToUint8Array(publicKey);
        let subscription = await registration.pushManager.getSubscription();

        if (subscription) {
            try {
                const json = subscription.toJSON();
                if (!json.keys?.p256dh || !json.keys?.auth) {
                    await subscription.unsubscribe();
                    subscription = null;
                }
            } catch {
                await subscription.unsubscribe();
                subscription = null;
            }
        }

        if (!subscription) {
            subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey
            });
        }

        const json = subscription.toJSON();

        return {
            success: true,
            message: "Notificações ativadas neste dispositivo.",
            subscription: {
                endpoint: json.endpoint,
                p256Dh: json.keys.p256dh,
                auth: json.keys.auth
            }
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

    getPushSupport() {
        const isIos = /iPad|iPhone|iPod/.test(navigator.userAgent)
            || (navigator.platform === "MacIntel" && navigator.maxTouchPoints > 1);
        const isStandalone = window.matchMedia("(display-mode: standalone)").matches
            || window.navigator.standalone === true;

        if (!window.isSecureContext) {
            return {
                supported: false,
                message: "Notificações push exigem HTTPS."
            };
        }

        if (isIos && !isStandalone) {
            return {
                supported: false,
                message: "No iPhone, instale o site na tela inicial e abra pelo ícone para ativar notificações na barra."
            };
        }

        if (!("Notification" in window)) {
            return {
                supported: false,
                message: "Este navegador não oferece a API de notificações."
            };
        }

        if (!("serviceWorker" in navigator)) {
            return {
                supported: false,
                message: "Este navegador não oferece service worker."
            };
        }

        if (!("PushManager" in window)) {
            return {
                supported: false,
                message: "Este navegador não oferece Web Push."
            };
        }

        return {
            supported: true,
            message: "Web Push suportado."
        };
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
