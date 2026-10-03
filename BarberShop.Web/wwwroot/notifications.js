window.barberShopNotifications = {
    async subscribe(publicKey) {
        try {
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
                        ? "As notificações estão bloqueadas para este site nas configurações do navegador (clique no cadeado da barra de endereço para permitir)."
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
                message: "Notificações ativadas neste dispositivo com sucesso!",
                subscription: {
                    endpoint: json.endpoint,
                    p256Dh: json.keys.p256dh,
                    auth: json.keys.auth
                }
            };
        } catch (err) {
            console.error("Erro ao ativar notificações push:", err);
            const errStr = (err && (err.message || err.toString())) || "";
            if (errStr.includes("incognito") || errStr.includes("InPrivate") || errStr.includes("private") || (err && err.name === "NotAllowedError")) {
                return {
                    success: false,
                    message: "Navegadores em modo anônimo não suportam notificações Push. Abra em uma aba normal para ativar.",
                    subscription: null
                };
            }

            const isBrave = typeof navigator.brave !== "undefined" && typeof navigator.brave.isBrave === "function";
            if (isBrave && (errStr.includes("push service error") || errStr.includes("Registration failed") || errStr.includes("AbortError"))) {
                return {
                    success: false,
                    message: "No Brave, ative 'Usar serviços do Google para mensagens push' em brave://settings/privacy para habilitar notificações.",
                    subscription: null
                };
            }

            return {
                success: false,
                message: "Não foi possível registrar o push neste navegador: " + errStr,
                subscription: null
            };
        }
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
    },

    getPermissionState() {
        const support = this.getPushSupport();
        if (!support.supported) {
            return {
                supported: false,
                permission: "unsupported",
                message: support.message
            };
        }

        return {
            supported: true,
            permission: Notification.permission,
            message: support.message
        };
    },

    isDismissed(storageKey) {
        try {
            const val = localStorage.getItem(storageKey || "barbershop_admin_push_dismissed");
            if (!val) return false;
            const exp = parseInt(val, 10);
            return isNaN(exp) ? true : Date.now() < exp;
        } catch {
            return false;
        }
    },

    setDismissed(storageKey, durationDays) {
        try {
            const days = durationDays || 7;
            const exp = Date.now() + (days * 24 * 60 * 60 * 1000);
            localStorage.setItem(storageKey || "barbershop_admin_push_dismissed", exp.toString());
        } catch {}
    },

    async isSubscribed() {
        try {
            if (!("serviceWorker" in navigator) || !("PushManager" in window)) return false;
            const registration = await navigator.serviceWorker.ready;
            const sub = await registration.pushManager.getSubscription();
            return !!sub;
        } catch {
            return false;
        }
    }
};
